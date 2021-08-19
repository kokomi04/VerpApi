using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class DepartmentDayOffCalendar
    {
        public int DepartmentId { get; set; }
        public int SubsidiaryId { get; set; }
        public DateTime Day { get; set; }
        public string Content { get; set; }
    }
}
