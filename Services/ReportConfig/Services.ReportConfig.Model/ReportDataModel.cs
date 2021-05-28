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
            Totals = new NonCamelCaseDictionary();
        }
        public NonCamelCaseDictionary Totals { get; set; }
        public PageDataTable Rows { get; set; }
        public NonCamelCaseDictionary Head { get; set; }
        public IList<NonCamelCaseDictionary> HeadTable { get; set; }
        public NonCamelCaseDictionary Foot { get; set; }
    }
}
