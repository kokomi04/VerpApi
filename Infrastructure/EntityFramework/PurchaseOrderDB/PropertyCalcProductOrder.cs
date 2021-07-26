using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class PropertyCalcProductOrder
    {
        public long PropertyCalcProductId { get; set; }
        public string OrderCode { get; set; }
        public decimal OrderProductQuantity { get; set; }

        public virtual PropertyCalcProduct PropertyCalcProduct { get; set; }
    }
}
