using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class ProductionStepOrder
    {
        public long ProductionStepId { get; set; }
        public long ProductionOrderDetailId { get; set; }
    }
}
