using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class InventoryDetail
    {
        public long InventoryDetailId { get; set; }
        public long InventoryId { get; set; }
        public int ProductId { get; set; }
        public int PrimaryUnitId { get; set; }
        public decimal PrimaryQuantity { get; set; }
        public decimal UnitPrice { get; set; }
        public int? ProductUnitConversionId { get; set; }
        public decimal ProductUnitConversionQuantity { get; set; }
        public long? FromPackageId { get; set; }
        public long? ToPackageId { get; set; }
        public int? PackageOptionId { get; set; }
        public int? RefObjectTypeId { get; set; }
        public long? RefObjectId { get; set; }
        public string RefObjectCode { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }

        public virtual Package FromPackage { get; set; }
        public virtual Inventory Inventory { get; set; }
        public virtual Product Product { get; set; }
        public virtual ProductUnitConversion ProductUnitConversion { get; set; }
        public virtual Package ToPackage { get; set; }
    }
}
