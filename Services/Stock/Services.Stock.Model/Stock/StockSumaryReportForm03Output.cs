using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Stock.Model.Stock
{
    public class StockSumaryReportForm3Data
    {
        public long RankNumber { get; set; }
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public int? ProductUnitConversionId { get; set; }
        public string ProductUnitConversionName { get; set; }
        public decimal? StartPrimaryRemaing { get; set; }
        public decimal? StartProductUnitConversionRemaining { get; set; }
        public decimal? InPrimary { get; set; }
        public decimal? InProductUnitConversion { get; set; }
        public decimal? OutPrimaryRemaing { get; set; }
        public decimal? OutProductUnitConversion { get; set; }
        public decimal? PrimaryRemaing { get; set; }
        public decimal? ProductUnitConversionRemaining { get; set; }
        public DateTime? MaxInputDate { get; set; }
        public long? TotalRecord { get; set; }
    }
    public class StockSumaryReportForm03Output
    {
        public long RankNumber { get; set; }
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        //public int UnitId { get; set; }

        //public string UnitName { get; set; }

        public decimal SumPrimaryQuantityBefore { get; set; }
        public decimal SumPrimaryQuantityInput { get; set; }
        public decimal SumPrimaryQuantityOutput { get; set; }
        public decimal SumPrimaryQuantityAfter { get; set; }

        public long PakageIdNotExport { set; get; }
        public string PakageCodeNotExport { set; get; }

        public long PakageDateNotExport { set; get; }

        public List<ProductAltSummary> ProductAltSummaryList { set; get; }
    }

    public class ProductAltSummary
    {
        public long RankNumber { get; set; }
        public int ProductId { get; set; }


        public int UnitId { get; set; }
        public decimal PrimaryQuantityBefore { get; set; }
        public decimal PrimaryQuantityInput { get; set; }
        public decimal PrimaryQuantityOutput { get; set; }
        public decimal PrimaryQuantityAfter { get; set; }


        public int? ProductUnitConversionId { get; set; }
        public string ProductUnitCoversionName { get; set; }

        //public string ConversionDescription { get; set; }

        public decimal ProductUnitConversionQuantityBefore { get; set; }
        public decimal ProductUnitConversionQuantityInput { get; set; }
        public decimal ProductUnitConversionQuantityOutput { get; set; }
        public decimal ProductUnitConversionQuantityAfter { get; set; }
    }
}
