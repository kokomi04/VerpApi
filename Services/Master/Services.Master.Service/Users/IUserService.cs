using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.StandardEnum;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.RolePermission;
using VErp.Services.Master.Model.Users;

namespace VErp.Services.Master.Service.Users
{
    public interface IUserService
    {
        Task<ServiceResult<UserInfoOutput>> GetInfo(int userId);
        Task<ServiceResult<int>> CreateUser(UserInfoInput req);
        Task<Enum> UpdateUser(int userId, UserInfoInput req);
        Task<Enum> DeleteUser(int userId);
        Task<PageData<UserInfoOutput>> GetList(string keyword, int page, int size);

        Task<IList<RolePermissionModel>> GetUserPermission(int userId);

    }
}
