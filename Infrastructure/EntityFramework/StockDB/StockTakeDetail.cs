using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class StockTakeDetail
    {
        public long StockTakeDetailId { get; set; }
        public long StockTakeId { get; set; }
        public int ProductId { get; set; }
        public int PackageId { get; set; }
        public decimal StockTakeQuantity { get; set; }
        public string Note { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual StockTake StockTake { get; set; }
    }
}
