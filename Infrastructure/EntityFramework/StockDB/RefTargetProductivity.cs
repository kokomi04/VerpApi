using System;

#nullable disable

namespace VErp.Infrastructure.EF.StockDB
{
    public partial class RefTargetProductivity
    {
        public int TargetProductivityId { get; set; }
        public string TargetProductivityCode { get; set; }
        public DateTime TargetProductivityDate { get; set; }
        public bool IsDefault { get; set; }
        public string Note { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
    }
}
