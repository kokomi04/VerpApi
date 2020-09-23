using System;
using System.Collections.Generic;
using System.Text;

namespace VErp.Commons.GlobalObject
{
    public class ModuleConfig
    {
        public int SubsidiaryId { get; set; }
        public long ClosingDate { get; set; }
        public bool AutoClosingDate { get; set; }
        public FreqClosingDate FreqClosingDate { get; set; }
    }


}
