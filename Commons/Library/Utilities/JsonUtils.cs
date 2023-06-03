using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Utilities.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VErp.Commons.Library
{
    public static class JsonUtils
    {

        private static ILogger __logger;
        private static ILogger _logger
        {
            get
            {
                if (__logger != null) return __logger;
                var loggerFactory = Utils.LoggerFactory;
                if (loggerFactory != null)
                {
                    __logger = Utils.LoggerFactory.CreateLogger(typeof(JsonUtils));
                    return __logger;
                }
                return Utils.DefaultLoggerFactory.CreateLogger(typeof(JsonUtils));
            }
        }

        private static JsonSerializerSettings CreateSettings()
        {
            return new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                PreserveReferencesHandling = PreserveReferencesHandling.None,
            };
        }

        private static readonly JsonSerializerSettings settings = CreateSettings();

        public static string JsonSerialize(this object obj, bool isIgnoreSensitiveData)
        {
            var cfg = CreateSettings();
            if (isIgnoreSensitiveData)
            {
                cfg.ContractResolver = new SensitiveDataResolver();
            }
            else
            {
                cfg.ContractResolver = null;
            }

            if (obj != null)
            {
                var type = obj.GetType();
                var fullTypeName = type.FullName;
                if (obj != null && fullTypeName.Contains(".EF.") && fullTypeName.Contains("DB"))
                {
                    cfg.MaxDepth = 3;
                    //return JsonConvert.SerializeObject(CloneEntityForSerialize(obj, new Stack<object>(), 1, 10), cfg);

                }
                else
                {
                    cfg.MaxDepth = 10;
                }
            }

            return JsonConvert.SerializeObject(obj, cfg);

        }

        private static object CloneEntityForSerialize(object obj, Stack<object> ancestors, int level, int maxDeep)
        {
            if (level > maxDeep || obj == null) return null;

            var type = obj.GetType();

            if (ObjectUtils.IsPrimitiveType(type))
            {
                return obj;
            }

            if (ancestors.Contains(obj))
            {
                return null;
            }

            ancestors.Push(obj);

            var props = obj.GetType().GetProperties();

            object instance;
            try
            {
                instance = Activator.CreateInstance(type);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CloneEntityForSerialize {0}", type);
                return obj;
            }
            
            foreach (var p in props)
            {
                var values = p.GetValue(obj, null);

                if (values != null)
                {
                    if (ObjectUtils.IsCollectionType(p.PropertyType))
                    {
                        dynamic lst = Activator.CreateInstance(values.GetType());

                        foreach (var v in (dynamic)values)
                        {
                            var v1 = CloneEntityForSerialize(v, ancestors, level + 1, maxDeep);

                            lst.Add(v1);
                        }
                        p.SetValue(instance, lst);
                    }
                    else
                    {
                        p.SetValue(instance, CloneEntityForSerialize(values, ancestors, level + 1, maxDeep));
                    }
                }

            }
            ancestors.Pop();
            return instance;
        }



        public static string JsonSerialize(this object obj)
        {
            return obj.JsonSerialize(false);
        }



        public static T JsonDeserialize<T>(this string obj)
        {
            if (string.IsNullOrWhiteSpace(obj)) return default(T);
            return JsonConvert.DeserializeObject<T>(obj);
        }

        public static T JsonDeserialize<T>(this string obj, JsonSerializerSettings settings)
        {
            if (string.IsNullOrWhiteSpace(obj)) return default(T);

            if (settings == null) return obj.JsonDeserialize<T>();

            return JsonConvert.DeserializeObject<T>(obj, settings);
        }

        public static object JsonDeserialize(this string obj)
        {
            if (string.IsNullOrWhiteSpace(obj)) return null;
            return JsonConvert.DeserializeObject(obj);
        }

        public static object JsonDeserialize(this string obj, Type type)
        {
            if (string.IsNullOrWhiteSpace(obj))
            {
                if (type.IsValueType)
                {
                    return Activator.CreateInstance(type);
                }
                return null;
            }
            return JsonConvert.DeserializeObject(obj, type);
        }

        public static string GetObjectJsonDiff(string existing, object modified)
        {
            if (existing == null)
            {
                return modified.JsonSerialize();
            }
            if (modified == null)
            {
                return existing;
            }

            JObject xptJson = JObject.FromObject(modified, JsonSerializer.Create(settings));
            JObject actualJson = JObject.Parse(existing);

            var xptProps = xptJson.Properties().ToList();
            var actProps = actualJson.Properties().ToList();

            var changes = (from existingProp in actProps
                           from modifiedProp in xptProps
                           where modifiedProp.Path.Equals(existingProp.Path)
                           where !modifiedProp.Value.Equals(existingProp.Value)
                           select new
                           {
                               Field = existingProp.Path,
                               OldValue = existingProp.Value.ToString(),
                               NewValue = modifiedProp.Value.ToString(),
                           }).ToList();

            var obj = JObject.Parse("{}");
            foreach (var item in changes)
            {
                obj[item.Field] = item.NewValue;
            }

            return obj.JsonSerialize();
        }

        public static string GetJsonDiff(string existing, object modified)
        {
            if (existing == null)
            {
                return modified.JsonSerialize();
            }
            if (modified == null)
            {
                return existing;
            }
            JToken mod = JToken.FromObject(modified, JsonSerializer.Create(settings));
            JToken org = JToken.Parse(existing);

            if (mod is JObject && org is JObject)
            {
                return GetObjectJsonDiff(existing, modified);
            }
            return mod.JsonSerialize();
        }

    }
}
