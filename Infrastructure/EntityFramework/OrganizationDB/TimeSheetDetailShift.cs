using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB;

public partial class TimeSheetDetailShift
{
    public long TimeSheetDetailId { get; set; }

    public int ShiftConfigurationId { get; set; }

    public int CountedSymbolId { get; set; }

    public TimeSpan? TimeIn { get; set; }

    public TimeSpan? TimeOut { get; set; }

    public int? AbsenceTypeSymbolId { get; set; }

    public long? MinsLate { get; set; }

    public long? MinsEarly { get; set; }

    public int? OvertimeLevelId { get; set; }

    public long? MinsOvertime { get; set; }

    public int? OvertimeLevelId2 { get; set; }

    public long? MinsOvertime2 { get; set; }

    public virtual TimeSheetDetail TimeSheetDetail { get; set; }
}
