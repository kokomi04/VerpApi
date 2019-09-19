using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        private readonly MasterDBContext _masterContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;

        public UserService(MasterDBContext masterContext
            , IOptions<AppSetting> appSetting
            , ILogger<UserService> logger
            )
        {
            _masterContext = masterContext;
            _appSetting = appSetting.Value;
            _logger = logger;
        }

        public async Task<ServiceResult<int>> CreateUser(UserInfoInput req)
        {
            var validate = ValidateUserInfoInput(req);
            if (!validate.IsSuccess())
            {
                return validate;
            }

            using (var trans = await _masterContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var user = await CreateUserAuthen(req);
                    if (!user.Code.IsSuccess())
                    {
                        trans.Rollback();
                        return user.Code;
                    }
                    var r = await CreateEmployee(user.Data, req);

                    if (!r.IsSuccess())
                    {
                        trans.Rollback();
                        return r;
                    }
                    trans.Commit();

                    _logger.LogInformation("CreateUser({0}) successful!", user.Data);
                    return user.Data;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    _logger.LogError(ex, "CreateUser");
                    return GeneralCode.InternalError;
                }
                
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
                try
                {
                    var user = await DeleteUserAuthen(userId);
                    if (!user.IsSuccess())
                    {
                        trans.Rollback();
                        return user;
                    }
                    var r = await DeleteEmployee(userId);

                    if (!r.IsSuccess())
                    {
                        trans.Rollback();
                        return r;
                    }
                    trans.Commit();

                    return GeneralCode.Success;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    _logger.LogError(ex, "DeleteUser");
                    return GeneralCode.InternalError;
                }
               
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
            var validate = ValidateUserInfoInput(req);
            if (!validate.IsSuccess())
            {
                return validate;
            }

            using (var trans = await _masterContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var r1 = await UpdateUserAuthen(userId, req);
                    if (!r1.IsSuccess())
                    {
                        trans.Rollback();
                        return r1;
                    }
                    var r2 = await UpdateEmployee(userId, req);

                    if (!r2.IsSuccess())
                    {
                        trans.Rollback();
                        return r2;
                    }
                    trans.Commit();

                    return GeneralCode.Success;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    _logger.LogError(ex, "UpdateUser");
                    return GeneralCode.InternalError;
                }
                
            }
        }


        #region private
        private Enum ValidateUserInfoInput(UserInfoInput req)
        {
            //if (!Enum.IsDefined(req.UserStatusId.GetType(), req.UserStatusId))
            //{
            //    return GeneralCode.InvalidParams;
            //}
            return GeneralCode.Success;
        }
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
                RoleId = req.RoleId,
                UpdatedDatetimeUtc = DateTime.UtcNow

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
            user.UpdatedDatetimeUtc = DateTime.UtcNow;

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
            user.UpdatedDatetimeUtc = DateTime.UtcNow;
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
