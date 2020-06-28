using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Library;
using VErp.Services.Master.Service.RolePermission;

namespace VErp.Services.Master.Service.RolePermission.Implement
{
    public class ApiEndpointService : IApiEndpointService
    {

        public Guid HashApiEndpointId(int serviceId, string route, EnumMethod method)
        {
            return Utils.HashApiEndpointId(serviceId, route, method);
        }
    }
}
