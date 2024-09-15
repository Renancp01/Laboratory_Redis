using Infra.Redis.Configurations;
using Infra.Redis.Interfaces;
using Infra.Redis.Interfaces.V2;
using Infra.Redis.Services;
using Infra.Redis.Services.V2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infra.Redis.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
    {
        var redisConfiguration = configuration
            .GetSection($"{nameof(RedisConfiguration)}")
            .Get<RedisConfiguration>();

        services.Configure<RedisConfiguration>(options =>
                configuration.GetSection($"{nameof(CacheConfiguration)}").Bind(options))
            .Configure<CacheConfiguration>(options =>
                configuration.GetSection($"{nameof(CacheConfiguration)}").Bind(options))
            .AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConfiguration.ConnectionString;
                options.InstanceName = "SampleInstance";
            });

        services.AddSingleton<ICacheService, CacheService>();
        services.AddSingleton<ICacheServiceV2, CacheServiceV2>();
        return services;
    }
}