using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB;

public partial class TimeSheetDepartment
{
    public long TimeSheetId { get; set; }

    public int DepartmentId { get; set; }

    public virtual TimeSheet TimeSheet { get; set; }
}
