using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class StockTakePeriod
    {
        public StockTakePeriod()
        {
            StockTake = new HashSet<StockTake>();
            StockTakeRepresentative = new HashSet<StockTakeRepresentative>();
        }

        public long StockTakePeriodId { get; set; }
        public string StockTakePeriodCode { get; set; }
        public DateTime StockTakePeriodDate { get; set; }
        public int StockId { get; set; }
        public int Status { get; set; }
        public string Content { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public bool IsDifference { get; set; }
        public bool IsProcessed { get; set; }
        public DateTime? FinishDate { get; set; }
        public DateTime? FinishAcDate { get; set; }
        public string ConclusionContent { get; set; }
        public virtual StockTakeAcceptanceCertificate StockTakeAcceptanceCertificate { get; set; }
        public virtual ICollection<StockTake> StockTake { get; set; }
        public virtual ICollection<StockTakeRepresentative> StockTakeRepresentative { get; set; }
    }
}
