﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Verp.Cache.Caching;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.RolePermission;
using VErp.Services.Master.Service.RolePermission;
using static VErp.Commons.Constants.Caching.AuthorizeCacheKeys;

namespace VErpApi.Controllers.System
{
    [Route("api/roles")]

    public class RolesController : VErpBaseController
    {
        private readonly IRoleService _roleService;
        private readonly ICurrentContextService _currentContextService;
        private readonly ICachingService _cachingService;

        public RolesController(IRoleService roleService, ICurrentContextService currentContextService, ICachingService cachingService)
        {
            _roleService = roleService;
            _currentContextService = currentContextService;
            _cachingService = cachingService;
        }

        /// <summary>
        /// Lấy danh sách nhóm quyền
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public async Task<PageData<RoleOutput>> Get([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _roleService.GetList(keyword, page, size);
        }

        /// <summary>
        /// Thêm mới nhóm quyền
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public async Task<int> AddRole([FromBody] RoleInput role)
        {
            return await _roleService.AddRole(role, EnumRoleType.Normal);
        }

        /// <summary>
        /// Lấy thông tin nhóm quyền
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{roleId}")]
        public async Task<RoleOutput> GetRoleInfo([FromRoute] int roleId)
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
        public async Task<bool> UpdateRole([FromRoute] int roleId, [FromBody] RoleInput role)
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
        public async Task<bool> DeleteRole([FromRoute] int roleId)
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
        public async Task<IList<RolePermissionModel>> GetPermissions([FromRoute] int roleId)
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
        public async Task<bool> UpdatePermissions([FromRoute] int roleId, [FromBody] IList<RolePermissionModel> permissions)
        {
            return await _roleService.UpdateRolePermission(roleId, permissions);
        }


        /// <summary>
        /// Lấy danh sách nhóm quyền có quyền trên kho
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("Stocks")]
        public async Task<IList<StockPemissionOutput>> Stocks()
        {
            return (await _roleService.GetStockPermission()).ToList();
        }

        /// <summary>
        /// Cập nhật quyền của nhóm quyền trên kho
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [Route("Stocks")]
        public async Task<bool> Stocks([FromBody] IList<StockPemissionOutput> req)
        {
            return await _roleService.UpdateStockPermission(req);
        }

        /// <summary>
        /// Lấy danh sách nhóm quyền có quyền trên danh mục
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("Categorys")]
        public async Task<IList<CategoryPermissionModel>> Categorys()
        {
            return (await _roleService.GetCategoryPermissions()).ToList();
        }

        /// <summary>
        /// Cập nhật quyền của nhóm quyền trên danh mục
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [Route("Categorys")]
        public async Task<bool> Categorys([FromBody] IList<CategoryPermissionModel> req)
        {
            return await _roleService.UpdateCategoryPermission(req);
        }


        [HttpGet]
        [Route("AuthCacheRemove")]
        [AllowAnonymous]
        public bool AuthCacheRemove()
        {
            //   if (!_currentContextService.IsDeveloper) throw new BadRequestException("Clean auth caching require developer permission!");
            _roleService.RemoveAuthCache();
            return true;
        }

        [HttpGet]
        [Route("AuthCacheGetApis")]
        [GlobalApi]
        public HashSet<Guid> AuthCacheGetApis([FromQuery] int moduleId)
        {
            return _cachingService.TryGet<HashSet<Guid>>(ModuleApiEndpointMappingsCacheKey(moduleId));
        }

        [HttpGet]
        [Route("ByModuleAndPermission")]
        public Task<IList<RolePermissionModel>> GetRolesPermissionByModuleAndPermission([FromQuery] int moduleId, [FromQuery] int premission)
        {
            return _roleService.GetRolesPermissionByModuleAndPermission(moduleId, premission);
        }

    }
}