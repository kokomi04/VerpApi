using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.PurchaseOrderDB;

public partial class PurchaseOrderOrderMapping
{
    public long PurchaseOrderOrderMappingId { get; set; }

    public long PurchaseOrderDetailId { get; set; }

    public string OrderCode { get; set; }

    public decimal? PrimaryQuantity { get; set; }

    public decimal? PuQuantity { get; set; }

    public string? Note { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedDatetimeUtc { get; set; }

    public int UpdatedByUserId { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedDatetimeUtc { get; set; }

    public virtual PurchaseOrderDetail PurchaseOrderDetail { get; set; }
}
