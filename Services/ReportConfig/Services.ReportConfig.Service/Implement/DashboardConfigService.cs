using AutoMapper;
using AutoMapper.QueryableExtensions;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Verp.Cache.RedisCache;
using Verp.Resources.Master.Config.ActionButton;
using Verp.Resources.Report.DashboardConfig;
using Verp.Services.ReportConfig.Model;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.ReportConfigDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;

namespace Verp.Services.ReportConfig.Service.Implement
{
    public interface IDashboardConfigService
    {
        Task<int> AddDashboardType(DashboardTypeModel data);
        Task<int> AddDashboardTypeGroup(DashboardTypeGroupModel model);
        Task<IList<DashboardTypeGroupModel>> DashboardTypeGroupList();
        Task<IList<DashboardTypeModel>> DashboardTypeList();
        Task<PageData<DashboardTypeListModel>> DashboardTypes(string keyword, int page, int size, int? moduleTypeId = null);
        Task<DashboardTypeViewModel> DashboardTypeViewGetInfo(int dashboardTypeId, bool isConfig = false);
        Task<bool> DashboardTypeViewUpdate(int dashboardTypeId, DashboardTypeViewModel model);
        Task<bool> DeleteDashboardType(int dashboardTypeId);
        Task<bool> DeleteDashboardTypeGroup(int dashboardTypeGroupId);
        Task<DashboardTypeModel> GetDashboardType(int dashboardTypeId);
        Task<DashboardTypeGroupModel> GetDashboardTypeGroup(int dashboardTypeGroupId);
        Task<bool> UpdateDashboardType(int dashboardTypeId, DashboardTypeModel data);
        Task<bool> UpdateDashboardTypeGroup(int dashboardTypeGroupId, DashboardTypeGroupModel model);
    }

    public class DashboardConfigService : IDashboardConfigService
    {
        private readonly ReportConfigDBContext _reportConfigContext;
        private readonly ObjectActivityLogFacade _objActivityLogFacadeDashTypeView;
        private readonly ObjectActivityLogFacade _objActivityLogFacadeDashTypeGroup;
        private readonly ObjectActivityLogFacade _objActivityLogFacadeDashType;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private readonly IRoleHelperService _roleHelperService;
        private readonly IDataProtectionProvider _protectionProvider;
        private readonly AppSetting _appSetting;

