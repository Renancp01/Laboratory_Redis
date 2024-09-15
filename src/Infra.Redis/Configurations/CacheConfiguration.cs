namespace Infra.Redis.Configurations;

public class CacheConfiguration
{
    public int CacheDurationInMin { get; set; }
    
    public int AbsoluteExpirationInMin { get; set; }
}