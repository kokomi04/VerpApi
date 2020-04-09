using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class InventoryDetailChange
    {
        public long InventoryDetailId { get; set; }
        public long InventoryId { get; set; }
        public int StockId { get; set; }
        public decimal OldPrimaryQuantity { get; set; }
        public bool IsDeleted { get; set; }
        public int ProductId { get; set; }
    }
}
