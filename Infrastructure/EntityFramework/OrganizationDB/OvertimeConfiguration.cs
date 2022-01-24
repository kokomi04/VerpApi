using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class OvertimeConfiguration
    {
        public OvertimeConfiguration()
        {
            ShiftConfiguration = new HashSet<ShiftConfiguration>();
        }

        public int OvertimeConfigurationId { get; set; }
        public int OvertimeLevel { get; set; }
        public bool IsOvertimeLevel { get; set; }
        public int WeekendLevel { get; set; }
        public bool IsWeekendLevel { get; set; }
        public int HolidayLevel { get; set; }
        public bool IsHolidayLevel { get; set; }
        public long MinsAfterWork { get; set; }
        public long IsMinsAfterWork { get; set; }
        public long MinsBeforeWork { get; set; }
        public long IsMinsBeforeWork { get; set; }
        public int TotalHourWillCountShift { get; set; }
        public int IsTotalHourWillCountShift { get; set; }
        public long MinsReachesBeforeWork { get; set; }
        public long MinsReachesAfterWork { get; set; }
        public long MinsBonusWhenMinsReachesBeforeWork { get; set; }
        public long MinsBonusWhenMinsReachesAfterWork { get; set; }
        public long MinsLimitOvertime1 { get; set; }
        public long MinsLimitOvertime2 { get; set; }
        public bool IsDayShiftLevel { get; set; }
        public int DayShiftLevel { get; set; }
        public int NightShiftLevel { get; set; }
        public bool IsNightShiftLevel { get; set; }
        public long MinsLimitOvertimeBeforeWork { get; set; }
        public long MinsLimitOvertimeAfterWork { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }

        public virtual ICollection<ShiftConfiguration> ShiftConfiguration { get; set; }
    }
}
