using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class MaterialCalcConsumptionGroup
    {
        public long MaterialCalcId { get; set; }
        public int ProductMaterialsConsumptionGroupId { get; set; }

        public virtual MaterialCalc MaterialCalc { get; set; }
    }
}
