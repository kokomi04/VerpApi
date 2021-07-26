using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.MasterDB
{
    public partial class CategoryViewField
    {
        public int CategoryViewFieldId { get; set; }
        public int CategoryViewId { get; set; }
        public string ParamerterName { get; set; }
        public int SortOrder { get; set; }
        public string Title { get; set; }
        public string Placeholder { get; set; }
        public int DataTypeId { get; set; }
        public int DataSize { get; set; }
        public int FormTypeId { get; set; }
        public string DefaultValue { get; set; }
        public string RefTableCode { get; set; }
        public string RefTableField { get; set; }
        public string RefTableTitle { get; set; }
        public string RefFilters { get; set; }
        public bool IsRequire { get; set; }
        public string RegularExpression { get; set; }
        public string ExtraFilter { get; set; }
        public int? Column { get; set; }
        public string TitleStyleJson { get; set; }
        public string InputStyleJson { get; set; }
        public string HelpText { get; set; }

        public virtual CategoryView CategoryView { get; set; }
    }
}
