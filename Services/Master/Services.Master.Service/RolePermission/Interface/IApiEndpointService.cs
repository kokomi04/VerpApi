using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Services.Master.Service.RolePermission.Interface
{
    public interface IApiEndpointService
    {
        Guid HashApiEndpointId(string route, EnumMethod method);
    }
}
