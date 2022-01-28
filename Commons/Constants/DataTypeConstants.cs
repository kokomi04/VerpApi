using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Commons.Constants
{
    public static class DataTypeConstants
    {
        public static readonly List<EnumFormType> SELECT_FORM_TYPES = new List<EnumFormType>() { EnumFormType.Select, EnumFormType.SearchTable, EnumFormType.MultiSelect };
        public static readonly List<EnumFormType> JOIN_FORM_TYPES = new List<EnumFormType>() { EnumFormType.Select, EnumFormType.SearchTable };
        public static readonly List<EnumDataType> TIME_TYPES = new List<EnumDataType>() { EnumDataType.Date, EnumDataType.DateRange, EnumDataType.Month, EnumDataType.QuarterOfYear, EnumDataType.Year };

    }
}
