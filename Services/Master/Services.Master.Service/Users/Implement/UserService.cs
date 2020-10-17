using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenXmlPowerTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.EFExtensions;
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
        private readonly UnAuthorizeMasterDBContext _unAuthorizeMasterDBContext;
        private readonly OrganizationDBContext _organizationContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IRoleService _roleService;
        private readonly IActivityLogService _activityLogService;
        private readonly ICurrentContextService _currentContextService;
        private readonly IAsyncRunnerService _asyncRunnerService;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IMapper _mapper;
        public UserService(MasterDBContext masterContext
            , UnAuthorizeMasterDBContext unAuthorizeMasterDBContext
            , OrganizationDBContext organizationContext
            , IOptions<AppSetting> appSetting
            , ILogger<UserService> logger
            , IRoleService roleService
            , IActivityLogService activityLogService
            , ICurrentContextService currentContextService
            , IAsyncRunnerService asyncRunnerService
            , IServiceScopeFactory serviceScopeFactory
            , IMapper mapper
            )
        {
            _masterContext = masterContext;
            _unAuthorizeMasterDBContext = unAuthorizeMasterDBContext;
            _organizationContext = organizationContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _roleService = roleService;
            _activityLogService = activityLogService;
            _currentContextService = currentContextService;
            _asyncRunnerService = asyncRunnerService;
            _serviceScopeFactory = serviceScopeFactory;
            _mapper = mapper;
        }


        public async Task<int> CreateOwnerUser(int subsidiaryId, UserInfoInput req)
        {
            var subsidiaryInfo = await _organizationContext.Subsidiary.IgnoreQueryFilters().FirstOrDefaultAsync(s => !s.IsDeleted && s.SubsidiaryId == subsidiaryId);
            if (subsidiaryInfo == null)
            {
                throw new BadRequestException(GeneralCode.InvalidParams, $"Không tìm thấy thông tin công ty");
            }

            var owner = await _organizationContext.Employee.IgnoreQueryFilters().FirstOrDefaultAsync(e => !e.IsDeleted && e.SubsidiaryId == subsidiaryId && e.EmployeeTypeId == (int)EnumEmployeeType.Owner);

            if (owner != null)
            {
                throw new BadRequestException(GeneralCode.InvalidParams, $"Công ty đã tạo tài khoản sở hữu!");
            }

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var currentContextFactory = scope.ServiceProvider.GetRequiredService<ICurrentContextFactory>();
                var newContext = new ScopeCurrentContextService(_currentContextService);
                newContext.SetSubsidiaryId(subsidiaryId);
                currentContextFactory.SetCurrentContext(newContext);

                var roleService = scope.ServiceProvider.GetService<IRoleService>();
                var adminRole = await roleService.GetAdminRoleInfo();
                if (adminRole == null)
                {
                    req.RoleId = await roleService.AddRole(new RoleInput()
                    {
                        RoleStatusId = EnumRoleStatus.Active,
                        RoleName = $"{subsidiaryInfo.SubsidiaryCode} Admin"
                    }, EnumRoleType.Administrator);

                    req.UserStatusId = EnumUserStatus.Actived;
                }
                else
                {
                    req.RoleId = adminRole.RoleId;
                }

                var obj = scope.ServiceProvider.GetService<IUserService>();
                return await obj.CreateUser(req, EnumEmployeeType.Owner);
            }
        }

        public async Task<int> CreateUser(UserInfoInput req, EnumEmployeeType employeeTypeId)
        {
            var validate = await ValidateUserInfoInput(-1, req);
            if (!validate.IsSuccess())
            {
                throw new BadRequestException(validate);
            }

            await using var trans = new MultipleDbTransaction(_masterContext, _organizationContext);
            try
            {
                var userId = await CreateEmployee(req, employeeTypeId);

                await CreateUserAuthen(userId, req);

                // Gắn phòng ban cho nhân sự
                if (req.DepartmentId.HasValue)
                {
                    await EmployeeDepartmentMapping(userId, req.DepartmentId.Value);
                }

                trans.Commit();

                var info = await GetUserFullInfo(userId);

                await _activityLogService.CreateLog(EnumObjectType.UserAndEmployee, userId, $"Thêm mới nhân viên {info?.Employee?.EmployeeCode}", req.JsonSerialize());

                _logger.LogInformation("CreateUser({0}) successful!", userId);

                if (req.AvatarFileId.HasValue)
                {
                    _asyncRunnerService.RunAsync<IPhysicalFileService>(f => f.FileAssignToObject(req.AvatarFileId.Value, EnumObjectType.UserAndEmployee, userId));
                }

                return userId;
            }

            catch (Exception)
            {
                await trans.TryRollbackTransactionAsync();
                throw;
            }
        }

        private async Task<bool> EmployeeDepartmentMapping(int userId, int departmentId)
        {
            var department = await _organizationContext.Department.FirstOrDefaultAsync(d => d.DepartmentId == departmentId);
            if (department == null)
            {
                throw new BadRequestException(DepartmentErrorCode.DepartmentNotFound);
            }
            if (!department.IsActived)
            {
                throw new BadRequestException(DepartmentErrorCode.DepartmentInActived);
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
                    UpdatedUserId = _currentContextService.UserId,
                    CreatedTime = DateTime.UtcNow,
                    UpdatedTime = DateTime.UtcNow
                });
            }
            else
            {
                current.ExpirationDate = DateTime.UtcNow.AddDays(-1).Date;
                current.UpdatedTime = DateTime.UtcNow;
                current.UpdatedUserId = _currentContextService.UserId;

                _organizationContext.EmployeeDepartmentMapping.Add(new EmployeeDepartmentMapping()
                {
                    DepartmentId = departmentId,
                    UserId = userId,
                    EffectiveDate = DateTime.UtcNow.Date,
                    ExpirationDate = current.ExpirationDate,
                    UpdatedUserId = _currentContextService.UserId,
                    CreatedTime = DateTime.UtcNow,
                    UpdatedTime = DateTime.UtcNow
                });
            }
            await _masterContext.SaveChangesAsync();
            return true;
        }

        public async Task<UserInfoOutput> GetInfo(int userId)
        {
            var ur = await _masterContext.User.FirstOrDefaultAsync(u => u.UserId == userId);
            var em = await _organizationContext.Employee.FirstOrDefaultAsync(e => e.UserId == userId);
            if (ur == null || em == null)
            {
                throw new BadRequestException(UserErrorCode.UserNotFound);
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
                AvatarFileId = em.AvatarFileId,
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

        public async Task<bool> DeleteUser(int userId)
        {
            var userInfo = await GetUserFullInfo(userId);
            long? oldAvatarFileId = userInfo.Employee.AvatarFileId;

            await using (var trans = new MultipleDbTransaction(_masterContext, _organizationContext))
            {
                try
                {
                    var r = await DeleteEmployee(userId);
                    if (!r.IsSuccess())
                    {
                        trans.Rollback();
                        throw new BadRequestException(r);
                    }

                    var user = await DeleteUserAuthen(userId);
                    if (!user.IsSuccess())
                    {
                        trans.Rollback();
                        throw new BadRequestException(user);
                    }

                    //await DeleteEmployeeSubsidiary(userId);

                    trans.Commit();

                    await _activityLogService.CreateLog(EnumObjectType.UserAndEmployee, userId, $"Xóa nhân viên {userInfo?.Employee?.EmployeeCode}", userInfo.JsonSerialize());


                }
                catch (Exception)
                {
                    trans.TryRollbackTransaction();
                    throw;
                }

            }

            if (oldAvatarFileId.HasValue)
            {
                _asyncRunnerService.RunAsync<IPhysicalFileService>(f => f.DeleteFile(oldAvatarFileId.Value));
            }

            return true;
        }

        public async Task<PageData<UserInfoOutput>> GetList(string keyword, int page, int size, Clause filters = null)
        {
            keyword = (keyword ?? "").Trim();
            var employees = _organizationContext.Employee.AsQueryable();
            var users = _masterContext.User.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                employees = employees.Where(em => em.FullName.Contains(keyword)
                    || em.EmployeeCode.Contains(keyword)
                    || em.Email.Contains(keyword));

                var userIds = await employees.Select(e => e.UserId).ToListAsync();
                users = users.Where(u => u.UserName.Contains(keyword) || userIds.Contains(u.UserId));
            }

            var total = await employees.CountAsync();

            var lstEmployees = await (size > 0 ? employees.OrderBy(u => u.UserStatusId).ThenBy(u => u.FullName).Skip((page - 1) * size).Take(size).ToListAsync() : employees.OrderBy(u => u.UserStatusId).ThenBy(u => u.FullName).ToListAsync());

            var selectedUserIds = lstEmployees.Select(u => u.UserId).ToList();

            var lstUsers = users.Where(e => selectedUserIds.Contains(e.UserId));

            var lst = lstUsers.AsEnumerable().Join(lstEmployees, u => u.UserId, em => em.UserId, (u, em) => new UserInfoOutput
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
            })
            .ToList();


            return (lst, total);
        }


        public async Task<IList<UserInfoOutput>> GetListByUserIds(IList<int> userIds)
        {
            if (userIds == null || userIds.Count == 0)
                return new List<UserInfoOutput>();

            var userInfos = await _masterContext.User.AsNoTracking().Where(u => userIds.Contains(u.UserId)).ToListAsync();

            var employees = await _organizationContext.Employee.AsNoTracking().Where(u => userIds.Contains(u.UserId)).ToListAsync();


            var lst = (from u in userInfos
                       join e in employees on u.UserId equals e.UserId
                       select new UserInfoOutput
                       {
                           UserId = u.UserId,
                           UserName = u.UserName,
                           UserStatusId = (EnumUserStatus)u.UserStatusId,
                           RoleId = u.RoleId,
                           EmployeeCode = e.EmployeeCode,
                           FullName = e.FullName,
                           Address = e.Address,
                           Email = e.Email,
                           GenderId = (EnumGender?)e.GenderId,
                           Phone = e.Phone
                       }).ToList();


            return lst;
        }

        public async Task<bool> UpdateUser(int userId, UserInfoInput req)
        {
            var validate = await ValidateUserInfoInput(userId, req);
            if (!validate.IsSuccess())
            {
                throw new BadRequestException(validate);
            }

            var userInfo = await GetUserFullInfo(userId);

            long? oldAvatarFileId = userInfo.Employee.AvatarFileId;
            await using (var trans = new MultipleDbTransaction(_masterContext, _organizationContext))
            {
                try
                {
                    var r1 = await UpdateUserAuthen(userId, req);
                    if (!r1.IsSuccess())
                    {
                        trans.Rollback();
                        throw new BadRequestException(r1);
                    }

                    var r2 = await UpdateEmployee(userId, req);
                    if (!r2.IsSuccess())
                    {
                        trans.Rollback();
                        throw new BadRequestException(r2);
                    }

                    // Lấy thông tin bộ phận hiện tại
                    DateTime currentDate = DateTime.UtcNow.Date;
                    var departmentId = _organizationContext.EmployeeDepartmentMapping
                        .Where(m => m.UserId == userId && m.ExpirationDate >= currentDate && m.EffectiveDate <= currentDate)
                        .Select(d => d.DepartmentId).FirstOrDefault();

                    // Nếu khác update lại thông tin
                    if (departmentId != req.DepartmentId && req.DepartmentId.HasValue)
                    {
                        await EmployeeDepartmentMapping(userId, req.DepartmentId.Value);
                    }

                    trans.Commit();

                    var newUserInfo = await GetUserFullInfo(userId);

                    await _activityLogService.CreateLog(EnumObjectType.UserAndEmployee, userId, $"Cập nhật nhân viên {newUserInfo?.Employee?.EmployeeCode}", req.JsonSerialize());

                }
                catch (Exception)
                {
                    trans.Rollback();
                    throw;
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

            return true;
        }

        public async Task<bool> ChangeUserPassword(int userId, UserChangepasswordInput req)
        {
            req.NewPassword = req.NewPassword ?? "";
            if (req.NewPassword.Length < 4)
            {
                throw new BadRequestException(UserErrorCode.PasswordTooShort);
            }

            var userLoginInfo = await _masterContext.User.FirstOrDefaultAsync(u => u.UserId == userId);
            if (userLoginInfo == null)
            {
                throw new BadRequestException(UserErrorCode.UserNotFound);
            }

            if (!Sercurity.VerifyPasswordHash(_appSetting.PasswordPepper, userLoginInfo.PasswordSalt, req.OldPassword, userLoginInfo.PasswordHash))
            {
                throw new BadRequestException(UserErrorCode.OldPasswordIncorrect);
            }

            var (salt, passwordHash) = Sercurity.GenerateHashPasswordHash(_appSetting.PasswordPepper, req.NewPassword);
            userLoginInfo.PasswordSalt = salt;
            userLoginInfo.PasswordHash = passwordHash;
            await _masterContext.SaveChangesAsync();

            return true;
        }

        public async Task<IList<RolePermissionModel>> GetMePermission()
        {
            return await _roleService.GetRolesPermission(_currentContextService.RoleInfo.RoleIds, _currentContextService.IsDeveloper);
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

        public CategoryNameModel GetCustomerFieldDataForMapping()
        {
            var result = new CategoryNameModel()
            {
                CategoryId = 1,
                CategoryCode = "Employee",
                CategoryTitle = "Nhân Viên",
                IsTreeView = false,
                Fields = new List<CategoryFieldNameModel>()
            };

            var fields = Utils.GetFieldNameModels<UserImportModel>();
            result.Fields = fields;
            return result;
        }

        public async Task<bool> ImportUserFromMapping(ImportExcelMapping mapping, Stream stream)
        {
            var reader = new ExcelReader(stream);

            var genderData = new Dictionary<string, EnumGender> {
                { EnumGender.Male.GetEnumDescription(), EnumGender.Male },
                { EnumGender.Female.GetEnumDescription(), EnumGender.Female }
            };
            var userStatusData = new Dictionary<string, EnumUserStatus> {
                { EnumUserStatus.InActived.GetEnumDescription(), EnumUserStatus.InActived },
                { EnumUserStatus.Actived.GetEnumDescription(), EnumUserStatus.Actived },
                { EnumUserStatus.Locked.GetEnumDescription(), EnumUserStatus.Locked }
            };
            var roleData = _masterContext.Role
                .Select(r => new { r.RoleId, r.RoleName })
                .ToList()
                .GroupBy(r => r.RoleName)
                .ToDictionary(r => r.Key, r => r.First().RoleId);

            var lstData = reader.ReadSheetEntity<UserImportModel>(mapping, (entity, propertyName, value) =>
            {
                if (string.IsNullOrWhiteSpace(value)) return true;
                switch (propertyName)
                {
                    case nameof(UserImportModel.GenderId):
                        if (!genderData.ContainsKey(value)) throw new BadRequestException(UserErrorCode.GenderTypeInvalid, $"Giới tính {value} không đúng");
                        entity.GenderId = genderData[value];
                        return true;
                    case nameof(UserImportModel.UserStatusId):
                        if (!userStatusData.ContainsKey(value)) throw new BadRequestException(UserErrorCode.StatusTypeInvalid, $"Trạng thái {value} không đúng");
                        entity.UserStatusId = userStatusData[value];
                        return true;
                    case nameof(UserImportModel.RoleId):
                        if (!roleData.ContainsKey(value)) throw new BadRequestException(RoleErrorCode.RoleNotFound, $"Nhóm quyền {value} không đúng");
                        entity.RoleId = roleData[value];
                        return true;
                }
                return false;
            });

            var userModels = new List<UserInfoInput>();

            foreach (var userModel in lstData)
            {
                var userInfo = _mapper.Map<UserInfoInput>(userModel);
                userModels.Add(userInfo);
            }

            await CreateBatchUser(userModels, EnumEmployeeType.Normal);

            return true;
        }

        #region private
        private async Task<bool> CreateBatchUser(List<UserInfoInput> req, EnumEmployeeType employeeTypeId)
        {
            await using var trans = new MultipleDbTransaction(_masterContext, _organizationContext);
            try
            {
                var employees = await AddBatchEmployees(req, employeeTypeId);

                await AddBatchUserAuthen(employees);

                trans.Commit();

                foreach (var info in employees)
                {

                    await _activityLogService.CreateLog(EnumObjectType.UserAndEmployee, info.userId, $"Thêm mới nhân viên {info.userInfo.EmployeeCode}", req.JsonSerialize());

                    _logger.LogInformation("CreateUser({0}) successful!", info.userId);
                }
                return true;
            }

            catch (Exception)
            {
                await trans.TryRollbackTransactionAsync();
                throw;
            }
        }
        private async Task<Enum> ValidateUserInfoInput(int currentUserId, UserInfoInput req)
        {
            var findByCode = await _organizationContext.Employee.AnyAsync(e => e.UserId != currentUserId && e.EmployeeCode == req.EmployeeCode);
            if (findByCode)
            {
                return UserErrorCode.EmployeeCodeAlreadyExisted;
            }

            var roleInfo = await _masterContext.Role.FirstOrDefaultAsync(r => r.RoleId == req.RoleId);
            if (roleInfo == null)
            {
                throw new BadRequestException(GeneralCode.ItemNotFound, $"Không tìm thấy nhóm quyền trong hệ thống");
            }


            if (currentUserId > 0)
            {
                var userInfo = await _masterContext.User.FirstOrDefaultAsync(u => u.UserId == currentUserId);
                var employeeInfo = await _organizationContext.Employee.FirstOrDefaultAsync(u => u.UserId == currentUserId);

                if (employeeInfo.EmployeeTypeId == (int)EnumEmployeeType.Owner && req.RoleId != userInfo.RoleId)
                {
                    throw new BadRequestException(GeneralCode.InvalidParams, $"Không thể thay đổi nhóm quyền sở hữu");
                }
            }


            //if (!Enum.IsDefined(req.UserStatusId.GetType(), req.UserStatusId))
            //{
            //    return GeneralCode.InvalidParams;
            //}
            return GeneralCode.Success;
        }
        private async Task<int> CreateUserAuthen(int userId, UserInfoInput req)
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
                    throw new BadRequestException(UserErrorCode.UserNameExisted);
                }
            }

            user = new User()
            {
                UserId = userId,
                UserName = req.UserName,
                UserNameHash = userNameHash,
                IsDeleted = false,
                CreatedDatetimeUtc = DateTime.UtcNow,
                UserStatusId = (int)req.UserStatusId,
                PasswordSalt = salt,
                PasswordHash = passwordHash,
                RoleId = req.RoleId,
                UpdatedDatetimeUtc = DateTime.UtcNow,
                AccessFailedCount = 0,
                SubsidiaryId = _currentContextService.SubsidiaryId
            };

            _masterContext.User.Add(user);

            await _masterContext.SaveChangesAsync();

            return user.UserId;
        }

        private async Task<List<User>> AddBatchUserAuthen(List<(int userId, UserInfoInput userInfo)> ls)
        {
            var users = new List<User>();
            foreach (var req in ls)
            {
                var (salt, passwordHash) = Sercurity.GenerateHashPasswordHash(_appSetting.PasswordPepper, req.userInfo.Password);
                req.userInfo.UserName = (req.userInfo.UserName ?? "").Trim().ToLower();

                var userNameHash = req.userInfo.UserName.ToGuid();
                User user;
                if (!string.IsNullOrWhiteSpace(req.userInfo.UserName))
                {
                    user = await _masterContext.User.FirstOrDefaultAsync(u => u.UserNameHash == userNameHash);
                    if (user != null)
                    {
                        throw new BadRequestException(UserErrorCode.UserNameExisted);
                    }
                }

                users.Add(new User()
                {
                    UserId = req.userId,
                    UserName = req.userInfo.UserName,
                    UserNameHash = userNameHash,
                    IsDeleted = false,
                    CreatedDatetimeUtc = DateTime.UtcNow,
                    UserStatusId = (int)req.userInfo.UserStatusId,
                    PasswordSalt = salt,
                    PasswordHash = passwordHash,
                    RoleId = req.userInfo.RoleId,
                    UpdatedDatetimeUtc = DateTime.UtcNow,
                    AccessFailedCount = 0,
                    SubsidiaryId = _currentContextService.SubsidiaryId
                });
            };

            await _masterContext.User.AddRangeAsync(users);

            await _masterContext.SaveChangesAsync();

            return users;
        }

        private async Task<int> CreateEmployee(UserInfoInput req, EnumEmployeeType employeeTypeId)
        {
            var employee = new Employee()
            {
                EmployeeCode = req.EmployeeCode,
                FullName = req.FullName,
                Email = req.Email,
                Address = req.Address,
                GenderId = (int?)req.GenderId,
                Phone = req.Phone,
                AvatarFileId = req.AvatarFileId,
                SubsidiaryId = _currentContextService.SubsidiaryId,
                EmployeeTypeId = (int)employeeTypeId,
                UserStatusId = (int)req.UserStatusId,
            };

            await _organizationContext.Employee.AddAsync(employee);

            await _organizationContext.SaveChangesAsync();

            return employee.UserId;
        }

        private async Task<List<(int userId, UserInfoInput userInfo)>> AddBatchEmployees(List<UserInfoInput> userInfos, EnumEmployeeType employeeTypeId)
        {
            var employees = userInfos.Select(e => new Employee()
            {
                EmployeeCode = e.EmployeeCode,
                FullName = e.FullName,
                Email = e.Email,
                Address = e.Address,
                GenderId = (int?)e.GenderId,
                Phone = e.Phone,
                AvatarFileId = e.AvatarFileId,
                SubsidiaryId = _currentContextService.SubsidiaryId,
                EmployeeTypeId = (int)employeeTypeId,
                UserStatusId = (int)e.UserStatusId,
            }).ToList();
            await _organizationContext.Employee.AddRangeAsync(employees);

            await _organizationContext.SaveChangesAsync();
            var rs = new List<(int userId, UserInfoInput userInfo)>();
            for (int i = 0; i < employees.Count(); i++)
            {
                rs.Add((employees[i].UserId, userInfos[i]));
            }
            return rs;
        }

        private async Task<Enum> UpdateUserAuthen(int userId, UserInfoInput req)
        {
            var user = await _masterContext.User.FirstOrDefaultAsync(u => u.UserId == userId);
            if (user == null)
            {
                return UserErrorCode.UserNotFound;
            }
            if (req.UserStatusId == EnumUserStatus.Actived)
                user.AccessFailedCount = 0;
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
            employee.UserStatusId = (int)req.UserStatusId;

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
            if (employee.EmployeeTypeId == (int)EnumEmployeeType.Owner)
            {
                throw new BadRequestException(GeneralCode.InvalidParams, $"Không thể xóa nhân viên sở hữu");
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
