using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Manafacturing.Model.ProductionHandover
{
    public class ProductionHandoverAcceptBatchInput
    {
        public long ProductionOrderId { get; set; }
        public long productionHandoverId { get; set; }
    }
}
