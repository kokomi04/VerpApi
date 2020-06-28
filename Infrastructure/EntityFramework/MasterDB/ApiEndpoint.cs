using System;
using System.Collections.Generic;

namespace VErp.Infrastructure.EF.MasterDB
{
    public partial class ApiEndpoint
    {
        public ApiEndpoint()
        {
            ModuleApiEndpointMapping = new HashSet<ModuleApiEndpointMapping>();
        }

        public Guid ApiEndpointId { get; set; }
        public string Route { get; set; }
        public int ServiceId { get; set; }
        public int MethodId { get; set; }
        public int ActionId { get; set; }

        public virtual Action Action { get; set; }
        public virtual Method Method { get; set; }
        public virtual ICollection<ModuleApiEndpointMapping> ModuleApiEndpointMapping { get; set; }
    }
}
