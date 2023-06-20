using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB;

public partial class ProductionOrderDetail
{
    public long ProductionOrderDetailId { get; set; }

    public long ProductionOrderId { get; set; }

    public int ProductId { get; set; }

    public decimal Quantity { get; set; }

    /// <summary>
    /// Bù hao (dự trữ)
    /// </summary>
    public decimal ReserveQuantity { get; set; }

    public string Note { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedDatetimeUtc { get; set; }

    public int UpdatedByUserId { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedDatetimeUtc { get; set; }

    public long? OrderDetailId { get; set; }

    public int SubsidiaryId { get; set; }

    public string OrderCode { get; set; }

    public string PartnerId { get; set; }

    public long? ProductionProcessVersion { get; set; }

    public virtual ICollection<OutsourcePartRequest> OutsourcePartRequest { get; set; } = new List<OutsourcePartRequest>();

    public virtual ProductionOrder ProductionOrder { get; set; }

    public virtual ICollection<ProductionPlanExtraInfo> ProductionPlanExtraInfo { get; set; } = new List<ProductionPlanExtraInfo>();

    public virtual ICollection<ProductionWeekPlan> ProductionWeekPlan { get; set; } = new List<ProductionWeekPlan>();
}
