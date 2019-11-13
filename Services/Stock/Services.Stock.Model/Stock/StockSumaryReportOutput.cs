using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Stock.Model.Stock
{
    public class StockSumaryReportOutput
    {
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public int UnitId { get; set; }
        public decimal PrimaryQualtityBefore { get; set; }
        public decimal PrimaryQualtityInput { get; set; }
        public decimal PrimaryQualtityOutput { get; set; }
        public decimal PrimaryQualtityAfter { get; set; }        
    }
}
