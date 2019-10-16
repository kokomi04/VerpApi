using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class Stock
    {
        public int StockId { get; set; }
        public string StockName { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
    }
}
