using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.MasterDB;

public partial class ModuleApiEndpointMapping
{
    public int ModuleApiEndpointMappingId { get; set; }

    public int ModuleId { get; set; }

    public Guid ApiEndpointId { get; set; }

    public virtual ApiEndpoint ApiEndpoint { get; set; }

    public virtual Module Module { get; set; }
}
