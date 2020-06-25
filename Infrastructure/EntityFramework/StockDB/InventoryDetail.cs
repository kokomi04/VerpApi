using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class InventoryDetail
    {
        public InventoryDetail()
        {
            InventoryDetailToPackage = new HashSet<InventoryDetailToPackage>();
        }

        public long InventoryDetailId { get; set; }
        public long InventoryId { get; set; }
        public int ProductId { get; set; }
        public decimal? RequestPrimaryQuantity { get; set; }
        public decimal PrimaryQuantity { get; set; }
        public decimal UnitPrice { get; set; }
        public int ProductUnitConversionId { get; set; }
        public decimal? RequestProductUnitConversionQuantity { get; set; }
        public decimal ProductUnitConversionQuantity { get; set; }
        public decimal? ProductUnitConversionPrice { get; set; }
        public long? FromPackageId { get; set; }
        public long? ToPackageId { get; set; }
        public int? PackageOptionId { get; set; }
        public int? RefObjectTypeId { get; set; }
        public long? RefObjectId { get; set; }
        public string RefObjectCode { get; set; }
        public string OrderCode { get; set; }
        public string Pocode { get; set; }
        public string ProductionOrderCode { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public int? SortOrder { get; set; }
        public decimal? PrimaryQuantityRemaning { get; set; }
        public decimal? ProductUnitConversionQuantityRemaning { get; set; }

        public virtual Package FromPackage { get; set; }
        public virtual Inventory Inventory { get; set; }
        public virtual Product Product { get; set; }
        public virtual ProductUnitConversion ProductUnitConversion { get; set; }
        public virtual Package ToPackage { get; set; }
        public virtual ICollection<InventoryDetailToPackage> InventoryDetailToPackage { get; set; }
    }
}
