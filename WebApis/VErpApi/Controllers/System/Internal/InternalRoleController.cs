using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Master.Model.RolePermission;
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
            return await _roleService.GrantPermissionForAllRoles(data.ModuleId, data.ObjectTypeId, data.ObjectId);
        }

        [HttpGet]
        [Route("ByModuleAndPermission")]
        public Task<IList<RolePermissionModel>> GetRolesPermissionByModuleAndPermission(int moduleId, int premission)
        {
            return _roleService.GetRolesPermissionByModuleAndPermission(moduleId, premission);
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

    }

    public class GrantActionPermissionRequestModel : GrantPermissionRequestBaseModel
    {
        public int ActionId { get; set; }
    }
}