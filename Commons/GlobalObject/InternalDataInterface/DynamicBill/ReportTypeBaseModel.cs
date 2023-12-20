using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VErp.Commons.GlobalObject.InternalDataInterface.DynamicBill
{
    public class ReportTypeBaseModel
    {
        public int ReportTypeGroupId { get; set; }
        public int? ReportTypeId { get; set; }
        public string ReportTypeName { get; set; }
    }

    public class ReportTypeGroupBaseModel
    {
        public int ReportTypeGroupId { get; set; }
        public string ReportTypeGroupName { get; set; }
    }
}
