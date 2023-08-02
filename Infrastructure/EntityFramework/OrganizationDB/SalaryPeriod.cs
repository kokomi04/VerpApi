using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB;

public partial class SalaryPeriod
{
    public int SalaryPeriodId { get; set; }

    public int Month { get; set; }

    public int Year { get; set; }

    public DateTime FromDate { get; set; }

    public DateTime ToDate { get; set; }

    public int? CheckedByUserId { get; set; }

    public DateTime? CheckedDatetimeUtc { get; set; }

    public int? CensorByUserId { get; set; }

    public DateTime? CensorDatetimeUtc { get; set; }

    public int SalaryPeriodCensorStatusId { get; set; }

    public int SubsidiaryId { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedDatetimeUtc { get; set; }

    public int UpdatedByUserId { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedDatetimeUtc { get; set; }

    public virtual ICollection<SalaryEmployee> SalaryEmployee { get; set; } = new List<SalaryEmployee>();

    public virtual ICollection<SalaryPeriodGroup> SalaryPeriodGroup { get; set; } = new List<SalaryPeriodGroup>();
}
