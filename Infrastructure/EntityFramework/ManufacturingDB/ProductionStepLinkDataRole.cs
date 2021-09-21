using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class ProductionStepLinkDataRole
    {
        public long ProductionStepLinkDataId { get; set; }
        public long ProductionStepId { get; set; }
        public int ProductionStepLinkDataRoleTypeId { get; set; }
        public string ProductionStepLinkDataGroup { get; set; }

        public virtual ProductionStep ProductionStep { get; set; }
        public virtual ProductionStepLinkData ProductionStepLinkData { get; set; }
    }
}
