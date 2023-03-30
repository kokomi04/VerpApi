using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class SalaryPeriodAdditionBill
    {
        public SalaryPeriodAdditionBill()
        {
            SalaryPeriodAdditionBillEmployee = new HashSet<SalaryPeriodAdditionBillEmployee>();
        }

        public long SalaryPeriodAdditionBillId { get; set; }
        public int SalaryPeriodAdditionTypeId { get; set; }
        public string BillCode { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public string Description { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual SalaryPeriodAdditionType SalaryPeriodAdditionType { get; set; }
        public virtual ICollection<SalaryPeriodAdditionBillEmployee> SalaryPeriodAdditionBillEmployee { get; set; }
    }
}
