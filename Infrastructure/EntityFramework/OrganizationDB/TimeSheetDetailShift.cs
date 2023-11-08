using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB;

public partial class TimeSheetDetailShift
{
    public long TimeSheetDetailId { get; set; }

    public int ShiftConfigurationId { get; set; }

    public TimeSpan? TimeIn { get; set; }

    public TimeSpan? TimeOut { get; set; }

    public int? AbsenceTypeSymbolId { get; set; }

    public long? MinsLate { get; set; }

    public long? MinsEarly { get; set; }

    public long ActualWorkMins { get; set; }

    public decimal WorkCounted { get; set; }

    public int? DateAsOvertimeLevelId { get; set; }

    public bool HasOvertimePlan { get; set; }

    public virtual TimeSheetDetail TimeSheetDetail { get; set; }

    public virtual ICollection<TimeSheetDetailShiftCounted> TimeSheetDetailShiftCounted { get; set; } = new List<TimeSheetDetailShiftCounted>();

    public virtual ICollection<TimeSheetDetailShiftOvertime> TimeSheetDetailShiftOvertime { get; set; } = new List<TimeSheetDetailShiftOvertime>();
}
