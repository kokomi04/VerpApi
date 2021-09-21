using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class PropertyCalcProduct
    {
        public PropertyCalcProduct()
        {
            PropertyCalcProductDetail = new HashSet<PropertyCalcProductDetail>();
            PropertyCalcProductOrder = new HashSet<PropertyCalcProductOrder>();
        }

        public long PropertyCalcProductId { get; set; }
        public long PropertyCalcId { get; set; }
        public int ProductId { get; set; }

        public virtual PropertyCalc PropertyCalc { get; set; }
        public virtual ICollection<PropertyCalcProductDetail> PropertyCalcProductDetail { get; set; }
        public virtual ICollection<PropertyCalcProductOrder> PropertyCalcProductOrder { get; set; }
    }
}
