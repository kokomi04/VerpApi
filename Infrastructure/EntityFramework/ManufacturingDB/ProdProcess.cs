using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class ProdProcess
    {
        public int ProdProcessId { get; set; }
        public string Title { get; set; }
        public long ProductionOrderId { get; set; }
    }
}
