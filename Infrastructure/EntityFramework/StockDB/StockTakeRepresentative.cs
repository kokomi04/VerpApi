using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class StockTakeRepresentative
    {
        public long StockTakePeriodId { get; set; }
        public int UserId { get; set; }

        public virtual StockTakePeriod StockTakePeriod { get; set; }
    }
}
