﻿using NPOI.OpenXmlFormats.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Constants;
using VErp.Commons.GlobalObject;

namespace Verp.Services.ReportConfig.Model
{
    public class BscConfigModel
    {
        public IList<BscColumnModel> BscColumns { get; set; }
        public IList<BscRowsModel> Rows { get; set; }
    }

    public class BscColumnModel
    {
        public bool IsRowKey { get; set; }
        public string Name { get; set; }
    }
    public class BscRowsModel
    {
        public int SortOrder { get; set; }
        public bool IsBold { get; set; }
        public NonCamelCaseDictionary Value { get; set; }

        public static bool IsSqlSelect(object valueConfig)
        {
            return valueConfig?.ToString()?.StartsWith("=") == true;
        }

        public static bool IsBscSelect(object valueConfig)
        {
            return IsSqlSelect(valueConfig) && valueConfig.ToString().Contains(AccountantConstants.REPORT_BSC_VALUE_PARAM_PREFIX);
        }
    }
}