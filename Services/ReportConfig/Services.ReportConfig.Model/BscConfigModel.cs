using System.Collections.Generic;
using VErp.Commons.Constants;
using VErp.Commons.GlobalObject;

namespace Verp.Services.ReportConfig.Model
{
    public class BscConfigModel
    {
        public IList<BscColumnModel> BscColumns { get; set; }
        public IList<BscRowsModel> Rows { get; set; }
        public IList<BscVariableDefined> Variables { get; set; }
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

        public NonCamelCaseDictionary<BscCellModel> RowData { get; set; }

        public NonCamelCaseDictionary<string> Value { get; set; }


        public static bool IsSqlSelect(object valueConfig)
        {
            return valueConfig?.ToString()?.StartsWith("=") == true;
        }

        public static bool IsBscSelect(object valueConfig)
        {
            return IsSqlSelect(valueConfig) && valueConfig.ToString().Contains(AccountantConstants.REPORT_BSC_VALUE_PARAM_PREFIX);
        }
    }

    public class BscVariableDefined
    {
        public int SortOrder { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public string Tk { get; set; }
        public string Expression { get; set; }
        public string OtherConditional { get; set; }
    }


    public class BscCellModel
    {
        public string Value { get; set; }
        public bool CanEdit { get; set; }
        public NonCamelCaseDictionary Style { get; set; }
    }
}
