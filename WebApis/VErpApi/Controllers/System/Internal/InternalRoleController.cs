using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.Config;
using VErp.Services.Master.Service.Activity;
using VErp.Services.Master.Service.Config;
using VErp.Services.Master.Service.RolePermission;

namespace VErpApi.Controllers.System.Internal
{
    [Route("api/internal/[controller]")]
    public class InternalRoleController : CrossServiceBaseController
    {
        private readonly IRoleService _roleService;
        public InternalRoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

      
        [Route("GrantDataForAllRoles")]
        [HttpPost]
        public async Task<bool> GrantDataForAllRoles([FromBody] EnumObjectType objectTypeId, [FromBody] long objectId)
        {
            return await _roleService.GrantDataForAllRoles(objectTypeId, objectId);
        }

        [Route("GrantPermissionForAllRoles")]
        [HttpPost]
        public async Task<bool> GrantPermissionForAllRoles([FromBody] EnumModule moduleId, [FromBody] EnumObjectType objectTypeId, [FromBody] long objectId, [FromBody] IList<int> actionIds)
        {
            return await _roleService.GrantPermissionForAllRoles(moduleId, objectTypeId, objectId, actionIds);
        }

        [Route("GrantActionPermissionForAllRoles")]
        [HttpPost]
        public async Task<bool> GrantActionPermissionForAllRoles([FromBody] EnumModule moduleId, [FromBody] EnumObjectType objectTypeId, [FromBody] long objectId, [FromBody] int actionId)
        {
            return await _roleService.GrantActionPermissionForAllRoles(moduleId, objectTypeId, objectId, actionId);
        }
    }
}