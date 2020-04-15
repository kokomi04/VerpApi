﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Model.RolePermission;
using VErp.Services.Master.Model.Users;
using VErp.Services.Master.Service.Activity;
using VErp.Services.Master.Service.RolePermission;
using VErp.Services.Organization.Model.Department;

namespace VErp.Services.Master.Service.Users.Implement
{
    public class UserService : IUserService
    {
        private readonly MasterDBContext _masterContext;
        private readonly OrganizationDBContext _organizationContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IRoleService _roleService;
        private readonly IActivityLogService _activityLogService;
        private readonly ICurrentContextService _currentContextService;
        private readonly IAsyncRunnerService _asyncRunnerService;

        public UserService(MasterDBContext masterContext
            , OrganizationDBContext organizationContext
            , IOptions<AppSetting> appSetting
            , ILogger<UserService> logger
            , IRoleService roleService
            , IActivityLogService activityLogService
            , ICurrentContextService currentContextService
            , IAsyncRunnerService asyncRunnerService
            )
        {
            _organizationContext = organizationContext;
            _masterContext = masterContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _roleService = roleService;
            _activityLogService = activityLogService;
            _currentContextService = currentContextService;
            _asyncRunnerService = asyncRunnerService;
        }

        public async Task<ServiceResult<int>> CreateUser(UserInfoInput req, int updatedUserId)
        {
            var validate = await ValidateUserInfoInput(-1, req);
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
                    // Gắn phòng ban cho nhân sự
                    var r2 = await EmployeeDepartmentMapping(user.Data, req.DepartmentId, updatedUserId);
                    if (!r2.IsSuccess())
                    {
                        trans.Rollback();
                        return r2;
                    }

                    trans.Commit();

                    var info = await GetUserFullInfo(user.Data);

                    await _activityLogService.CreateLog(EnumObjectType.UserAndEmployee, user.Data, $"Thêm mới nhân viên {info?.Employee?.EmployeeCode}", req.JsonSerialize());

                    _logger.LogInformation("CreateUser({0}) successful!", user.Data);

                    if (req.AvatarFileId.HasValue)
                    {
                        _asyncRunnerService.RunAsync<IPhysicalFileService>(f => f.FileAssignToObject(req.AvatarFileId.Value, EnumObjectType.UserAndEmployee, user.Data));
                    }

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

        private async Task<Enum> EmployeeDepartmentMapping(int userId, int departmentId, int updatedUserId)
        {
            var department = await _organizationContext.Department.FirstOrDefaultAsync(d => d.DepartmentId == departmentId);
            if (department == null)
            {
                return DepartmentErrorCode.DepartmentNotFound;
            }
            if (!department.IsActived)
            {
                return DepartmentErrorCode.DepartmentInActived;
            }
            var EmployeeDepartmentMapping = _organizationContext.EmployeeDepartmentMapping
                .Where(d => d.DepartmentId == departmentId && d.UserId == userId);
            var current = EmployeeDepartmentMapping
           .Where(d => d.ExpirationDate >= DateTime.UtcNow.Date && d.EffectiveDate <= DateTime.UtcNow.Date)
           .FirstOrDefault();

            if (current == null)
            {
                // kiểm tra xem có bản ghi trong tương lai
                var future = EmployeeDepartmentMapping.Where(d => d.EffectiveDate > DateTime.UtcNow.Date).OrderBy(d => d.EffectiveDate).FirstOrDefault();
                DateTime expirationDate = future?.EffectiveDate.AddDays(-1) ?? DateTime.MaxValue.Date;

                _organizationContext.EmployeeDepartmentMapping.Add(new EmployeeDepartmentMapping()
                {
                    DepartmentId = departmentId,
                    UserId = userId,
                    EffectiveDate = DateTime.UtcNow.Date,
                    ExpirationDate = expirationDate,
                    UpdatedUserId = updatedUserId,
                    CreatedTime = DateTime.UtcNow,
                    UpdatedTime = DateTime.UtcNow
                });
            }
            else
            {
                current.ExpirationDate = DateTime.UtcNow.AddDays(-1).Date;
                current.UpdatedTime = DateTime.UtcNow;
                current.UpdatedUserId = updatedUserId;

                _organizationContext.EmployeeDepartmentMapping.Add(new EmployeeDepartmentMapping()
                {
                    DepartmentId = departmentId,
                    UserId = userId,
                    EffectiveDate = DateTime.UtcNow.Date,
                    ExpirationDate = current.ExpirationDate,
                    UpdatedUserId = updatedUserId,
                    CreatedTime = DateTime.UtcNow,
                    UpdatedTime = DateTime.UtcNow
                });
            }
            await _masterContext.SaveChangesAsync();
            return GeneralCode.Success;
        }

        public async Task<ServiceResult<UserInfoOutput>> GetInfo(int userId)
        {
            var ur = await _masterContext.User.FirstOrDefaultAsync(u => u.UserId == userId);
            var em = await _organizationContext.Employee.FirstOrDefaultAsync(e => e.UserId == userId);
            if (ur == null || em == null)
            {
                return UserErrorCode.UserNotFound;
            }
            var user = new UserInfoOutput
            {
                UserId = ur.UserId,
                UserName = ur.UserName,
                UserStatusId = (EnumUserStatus)ur.UserStatusId,
                RoleId = ur.RoleId,
                EmployeeCode = em.EmployeeCode,
                FullName = em.FullName,
                Address = em.Address,
                Email = em.Email,
                GenderId = (EnumGender?)em.GenderId,
                Phone = em.Phone,
                AvatarFileId = em.AvatarFileId
            };

            // Thêm thông tin phòng ban cho nhân viên
            DateTime currentDate = DateTime.UtcNow.Date;
            var department = _organizationContext.EmployeeDepartmentMapping.Where(m => m.UserId == user.UserId && m.ExpirationDate >= currentDate && m.EffectiveDate <= currentDate)
                .Join(_organizationContext.Department, m => m.DepartmentId, d => d.DepartmentId, (m, d) => d)
                .Select(d => new DepartmentModel()
                {
                    DepartmentId = d.DepartmentId,
                    DepartmentCode = d.DepartmentCode,
                    DepartmentName = d.DepartmentName,
                    Description = d.Description,
                    IsActived = d.IsActived,
                    ParentId = d.ParentId,
                    ParentName = d.Parent != null ? d.Parent.DepartmentName : null
                })
                .FirstOrDefault();
            user.Department = department;
            return user;
        }

        public async Task<Enum> DeleteUser(int userId)
        {
            var userInfo = await GetUserFullInfo(userId);
            long? oldAvatarFileId = userInfo.Employee.AvatarFileId;

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

                    await _activityLogService.CreateLog(EnumObjectType.UserAndEmployee, userId, $"Xóa nhân viên {userInfo?.Employee?.EmployeeCode}", userInfo.JsonSerialize());


                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    _logger.LogError(ex, "DeleteUser");
                    return GeneralCode.InternalError;
                }

            }

