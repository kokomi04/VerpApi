using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class MaterialCalcProductDetail
    {
        public long MaterialCalcProductId { get; set; }
        public int ProductMaterialsConsumptionGroupId { get; set; }
        public int MaterialProductId { get; set; }
        public decimal MaterialQuantity { get; set; }

        public virtual MaterialCalcProduct MaterialCalcProduct { get; set; }
    }
}
