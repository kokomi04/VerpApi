using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.MasterDB
{
    public partial class ApiEndpoint
    {
        public Guid ApiEndpointId { get; set; }
        public string Route { get; set; }
        public int MethodId { get; set; }
        public int ActionId { get; set; }
    }
}
