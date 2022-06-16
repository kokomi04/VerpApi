using System.Collections.Generic;
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
    }
}
