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
using Verp.Services.ReportConfig.Model;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.Report;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.ReportConfigDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;

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
        private readonly IRoleHelperService _roleHelperService;
        public ReportConfigService(ReportConfigDBContext reportConfigContext
            , IOptions<AppSetting> appSetting
            , ILogger<ReportConfigService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            , IMenuHelperService menuHelperService
            , IDataProtectionProvider protectionProvider
            , IRoleHelperService roleHelperService
            )
        {
            _reportConfigContext = reportConfigContext;
            _activityLogService = activityLogService;
            _mapper = mapper;
            _logger = logger;
            _menuHelperService = menuHelperService;
            _protectionProvider = protectionProvider;
            _appSetting = appSetting.Value;
            _roleHelperService = roleHelperService;
        }

        public async Task<ReportTypeViewModel> ReportTypeViewGetInfo(int reportTypeId, bool isConfig = false)
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

        public CipherFilterModel DecryptExtraFilter(CipherFilterModel cipherFilter)
        {
            var protector = _protectionProvider.CreateProtector(_appSetting.ExtraFilterEncryptPepper);
            cipherFilter.Content = protector.Unprotect(cipherFilter.CipherContent);
            return cipherFilter;
        }

        public async Task<bool> ReportTypeViewUpdate(int reportTypeId, ReportTypeViewModel model)
        {
            var reportTypeInfo = await _reportConfigContext.ReportType.FirstOrDefaultAsync(t => t.ReportTypeId == reportTypeId);
            if (reportTypeInfo == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy loại báo cáo");

            var info = await _reportConfigContext.ReportTypeView.FirstOrDefaultAsync(v => v.ReportTypeId == reportTypeId);

            if (info == null)
            {
                await ReportTypeViewCreate(reportTypeId, model);
            }
            else
            {
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

                }
                catch (Exception ex)
                {
                    await trans.TryRollbackTransactionAsync();
                    _logger.LogError(ex, "ReportTypeViewUpdate");
                    throw;
                }

            }


            await CloneAccountancyReportViewToPublic(reportTypeId, model);

            return true;
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
            //var protector = _protectionProvider.CreateProtector(_appSetting.ExtraFilterEncryptPepper);

            foreach (var f in fields)
            {
                f.ReportTypeViewId = ReportTypeViewId;
                //if (!string.IsNullOrEmpty(f.ExtraFilter)) f.ExtraFilter = protector.Protect(f.ExtraFilter);
            }

            await _reportConfigContext.ReportTypeViewField.AddRangeAsync(fields);

        }


        public async Task<PageData<ReportTypeListModel>> ReportTypes(string keyword, int page, int size, int? moduleTypeId = null)
        {
            keyword = (keyword ?? "").Trim();

            var query = _reportConfigContext.ReportType.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(r => r.ReportPath.Contains(keyword) || r.ReportTypeName.Contains(keyword));
            }
            if (moduleTypeId.HasValue)
            {
                query = query.Where(r => r.ReportTypeGroup.ModuleTypeId == moduleTypeId.Value);
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


        public async Task<ReportTypeModel> Info(int reportTypeId)
        {
            var reportType = await _reportConfigContext.ReportType.Include(x => x.ReportTypeGroup)
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
            //var existedReport = await _reportConfigContext.ReportType
            //    .FirstOrDefaultAsync(r => r.ReportTypeName == data.ReportTypeName);
            //if (existedReport != null)
            //{
            //    throw new BadRequestException(ReportErrorCode.ReportNameAlreadyExisted);
            //}
           
            if (data.Columns == null || data.Columns.Any(c => string.IsNullOrWhiteSpace(c.Alias)))
            {
                throw GeneralCode.InvalidParams.BadRequest("Phải có ít nhất một cột và các cột phải có alias");
            }

            ReportType report = _mapper.Map<ReportType>(data);

            using var trans = await _reportConfigContext.Database.BeginTransactionAsync();
            try
            {

                await _reportConfigContext.ReportType.AddAsync(report);
                await _reportConfigContext.SaveChangesAsync();
                trans.Commit();


                await _activityLogService.CreateLog(EnumObjectType.ReportType, report.ReportTypeId, $"Thêm báo cáo {report.ReportTypeName}", data.JsonSerialize());

                await _roleHelperService.GrantPermissionForAllRoles(EnumModule.ReportView, EnumObjectType.ReportType, report.ReportTypeId);

            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "Create");
                throw new BadRequestException(GeneralCode.InternalError);
            }

            data.ReportTypeId = null;
            await CloneAccountancyReportToPublic(data);

            return report.ReportTypeId;
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
            if (data.Columns == null || data.Columns.Any(c => string.IsNullOrWhiteSpace(c.Alias)))
            {
                throw GeneralCode.InvalidParams.BadRequest("Phải có ít nhất một cột và các cột phải có alias");
            }

            var existedReport = await _reportConfigContext.ReportType
              .Where(r => r.ReportTypeId != reportTypeId)
              .FirstOrDefaultAsync(r => r.ReportTypeName == data.ReportTypeName);

            if (existedReport != null)
            {
                throw new BadRequestException(ReportErrorCode.ReportNameAlreadyExisted);
            }

            if (data.UpdatedDatetimeUtc != report.UpdatedDatetimeUtc.GetUnix())
            {
                throw GeneralCode.DataIsOld.BadRequest();
            }

            using var trans = await _reportConfigContext.Database.BeginTransactionAsync();
            try
            {
                _mapper.Map(data, report);

                await _reportConfigContext.SaveChangesAsync();
                trans.Commit();
                await _activityLogService.CreateLog(EnumObjectType.ReportType, report.ReportTypeId, $"Cập nhật báo cáo {report.ReportTypeName}", data.JsonSerialize());

            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "Update");
                throw new BadRequestException(GeneralCode.InternalError);
            }

            data.ReportTypeId = reportTypeId;
            await CloneAccountancyReportToPublic(data);

            return report.ReportTypeId;
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


        private async Task<ReportTypeGroup> CloneAccountancyReportGroupToPublic(int groupId)
        {
            var groupInfo = await _reportConfigContext.ReportTypeGroup.FirstOrDefaultAsync(g => g.ReportTypeGroupId == groupId);

            var cloneGroupInfo = await _reportConfigContext.ReportTypeGroup.FirstOrDefaultAsync(g => g.ReplicatedFromReportTypeGroupId == groupId && g.ModuleTypeId == (int)EnumModuleType.AccountantPublic);
            if (cloneGroupInfo == null)
            {
                cloneGroupInfo = new ReportTypeGroup
                {
                    ReportTypeGroupName = groupInfo.ReportTypeGroupName,
                    ModuleTypeId = (int)EnumModuleType.AccountantPublic,
                    ReplicatedFromReportTypeGroupId = groupId,
                    SortOrder = groupInfo.SortOrder,
                };
                await _reportConfigContext.ReportTypeGroup.AddAsync(cloneGroupInfo);
            }
            cloneGroupInfo.ReportTypeGroupName = groupInfo.ReportTypeGroupName;
            cloneGroupInfo.SortOrder = groupInfo.SortOrder;

            await _reportConfigContext.SaveChangesAsync();
            return cloneGroupInfo;
        }

        private async Task CloneAccountancyReportViewToPublic(int originalReportTypeId, ReportTypeViewModel info)
        {
            var cloneReportEntity = await _reportConfigContext.ReportType
              .Include(r => r.ReportTypeGroup)
              .FirstOrDefaultAsync(r => r.ReportTypeGroup.ModuleTypeId == (int)EnumModuleType.AccountantPublic
              && r.ReplicatedFromReportTypeId == originalReportTypeId);
            if (cloneReportEntity == null)
            {
                return;
            }
            info.ReportTypeViewId = 0;
            foreach (var f in info.Fields)
            {
                f.ReportTypeViewFieldId = 0;
            }

            await ReportTypeViewUpdate(cloneReportEntity.ReportTypeId, info);
        }

        private async Task CloneAccountancyReportToPublic(ReportTypeModel info)
        {
            if (info.ReportModuleTypeId != EnumModuleType.Accountant) return;

            var cloneReportEntity = await _reportConfigContext.ReportType
                .Include(r => r.ReportTypeGroup)
                .FirstOrDefaultAsync(r => r.ReportTypeGroup.ModuleTypeId == (int)EnumModuleType.AccountantPublic
                && r.ReplicatedFromReportTypeId == info.ReportTypeId);

            var newReportId = cloneReportEntity?.ReportTypeId;
            info.ReplicatedFromReportTypeId = info.ReportTypeId;
            info.ReportTypeId = newReportId;
            info.ReportModuleTypeId = EnumModuleType.AccountantPublic;

            var groupInfo = await CloneAccountancyReportGroupToPublic(info.ReportTypeGroupId);

            info.ReportTypeGroupId = groupInfo.ReportTypeGroupId;

            if (info.DetailTargetId == EnumReportDetailTarget.Report)
            {
                info.DetailReportId = await GetRefTargetReportId(info.DetailReportId);
            }

            if (info.DetailTargetId == EnumReportDetailTarget.AccountancyBill)
            {
                info.DetailTargetId = EnumReportDetailTarget.AccountancyBillPublic;
            }

            if (info.Columns != null)
            {
                foreach (var c in info.Columns)
                {
                    if (c.DetailTargetId == EnumReportDetailTarget.Report)
                    {
                        c.DetailReportId = await GetRefTargetReportId(info.DetailReportId);
                    }

                    if (c.DetailTargetId == EnumReportDetailTarget.AccountancyBill)
                    {
                        c.DetailTargetId = EnumReportDetailTarget.AccountancyBillPublic;
                    }

                    if (c.HeaderTargetId == EnumReportDetailTarget.Report)
                    {
                        c.HeaderReportId = await GetRefTargetReportId(info.DetailReportId);
                    }

                    if (c.HeaderTargetId == EnumReportDetailTarget.AccountancyBill)
                    {
                        c.HeaderTargetId = EnumReportDetailTarget.AccountancyBillPublic;
                    }
                }
            }

            if (newReportId > 0)
            {
                await UpdateReportType(newReportId.Value, info);
            }
            else
            {
                await AddReportType(info);
                var viewInfo = await ReportTypeViewGetInfo(info.ReplicatedFromReportTypeId.Value, true);

                await CloneAccountancyReportViewToPublic(info.ReplicatedFromReportTypeId.Value, viewInfo);
            }
        }

        private async Task<int?> GetRefTargetReportId(int? detailReportId)
        {
            var targetReport = await _reportConfigContext.ReportType
                .Include(r => r.ReportTypeGroup)
                .FirstOrDefaultAsync(r => r.ReportTypeGroup.ModuleTypeId == (int)EnumModuleType.Accountant && r.ReportTypeId == detailReportId);
            if (targetReport != null)
            {
                var clonedTarget = await _reportConfigContext.ReportType
                    .Include(r => r.ReportTypeGroup)
                    .FirstOrDefaultAsync(r => r.ReportTypeGroup.ModuleTypeId == (int)EnumModuleType.AccountantPublic
                        && r.ReplicatedFromReportTypeId == targetReport.ReportTypeId
                    );
                if (clonedTarget != null)
                {
                    return clonedTarget.ReportTypeId;
                }
            }
            return detailReportId;
        }
    }
}
