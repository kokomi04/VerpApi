using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB;

public partial class TimeSheetDayOff
{
    public long TimeSheetDayOffId { get; set; }

    public long TimeSheetId { get; set; }

    public int EmployeeId { get; set; }

    public int AbsenceTypeSymbolId { get; set; }

    public int CountedDayOff { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedDatetimeUtc { get; set; }

    public int UpdatedByUserId { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public virtual TimeSheet TimeSheet { get; set; }
}
