using System;
using System.Collections.Generic;

#nullable disable

namespace VErp.Infrastructure.EF.OrganizationDB
{
    public partial class ObjectProcessStepUser
    {
        public int ObjectProcessStepId { get; set; }
        public int UserId { get; set; }

        public virtual ObjectProcessStep ObjectProcessStep { get; set; }
    }
}
