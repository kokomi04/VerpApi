using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VErp.Commons.GlobalObject.InternalDataInterface.Organization
{
    public class BaseWorkingDateModel
    {
        public int WorkingDateId { get; set; }

        public int UserId { get; set; }

        public int SubsidiaryId { get; set; }

        public bool? IsIgnoreFilterAccountant { get; set; }

        public bool? IsAutoUpdateWorkingDate { get; set; }

        public long? WorkingFromDate { get; set; }

        public long? WorkingToDate { get; set; }
        public string WorkingDateConfig { get; set; }
    }
}