            if (oldAvatarFileId.HasValue)
            {
                _asyncRunnerService.RunAsync<IPhysicalFileService>(f => f.DeleteFile(oldAvatarFileId.Value));
            }

            return GeneralCode.Success;
        }

        public async Task<PageData<UserInfoOutput>> GetList(string keyword, int page, int size)
        {
            keyword = (keyword ?? "").Trim();
            IQueryable<Employee> employees = _organizationContext.Employee;
            IQueryable<User> users = _masterContext.User;

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var userIds = employees.Where(em => em.FullName.Contains(keyword)
                    || em.EmployeeCode.Contains(keyword)
                    || em.Email.Contains(keyword)).Select(em => em.UserId).AsEnumerable();

                users = users.Where(u => u.UserName.Contains(keyword) || userIds.Contains(u.UserId));
            }

            var query = users.AsEnumerable().Join(employees.AsEnumerable(), u => u.UserId, em => em.UserId, (u, em) => new UserInfoOutput
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
            });

            var total = query.Count();

            var lst = query.OrderBy(u => u.UserStatusId).ThenBy(u => u.FullName).Skip((page - 1) * size).Take(size).ToList();



            return (lst, total);
        }

        public async Task<Enum> UpdateUser(int userId, UserInfoInput req, int updatedUserId)
        {
            var validate = await ValidateUserInfoInput(userId, req);
            if (!validate.IsSuccess())
            {
                return validate;
            }

            var userInfo = await GetUserFullInfo(userId);

            long? oldAvatarFileId = userInfo.Employee.AvatarFileId;
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

                    // Lấy thông tin bộ phận hiện tại
                    DateTime currentDate = DateTime.UtcNow.Date;
                    var departmentId = _organizationContext.EmployeeDepartmentMapping
                        .Where(m => m.UserId == userId && m.ExpirationDate >= currentDate && m.EffectiveDate <= currentDate)
                        .Select(d => d.DepartmentId).FirstOrDefault();
                    // Nếu khác update lại thông tin
                    if (departmentId != req.DepartmentId)
                    {
                        var r3 = await EmployeeDepartmentMapping(userId, req.DepartmentId, updatedUserId);
                        if (!r3.IsSuccess())
                        {
                            trans.Rollback();
                            return r3;
                        }
                    }

                    trans.Commit();

                    var newUserInfo = await GetUserFullInfo(userId);

                    await _activityLogService.CreateLog(EnumObjectType.UserAndEmployee, userId, $"Cập nhật nhân viên {newUserInfo?.Employee?.EmployeeCode}", req.JsonSerialize());

                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    _logger.LogError(ex, "UpdateUser");
                    return GeneralCode.InternalError;
                }

            }

            if (req.AvatarFileId.HasValue && oldAvatarFileId != req.AvatarFileId)
            {
                _asyncRunnerService.RunAsync<IPhysicalFileService>(f => f.FileAssignToObject(req.AvatarFileId.Value, EnumObjectType.UserAndEmployee, userId));
                if (oldAvatarFileId.HasValue)
                {
                    _asyncRunnerService.RunAsync<IPhysicalFileService>(f => f.DeleteFile(oldAvatarFileId.Value));
                }
            }

            return GeneralCode.Success;
        }

        public async Task<Enum> ChangeUserPassword(int userId, UserChangepasswordInput req)
        {
            req.NewPassword = req.NewPassword ?? "";
            if (req.NewPassword.Length < 4)
            {
                return UserErrorCode.PasswordTooShort;
            }

            var userLoginInfo = await _masterContext.User.FirstOrDefaultAsync(u => u.UserId == userId);
            if (userLoginInfo == null)
            {
                return UserErrorCode.UserNotFound;
            }

            if (!Sercurity.VerifyPasswordHash(_appSetting.PasswordPepper, userLoginInfo.PasswordSalt, req.OldPassword, userLoginInfo.PasswordHash))
            {
                return UserErrorCode.OldPasswordIncorrect;
            }

            var (salt, passwordHash) = Sercurity.GenerateHashPasswordHash(_appSetting.PasswordPepper, req.NewPassword);
            userLoginInfo.PasswordSalt = salt;
            userLoginInfo.PasswordHash = passwordHash;
            await _masterContext.SaveChangesAsync();

            return GeneralCode.Success;
        }

        public async Task<IList<RolePermissionModel>> GetMePermission()
        {
            return await _roleService.GetRolesPermission(_currentContextService.RoleInfo.RoleIds);
        }

        /// <summary>
        /// Lấy danh sách user đc quyền truy cập vào moduleId input
        /// </summary>
        /// <param name="currentUserId">Id người dùng hiện tại</param>
        /// <param name="moduleId">moduleId input</param>
        /// <param name="keyword">Từ khóa tìm kiếm</param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize">Bản ghi trên 1 trang</param>
        /// <returns></returns>
        public async Task<PageData<UserInfoOutput>> GetListByModuleId(int currentUserId, int moduleId, string keyword, int pageIndex, int pageSize)
        {
            // user current ? 
            var currentRoleId = _masterContext.User.FirstOrDefault(q => q.UserId == currentUserId).RoleId ?? 0;

            var rolePermissionList = await _roleService.GetRolePermission(currentRoleId);

            var result = new PageData<UserInfoOutput> { Total = 0, List = null };

            if (rolePermissionList.Count > 0)
            {
                var checkPermission = rolePermissionList.Any(q => q.ModuleId == moduleId);

                if (checkPermission)
                {
                    keyword = (keyword ?? "").Trim();

                    var users = (from u in _masterContext.User
                                 join rp in _masterContext.RolePermission on u.RoleId equals rp.RoleId
                                 where rp.ModuleId == moduleId
                                 select u).AsEnumerable();
                    var userIds = users.Select(u => u.UserId);
                    var employees = _organizationContext.Employee.Where(em => userIds.Contains(em.UserId)).AsEnumerable();


                    var query = users.AsEnumerable().Join(employees.AsEnumerable(), u => u.UserId, em => em.UserId, (u, em) => new UserInfoOutput
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
                    });

                    if (!string.IsNullOrWhiteSpace(keyword))
                    {
                        query = from u in query
                                where u.UserName.Contains(keyword)
                                || u.FullName.Contains(keyword)
                                || u.EmployeeCode.Contains(keyword)
                                || u.Email.Contains(keyword)
                                select u;
                    }
                    var userList = query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
                    var totalRecords = query.Count();
                    result.List = userList;
                    result.Total = totalRecords;
                }
                else
                {
                    _logger.LogInformation(message: string.Format("{0} - {1}|{2}", "UserService.GetListByModuleId", currentUserId, "Không có quyền thực hiện chức năng này"));
                }
            }
            return result;
        }

        public async Task<IList<UserBasicInfoOutput>> GetBasicInfos(IList<int> userIds)
        {
            var users = await _masterContext.User.Where(u => userIds.Contains(u.UserId)).Select(u => new { u.UserId, u.UserName }).ToListAsync();

            var employees = await _organizationContext.Employee.Where(e => userIds.Contains(e.UserId)).Select(e => new { e.UserId, e.FullName, e.AvatarFileId }).ToListAsync();

            return users.Join(employees, u => u.UserId, e => e.UserId, (u, e) => new UserBasicInfoOutput
            {
                UserId = u.UserId,
                UserName = u.UserName,
                FullName = e.FullName,
                AvatarFileId = e.AvatarFileId
            }).ToList();
        }


        #region private
        private async Task<Enum> ValidateUserInfoInput(int currentUserId, UserInfoInput req)
        {
            var findByCode = await _organizationContext.Employee.AnyAsync(e => e.UserId != currentUserId && e.EmployeeCode == req.EmployeeCode);
            if (findByCode)
            {
                return UserErrorCode.EmployeeCodeAlreadyExisted;
            }
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
            User user;
            if (!string.IsNullOrWhiteSpace(req.UserName))
            {
                user = await _masterContext.User.FirstOrDefaultAsync(u => u.UserNameHash == userNameHash);
                if (user != null)
                {
                    return UserErrorCode.UserNameExisted;
                }
            }

            user = new User()
            {
                UserName = req.UserName,
                UserNameHash = userNameHash,
                IsDeleted = false,
                CreatedDatetimeUtc = DateTime.UtcNow,
                UserStatusId = (int)req.UserStatusId,
                PasswordSalt = salt,
                PasswordHash = passwordHash,
                RoleId = req.RoleId,
                UpdatedDatetimeUtc = DateTime.UtcNow

            };

            _masterContext.User.Add(user);

            await _masterContext.SaveChangesAsync();

            var a = _masterContext.User.ToList();
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
                UserId = userId,
                AvatarFileId = req.AvatarFileId
            };

            await _organizationContext.Employee.AddAsync(employee);

            await _organizationContext.SaveChangesAsync();

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

            var employee = await _organizationContext.Employee.FirstOrDefaultAsync(u => u.UserId == userId);
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
            employee.AvatarFileId = req.AvatarFileId;

            await _organizationContext.SaveChangesAsync();

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

            var employee = await _organizationContext.Employee.FirstOrDefaultAsync(u => u.UserId == userId);
            if (employee == null)
            {
                return UserErrorCode.UserNotFound;
            }

            employee.IsDeleted = true;

            await _organizationContext.SaveChangesAsync();

            return GeneralCode.Success;
        }

        private async Task<UserFullDbInfo> GetUserFullInfo(int userId)
        {
            var user = await (
                 from u in _masterContext.User
                 where u.UserId == userId
                 select new UserFullDbInfo
                 {
                     User = u,
                 }
             )
             .FirstOrDefaultAsync();

            user.Employee = _organizationContext.Employee.FirstOrDefault(e => e.UserId == userId);

            return user;
        }



        private class UserFullDbInfo
        {
            public User User { get; set; }
            public Employee Employee { get; set; }
        }
        #endregion
    }
}
