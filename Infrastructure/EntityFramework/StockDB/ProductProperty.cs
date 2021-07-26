using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class ProductProperty
    {
        public long ProductPropertyId { get; set; }
        public int RootProductId { get; set; }
        public int ProductId { get; set; }
        public string PathProductIds { get; set; }
        public int PropertyId { get; set; }

        public virtual Product Product { get; set; }
        public virtual Property Property { get; set; }
        public virtual Product RootProduct { get; set; }
    }
}
