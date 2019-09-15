using System;
using System.Collections.Generic;
using System.ComponentModel;
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
            string prefix = string.Empty;
            var attrs = enumValue.GetType().GetCustomAttributes(typeof(ErrorCodePrefixAttribute), true);
            if (attrs != null && attrs.Length > 0)
            {
                prefix = ((ErrorCodePrefixAttribute)attrs[0]).Prefix;
            }
            else
            {
                prefix = enumValue.GetType().Name;
            }
            return $"{prefix}-{Convert.ToInt32(enumValue)}";
        }

        public static string GetEnumDescription(this Enum value)
        {
            try
            {
                FieldInfo fi = value.GetType().GetField(value.ToString());
                if (fi == null)
                    return value.ToString();
                var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
                return (attributes.Length > 0) ? attributes[0].Description : string.Empty;
            }
            catch (Exception)
            {
                return value.ToString();
            }
        }

    }
}
