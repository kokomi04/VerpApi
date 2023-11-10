using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB;

public partial class OvertimePlan
{
    public long EmployeeId { get; set; }

    public DateTime AssignedDate { get; set; }

    public int OvertimeLevelId { get; set; }

    public int OvertimeMins { get; set; }

    public virtual OvertimeLevel OvertimeLevel { get; set; }
}
