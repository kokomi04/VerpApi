using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class SalaryGroup
    {
        public SalaryGroup()
        {
            SalaryEmployee = new HashSet<SalaryEmployee>();
            SalaryGroupField = new HashSet<SalaryGroupField>();
            SalaryPeriodGroup = new HashSet<SalaryPeriodGroup>();
        }

        public int SalaryGroupId { get; set; }
        public string Title { get; set; }
        public string EmployeeFilter { get; set; }
        public int SubsidiaryId { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual ICollection<SalaryEmployee> SalaryEmployee { get; set; }
        public virtual ICollection<SalaryGroupField> SalaryGroupField { get; set; }
        public virtual ICollection<SalaryPeriodGroup> SalaryPeriodGroup { get; set; }
    }
}
