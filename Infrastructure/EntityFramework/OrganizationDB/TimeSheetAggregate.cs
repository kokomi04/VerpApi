using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class TimeSheetAggregate
    {
        public long TimeSheetAggregateId { get; set; }
        public long TimeSheetId { get; set; }
        public int EmployeeId { get; set; }
        public int CountedWeekday { get; set; }
        public int CountedWeekend { get; set; }
        public long CountedWeekdayHour { get; set; }
        public long CountedWeekendHour { get; set; }
        public long MinsLate { get; set; }
        public int CountedLate { get; set; }
        public long MinsEarly { get; set; }
        public int CountedEarly { get; set; }
        public long Overtime1 { get; set; }
        public long Overtime2 { get; set; }
        public long Overtime3 { get; set; }
        public int CountedAbsence { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }

        public virtual TimeSheet TimeSheet { get; set; }
    }
}
