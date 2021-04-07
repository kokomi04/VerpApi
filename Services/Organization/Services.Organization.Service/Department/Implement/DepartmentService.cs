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
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Organization.Model.Department;
using VErp.Infrastructure.EF.EFExtensions;
using DepartmentEntity = VErp.Infrastructure.EF.OrganizationDB.Department;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;

namespace VErp.Services.Organization.Service.Department.Implement
{
    public class DepartmentService : IDepartmentService
    {
        private readonly OrganizationDBContext _organizationContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly ICurrentContextService _currentContextService;
        private readonly IAsyncRunnerService _asyncRunnerService;

        public DepartmentService(OrganizationDBContext organizationContext
            , IOptions<AppSetting> appSetting
            , ILogger<DepartmentService> logger
            , IActivityLogService activityLogService
            , ICurrentContextService currentContextService
            , IAsyncRunnerService asyncRunnerService
            )
        {
            _organizationContext = organizationContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
            _currentContextService = currentContextService;
            _asyncRunnerService = asyncRunnerService;
        }

        public async Task<int> AddDepartment(DepartmentModel data)
        {
            var department = await _organizationContext.Department.FirstOrDefaultAsync(d => d.DepartmentCode == data.DepartmentCode || d.DepartmentName == data.DepartmentName);

            if (department != null)
            {
                if (string.Compare(department.DepartmentCode, data.DepartmentCode, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    throw new BadRequestException(DepartmentErrorCode.DepartmentCodeAlreadyExisted);
                }

                throw new BadRequestException(DepartmentErrorCode.DepartmentNameAlreadyExisted);
            }
            if (data.ParentId.HasValue)
            {
                var parent = await _organizationContext.Department.FirstOrDefaultAsync(d => d.DepartmentId == data.ParentId.Value);
                if (parent == null)
                {
                    throw new BadRequestException(DepartmentErrorCode.DepartmentParentNotFound);
                }
                if (!parent.IsActived)
                {
                    throw new BadRequestException(DepartmentErrorCode.DepartmentParentInActived);
                }
            }
            department = new DepartmentEntity()
            {
                DepartmentCode = data.DepartmentCode,
                DepartmentName = data.DepartmentName,
                ParentId = data.ParentId,
                Description = data.Description,
                IsActived = data.IsActived,
                IsProduction = data.IsProduction,
                WorkingHoursPerDay = data.WorkingHoursPerDay,
                ImageFileId = data.ImageFileId,
                IsDeleted = false,
            };

            await _organizationContext.Department.AddAsync(department);
            await _organizationContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.Department, department.DepartmentId, $"Thêm bộ phận {department.DepartmentName}", data.JsonSerialize());
            if (data.ImageFileId.HasValue)
            {
                _asyncRunnerService.RunAsync<IPhysicalFileService>(s => s.FileAssignToObject(data.ImageFileId.Value, EnumObjectType.Department, department.DepartmentId));
            }
            return department.DepartmentId;
        }


        public async Task<bool> DeleteDepartment(int departmentId)
        {
            var department = await _organizationContext.Department.FirstOrDefaultAsync(d => d.DepartmentId == departmentId);
            if (department == null)
            {
                throw new BadRequestException(DepartmentErrorCode.DepartmentNotFound);
            }
            if (department.IsActived)
            {
                throw new BadRequestException(DepartmentErrorCode.DepartmentActived);
            }
            if (_organizationContext.Department.Any(d => d.ParentId == departmentId))
            {
                throw new BadRequestException(DepartmentErrorCode.DepartmentChildAlreadyExisted);
            }
            department.IsDeleted = true;
            await _organizationContext.SaveChangesAsync();
            await _activityLogService.CreateLog(EnumObjectType.Department, departmentId, $"Xóa bộ phận {department.DepartmentName}", department.JsonSerialize());
            if (department.ImageFileId.HasValue)
            {
                _asyncRunnerService.RunAsync<IPhysicalFileService>(s => s.DeleteFile(department.ImageFileId.Value));
            }
            return true;
        }

        public async Task<DepartmentModel> GetDepartmentInfo(int departmentId)
        {
            var department = await _organizationContext.Department.Include(d => d.Parent).FirstOrDefaultAsync(d => d.DepartmentId == departmentId);
            if (department == null)
            {
                throw new BadRequestException(DepartmentErrorCode.DepartmentNotFound);
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
                IsProduction = department.IsProduction,
                WorkingHoursPerDay = department.WorkingHoursPerDay,
                ImageFileId = department.ImageFileId
            };
        }

