using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class InventoryFile
    {
        public long InventoryId { get; set; }
        public long FileId { get; set; }
        public bool IsDeleted { get; set; }
    }
}
