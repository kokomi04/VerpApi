using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class SalaryEmployee
    {
        public long SalaryEmployeeId { get; set; }
        public long EmployeeId { get; set; }
        public int SalaryPeriodId { get; set; }
        public int SalaryGroupId { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual HrBill Employee { get; set; }
        public virtual SalaryGroup SalaryGroup { get; set; }
        public virtual SalaryPeriod SalaryPeriod { get; set; }
    }
}
