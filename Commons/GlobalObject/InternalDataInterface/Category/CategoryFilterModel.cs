using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Commons.GlobalObject.InternalDataInterface.Category
{
    public class CategoryFilterModel
    {
        public string Keyword { get; set; }
        public Clause Filters { get; set; }
        public string ExtraFilter { get; set; }

        public ExtraFilterParam[] ExtraFilterParams { get; set; }

        public int Page { get; set; }
        public int Size { get; set; }
        public string OrderBy { get; set; }
        public bool Asc { get; set; } = true;
    }

    public class ExtraFilterParam
    { 
        public string ParamName { get; set; }
        public EnumDataType DataType { get; set; }
        public object Value { get; set; }
    }
}
