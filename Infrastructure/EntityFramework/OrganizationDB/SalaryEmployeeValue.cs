using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class SalaryEmployeeValue
    {
        public long SalaryEmployeeId { get; set; }
        public int SalaryFieldId { get; set; }
        public object Value { get; set; }
        public bool IsEdited { get; set; }

        public virtual SalaryEmployee SalaryEmployee { get; set; }
        public virtual SalaryField SalaryField { get; set; }
    }
}
