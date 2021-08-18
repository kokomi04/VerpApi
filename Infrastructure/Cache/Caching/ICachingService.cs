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
        T TryGetSet<T>(string key, TimeSpan ttl, Func<T> queryData);
        Task<T> TryGetSet<T>(string key, TimeSpan ttl, Func<Task<T>> queryData);
        void TrySet<T>(string key, T value, TimeSpan ttl);
    }
}
