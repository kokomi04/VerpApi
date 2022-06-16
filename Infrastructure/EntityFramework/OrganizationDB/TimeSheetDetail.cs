using System;

#nullable disable

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class TimeSheetDetail
    {
        public long TimeSheetDetailId { get; set; }
        public long TimeSheetId { get; set; }
        public long EmployeeId { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan? TimeIn { get; set; }
        public TimeSpan? TimeOut { get; set; }
        public int? AbsenceTypeSymbolId { get; set; }
        public long? MinsOvertime { get; set; }
        public long? MinsLate { get; set; }
        public long? MinsEarly { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual TimeSheet TimeSheet { get; set; }
    }
}
