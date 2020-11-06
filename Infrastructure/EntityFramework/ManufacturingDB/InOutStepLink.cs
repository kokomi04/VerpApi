using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class InOutStepLink
    {
        public int ProductionStepId { get; set; }
        public int ProductInStepId { get; set; }
        public int InOutStepType { get; set; }

        public virtual ProductInStep ProductInStep { get; set; }
        public virtual ProductionStep ProductionStep { get; set; }
    }
}
