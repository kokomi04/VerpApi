﻿using VErp.Commons.Enums.MasterEnum;

namespace VErp.Services.Master.Model.RolePermission
{
    public class RoleInput
    {
        public int? ParentRoleId { get; set; }
        public string RoleName { get; set; }
        public string Description { get; set; }
        public EnumRoleStatus RoleStatusId { get; set; }
        public bool IsModulePermissionInherit { get; set; }
        public bool IsDataPermissionInheritOnStock { get; set; }
    }
    public class RoleOutput : RoleInput
    {
        public int RoleId { get; set; }
        public bool IsEditable { get; set; }
        public string RootPath { get; set; }
    }
}
