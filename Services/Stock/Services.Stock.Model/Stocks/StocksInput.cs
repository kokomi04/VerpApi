using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Stock.Model.Stocks
{
    public class StocksInput
    {
        public int StockId { get; set; }
        public string StockName { get; set; }
    }

    public class StocksModel
    {
        //public int StockId { get; set; }
        public string StockName { get; set; }
    }
}
