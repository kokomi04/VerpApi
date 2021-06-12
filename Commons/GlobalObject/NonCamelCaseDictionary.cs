using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VErp.Commons.GlobalObject
{
    public class NonCamelCaseDictionary : Dictionary<string, object>
    {
        public bool TryGetValue(string key, out string value)
        {
            var isSuccess = base.TryGetValue(key, out var objValue);
            value = objValue?.ToString()?.Trim();
            return isSuccess;
        }
    }

    public class NonCamelCaseDictionary<T> : Dictionary<string, T>
    {
        public bool TryGetValue(string key, out string value)
        {
            var isSuccess = base.TryGetValue(key, out var objValue);
            value = objValue?.ToString()?.Trim();
            return isSuccess;
        }
    }

    public static class NonCamelCaseDictionaryExtensions
    {
        public static NonCamelCaseDictionary ToNonCamelCaseDictionary<T>(this IEnumerable<T> source, Func<T, string> keySelector, Func<T, object> elementSelector)
        {
            var data = new NonCamelCaseDictionary();
            foreach (var item in source)
            {
                var key = keySelector(item);
                var value = elementSelector(item);
                data.Add(key, value);
            }
            return data;
        }

        public static NonCamelCaseDictionary<TEntity> ToNonCamelCaseDictionaryData<T, TEntity>(this IEnumerable<T> source, Func<T, string> keySelector, Func<T, TEntity> elementSelector)
        {
            var data = new NonCamelCaseDictionary<TEntity>();
            foreach (var item in source)
            {
                var key = keySelector(item);
                var value = elementSelector(item);
                data.Add(key, value);
            }
            return data;
        }
    }
}
