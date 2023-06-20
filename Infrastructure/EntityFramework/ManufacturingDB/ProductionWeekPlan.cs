using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB;

public partial class ProductionWeekPlan
{
    public long ProductionWeekPlanId { get; set; }

    public long ProductionOrderDetailId { get; set; }

    public DateTime StartDate { get; set; }

    public decimal? ProductQuantity { get; set; }

    public virtual ProductionOrderDetail ProductionOrderDetail { get; set; }

    public virtual ICollection<ProductionWeekPlanDetail> ProductionWeekPlanDetail { get; set; } = new List<ProductionWeekPlanDetail>();
}
