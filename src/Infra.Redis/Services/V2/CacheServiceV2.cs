using System.Text.Json;
using Infra.Redis.Configurations;
using Infra.Redis.Factories;
using Infra.Redis.Interfaces.V2;
using Infra.Redis.Responses;
using Infra.Redis.Strategies;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infra.Redis.Services.V2;

public class CacheServiceV2(
    IOptionsMonitor<CacheConfiguration> cacheConfiguration,
    IDistributedCache redisCache,
    ILogger<CacheServiceV2> logger) : ICacheServiceV2
{
    private readonly CacheConfiguration _cacheOptions = cacheConfiguration.CurrentValue;

    public async Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> func, bool disableCache = false)
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
        var validationStrategy = new ValidationStrategy<T>(logger);

        if (!validationStrategy.IsValid(item, key))
        {
            return CacheExpirationFactory<T>.HandleExpiration(cacheResponse, disableCache, item, logger, key);
        }

        await SetValue(key, item);

        logger.LogInformation("Cache set for key: {Key}", key);
        return item;
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
}