using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.MasterDB
{
    public partial class OutSideDataConfig
    {
        public OutSideDataConfig()
        {
            OutsideDataFieldConfig = new HashSet<OutsideDataFieldConfig>();
        }

        public int CategoryId { get; set; }
        public int? ModuleType { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
        public string Key { get; set; }
        public string ParentKey { get; set; }
        public string Joins { get; set; }
        public string RawSql { get; set; }

        public virtual Category Category { get; set; }
        public virtual ICollection<OutsideDataFieldConfig> OutsideDataFieldConfig { get; set; }
    }
}
