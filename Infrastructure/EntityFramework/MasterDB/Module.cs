using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.MasterDB
{
    public partial class Module
    {
        public Module()
        {
            RolePermission = new HashSet<RolePermission>();
        }

        public int ModuleId { get; set; }
        public string ModuleName { get; set; }
        public string Description { get; set; }

        public virtual ICollection<RolePermission> RolePermission { get; set; }
    }
}
