﻿using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB;

public partial class ProductionStepLinkDataRole
{
    public long ProductionStepLinkDataId { get; set; }

    public long ProductionStepId { get; set; }

    /// <summary>
    /// 1: Input
    /// 2: Output
    /// </summary>
    public int ProductionStepLinkDataRoleTypeId { get; set; }

    public string ProductionStepLinkDataGroupBak { get; set; }

    public virtual ProductionStep ProductionStep { get; set; }

    public virtual ProductionStepLinkData ProductionStepLinkData { get; set; }
}
