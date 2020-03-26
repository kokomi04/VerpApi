using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Reflection;
using System.Text;

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
    }
}
