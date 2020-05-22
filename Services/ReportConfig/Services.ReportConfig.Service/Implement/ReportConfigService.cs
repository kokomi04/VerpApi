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
using Verp.Services.ReportConfig.Model;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.ReportConfigDB;
using VErp.Infrastructure.ServiceCore.Service;

namespace Verp.Services.ReportConfig.Service.Implement
{
    public class ReportConfigService : IReportConfigService
    {
        private readonly ReportConfigDBContext _reportConfigContext;
        private readonly IActivityLogService _activityLogService;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        public ReportConfigService(ReportConfigDBContext reportConfigContext
            , IOptions<AppSetting> appSetting
            , ILogger<ReportConfigService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            )
        {
            _reportConfigContext = reportConfigContext;
            _activityLogService = activityLogService;
            _logger = logger;
        }


        public async Task<ReportTypeViewModel> ReportTypeViewGetInfo(int reportTypeId)
        {
            var info = await _reportConfigContext.ReportTypeView.AsNoTracking().Where(t => t.ReportTypeId == reportTypeId).ProjectTo<ReportTypeViewModel>(_mapper.ConfigurationProvider).FirstOrDefaultAsync();

            if (info == null)
            {
                throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy cấu hình trong hệ thống");
            }

            var fields = await _reportConfigContext.ReportTypeViewField.AsNoTracking()
                .Where(t => t.ReportTypeViewId == info.ReportTypeViewId)
                .ProjectTo<ReportTypeViewFieldModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

            info.Fields = fields;

            return info;
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
                await trans.RollbackAsync();
                _logger.LogError(ex, "ReportTypeViewUpdate");
                throw ex;
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
                await trans.RollbackAsync();
                _logger.LogError(ex, "ReportTypeViewCreate");
                throw ex;
            }

        }


        private async Task ReportTypeViewFieldAddRange(int ReportTypeViewId, IList<ReportTypeViewFieldModel> fieldModels)
        {
            var categoryFieldIds = fieldModels.Where(f => f.ReferenceCategoryFieldId.HasValue).Select(f => f.ReferenceCategoryFieldId.Value).ToList();
            categoryFieldIds.Union(fieldModels.Where(f => f.ReferenceCategoryTitleFieldId.HasValue).Select(f => f.ReferenceCategoryFieldId.Value).ToList());

            if (categoryFieldIds.Count > 0)
            {

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

            }

            var fields = fieldModels.Select(f => _mapper.Map<ReportTypeViewField>(f)).ToList();

            foreach (var f in fields)
            {
                f.ReportTypeViewId = ReportTypeViewId;
            }

            await _reportConfigContext.ReportTypeViewField.AddRangeAsync(fields);

        }
    }
}
