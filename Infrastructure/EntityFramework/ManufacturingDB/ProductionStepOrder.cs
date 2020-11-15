using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class ProductionStepOrder
    {
        public long ProductionStepId { get; set; }
        public int ProductionOrderDetailId { get; set; }

        public virtual ProductionOrderDetail ProductionOrderDetail { get; set; }
        public virtual ProductionStep ProductionStep { get; set; }
    }
}