        public DashboardConfigService(ReportConfigDBContext reportConfigContext
            , ILogger<ReportConfigService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            , IRoleHelperService roleHelperService
            , IOptions<AppSetting> appSetting
            , IDataProtectionProvider protectionProvider)
        {
            _reportConfigContext = reportConfigContext;
            _objActivityLogFacadeDashTypeView = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.DashboardTypeView);
            _objActivityLogFacadeDashType = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.DashboardType);
            _objActivityLogFacadeDashTypeGroup = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.DashboardTypeGroup);
            _mapper = mapper;
            _logger = logger;
            _roleHelperService = roleHelperService;
            _protectionProvider = protectionProvider;
            _appSetting = appSetting.Value;
        }

        #region DashboardTypeView
        public async Task<DashboardTypeViewModel> DashboardTypeViewGetInfo(int dashboardTypeId, bool isConfig = false)
        {
            var info = await _reportConfigContext.DashboardTypeView.AsNoTracking().Where(t => t.DashboardTypeId == dashboardTypeId).ProjectTo<DashboardTypeViewModel>(_mapper.ConfigurationProvider).FirstOrDefaultAsync();

            if (info == null)
            {
                return new DashboardTypeViewModel()
                {
                    Fields = new List<DashboardTypeViewFieldModel>(),
                    IsDefault = true,
                    DashboardTypeViewName = "Lọc dữ liệu"
                };
            }

            var fields = await _reportConfigContext.DashboardTypeViewField.AsNoTracking()
                .Where(t => t.DashboardTypeViewId == info.DashboardTypeViewId)
                .OrderBy(f => f.SortOrder)
                .ProjectTo<DashboardTypeViewFieldModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

            if (!isConfig)
            {
                var protector = _protectionProvider.CreateProtector(_appSetting.ExtraFilterEncryptPepper);

                foreach (var field in fields)
                {
                    if (!string.IsNullOrEmpty(field.ExtraFilter))
                    {
                        field.ExtraFilter = protector.Protect(field.ExtraFilter);
                    }
                }
            }

            info.Fields = fields.OrderBy(f => f.SortOrder).ToList();

            return info;
        }

        public async Task<bool> DashboardTypeViewUpdate(int dashboardTypeId, DashboardTypeViewModel model)
        {
            var dashboardTypeInfo = await _reportConfigContext.DashboardType.FirstOrDefaultAsync(t => t.DashboardTypeId == dashboardTypeId);
            if (dashboardTypeInfo == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy loại biểu đồ báo cáo");

            var info = await _reportConfigContext.DashboardTypeView.FirstOrDefaultAsync(v => v.DashboardTypeId == dashboardTypeId);

            if (info == null)
            {
                return await DashboardTypeViewCreate(dashboardTypeId, model);
            }

            using var trans = await _reportConfigContext.Database.BeginTransactionAsync();
            try
            {

                _mapper.Map(model, info);

                var oldFields = await _reportConfigContext.DashboardTypeViewField.Where(f => f.DashboardTypeViewId == info.DashboardTypeViewId).ToListAsync();

                _reportConfigContext.DashboardTypeViewField.RemoveRange(oldFields);

                await DashboardTypeViewFieldAddRange(info.DashboardTypeViewId, model.Fields);

                await _reportConfigContext.SaveChangesAsync();

                await trans.CommitAsync();

                await _objActivityLogFacadeDashTypeView.LogBuilder(() => DashboardConfigActivityLogMessage.UpdateDashBoardFilter)
                   .MessageResourceFormatDatas(info.DashboardTypeViewName,dashboardTypeInfo.DashboardTypeName)
                   .ObjectId(info.DashboardTypeViewId)
                   .JsonData(model)
                   .CreateLog();

                return true;
            }
            catch (Exception ex)
            {
                await trans.TryRollbackTransactionAsync();
                _logger.LogError(ex, "DashboardTypeViewUpdate");
                throw;
            }
        }

        private async Task<bool> DashboardTypeViewCreate(int dashboardTypeId, DashboardTypeViewModel model)
        {
            using var trans = await _reportConfigContext.Database.BeginTransactionAsync();
            try
            {
                var dashboardTypeInfo = await _reportConfigContext.DashboardType.FirstOrDefaultAsync(t => t.DashboardTypeId == dashboardTypeId);
                if (dashboardTypeInfo == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy loại biểu đồ báo cáo");

                var info = _mapper.Map<DashboardTypeView>(model);

                info.DashboardTypeId = dashboardTypeId;

                await _reportConfigContext.DashboardTypeView.AddAsync(info);
                await _reportConfigContext.SaveChangesAsync();

                await DashboardTypeViewFieldAddRange(info.DashboardTypeViewId, model.Fields);

                await _reportConfigContext.SaveChangesAsync();

                await trans.CommitAsync();

                await _objActivityLogFacadeDashTypeView.LogBuilder(() => DashboardConfigActivityLogMessage.CreateDashBoardFilter)
                   .MessageResourceFormatDatas(info.DashboardTypeViewName,dashboardTypeInfo.DashboardTypeName)
                   .ObjectId(info.DashboardTypeViewId)
                   .JsonData(model)
                   .CreateLog();

                return true;
            }
            catch (Exception ex)
            {
                await trans.TryRollbackTransactionAsync();
                _logger.LogError(ex, "DashboardTypeViewCreate");
                throw;
            }

        }

        private async Task DashboardTypeViewFieldAddRange(int DashboardTypeViewId, IList<DashboardTypeViewFieldModel> fieldModels)
        {
            var fields = fieldModels.Select(f => _mapper.Map<DashboardTypeViewField>(f)).ToList();

            foreach (var f in fields)
            {
                f.DashboardTypeViewId = DashboardTypeViewId;
            }

            await _reportConfigContext.DashboardTypeViewField.AddRangeAsync(fields);

        }
        #endregion

        #region DashboardTypeGroup
        public async Task<int> AddDashboardTypeGroup(DashboardTypeGroupModel model)
        {
            var info = _mapper.Map<DashboardTypeGroup>(model);
            await _reportConfigContext.DashboardTypeGroup.AddAsync(info);
            await _reportConfigContext.SaveChangesAsync();

            await _objActivityLogFacadeDashTypeGroup.LogBuilder(() => DashboardConfigActivityLogMessage.CreateDashBoardGroup)
                   .MessageResourceFormatDatas(info.DashboardTypeGroupName)
                   .ObjectId(info.DashboardTypeGroupId)
                   .JsonData(model)
                   .CreateLog();

            return info.DashboardTypeGroupId;
        }

        public async Task<bool> UpdateDashboardTypeGroup(int dashboardTypeGroupId, DashboardTypeGroupModel model)
        {
            var info = await _reportConfigContext.DashboardTypeGroup.FirstOrDefaultAsync(g => g.DashboardTypeGroupId == dashboardTypeGroupId);

            if (info == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Nhóm biểu đồ báo cáo không tồn tại");

            _mapper.Map(model, info);

            await _reportConfigContext.SaveChangesAsync();

            await _objActivityLogFacadeDashTypeGroup.LogBuilder(() => DashboardConfigActivityLogMessage.UpdateDashBoardGroup)
                   .MessageResourceFormatDatas(info.DashboardTypeGroupName)
                   .ObjectId(info.DashboardTypeGroupId)
                   .JsonData(model)
                   .CreateLog();

            return true;
        }

        public async Task<bool> DeleteDashboardTypeGroup(int dashboardTypeGroupId)
        {
            var info = await _reportConfigContext.DashboardTypeGroup.FirstOrDefaultAsync(g => g.DashboardTypeGroupId == dashboardTypeGroupId);

            if (info == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Nhóm biểu đồ báo cáo không tồn tại");

            if (_reportConfigContext.DashboardType.Any(x => x.DashboardTypeGroupId == dashboardTypeGroupId))
                throw new BadRequestException(GeneralCode.InvalidParams, "Tồn tại các biểu đồ báo trong trong nhóm. Hãy đảm bảo nhóm biểu đồ báo cáo không có phân tử bên trong trước khi xóa.");

            info.IsDeleted = true;

            await _reportConfigContext.SaveChangesAsync();

            await _objActivityLogFacadeDashTypeGroup.LogBuilder(() => DashboardConfigActivityLogMessage.DeleteDashBoardGroup)
                   .MessageResourceFormatDatas(info.DashboardTypeGroupName)
                   .ObjectId(info.DashboardTypeGroupId)
                   .JsonData(new { dashboardTypeGroupId })
                   .CreateLog();

            return true;
        }

        public async Task<IList<DashboardTypeGroupModel>> DashboardTypeGroupList()
        {
            return await _reportConfigContext.DashboardTypeGroup.ProjectTo<DashboardTypeGroupModel>(_mapper.ConfigurationProvider).OrderBy(g => g.SortOrder).ToListAsync();
        }

        public async Task<DashboardTypeGroupModel> GetDashboardTypeGroup(int dashboardTypeGroupId)
        {
            var reportType = await _reportConfigContext.DashboardTypeGroup
                .FirstOrDefaultAsync(r => r.DashboardTypeGroupId == dashboardTypeGroupId);

            if (reportType == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);

            var info = _mapper.Map<DashboardTypeGroupModel>(reportType);
            return info;
        }
        #endregion

        #region DashboardType
        public async Task<PageData<DashboardTypeListModel>> DashboardTypes(string keyword, int page, int size, int? moduleTypeId = null)
        {
            keyword = (keyword ?? "").Trim();

            var query = _reportConfigContext.DashboardType.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(r => r.DashboardTypeName.Contains(keyword));
            }
            if (moduleTypeId.HasValue)
            {
                query = query.Where(r => r.DashboardTypeGroup.ModuleTypeId == moduleTypeId.Value);
            }
            query = query.OrderBy(r => r.DashboardTypeName);
            var total = await query.CountAsync();
            if (size > 0)
            {
                query = query.Skip((page - 1) * size).Take(size);
            }
            List<DashboardTypeListModel> lst = query.ProjectTo<DashboardTypeListModel>(_mapper.ConfigurationProvider).ToList();

            return (lst, total);
        }

        public async Task<int> AddDashboardType(DashboardTypeModel data)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockReportKey(0));
            var existedReport = await _reportConfigContext.DashboardType
                .FirstOrDefaultAsync(r => r.DashboardTypeName == data.DashboardTypeName);
            if (existedReport != null)
            {
                throw new BadRequestException(ReportErrorCode.ReportNameAlreadyExisted);
            }

            using var trans = await _reportConfigContext.Database.BeginTransactionAsync();
            try
            {
                DashboardType dashboard = _mapper.Map<DashboardType>(data);
                await _reportConfigContext.DashboardType.AddAsync(dashboard);
                await _reportConfigContext.SaveChangesAsync();
                trans.Commit();


                await _objActivityLogFacadeDashType.LogBuilder(() => DashboardConfigActivityLogMessage.CreateDashBoard)
                   .MessageResourceFormatDatas(dashboard.DashboardTypeName)
                   .ObjectId(dashboard.DashboardTypeId)
                   .JsonData(data)
                   .CreateLog();

                await _roleHelperService.GrantPermissionForAllRoles(EnumModule.DashboardView, EnumObjectType.DashboardType, dashboard.DashboardTypeId);

                return dashboard.DashboardTypeId;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "Create");
                throw new BadRequestException(GeneralCode.InternalError);
            }
        }

        public async Task<bool> UpdateDashboardType(int dashboardTypeId, DashboardTypeModel data)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockReportKey(dashboardTypeId));
            var dashboard = await _reportConfigContext.DashboardType
                .Where(r => r.DashboardTypeId == dashboardTypeId)
                .FirstOrDefaultAsync();
            if (dashboard == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);

            var existedReport = await _reportConfigContext.DashboardType
              .Where(r => r.DashboardTypeId != dashboardTypeId)
              .FirstOrDefaultAsync(r => r.DashboardTypeName == data.DashboardTypeName);

            if (existedReport != null)
            {
                throw new BadRequestException(ReportErrorCode.ReportNameAlreadyExisted);
            }

            using var trans = await _reportConfigContext.Database.BeginTransactionAsync();
            try
            {
                _mapper.Map(data, dashboard);

                await _reportConfigContext.SaveChangesAsync();
                trans.Commit();

                await _objActivityLogFacadeDashType.LogBuilder(() => DashboardConfigActivityLogMessage.UpdateDashBoard)
                   .MessageResourceFormatDatas(dashboard.DashboardTypeName)
                   .ObjectId(dashboard.DashboardTypeId)
                   .JsonData(data)
                   .CreateLog();

                return true;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "Update");
                throw new BadRequestException(GeneralCode.InternalError);
            }
        }

        public async Task<bool> DeleteDashboardType(int dashboardTypeId)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockReportKey(dashboardTypeId));
            var dashboard = await _reportConfigContext.DashboardType
                .Where(r => r.DashboardTypeId == dashboardTypeId)
                .FirstOrDefaultAsync();
            if (dashboard == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);

            using var trans = await _reportConfigContext.Database.BeginTransactionAsync();
            try
            {
                dashboard.IsDeleted = true;

                await _reportConfigContext.SaveChangesAsync();
                trans.Commit();

                await _objActivityLogFacadeDashType.LogBuilder(() => DashboardConfigActivityLogMessage.DeleteDashBoard)
                   .MessageResourceFormatDatas(dashboard.DashboardTypeName)
                   .ObjectId(dashboard.DashboardTypeId)
                   .JsonData(dashboard)
                   .CreateLog();

                return true;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "Delete");
                throw new BadRequestException(GeneralCode.InternalError);
            }
        }

        public async Task<IList<DashboardTypeModel>> DashboardTypeList()
        {
            return await _reportConfigContext.DashboardType.ProjectTo<DashboardTypeModel>(_mapper.ConfigurationProvider).ToListAsync();
        }

        public async Task<DashboardTypeModel> GetDashboardType(int dashboardTypeId)
        {
            var reportType = await _reportConfigContext.DashboardType
                .FirstOrDefaultAsync(r => r.DashboardTypeId == dashboardTypeId);

            if (reportType == null)
                throw new BadRequestException(GeneralCode.ItemNotFound);

            var info = _mapper.Map<DashboardTypeModel>(reportType);
            return info;
        }
        #endregion
    }
}