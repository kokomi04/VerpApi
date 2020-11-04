using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class ProductMaterial
    {
        public int ProductId { get; set; }
        public int RootProductId { get; set; }

        public virtual Product Product { get; set; }
        public virtual Product RootProduct { get; set; }
    }
}
