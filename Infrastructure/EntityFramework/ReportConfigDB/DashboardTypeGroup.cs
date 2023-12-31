﻿using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.ReportConfigDB;

public partial class DashboardTypeGroup
{
    public int DashboardTypeGroupId { get; set; }

    public string DashboardTypeGroupName { get; set; }

    public int ModuleTypeId { get; set; }

    public int SortOrder { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime CreatedDatetimeUtc { get; set; }

    public int UpdatedByUserId { get; set; }

    public DateTime UpdatedDatetimeUtc { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedDatetimeUtc { get; set; }

    public virtual ICollection<DashboardType> DashboardType { get; set; } = new List<DashboardType>();
}
