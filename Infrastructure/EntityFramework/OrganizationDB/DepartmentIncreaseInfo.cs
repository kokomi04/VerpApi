using System;

#nullable disable

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class DepartmentIncreaseInfo
    {
        public long DepartmentIncreaseInfoId { get; set; }
        public int DepartmentId { get; set; }
        public int SubsidiaryId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int NumberOfPerson { get; set; }
        public string Content { get; set; }
    }
}