        public async Task<PageData<DepartmentModel>> GetList(string keyword, IList<int> departmentIds, bool? isProduction,  bool? isActived, int page, int size, Clause filters = null)
        {
            keyword = (keyword ?? "").Trim();
            var query = _organizationContext.Department.Include(d => d.Parent).AsQueryable();
            if (departmentIds != null && departmentIds.Count > 0)
            {
                query = query.Where(d => departmentIds.Contains(d.DepartmentId));
            }
            if (isActived.HasValue)
            {
                query = query.Where(d => d.IsActived == isActived);
            }

            if (isProduction.HasValue)
            {
                query = query.Where(d => d.IsProduction == isProduction.Value);
            }
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(d => d.DepartmentCode.Contains(keyword) || d.DepartmentName.Contains(keyword) || d.Description.Contains(keyword));
            }
            query = query.InternalFilter(filters, _currentContextService.TimeZoneOffset);
            var lst = await (size > 0 ? query.Skip((page - 1) * size).Take(size) : query).Select(d => new DepartmentModel
            {
                DepartmentId = d.DepartmentId,
                DepartmentCode = d.DepartmentCode,
                DepartmentName = d.DepartmentName,
                Description = d.Description,
                IsActived = d.IsActived,
                ParentId = d.ParentId,
                ParentName = d.Parent == null ? null : d.Parent.DepartmentName,
                IsProduction = d.IsProduction,
                WorkingHoursPerDay = d.WorkingHoursPerDay,
                ImageFileId = d.ImageFileId
            }).ToListAsync();

            var total = await query.CountAsync();

            return (lst, total);
        }


        public async Task<IList<DepartmentModel>> GetListByIds(IList<int> departmentIds)
        {
            if (departmentIds == null || departmentIds.Count == 0)
            {
                return new List<DepartmentModel>();
            }
            var query = _organizationContext.Department.Where(d => departmentIds.Contains(d.DepartmentId)).Include(d => d.Parent).AsQueryable();

            var lst = await query.Select(d => new DepartmentModel
            {
                DepartmentId = d.DepartmentId,
                DepartmentCode = d.DepartmentCode,
                DepartmentName = d.DepartmentName,
                Description = d.Description,
                IsActived = d.IsActived,
                ParentId = d.ParentId,
                ParentName = d.Parent == null ? null : d.Parent.DepartmentName,
                IsProduction = d.IsProduction,
                WorkingHoursPerDay = d.WorkingHoursPerDay,
                ImageFileId = d.ImageFileId
            }).ToListAsync();

            return lst;
        }


        public async Task<bool> UpdateDepartment(int departmentId, DepartmentModel data)
        {
            long? deleteImageFileId = null;
            var department = await _organizationContext.Department.FirstOrDefaultAsync(d => d.DepartmentId == departmentId);
            if (department == null)
            {
                throw new BadRequestException(DepartmentErrorCode.DepartmentNotFound);
            }
            if (department.ImageFileId != data.ImageFileId)
            {
                deleteImageFileId = department.ImageFileId;
            }
            if (data.ParentId.HasValue && department.ParentId != data.ParentId)
            {
                var parent = await _organizationContext.Department.FirstOrDefaultAsync(d => d.DepartmentId == data.ParentId.Value);
                if (parent == null)
                {
                    throw new BadRequestException(DepartmentErrorCode.DepartmentParentNotFound);
                }
                if (!parent.IsActived)
                {
                    throw new BadRequestException(DepartmentErrorCode.DepartmentParentInActived);
                }
            }
            // Kiểm tra nếu inActive bộ phận
            if (department.IsActived && !data.IsActived)
            {
                // Check còn phòng ban trực thuộc đang hoạt động
                if (_organizationContext.Department.Any(d => d.ParentId == departmentId && d.IsActived))
                {
                    throw new BadRequestException(DepartmentErrorCode.DepartmentChildActivedAlreadyExisted);
                }
                // Check nhân viên trực thuộc phòng ban đang hoạt động
                bool isExisted = _organizationContext.EmployeeDepartmentMapping
                    .Where(m => m.DepartmentId == departmentId)
                    .Join(_organizationContext.Employee, m => m.UserId, e => e.UserId, (m, e) => e)
                    .Any(e => !e.IsDeleted);
                if (isExisted)
                {
                    throw new BadRequestException(DepartmentErrorCode.DepartmentUserActivedAlreadyExisted);
                }
            }

            var exitedDepartment = await _organizationContext.Department.FirstOrDefaultAsync(d => d.DepartmentId != departmentId && (d.DepartmentCode == data.DepartmentCode || d.DepartmentName == data.DepartmentName));
            if (exitedDepartment != null)
            {
                if (string.Compare(department.DepartmentCode, data.DepartmentCode, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    throw new BadRequestException(DepartmentErrorCode.DepartmentCodeAlreadyExisted);
                }

                throw new BadRequestException(DepartmentErrorCode.DepartmentNameAlreadyExisted);
            }
            department.DepartmentCode = data.DepartmentCode;
            department.DepartmentName = data.DepartmentName;
            department.Description = data.Description;
            department.ParentId = data.ParentId;
            department.IsActived = data.IsActived;
            department.IsProduction = data.IsProduction;
            department.WorkingHoursPerDay = data.WorkingHoursPerDay;
            department.ImageFileId = data.ImageFileId;

            await _organizationContext.SaveChangesAsync();
            await _activityLogService.CreateLog(EnumObjectType.Department, departmentId, $"Cập nhật bộ phận {department.DepartmentName}", data.JsonSerialize());

            if (deleteImageFileId.HasValue)
            {
                _asyncRunnerService.RunAsync<IPhysicalFileService>(s => s.DeleteFile(deleteImageFileId.Value));
            }

            if (data.ImageFileId.HasValue)
            {
                _asyncRunnerService.RunAsync<IPhysicalFileService>(s => s.FileAssignToObject(data.ImageFileId.Value, EnumObjectType.Department, department.DepartmentId));
            }
            return true;
        }
    }
}
