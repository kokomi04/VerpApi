using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class PropertyCalcProductDetail
    {
        public long PropertyCalcProductId { get; set; }
        public int PropertyId { get; set; }
        public int MaterialProductId { get; set; }
        public decimal MaterialQuantity { get; set; }

        public virtual PropertyCalcProduct PropertyCalcProduct { get; set; }
    }
}
