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
        Task<int> AddRole(RoleInput role);
        Task<RoleOutput> GetRoleInfo(int roleId);
        Task<bool> UpdateRole(int roleId, RoleInput role);
        Task<bool> DeleteRole(int roleId);
        Task<IList<RolePermissionModel>> GetRolePermission(int roleId);
        Task<IList<RolePermissionModel>> GetRolesPermission(IList<int> roleIds);
        Task<bool> UpdateRolePermission(int roleId, IList<RolePermissionModel> permissions);

        Task<IList<StockPemissionOutput>> GetStockPermission();
        Task<bool> UpdateStockPermission(IList<StockPemissionOutput> req);
    }
}
