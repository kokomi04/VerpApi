using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB;

public partial class TimeSheetAggregateAbsence
{
    public long TimeSheetAggregateId { get; set; }

    public int AbsenceTypeSymbolId { get; set; }

    public int CountedDay { get; set; }

    public virtual TimeSheetAggregate TimeSheetAggregate { get; set; }
}
