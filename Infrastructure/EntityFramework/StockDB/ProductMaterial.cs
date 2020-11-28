using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class ProductMaterial
    {
        public int ProductMaterialId { get; set; }
        public int RootProductId { get; set; }
        public int ProductId { get; set; }
        public string PathProductIds { get; set; }
    }
}
