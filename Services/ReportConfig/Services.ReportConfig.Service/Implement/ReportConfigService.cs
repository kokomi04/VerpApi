using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verp.Cache.RedisCache;
using Verp.Services.ReportConfig.Model;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.ReportConfigDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using Microsoft.AspNetCore.DataProtection;

namespace Verp.Services.ReportConfig.Service.Implement
{
    public class ReportConfigService : IReportConfigService
    {
        private readonly ReportConfigDBContext _reportConfigContext;
        private readonly AppSetting _appSetting;
        private readonly IActivityLogService _activityLogService;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private readonly IMenuHelperService _menuHelperService;
        private readonly IDataProtectionProvider _protectionProvider;

        public ReportConfigService(ReportConfigDBContext reportConfigContext
            , IOptions<AppSetting> appSetting
            , ILogger<ReportConfigService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            , IMenuHelperService menuHelperService
            , IDataProtectionProvider protectionProvider
            )
        {
            _reportConfigContext = reportConfigContext;
            _activityLogService = activityLogService;
            _mapper = mapper;
            _logger = logger;
            _menuHelperService = menuHelperService;
            _protectionProvider = protectionProvider;
            _appSetting = appSetting.Value;
        }

        public async Task<ReportTypeViewModel> ReportTypeViewGetInfo(int reportTypeId)
        {
            var info = await _reportConfigContext.ReportTypeView.AsNoTracking().Where(t => t.ReportTypeId == reportTypeId).ProjectTo<ReportTypeViewModel>(_mapper.ConfigurationProvider).FirstOrDefaultAsync();

            if (info == null)
            {
                return new ReportTypeViewModel()
                {
                    Fields = new List<ReportTypeViewFieldModel>(),
                    IsDefault = true,
                    ReportTypeViewName = "Lọc dữ liệu"
                };
            }

            var fields = await _reportConfigContext.ReportTypeViewField.AsNoTracking()
                .Where(t => t.ReportTypeViewId == info.ReportTypeViewId)
                .OrderBy(f => f.SortOrder)
                .ProjectTo<ReportTypeViewFieldModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

            info.Fields = fields;

            return info;
        }

        public string DecryptExtraFilter(string cipherFilter)
        {
            var protector = _protectionProvider.CreateProtector(_appSetting.ExtraFilterEncryptPepper);
            return protector.Unprotect(cipherFilter);
        }

        public async Task<bool> ReportTypeViewUpdate(int reportTypeId, ReportTypeViewModel model)
        {
            var reportTypeInfo = await _reportConfigContext.ReportType.FirstOrDefaultAsync(t => t.ReportTypeId == reportTypeId);
            if (reportTypeInfo == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy loại báo cáo");

            var info = await _reportConfigContext.ReportTypeView.FirstOrDefaultAsync(v => v.ReportTypeId == reportTypeId);

            if (info == null)
            {
                return await ReportTypeViewCreate(reportTypeId, model);
            }

            using var trans = await _reportConfigContext.Database.BeginTransactionAsync();
            try
            {

                _mapper.Map(model, info);

                var oldFields = await _reportConfigContext.ReportTypeViewField.Where(f => f.ReportTypeViewId == info.ReportTypeViewId).ToListAsync();

                _reportConfigContext.ReportTypeViewField.RemoveRange(oldFields);

                await ReportTypeViewFieldAddRange(info.ReportTypeViewId, model.Fields);

                await _reportConfigContext.SaveChangesAsync();

                await trans.CommitAsync();

                await _activityLogService.CreateLog(EnumObjectType.ReportTypeView, info.ReportTypeViewId, $"Cập nhật bộ lọc {info.ReportTypeViewName} cho báo cáo  {reportTypeInfo.ReportTypeName}", model.JsonSerialize());

                return true;
            }
            catch (Exception ex)
            {
                await trans.TryRollbackTransactionAsync();
                _logger.LogError(ex, "ReportTypeViewUpdate");
                throw;
            }
        }



        #region ReportTypeGroup
        public async Task<int> ReportTypeGroupCreate(ReportTypeGroupModel model)
        {
            var info = _mapper.Map<ReportTypeGroup>(model);
            await _reportConfigContext.ReportTypeGroup.AddAsync(info);
            await _reportConfigContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.ReportTypeGroup, info.ReportTypeGroupId, $"Thêm nhóm báo cáo {info.ReportTypeGroupName}", model.JsonSerialize());

            return info.ReportTypeGroupId;
        }

