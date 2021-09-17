using System;
using System.Collections.Generic;
using System.Text;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Commons.GlobalObject;

namespace Verp.Services.ReportConfig.Model
{
    public class ReportFilterDataModel
    {
        public Dictionary<string, object> Filters { get; set; }
        public string OrderByFieldName { get; set; }
        public bool Asc { get; set; }

        public Clause ColumnsFilters { get; set; }

    }

    public class ReportFilterModel : ReportFilterDataModel
    {
        public int Page { get; set; }
        public int Size { get; set; }

    }
}
