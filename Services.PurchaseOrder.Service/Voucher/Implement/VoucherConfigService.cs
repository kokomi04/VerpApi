﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenXmlPowerTools;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Verp.Cache.RedisCache;
using Verp.Resources.Master.Config.ActionButton;
using Verp.Resources.PurchaseOrder.Voucher;
using VErp.Commons.Constants;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface.DynamicBill;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.PurchaseOrderDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper.System;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.PurchaseOrder.Model.Voucher;
using static NPOI.HSSF.UserModel.HeaderFooter;

namespace VErp.Services.PurchaseOrder.Service.Voucher.Implement
{
    public class VoucherConfigService : IVoucherConfigService
    {
        private const string VOUCHERVALUEROW_TABLE = PurchaseOrderConstants.VOUCHERVALUEROW_TABLE;

        private readonly ILogger _logger;
        private readonly ObjectActivityLogFacade _objActivityLogFacadeInputType;
        private readonly ObjectActivityLogFacade _objActivityLogFacadeInputTypeGroup;
        private readonly ObjectActivityLogFacade _objActivityLogFacadeVoucherType;
        private readonly ObjectActivityLogFacade _objActivityLogFacadeVoucherTypeView;
        private readonly IMapper _mapper;
        private readonly PurchaseOrderDBContext _purchaseOrderDBContext;
        private readonly ICustomGenCodeHelperService _customGenCodeHelperService;
        private readonly IHttpCrossService _httpCrossService;
        private readonly IRoleHelperService _roleHelperService;
        private readonly IVoucherActionConfigService _voucherActionConfigService;

