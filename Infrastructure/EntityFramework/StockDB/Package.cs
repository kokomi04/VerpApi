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
            InventoryDetailToPackageNavigation = new HashSet<InventoryDetailToPackage>();
            PackageRefPackage = new HashSet<PackageRef>();
            PackageRefRefPackage = new HashSet<PackageRef>();
        }

        public long PackageId { get; set; }
        public int PackageTypeId { get; set; }
        public string PackageCode { get; set; }
        public int? LocationId { get; set; }
        public int StockId { get; set; }
        public int ProductId { get; set; }
        public int ProductUnitConversionId { get; set; }
        public decimal PrimaryQuantityWaiting { get; set; }
        public decimal PrimaryQuantityRemaining { get; set; }
        public decimal ProductUnitConversionWaitting { get; set; }
        public decimal ProductUnitConversionRemaining { get; set; }
        public DateTime? Date { get; set; }
        public DateTime? ExpiryTime { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }

        public virtual Location Location { get; set; }
        public virtual ProductUnitConversion ProductUnitConversion { get; set; }
        public virtual Stock Stock { get; set; }
        public virtual ICollection<InventoryDetail> InventoryDetailFromPackage { get; set; }
        public virtual ICollection<InventoryDetail> InventoryDetailToPackage { get; set; }
        public virtual ICollection<InventoryDetailToPackage> InventoryDetailToPackageNavigation { get; set; }
        public virtual ICollection<PackageRef> PackageRefPackage { get; set; }
        public virtual ICollection<PackageRef> PackageRefRefPackage { get; set; }
    }
}
