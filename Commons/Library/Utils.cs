using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Commons.Library
{
    public static class Utils
    {
        public static Guid ToGuid(this string value)
        {
            MD5 md5Hasher = MD5.Create();
            byte[] data = md5Hasher.ComputeHash(Encoding.Unicode.GetBytes(value.ToLower()));
            return new Guid(data);
        }

        public static Guid HashApiEndpointId(string route, EnumMethod method)
        {
            route = (route ?? "").Trim().ToLower();
            return $"{route}{method}".ToGuid();
        }

        public static EnumAction GetDefaultAction(this EnumMethod method)
        {
            switch (method)
            {
                case EnumMethod.Get:
                    return EnumAction.View;
                case EnumMethod.Post:
                    return EnumAction.Add;
                case EnumMethod.Put:
                    return EnumAction.Update;
                case EnumMethod.Delete:
                    return EnumAction.Delete;
            }

            return EnumAction.View;
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

        private static readonly JsonSerializerSettings settings = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            PreserveReferencesHandling = PreserveReferencesHandling.None
        };
        public static string JsonSerialize(this object obj)
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(obj, settings);
        }

        public static T JsonDeserialize<T>(this string obj)
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(obj);
        }

        public static long GetUnix(this DateTime dateTime)
        {
            return (long)dateTime.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }

        public static decimal Eval(string expression)
        {
            try
            {
                var outPut = new NCalc.Expression(expression).Evaluate();
                var result = Convert.ToDecimal(outPut);
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}
