using System;

#nullable disable

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class Leave
    {
        public long LeaveId { get; set; }
        public int LeaveConfigId { get; set; }
        public int? UserId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DateStart { get; set; }
        public bool DateStartIsHalf { get; set; }
        public DateTime DateEnd { get; set; }
        public bool DateEndIsHalf { get; set; }
        public decimal TotalDays { get; set; }
        public decimal TotalDaysLastYearUsed { get; set; }
        public long? FileId { get; set; }
        public int AbsenceTypeSymbolId { get; set; }
        public int LeaveStatusId { get; set; }
        public int? CheckedByUserId { get; set; }
        public int? CensoredByUserId { get; set; }
        public DateTime? CheckedDatetimeUtc { get; set; }
        public DateTime? CensoredDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual AbsenceTypeSymbol AbsenceTypeSymbol { get; set; }
        public virtual LeaveConfig LeaveConfig { get; set; }
    }
}
