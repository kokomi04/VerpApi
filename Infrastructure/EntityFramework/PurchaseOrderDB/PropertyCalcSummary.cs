using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class PropertyCalcSummary
    {
        public long PropertyCalcSummaryId { get; set; }
        public long PropertyCalcId { get; set; }
        public int OriginalMaterialProductId { get; set; }
        public int MaterialProductId { get; set; }
        public decimal MaterialQuantity { get; set; }
        public decimal ExchangeRate { get; set; }
        public int PropertyId { get; set; }

        public virtual PropertyCalc PropertyCalc { get; set; }
    }
}
