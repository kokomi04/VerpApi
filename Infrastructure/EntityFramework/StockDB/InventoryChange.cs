using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class InventoryChange
    {
        public long InventoryId { get; set; }
        public DateTime? OldDate { get; set; }
        public bool IsSync { get; set; }
        public DateTime LastSyncTime { get; set; }
    }
}
