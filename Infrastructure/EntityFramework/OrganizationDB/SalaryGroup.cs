using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB;

public partial class SalaryGroup
{
    public int SalaryGroupId { get; set; }

    public string Title { get; set; }

    public string EmployeeFilter { get; set; }

    public bool IsActived { get; set; }

    public int SubsidiaryId { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedDatetimeUtc { get; set; }

    public int UpdatedByUserId { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedDatetimeUtc { get; set; }

    public virtual ICollection<SalaryEmployee> SalaryEmployee { get; set; } = new List<SalaryEmployee>();

    public virtual ICollection<SalaryGroupField> SalaryGroupField { get; set; } = new List<SalaryGroupField>();

    public virtual ICollection<SalaryPeriodGroup> SalaryPeriodGroup { get; set; } = new List<SalaryPeriodGroup>();
}
