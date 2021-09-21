using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class StockTake
    {
        public StockTake()
        {
            StockTakeDetail = new HashSet<StockTakeDetail>();
        }

        public long StockTakeId { get; set; }
        public long StockTakePeriodId { get; set; }
        public string StockTakeCode { get; set; }
        public DateTime StockTakeDate { get; set; }
        public int StockRepresentativeId { get; set; }
        public string Content { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public int AccountancyRepresentativeId { get; set; }
        public int StockStatus { get; set; }
        public int AccountancyStatus { get; set; }

        public virtual StockTakePeriod StockTakePeriod { get; set; }
        public virtual ICollection<StockTakeDetail> StockTakeDetail { get; set; }
    }
}
