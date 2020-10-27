using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Master.Model.RolePermission
{
    public class RolePermissionModel
    {
        public int ModuleGroupId { get; set; }
        public int ModuleId { get; set; }
        public int ObjectTypeId { get; set; }
        public long ObjectId { get; set; }
        public int Permission { get; set; }
        public IList<int> ActionIds { get; set; }
    }
}
