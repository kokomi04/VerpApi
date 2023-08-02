using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB;

public partial class SalaryPeriodAdditionBillEmployee
{
    public long SalaryPeriodAdditionBillEmployeeId { get; set; }

    public long SalaryPeriodAdditionBillId { get; set; }

    public long EmployeeId { get; set; }

    public string Description { get; set; }

    public virtual SalaryPeriodAdditionBill SalaryPeriodAdditionBill { get; set; }

    public virtual ICollection<SalaryPeriodAdditionBillEmployeeValue> SalaryPeriodAdditionBillEmployeeValue { get; set; } = new List<SalaryPeriodAdditionBillEmployeeValue>();
}
