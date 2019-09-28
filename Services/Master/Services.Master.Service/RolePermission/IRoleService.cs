using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.RolePermission;

namespace VErp.Services.Master.Service.RolePermission
{
    public interface IRoleService
    {
        Task<PageData<RoleOutput>> GetList(string keyword, int page, int size);
        Task<ServiceResult<int>> AddRole(RoleInput role);
        Task<ServiceResult<RoleOutput>> GetRoleInfo(int roleId);
        Task<Enum> UpdateRole(int roleId, RoleInput role);
        Task<Enum> DeleteRole(int roleId);
        Task<IList<RolePermissionModel>> GetRolePermission(int roleId);
        Task<Enum> UpdateRolePermission(int roleId, IList<RolePermissionModel> permissions);
    }
}
