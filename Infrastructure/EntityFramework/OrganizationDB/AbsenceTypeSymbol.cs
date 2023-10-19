using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB;

public partial class AbsenceTypeSymbol
{
    public int AbsenceTypeSymbolId { get; set; }

    public string SymbolCode { get; set; }

    public string TypeSymbolDescription { get; set; }

    public int MaxOfDaysOffPerMonth { get; set; }

    public bool IsUsed { get; set; }

    public bool IsCounted { get; set; }

    public double SalaryRate { get; set; }

    public bool IsAnnualLeave { get; set; }

    public bool IsUnpaidLeave { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedDatetimeUtc { get; set; }

    public int UpdatedByUserId { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedDatetimeUtc { get; set; }

    public virtual ICollection<Leave> Leave { get; set; } = new List<Leave>();
}
