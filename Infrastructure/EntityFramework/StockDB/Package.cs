using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class Package
    {
        public long PackageId { get; set; }
        public long? InventoryDetailId { get; set; }
        public string PackageCode { get; set; }
        public int? LocationId { get; set; }
        public DateTime? Date { get; set; }
        public DateTime? ExpiryTime { get; set; }
        public int PrimaryUnitId { get; set; }
        public decimal PrimaryQuantity { get; set; }
        public decimal? SecondaryUnitId { get; set; }
        public decimal? SecondaryQuantity { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public decimal PrimaryQuantityWaiting { get; set; }
        public decimal PrimaryQuantityRemaining { get; set; }
        public decimal SecondaryQuantityWaitting { get; set; }
        public decimal SecondaryQuantityRemaining { get; set; }

        public virtual Location Location { get; set; }
    }
}
