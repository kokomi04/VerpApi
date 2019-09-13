using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Library;
using VErp.Services.Master.Service.RolePermission.Interface;

namespace VErp.Services.Master.Service.RolePermission.Implement
{
    public class ApiEndpointService : IApiEndpointService
    {      

        public Guid HashApiEndpointId(string route, EnumMethod method, EnumAction action)
        {
            route = (route ?? "").Trim().ToLower();
            return $"{route}{method}{action}".ToGuid();
        }
    }
}
