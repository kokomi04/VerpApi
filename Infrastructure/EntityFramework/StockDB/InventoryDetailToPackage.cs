using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class InventoryDetailToPackage
    {
        public long InventoryDetailId { get; set; }
        public long ToPackageId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }

        public virtual InventoryDetail InventoryDetail { get; set; }
        public virtual Package ToPackage { get; set; }
    }
}
