﻿using System.Collections.Generic;
using VErp.Commons.GlobalObject;

namespace VErp.Services.Accountancy.Model.Input
{
    public class InputTypeBillsRequestFilterModel
    {
        public string Keyword { get; set; }
        public Dictionary<int, object> Filters { get; set; }
        public string OrderBy { get; set; }
        public bool Asc { get; set; } = true;
        public long? FromDate { get; set; }
        public long? ToDate { get; set; }
        public Clause ColumnsFilters { get; set; }
    }

    public class InputTypeBillsRequestModel : InputTypeBillsRequestFilterModel
    {
        public bool IsMultirow { get; set; }
        public int Page { get; set; }
        public int Size { get; set; }
    }

    public class InputTypeBillsExporttFilterModel : InputTypeBillsRequestFilterModel
    {
        public IList<string> FieldNames { get; set; }
    }
}
