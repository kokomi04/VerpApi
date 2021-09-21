using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class ProductionOrderStatus
    {
        public int ProductionOrderStatusId { get; set; }
        public string ProductionOrderStatusName { get; set; }
        public string Description { get; set; }
    }
}
