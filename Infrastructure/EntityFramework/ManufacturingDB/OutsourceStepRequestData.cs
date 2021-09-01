using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class OutsourceStepRequestData
    {
        public long OutsourceStepRequestId { get; set; }
        public long ProductionStepId { get; set; }
        public long ProductionStepLinkDataId { get; set; }
        public decimal Quantity { get; set; }
        public int ProductionStepLinkDataRoleTypeId { get; set; }
        public bool? IsImportant { get; set; }

        public virtual OutsourceStepRequest OutsourceStepRequest { get; set; }
        public virtual ProductionStep ProductionStep { get; set; }
        public virtual ProductionStepLinkData ProductionStepLinkData { get; set; }
    }
}
