using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class ProductionStepWorkInfo
    {
        public long ProductionStepId { get; set; }
        public int HandoverType { get; set; }
        public decimal? MinHour { get; set; }
        public decimal? MaxHour { get; set; }

        public virtual ProductionStep ProductionStep { get; set; }
    }
}
