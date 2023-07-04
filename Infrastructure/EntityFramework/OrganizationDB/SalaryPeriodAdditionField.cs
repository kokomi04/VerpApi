using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB;

public partial class SalaryPeriodAdditionField
{
    public int SalaryPeriodAdditionFieldId { get; set; }

    public int SubsidiaryId { get; set; }

    public string FieldName { get; set; }

    public string Title { get; set; }

    public int DecimalPlace { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedDatetimeUtc { get; set; }

    public int UpdatedByUserId { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedDatetimeUtc { get; set; }

    public virtual ICollection<SalaryPeriodAdditionBillEmployeeValue> SalaryPeriodAdditionBillEmployeeValue { get; set; } = new List<SalaryPeriodAdditionBillEmployeeValue>();

    public virtual ICollection<SalaryPeriodAdditionTypeField> SalaryPeriodAdditionTypeField { get; set; } = new List<SalaryPeriodAdditionTypeField>();
}
