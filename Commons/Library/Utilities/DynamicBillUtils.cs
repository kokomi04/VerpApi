using System;
using VErp.Commons.Constants;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Commons.Library
{
    public static class DynamicBillUtils
    {

        public static bool IsVndColumn(this string columnName)
        {
            return columnName.ToLower().StartsWith(AccountantConstants.THANH_TIEN_VND_PREFIX.ToLower());
        }

        public static bool IsTkCoColumn(this string columnName)
        {
            return columnName.ToLower().StartsWith(AccountantConstants.TAI_KHOAN_CO_PREFIX.ToLower());
        }
        public static bool IsTkNoColumn(this string columnName)
        {
            return columnName.ToLower().StartsWith(AccountantConstants.TAI_KHOAN_NO_PREFIX.ToLower());
        }

        public static string VndSumName(this string columnName)
        {
            return $"{AccountantConstants.SUM_RECIPROCAL_PREFIX}{columnName}";
        }
        public static bool IsNgoaiTeColumn(this string columnName)
        {
            return columnName.ToLower().StartsWith(AccountantConstants.THANH_TIEN_NGOAI_TE_PREFIX.ToLower());
        }

        public static bool IsSelectForm(this EnumFormType formType)
        {
            return DataTypeConstants.SELECT_FORM_TYPES.Contains(formType);
        }

        public static bool IsJoinForm(this EnumFormType formType)
        {
            return DataTypeConstants.JOIN_FORM_TYPES.Contains(formType);
        }

        public static bool IsTimeType(this EnumDataType type)
        {
            return DataTypeConstants.TIME_TYPES.Contains(type);
        }


        public static int CompareValue(this EnumDataType dataType, object value1, object value2)
        {
            if (value1.IsNullObject() && value2.IsNullObject()) return 0;
            if (value1.IsNullObject() && !value2.IsNullObject()) return -1;
            if (!value1.IsNullObject() && value2.IsNullObject()) return 1;
            var dataValue1 = dataType.GetSqlValue(value1);
            var dataValue2 = dataType.GetSqlValue(value2);

            switch (dataType)
            {
                case EnumDataType.Text:
                case EnumDataType.PhoneNumber:
                case EnumDataType.Email:
                    return string.Compare((string)dataValue1, (string)dataValue2);
                case EnumDataType.Int:
                    return ((int)dataValue1).CompareTo((int)dataValue2);
                case EnumDataType.Date:
                case EnumDataType.Year:
                case EnumDataType.Month:
                case EnumDataType.QuarterOfYear:
                case EnumDataType.DateRange:
                    return ((DateTime)dataValue1).CompareTo((DateTime)dataValue2);
                case EnumDataType.BigInt:
                    return ((long)dataValue1).CompareTo((long)dataValue2);
                case EnumDataType.Boolean:
                    return ((bool)dataValue1).CompareTo((bool)dataValue2);
                case EnumDataType.Percentage:
                    return ((float)dataValue1).CompareTo((float)dataValue2);
                case EnumDataType.Decimal:
                    return ((decimal)dataValue1).CompareTo((decimal)dataValue2);
                default: return 0;
            }
        }


    }
}
