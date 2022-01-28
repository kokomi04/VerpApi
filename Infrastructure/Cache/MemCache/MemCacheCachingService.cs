
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
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


        public T TryGetSet<T>(string tag, string key, TimeSpan ttl, Func<T> queryData, TimeSpan? sliding = null)
        {
            return _cache.GetOrCreate(key, (v) =>
            {
                v.AbsoluteExpirationRelativeToNow = ttl;
                v.AddExpirationToken(TagToken(tag, ttl));
                if (sliding != null)
                {
                    v.SlidingExpiration = sliding;
                }
                return queryData.Invoke();
            });
        }

        public Task<T> TryGetSet<T>(string tag, string key, TimeSpan ttl, Func<Task<T>> queryData, TimeSpan? sliding = null)
        {
            return _cache.GetOrCreateAsync(key, (v) =>
            {
                v.AbsoluteExpirationRelativeToNow = ttl;
                v.AddExpirationToken(TagToken(tag, ttl));
                if (sliding != null)
                {
                    v.SlidingExpiration = sliding;
                }
                return queryData.Invoke();
            });
        }

        public void TrySet<T>(string tag, string key, T value, TimeSpan ttl, TimeSpan? sliding = null)
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = ttl
            }
            .AddExpirationToken(TagToken(tag, ttl));

            if (sliding != null)
            {
                cacheEntryOptions.SlidingExpiration = sliding;
            }

            _cache.Set(key, value, cacheEntryOptions);
        }

        public void TryRemoveByTag(string tag)
        {
            var key = TagCacheKey(tag);
            var tagToken = TryGet<CancellationTokenSource>(key);
            if (tagToken != null)
            {
                _cache.Remove(key);
                tagToken.Cancel();
                tagToken.Dispose();
            }
        }

        private CancellationChangeToken TagToken(string tag, TimeSpan ttl)
        {
            var token = _cache.GetOrCreate(TagCacheKey(tag), (v) =>
             {
                 v.AbsoluteExpirationRelativeToNow = ttl;
                 return new CancellationTokenSource();
             }).Token;

            return new CancellationChangeToken(token);
        }

        private string TagCacheKey(string tag)
        {
            return $"TAG_CACHE_{tag}";
        }


    }
}
