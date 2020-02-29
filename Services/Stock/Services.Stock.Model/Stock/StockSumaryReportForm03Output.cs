using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Stock.Model.Stock
{
    public class StockSumaryReportForm03Output
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public int UnitId { get; set; }

        public string UnitName { get; set; }

        public decimal PrimaryQualtityBefore { get; set; }
        public decimal PrimaryQualtityInput { get; set; }
        public decimal PrimaryQualtityOutput { get; set; }
        public decimal PrimaryQualtityAfter { get; set; }
               
        public long PakageIdNotExport { set; get; }
        public string PakageCodeNotExport { set; get; }

        public long PakageDateNotExport { set; get; }

        public List<ProductAltSummary> ProductAltSummaryList { set; get; }
    }

    public class ProductAltSummary
    {
        public int ProductId { get; set; }

        public int? ProductUnitConversionId { get; set; }

        public int UnitId { get; set; }

        public string ProductUnitCoversionName { get; set; }

        public string ConversionDescription { get; set; }

        public decimal ProductUnitConversionQuantityBefore { get; set; }
        public decimal ProductUnitConversionQuantityInput { get; set; }
        public decimal ProductUnitConversionQuantityOutput { get; set; }
        public decimal ProductUnitConversionQuantityAfter { get; set; }
    }
}
