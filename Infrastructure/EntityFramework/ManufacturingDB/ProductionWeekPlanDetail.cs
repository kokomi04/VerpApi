using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class ProductionWeekPlanDetail
    {
        public long ProductionWeekPlanId { get; set; }
        public int ProductCateId { get; set; }
        public decimal MaterialQuantity { get; set; }

        public virtual ProductionWeekPlan ProductionWeekPlan { get; set; }
    }
}
