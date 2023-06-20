using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB;

public partial class SalaryPeriodAdditionBillEmployeeValue
{
    public long SalaryPeriodAdditionBillEmployeeId { get; set; }

    public int SalaryPeriodAdditionFieldId { get; set; }

    public decimal? Value { get; set; }

    public virtual SalaryPeriodAdditionBillEmployee SalaryPeriodAdditionBillEmployee { get; set; }

    public virtual SalaryPeriodAdditionField SalaryPeriodAdditionField { get; set; }
}
