using System.Collections.Generic;
using VErp.Commons.Constants;
using VErp.Commons.GlobalObject;

namespace Verp.Services.ReportConfig.Model
{
    public class BscConfigModel
    {
        public IList<BscColumnModel> BscColumns { get; set; }
        public IList<BscRowsModel> Rows { get; set; }
        public IList<BscVariableViewDefined> VariableViews { get; set; }
        public IList<BscVariableDefined> Variables { get; set; }
        public bool IsPeriodCalc { get; set; }
        public string PeriodCalcPrefixSqlStatement { get; set; }
    }

    public class BscColumnModel
    {
        public bool IsRowKey { get; set; }
        public string Name { get; set; }
    }

    public class BscRowsModel
    {
        public const string EscapseBscParamPrefix = "\\" + AccountantConstants.REPORT_BSC_VALUE_PARAM_PREFIX;
        public const string EscapseBscParamSpecialReplacingString = "AAAAAAA___BB_CC___MMMA_A";

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
            var str = valueConfig.ToString()??"";

            str = str.Replace(EscapseBscParamPrefix, EscapseBscParamSpecialReplacingString);
            var isBscSelect = IsSqlSelect(valueConfig) && str.Contains(AccountantConstants.REPORT_BSC_VALUE_PARAM_PREFIX);
            return isBscSelect;
        }
    }

    public class BscVariableViewDefined
    {
        public int SortOrder { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string RawSql { get; set; }
    }

    public class BscVariableDefined
    {
        public int SortOrder { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public string Tk { get; set; }
        public string Expression { get; set; }
        public string OtherConditional { get; set; }
        public string VariableViewName { get; set; }
    }


    public class BscCellModel
    {
        public string Value { get; set; }
        public bool CanEdit { get; set; }
        public NonCamelCaseDictionary Style { get; set; }
    }
}
