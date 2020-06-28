﻿using System;
using System.Collections.Generic;
using System.Text;
using VErp.Commons.Enums.MasterEnum;

namespace VErp.Services.Master.Service.RolePermission
{
    public interface IApiEndpointService
    {
        Guid HashApiEndpointId(int serviceId, string route, EnumMethod method);
    }
}
