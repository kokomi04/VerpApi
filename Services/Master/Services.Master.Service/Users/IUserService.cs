using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.RolePermission;
using VErp.Services.Master.Model.Users;

namespace VErp.Services.Master.Service.Users
{
    public interface IUserService
    {
        Task<UserInfoOutput> GetInfo(int userId);
        Task<int> CreateOwnerUser(int subsidiaryId, UserInfoInput req);
        Task<int> CreateUser(UserInfoInput req, EnumEmployeeType employeeTypeId);
        Task<bool> UpdateUser(int userId, UserInfoInput req);
        Task<bool> ChangeUserPassword(int userId, UserChangepasswordInput req);
        Task<bool> DeleteUser(int userId);
        Task<PageData<UserInfoOutput>> GetList(string keyword, int page, int size, Clause filters = null);
        Task<IList<UserBasicInfoOutput>> GetBasicInfos(IList<int> userIds);

        Task<IList<RolePermissionModel>> GetMePermission();

        /// <summary>
        /// Lấy danh sách user đc quyền truy cập vào moduleId input
        /// </summary>
        /// <param name="currentUserId">UserId</param>
        /// <param name="moduleId">moduleId input</param>
        /// <param name="keyword">Từ khóa tìm kiếm</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize">Bản ghi trên 1 trang</param>
        /// <returns></returns>
        Task<PageData<UserInfoOutput>> GetListByModuleId(int currentUserId, int moduleId,string keyword, int pageIndex, int pageSize);

        Task<IList<UserInfoOutput>> GetListByUserIds(IList<int> userIds);
    }
}
