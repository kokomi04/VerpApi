using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB;

public partial class ShiftScheduleDetail
{
    public long ShiftScheduleId { get; set; }

    public int ShiftConfigurationId { get; set; }

    public DateTime AssignedDate { get; set; }

    public int EmployeeId { get; set; }

    public bool HasOvertimePlan { get; set; }

    public virtual ShiftSchedule ShiftSchedule { get; set; }
}
