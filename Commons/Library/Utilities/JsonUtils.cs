using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace VErp.Commons.Library
{
    public static class JsonUtils
    {
        private static readonly JsonSerializerSettings settings = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            PreserveReferencesHandling = PreserveReferencesHandling.None,
        };

        public static string JsonSerialize(this object obj, bool isIgnoreSensitiveData)
        {
            try
            {
                var cfg = settings;
                if (isIgnoreSensitiveData)
                {
                    cfg.ContractResolver = new SensitiveDataResolver();
                }
                else
                {
                    cfg.ContractResolver = null;
                }

                var fullTypeName = obj.GetType().FullName;
                if (obj != null && fullTypeName.Contains(".EF.") && fullTypeName.Contains("DB"))
                {
                    cfg.MaxDepth = 2;
                }
                else
                {
                    cfg.MaxDepth = 10;
                }

                return JsonConvert.SerializeObject(obj, cfg);
            }
            catch (Exception)
            {

                throw;
            }

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
