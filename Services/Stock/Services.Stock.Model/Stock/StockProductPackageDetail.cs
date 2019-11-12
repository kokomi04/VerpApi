using System;

namespace VErp.Services.Stock.Model.Stock
{
    public class StockProductPackageDetail
    {
        public long PackageId { get; set; }
        public string PackageCode { get; set; }
        public int? LocationId { get; set; }
        public DateTime Date { get; set; }
        public DateTime? ExpriredDate { get; set; }
        public int PrimaryUnitId { get; set; }
        public decimal PrimaryQuantity { get; set; }
        public int? SecondaryUnitId { get; set; }
        public decimal? SecondaryQuantity { get; set; }
        public long? RefObjectId { get; set; }
        public string RefObjectCode { get; set; }
    }
}
