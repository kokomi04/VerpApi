using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class WorkSchedule
    {
        public int WorkScheduleId { get; set; }
        public string WorkScheduleTitle { get; set; }
        public bool IsAbsenceForSaturday { get; set; }
        public bool IsAbsenceForSunday { get; set; }
        public bool IsAbsenceForHoliday { get; set; }
        public bool IsCountWorkForHoliday { get; set; }
        public bool IsDayOfTimeOut { get; set; }
        public int TimeSortConfigurationId { get; set; }
        public int FirstDayForCountedWork { get; set; }
        public bool IsRoundBackForTimeOutAfterWork { get; set; }
        public int RoundLevelForTimeOutAfterWork { get; set; }
        public int DecimalPlace { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual TimeSortConfiguration TimeSortConfiguration { get; set; }
    }
}
