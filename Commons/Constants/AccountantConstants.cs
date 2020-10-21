using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Commons.Constants
{
    public static class AccountantConstants
    {
        public static readonly List<EnumFormType> SELECT_FORM_TYPES = new List<EnumFormType>() { EnumFormType.Select, EnumFormType.SearchTable, EnumFormType.MultiSelect };
        public static readonly List<EnumFormType> JOIN_FORM_TYPES = new List<EnumFormType>() { EnumFormType.Select, EnumFormType.SearchTable };
        public static readonly List<EnumDataType> TIME_TYPES = new List<EnumDataType>() { EnumDataType.Date, EnumDataType.DateRange, EnumDataType.Month, EnumDataType.QuarterOfYear, EnumDataType.Year };

        public const int INPUT_TYPE_FIELD_NUMBER = 101;
        public const int CONVERT_VALUE_TO_NUMBER_FACTOR = 100000;
        public const string INPUT_TYPE_FIELDNAME_FORMAT = "Field{0}";
        public const string F_IDENTITY = "F_Id";
        public const string PARENT_ID_TITLE = "Đối tượng cha";
        public const string PARENT_ID_FIELD_NAME = "ParentId";
        public const string IDENTITY_AREA = "IdentityArea";
        public const string IDENTITY_AREA_TITLE = "Thông tin định danh";


        public const string THANH_TIEN_VND_PREFIX = "vnd";
        public const string SUM_RECIPROCAL_PREFIX = "sum_";
        public const string THANH_TIEN_NGOAI_TE_PREFIX = "ngoai_te";
        public const string TAI_KHOAN_CO_PREFIX = "tk_co";
        public const string TAI_KHOAN_NO_PREFIX = "tk_no";
        public const int MAX_COUPLE_RECIPROCAL = 5;

        public const string INPUTVALUEROW_TABLE = "InputValueRow";
        public const string INPUTVALUEROW_VIEW = "vInputValueRow";


        public const string REPORT_HEAD_PARAM_PREFIX = "HEAD_";
        public const string REPORT_BSC_VALUE_PARAM_PREFIX = "BSC_VALUE_";


        public const string BILL_CODE = "so_ct";
        public const string BILL_DATE = "ngay_ct";


        public static readonly TimeSpan REPORT_QUERY_TIMEOUT = new TimeSpan(0, 10, 0);
    }
}
