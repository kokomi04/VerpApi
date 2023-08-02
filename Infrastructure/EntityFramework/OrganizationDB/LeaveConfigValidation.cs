using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB;

public partial class LeaveConfigValidation
{
    public int LeaveConfigId { get; set; }

    public int TotalDays { get; set; }

    public int? MinDaysFromCreateToStart { get; set; }

    public bool IsWarning { get; set; }

    public virtual LeaveConfig LeaveConfig { get; set; }
}
