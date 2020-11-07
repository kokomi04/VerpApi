using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class ProductionStepLinkDataRole
    {
        public long ProductionStepLinkDataId { get; set; }
        public long ProductionStepId { get; set; }
        public int ProductionStepLinkDataRoleTypeId { get; set; }

        public virtual ProductionStepLinkData ProductionStep { get; set; }
        public virtual ProductionStep ProductionStepLinkData { get; set; }
    }
}