        public async Task<bool> ReportTypeGroupUpdate(int reportTypeGroupId, ReportTypeGroupModel model)
        {
            var info = await _reportConfigContext.ReportTypeGroup.FirstOrDefaultAsync(g => g.ReportTypeGroupId == reportTypeGroupId);

            if (info == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Nhóm báo cáo không tồn tại");

            _mapper.Map(model, info);

            await _reportConfigContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.ReportTypeGroup, info.ReportTypeGroupId, $"Cập nhật nhóm báo cáo {info.ReportTypeGroupName}", model.JsonSerialize());

            return true;
        }

        public async Task<bool> ReportTypeGroupDelete(int reportTypeGroupId)
        {
            var info = await _reportConfigContext.ReportTypeGroup.FirstOrDefaultAsync(g => g.ReportTypeGroupId == reportTypeGroupId);

            if (info == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Nhóm báo cáo không tồn tại");

            info.IsDeleted = true;

            await _reportConfigContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.ReportTypeGroup, info.ReportTypeGroupId, $"Xóa nhóm báo cáo {info.ReportTypeGroupName}", new { reportTypeGroupId }.JsonSerialize());

            return true;
        }

        public async Task<IList<ReportTypeGroupList>> ReportTypeGroupList()
        {
            return await _reportConfigContext.ReportTypeGroup.ProjectTo<ReportTypeGroupList>(_mapper.ConfigurationProvider).OrderBy(g => g.SortOrder).ToListAsync();
        }

