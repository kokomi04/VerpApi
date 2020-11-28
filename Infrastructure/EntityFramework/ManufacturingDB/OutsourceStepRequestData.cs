﻿using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class OutsourceStepRequestData
    {
        public long OutsourceStepRequestId { get; set; }
        public long ProductionStepLinkDataId { get; set; }
        public decimal? Quantity { get; set; }
        public int ProductionStepLinkDataRoleTypeId { get; set; }

        public virtual OutsourceStepRequest OutsourceStepRequest { get; set; }
        public virtual ProductionStepLinkData ProductionStepLinkData { get; set; }
    }
}
