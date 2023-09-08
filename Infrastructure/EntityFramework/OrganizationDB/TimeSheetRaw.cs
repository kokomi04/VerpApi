using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB;

public partial class TimeSheetRaw
{
    public long TimeSheetRawId { get; set; }

    public long EmployeeId { get; set; }

    public DateTime Date { get; set; }

    public TimeSpan Time { get; set; }

    public int TimeKeepingMethod { get; set; }

    public string TimeKeepingRecorder { get; set; }

    public DateTime CreatedDatetimeUtc { get; set; }

    public int CreatedByUserId { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public int UpdatedByUserId { get; set; }

    public DateTime? DeletedDatetimeUtc { get; set; }
}
