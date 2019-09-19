using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.RolePermission;
using VErp.Services.Master.Service.RolePermission.Interface;

namespace VErpApi.Controllers.System
{
    [Route("api/roles")]

    public class RolesController : VErpBaseController
    {
        private readonly IRoleService _roleService;
        public RolesController(IRoleService roleService
            )
        {
            _roleService = roleService;
        }

        /// <summary>
        /// Lấy danh sách nhóm quyền
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public async Task<ApiResponse<PageData<RoleOutput>>> Get([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _roleService.GetList(keyword, page, size);
        }

        /// <summary>
        /// Thêm mới nhóm quyền
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public async Task<ApiResponse<int>> AddRole([FromBody] RoleInput role)
        {
            return await _roleService.AddRole(role);
        }

        /// <summary>
        /// Lấy thông tin nhóm quyền
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{roleId}")]
        public async Task<ApiResponse<RoleOutput>> GetRoleInfo([FromRoute] int roleId)
        {
            return await _roleService.GetRoleInfo(roleId);
        }

        /// <summary>
        /// Cập nhật thông tin nhóm quyền
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{roleId}")]
        public async Task<ApiResponse> UpdateRole([FromRoute] int roleId, [FromBody] RoleInput role)
        {
            return await _roleService.UpdateRole(roleId, role);
        }

        /// <summary>
        /// Xóa nhóm quyền
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{roleId}")]
        public async Task<ApiResponse> DeleteRole([FromRoute] int roleId)
        {
            return await _roleService.DeleteRole(roleId);
        }

        /// <summary>
        /// Lấy danh sách module và quyền truy cập tương ứng
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{roleId}/Permissions")]
        public async Task<ApiResponse<IList<RolePermissionModel>>> GetPermissions([FromRoute] int roleId)
        {
            return (await _roleService.GetRolePermission(roleId)).ToList();
        }

        /// <summary>
        /// Phân quyền cho nhóm quyền
        /// </summary>
        /// <param name="roleId"></param>
        /// <param name="permissions"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{roleId}/Permissions")]
        public async Task<ApiResponse> UpdatePermissions([FromRoute] int roleId, [FromBody] IList<RolePermissionModel> permissions)
        {
            return await _roleService.UpdateRolePermission(roleId, permissions);
        }
    }
}