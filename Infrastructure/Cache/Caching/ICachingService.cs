using System;
using System.Threading.Tasks;

namespace Verp.Cache.Caching
{
    public interface ICachingService
    {
        T TryGet<T>(string key);
        void TryUpdate<T>(string tag, string key, TimeSpan ttl, Func<T, T> queryData);
        T TryGetSet<T>(string tag, string key, TimeSpan ttl, Func<T> queryData, TimeSpan? sliding = null);
        Task<T> TryGetSet<T>(string tag, string key, TimeSpan ttl, Func<Task<T>> queryData, TimeSpan? sliding = null);
        void TrySet<T>(string tag, string key, T value, TimeSpan ttl, TimeSpan? sliding = null);

        void TryRemoveByTag(string tag);

        void TryRemove(string key);
    }
}
