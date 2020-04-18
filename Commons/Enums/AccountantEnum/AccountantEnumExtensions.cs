using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Reflection;
using System.Text;

namespace VErp.Commons.Enums.AccountantEnum
{
    public static class AccountantEnumExtensions
    {
        public static int GetParamNumber(this Enum value)
        {
            try
            {
                FieldInfo fi = value.GetType().GetField(value.ToString());
                if (fi == null)
                    return 0;
                var attributes = (ParamNumberAttribute[])fi.GetCustomAttributes(typeof(ParamNumberAttribute), false);
                return (attributes.Length > 0) ? attributes[0].ParamNumber : 0;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public static bool IsRef(this Enum value)
        {
            try
            {
                FieldInfo fi = value.GetType().GetField(value.ToString());
                if (fi == null)
                    return false;
                var attributes = (IsRefAttribute[])fi.GetCustomAttributes(typeof(IsRefAttribute), false);
                return (attributes.Length > 0) ? attributes[0].IsReference : false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
