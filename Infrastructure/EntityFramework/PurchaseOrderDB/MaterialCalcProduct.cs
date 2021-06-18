using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class MaterialCalcProduct
    {
        public MaterialCalcProduct()
        {
            MaterialCalcProductDetail = new HashSet<MaterialCalcProductDetail>();
            MaterialCalcProductOrder = new HashSet<MaterialCalcProductOrder>();
        }

        public long MaterialCalcProductId { get; set; }
        public long MaterialCalcId { get; set; }
        public int ProductId { get; set; }

        public virtual MaterialCalc MaterialCalc { get; set; }
        public virtual ICollection<MaterialCalcProductDetail> MaterialCalcProductDetail { get; set; }
        public virtual ICollection<MaterialCalcProductOrder> MaterialCalcProductOrder { get; set; }
    }
}
