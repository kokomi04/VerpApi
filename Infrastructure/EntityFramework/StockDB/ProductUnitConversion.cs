using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class ProductUnitConversion
    {
        public ProductUnitConversion()
        {
            InventoryDetail = new HashSet<InventoryDetail>();
            InventoryRequirementDetail = new HashSet<InventoryRequirementDetail>();
            Package = new HashSet<Package>();
            PackageRef = new HashSet<PackageRef>();
        }

        public int ProductUnitConversionId { get; set; }
        public string ProductUnitConversionName { get; set; }
        public int ProductId { get; set; }
        public int SecondaryUnitId { get; set; }
        public string FactorExpression { get; set; }
        public string ConversionDescription { get; set; }
        public bool? IsFreeStyle { get; set; }
        public bool IsDefault { get; set; }
        public int DecimalPlace { get; set; }

        public virtual Product Product { get; set; }
        public virtual ICollection<InventoryDetail> InventoryDetail { get; set; }
        public virtual ICollection<InventoryRequirementDetail> InventoryRequirementDetail { get; set; }
        public virtual ICollection<Package> Package { get; set; }
        public virtual ICollection<PackageRef> PackageRef { get; set; }
    }
}
