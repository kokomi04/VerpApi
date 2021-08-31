using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class MaterialCalcProductOrder
    {
        public long MaterialCalcProductId { get; set; }
        public string OrderCode { get; set; }
        public decimal OrderProductQuantity { get; set; }

        public virtual MaterialCalcProduct MaterialCalcProduct { get; set; }
    }
}
