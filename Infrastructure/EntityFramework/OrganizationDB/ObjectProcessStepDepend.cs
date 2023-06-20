﻿using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class ObjectProcessStepDepend
    {
        public long ObjectProcessStepDependId { get; set; }
        public int ObjectProcessStepId { get; set; }
        public int DependObjectProcessStepId { get; set; }

        public virtual ObjectProcessStep DependObjectProcessStep { get; set; }
        public virtual ObjectProcessStep ObjectProcessStep { get; set; }
    }
}
