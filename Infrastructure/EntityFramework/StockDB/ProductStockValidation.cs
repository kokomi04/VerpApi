using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class ProductStockValidation
    {
        public int ProductId { get; set; }
        public int StockId { get; set; }

        public virtual Product Product { get; set; }
    }
}
