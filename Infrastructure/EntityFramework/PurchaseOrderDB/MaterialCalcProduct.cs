using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.PurchaseOrderDB;

public partial class MaterialCalcProduct
{
    public long MaterialCalcProductId { get; set; }

    public long MaterialCalcId { get; set; }

    public int ProductId { get; set; }

    public virtual MaterialCalc MaterialCalc { get; set; }

    public virtual ICollection<MaterialCalcProductDetail> MaterialCalcProductDetail { get; set; } = new List<MaterialCalcProductDetail>();

    public virtual ICollection<MaterialCalcProductOrder> MaterialCalcProductOrder { get; set; } = new List<MaterialCalcProductOrder>();
}
