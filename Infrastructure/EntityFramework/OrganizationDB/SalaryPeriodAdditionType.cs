using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB;

public partial class SalaryPeriodAdditionType
{
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

    public virtual ICollection<SalaryPeriodAdditionBill> SalaryPeriodAdditionBill { get; set; } = new List<SalaryPeriodAdditionBill>();

    public virtual ICollection<SalaryPeriodAdditionTypeField> SalaryPeriodAdditionTypeField { get; set; } = new List<SalaryPeriodAdditionTypeField>();
}
