using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
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
        public async Task<bool> GrantDataForAllRoles([FromBody] GrantDataRequestModel data)
        {
            if (data == null) throw new BadRequestException(GeneralCode.InvalidParams);
            return await _roleService.GrantDataForAllRoles(data.ObjectTypeId, data.ObjectId);
        }

        [Route("GrantPermissionForAllRoles")]
        [HttpPost]
        public async Task<bool> GrantPermissionForAllRoles([FromBody] GrantPermissionRequestModel data)
        {
            if (data == null) throw new BadRequestException(GeneralCode.InvalidParams);
            return await _roleService.GrantPermissionForAllRoles(data.ModuleId, data.ObjectTypeId, data.ObjectId, data.ActionIds);
        }

        [Route("GrantActionPermissionForAllRoles")]
        [HttpPost]
        public async Task<bool> GrantActionPermissionForAllRoles([FromBody] GrantActionPermissionRequestModel data)
        {
            if (data == null) throw new BadRequestException(GeneralCode.InvalidParams);
            return await _roleService.GrantActionPermissionForAllRoles(data.ModuleId, data.ObjectTypeId, data.ObjectId, data.ActionId);
        }
    }

    public class GrantDataRequestModel
    {
        public EnumObjectType ObjectTypeId { get; set; }
        public long ObjectId { get; set; }
    }

    public abstract class GrantPermissionRequestBaseModel
    {
        public EnumModule ModuleId { get; set; }
        public EnumObjectType ObjectTypeId { get; set; }
        public long ObjectId { get; set; }
    }

    public class GrantPermissionRequestModel : GrantPermissionRequestBaseModel
    {
#pragma warning disable CA2227 // Collection properties should be read only
        public IList<int> ActionIds { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
    }

    public class GrantActionPermissionRequestModel : GrantPermissionRequestBaseModel
    {
        public int ActionId { get; set; }
    }
}