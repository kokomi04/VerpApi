using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Verp.Resources.Organization.Department;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.Organization;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface.DynamicBill;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Organization.Model.Customer;
using VErp.Services.Organization.Model.Department;
using DepartmentEntity = VErp.Infrastructure.EF.OrganizationDB.Department;

namespace VErp.Services.Organization.Service.Department.Implement
{
    public class DepartmentService : IDepartmentService
    {
        private readonly OrganizationDBContext _organizationContext;
        private readonly IAsyncRunnerService _asyncRunnerService;
        private readonly ObjectActivityLogFacade _departmentActivityLog;

        public DepartmentService(OrganizationDBContext organizationContext
            , IActivityLogService activityLogService
            , IAsyncRunnerService asyncRunnerService
            )
        {
            _organizationContext = organizationContext;
            _asyncRunnerService = asyncRunnerService;
            _departmentActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.Department);
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
                ImageFileId = data.ImageFileId,
                IsDeleted = false,
                NumberOfPerson = data.NumberOfPerson,
                IsFactory = data.IsFactory
            };

            await _organizationContext.Department.AddAsync(department);
            await _organizationContext.SaveChangesAsync();

            if (data.ImageFileId.HasValue)
            {
                _asyncRunnerService.RunAsync<IPhysicalFileService>(s => s.FileAssignToObject(data.ImageFileId.Value, EnumObjectType.Department, department.DepartmentId));
            }

            await _departmentActivityLog.LogBuilder(() => DepartmentActivityLogMessage.Create)
                 .MessageResourceFormatDatas(department.DepartmentCode)
                 .ObjectId(department.DepartmentId)
                 .JsonData(data.JsonSerialize())
                 .CreateLog();

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
            //var isInUsed = new SqlParameter("@IsUsed", SqlDbType.Bit) { Direction = ParameterDirection.Output };
            //var checkParams = new[]
            //{
            //        new SqlParameter("@DepartmentId",departmentId),
            //        new SqlParameter("@TypeCheck",EnumTypeDepartmentCheckUsed.All),
            //        isInUsed
            //    };

            //await _organizationContext.ExecuteStoreProcedure("asp_Department_CheckUsed", checkParams);

            //if (isInUsed.Value as bool? == true)
            //{
            //    throw new BadRequestException("Bộ phận đã được sử dụng, không được phép xóa");
            //}

            var departmentTopUsed = await GetDepartmentTopInUsed(new[] { departmentId }, true);
            if (departmentTopUsed.Count > 0)
            {
                throw GeneralCode.ItemInUsed.BadRequestFormatWithData(departmentTopUsed, DepartmentErrorCode.DepartmentInUsed.GetEnumDescription(), $"{department.DepartmentCode} {departmentTopUsed.First().Description}");
            }

            department.IsDeleted = true;
            await _organizationContext.SaveChangesAsync();

            if (department.ImageFileId.HasValue)
            {
                _asyncRunnerService.RunAsync<IPhysicalFileService>(s => s.DeleteFile(department.ImageFileId.Value));
            }


            await _departmentActivityLog.LogBuilder(() => DepartmentActivityLogMessage.Delete)
                 .MessageResourceFormatDatas(department.DepartmentCode)
                 .ObjectId(department.DepartmentId)
                 .JsonData(department.JsonSerialize())
                 .CreateLog();

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
                ImageFileId = department.ImageFileId,
                NumberOfPerson = department.NumberOfPerson,
                IsFactory = department.IsFactory
            };
        }

        public async Task<PageData<DepartmentModel>> GetList(string keyword, IList<int> departmentIds, bool? isProduction, bool? isActived, int page, int size, Clause filters = null)
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
            query = query.InternalFilter(filters);
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
                ImageFileId = d.ImageFileId,
                NumberOfPerson = d.NumberOfPerson,
                IsFactory = d.IsFactory
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
                ImageFileId = d.ImageFileId,
                NumberOfPerson = d.NumberOfPerson,
                IsFactory = d.IsFactory
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
            //Kiểm tra nếu bỏ tích BPSX
            if (department.IsProduction && !data.IsProduction)
            {
                var isInUsed = new SqlParameter("@IsUsed", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                var checkParams = new[]
                {
                    new SqlParameter("@DepartmentId",departmentId),
                    new SqlParameter("@TypeCheck",EnumTypeDepartmentCheckUsed.AssignmentAndStep),
                    isInUsed
                };

                await _organizationContext.ExecuteStoreProcedure("asp_Department_CheckUsed", checkParams);
                // Check đã được phân công chưa
                if (isInUsed.Value as bool? == true)
                {
                    throw new BadRequestException("Bộ phận đã được sử dụng là bộ phận sản xuất, không được phép bỏ thiết lập là bộ phận sản xuất");
                }
            }
            //Kiểm tra nếu bỏ tích nhà máy
            if (department.IsFactory && !data.IsFactory)
            {
                var isInUsed = new SqlParameter("@IsUsed", SqlDbType.Bit) { Direction = ParameterDirection.Output };
                var checkParams = new[]
                {
                    new SqlParameter("@DepartmentId",departmentId),
                    new SqlParameter("@TypeCheck",EnumTypeDepartmentCheckUsed.ProductionOrder),
                    isInUsed
                };

                await _organizationContext.ExecuteStoreProcedure("asp_Department_CheckUsed", checkParams);
                // Check đã được thiết lập trong LSX chưa
                if (isInUsed.Value as bool? == true)
                {
                    throw new BadRequestException("Bộ phận đã được sử dụng là nhà máy sản xuất, không được phép bỏ thiết lập nhà máy");
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
            department.ImageFileId = data.ImageFileId;
            department.NumberOfPerson = data.NumberOfPerson;
            department.IsFactory = data.IsFactory;

            await _organizationContext.SaveChangesAsync();

            if (deleteImageFileId.HasValue)
            {
                _asyncRunnerService.RunAsync<IPhysicalFileService>(s => s.DeleteFile(deleteImageFileId.Value));
            }

            if (data.ImageFileId.HasValue)
            {
                _asyncRunnerService.RunAsync<IPhysicalFileService>(s => s.FileAssignToObject(data.ImageFileId.Value, EnumObjectType.Department, department.DepartmentId));
            }


            await _departmentActivityLog.LogBuilder(() => DepartmentActivityLogMessage.Update)
                 .MessageResourceFormatDatas(department.DepartmentCode)
                 .ObjectId(department.DepartmentId)
                 .JsonData(data.JsonSerialize())
                 .CreateLog();
            return true;
        }

        public async Task<IList<ObjectBillInUsedInfo>> GetDepartmentTopInUsed(IList<int> departmentIds, bool isCheckExistOnly)
        {
            var checkParams = new[]
            {
                departmentIds.Select(d=>(long)d).ToList().ToSqlParameter("@DepartmentIds"),
                new SqlParameter("@IsCheckExistOnly", SqlDbType.Bit){ Value  = isCheckExistOnly }
            };
            return await _organizationContext.QueryListProc<ObjectBillInUsedInfo>("asp_Department_GetTopUsed_ByList", checkParams);
        }
    }
}
