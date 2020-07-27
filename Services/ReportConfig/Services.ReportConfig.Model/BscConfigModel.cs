using NPOI.OpenXmlFormats.Spreadsheet;
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
        public NonCamelCaseDictionary<string> Value { get; set; }

        private NonCamelCaseDictionary<BscCellModel> rowData;
        public NonCamelCaseDictionary<BscCellModel> RowData
        {
            get
            {
                if (rowData == null && Value != null)
                {
                    var dic = new NonCamelCaseDictionary<BscCellModel>();
                    foreach (var (key, value) in Value)
                    {
                        dic.Add(key, new BscCellModel()
                        {
                            Value = value,
                            Style = new NonCamelCaseDictionary()
                        });
                    }
                    return dic;
                }
                return rowData;
            }
            set { rowData = value; }
        }

        public static bool IsSqlSelect(object valueConfig)
        {
            return valueConfig?.ToString()?.StartsWith("=") == true;
        }

        public static bool IsBscSelect(object valueConfig)
        {
            return IsSqlSelect(valueConfig) && valueConfig.ToString().Contains(AccountantConstants.REPORT_BSC_VALUE_PARAM_PREFIX);
        }
    }

    public class BscCellModel
    {
        public string Value { get; set; }
        public NonCamelCaseDictionary Style { get; set; }
    }
}
