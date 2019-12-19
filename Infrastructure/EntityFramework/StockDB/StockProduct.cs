﻿using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class StockProduct
    {
        public int StockProductId { get; set; }
        public int StockId { get; set; }
        public int ProductId { get; set; }
        public int? ProductUnitConversionId { get; set; }
        public int PrimaryUnitId { get; set; }
        public decimal PrimaryQuantityWaiting { get; set; }
        public decimal PrimaryQuantityRemaining { get; set; }
        public decimal ProductUnitConversionWaitting { get; set; }
        public decimal ProductUnitConversionRemaining { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
    }
}
