﻿using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class ProductionStepWorkInfo
    {
        public long ProductionStepId { get; set; }
        public long ScheduleTurnId { get; set; }
        public int HandoverType { get; set; }

        public virtual ProductionStep ProductionStep { get; set; }
    }
}
