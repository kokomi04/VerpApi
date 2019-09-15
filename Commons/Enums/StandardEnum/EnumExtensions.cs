using System;
using System.Collections.Generic;
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
    }
}
