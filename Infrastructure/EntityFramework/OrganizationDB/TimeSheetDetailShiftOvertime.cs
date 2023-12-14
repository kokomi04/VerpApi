using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB;

public partial class TimeSheetDetailShiftOvertime
{
    public long TimeSheetDetailId { get; set; }

    public int ShiftConfigurationId { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public int OvertimeLevelId { get; set; }

    public long MinsOvertime { get; set; }

    public int OvertimeType { get; set; }

    public virtual TimeSheetDetailShift TimeSheetDetailShift { get; set; }
}
