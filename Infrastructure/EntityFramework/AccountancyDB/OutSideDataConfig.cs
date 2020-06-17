using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.AccountancyDB
{
    public partial class OutSideDataConfig
    {
        public int CategoryId { get; set; }
        public int? ModuleType { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
        public string Key { get; set; }
        public string ParentKey { get; set; }

        public virtual Category Category { get; set; }
    }
}
