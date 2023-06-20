using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.MasterDB;

public partial class Module
{
    public int ModuleId { get; set; }

    public string ModuleName { get; set; }

    public string Description { get; set; }

    public int ModuleGroupId { get; set; }

    public int SortOrder { get; set; }

    public bool IsDeveloper { get; set; }

    public virtual ICollection<ModuleApiEndpointMapping> ModuleApiEndpointMapping { get; set; } = new List<ModuleApiEndpointMapping>();

    public virtual ModuleGroup ModuleGroup { get; set; }

    public virtual ICollection<RolePermission> RolePermission { get; set; } = new List<RolePermission>();
}
