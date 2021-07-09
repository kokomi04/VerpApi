using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class ProductMaterialProperty
    {
        public int ProductMaterialPropertyId { get; set; }
        public int RootProductId { get; set; }
        public int ProductId { get; set; }
        public string PathProductIds { get; set; }
        public int ProductPropertyId { get; set; }

        public virtual ProductProperty ProductProperty { get; set; }
    }
}
