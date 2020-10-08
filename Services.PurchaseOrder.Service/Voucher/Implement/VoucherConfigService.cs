using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Verp.Cache.RedisCache;
using VErp.Commons.Constants;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.PurchaseOrderDB;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.PurchaseOrder.Model.Voucher;

namespace VErp.Services.PurchaseOrder.Service.Voucher.Implement
{
    public class VoucherConfigService : IVoucherConfigService
    {
        private const string VOUCHERVALUEROW_TABLE = PurchaseOrderConstants.VOUCHERVALUEROW_TABLE;

        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly IMapper _mapper;
        private readonly PurchaseOrderDBContext _purchaseOrderDBContext;
        private readonly ICustomGenCodeHelperService _customGenCodeHelperService;
        private readonly IMenuHelperService _menuHelperService;
        private readonly ICurrentContextService _currentContextService;

        public VoucherConfigService(PurchaseOrderDBContext purchaseOrderDBContext
            , IOptions<AppSetting> appSetting
            , ILogger<VoucherConfigService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            , ICustomGenCodeHelperService customGenCodeHelperService
            , IMenuHelperService menuHelperService
            , ICurrentContextService currentContextService
            )
        {
            _purchaseOrderDBContext = purchaseOrderDBContext;
            _logger = logger;
            _activityLogService = activityLogService;
            _mapper = mapper;
            _customGenCodeHelperService = customGenCodeHelperService;
            _menuHelperService = menuHelperService;
            _currentContextService = currentContextService;
        }

        #region InputType

        public async Task<VoucherTypeFullModel> GetVoucherType(int voucherTypeId)
        {
            var inputType = await _purchaseOrderDBContext.VoucherType
           .Where(i => i.VoucherTypeId == voucherTypeId)
           .Include(t => t.VoucherArea)
           .ThenInclude(a => a.VoucherAreaField)
           .ThenInclude(af => af.VoucherField)
           .Include(t => t.VoucherArea)
           .ThenInclude(a => a.VoucherAreaField)
           .ThenInclude(af => af.VoucherField)
           .ProjectTo<VoucherTypeFullModel>(_mapper.ConfigurationProvider)
           .FirstOrDefaultAsync();
            if (inputType == null)
            {
                throw new BadRequestException(VoucherErrorCode.VoucherTypeNotFound);
            }
            return inputType;
        }

        public async Task<VoucherTypeFullModel> GetVoucherType(string voucherTypeCode)
        {
            var inputType = await _purchaseOrderDBContext.VoucherType
           .Where(i => i.VoucherTypeCode == voucherTypeCode)
           .Include(t => t.VoucherArea)
           .ThenInclude(a => a.VoucherAreaField)
           .ThenInclude(af => af.VoucherField)
           .Include(t => t.VoucherArea)
           .ThenInclude(a => a.VoucherAreaField)
           .ThenInclude(af => af.VoucherField)
           .ProjectTo<VoucherTypeFullModel>(_mapper.ConfigurationProvider)
           .FirstOrDefaultAsync();
            if (inputType == null)
            {
                throw new BadRequestException(VoucherErrorCode.VoucherTypeNotFound);
            }
            return inputType;
        }

        public async Task<PageData<VoucherTypeModel>> GetVoucherTypes(string keyword, int page, int size)
        {
            keyword = (keyword ?? "").Trim();

            var query = _purchaseOrderDBContext.VoucherType.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(i => i.VoucherTypeCode.Contains(keyword) || i.Title.Contains(keyword));
            }

            query = query.OrderBy(c => c.Title);
            var total = await query.CountAsync();
            if (size > 0)
            {
                query = query.Skip((page - 1) * size).Take(size);
            }
            var lst = await query.ProjectTo<VoucherTypeModel>(_mapper.ConfigurationProvider).OrderBy(t => t.SortOrder).ToListAsync();
            return (lst, total);
        }