        #endregion
        private async Task<bool> ReportTypeViewCreate(int reportTypeId, ReportTypeViewModel model)
        {
            using var trans = await _reportConfigContext.Database.BeginTransactionAsync();
            try
            {
                var reportTypeInfo = await _reportConfigContext.ReportType.FirstOrDefaultAsync(t => t.ReportTypeId == reportTypeId);
                if (reportTypeInfo == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy loại chứng từ");

                var info = _mapper.Map<ReportTypeView>(model);

                info.ReportTypeId = reportTypeId;

                await _reportConfigContext.ReportTypeView.AddAsync(info);
                await _reportConfigContext.SaveChangesAsync();

                await ReportTypeViewFieldAddRange(info.ReportTypeViewId, model.Fields);

                await _reportConfigContext.SaveChangesAsync();

                await trans.CommitAsync();

                await _activityLogService.CreateLog(EnumObjectType.ReportTypeView, info.ReportTypeViewId, $"Tạo bộ lọc {info.ReportTypeViewName} cho báo cáo  {reportTypeInfo.ReportTypeName}", model.JsonSerialize());

                return true;
            }
            catch (Exception ex)
            {
                await trans.TryRollbackTransactionAsync();
                _logger.LogError(ex, "ReportTypeViewCreate");
                throw;
            }

        }

        private async Task ReportTypeViewFieldAddRange(int ReportTypeViewId, IList<ReportTypeViewFieldModel> fieldModels)
        {
            //var categoryFieldIds = fieldModels.Where(f => f.ReferenceCategoryFieldId.HasValue).Select(f => f.ReferenceCategoryFieldId.Value).ToList();
            //categoryFieldIds.Union(fieldModels.Where(f => f.ReferenceCategoryTitleFieldId.HasValue).Select(f => f.ReferenceCategoryFieldId.Value).ToList());

            //if (categoryFieldIds.Count > 0)
            //{

            //var categoryFields = (await _reportConfigContext.CategoryField
            //    .Where(f => categoryFieldIds.Contains(f.CategoryFieldId))
            //    .Select(f => new { f.CategoryFieldId, f.CategoryId })
            //    .AsNoTracking()
            //    .ToListAsync())
            //    .ToDictionary(f => f.CategoryFieldId, f => f);

            //foreach (var f in fieldModels)
            //{
            //    if (f.ReferenceCategoryFieldId.HasValue && categoryFields.TryGetValue(f.ReferenceCategoryFieldId.Value, out var cateField) && cateField.CategoryId != f.ReferenceCategoryId)
            //    {
            //        throw new BadRequestException(GeneralCode.InvalidParams, "Trường dữ liệu của danh mục không thuộc danh mục");
            //    }

            //    if (f.ReferenceCategoryTitleFieldId.HasValue && categoryFields.TryGetValue(f.ReferenceCategoryTitleFieldId.Value, out cateField) && cateField.CategoryId != f.ReferenceCategoryId)
            //    {
            //        throw new BadRequestException(GeneralCode.InvalidParams, "Trường hiển thị của danh mục không thuộc danh mục");
            //    }
            //}

            // }

            var fields = fieldModels.Select(f => _mapper.Map<ReportTypeViewField>(f)).ToList();
            var protector = _protectionProvider.CreateProtector(_appSetting.ExtraFilterEncryptPepper);

            foreach (var f in fields)
            {
                f.ReportTypeViewId = ReportTypeViewId;
                if (!string.IsNullOrEmpty(f.ExtraFilter)) f.ExtraFilter = protector.Protect(f.ExtraFilter);
            }

            await _reportConfigContext.ReportTypeViewField.AddRangeAsync(fields);

        }


        public async Task<PageData<ReportTypeListModel>> ReportTypes(string keyword, int page, int size, int? reportTypeGroupId = null)
        {
            keyword = (keyword ?? "").Trim();

            var query = _reportConfigContext.ReportType.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(r => r.ReportPath.Contains(keyword) || r.ReportTypeName.Contains(keyword));
            }
            if (reportTypeGroupId.HasValue)
            {
                query = query.Where(r => r.ReportTypeGroupId == reportTypeGroupId.Value);
            }
            query = query.OrderBy(r => r.ReportTypeName);
            var total = await query.CountAsync();
            if (size > 0)
            {
                query = query.Skip((page - 1) * size).Take(size);
            }
            List<ReportTypeListModel> lst = query.ProjectTo<ReportTypeListModel>(_mapper.ConfigurationProvider).ToList();

            return (lst, total);
        }


        public async Task<ReportTypeModel> ReportType(int reportTypeId)
        {
            var reportType = await _reportConfigContext.ReportType
                //.ProjectTo<ReportTypeModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(r => r.ReportTypeId == reportTypeId);
            if (reportType == null)
            {
                throw new BadRequestException(ReportErrorCode.ReportNotFound);
            }
            var info = _mapper.Map<ReportTypeModel>(reportType);
            if (info.BscConfig?.Rows != null)
            {
                foreach (var row in info.BscConfig.Rows)
                {
                    if (row.RowData == null)
                    {
                        row.RowData = row.Value?.ToNonCamelCaseDictionaryData(v => v.Key, v => new BscCellModel() { Value = v.Value, Style = new NonCamelCaseDictionary() });
                    }
                }
            }
            return info;
        }

        public async Task<int> AddReportType(ReportTypeModel data)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockReportKey(0));
            var existedReport = await _reportConfigContext.ReportType
                .FirstOrDefaultAsync(r => r.ReportTypeName == data.ReportTypeName);
            if (existedReport != null)
            {
                throw new BadRequestException(ReportErrorCode.ReportNameAlreadyExisted);
            }

