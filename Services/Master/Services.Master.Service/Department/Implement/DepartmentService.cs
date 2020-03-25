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
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Model.BusinessInfo;
using VErp.Services.Master.Model.Department;
using DepartmentEntity = VErp.Infrastructure.EF.MasterDB.Department;

namespace VErp.Services.Master.Service.Department.Implement
{
    public class DepartmentService : IDepartmentService
    {
        private readonly MasterDBContext _masterContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;

        public DepartmentService(MasterDBContext masterContext
            , IOptions<AppSetting> appSetting
            , ILogger<DepartmentService> logger
            , IActivityLogService activityLogService
            )
        {
            _masterContext = masterContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
        }

        public async Task<ServiceResult<int>> AddDepartment(int updatedUserId, DepartmentModel data)
        {
            var department = await _masterContext.Department.FirstOrDefaultAsync(d => d.DepartmentCode == data.DepartmentCode || d.DepartmentName == data.DepartmentName);

            if (department != null)
            {
                if (string.Compare(department.DepartmentCode, data.DepartmentCode, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return DepartmentErrorCode.DepartmentCodeAlreadyExisted;
                }

                return DepartmentErrorCode.DepartmentNameAlreadyExisted;
            }
            if (data.ParentId.HasValue)
            {
                var parent = await _masterContext.Department.FirstOrDefaultAsync(d => d.DepartmentId == data.ParentId.Value);
                if (parent == null)
                {
                    return DepartmentErrorCode.DepartmentParentNotFound;
                }
                if (!parent.IsActived)
                {
                    return DepartmentErrorCode.DepartmentParentInActived;
                }
            }
            department = new DepartmentEntity()
            {
                DepartmentCode = data.DepartmentCode,
                DepartmentName = data.DepartmentName,
                ParentId = data.ParentId,
                Description = data.Description,
                IsActived = data.IsActived,
                IsDeleted = false,
                UpdatedTime = DateTime.UtcNow,
                CreatedTime = DateTime.UtcNow,
                UpdatedUserId = updatedUserId
            };

            await _masterContext.Department.AddAsync(department);
            await _masterContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.Department, department.DepartmentId, $"Thêm bộ phận {department.DepartmentName}", data.JsonSerialize());
            return department.DepartmentId;
        }


        public async Task<Enum> DeleteDepartment(int updatedUserId, int departmentId)
        {
            var department = await _masterContext.Department.FirstOrDefaultAsync(d => d.DepartmentId == departmentId);
            if (department == null)
            {
                return DepartmentErrorCode.DepartmentNotFound;
            }
            if (department.IsActived)
            {
                return DepartmentErrorCode.DepartmentActived;
            }
            if (_masterContext.Department.Any(d => d.ParentId == departmentId))
            {
                return DepartmentErrorCode.DepartmentChildAlreadyExisted;
            }
            department.IsDeleted = true;
            department.UpdatedTime = DateTime.UtcNow;
            await _masterContext.SaveChangesAsync();
            await _activityLogService.CreateLog(EnumObjectType.Department, departmentId, $"Xóa bộ phận {department.DepartmentName}", department.JsonSerialize());
            return GeneralCode.Success;
        }

        public async Task<ServiceResult<DepartmentModel>> GetDepartmentInfo(int departmentId)
        {
            var department = await _masterContext.Department.Include(d => d.Parent).FirstOrDefaultAsync(d => d.DepartmentId == departmentId);
            if (department == null)
            {
                return DepartmentErrorCode.DepartmentNotFound;
            }
            return new DepartmentModel()
            {
                DepartmentId = department.DepartmentId,
                DepartmentName = department.DepartmentName,
                DepartmentCode = department.DepartmentCode,
                Description = department.Description,
                ParentId = department.ParentId,
                ParentName = department.Parent?.DepartmentName ?? null,
                IsActived = department.IsActived,
            };
        }

        public async Task<PageData<DepartmentModel>> GetList(string keyword, bool? isActived, int page, int size)
        {
            keyword = (keyword ?? "").Trim();
            var query = _masterContext.Department.Include(d => d.Parent).AsQueryable();
            if (isActived.HasValue)
            {
                query = query.Where(d => d.IsActived == isActived);
            }
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(d => d.DepartmentCode.Contains(keyword) || d.DepartmentName.Contains(keyword) || d.Description.Contains(keyword));
            }
            var a = query.ToList();
            var lst = await (size > 0 ? query.Skip((page - 1) * size).Take(size) : query).Select(d => new DepartmentModel
            {
                DepartmentId = d.DepartmentId,
                DepartmentCode = d.DepartmentCode,
                DepartmentName = d.DepartmentName,
                Description = d.Description,
                IsActived = d.IsActived,
                ParentId = d.ParentId,
                ParentName = d.Parent == null ? null : d.Parent.DepartmentName
            }).ToListAsync();

            var total = await query.CountAsync();

            return (lst, total);
        }

        public async Task<Enum> UpdateDepartment(int updatedUserId, int departmentId, DepartmentModel data)
        {
            var department = await _masterContext.Department.FirstOrDefaultAsync(d => d.DepartmentId == departmentId);
            if (department == null)
            {
                return DepartmentErrorCode.DepartmentNotFound;
            }
            if (data.ParentId.HasValue && department.ParentId != data.ParentId)
            {
                var parent = await _masterContext.Department.FirstOrDefaultAsync(d => d.DepartmentId == data.ParentId.Value);
                if (parent == null)
                {
                    return DepartmentErrorCode.DepartmentParentNotFound;
                }
                if (!parent.IsActived)
                {
                    return DepartmentErrorCode.DepartmentParentInActived;
                }
            }
            // Kiểm tra nếu inActive bộ phận
            if (department.IsActived && !data.IsActived)
            {
                // Check còn phòng ban trực thuộc đang hoạt động
                if(_masterContext.Department.Any(d => d.ParentId == departmentId && d.IsActived))
                {
                    return DepartmentErrorCode.DepartmentChildActivedAlreadyExisted;
                }
                // Check nhân viên trực thuộc phòng ban đang hoạt động
                bool isExisted = _masterContext.UserDepartmentMapping
                    .Where(m => m.DepartmentId == departmentId)
                    .Join(_masterContext.User, m => m.UserId, u => u.UserId, (m, u) => u)
                    .Any(u => !u.IsDeleted && u.UserStatusId == (int)EnumUserStatus.Actived);
                if (isExisted)
                {
                    return DepartmentErrorCode.DepartmentUserActivedAlreadyExisted;
                }
            }

            var exitedDepartment = await _masterContext.Department.FirstOrDefaultAsync(d => d.DepartmentId != departmentId && (d.DepartmentCode == data.DepartmentCode || d.DepartmentName == data.DepartmentName));
            if (exitedDepartment != null)
            {
                if (string.Compare(department.DepartmentCode, data.DepartmentCode, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return DepartmentErrorCode.DepartmentCodeAlreadyExisted;
                }

                return DepartmentErrorCode.DepartmentNameAlreadyExisted;
            }
            department.DepartmentCode = data.DepartmentCode;
            department.DepartmentName = data.DepartmentName;
            department.Description = data.Description;
            department.ParentId = data.ParentId;
            department.IsActived = data.IsActived;
            department.UpdatedTime = DateTime.UtcNow;
            department.UpdatedUserId = updatedUserId;
            await _masterContext.SaveChangesAsync();
            await _activityLogService.CreateLog(EnumObjectType.Department, departmentId, $"Cập nhật bộ phận {department.DepartmentName}", data.JsonSerialize());
            return GeneralCode.Success;
        }
    }
}
