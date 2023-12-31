﻿using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB;

public partial class IgnoreAllocation
{
    public long ProductionOrderId { get; set; }

    public string InventoryCode { get; set; }

    public int ProductId { get; set; }

    public int SubsidiaryId { get; set; }

    public long? InventoryDetailId { get; set; }
}
