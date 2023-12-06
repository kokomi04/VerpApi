using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB;

public partial class OvertimeConfiguration
{
    public int OvertimeConfigurationId { get; set; }

    public int? RoundMinutes { get; set; }

    public bool IsRoundBack { get; set; }

    public int OvertimeCalculationMode { get; set; }

    public int? OvertimeThresholdMins { get; set; }

    public bool IsOvertimeThresholdMins { get; set; }

    public bool IsCalculationThresholdMins { get; set; }

    public int MinsLimitOvertime { get; set; }

    public int MinsReaches { get; set; }

    public int MinsBonusWhenMinsReaches { get; set; }

    public int? MinThresholdMinutesBeforeWork { get; set; }

    public int? MinThresholdMinutesAfterWork { get; set; }

    public bool IsMinThresholdMinutesBeforeWork { get; set; }

    public bool IsMinThresholdMinutesAfterWork { get; set; }

    public bool IsCalculationThresholdMinsBeforeWork { get; set; }

    public bool IsCalculationThresholdMinsAfterWork { get; set; }

    public int MinsLimitOvertimeBeforeWork { get; set; }

    public int MinsLimitOvertimeAfterWork { get; set; }

    public int MinsReachesBeforeWork { get; set; }

    public int MinsReachesAfterWork { get; set; }

    public int MinsBonusWhenMinsReachesBeforeWork { get; set; }

    public int MinsBonusWhenMinsReachesAfterWork { get; set; }

    public int UpdatedByUserId { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedDatetimeUtc { get; set; }

    public virtual ICollection<OvertimeConfigurationMapping> OvertimeConfigurationMapping { get; set; } = new List<OvertimeConfigurationMapping>();

    public virtual ICollection<OvertimeConfigurationTimeFrame> OvertimeConfigurationTimeFrame { get; set; } = new List<OvertimeConfigurationTimeFrame>();

    public virtual ICollection<ShiftConfiguration> ShiftConfiguration { get; set; } = new List<ShiftConfiguration>();
}
