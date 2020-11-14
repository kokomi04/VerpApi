using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class ProductionStepRoleClient
    {
        public int ContainerId { get; set; }
        public int ContainerTypeId { get; set; }
        public string ClientData { get; set; }
    }
}
