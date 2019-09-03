using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.MasterDB
{
    public partial class RolePermission
    {
        public int RoleId { get; set; }
        public int ModuleId { get; set; }
        public int Permission { get; set; }
    }
}
