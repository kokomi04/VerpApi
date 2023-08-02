using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB;

public partial class ProductionStepRoleClient
{
    public long ContainerId { get; set; }

    /// <summary>
    /// 1-SP 2-LSX
    /// </summary>
    public int ContainerTypeId { get; set; }

    public string ClientData { get; set; }
}
