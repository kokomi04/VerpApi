﻿using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.MasterDB;

public partial class ModuleGroup
{
    public int ModuleGroupId { get; set; }

    public string ModuleGroupName { get; set; }

    public int SortOrder { get; set; }

    public int? ModuleTypeId { get; set; }

    public virtual ICollection<Module> Module { get; set; } = new List<Module>();
}
