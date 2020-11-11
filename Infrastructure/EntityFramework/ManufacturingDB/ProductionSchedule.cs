using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class ProductionSchedule
    {
        public int ProductionOrderDetailId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Status { get; set; }

        public virtual ProductionOrderDetail ProductionOrderDetail { get; set; }
    }
}
