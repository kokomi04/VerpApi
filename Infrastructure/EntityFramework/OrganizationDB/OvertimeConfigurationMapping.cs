using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB;

public partial class OvertimeConfigurationMapping
{
    public int OvertimeConfigurationId { get; set; }

    public int OvertimeLevelId { get; set; }

    public int MinsLimit { get; set; }

    public virtual OvertimeConfiguration OvertimeConfiguration { get; set; }

    public virtual OvertimeLevel OvertimeLevel { get; set; }
}
