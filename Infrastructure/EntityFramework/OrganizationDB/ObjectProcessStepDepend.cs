using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class ObjectProcessStepDepend
    {
        public int ObjectProcessStepId { get; set; }
        public int DependObjectProcessStepId { get; set; }

        public virtual ObjectProcessStep DependObjectProcessStep { get; set; }
        public virtual ObjectProcessStep ObjectProcessStep { get; set; }
    }
}
