using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class WorkingWeekInfo
    {
        public int DayOfWeek { get; set; }
        public int SubsidiaryId { get; set; }
        public bool IsDayOff { get; set; }
        public DateTime StartDate { get; set; }
    }
}
