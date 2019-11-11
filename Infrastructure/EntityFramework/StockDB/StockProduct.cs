using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class StockProduct
    {
        public int StockId { get; set; }
        public int ProductId { get; set; }
        public int SecondaryUnitId { get; set; }
        public decimal? SecondaryQuantity { get; set; }

        public int PrimaryUnitId { get; set; }
        public decimal PrimaryQuantity { get; set; }

        public decimal PrimaryQuantityWaiting { get; set; }
        public decimal PrimaryQuantityRemaining { get; set; }
        public decimal SecondaryQuantityWaitting { get; set; }
        public decimal SecondaryQuantityRemaining { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }

        public virtual Product Product { get; set; }
        public virtual Stock Stock { get; set; }
    }
}
