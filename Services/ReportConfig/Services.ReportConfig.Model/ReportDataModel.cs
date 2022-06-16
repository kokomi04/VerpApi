using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ServiceCore.Model;

namespace Verp.Services.ReportConfig.Model
{
    public class ReportDataModel
    {
        public ReportDataModel()
        {
            Totals = new NonCamelCaseDictionary<decimal>();
        }
        public NonCamelCaseDictionary<decimal> Totals { get; set; }
        public PageDataTable Rows { get; set; }
        public NonCamelCaseDictionary Head { get; set; }
        public IList<NonCamelCaseDictionary> HeadTable { get; set; }
        public NonCamelCaseDictionary Foot { get; set; }
        public RBody Body { get; set; }
        public class RBody
        {
            public string Title { get; set; }
            public List<ReportHeadModel> HeadDetails { get; set; }
            public ReportFilterDataModel FilterData { get; set; }
        }
    }
}
