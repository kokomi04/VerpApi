using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class BillOfMaterial
    {
        public long BillOfMaterialId { get; set; }
        public int? Level { get; set; }
        public int RootProductId { get; set; }
        public int ProductId { get; set; }
        public int? ParentProductId { get; set; }
        public decimal? Quantity { get; set; }
        public decimal? Wastage { get; set; }
        public string Description { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }

        public virtual Product ParentProduct { get; set; }
        public virtual Product Product { get; set; }
    }
}
