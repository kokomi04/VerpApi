using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Stock.Model.Stock
{
    public class RemainStock
    {
        public int StockId { get; set; }
        public string StockName { get; set; }
        public decimal PrimaryQuantityRemaining { get; set; }
    }
}
