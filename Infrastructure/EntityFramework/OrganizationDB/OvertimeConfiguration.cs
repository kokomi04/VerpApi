﻿using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB;

public partial class OvertimeConfiguration
{
    public int OvertimeConfigurationId { get; set; }

    public int? WeekdayLevel { get; set; }

    public bool IsWeekdayLevel { get; set; }

    public int? WeekendLevel { get; set; }

    public bool IsWeekendLevel { get; set; }

    public int? HolidayLevel { get; set; }

    public bool IsHolidayLevel { get; set; }

    public int? WeekdayOvertimeLevel { get; set; }

    public bool IsWeekdayOvertimeLevel { get; set; }

    public int? WeekendOvertimeLevel { get; set; }

    public bool IsWeekendOvertimeLevel { get; set; }

    public int? HolidayOvertimeLevel { get; set; }

    public bool IsHolidayOvertimeLevel { get; set; }

    public int? RoundMinutes { get; set; }

    public bool IsRoundBack { get; set; }

    public int OvertimeCalculationMode { get; set; }

    public int? OvertimeThresholdMins { get; set; }

    public bool IsOvertimeThresholdMins { get; set; }

    public int? MinThresholdMinutesBeforeWork { get; set; }

    public int? MinThresholdMinutesAfterWork { get; set; }

    public bool IsMinThresholdMinutesBeforeWork { get; set; }

    public bool IsMinThresholdMinutesAfterWork { get; set; }

    public long MinsLimitOvertimeBeforeWork { get; set; }

    public long MinsLimitOvertimeAfterWork { get; set; }

    public long MinsReachesBeforeWork { get; set; }

    public long MinsReachesAfterWork { get; set; }

    public long MinsBonusWhenMinsReachesBeforeWork { get; set; }

    public long MinsBonusWhenMinsReachesAfterWork { get; set; }

    public int UpdatedByUserId { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedDatetimeUtc { get; set; }

    public virtual ICollection<OvertimeConfigurationMapping> OvertimeConfigurationMapping { get; set; } = new List<OvertimeConfigurationMapping>();

    public virtual ICollection<ShiftConfiguration> ShiftConfiguration { get; set; } = new List<ShiftConfiguration>();
}
