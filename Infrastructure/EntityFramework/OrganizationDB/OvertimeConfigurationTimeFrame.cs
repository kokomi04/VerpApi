using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB;

public partial class OvertimeConfigurationTimeFrame
{
    public int OvertimeConfigurationId { get; set; }

    public int TimeSheetDateType { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public int? OvertimeLevelId { get; set; }

    public bool IsWorkingHours { get; set; }

    public virtual OvertimeConfiguration OvertimeConfiguration { get; set; }
}
