using System;
using AutoMapper;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.OrganizationDB;

namespace Services.Organization.Model.TimeKeeping
{
    public class OvertimeConfigurationModel : IMapFrom<OvertimeConfiguration>
    {
        public int OvertimeConfigurationId { get; set; }
        public int ShiftConfigurationId { get; set; }
        public int OvertimeLevel { get; set; }
        public bool IsOvertimeLevel { get; set; }
        public int WeekendLevel { get; set; }
        public bool IsWeekendLevel { get; set; }
        public int HolidayLevel { get; set; }
        public bool IsHolidayLevel { get; set; }
        public long MinsAfterWork { get; set; }
        public bool IsMinsAfterWork { get; set; }
        public long MinsBeforeWork { get; set; }
        public bool IsMinsBeforeWork { get; set; }
        public int TotalHourWillCountShift { get; set; }
        public bool IsTotalHourWillCountShift { get; set; }
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

    }
}