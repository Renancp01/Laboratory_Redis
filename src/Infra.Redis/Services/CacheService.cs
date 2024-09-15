using System.Text.Json;
using Contracts.Models;
using Infra.Redis.Configurations;
using Infra.Redis.Interfaces;
using Infra.Redis.Responses;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Infra.Redis.Services;

public class CacheService(
    IOptionsMonitor<CacheConfiguration> cacheConfiguration,
    IDistributedCache redisCache,
    ILogger<CacheService> logger)
    : ICacheService
{
    private readonly CacheConfiguration _cacheOptions = cacheConfiguration.CurrentValue;

    public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> func, bool disableCache = false)
    {
        try
        {
            if (disableCache)
            {
                logger.LogInformation("Cache is disabled for key: {Key}", key);
                return await func();
            }

            var (expired, cacheResponse) = await GetValueFromCache<T>(key);

            if (cacheResponse is not null && !expired)
            {
                logger.LogInformation("Cache hit for key: {Key}", key);
                return cacheResponse.Data;
            }

            var item = await func();

            if (!IsValid(item, key))
            {
                logger.LogWarning("Invalid item for key: {Key}", key);
                return ExpiredCache(item, cacheResponse, disableCache, key);
            }

            await SetValue(key, item);

            logger.LogInformation("Cache set for key: {Key}", key);
            return item;
        }
        catch (RedisConnectionException ex)
        {
            logger.LogError(ex, "Redis connection exception for key: {Key}", key);
            return await func();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception for key: {Key}", key);
            return await func();
        }
    }

    private async Task SetValue<T>(string key, T item)
    {
        var cacheEntryOptions = new DistributedCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(_cacheOptions.AbsoluteExpirationInMin));

        var response = new Response<T>()
        {
            Data = item,
            ExpireAt = DateTime.UtcNow.AddMinutes(_cacheOptions.CacheDurationInMin)
        };

        var serializedResponse = JsonSerializer.Serialize(response);

        await redisCache.SetStringAsync(key, serializedResponse, cacheEntryOptions);
        logger.LogInformation("Value set in cache for key: {Key}", key);
    }

    private T ExpiredCache<T>(T item, Response<T>? cacheResponse, bool disableCache, string key)
    {
        if (cacheResponse is not null && !disableCache)
        {
            logger.LogWarning("Returning expired cache for key: {Key}", key);
            return cacheResponse.Data!;
        }

        logger.LogWarning("Returning new item for key: {Key}", key);
        return item;
    }

    private bool IsValid<T>(T item, string key)
    {
        if (item is Result { IsValid: true })
        {
            logger.LogInformation("Item is valid for key: {Key}", key);
            return true;
        }

        logger.LogWarning("Item is invalid for key: {Key}", key);
        return false;
    }

    private async Task<(bool expired, Response<T>? response)> GetValueFromCache<T>(string key)
    {
        var cacheItem = await redisCache.GetStringAsync(key);

        if (string.IsNullOrEmpty(cacheItem))
        {
            logger.LogInformation("Cache miss for key: {Key}", key);
            return (false, null);
        }

        var response = JsonSerializer.Deserialize<Response<T>>(cacheItem);

        if (response is not null && !response.Expired())
        {
            logger.LogInformation("Cache item is valid for key: {Key}", key);
            return (false, response);
        }

        logger.LogWarning("Cache item is expired for key: {Key}", key);
        return (true, response);
    }
}