        public async Task<int> AddVoucherType(VoucherTypeModel data)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockVoucherTypeKey(0));
            var existedInput = await _purchaseOrderDBContext.VoucherType
                .FirstOrDefaultAsync(i => i.VoucherTypeCode == data.VoucherTypeCode || i.Title == data.Title);
            if (existedInput != null)
            {
                if (string.Compare(existedInput.VoucherTypeCode, data.VoucherTypeCode, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    throw new BadRequestException(VoucherErrorCode.VoucherCodeAlreadyExisted);
                }

                throw new BadRequestException(VoucherErrorCode.VoucherTitleAlreadyExisted);
            }

            using var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync();
            try
            {
                VoucherType voucherType = _mapper.Map<VoucherType>(data);
                await _purchaseOrderDBContext.VoucherType.AddAsync(voucherType);
                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();
                await _activityLogService.CreateLog(EnumObjectType.VoucherType, voucherType.VoucherTypeId, $"Thêm chứng từ {voucherType.Title}", data.JsonSerialize());

                return voucherType.VoucherTypeId;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "Create");
                throw;
            }
        }

        public string GetStringClone(string source, int suffix = 0)
        {
            string suffixText = suffix > 0 ? string.Format("({0})", suffix) : string.Empty;
            string code = string.Format("{0}_{1}{2}", source, "Copy", suffixText);
            if (_purchaseOrderDBContext.VoucherType.Any(i => i.VoucherTypeCode == code))
            {
                suffix++;
                code = GetStringClone(source, suffix);
            }
            return code;
        }

        public async Task<int> CloneVoucherType(int voucherTypeId)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockVoucherTypeKey(0));
            var sourceVoucherType = await _purchaseOrderDBContext.VoucherType
                .Include(i => i.VoucherArea)
                .Include(a => a.VoucherAreaField)
                .FirstOrDefaultAsync(i => i.VoucherTypeId == voucherTypeId);
            if (sourceVoucherType == null)
            {
                throw new BadRequestException(VoucherErrorCode.SourceVoucherTypeNotFound);
            }

            using var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync();
            try
            {
                var cloneType = new VoucherType
                {
                    VoucherTypeCode = GetStringClone(sourceVoucherType.VoucherTypeCode),
                    Title = GetStringClone(sourceVoucherType.Title),
                    VoucherTypeGroupId = sourceVoucherType.VoucherTypeGroupId,
                    SortOrder = sourceVoucherType.SortOrder,
                    PreLoadAction = sourceVoucherType.PreLoadAction,
                    PostLoadAction = sourceVoucherType.PostLoadAction,
                    AfterLoadAction = sourceVoucherType.AfterLoadAction,
                    BeforeSubmitAction = sourceVoucherType.BeforeSubmitAction,
                    BeforeSaveAction = sourceVoucherType.BeforeSaveAction,
                    AfterSaveAction = sourceVoucherType.AfterSaveAction
                };
                await _purchaseOrderDBContext.VoucherType.AddAsync(cloneType);
                await _purchaseOrderDBContext.SaveChangesAsync();

                foreach (var area in sourceVoucherType.VoucherArea)
                {
                    var cloneArea = new VoucherArea
                    {
                        VoucherTypeId = cloneType.VoucherTypeId,
                        VoucherAreaCode = area.VoucherAreaCode,
                        Title = area.Title,
                        IsMultiRow = area.IsMultiRow,
                        Columns = area.Columns,
                        SortOrder = area.SortOrder
                    };
                    await _purchaseOrderDBContext.VoucherArea.AddAsync(cloneArea);
                    await _purchaseOrderDBContext.SaveChangesAsync();

                    foreach (var field in sourceVoucherType.VoucherAreaField.Where(f => f.VoucherAreaId == area.VoucherAreaId).ToList())
                    {
                        var cloneField = new VoucherAreaField
                        {
                            VoucherFieldId = field.VoucherFieldId,
                            VoucherTypeId = cloneType.VoucherTypeId,
                            VoucherAreaId = cloneArea.VoucherTypeId,
                            Title = field.Title,
                            Placeholder = field.Placeholder,
                            SortOrder = field.SortOrder,
                            IsAutoIncrement = field.IsAutoIncrement,
                            IsRequire = field.IsRequire,
                            IsUnique = field.IsUnique,
                            IsHidden = field.IsHidden,
                            RegularExpression = field.RegularExpression,
                            DefaultValue = field.DefaultValue,
                            Filters = field.Filters,
                            Width = field.Width,
                            Height = field.Height,
                            TitleStyleJson = field.TitleStyleJson,
                            InputStyleJson = field.InputStyleJson,
                            OnFocus = field.OnFocus,
                            OnKeydown = field.OnKeydown,
                            OnKeypress = field.OnKeypress,
                            OnBlur = field.OnBlur,
                            OnChange = field.OnChange,
                            AutoFocus = field.AutoFocus,
                            Column = field.Column
                        };
                        await _purchaseOrderDBContext.VoucherAreaField.AddAsync(cloneField);
                    }
                }
                await _purchaseOrderDBContext.SaveChangesAsync();
                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.VoucherType, cloneType.VoucherTypeId, $"Thêm chứng từ {cloneType.Title}", cloneType.JsonSerialize());
                return cloneType.VoucherTypeId;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "Create");
                throw;
            }
        }

        public async Task<bool> UpdateVoucherType(int voucherTypeId, VoucherTypeModel data)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockVoucherTypeKey(voucherTypeId));
            var inputType = await _purchaseOrderDBContext.VoucherType.FirstOrDefaultAsync(i => i.VoucherTypeId == voucherTypeId);
            if (inputType == null)
            {
                throw new BadRequestException(VoucherErrorCode.VoucherTypeNotFound);
            }
            if (inputType.VoucherTypeCode != data.VoucherTypeCode || inputType.Title != data.Title)
            {
                var existedInput = await _purchaseOrderDBContext.VoucherType
                    .FirstOrDefaultAsync(i => i.VoucherTypeId != voucherTypeId && (i.VoucherTypeCode == data.VoucherTypeCode || i.Title == data.Title));
                if (existedInput != null)
                {
                    if (string.Compare(existedInput.VoucherTypeCode, data.VoucherTypeCode, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        throw new BadRequestException(VoucherErrorCode.VoucherCodeAlreadyExisted);
                    }

                    throw new BadRequestException(VoucherErrorCode.VoucherTitleAlreadyExisted);
                }
            }

            using var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync();
            try
            {
                inputType.VoucherTypeCode = data.VoucherTypeCode;
                inputType.Title = data.Title;
                inputType.SortOrder = data.SortOrder;
                inputType.VoucherTypeGroupId = data.VoucherTypeGroupId;
                inputType.PreLoadAction = data.PreLoadAction;
                inputType.PostLoadAction = data.PostLoadAction;
                inputType.AfterLoadAction = data.AfterLoadAction;
                inputType.BeforeSubmitAction = data.BeforeSubmitAction;
                inputType.BeforeSaveAction = data.BeforeSaveAction;
                inputType.AfterSaveAction = data.AfterSaveAction;

                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.VoucherType, inputType.VoucherTypeId, $"Cập nhật chứng từ {inputType.Title}", data.JsonSerialize());
                return true;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "Update");
                throw;
            }
        }

        public async Task<bool> DeleteVoucherType(int voucherTypeId)
        {
            var voucherType = await _purchaseOrderDBContext.VoucherType.FirstOrDefaultAsync(i => i.VoucherTypeId == voucherTypeId);
            if (voucherType == null)
            {
                throw new BadRequestException(VoucherErrorCode.VoucherTypeNotFound);
            }

            await _purchaseOrderDBContext.ExecuteStoreProcedure("asp_VoucherType_Delete", new[] {
                    new SqlParameter("@VoucherTypeId",voucherTypeId ),
                    new SqlParameter("@ResStatus",0){ Direction = ParameterDirection.Output },
                    });


            await _activityLogService.CreateLog(EnumObjectType.InventoryInput, voucherType.VoucherTypeId, $"Xóa chứng từ {voucherType.Title}", voucherType.JsonSerialize());
            return true;
        }


        #region InputTypeView
        public async Task<IList<VoucherTypeViewModelList>> VoucherTypeViewList(int voucherTypeId)
        {
            return await _purchaseOrderDBContext.VoucherTypeView.Where(v => v.VoucherTypeId == voucherTypeId).ProjectTo<VoucherTypeViewModelList>(_mapper.ConfigurationProvider).ToListAsync();
        }

        public async Task<VoucherTypeBasicOutput> GetVoucherTypeBasicInfo(int voucherTypeId)
        {
            var voucherTypeInfo = await _purchaseOrderDBContext.VoucherType.AsNoTracking().Where(t => t.VoucherTypeId == voucherTypeId).ProjectTo<VoucherTypeBasicOutput>(_mapper.ConfigurationProvider).FirstOrDefaultAsync();

            voucherTypeInfo.Areas = await _purchaseOrderDBContext.VoucherArea.AsNoTracking().Where(a => a.VoucherTypeId == voucherTypeId).OrderBy(a => a.SortOrder).ProjectTo<VoucherAreaBasicOutput>(_mapper.ConfigurationProvider).ToListAsync();

            var fields = await (
                from af in _purchaseOrderDBContext.VoucherAreaField
                join f in _purchaseOrderDBContext.VoucherField on af.VoucherFieldId equals f.VoucherFieldId
                where af.VoucherTypeId == voucherTypeId
                orderby af.SortOrder
                select new VoucherAreaFieldBasicOutput
                {
                    VoucherAreaId = af.VoucherAreaId,
                    VoucherAreaFieldId = af.VoucherAreaFieldId,
                    FieldName = f.FieldName,
                    Title = af.Title,
                    Placeholder = af.Placeholder,
                    DataTypeId = (EnumDataType)f.DataTypeId,
                    DataSize = f.DataSize,
                    FormTypeId = (EnumFormType)f.FormTypeId,
                    DefaultValue = af.DefaultValue,
                    RefTableCode = f.RefTableCode,
                    RefTableField = f.RefTableField,
                    RefTableTitle = f.RefTableTitle

                }).ToListAsync();

            var views = await _purchaseOrderDBContext.VoucherTypeView.AsNoTracking().Where(t => t.VoucherTypeId == voucherTypeId).OrderByDescending(v => v.IsDefault).ProjectTo<VoucherTypeViewModelList>(_mapper.ConfigurationProvider).ToListAsync();

            foreach (var item in voucherTypeInfo.Areas)
            {
                item.Fields = fields.Where(f => f.VoucherAreaId == item.VoucherAreaId).ToList();
            }

            voucherTypeInfo.Views = views;

            return voucherTypeInfo;
        }

        public async Task<VoucherTypeViewModel> GetVoucherTypeViewInfo(int voucherTypeId, int voucherTypeViewId)
        {
            var info = await _purchaseOrderDBContext.VoucherTypeView.AsNoTracking().Where(t => t.VoucherTypeId == voucherTypeId && t.VoucherTypeViewId == voucherTypeViewId).ProjectTo<VoucherTypeViewModel>(_mapper.ConfigurationProvider).FirstOrDefaultAsync();

            if (info == null)
            {
                throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy cấu hình trong hệ thống");
            }

            var fields = await _purchaseOrderDBContext.VoucherTypeViewField.AsNoTracking()
                .Where(t => t.VoucherTypeViewId == voucherTypeViewId)
                .ProjectTo<VoucherTypeViewFieldModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

            info.Fields = fields;

            return info;
        }

        public async Task<int> VoucherTypeViewCreate(int voucherTypeId, VoucherTypeViewModel model)
        {
            using var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync();
            try
            {
                var voucherTypeInfo = await _purchaseOrderDBContext.VoucherType.FirstOrDefaultAsync(t => t.VoucherTypeId == voucherTypeId);
                if (voucherTypeInfo == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy loại chứng từ");

                var info = _mapper.Map<VoucherTypeView>(model);

                info.VoucherTypeId = voucherTypeId;

                await _purchaseOrderDBContext.VoucherTypeView.AddAsync(info);
                await _purchaseOrderDBContext.SaveChangesAsync();

                await VoucherTypeViewFieldAddRange(info.VoucherTypeViewId, model.Fields);

                await _purchaseOrderDBContext.SaveChangesAsync();

                await trans.CommitAsync();

                await _activityLogService.CreateLog(EnumObjectType.VoucherTypeView, info.VoucherTypeViewId, $"Tạo bộ lọc {info.VoucherTypeViewName} cho chứng từ  {voucherTypeInfo.Title}", model.JsonSerialize());

                return info.VoucherTypeViewId;
            }
            catch (Exception ex)
            {
                await trans.TryRollbackTransactionAsync();
                _logger.LogError(ex, "InputTypeViewCreate");
                throw;
            }
        }

        public async Task<bool> VoucherTypeViewUpdate(int voucherTypeViewId, VoucherTypeViewModel model)
        {
            using var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync();
            try
            {

                var info = await _purchaseOrderDBContext.VoucherTypeView.FirstOrDefaultAsync(v => v.VoucherTypeViewId == voucherTypeViewId);

                if (info == null) throw new BadRequestException(GeneralCode.ItemNotFound, "View không tồn tại");

                var voucherTypeInfo = await _purchaseOrderDBContext.VoucherType.FirstOrDefaultAsync(t => t.VoucherTypeId == info.VoucherTypeId);
                if (voucherTypeInfo == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy loại chứng từ");


                _mapper.Map(model, info);

                var oldFields = await _purchaseOrderDBContext.VoucherTypeViewField.Where(f => f.VoucherTypeViewId == voucherTypeViewId).ToListAsync();

                _purchaseOrderDBContext.VoucherTypeViewField.RemoveRange(oldFields);

                await VoucherTypeViewFieldAddRange(voucherTypeViewId, model.Fields);

                await _purchaseOrderDBContext.SaveChangesAsync();

                await trans.CommitAsync();

                await _activityLogService.CreateLog(EnumObjectType.VoucherTypeView, info.VoucherTypeViewId, $"Cập nhật bộ lọc {info.VoucherTypeViewName} cho chứng từ  {voucherTypeInfo.Title}", model.JsonSerialize());

                return true;
            }
            catch (Exception ex)
            {
                await trans.TryRollbackTransactionAsync();
                _logger.LogError(ex, "InputTypeViewUpdate");
                throw;
            }
        }

        public async Task<bool> VoucherTypeViewDelete(int voucherTypeViewId)
        {
            var info = await _purchaseOrderDBContext.VoucherTypeView.FirstOrDefaultAsync(v => v.VoucherTypeViewId == voucherTypeViewId);

            if (info == null) throw new BadRequestException(GeneralCode.ItemNotFound, "View không tồn tại");

            info.IsDeleted = true;
            info.DeletedDatetimeUtc = DateTime.UtcNow;


            var voucherTypeInfo = await _purchaseOrderDBContext.VoucherType.FirstOrDefaultAsync(t => t.VoucherTypeId == info.VoucherTypeId);
            if (voucherTypeInfo == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy loại chứng từ");


            await _purchaseOrderDBContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.VoucherTypeView, info.VoucherTypeViewId, $"Xóa bộ lọc {info.VoucherTypeViewName} chứng từ  {voucherTypeInfo.Title}", new { voucherTypeViewId }.JsonSerialize());

            return true;

        }
        #endregion InputTypeView

        #region InputTypeGroup
        public async Task<int> VoucherTypeGroupCreate(VoucherTypeGroupModel model)
        {
            var info = _mapper.Map<VoucherTypeGroup>(model);
            await _purchaseOrderDBContext.VoucherTypeGroup.AddAsync(info);
            await _purchaseOrderDBContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.VoucherTypeGroup, info.VoucherTypeGroupId, $"Thêm nhóm chứng từ {info.VoucherTypeGroupName}", model.JsonSerialize());

            return info.VoucherTypeGroupId;
        }

        public async Task<bool> VoucherTypeGroupUpdate(int voucherTypeGroupId, VoucherTypeGroupModel model)
        {
            var info = await _purchaseOrderDBContext.VoucherTypeGroup.FirstOrDefaultAsync(g => g.VoucherTypeGroupId == voucherTypeGroupId);

            if (info == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Nhóm chứng từ không tồn tại");

            _mapper.Map(model, info);

            await _purchaseOrderDBContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.VoucherTypeGroup, info.VoucherTypeGroupId, $"Cập nhật nhóm chứng từ {info.VoucherTypeGroupName}", model.JsonSerialize());

            return true;
        }

        public async Task<bool> VoucherTypeGroupDelete(int voucherTypeGroupId)
        {
            var info = await _purchaseOrderDBContext.VoucherTypeGroup.FirstOrDefaultAsync(g => g.VoucherTypeGroupId == voucherTypeGroupId);

            if (info == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Nhóm chứng từ không tồn tại");

            info.IsDeleted = true;

            await _purchaseOrderDBContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.VoucherTypeGroup, info.VoucherTypeGroupId, $"Xóa nhóm chứng từ {info.VoucherTypeGroupName}", new { voucherTypeGroupId }.JsonSerialize());

            return true;
        }

        public async Task<IList<VoucherTypeGroupList>> VoucherTypeGroupList()
        {
            return await _purchaseOrderDBContext.VoucherTypeGroup.ProjectTo<VoucherTypeGroupList>(_mapper.ConfigurationProvider).OrderBy(g => g.SortOrder).ToListAsync();
        }

        #endregion


        private async Task VoucherTypeViewFieldAddRange(int VoucherTypeViewId, IList<VoucherTypeViewFieldModel> fieldModels)
        {
            //var categoryFieldIds = fieldModels.Where(f => f.ReferenceCategoryFieldId.HasValue).Select(f => f.ReferenceCategoryFieldId.Value).ToList();
            //categoryFieldIds.Union(fieldModels.Where(f => f.ReferenceCategoryTitleFieldId.HasValue).Select(f => f.ReferenceCategoryFieldId.Value).ToList());

            //if (categoryFieldIds.Count > 0)
            //{

            //    var categoryFields = (await _accountancyDBContext.CategoryField
            //        .Where(f => categoryFieldIds.Contains(f.CategoryFieldId))
            //        .Select(f => new { f.CategoryFieldId, f.CategoryId })
            //        .AsNoTracking()
            //        .ToListAsync())
            //        .ToDictionary(f => f.CategoryFieldId, f => f);

            //    foreach (var f in fieldModels)
            //    {
            //        if (f.ReferenceCategoryFieldId.HasValue && categoryFields.TryGetValue(f.ReferenceCategoryFieldId.Value, out var cateField) && cateField.CategoryId != f.ReferenceCategoryId)
            //        {
            //            throw new BadRequestException(GeneralCode.InvalidParams, "Trường dữ liệu của danh mục không thuộc danh mục");
            //        }

            //        if (f.ReferenceCategoryTitleFieldId.HasValue && categoryFields.TryGetValue(f.ReferenceCategoryTitleFieldId.Value, out cateField) && cateField.CategoryId != f.ReferenceCategoryId)
            //        {
            //            throw new BadRequestException(GeneralCode.InvalidParams, "Trường hiển thị của danh mục không thuộc danh mục");
            //        }
            //    }

            //}

            var fields = fieldModels.Select(f => _mapper.Map<VoucherTypeViewField>(f)).ToList();

            foreach (var f in fields)
            {
                f.VoucherTypeViewId = VoucherTypeViewId;
            }

            await _purchaseOrderDBContext.VoucherTypeViewField.AddRangeAsync(fields);

        }

        #endregion

        #region Area

        public async Task<VoucherAreaModel> GetVoucherArea(int voucherTypeId, int voucherAreaId)
        {
            var inputArea = await _purchaseOrderDBContext.VoucherArea
                .Where(i => i.VoucherTypeId == voucherTypeId && i.VoucherAreaId == voucherAreaId)
                .ProjectTo<VoucherAreaModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
            if (inputArea == null)
            {
                throw new BadRequestException(VoucherErrorCode.VoucherTypeNotFound);
            }
            return inputArea;
        }

        public async Task<PageData<VoucherAreaModel>> GetVoucherAreas(int voucherTypeId, string keyword, int page, int size)
        {
            keyword = (keyword ?? "").Trim();
            var query = _purchaseOrderDBContext.VoucherType.Where(a => a.VoucherTypeId == voucherTypeId).AsQueryable();
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(a => a.VoucherTypeCode.Contains(keyword) || a.Title.Contains(keyword));
            }
            query = query.OrderBy(c => c.Title);
            var total = await query.CountAsync();
            if (size > 0)
            {
                query = query.Skip((page - 1) * size).Take(size);
            }
            var lst = await query.ProjectTo<VoucherAreaModel>(_mapper.ConfigurationProvider).OrderBy(a => a.SortOrder).ToListAsync();
            return (lst, total);
        }

        public async Task<int> AddVoucherArea(int voucherTypeId, VoucherAreaInputModel data)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockVoucherTypeKey(voucherTypeId));
            var existedVoucher = await _purchaseOrderDBContext.VoucherArea
                .FirstOrDefaultAsync(a => a.VoucherTypeId == voucherTypeId && (a.VoucherAreaCode == data.VoucherAreaCode || a.Title == data.Title));
            if (existedVoucher != null)
            {
                if (string.Compare(existedVoucher.VoucherAreaCode, data.VoucherAreaCode, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    throw new BadRequestException(VoucherErrorCode.VoucherCodeAlreadyExisted);
                }

                throw new BadRequestException(VoucherErrorCode.VoucherTitleAlreadyExisted);
            }

            if (data.IsMultiRow && _purchaseOrderDBContext.VoucherArea.Any(a => a.VoucherTypeId == voucherTypeId && a.IsMultiRow))
            {
                throw new BadRequestException(VoucherErrorCode.MultiRowAreaAlreadyExisted);
            }


            using var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync();
            try
            {
                VoucherArea voucherArea = _mapper.Map<VoucherArea>(data);
                voucherArea.VoucherTypeId = voucherTypeId;
                await _purchaseOrderDBContext.VoucherArea.AddAsync(voucherArea);
                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.VoucherType, voucherArea.VoucherAreaId, $"Thêm vùng thông tin {voucherArea.Title}", data.JsonSerialize());
                return voucherArea.VoucherAreaId;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "Create");
                throw;
            }
        }

        public async Task<bool> UpdateVoucherArea(int voucherTypeId, int voucherAreaId, VoucherAreaInputModel data)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockVoucherTypeKey(voucherTypeId));
            var voucherArea = await _purchaseOrderDBContext.VoucherArea.FirstOrDefaultAsync(a => a.VoucherTypeId == voucherTypeId && a.VoucherAreaId == voucherAreaId);
            if (voucherArea == null)
            {
                throw new BadRequestException(VoucherErrorCode.VoucherAreaNotFound);
            }
            if (voucherArea.VoucherAreaCode != data.VoucherAreaCode || voucherArea.Title != data.Title)
            {
                var existedInput = await _purchaseOrderDBContext.VoucherArea
                    .FirstOrDefaultAsync(a => a.VoucherTypeId == voucherTypeId && a.VoucherAreaId != voucherAreaId && (a.VoucherAreaCode == data.VoucherAreaCode || a.Title == data.Title));
                if (existedInput != null)
                {
                    if (string.Compare(existedInput.VoucherAreaCode, data.VoucherAreaCode, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        throw new BadRequestException(VoucherErrorCode.VoucherAreaCodeAlreadyExisted);
                    }

                    throw new BadRequestException(VoucherErrorCode.VoucherAreaTitleAlreadyExisted);
                }
            }
            if (data.IsMultiRow && _purchaseOrderDBContext.VoucherArea.Any(a => a.VoucherTypeId == voucherTypeId && a.VoucherAreaId != voucherAreaId && a.IsMultiRow))
            {
                throw new BadRequestException(VoucherErrorCode.MultiRowAreaAlreadyExisted);
            }

            using var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync();
            try
            {
                voucherArea.VoucherAreaCode = data.VoucherAreaCode;
                voucherArea.Title = data.Title;
                voucherArea.IsMultiRow = data.IsMultiRow;
                voucherArea.Columns = data.Columns;
                voucherArea.SortOrder = data.SortOrder;
                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.VoucherType, voucherArea.VoucherTypeId, $"Cập nhật vùng dữ liệu {voucherArea.Title}", data.JsonSerialize());
                return true;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "Update");
                throw;
            }
        }

        public async Task<bool> DeleteVoucherArea(int voucherTypeId, int voucherAreaId)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockVoucherTypeKey(voucherTypeId));
            var voucherArea = await _purchaseOrderDBContext.VoucherArea.FirstOrDefaultAsync(a => a.VoucherTypeId == voucherTypeId && a.VoucherAreaId == voucherAreaId);
            if (voucherArea == null)
            {
                throw new BadRequestException(VoucherErrorCode.VoucherAreaNotFound);
            }

            await _purchaseOrderDBContext.ExecuteStoreProcedure("asp_VoucherArea_Delete", new[] {
                    new SqlParameter("@VoucherTypeId", voucherTypeId),
                    new SqlParameter("@VoucherAreaId", voucherAreaId),
                    new SqlParameter("@ResStatus", 0){ Direction = ParameterDirection.Output },
                    });

            voucherArea.IsDeleted = true;
            await _purchaseOrderDBContext.SaveChangesAsync();
            await _activityLogService.CreateLog(EnumObjectType.InventoryInput, voucherArea.VoucherTypeId, $"Xóa vùng chứng từ {voucherArea.Title}", voucherArea.JsonSerialize());
            return true;
        }

        #endregion

        #region Field

        public async Task<PageData<VoucherAreaFieldOutputFullModel>> GetVoucherAreaFields(int voucherTypeId, int voucherAreaId, string keyword, int page, int size)
        {
            keyword = (keyword ?? "").Trim();
            var query = _purchaseOrderDBContext.VoucherAreaField
                .Include(f => f.VoucherField)
                .Where(f => f.VoucherTypeId == voucherTypeId && f.VoucherAreaId == voucherAreaId);
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(f => f.Title.Contains(keyword));
            }
            query = query.OrderBy(c => c.SortOrder);
            var total = await query.CountAsync();
            if (size > 0)
            {
                query = query.Skip((page - 1) * size).Take(size);
            }
            var lst = await query.ProjectTo<VoucherAreaFieldOutputFullModel>(_mapper.ConfigurationProvider).ToListAsync();
            return (lst, total);
        }

        public async Task<PageData<VoucherFieldOutputModel>> GetVoucherFields(string keyword, int page, int size)
        {
            keyword = (keyword ?? "").Trim();
            var query = _purchaseOrderDBContext.VoucherField
                .AsQueryable();
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(f => f.FieldName.Contains(keyword) || f.Title.Contains(keyword));
            }
            var total = await query.CountAsync();

            if (size > 0)
            {
                query = query.Skip((page - 1) * size).Take(size);
            }
            var lst = query.ProjectTo<VoucherFieldOutputModel>(_mapper.ConfigurationProvider).ToList();
            return (lst, total);
        }

        public async Task<VoucherAreaFieldOutputFullModel> GetVoucherAreaField(int voucherTypeId, int voucherAreaId, int voucherAreaFieldId)
        {
            var voucherAreaField = await _purchaseOrderDBContext.VoucherAreaField
                .Where(f => f.VoucherAreaFieldId == voucherAreaFieldId && f.VoucherTypeId == voucherTypeId && f.VoucherAreaId == voucherAreaId)
                .Include(f => f.VoucherField)
                .ProjectTo<VoucherAreaFieldOutputFullModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
            if (voucherAreaField == null)
            {
                throw new BadRequestException(VoucherErrorCode.VoucherAreaFieldNotFound);
            }

            return voucherAreaField;
        }

        private void ValidateVoucherField(VoucherFieldInputModel data, VoucherField voucherField = null, int? voucherFieldId = null)
        {
            if (voucherFieldId.HasValue && voucherFieldId.Value > 0)
            {
                if (voucherField == null)
                {
                    throw new BadRequestException(VoucherErrorCode.VoucherFieldNotFound);
                }
                if (_purchaseOrderDBContext.VoucherField.Any(f => f.VoucherFieldId != voucherFieldId.Value && f.FieldName == data.FieldName))
                {
                    throw new BadRequestException(VoucherErrorCode.VoucherFieldAlreadyExisted);
                }
                if (!((EnumDataType)voucherField.DataTypeId).Convertible((EnumDataType)data.DataTypeId))
                {
                    throw new BadRequestException(GeneralCode.InvalidParams, $"Không thể chuyển đổi kiểu dữ liệu từ {((EnumDataType)voucherField.DataTypeId).GetEnumDescription()} sang {((EnumDataType)data.DataTypeId).GetEnumDescription()}");
                }
            }
            //if (data.ReferenceCategoryFieldId.HasValue)
            //{
            //    var sourceCategoryField = _accountancyDBContext.CategoryField.FirstOrDefault(f => f.CategoryFieldId == data.ReferenceCategoryFieldId.Value);
            //    if (sourceCategoryField == null)
            //    {
            //        return VoucherErrorCode.SourceCategoryFieldNotFound;
            //    }
            //}
        }

        private void FieldDataProcess(ref VoucherFieldInputModel data)
        {
            //if (data.ReferenceCategoryFieldId.HasValue)
            //{
            //    int referId = data.ReferenceCategoryFieldId.Value;
            //    var sourceCategoryField = _accountancyDBContext.CategoryField.FirstOrDefault(f => f.CategoryFieldId == referId);
            //    data.DataTypeId = sourceCategoryField.DataTypeId;
            //    data.DataSize = sourceCategoryField.DataSize;
            //}
            //if (data.FormTypeId == (int)EnumFormType.Generate)
            //{
            //    data.DataTypeId = (int)EnumDataType.Text;
            //    data.DataSize = 0;
            //}
            //if (!AccountantConstants.SELECT_FORM_TYPES.Contains((EnumFormType)data.FormTypeId))
            //{
            //    data.ReferenceCategoryFieldId = null;
            //    data.ReferenceCategoryTitleFieldId = null;
            //}
        }

        public async Task<bool> UpdateMultiField(int voucherTypeId, List<VoucherAreaFieldInputModel> fields)
        {
            var voucherTypeInfo = await _purchaseOrderDBContext.VoucherType.AsNoTracking()
                .Where(t => t.VoucherTypeId == voucherTypeId)
                .FirstOrDefaultAsync();

            if (voucherTypeInfo == null)
            {
                throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy loại chứng từ!");
            }

            var areaIds = fields.Select(f => f.VoucherAreaId).Distinct().ToList();

            var voucherAreas = await _purchaseOrderDBContext.VoucherArea.Where(a => a.VoucherTypeId == voucherTypeId).AsNoTracking().ToListAsync();

            foreach (var areaId in areaIds)
            {
                if (!voucherAreas.Any(a => a.VoucherAreaId == areaId))
                {
                    throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy vùng dữ liệu chứng từ!");
                }
            }

            foreach (var field in fields)
            {
                field.VoucherTypeId = voucherTypeId;
            }


            // Validate trùng trong danh sách
            if (fields.Select(f => new { f.VoucherTypeId, f.VoucherFieldId }).Distinct().Count() != fields.Count)
            {
                throw new BadRequestException(VoucherErrorCode.VoucherAreaFieldAlreadyExisted);
            }

            var curFields = _purchaseOrderDBContext.VoucherAreaField
                .Include(af => af.VoucherField)
                .IgnoreQueryFilters()
                .Where(f => f.VoucherTypeId == voucherTypeId)
                .ToList();

            var deleteFields = curFields
                .Where(cf => !cf.IsDeleted)
                .Where(cf => fields.All(f => f.VoucherFieldId != cf.VoucherFieldId))
                .ToList();

            List<int> singleNewFieldIds = new List<int>();

            using var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync();
            try
            {
                // delete
                foreach (var deleteField in deleteFields)
                {
                    deleteField.IsDeleted = true;

                    await _purchaseOrderDBContext.ExecuteStoreProcedure("asp_VoucherType_Clear_FieldData", new[] {
                        new SqlParameter("@VoucherTypeId",voucherTypeId ),
                        new SqlParameter("@FieldName", deleteField.VoucherField.FieldName ),
                        new SqlParameter("@ResStatus", 0){ Direction = ParameterDirection.Output },
                    });
                }

                foreach (var field in fields)
                {
                    // validate
                    var curField = curFields.FirstOrDefault(f => f.VoucherFieldId == field.VoucherFieldId && f.VoucherTypeId == field.VoucherTypeId);
                    if (curField == null)
                    {
                        // create new
                        curField = _mapper.Map<VoucherAreaField>(field);
                        await _purchaseOrderDBContext.VoucherAreaField.AddAsync(curField);
                        // await _accountancyDBContext.SaveChangesAsync();
                        //field.InputAreaFieldId = curField.InputAreaFieldId;
                        // Add field need clear old data
                        //var area = areas.First(a => a.InputAreaId == curField.InputAreaId);
                        //if (!area.IsMultiRow)
                        //{
                        //    singleNewFieldIds.Add(curField.InputAreaFieldId);
                        //}
                    }
                    else if (!field.Compare(curField))
                    {
                        // update field
                        curField.VoucherAreaId = field.VoucherAreaId;
                        curField.Title = field.Title;
                        curField.Placeholder = field.Placeholder;
                        curField.SortOrder = field.SortOrder;
                        curField.IsAutoIncrement = field.IsAutoIncrement;
                        curField.IsRequire = field.IsRequire;
                        curField.IsUnique = field.IsUnique;
                        curField.IsHidden = field.IsHidden;
                        curField.IsCalcSum = field.IsCalcSum;
                        curField.RegularExpression = field.RegularExpression;
                        curField.DefaultValue = field.DefaultValue;
                        curField.Filters = field.Filters;
                        curField.IsDeleted = false;
                        // update field id
                        curField.VoucherFieldId = field.VoucherFieldId;
                        // update style
                        curField.Width = field.Width;
                        curField.Height = field.Height;
                        curField.TitleStyleJson = field.TitleStyleJson;
                        curField.InputStyleJson = field.InputStyleJson;
                        curField.OnFocus = field.OnFocus;
                        curField.OnKeydown = field.OnKeydown;
                        curField.OnKeypress = field.OnKeypress;
                        curField.OnBlur = field.OnBlur;
                        curField.OnChange = field.OnChange;
                        curField.AutoFocus = field.AutoFocus;
                        curField.Column = field.Column;
                        curField.RequireFilters = field.RequireFilters;
                    }
                }

                await _purchaseOrderDBContext.SaveChangesAsync();

                // Get list gen code
                var genCodeConfigs = fields
                    .Where(f => f.IdGencode.HasValue)
                    .Select(f => new
                    {
                        InputAreaFieldId = f.VoucherAreaFieldId.Value,
                        IdGencode = f.IdGencode.Value
                    })
                    .ToDictionary(c => c.InputAreaFieldId, c => c.IdGencode);

                var result = await _customGenCodeHelperService.MapObjectCustomGenCode(EnumObjectType.VoucherType, genCodeConfigs);

                if (!result)
                {
                    trans.TryRollbackTransaction();
                    throw new BadRequestException(VoucherErrorCode.MapGenCodeConfigFail);
                }
                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.VoucherType, voucherTypeId, $"Cập nhật trường dữ liệu chứng từ {voucherTypeInfo.Title}", fields.JsonSerialize());

                return true;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "Create");
                throw;
            }
        }

        public async Task<int> AddVoucherField(VoucherFieldInputModel data)
        {
            ValidateVoucherField(data);

            FieldDataProcess(ref data);

            using var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync();
            try
            {
                var voucherField = _mapper.Map<VoucherField>(data);

                await _purchaseOrderDBContext.VoucherField.AddAsync(voucherField);
                await _purchaseOrderDBContext.SaveChangesAsync();

                if (voucherField.FormTypeId != (int)EnumFormType.ViewOnly)
                {
                    await _purchaseOrderDBContext.AddColumn(VOUCHERVALUEROW_TABLE, data.FieldName, data.DataTypeId, data.DataSize, data.DecimalPlace, data.DefaultValue, true);
                }
                await UpdateVoucherValueView();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.VoucherType, voucherField.VoucherFieldId, $"Thêm trường dữ liệu chung {voucherField.Title}", data.JsonSerialize());
                return voucherField.VoucherFieldId;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "Create");
                throw;
            }
        }

        public async Task<bool> UpdateVoucherField(int voucherFieldId, VoucherFieldInputModel data)
        {
            var voucherField = await _purchaseOrderDBContext.VoucherField.FirstOrDefaultAsync(f => f.VoucherFieldId == voucherFieldId);

            ValidateVoucherField(data, voucherField, voucherFieldId);

            //FieldDataProcess(ref data);

            using var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync();
            try
            {
                if (voucherField.FormTypeId != (int)EnumFormType.ViewOnly)
                {
                    if (data.FieldName != voucherField.FieldName)
                    {
                        await _purchaseOrderDBContext.RenameColumn(VOUCHERVALUEROW_TABLE, voucherField.FieldName, data.FieldName);
                    }
                    await _purchaseOrderDBContext.UpdateColumn(VOUCHERVALUEROW_TABLE, data.FieldName, data.DataTypeId, data.DataSize, data.DecimalPlace, data.DefaultValue, true);
                }
                _mapper.Map(data, voucherField);

                await _purchaseOrderDBContext.SaveChangesAsync();

                await UpdateVoucherValueView();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.VoucherType, voucherField.VoucherFieldId, $"Cập nhật trường dữ liệu chung {voucherField.Title}", data.JsonSerialize());
                return true;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "Update");
                throw;
            }
        }

        public async Task<bool> DeleteVoucherField(int voucherFieldId)
        {
            var voucherField = await _purchaseOrderDBContext.VoucherField.FirstOrDefaultAsync(f => f.VoucherFieldId == voucherFieldId);
            if (voucherField == null)
            {
                throw new BadRequestException(VoucherErrorCode.VoucherFieldNotFound);
            }
            // Check used
            bool isUsed = _purchaseOrderDBContext.VoucherAreaField.Any(af => af.VoucherFieldId == voucherFieldId);
            if (isUsed)
            {
                throw new BadRequestException(VoucherErrorCode.VoucherFieldIsUsed);
            }
            using var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync();
            try
            {
                // Delete field
                voucherField.IsDeleted = true;
                await _purchaseOrderDBContext.SaveChangesAsync();
                if (voucherField.FormTypeId != (int)EnumFormType.ViewOnly)
                {
                    await _purchaseOrderDBContext.DropColumn(VOUCHERVALUEROW_TABLE, voucherField.FieldName);
                }
                await UpdateVoucherValueView();

                trans.Commit();
                await _activityLogService.CreateLog(EnumObjectType.VoucherType, voucherField.VoucherFieldId, $"Xóa trường dữ liệu chung {voucherField.Title}", voucherField.JsonSerialize());
                return true;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "Delete");
                throw;
            }
        }

        #endregion

        private async Task UpdateVoucherValueView()
        {
            await _purchaseOrderDBContext.ExecuteStoreProcedure("asp_VoucherValueRow_UpdateView", Array.Empty<SqlParameter>());
        }
    }
}

