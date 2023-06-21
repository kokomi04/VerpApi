using AutoMapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using OpenXmlPowerTools;
using Org.BouncyCastle.Utilities.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library.Utilities;

namespace VErp.Commons.Library
{
    public static class JsonUtils
    {

        public const int JSON_MAX_DEPTH = 10;
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

        private static JsonSerializerSettings CreateSettings(int maxDept = JSON_MAX_DEPTH)
        {
            return new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                PreserveReferencesHandling = PreserveReferencesHandling.None,
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCaseExceptDictionaryKeysResolver(),
                Converters = new List<JsonConverter>
                {
                    new JsonSerializeDeepConverter(maxDept)
                }
            };
        }

        private static readonly JsonSerializerSettings settings = CreateSettings();
        public static readonly JsonSerializer JsonSerializer = JsonSerializer.Create(new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            PreserveReferencesHandling = PreserveReferencesHandling.None,
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new CamelCaseExceptDictionaryKeysResolver()           
        });

        class CamelCaseExceptDictionaryKeysResolver : CamelCasePropertyNamesContractResolver
        {
            protected override JsonDictionaryContract CreateDictionaryContract(Type objectType)
            {
                JsonDictionaryContract contract = base.CreateDictionaryContract(objectType);
                if (objectType == typeof(NonCamelCaseDictionary) || (objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(NonCamelCaseDictionary<>)))
                {
                    contract.DictionaryKeyResolver = propertyName => propertyName;
                }

                return contract;
            }
        }

        public static JToken GetJobjectNoneLoopDeep(object obj, Stack<object> ancestors, int level, int maxDeep)
        {
            //return obj;
            if (level > maxDeep || obj == null)
            {
                return null;
            }

            if (obj is Exception ex)
            {
                var jEx = new JObject();
                jEx.Add(nameof(ex.Message), ex.Message);
                jEx.Add(nameof(ex.Source), ex.Source);
                jEx.Add(nameof(ex.StackTrace), ex.StackTrace);
                jEx.Add(nameof(ex.InnerException), GetJobjectNoneLoopDeep(ex.InnerException, ancestors, level, maxDeep));
                return jEx;
            }
          

            var type = obj.GetType();

            if (ObjectUtils.IsPrimitiveType(type))
            {
                return JToken.FromObject(obj, JsonSerializer);
            }

            if (ancestors.Contains(obj))
            {
                return null;
            }


            ancestors.Push(obj);

            if (ObjectUtils.IsCollectionType(type))
            {
                var lst = new JArray();

                foreach (var v in (dynamic)obj)
                {
                    var v1 = GetJobjectNoneLoopDeep(v, ancestors, level + 1, maxDeep);

                    lst.Add(v1);
                }
                ancestors.Pop();
                return lst;
            }

            var props = obj.GetType().GetProperties().Where(p => p.CanRead).ToList();
            if (props.Any(p => p.GetIndexParameters().Length > 0)) return JToken.FromObject(obj, JsonSerializer);

            var instance = new Dictionary<string, JToken>();

            foreach (var p in props)
            {

                object values;
                try
                {
                    values = p.GetValue(obj, null);
                }
                catch (Exception)
                {
                    continue;
                }

                if (values != null)
                {
                    instance.Add(p.Name, GetJobjectNoneLoopDeep(values, ancestors, level + 1, maxDeep));
                }

            }
            ancestors.Pop();
            return JObject.FromObject(instance, JsonSerializer);
        }



        public static string JsonSerialize(this object obj)
        {
            var cfg = CreateSettings();

            cfg.ContractResolver = new SensitiveDataResolver();

            return JsonConvert.SerializeObject(obj, cfg);
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
