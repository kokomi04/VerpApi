using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ManufacturingDB
{
    public partial class TrackOutsource
    {
        public int TrackOutsourceId { get; set; }
        public int OutsourceType { get; set; }
        public int OutsourceId { get; set; }
        public string Description { get; set; }
        public int Status { get; set; }
        public DateTime DateTrack { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
    }
}
