using Contracts.Models;
using Infra.Redis.Interfaces.V2;
using Infra.Redis.Services.V2;
using Microsoft.Extensions.Logging;

namespace Infra.Redis.Strategies;

public class ValidationStrategy<T>(ILogger<CacheServiceV2> logger) : IValidationStrategy<T>
{
    public bool IsValid(T item, string key)
    {
        if (item is Result { IsValid: true })
        {
            logger.LogInformation("Item is valid for key: {Key}", key);
            return true;
        }

        logger.LogWarning("Item is invalid for key: {Key}", key);
        return false;
    }
}