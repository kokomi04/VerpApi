﻿using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class RefProductionOrder
    {
        public long ProductionOrderId { get; set; }
        public string ProductionOrderCode { get; set; }
    }
}
