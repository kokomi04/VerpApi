﻿using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class ProductionOrderMaterialSetConsumptionGroup
    {
        public long ProductionOrderMaterialSetId { get; set; }
        public int ProductMaterialsConsumptionGroupId { get; set; }

        public virtual ProductionOrderMaterialSet ProductionOrderMaterialSet { get; set; }
    }
}
