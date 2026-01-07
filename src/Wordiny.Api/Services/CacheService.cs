using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics.CodeAnalysis;

namespace Wordiny.Api.Services;

public interface ICacheService
{
    void Set(object key, object value, TimeSpan? expiration = null);
    T? Get<T>(object key);
    bool TryGetValue<T>(object key, out T? value);
    void Flush();
    void Clear();
}

public class CacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly Dictionary<object, (object, TimeSpan?)> _buffer = new();

    public CacheService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public void Set(object key, object value, TimeSpan? expiration = null)
    {
        _buffer.Add(key, (value, expiration));
    }

    public T? Get<T>(object key)
    {
        if (_buffer.TryGetValue(key, out var valueAndExpiration))
        {
            if (valueAndExpiration.Item1 is T value)
            {
                return value;
            }

            return default;
        }

        return _memoryCache.Get<T>(key);
    }

    public bool TryGetValue<T>(object key, out T? value)
    {
        if (_buffer.TryGetValue(key, out var valueAndExpiration))
        {
            if (valueAndExpiration.Item1 is T cachedValue)
            {
                value = cachedValue;
                return true;
            }
        }

        return _memoryCache.TryGetValue(key, out value);
    }

    public void Flush()
    {
        foreach (var (key, (value, expiration)) in _buffer)
        {
            if (expiration.HasValue)
            {
                _memoryCache.Set(key, value, expiration.Value);
            }
            else
            {
                _memoryCache.Set(key, value);
            }
        }
    }

    public void Clear()
    {
        _buffer.Clear();
    }
}
