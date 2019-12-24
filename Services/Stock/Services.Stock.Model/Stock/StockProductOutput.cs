using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Stock.Model.Stock
{
    public class StockProductOutput
    {
        public int StockId { set; get; }

        public int ProductId { set; get; }

        public int PrimaryUnitId { set; get; }

        public decimal PrimaryQuantityRemaining { set; get; }

        public int? ProductUnitConversionId { set; get; }

        public decimal ProductUnitConversionRemaining { set; get; }
    }
}
