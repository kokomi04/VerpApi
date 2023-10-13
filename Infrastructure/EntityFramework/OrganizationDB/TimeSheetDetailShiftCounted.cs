using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB;

public partial class TimeSheetDetailShiftCounted
{
    public long TimeSheetDetailShiftCountedId { get; set; }

    public long TimeSheetDetailId { get; set; }

    public int ShiftConfigurationId { get; set; }

    public int CountedSymbolId { get; set; }

    public virtual TimeSheetDetailShift TimeSheetDetailShift { get; set; }
}
