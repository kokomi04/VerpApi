using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB;

public partial class ArrangeShift
{
    public int ArrangeShiftId { get; set; }

    public int ArrangeShiftMode { get; set; }

    public int WorkScheduleId { get; set; }

    public int OrdinalNumber { get; set; }

    public virtual ICollection<ArrangeShiftItem> ArrangeShiftItem { get; set; } = new List<ArrangeShiftItem>();

    public virtual WorkSchedule WorkSchedule { get; set; }
}
