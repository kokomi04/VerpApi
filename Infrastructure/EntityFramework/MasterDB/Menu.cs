using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.MasterDB
{
    public partial class Menu
    {
        public int MenuId { get; set; }
        public int ParentId { get; set; }
        public bool IsDisabled { get; set; }
        public int ModuleId { get; set; }
        public string MenuName { get; set; }
        public string Url { get; set; }
        public string Icon { get; set; }
        public bool IsDeleted { get; set; }
        public int UpdatedByUserId { get; set; }
        public DateTime CreatedDatetimeUtc { get; set; }
        public DateTime UpdatedDatetimeUtc { get; set; }
        public int CreatedByUserId { get; set; }
        public DateTime? DeletedDatetimeUtc { get; set; }
        public string Param { get; set; }
        public int SortOrder { get; set; }
    }
}
