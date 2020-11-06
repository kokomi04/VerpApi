using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class ProductInStepLink
    {
        public int InputProductInStepId { get; set; }
        public int OutputProductInStepId { get; set; }
    }
}
