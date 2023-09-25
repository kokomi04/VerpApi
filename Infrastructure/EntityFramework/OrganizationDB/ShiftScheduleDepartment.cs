using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB;

public partial class ShiftScheduleDepartment
{
    public long ShiftScheduleId { get; set; }

    public int DepartmentId { get; set; }

    public virtual ShiftSchedule ShiftSchedule { get; set; }
}
