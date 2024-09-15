namespace Infra.Redis.Interfaces.V2;

public interface ICacheServiceV2
{
    Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> func, bool disableCache = false);
}