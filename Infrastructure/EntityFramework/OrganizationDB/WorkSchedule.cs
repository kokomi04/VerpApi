using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class WorkSchedule
    {
        public WorkSchedule()
        {
            ArrangeShift = new HashSet<ArrangeShift>();
            WorkScheduleMark = new HashSet<WorkScheduleMark>();
        }

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
        public bool IsCountInShift { get; set; }
        public long? TotalMins { get; set; }
        public int? CountShift { get; set; }
        public long? MinsThresholdRecord { get; set; }
        public bool? IsMinsThresholdRecord { get; set; }
        public long? MinsThresholdOvertime1 { get; set; }
        public long? MinsThresholdOvertime2 { get; set; }

        public virtual TimeSortConfiguration TimeSortConfiguration { get; set; }
        public virtual ICollection<ArrangeShift> ArrangeShift { get; set; }
        public virtual ICollection<WorkScheduleMark> WorkScheduleMark { get; set; }
    }
}
