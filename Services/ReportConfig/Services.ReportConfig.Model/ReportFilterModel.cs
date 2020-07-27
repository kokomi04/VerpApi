using System;
using System.Collections.Generic;
using System.Text;

namespace Verp.Services.ReportConfig.Model
{
    public class ReportFilterModel
    {
        public Dictionary<string, object> Filters { get; set; }
        public string OrderByFieldName { get; set; }
        public bool Asc { get; set; }
        public int Page { get; set; }
        public int Size { get; set; }
    }
}
