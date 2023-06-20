using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB;

public partial class TimeSheetOvertime
{
    public long TimeSheetOvertimeId { get; set; }

    public long TimeSheetId { get; set; }

    public int EmployeeId { get; set; }

    public int OvertimeLevelId { get; set; }

    public decimal MinsOvertime { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedDatetimeUtc { get; set; }

    public int UpdatedByUserId { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public virtual OvertimeLevel OvertimeLevel { get; set; }

    public virtual TimeSheet TimeSheet { get; set; }
}
