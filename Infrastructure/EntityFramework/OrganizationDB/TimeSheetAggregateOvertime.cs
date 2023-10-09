using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB;

public partial class TimeSheetAggregateOvertime
{
    public long TimeSheetAggregateId { get; set; }

    public int OvertimeLevelId { get; set; }

    public decimal CountedMins { get; set; }

    public virtual TimeSheetAggregate TimeSheetAggregate { get; set; }
}
