﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.RolePermission;

namespace VErp.Services.Master.Service.RolePermission
{
    public interface IRoleService
    {
        Task<PageData<RoleOutput>> GetList(string keyword, int page, int size);
        Task<int> AddRole(RoleInput role, EnumRoleType enumRoleId);
        Task<RoleOutput> GetRoleInfo(int roleId);
        Task<RoleOutput> GetAdminRoleInfo();
        Task<bool> UpdateRole(int roleId, RoleInput role);
        Task<bool> DeleteRole(int roleId);
        Task<IList<RolePermissionModel>> GetRolePermission(int roleId);
        Task<IList<RolePermissionModel>> GetRolesPermission(IList<int> roleIds, bool? isDeveloper = null);
        Task<bool> UpdateRolePermission(int roleId, IList<RolePermissionModel> permissions);

        Task<IList<StockPemissionOutput>> GetStockPermission();
        Task<bool> UpdateStockPermission(IList<StockPemissionOutput> req);

        Task<IList<CategoryPermissionModel>> GetCategoryPermissions();
        Task<bool> UpdateCategoryPermission(IList<CategoryPermissionModel> req);

        Task<bool> GrantDataForAllRoles(EnumObjectType objectTypeId, long objectId);

        Task<bool> GrantPermissionForAllRoles(EnumModule moduleId, EnumObjectType objectTypeId, long objectId);

        void RemoveAuthCache();

        Task<IList<RolePermissionModel>> GetRolesPermissionByModuleAndPermission(int moduleId, int premission);


    }
}
