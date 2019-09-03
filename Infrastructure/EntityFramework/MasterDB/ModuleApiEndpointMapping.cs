using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.MasterDB
{
    public partial class ModuleApiEndpointMapping
    {
        public int ModuleId { get; set; }
        public Guid ApiEndpointId { get; set; }
    }
}
