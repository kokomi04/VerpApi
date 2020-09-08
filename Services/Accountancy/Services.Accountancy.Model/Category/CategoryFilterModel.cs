﻿using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Commons.GlobalObject;

namespace VErp.Services.Accountancy.Model.Category
{
    public class CategoryFilterModel
    {
        public string Keyword { get; set; }
        public string Filters { get; set; }
        public string ExtraFilter { get; set; }

        public ExtraFilterParam[] ExtraFilterParams { get; set; }

        public int Page { get; set; }
        public int Size { get; set; }
    }

    public class ExtraFilterParam
    { 
        public string ParamName { get; set; }
        public EnumDataType DataType { get; set; }
        public object Value { get; set; }
    }
}
