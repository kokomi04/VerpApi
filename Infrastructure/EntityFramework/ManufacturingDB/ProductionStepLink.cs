using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class ProductionStepLink
    {
        public int ProductId { get; set; }
        public int FromStepId { get; set; }
        public int ToStepId { get; set; }
    }
}
