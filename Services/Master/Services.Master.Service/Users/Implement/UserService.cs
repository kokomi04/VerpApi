using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.Users;
using VErp.Services.Master.Service.Users.Interface;

namespace VErp.Services.Master.Service.Users.Implement
{
    public class UserService : IUserService
    {
        private MasterDBContext _masterContext;
        private readonly AppSetting _appSetting;
        public UserService(MasterDBContext masterContext
            , IOptions<AppSetting> appSetting)
        {
            _masterContext = masterContext;
            _appSetting = appSetting.Value;
        }

        public async Task<ServiceResult<int>> CreateUser(UserInfoInput req)
        {
            using (var trans = await _masterContext.Database.BeginTransactionAsync())
            {
                var user = await CreateUserAuthen(req);
                if (!user.Code.IsSuccess())
                {
                    return user.Code;
                }
                var r = await CreateEmployee(user.Data, req);

                if (!r.IsSuccess())
                {
                    return r;
                }
                trans.Commit();

                return user.Data;
            }
        }

        public async Task<ServiceResult<UserInfoOutput>> GetInfo(int userId)
        {
            var user = await (
                 from u in _masterContext.User
                 join em in _masterContext.Employee on u.UserId equals em.UserId
                 where u.UserId == userId
                 select new UserInfoOutput
                 {
                     UserId = u.UserId,
                     UserName = u.UserName,
                     UserStatusId = (EnumUserStatus)u.UserStatusId,
                     RoleId = u.RoleId,
                     EmployeeCode = em.EmployeeCode,
                     FullName = em.FullName,
                     Address = em.Address,
                     Email = em.Email,
                     GenderId = (EnumGender?)em.GenderId,
                     Phone = em.Phone
                 }
             )
             .FirstOrDefaultAsync();

            if (user == null)
            {
                return UserErrorCode.UserNotFound;
            }

            return user;
        }

        public async Task<Enum> DeleteUser(int userId)
        {
            using (var trans = await _masterContext.Database.BeginTransactionAsync())
            {
                var user = await DeleteUserAuthen(userId);
                if (!user.IsSuccess())
                {
                    return user;
                }
                var r = await DeleteEmployee(userId);

                if (!r.IsSuccess())
                {
                    return r;
                }
                trans.Commit();

                return GeneralCode.Success;
            }
        }

        public async Task<PageData<UserInfoOutput>> GetList(string keyword, int page, int size)
        {
            keyword = (keyword ?? "").Trim();

            var query = (
                 from u in _masterContext.User
                 join em in _masterContext.Employee on u.UserId equals em.UserId
                 select new UserInfoOutput
                 {
                     UserId = u.UserId,
                     UserName = u.UserName,
                     UserStatusId = (EnumUserStatus)u.UserStatusId,
                     RoleId = u.RoleId,
                     EmployeeCode = em.EmployeeCode,
                     FullName = em.FullName,
                     Address = em.Address,
                     Email = em.Email,
                     GenderId = (EnumGender?)em.GenderId,
                     Phone = em.Phone
                 }
             );

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = from u in query
                        where u.UserName.Contains(keyword)
                        || u.FullName.Contains(keyword)
                        || u.EmployeeCode.Contains(keyword)
                        || u.Email.Contains(keyword)
                        select u;
            }

            var lst = await query.Skip((page - 1) * size).Take(size).ToListAsync();
            var total = await query.CountAsync();

            return (lst, total);
        }

        public async Task<Enum> UpdateUser(int userId, UserInfoInput req)
        {
            using (var trans = await _masterContext.Database.BeginTransactionAsync())
            {
                var r1 = await UpdateUserAuthen(userId, req);
                if (!r1.IsSuccess())
                {
                    return r1;
                }
                var r2 = await UpdateEmployee(userId, req);

                if (!r2.IsSuccess())
                {
                    return r2;
                }
                trans.Commit();

                return GeneralCode.Success;
            }
        }


        #region private

        private async Task<ServiceResult<int>> CreateUserAuthen(UserInfoInput req)
        {
            var (salt, passwordHash) = Sercurity.GenerateHashPasswordHash(_appSetting.PasswordPepper, req.Password);
            req.UserName = (req.UserName ?? "").Trim().ToLower();

            var userNameHash = req.UserName.ToGuid();
            var user = await _masterContext.User.FirstOrDefaultAsync(u => u.UserNameHash == userNameHash);
            if (user != null)
            {
                return UserErrorCode.UserNameExisted;
            }

            user = new User()
            {
                UserName = req.UserName,
                UserNameHash = req.UserName.ToGuid(),
                IsDeleted = false,
                CreatedDatetimeUtc = DateTime.UtcNow,
                UserStatusId = (int)req.UserStatusId,
                PasswordSalt = salt,
                PasswordHash = passwordHash,
                RoleId = req.RoleId
            };

            await _masterContext.User.AddAsync(user);

            await _masterContext.SaveChangesAsync();

            return user.UserId;
        }

        private async Task<Enum> CreateEmployee(int userId, UserInfoInput req)
        {
            var employee = new Employee()
            {
                EmployeeCode = req.EmployeeCode,
                FullName = req.FullName,
                Email = req.Email,
                Address = req.Address,
                GenderId = (int?)req.GenderId,
                Phone = req.Phone,
                UserId = userId
            };

            await _masterContext.Employee.AddAsync(employee);

            await _masterContext.SaveChangesAsync();

            return GeneralCode.Success;
        }

        private async Task<Enum> UpdateUserAuthen(int userId, UserInfoInput req)
        {
            var user = await _masterContext.User.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                return UserErrorCode.UserNotFound;
            }

            user.UserStatusId = (int)req.UserStatusId;
            user.RoleId = req.RoleId;
            if (!string.IsNullOrWhiteSpace(req.Password))
            {
                var (salt, passwordHash) = Sercurity.GenerateHashPasswordHash(_appSetting.PasswordPepper, req.Password);
                user.PasswordSalt = salt;
                user.PasswordHash = passwordHash;
            }

            await _masterContext.SaveChangesAsync();

            return GeneralCode.Success;
        }

        private async Task<Enum> UpdateEmployee(int userId, UserInfoInput req)
        {

            var employee = await _masterContext.Employee.FirstOrDefaultAsync(u => u.UserId == userId);
            if (employee == null)
            {
                return UserErrorCode.UserNotFound;
            }

            employee.EmployeeCode = req.EmployeeCode;
            employee.FullName = req.FullName;
            employee.Email = req.Email;
            employee.Address = req.Address;
            employee.GenderId = (int?)req.GenderId;
            employee.Phone = req.Phone;

            await _masterContext.SaveChangesAsync();

            return GeneralCode.Success;
        }

        private async Task<Enum> DeleteUserAuthen(int userId)
        {
            var user = await _masterContext.User.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                return UserErrorCode.UserNotFound;
            }

            user.IsDeleted = true;

            await _masterContext.SaveChangesAsync();

            return GeneralCode.Success;
        }

        private async Task<Enum> DeleteEmployee(int userId)
        {

            var employee = await _masterContext.Employee.FirstOrDefaultAsync(u => u.UserId == userId);
            if (employee == null)
            {
                return UserErrorCode.UserNotFound;
            }

            employee.IsDeleted = true;

            await _masterContext.SaveChangesAsync();

            return GeneralCode.Success;
        }
        #endregion
    }
}
