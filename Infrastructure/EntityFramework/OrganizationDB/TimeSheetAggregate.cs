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

    public decimal CountedHoliday { get; set; }

    public decimal CountedWeekdayHour { get; set; }

    public decimal CountedWeekendHour { get; set; }

    public decimal CountedHolidayHour { get; set; }

    public long MinsLate { get; set; }

    public int CountedLate { get; set; }

    public long MinsEarly { get; set; }

    public int CountedEarly { get; set; }

    public decimal WorkCountedTotal { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedDatetimeUtc { get; set; }

    public int UpdatedByUserId { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public virtual TimeSheet TimeSheet { get; set; }

    public virtual ICollection<TimeSheetAggregateAbsence> TimeSheetAggregateAbsence { get; set; } = new List<TimeSheetAggregateAbsence>();

    public virtual ICollection<TimeSheetAggregateOvertime> TimeSheetAggregateOvertime { get; set; } = new List<TimeSheetAggregateOvertime>();
}
