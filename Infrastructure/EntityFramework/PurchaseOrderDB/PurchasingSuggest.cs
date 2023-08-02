using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.PurchaseOrderDB;

public partial class PurchasingSuggest
{
    public long PurchasingSuggestId { get; set; }

    public int SubsidiaryId { get; set; }

    public string PurchasingSuggestCode { get; set; }

    public DateTime Date { get; set; }

    public string Content { get; set; }

    public int RejectCount { get; set; }

    public int PurchasingSuggestStatusId { get; set; }

    public bool? IsApproved { get; set; }

    public int? PoProcessStatusId { get; set; }

    public bool IsDeleted { get; set; }

    public int CreatedByUserId { get; set; }

    public int UpdatedByUserId { get; set; }

    public int? CensorByUserId { get; set; }

    public DateTime? CensorDatetimeUtc { get; set; }

    public DateTime CreatedDatetimeUtc { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public DateTime? DeletedDatetimeUtc { get; set; }

    public decimal? TaxInMoney { get; set; }

    public decimal? TaxInPercent { get; set; }

    public decimal? TotalMoney { get; set; }

    public virtual ICollection<PoAssignment> PoAssignment { get; set; } = new List<PoAssignment>();

    public virtual ICollection<PurchasingSuggestDetail> PurchasingSuggestDetail { get; set; } = new List<PurchasingSuggestDetail>();

    public virtual ICollection<PurchasingSuggestFile> PurchasingSuggestFile { get; set; } = new List<PurchasingSuggestFile>();
}