        public VoucherConfigService(PurchaseOrderDBContext purchaseOrderDBContext
            , IOptions<AppSetting> appSetting
            , ILogger<VoucherConfigService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            , ICustomGenCodeHelperService customGenCodeHelperService
            , IMenuHelperService menuHelperService
            , ICurrentContextService currentContextService
            , IHttpCrossService httpCrossService
            , IRoleHelperService roleHelperService
            , IVoucherActionConfigService voucherActionConfigService
            )
        {
            _purchaseOrderDBContext = purchaseOrderDBContext;
            _logger = logger;
            _objActivityLogFacadeInputType = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.InputType);
            _objActivityLogFacadeInputTypeGroup = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.InputTypeGroup);
            _objActivityLogFacadeVoucherType = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.VoucherType);
            _objActivityLogFacadeVoucherTypeView = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.VoucherTypeView);
            _mapper = mapper;
            _customGenCodeHelperService = customGenCodeHelperService;
            _httpCrossService = httpCrossService;
            _roleHelperService = roleHelperService;
            _voucherActionConfigService = voucherActionConfigService;
        }


        public async Task<VoucherTypeGlobalSettingModel> GetVoucherGlobalSetting()
        {
            var inputTypeSetting = await _purchaseOrderDBContext.VoucherTypeGlobalSetting.FirstOrDefaultAsync();
            if (inputTypeSetting == null)
            {
                inputTypeSetting = new VoucherTypeGlobalSetting();
            }

            return _mapper.Map<VoucherTypeGlobalSettingModel>(inputTypeSetting);
        }

        public async Task<bool> UpdateVoucherGlobalSetting(VoucherTypeGlobalSettingModel data)
        {
            var inputTypeSetting = await _purchaseOrderDBContext.VoucherTypeGlobalSetting.FirstOrDefaultAsync();
            if (inputTypeSetting == null)
            {
                inputTypeSetting = new VoucherTypeGlobalSetting();
            }

            using var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync();
            try
            {
                _mapper.Map(data, inputTypeSetting);
                if (inputTypeSetting.VoucherTypeGlobalSettingId <= 0)
                {
                    _purchaseOrderDBContext.VoucherTypeGlobalSetting.Add(inputTypeSetting);
                }

                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _objActivityLogFacadeInputType.LogBuilder(() => VoucherConfigActivityLogMessage.UpdateVoucherConfig)
                    .ObjectId(0)
                   .JsonData(data)
                   .CreateLog();
                return true;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "UpdateVoucherGlobalSetting");
                throw;
            }
        }

        #region InputType

        public async Task<VoucherTypeFullModel> GetVoucherType(int voucherTypeId)
        {
            var globalSetting = await GetVoucherGlobalSetting();

            var voucherType = await _purchaseOrderDBContext.VoucherType
           .Where(i => i.VoucherTypeId == voucherTypeId)
           .Include(t => t.VoucherArea)
           .ThenInclude(a => a.VoucherAreaField)
           .ThenInclude(af => af.VoucherField)
           .Include(t => t.VoucherArea)
           .ThenInclude(a => a.VoucherAreaField)
           .ThenInclude(af => af.VoucherField)
           .ProjectTo<VoucherTypeFullModel>(_mapper.ConfigurationProvider)
           .FirstOrDefaultAsync();
            if (voucherType == null)
            {
                throw new BadRequestException(VoucherErrorCode.VoucherTypeNotFound);
            }

            voucherType.VoucherAreas = voucherType.VoucherAreas.OrderBy(f => f.SortOrder).ToList();
            foreach (var item in voucherType.VoucherAreas)
            {
                item.VoucherAreaFields = item.VoucherAreaFields.OrderBy(f => f.SortOrder).ToList();
            }

            voucherType.SetGlobalSetting(globalSetting);

            return voucherType;
        }

        public async Task<IList<VoucherTypeFullModel>> GetAllVoucherTypes()
        {
            var globalSetting = await GetVoucherGlobalSetting();

            var voucherTypes = await _purchaseOrderDBContext.VoucherType
           .Include(t => t.VoucherArea)
           .ThenInclude(a => a.VoucherAreaField)
           .ThenInclude(af => af.VoucherField)
           .Include(t => t.VoucherArea)
           .ThenInclude(a => a.VoucherAreaField)
           .ThenInclude(af => af.VoucherField)
           .ProjectTo<VoucherTypeFullModel>(_mapper.ConfigurationProvider)
           .ToListAsync();

            foreach (var voucherType in voucherTypes)
            {
                voucherType.VoucherAreas = voucherType.VoucherAreas.OrderBy(f => f.SortOrder).ToList();
                foreach (var item in voucherType.VoucherAreas)
                {
                    item.VoucherAreaFields = item.VoucherAreaFields.OrderBy(f => f.SortOrder).ToList();
                }

                voucherType.SetGlobalSetting(globalSetting);
            }

            return voucherTypes;
        }

        public async Task<VoucherTypeFullModel> GetVoucherType(string voucherTypeCode)
        {
            var globalSetting = await GetVoucherGlobalSetting();

            var voucherType = await _purchaseOrderDBContext.VoucherType
           .Where(i => i.VoucherTypeCode == voucherTypeCode)
           .Include(t => t.VoucherArea)
           .ThenInclude(a => a.VoucherAreaField)
           .ThenInclude(af => af.VoucherField)
           .Include(t => t.VoucherArea)
           .ThenInclude(a => a.VoucherAreaField)
           .ThenInclude(af => af.VoucherField)
           .ProjectTo<VoucherTypeFullModel>(_mapper.ConfigurationProvider)
           .FirstOrDefaultAsync();
            if (voucherType == null)
            {
                throw new BadRequestException(VoucherErrorCode.VoucherTypeNotFound);
            }

            voucherType.VoucherAreas = voucherType.VoucherAreas.OrderBy(f => f.SortOrder).ToList();
            foreach (var item in voucherType.VoucherAreas)
            {
                item.VoucherAreaFields = item.VoucherAreaFields.OrderBy(f => f.SortOrder).ToList();
            }


            voucherType.SetGlobalSetting(globalSetting);

            return voucherType;
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

        public async Task<IList<VoucherTypeSimpleModel>> GetVoucherTypeSimpleList()
        {
            var voucherTypes = await _purchaseOrderDBContext.VoucherType.Where(x => !x.IsHide).ProjectTo<VoucherTypeSimpleProjectMappingModel>(_mapper.ConfigurationProvider).OrderBy(t => t.SortOrder).ToListAsync();


            var areaFields = await (
                from a in _purchaseOrderDBContext.VoucherArea
                join af in _purchaseOrderDBContext.VoucherAreaField on a.VoucherAreaId equals af.VoucherAreaId
                join f in _purchaseOrderDBContext.VoucherField on af.VoucherFieldId equals f.VoucherFieldId
                select new
                {
                    a.VoucherTypeId,
                    a.VoucherAreaId,
                    VoucherAreaTitle = a.Title,

                    af.VoucherAreaFieldId,
                    VoucherAreaFieldTitle = af.Title,
                    f.VoucherFieldId,
                    f.FormTypeId,
                }
                ).ToListAsync();

            var typeFields = areaFields.GroupBy(t => t.VoucherTypeId)
                .ToDictionary(t => t.Key, t => t.Select(f => new VoucherAreaFieldSimpleModel()
                {
                    VoucherAreaId = f.VoucherAreaId,
                    VoucherAreaTitle = f.VoucherAreaTitle,
                    VoucherAreaFieldId = f.VoucherAreaFieldId,
                    VoucherAreaFieldTitle = f.VoucherAreaFieldTitle,
                    VoucherFieldId = f.VoucherFieldId,
                    FormTypeId = (EnumFormType)f.FormTypeId
                }).ToList()
                );

            foreach (var item in voucherTypes)
            {

                if (typeFields.TryGetValue(item.VoucherTypeId, out var _fields))
                {
                    item.AreaFields = _fields;
                }
            }

            return voucherTypes.Cast<VoucherTypeSimpleModel>().ToList();
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

                await UpdateVoucherTableView(voucherType.VoucherTypeId, voucherType.VoucherTypeCode);

                trans.Commit();

                await _objActivityLogFacadeVoucherType.LogBuilder(() => VoucherConfigActivityLogMessage.CreateVoucher)
                   .MessageResourceFormatDatas(voucherType.Title)
                   .ObjectId(voucherType.VoucherTypeId)
                   .JsonData(data)
                   .CreateLog();

                await _roleHelperService.GrantPermissionForAllRoles(EnumModule.SalesBill, EnumObjectType.VoucherType, voucherType.VoucherTypeId);
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
                    AfterSaveAction = sourceVoucherType.AfterSaveAction,
                    AfterUpdateRowsJsAction = sourceVoucherType.AfterUpdateRowsJsAction
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
                            VoucherAreaId = cloneArea.VoucherAreaId,
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
                            Column = field.Column,
                            MouseEnter = field.MouseEnter,
                            MouseLeave = field.MouseLeave,
                            CustomButtonHtml = field.CustomButtonHtml,
                            CustomButtonOnClick = field.CustomButtonOnClick
                        };
                        await _purchaseOrderDBContext.VoucherAreaField.AddAsync(cloneField);
                    }
                }
                await _purchaseOrderDBContext.SaveChangesAsync();

                await UpdateVoucherTableView(cloneType.VoucherTypeId, cloneType.VoucherTypeCode);

                trans.Commit();

                await _objActivityLogFacadeVoucherType.LogBuilder(() => VoucherConfigActivityLogMessage.CreateVoucher)
                   .MessageResourceFormatDatas(cloneType.Title)
                   .ObjectId(cloneType.VoucherTypeId)
                   .JsonData(cloneType)
                   .CreateLog();
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
            var voucherType = await _purchaseOrderDBContext.VoucherType.FirstOrDefaultAsync(i => i.VoucherTypeId == voucherTypeId);
            if (voucherType == null)
            {
                throw new BadRequestException(VoucherErrorCode.VoucherTypeNotFound);
            }

            if (data.UpdatedDatetimeUtc != voucherType.UpdatedDatetimeUtc.GetUnix())
            {
                throw GeneralCode.DataIsOld.BadRequest();
            }

            if (voucherType.VoucherTypeCode != data.VoucherTypeCode || voucherType.Title != data.Title)
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
                await DeleteVoucherTableView(voucherType.VoucherTypeCode);

                voucherType.VoucherTypeCode = data.VoucherTypeCode;
                voucherType.Title = data.Title;
                voucherType.SortOrder = data.SortOrder;
                voucherType.VoucherTypeGroupId = data.VoucherTypeGroupId;
                voucherType.PreLoadAction = data.PreLoadAction;
                voucherType.PostLoadAction = data.PostLoadAction;
                voucherType.AfterLoadAction = data.AfterLoadAction;
                voucherType.BeforeSubmitAction = data.BeforeSubmitAction;
                voucherType.BeforeSaveAction = data.BeforeSaveAction;
                voucherType.AfterSaveAction = data.AfterSaveAction;
                voucherType.AfterUpdateRowsJsAction = data.AfterUpdateRowsJsAction;
                voucherType.IsHide = data.IsHide;

                if (_purchaseOrderDBContext.HasChanges())
                    voucherType.UpdatedDatetimeUtc = DateTime.UtcNow;

                await _purchaseOrderDBContext.SaveChangesAsync();

                await UpdateVoucherTableView(voucherType.VoucherTypeId, voucherType.VoucherTypeCode);

                trans.Commit();

                await _objActivityLogFacadeVoucherType.LogBuilder(() => VoucherConfigActivityLogMessage.UpdateVoucher)
                   .MessageResourceFormatDatas(voucherType.Title)
                   .ObjectId(voucherType.VoucherTypeId)
                   .JsonData(data)
                   .CreateLog();

                return true;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "UpdateVoucherType");
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

            await DeleteVoucherTableView(voucherType.VoucherTypeCode);

            try
            {
                await _voucherActionConfigService.RemoveAllByBillType(voucherTypeId, voucherType.Title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"DeleteActionButtonsByType ({voucherTypeId})");
            }

            await _objActivityLogFacadeVoucherType.LogBuilder(() => VoucherConfigActivityLogMessage.DeleteVoucher)
                   .MessageResourceFormatDatas(voucherType.Title)
                   .ObjectId(voucherType.VoucherTypeId)
                   .JsonData(voucherType)
                   .CreateLog();

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
                    RefTableTitle = f.RefTableTitle,
                    IsRequire = af.IsRequire,
                    decimalPlace = f.DecimalPlace,
                    ReferenceUrlExec = string.IsNullOrWhiteSpace(af.ReferenceUrl) ? f.ReferenceUrl : af.ReferenceUrl,
                    ObjectApprovalStepId = f.ObjectApprovalStepTypeId,
                    VoucherFieldId = f.VoucherFieldId

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

                await _objActivityLogFacadeVoucherType.LogBuilder(() => VoucherConfigActivityLogMessage.CreateVoucherFilter)
                   .MessageResourceFormatDatas(info.VoucherTypeViewName, voucherTypeInfo.Title)
                   .ObjectId(info.VoucherTypeViewId)
                   .JsonData(model)
                   .CreateLog();

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

                await _objActivityLogFacadeVoucherTypeView.LogBuilder(() => VoucherConfigActivityLogMessage.UpdateVoucherFilter)
                   .MessageResourceFormatDatas(info.VoucherTypeViewName, voucherTypeInfo.Title)
                   .ObjectId(info.VoucherTypeViewId)
                   .JsonData(model)
                   .CreateLog();

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

            await _objActivityLogFacadeVoucherTypeView.LogBuilder(() => VoucherConfigActivityLogMessage.DeleteVoucherFilter)
                   .MessageResourceFormatDatas(info.VoucherTypeViewName, voucherTypeInfo.Title)
                   .ObjectId(info.VoucherTypeViewId)
                   .JsonData(new { voucherTypeViewId })
                   .CreateLog();

            return true;

        }
        #endregion InputTypeView

        #region InputTypeGroup
        public async Task<int> VoucherTypeGroupCreate(VoucherTypeGroupModel model)
        {
            var info = _mapper.Map<VoucherTypeGroup>(model);
            await _purchaseOrderDBContext.VoucherTypeGroup.AddAsync(info);
            await _purchaseOrderDBContext.SaveChangesAsync();

            await _objActivityLogFacadeInputTypeGroup.LogBuilder(() => VoucherConfigActivityLogMessage.CreateVoucherGroup)
                   .MessageResourceFormatDatas(info.VoucherTypeGroupName)
                   .ObjectId(info.VoucherTypeGroupId)
                   .JsonData(model)
                   .CreateLog();

            return info.VoucherTypeGroupId;
        }

        public async Task<bool> VoucherTypeGroupUpdate(int voucherTypeGroupId, VoucherTypeGroupModel model)
        {
            var info = await _purchaseOrderDBContext.VoucherTypeGroup.FirstOrDefaultAsync(g => g.VoucherTypeGroupId == voucherTypeGroupId);

            if (info == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Nhóm chứng từ không tồn tại");

            _mapper.Map(model, info);

            await _purchaseOrderDBContext.SaveChangesAsync();

            await _objActivityLogFacadeInputTypeGroup.LogBuilder(() => VoucherConfigActivityLogMessage.UpdateVoucherGroup)
                   .MessageResourceFormatDatas(info.VoucherTypeGroupName)
                   .ObjectId(info.VoucherTypeGroupId)
                   .JsonData(model)
                   .CreateLog();

            return true;
        }

        public async Task<bool> VoucherTypeGroupDelete(int voucherTypeGroupId)
        {
            var info = await _purchaseOrderDBContext.VoucherTypeGroup.FirstOrDefaultAsync(g => g.VoucherTypeGroupId == voucherTypeGroupId);

            if (info == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Nhóm chứng từ không tồn tại");

            info.IsDeleted = true;

            await _purchaseOrderDBContext.SaveChangesAsync();

            await _objActivityLogFacadeInputTypeGroup.LogBuilder(() => VoucherConfigActivityLogMessage.DeleteVoucherGroup)
                  .MessageResourceFormatDatas(info.VoucherTypeGroupName)
                  .ObjectId(info.VoucherTypeGroupId)
                  .JsonData(new { voucherTypeGroupId })
                  .CreateLog();

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
            var query = _purchaseOrderDBContext.VoucherArea.Where(a => a.VoucherTypeId == voucherTypeId).AsQueryable();
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(a => a.VoucherAreaCode.Contains(keyword) || a.Title.Contains(keyword));
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

                await _objActivityLogFacadeVoucherType.LogBuilder(() => VoucherConfigActivityLogMessage.CreateVoucherArea)
                  .MessageResourceFormatDatas(voucherArea.Title)
                  .ObjectId(voucherArea.VoucherAreaId)
                  .JsonData(data)
                  .CreateLog();
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
                voucherArea.ColumnStyles = data.ColumnStyles;
                voucherArea.SortOrder = data.SortOrder;
                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _objActivityLogFacadeVoucherType.LogBuilder(() => VoucherConfigActivityLogMessage.UpdateVoucerArea)
                  .MessageResourceFormatDatas(voucherArea.Title)
                  .ObjectId(voucherArea.VoucherAreaId)
                  .JsonData(data)
                  .CreateLog();
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
            await _objActivityLogFacadeVoucherType.LogBuilder(() => VoucherConfigActivityLogMessage.DeleteVoucherArea)
                  .MessageResourceFormatDatas(voucherArea.Title)
                  .ObjectId(voucherArea.VoucherAreaId)
                  .JsonData(voucherArea)
                  .CreateLog();
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

        public async Task<PageData<VoucherFieldOutputModel>> GetVoucherFields(string keyword, int page, int size, int? objectApprovalStepTypeId)
        {
            keyword = (keyword ?? "").Trim();
            var query = _purchaseOrderDBContext.VoucherField
                .AsQueryable();
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(f => f.FieldName.Contains(keyword) || f.Title.Contains(keyword));
            }

            if (objectApprovalStepTypeId.HasValue)
            {
                query = query.Where(f => f.ObjectApprovalStepTypeId.HasValue && (f.ObjectApprovalStepTypeId & objectApprovalStepTypeId.Value) == objectApprovalStepTypeId.Value);
            }

            var total = await query.CountAsync();

            if (size > 0)
            {
                query = query.Skip((page - 1) * size).Take(size);
            }
            var lst = query.ProjectTo<VoucherFieldOutputModel>(_mapper.ConfigurationProvider).ToList();
            return (lst, total);
        }

        //public async Task<VoucherAreaFieldOutputFullModel> GetVoucherAreaField(int voucherTypeId, int voucherAreaId, int voucherAreaFieldId)
        //{
        //    var voucherAreaField = await _purchaseOrderDBContext.VoucherAreaField
        //        .Where(f => f.VoucherAreaFieldId == voucherAreaFieldId && f.VoucherTypeId == voucherTypeId && f.VoucherAreaId == voucherAreaId)
        //        .Include(f => f.VoucherField)
        //        .ProjectTo<VoucherAreaFieldOutputFullModel>(_mapper.ConfigurationProvider)
        //        .FirstOrDefaultAsync();
        //    if (voucherAreaField == null)
        //    {
        //        throw new BadRequestException(VoucherErrorCode.VoucherAreaFieldNotFound);
        //    }

        //    return voucherAreaField;
        //}

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
            if (!string.IsNullOrEmpty(data.RefTableCode) && !string.IsNullOrEmpty(data.RefTableField))
            {
                var categoryCode = data.RefTableCode;
                var fieldName = data.RefTableField;
                var task = Task.Run(async () => (await _httpCrossService.Post<List<ReferFieldModel>>($"api/internal/InternalCategory/ReferFields", new
                {
                    CategoryCodes = new List<string>() { categoryCode },
                    FieldNames = new List<string>() { fieldName }
                })).FirstOrDefault());
                task.Wait();
                var sourceCategoryField = task.Result;
                if (sourceCategoryField == null)
                {
                    throw new BadRequestException(VoucherErrorCode.SourceCategoryFieldNotFound);
                }
            }
            if (data.DataTypeId == EnumDataType.Text && data.DataSize <= 0 && data.FormTypeId != EnumFormType.DynamicControl)
            {
                throw new BadRequestException(VoucherErrorCode.VoucherFieldDataSizeInValid);
            }

            // Validate decimal size
            if (data.DataTypeId == EnumDataType.Decimal && data.DataSize <= 1)
            {
                throw new BadRequestException(VoucherErrorCode.VoucherFieldDataSizeInValid);
            }
        }

        private void FieldDataProcess(ref VoucherFieldInputModel data)
        {
            if (!string.IsNullOrEmpty(data.RefTableCode) && !string.IsNullOrEmpty(data.RefTableField))
            {
                var categoryCode = data.RefTableCode;
                var fieldName = data.RefTableField;
                var task = Task.Run(async () => (await _httpCrossService.Post<List<ReferFieldModel>>($"api/internal/InternalCategory/ReferFields", new
                {
                    CategoryCodes = new List<string>() { categoryCode },
                    FieldNames = new List<string>() { fieldName }
                })).FirstOrDefault());
                task.Wait();
                var sourceCategoryField = task.Result;
                if (sourceCategoryField != null)
                {
                    data.DataTypeId = (EnumDataType)sourceCategoryField.DataTypeId;
                    data.DataSize = sourceCategoryField.DataSize;
                }
            }

            if (data.FormTypeId == EnumFormType.Generate || data.FormTypeId == EnumFormType.DynamicControl)
            {
                data.DataTypeId = EnumDataType.Text;
                if (data.FormTypeId == EnumFormType.DynamicControl)
                {
                    data.DataSize = -1;
                }
            }

            if (!DataTypeConstants.SELECT_FORM_TYPES.Contains(data.FormTypeId))
            {
                data.RefTableField = null;
                if (data.FormTypeId != EnumFormType.Input)
                {
                    data.RefTableCode = null;
                    data.RefTableTitle = null;
                }
                else if (string.IsNullOrEmpty(data.RefTableCode))
                {
                    data.RefTableTitle = null;
                }
            }
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
                        curField.FiltersName = field.FiltersName;
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
                        curField.RequireFiltersName = field.RequireFiltersName;
                        curField.RequireFilters = field.RequireFilters;
                        curField.ReferenceUrl = field.ReferenceUrl;
                        curField.IsBatchSelect = field.IsBatchSelect;
                        curField.OnClick = field.OnClick;

                        curField.CustomButtonHtml = field.CustomButtonHtml;
                        curField.CustomButtonOnClick = field.CustomButtonOnClick;
                        curField.MouseEnter = field.MouseEnter;
                        curField.MouseLeave = field.MouseLeave;
                    }
                }

                await _purchaseOrderDBContext.SaveChangesAsync();

                // Get list gen code
                var genCodeConfigs = fields
                    .Where(f => f.IdGencode.HasValue)
                    .Select(f => new
                    {
                        VoucherAreaFieldId = f.VoucherAreaFieldId.Value,
                        IdGencode = f.IdGencode.Value
                    })
                    .ToDictionary(c => (long)c.VoucherAreaFieldId, c => c.IdGencode);

                var result = await _customGenCodeHelperService.MapObjectCustomGenCode(EnumObjectType.VoucherTypeRow, EnumObjectType.VoucherAreaField, genCodeConfigs);

                if (!result)
                {
                    trans.TryRollbackTransaction();
                    throw new BadRequestException(VoucherErrorCode.MapGenCodeConfigFail);
                }

                await UpdateVoucherTableView(voucherTypeInfo.VoucherTypeId, voucherTypeInfo.VoucherTypeCode);

                trans.Commit();

                await _objActivityLogFacadeVoucherType.LogBuilder(() => VoucherConfigActivityLogMessage.UpdateVoucherInfo)
                  .MessageResourceFormatDatas(voucherTypeInfo.Title)
                  .ObjectId(voucherTypeId)
                  .JsonData(fields)
                  .CreateLog();

                return true;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "Create");
                throw;
            }
        }

        public async Task<VoucherFieldInputModel> AddVoucherField(VoucherFieldInputModel data)
        {
            FieldDataProcess(ref data);
            ValidateVoucherField(data);
            using var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync();
            try
            {
                var voucherField = _mapper.Map<VoucherField>(data);

                await _purchaseOrderDBContext.VoucherField.AddAsync(voucherField);
                await _purchaseOrderDBContext.SaveChangesAsync();

                if (voucherField.FormTypeId != (int)EnumFormType.ViewOnly && voucherField.FormTypeId != (int)EnumFormType.SqlSelect)
                {
                    await _purchaseOrderDBContext.AddColumn(VOUCHERVALUEROW_TABLE, data.FieldName, data.DataTypeId, data.DataSize, data.DecimalPlace, data.DefaultValue, true);
                }
                await UpdateVoucherValueView();
                await UpdateVoucherTableType();
                trans.Commit();

                await _objActivityLogFacadeVoucherType.LogBuilder(() => VoucherConfigActivityLogMessage.CreateVoucherInfo)
                  .MessageResourceFormatDatas(voucherField.Title)
                  .ObjectId(voucherField.VoucherFieldId)
                  .JsonData(data)
                  .CreateLog();

                return data;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "Create");
                throw;
            }
        }

        public async Task<VoucherFieldInputModel> UpdateVoucherField(int voucherFieldId, VoucherFieldInputModel data)
        {
            var voucherField = await _purchaseOrderDBContext.VoucherField.FirstOrDefaultAsync(f => f.VoucherFieldId == voucherFieldId);
            FieldDataProcess(ref data);
            ValidateVoucherField(data, voucherField, voucherFieldId);

            using var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync();
            try
            {
                if (voucherField.FormTypeId != (int)EnumFormType.ViewOnly && voucherField.FormTypeId != (int)EnumFormType.SqlSelect)
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
                await UpdateVoucherTableType();

                trans.Commit();

                await _objActivityLogFacadeVoucherType.LogBuilder(() => VoucherConfigActivityLogMessage.UpdateVoucherField)
                  .MessageResourceFormatDatas(voucherField.Title)
                  .ObjectId(voucherField.VoucherFieldId)
                  .JsonData(data)
                  .CreateLog();

                return data;
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
                if (voucherField.FormTypeId != (int)EnumFormType.ViewOnly && voucherField.FormTypeId != (int)EnumFormType.SqlSelect)
                {
                    await _purchaseOrderDBContext.DropColumn(VOUCHERVALUEROW_TABLE, voucherField.FieldName);
                }
                await UpdateVoucherValueView();
                await UpdateVoucherTableType();

                trans.Commit();

                await _objActivityLogFacadeVoucherType.LogBuilder(() => VoucherConfigActivityLogMessage.DeleteVoucherField)
                  .MessageResourceFormatDatas(voucherField.Title)
                  .ObjectId(voucherField.VoucherFieldId)
                  .JsonData(voucherField)
                  .CreateLog();

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

            var types = await _purchaseOrderDBContext.VoucherType.ToListAsync();
            foreach (var type in types)
            {
                await UpdateVoucherTableView(type.VoucherTypeId, type.VoucherTypeCode);
            }
        }

        private async Task UpdateVoucherTableType()
        {
            await _purchaseOrderDBContext.ExecuteStoreProcedure("asp_UpdateVoucherTableType", Array.Empty<SqlParameter>());
        }

        private async Task UpdateVoucherTableView(int voucherTypeId, string voucherTypeCode)
        {
            var viewName = PurchaseOrderConstants.VoucherTypeView(voucherTypeCode);

            await _purchaseOrderDBContext.ExecuteStoreProcedure("asp_VoucherType_UpdateView", new[] {
                new SqlParameter("@VoucherTypeId", voucherTypeId) ,
                new SqlParameter("@ViewName", viewName)
            });
        }

        private async Task DeleteVoucherTableView(string voucherTypeCode)
        {
            var viewName = PurchaseOrderConstants.VoucherTypeView(voucherTypeCode);
            await _purchaseOrderDBContext.Database.ExecuteSqlRawAsync($"DROP VIEW IF EXISTS {viewName}", new SqlParameter("@ViewName", viewName));
        }
    }
}

