using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB;

public partial class ProductionPlanExtraInfo
{
    public int MonthPlanId { get; set; }

    public long ProductionOrderDetailId { get; set; }

    public int SortOrder { get; set; }

    public int SubsidiaryId { get; set; }

    public string Note { get; set; }

    public virtual ProductionOrderDetail ProductionOrderDetail { get; set; }
}
