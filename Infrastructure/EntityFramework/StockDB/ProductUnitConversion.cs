using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.StockDB;

public partial class ProductUnitConversion
{
    public int ProductUnitConversionId { get; set; }

    public string ProductUnitConversionName { get; set; }

    public int ProductId { get; set; }

    public int SecondaryUnitId { get; set; }

    public string FactorExpression { get; set; }

    public string ConversionDescription { get; set; }

    public bool? IsFreeStyle { get; set; }

    public bool IsDefault { get; set; }

    public int DecimalPlace { get; set; }

    public virtual ICollection<InventoryDetail> InventoryDetail { get; set; } = new List<InventoryDetail>();

    public virtual ICollection<InventoryRequirementDetail> InventoryRequirementDetail { get; set; } = new List<InventoryRequirementDetail>();

    public virtual ICollection<Package> Package { get; set; } = new List<Package>();

    public virtual ICollection<PackageRef> PackageRef { get; set; } = new List<PackageRef>();

    public virtual Product Product { get; set; }
}
