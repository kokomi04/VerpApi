using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.MasterDB
{
    public partial class Module
    {
        public int ModuleId { get; set; }
        public string ModuleName { get; set; }
        public string Description { get; set; }
    }
}
