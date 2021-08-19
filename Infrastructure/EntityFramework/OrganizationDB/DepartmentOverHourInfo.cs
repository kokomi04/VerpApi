using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class DepartmentOverHourInfo
    {
        public long DepartmentOverHourInfoId { get; set; }
        public int DepartmentId { get; set; }
        public int SubsidiaryId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public double OverHour { get; set; }
        public int NumberOfPerson { get; set; }
        public string Content { get; set; }
    }
}
