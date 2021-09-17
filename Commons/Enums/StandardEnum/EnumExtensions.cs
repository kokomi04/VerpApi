using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.ObjectExtensions.CustomAttributes;
using VErp.Commons.ObjectExtensions.Extensions;

namespace VErp.Commons.Enums.StandardEnum
{
    public static class EnumExtensions
    {
        public static bool IsSuccess(this Enum enumValue)
        {
            return (GeneralCode)enumValue == GeneralCode.Success;
        }

        public static string GetErrorCodeString(this Enum enumValue)
        {
            string prefix = enumValue.GetType().GetErrorCodePrefix();

            return $"{prefix}-{Convert.ToInt32(enumValue)}";
        }

        public static string GetErrorCodePrefix(this Type type, bool fallbackToName = true)
        {
            string prefix = string.Empty;
            var attrs = type.GetCustomAttributes(typeof(ErrorCodePrefixAttribute), true);
            if (attrs != null && attrs.Length > 0)
            {
                prefix = ((ErrorCodePrefixAttribute)attrs[0]).Prefix;
            }

            if (string.IsNullOrWhiteSpace(prefix) && fallbackToName)
            {
                prefix = type.Name;
            }
            return prefix;
        }

        public static string GetEnumDescription(this Enum value)
        {
            try
            {
                var enumResourceAttr = (LocalizedDescriptionAttribute[])value.GetType().GetCustomAttributes(typeof(LocalizedDescriptionAttribute), false);
                if (enumResourceAttr.Length > 0)
                {
                    System.Resources.ResourceManager rs = new System.Resources.ResourceManager(enumResourceAttr[0].ResourceType);
                    var des = rs.GetString(value.ToString());
                    if (!string.IsNullOrWhiteSpace(des))
                    {
                        return des;
                    }
                }

                FieldInfo fi = value.GetType().GetField(value.ToString());
                if (fi == null)
                    return value.ToString();
                var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
                return (attributes.Length > 0) ? attributes[0].Description : value.ToString();
            }
            catch (Exception)
            {
                return value.ToString();
            }
        }

       
        public static string GetEnumDescription<T>(this int value) where T : Enum
        {
         
            return ((Enum)Enum.ToObject(typeof(T), value)).GetEnumDescription();
        }

        public static T? GetEnumValue<T>(this string value) where T : struct, Enum
        {
            try
            {
                var members = GetEnumMembers<T>();
                value = value?.NormalizeAsInternalName();
                foreach (var member in members)
                {
                    var rangeValueAttr = member.Attributes.FirstOrDefault(a => a.GetType() == typeof(RangeValueAttribute));
                    if (rangeValueAttr != null)
                    {
                        var rangeValues = ((RangeValueAttribute)rangeValueAttr).RangeValue;
                        if (rangeValues.Any(v => v.Equals(value, StringComparison.OrdinalIgnoreCase))) return member.Enum;
                    }
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static IList<object> GetEnumAttributes(this Enum value)
        {
            try
            {
                FieldInfo fi = value.GetType().GetField(value.ToString());
                if (fi == null)
                    return new List<object>();
                return fi.GetCustomAttributes(false);
            }
            catch (Exception)
            {
                return new List<object>();
            }
        }

        public static IEnumerable<EnumInfo<T>> GetEnumMembers<T>() where T : Enum
        {
            var values = Enum.GetValues(typeof(T)).Cast<T>();
            foreach (var value in values)
            {
                yield return new EnumInfo<T>()
                {
                    Enum = value,
                    Name = value.ToString(),
                    Description = value.GetEnumDescription(),
                    Attributes = value.GetEnumAttributes()
                };
            }
        }
    }

    public class EnumInfo<T> where T : Enum
    {
        public T Enum { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public IList<object> Attributes { get; set; }
    }
}
