using System;
using System.Collections.Generic;
using System.Text;
using VErp.Infrastructure.EF.StockDB;
using VErp.Services.Stock.Model.Location;
using VErp.Services.Stock.Model.Product;

namespace VErp.Services.Stock.Model.Package
{
    public class PackageOutputModel
    {
        public PackageOutputModel() 
        {
            LocationId = null;
            LocationOutputModel = null;
            ProductUnitConversionModel = null;
        }

        public long PackageId { get; set; }
        public string PackageCode { get; set; }
        public int? LocationId { get; set; }

        public int? StockId { set; get; }

        public int? ProductId { set; get; }

        public DateTime? Date { get; set; }
        public DateTime? ExpiryTime { get; set; }
        public int PrimaryUnitId { get; set; }
        public decimal PrimaryQuantity { get; set; }

        public int? ProductUnitConversionId { set; get; }

        public decimal? SecondaryQuantity { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        //public bool IsDeleted { get; set; }
        public decimal PrimaryQuantityWaiting { get; set; }
        public decimal PrimaryQuantityRemaining { get; set; }
        public decimal SecondaryQuantityWaitting { get; set; }
        public decimal SecondaryQuantityRemaining { get; set; }

        public int PackageType { set; get; }

        public LocationOutput LocationOutputModel { get; set; }

        public ProductUnitConversion ProductUnitConversionModel { set; get; }

        public ProductListOutput ProductOutputModel { set; get; }
    }
}
