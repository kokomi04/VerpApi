﻿using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.MasterDB
{
    public partial class PrintConfigCustomModuleType
    {
        public int PrintConfigCustomId { get; set; }
        public int ModuleTypeId { get; set; }

        public virtual PrintConfigCustom PrintConfigCustom { get; set; }
    }
}
