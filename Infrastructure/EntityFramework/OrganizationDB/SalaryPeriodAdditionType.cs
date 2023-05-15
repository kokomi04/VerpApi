using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class SalaryPeriodAdditionType
    {
        public SalaryPeriodAdditionType()
        {
            SalaryPeriodAdditionBill = new HashSet<SalaryPeriodAdditionBill>();
            SalaryPeriodAdditionTypeField = new HashSet<SalaryPeriodAdditionTypeField>();
        }

        public int SalaryPeriodAdditionTypeId { get; set; }
        public string Title { get; set; }
        public bool IsActived { get; set; }
        public string Description { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual ICollection<SalaryPeriodAdditionBill> SalaryPeriodAdditionBill { get; set; }
        public virtual ICollection<SalaryPeriodAdditionTypeField> SalaryPeriodAdditionTypeField { get; set; }
    }
}
