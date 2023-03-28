using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class SalaryField
    {
        public SalaryField()
        {
            SalaryEmployeeValue = new HashSet<SalaryEmployeeValue>();
            SalaryGroupField = new HashSet<SalaryGroupField>();
        }

        public int SalaryFieldId { get; set; }
        public int SubsidiaryId { get; set; }
        public string GroupName { get; set; }
        public string SalaryFieldName { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int DataTypeId { get; set; }
        public int DecimalPlace { get; set; }
        public int SortOrder { get; set; }
        public string Expression { get; set; }
        public bool IsEditable { get; set; }
        public bool IsHidden { get; set; }
        public bool IsDisplayRefData { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual ICollection<SalaryEmployeeValue> SalaryEmployeeValue { get; set; }
        public virtual ICollection<SalaryGroupField> SalaryGroupField { get; set; }
    }
}
