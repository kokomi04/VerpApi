using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class ProductMaterial
    {
        public int RootProductId { get; set; }
        public int ParentProductId { get; set; }
        public int ProductId { get; set; }
    }
}
