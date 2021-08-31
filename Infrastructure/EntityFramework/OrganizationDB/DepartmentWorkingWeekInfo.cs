using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class DepartmentWorkingWeekInfo
    {
        public int DepartmentId { get; set; }
        public int DayOfWeek { get; set; }
        public int SubsidiaryId { get; set; }
        public bool IsDayOff { get; set; }
        public DateTime StartDate { get; set; }
    }
}
