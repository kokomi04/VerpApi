using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.PurchaseOrderDB
{
    public partial class PropertyCalcProperty
    {
        public long PropertyCalcId { get; set; }
        public int PropertyId { get; set; }

        public virtual PropertyCalc PropertyCalc { get; set; }
    }
}
