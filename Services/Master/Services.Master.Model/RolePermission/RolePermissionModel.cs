using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Services.Master.Model.RolePermission
{
    public class RolePermissionModel
    {
        public int ModuleId { get; set; }
        public int ObjectTypeId { get; set; }
        public long ObjectId { get; set; }
        public int Permission { get; set; }
    }
}
