﻿using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.MasterDB
{
    public partial class PrintConfigStandardModuleType
    {
        public int PrintConfigStandardId { get; set; }
        public int ModuleTypeId { get; set; }

        public virtual PrintConfigStandard PrintConfigStandard { get; set; }
    }
}