            using var trans = await _reportConfigContext.Database.BeginTransactionAsync();
            try
            {
                ReportType report = _mapper.Map<ReportType>(data);
                await _reportConfigContext.ReportType.AddAsync(report);
                await _reportConfigContext.SaveChangesAsync();
                trans.Commit();

                if (data.MenuStyle != null)
                {
                    var url = Utils.FormatStyle(data.MenuStyle.UrlFormat, string.Empty, report.ReportTypeId);
                    var param = Utils.FormatStyle(data.MenuStyle.ParamFormat, string.Empty, report.ReportTypeId);
                    await _menuHelperService.CreateMenu(data.MenuStyle.ParentId, false, data.MenuStyle.ModuleId, data.MenuStyle.MenuName, url, param, data.MenuStyle.Icon, data.MenuStyle.SortOrder, data.MenuStyle.IsDisabled);
                }

                await _activityLogService.CreateLog(EnumObjectType.ReportType, report.ReportTypeId, $"Thêm báo cáo {report.ReportTypeName}", data.JsonSerialize());
                return report.ReportTypeId;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "Create");
                throw new BadRequestException(GeneralCode.InternalError);
            }
        }


        public async Task<int> UpdateReportType(int reportTypeId, ReportTypeModel data)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockReportKey(reportTypeId));
            var report = await _reportConfigContext.ReportType
                .Where(r => r.ReportTypeId == reportTypeId)
                .FirstOrDefaultAsync();
            if (report == null)
            {
                throw new BadRequestException(ReportErrorCode.ReportNotFound);
            }

            var existedReport = await _reportConfigContext.ReportType
              .Where(r => r.ReportTypeId != reportTypeId)
              .FirstOrDefaultAsync(r => r.ReportTypeName == data.ReportTypeName);

            if (existedReport != null)
            {
                throw new BadRequestException(ReportErrorCode.ReportNameAlreadyExisted);
            }

            using var trans = await _reportConfigContext.Database.BeginTransactionAsync();
            try
            {
                _mapper.Map(data, report);

                await _reportConfigContext.SaveChangesAsync();
                trans.Commit();
                await _activityLogService.CreateLog(EnumObjectType.ReportType, report.ReportTypeId, $"Cập nhật báo cáo {report.ReportTypeName}", data.JsonSerialize());
                return report.ReportTypeId;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "Update");
                throw new BadRequestException(GeneralCode.InternalError);
            }
        }
        public async Task<int> DeleteReportType(int reportTypeId)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockReportKey(reportTypeId));
            var report = await _reportConfigContext.ReportType
                .Where(r => r.ReportTypeId == reportTypeId)
                .FirstOrDefaultAsync();
            if (report == null)
            {
                throw new BadRequestException(ReportErrorCode.ReportNotFound);
            }

            using var trans = await _reportConfigContext.Database.BeginTransactionAsync();
            try
            {
                report.IsDeleted = true;
                // Xóa View
                var reportTypeViews = _reportConfigContext.ReportTypeView.Where(v => v.ReportTypeId == reportTypeId).ToList();
                foreach (var reportTypeView in reportTypeViews)
                {
                    reportTypeView.IsDeleted = true;
                }
                // Xóa view field
                var viewIds = reportTypeViews.Select(v => v.ReportTypeViewId).ToList();
                var reportTypeViewFields = _reportConfigContext.ReportTypeView.Where(f => viewIds.Contains(f.ReportTypeViewId)).ToList();
                foreach (var reportTypeViewField in reportTypeViewFields)
                {
                    reportTypeViewField.IsDeleted = true;
                }
                await _reportConfigContext.SaveChangesAsync();
                trans.Commit();
                await _activityLogService.CreateLog(EnumObjectType.ReportType, report.ReportTypeId, $"Xóa báo cáo {report.ReportTypeName}", report.JsonSerialize());
                return report.ReportTypeId;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "Delete");
                throw new BadRequestException(GeneralCode.InternalError);
            }
        }
    }
}
