using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB;

public partial class SalaryField
{
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

    public bool IsDisplayRefData { get; set; }

    public bool IsCalcSum { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedDatetimeUtc { get; set; }

    public int UpdatedByUserId { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedDatetimeUtc { get; set; }

    public virtual ICollection<SalaryEmployeeValue> SalaryEmployeeValue { get; set; } = new List<SalaryEmployeeValue>();

    public virtual ICollection<SalaryGroupField> SalaryGroupField { get; set; } = new List<SalaryGroupField>();
}
