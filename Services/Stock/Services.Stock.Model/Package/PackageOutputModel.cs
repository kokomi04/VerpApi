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

        public int PackageTypeId { get; set; }

        public string PackageCode { get; set; }
        public int? LocationId { get; set; }

        public int StockId { set; get; }

        public int ProductId { set; get; }

        public long? Date { get; set; }
        public long? ExpiryTime { get; set; }
        public string Description { get; set; }
        public int PrimaryUnitId { get; set; }        

        [Obsolete]
        public decimal PrimaryQuantity { get { return PrimaryQuantityRemaining; } }

        public int? ProductUnitConversionId { get; set; }

        [Obsolete]
        public decimal ProductUnitConversionQuantity { get { return ProductUnitConversionRemaining; } }

        public decimal PrimaryQuantityWaiting { get; set; }
        public decimal PrimaryQuantityRemaining { get; set; }
        public decimal ProductUnitConversionWaitting { get; set; }
        public decimal ProductUnitConversionRemaining { get; set; }

        public long? CreatedDatetimeUtc { get; set; }
        public long? UpdatedDatetimeUtc { get; set; }

        public LocationOutput LocationOutputModel { get; set; }

        public ProductUnitConversion ProductUnitConversionModel { set; get; }

        public ProductListOutput ProductOutputModel { set; get; }
    }
}
