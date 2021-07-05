using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Stock.Model.Stock
{
    public class StockProductListOutput
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public int? ProductTypeId { get; set; }
        public int ProductCateId { get; set; }
        public string Specification { get; set; }
        public int UnitId { get; set; }
        public decimal PrimaryQuantityRemaining { get; set; }
        public int ProductUnitConversionId { get; set; }
        public string ProductUnitConversionName { get; set; }
        public decimal ProductUnitConversionRemaining { get; set; }
        public int DecimalPlace { get; set; }
    }
}
