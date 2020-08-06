using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Commons.GlobalObject.InternalDataInterface
{
    public class MenuStyleModel
    {
        public int? ParentId { get; set; }
        public int ModuleId { get; set; }
        public string MenuName { get; set; }
        public string UrlFormat { get; set; }
        public string ParamFormat { get; set; }
        public string Icon { get; set; }
        public int SortOrder { get; set; }
        public bool IsDisabled { get; set; }
    }
}
