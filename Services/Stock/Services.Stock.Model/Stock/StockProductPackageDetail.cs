using System;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Services.Stock.Model.Stock
{
    public class StockProductPackageDetail
    {
        public long PackageId { get; set; }
        public string PackageCode { get; set; }
        public int? LocationId { get; set; }
        public string LocationName { get; set; }
        public long? Date { get; set; }
        public long? ExpriredDate { get; set; }
        public int PrimaryUnitId { get; set; }
        public decimal PrimaryQuantity { get; set; }       
        public int? ProductUnitConversionId { get; set; }
        public string ProductUnitConversionName { get; set; }
        public int? SecondaryUnitId { get; set; }
        public decimal ProductUnitConversionQualtity { get; set; }
        public decimal SecondaryUnitQualtity { get { return ProductUnitConversionQualtity; } }
        public EnumPackageType PackageTypeId { get; set; }
        public long? RefObjectId { get; set; }
        public string RefObjectCode { get; set; }
    }
}
