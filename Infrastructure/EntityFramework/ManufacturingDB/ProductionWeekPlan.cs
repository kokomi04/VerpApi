using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class ProductionWeekPlan
    {
        public ProductionWeekPlan()
        {
            ProductionWeekPlanDetail = new HashSet<ProductionWeekPlanDetail>();
        }

        public long ProductionWeekPlanId { get; set; }
        public long ProductionOrderId { get; set; }
        public DateTime StartDate { get; set; }
        public decimal? ProductQuantity { get; set; }
        public int ProductId { get; set; }

        public virtual ICollection<ProductionWeekPlanDetail> ProductionWeekPlanDetail { get; set; }
    }
}
