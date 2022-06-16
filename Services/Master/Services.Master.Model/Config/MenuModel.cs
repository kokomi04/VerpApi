﻿namespace VErp.Services.Master.Model.Config
{
    public class MenuInputModel
    {
        public int ParentId { get; set; }
        public bool IsDisabled { get; set; }
        public int ModuleId { get; set; }
        public int ObjectTypeId { get; set; }
        public long ObjectId { get; set; }
        public string MenuName { get; set; }
        public string Url { get; set; }
        public string Icon { get; set; }
        public string Param { get; set; }
        public int SortOrder { get; set; }
        public bool IsGroup { get; set; }
        public bool? IsAlwaysShowTopMenu { get; set; }
    }

    public class MenuOutputModel : MenuInputModel
    {
        public int MenuId { get; set; }
    }
}
