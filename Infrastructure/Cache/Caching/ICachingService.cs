using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Verp.Cache.Caching
{
    public interface ICachingService
    {
        T TryGet<T>(string key);
        T TryGetSet<T>(string tag, string key, TimeSpan ttl, Func<T> queryData, TimeSpan? sliding = null);
        Task<T> TryGetSet<T>(string tag, string key, TimeSpan ttl, Func<Task<T>> queryData, TimeSpan? sliding = null);
        void TrySet<T>(string tag, string key, T value, TimeSpan ttl, TimeSpan? sliding = null);

        void TryRemoveByTag(string tag);
    }
}
