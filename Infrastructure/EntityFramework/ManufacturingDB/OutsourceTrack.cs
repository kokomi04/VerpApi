using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class OutsourceTrack
    {
        public long OutsourceTrackId { get; set; }
        public long OutsourceOrderId { get; set; }
        public int OutsourceTrackTypeId { get; set; }
        public DateTime OutsourceTrackDate { get; set; }
        public long? ObjectId { get; set; }
        public string Description { get; set; }
        public int OutsourceTrackStatusId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public int SubsidiaryId { get; set; }
        public decimal? Quantity { get; set; }

        public virtual OutsourceOrder OutsourceOrder { get; set; }
    }
}
