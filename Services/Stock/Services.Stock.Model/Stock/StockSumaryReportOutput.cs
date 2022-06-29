﻿namespace VErp.Services.Stock.Model.Stock
{
    public class StockSumaryReportOutput
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
    }
}
