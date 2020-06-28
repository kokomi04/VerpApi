using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.AccountancyDB
{
    public partial class InputTypeViewField
    {
        public int InputTypeViewFieldId { get; set; }
        public int InputTypeViewId { get; set; }
        public int Column { get; set; }
        public int SortOrder { get; set; }
        public string Title { get; set; }
        public string Placeholder { get; set; }
        public int DataTypeId { get; set; }
        public int DataSize { get; set; }
        public int FormTypeId { get; set; }
        public string DefaultValue { get; set; }
        public string SelectFilters { get; set; }
        public string RefTableCode { get; set; }
        public string RefTableField { get; set; }
        public string RefTableTitle { get; set; }
        public bool IsRequire { get; set; }
        public string RegularExpression { get; set; }

        public virtual InputTypeView InputTypeView { get; set; }
    }
}
