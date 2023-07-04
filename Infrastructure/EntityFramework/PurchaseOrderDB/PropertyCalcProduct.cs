using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.PurchaseOrderDB;

public partial class PropertyCalcProduct
{
    public long PropertyCalcProductId { get; set; }

    public long PropertyCalcId { get; set; }

    public int ProductId { get; set; }

    public virtual PropertyCalc PropertyCalc { get; set; }

    public virtual ICollection<PropertyCalcProductDetail> PropertyCalcProductDetail { get; set; } = new List<PropertyCalcProductDetail>();

    public virtual ICollection<PropertyCalcProductOrder> PropertyCalcProductOrder { get; set; } = new List<PropertyCalcProductOrder>();
}
