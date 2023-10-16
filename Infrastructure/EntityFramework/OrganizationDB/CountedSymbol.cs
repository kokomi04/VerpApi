using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB;

public partial class CountedSymbol
{
    public int CountedSymbolId { get; set; }

    public int CountedSymbolType { get; set; }

    public string SymbolCode { get; set; }

    public string SymbolDescription { get; set; }

    public int CountedPriority { get; set; }

    public bool IsHide { get; set; }

    public int UpdatedByUserId { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedDatetimeUtc { get; set; }
}
