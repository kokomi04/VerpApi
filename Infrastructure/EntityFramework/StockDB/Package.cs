using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class Package
    {
        public Package()
        {
            InventoryDetailFromPackage = new HashSet<InventoryDetail>();
            InventoryDetailToPackage = new HashSet<InventoryDetail>();
        }

        public long PackageId { get; set; }
        public string PackageCode { get; set; }
        public int? LocationId { get; set; }
        public int? StockId { get; set; }
        public int? ProductId { get; set; }
        public DateTime? Date { get; set; }
        public DateTime? ExpiryTime { get; set; }
        public int PrimaryUnitId { get; set; }
        public decimal PrimaryQuantity { get; set; }
        public int? ProductUnitConversionId { get; set; }
        public int? SecondaryUnitId { get; set; }
        public decimal? SecondaryQuantity { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public decimal PrimaryQuantityWaiting { get; set; }
        public decimal PrimaryQuantityRemaining { get; set; }
        public decimal SecondaryQuantityWaitting { get; set; }
        public decimal SecondaryQuantityRemaining { get; set; }
        public int PackageType { get; set; }

        public virtual Location Location { get; set; }
        public virtual Stock Stock { get; set; }
        public virtual ICollection<InventoryDetail> InventoryDetailFromPackage { get; set; }
        public virtual ICollection<InventoryDetail> InventoryDetailToPackage { get; set; }
    }
}
