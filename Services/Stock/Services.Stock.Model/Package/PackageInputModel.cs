using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Stock.Model.Package
{
    public class PackageInputModel
    {
        //public long PackageId { get; set; }
        //public long? InventoryDetailId { get; set; }
        public string PackageCode { get; set; }
        public int? LocationId { get; set; }

        public int StockId { set; get; }

        public int ProductId { set; get; }

        public string Date { get; set; }
        public string ExpiryTime { get; set; }

        public int ProductUnitConversionId { set; get; }

        public int PrimaryUnitId { get; set; }
        public decimal PrimaryQuantity { get; set; }
        public int? SecondaryUnitId { get; set; }
        public decimal SecondaryQuantity { get; set; }
        //public DateTime CreatedDatetimeUtc { get; set; }
        //public DateTime UpdatedDatetimeUtc { get; set; }
        //public bool IsDeleted { get; set; }
        public decimal PrimaryQuantityWaiting { get; set; }
        public decimal PrimaryQuantityRemaining { get; set; }
        public decimal SecondaryQuantityWaitting { get; set; }
        public decimal SecondaryQuantityRemaining { get; set; }

        public int PackageType { set; get; }

        //public virtual Location Location { get; set; }
    }
}
