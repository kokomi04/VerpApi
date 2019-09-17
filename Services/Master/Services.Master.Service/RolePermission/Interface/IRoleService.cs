using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.RolePermission;

namespace VErp.Services.Master.Service.RolePermission.Interface
{
    public interface IRoleService
    {
        Task<IList<RoleOutput>> GetList();
        Task<ServiceResult<int>> AddRole(RoleInput role);
        Task<Enum> UpdateRole(int roleId, RoleInput role);
        Task<Enum> DeleteRole(int roleId);
        Task<IList<RolePermissionModel>> GetRolePermission(int roleId);
        Task<Enum> UpdateRolePermission(int roleId, IList<RolePermissionModel> permissions);
    }
}
