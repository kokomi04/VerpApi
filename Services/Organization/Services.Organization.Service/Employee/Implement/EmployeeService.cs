using Microsoft.EntityFrameworkCore;
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
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Organization.Model.Department;
using VErp.Services.Organization.Model.Employee;
using EmployeeEntity = VErp.Infrastructure.EF.OrganizationDB.Employee;

namespace VErp.Services.Organization.Service.Employee.Implement
{
    public class EmployeeService : IEmployeeService
    {
        private readonly OrganizationDBContext _organizationContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IAsyncRunnerService _asyncRunnerService;

        public EmployeeService(OrganizationDBContext organizationContext
            , IOptions<AppSetting> appSetting
            , ILogger<EmployeeService> logger
            , IAsyncRunnerService asyncRunnerService
            )
        {
            _organizationContext = organizationContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _asyncRunnerService = asyncRunnerService;
        }

        public async Task<int> CreateEmployee(int userId, EmployeeModel req, int updatedUserId)
        {
            var validate = await ValidateEmployee(-1, req);
            if (!validate.IsSuccess())
            {
                throw new BadRequestException(validate);
            }
            using (var trans = await _organizationContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var employee = new EmployeeEntity()
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

                    // Gắn phòng ban cho nhân sự
                    await EmployeeDepartmentMapping(userId, req.DepartmentId, updatedUserId);

                    trans.Commit();
                    return userId;
                }
                catch (Exception)
                {
                    trans.TryRollbackTransaction();
                    throw;
                }
            }
        }

        private async Task<bool> EmployeeDepartmentMapping(int userId, int departmentId, int updatedUserId)
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
            var userDepartmentMapping = _organizationContext.EmployeeDepartmentMapping
                .Where(d => d.DepartmentId == departmentId && d.UserId == userId);
            var current = userDepartmentMapping
           .Where(d => d.ExpirationDate >= DateTime.UtcNow.Date && d.EffectiveDate <= DateTime.UtcNow.Date)
           .FirstOrDefault();

            if (current == null)
            {
                // kiểm tra xem có bản ghi trong tương lai
                var future = userDepartmentMapping.Where(d => d.EffectiveDate > DateTime.UtcNow.Date).OrderBy(d => d.EffectiveDate).FirstOrDefault();
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
            await _organizationContext.SaveChangesAsync();
            return true;
        }

        public async Task<EmployeeModel> GetInfo(int userId)
        {
            var user = await (
                 from em in _organizationContext.Employee
                 where em.UserId == userId
                 select new EmployeeModel
                 {
                     UserId = em.UserId,
                     EmployeeCode = em.EmployeeCode,
                     FullName = em.FullName,
                     Address = em.Address,
                     Email = em.Email,
                     GenderId = (EnumGender?)em.GenderId,
                     Phone = em.Phone,
                     AvatarFileId = em.AvatarFileId
                 }
             )
             .FirstOrDefaultAsync();

            if (user == null)
            {
                throw new BadRequestException(UserErrorCode.UserNotFound);
            }
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

        public async Task<bool> DeleteEmployee(int userId)
        {
            var employee = await _organizationContext.Employee.FirstOrDefaultAsync(e => e.UserId == userId);
            long? oldAvatarFileId = employee.AvatarFileId;
            if (employee == null)
            {
                throw new BadRequestException(EmployeeErrorCode.EmployeeNotFound);
            }
            employee.IsDeleted = true;
            await _organizationContext.SaveChangesAsync();

            if (oldAvatarFileId.HasValue)
            {
                _asyncRunnerService.RunAsync<IPhysicalFileService>(f => f.DeleteFile(oldAvatarFileId.Value));
            }

            return true;
        }

        public async Task<bool> UpdateEmployee(int userId, EmployeeModel req, int updatedUserId)
        {
            var validate = await ValidateEmployee(userId, req);
            if (!validate.IsSuccess())
            {
                throw new BadRequestException(validate);
            }
            var employee = await _organizationContext.Employee.FirstOrDefaultAsync(u => u.UserId == userId);
            if (employee == null)
            {
                throw new BadRequestException(UserErrorCode.UserNotFound);
            }
            long? oldAvatarFileId = employee.AvatarFileId;
            using (var trans = await _organizationContext.Database.BeginTransactionAsync())
            {
                try
                {
                    employee.EmployeeCode = req.EmployeeCode;
                    employee.FullName = req.FullName;
                    employee.Email = req.Email;
                    employee.Address = req.Address;
                    employee.GenderId = (int?)req.GenderId;
                    employee.Phone = req.Phone;
                    employee.AvatarFileId = req.AvatarFileId;

                    await _organizationContext.SaveChangesAsync();

                    // Lấy thông tin bộ phận hiện tại
                    DateTime currentDate = DateTime.UtcNow.Date;
                    var departmentId = _organizationContext.EmployeeDepartmentMapping
                        .Where(m => m.UserId == userId && m.ExpirationDate >= currentDate && m.EffectiveDate <= currentDate)
                        .Select(d => d.DepartmentId).FirstOrDefault();
                    // Nếu khác update lại thông tin
                    if (departmentId != req.DepartmentId)
                    {
                        await EmployeeDepartmentMapping(userId, req.DepartmentId, updatedUserId);                       
                    }
                    trans.Commit();
                }
                catch (Exception)
                {
                    trans.TryRollbackTransaction();
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

        private async Task<Enum> ValidateEmployee(int currentUserId, EmployeeModel req)
        {
            var findByCode = await _organizationContext.Employee.AnyAsync(e => e.UserId != currentUserId && e.EmployeeCode == req.EmployeeCode);
            if (findByCode)
            {
                return EmployeeErrorCode.EmployeeCodeAlreadyExisted;
            }
            return GeneralCode.Success;
        }
    }
}
