
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Verp.Cache.Caching;

namespace Verp.Cache.MemCache
{
    public class MemCacheCachingService : ICachingService
    {
        private IMemoryCache _cache;

        public MemCacheCachingService(IMemoryCache memoryCache)
        {
            _cache = memoryCache;
        }

        public T TryGet<T>(string key)
        {
            if (_cache.TryGetValue<T>(key, out var value))
                return value;
            return default(T);
        }

        public T TryGetSet<T>(string key, TimeSpan ttl, Expression<Func<T>> queryData)
        {
            return _cache.GetOrCreate(key, (v) =>
            {
                v.AbsoluteExpirationRelativeToNow = ttl;
                return queryData.Compile().Invoke();
            });

        }

        public T TryGetSet<T>(string key, TimeSpan ttl, Func<T> queryData)
        {
            return _cache.GetOrCreate(key, (v) =>
            {
                v.AbsoluteExpirationRelativeToNow = ttl;
                return queryData.Invoke();
            });
        }

        public Task<T> TryGetSet<T>(string key, TimeSpan ttl, Func<Task<T>> queryData)
        {
            return _cache.GetOrCreateAsync(key, (v) =>
            {
                v.AbsoluteExpirationRelativeToNow = ttl;
                return queryData.Invoke();
            });
        }

        public void TrySet<T>(string key, T value, TimeSpan ttl)
        {
            _cache.Set(key, value, ttl);
        }
    }
}
