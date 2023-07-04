using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.StockDB;

public partial class Package
{
    public long PackageId { get; set; }

    public int SubsidiaryId { get; set; }

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

    public int CreatedByUserId { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public int UpdatedByUserId { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedDatetimeUtc { get; set; }

    public string OrderCode { get; set; }

    public string ProductionOrderCode { get; set; }

    /// <summary>
    /// Purchasing Order Code
    /// </summary>
    public string Pocode { get; set; }

    public string CustomPropertyValue { get; set; }

    public virtual ICollection<InventoryDetail> InventoryDetailFromPackage { get; set; } = new List<InventoryDetail>();

    public virtual ICollection<InventoryDetail> InventoryDetailToPackage { get; set; } = new List<InventoryDetail>();

    public virtual ICollection<InventoryDetailToPackage> InventoryDetailToPackageNavigation { get; set; } = new List<InventoryDetailToPackage>();

    public virtual Location Location { get; set; }

    public virtual ICollection<PackageRef> PackageRefPackage { get; set; } = new List<PackageRef>();

    public virtual ICollection<PackageRef> PackageRefRefPackage { get; set; } = new List<PackageRef>();

    public virtual ProductUnitConversion ProductUnitConversion { get; set; }

    public virtual Stock Stock { get; set; }
}
