using System;
using System.ComponentModel;
using System.Reflection;
using Verp.Resources.Values;

namespace VErp.Commons.Enums.MasterEnum
{
    public static class EnumValueExtensions
    {
        public static EnumDataType[] GetAllowedDataType(this Enum value)
        {
            try
            {
                FieldInfo fi = value.GetType().GetField(value.ToString());
                if (fi == null)
                    return Array.Empty<EnumDataType>();
                var attributes = (AllowedDataTypeAttribute[])fi.GetCustomAttributes(typeof(AllowedDataTypeAttribute), false);
                return (attributes.Length > 0) ? attributes[0].AllowedDataType : Array.Empty<EnumDataType>();
            }
            catch (Exception)
            {
                return Array.Empty<EnumDataType>();
            }
        }

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

        public static int GetDataSize(this Enum value)
        {
            try
            {
                FieldInfo fi = value.GetType().GetField(value.ToString());
                if (fi == null)
                    return 0;
                var attributes = (DataSizeAttribute[])fi.GetCustomAttributes(typeof(DataSizeAttribute), false);
                return (attributes.Length > 0) ? attributes[0].DataSize : 0;
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public static string GetRegex(this Enum value)
        {
            try
            {
                FieldInfo fi = value.GetType().GetField(value.ToString());
                if (fi == null)
                    return string.Empty;
                var attributes = (RegexAttribute[])fi.GetCustomAttributes(typeof(RegexAttribute), false);
                return (attributes.Length > 0) ? attributes[0].Regex : string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public static T GetValueFromDescription<T>(string description)
        {
            var type = typeof(T);
            if (!type.IsEnum) throw new InvalidOperationException();
            foreach (var field in type.GetFields())
            {
                var attribute = Attribute.GetCustomAttribute(field,
                    typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (attribute != null)
                {
                    if (attribute.Description == description)
                        return (T)field.GetValue(null);
                }
                else
                {
                    if (field.Name == description)
                        return (T)field.GetValue(null);
                }
            }
            throw new ArgumentException("Not found.", nameof(description));
        }

        public static string[] GetRangeValue(this Enum value)
        {
            try
            {
                FieldInfo fi = value.GetType().GetField(value.ToString());
                if (fi == null)
                    return new string[] { };
                var attributes = (RangeValueAttribute[])fi.GetCustomAttributes(typeof(RangeValueAttribute), false);
                return (attributes.Length > 0) ? attributes[0].RangeValue : new string[] { };
            }
            catch (Exception)
            {
                return new string[] { };
            }
        }

        public static string GetDataTypeValueTitleByLanguage(this EnumDataType dataTypeId, object value)
        {
            switch(dataTypeId)
            {
                case EnumDataType.Boolean:
                    if (value?.ToString()?.ToLower() == "true") return ValuesTitleBoolean.True;
                    break;
            }

            return value?.ToString();
        }
    }
}
