﻿using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class ProductionStepInOutConverter
    {
        public long InputProductionStepLinkDataId { get; set; }
        public long OutputProductionStepLinkDataId { get; set; }
    }
}