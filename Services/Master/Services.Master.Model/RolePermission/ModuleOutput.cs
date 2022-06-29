﻿namespace VErp.Services.Master.Model.RolePermission
{
    public class ModuleOutput
    {
        public int ModuleGroupId { get; set; }
        public int ModuleId { get; set; }
        public string ModuleName { get; set; }
        public string Description { get; set; }
        public bool? IsDeveloper { get; set; }
    }
}
