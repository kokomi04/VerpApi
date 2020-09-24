using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class InventoryConfig
    {
        public int SubsidiaryId { get; set; }
        public DateTime ClosingDate { get; set; }
        public bool AutoClosingDate { get; set; }
        public string FreqClosingDate { get; set; }
    }
}
