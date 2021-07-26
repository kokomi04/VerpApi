using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class ProductMaterial
    {
        public long ProductMaterialId { get; set; }
        public int RootProductId { get; set; }
        public int ProductId { get; set; }
        public string PathProductIds { get; set; }

        public virtual Product Product { get; set; }
        public virtual Product RootProduct { get; set; }
    }
}
