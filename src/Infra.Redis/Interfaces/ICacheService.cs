﻿namespace Infra.Redis.Interfaces;

public interface ICacheService
{
    Task<T?> GetOrSetAsync<T>(string key, Func<Task<T>> func, bool disableCache = false);
}