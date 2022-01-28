using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.MasterDB
{
    public partial class Module
    {
        public Module()
        {
            ModuleApiEndpointMapping = new HashSet<ModuleApiEndpointMapping>();
            RolePermission = new HashSet<RolePermission>();
        }

        public int ModuleId { get; set; }
        public string ModuleName { get; set; }
        public string Description { get; set; }
        public int ModuleGroupId { get; set; }
        public int SortOrder { get; set; }
        public bool IsDeveloper { get; set; }

        public virtual ModuleGroup ModuleGroup { get; set; }
        public virtual ICollection<ModuleApiEndpointMapping> ModuleApiEndpointMapping { get; set; }
        public virtual ICollection<RolePermission> RolePermission { get; set; }
    }
}
