using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class ProductExtraInfo
    {
        public int ProductId { get; set; }
        public string Specification { get; set; }
        public string Description { get; set; }
        public bool IsDeleted { get; set; }

        public virtual Product Product { get; set; }
    }
}
