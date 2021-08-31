using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class DepartmentWorkingHourInfo
    {
        public int DepartmentId { get; set; }
        public DateTime StartDate { get; set; }
        public int SubsidiaryId { get; set; }
        public double WorkingHourPerDay { get; set; }
    }
}
