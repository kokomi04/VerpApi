using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class SalaryGroupField
    {
        public int SalaryGroupId { get; set; }
        public int SalaryFieldId { get; set; }
        public bool IsEditable { get; set; }
        public bool IsHidden { get; set; }
        public int SortOrder { get; set; }

        public virtual SalaryField SalaryField { get; set; }
        public virtual SalaryGroup SalaryGroup { get; set; }
    }
}
