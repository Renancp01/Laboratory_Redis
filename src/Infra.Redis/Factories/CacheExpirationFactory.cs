using Infra.Redis.Responses;
using Microsoft.Extensions.Logging;

namespace Infra.Redis.Factories;

public class CacheExpirationFactory<T>
{
    public static T HandleExpiration(Response<T>? cacheResponse, bool disableExpiredCache, T newItem, ILogger logger,
        string key)
    {
        if (cacheResponse is not null && !disableExpiredCache)
        {
            logger.LogWarning("Returning expired cache for key: {Key}", key);
            return cacheResponse.Data!;
        }

        logger.LogWarning("Returning new item for key: {Key}", key);
        return newItem;
    }
}