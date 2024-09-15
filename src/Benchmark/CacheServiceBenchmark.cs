using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Infra.Redis.Configurations;
using Infra.Redis.Services;
using Infra.Redis.Services.V2;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Benchmark;

[MemoryDiagnoser]
public class CacheServiceBenchmark
{
    private readonly CacheService _originalService;
    private readonly CacheServiceV2 _refactoredService;
    
    public CacheServiceBenchmark()
    {
        // var cacheConfig = Options.Create(new CacheConfiguration { CacheDurationInMin = 60, AbsoluteExpirationInMin = 120 });
        var cacheConfig = OptionsMonitorMock.Create(new CacheConfiguration { CacheDurationInMin = 60, AbsoluteExpirationInMin = 120 });
        var distributedCache = new Mock<IDistributedCache>().Object;  // Mock do cache para benchmark
        var logger = new Mock<ILogger<CacheService>>().Object;        // Mock do logger
        var loggerV2 = new Mock<ILogger<CacheServiceV2>>().Object;        // Mock do logger

        _originalService = new CacheService(cacheConfig, distributedCache, logger);
        _refactoredService = new CacheServiceV2(cacheConfig, distributedCache, loggerV2);
    }
    
    [Benchmark(Baseline = true)]
    [IterationCount(10)] 
    [WarmupCount(5)]
    public async Task OriginalMethod()
    {
        await _originalService.GetOrSetAsync("testKey", async () => await Task.FromResult("testValue"));
    }

    [Benchmark]
    [IterationCount(10)] 
    [WarmupCount(5)]
    public async Task RefactoredMethod()
    {
        await _refactoredService.GetOrSetAsync("testKey", async () => await Task.FromResult("testValue"));
    }
    
    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<CacheServiceBenchmark>();
        }
    }
}