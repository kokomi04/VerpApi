﻿using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.MasterDB
{
    public partial class ModuleGroup
    {
        public ModuleGroup()
        {
            Module = new HashSet<Module>();
        }

        public int ModuleGroupId { get; set; }
        public string ModuleGroupName { get; set; }

        public virtual ICollection<Module> Module { get; set; }
    }
}
