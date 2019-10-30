using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class Stock
    {
        public Stock()
        {
            Inventory = new HashSet<Inventory>();
        }

        public int StockId { get; set; }
        public string StockName { get; set; }
        public string Description { get; set; }
        public int? StockKeeperId { get; set; }
        public string StockKeeperName { get; set; }
        public int? Type { get; set; }
        public int? Status { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }

        public virtual ICollection<Inventory> Inventory { get; set; }
    }
}
