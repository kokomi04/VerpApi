using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB;

public partial class TimeSheetAggregate
{
    public long TimeSheetAggregateId { get; set; }

    public long TimeSheetId { get; set; }

    public int EmployeeId { get; set; }

    public decimal CountedWeekday { get; set; }

    public decimal CountedWeekend { get; set; }

    public decimal CountedWeekdayHour { get; set; }

    public decimal CountedWeekendHour { get; set; }

    public long MinsLate { get; set; }

    public int CountedLate { get; set; }

    public long MinsEarly { get; set; }

    public int CountedEarly { get; set; }

    public decimal Overtime1 { get; set; }

    public decimal Overtime2 { get; set; }

    public decimal Overtime3 { get; set; }

    public int CountedAbsence { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedDatetimeUtc { get; set; }

    public int UpdatedByUserId { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public virtual TimeSheet TimeSheet { get; set; }
}
