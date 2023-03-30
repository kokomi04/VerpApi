using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class SalaryPeriodAdditionTypeField
    {
        public int SalaryPeriodAdditionTypeId { get; set; }
        public int SalaryPeriodAdditionFieldId { get; set; }
        public int SortOrder { get; set; }

        public virtual SalaryPeriodAdditionField SalaryPeriodAdditionField { get; set; }
        public virtual SalaryPeriodAdditionType SalaryPeriodAdditionType { get; set; }
    }
}
