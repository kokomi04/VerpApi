﻿using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class ProductionStepMoldLink
    {
        public long ProductionStepMoldLinkId { get; set; }
        public long FromProductionStepMoldId { get; set; }
        public long ToProductionStepMoldId { get; set; }

        public virtual ProductionStepMold FromProductionStepMold { get; set; }
        public virtual ProductionStepMold ToProductionStepMold { get; set; }
    }
}
