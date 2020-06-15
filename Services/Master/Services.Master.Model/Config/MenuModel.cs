using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace VErp.Services.Master.Model.Config
{
    public class MenuInputModel
    {
        public int ParentId { get; set; }
        public bool IsDisabled { get; set; }
        public int ModuleId { get; set; }
        public string MenuName { get; set; }
        public string Url { get; set; }
        public string Icon { get; set; }
        public string Param { get; set; }
        public int SortOrder { get; set; }
    }

    public class MenuOutputModel : MenuInputModel
    {
        public int MenuId { get; set; }
    }
}
