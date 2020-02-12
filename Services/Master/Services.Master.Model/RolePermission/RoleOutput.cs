using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Services.Master.Model.RolePermission
{
    public class RoleInput
    {        
        public int? ParentRoleId { get; set; }
        public string RoleName { get; set; }
        public string Description { get; set; }
        public EnumRoleStatus RoleStatusId { get; set; }
    }
    public class RoleOutput : RoleInput
    {
        public int RoleId { get; set; }
        public bool IsEditable { get; set; }
        public string RootPath { get; set; }
    }
}
