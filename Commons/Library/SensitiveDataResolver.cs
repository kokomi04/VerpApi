using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace VErp.Commons.Library
{
    public class SensitiveDataResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            if (member is PropertyInfo)
            {
                var prop = (PropertyInfo)member;
                //var isSensitiveData = Attribute.IsDefined(prop, typeof(SensitiveDataAttribute));

                var isSensitiveData = prop.Name.ToLower().Contains("password");

                if (isSensitiveData)
                {
                    property.ValueProvider = new SensitiveDataProvider();
                }
            }

            return property;
        }
    }

    public class SensitiveDataProvider : IValueProvider
    {
        readonly string sesitiveDatatag = "Sensitive Data ***";
        public object GetValue(object target)
        {
            return sesitiveDatatag;
        }

        public void SetValue(object target, object value)
        {
            target = sesitiveDatatag;
        }
    }
}
