﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Verp.Cache.RedisCache;
using VErp.Commons.Constants;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.AccountancyDB;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Accountancy.Model.Input;

namespace VErp.Services.Accountancy.Service.Input.Implement
{
    public class InputConfigService : IInputConfigService
    {
        private const string INPUTVALUEROW_TABLE = AccountantConstants.INPUTVALUEROW_TABLE;

        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly IMapper _mapper;
        private readonly AccountancyDBContext _accountancyDBContext;
        private readonly ICustomGenCodeHelperService _customGenCodeHelperService;
        private readonly IMenuHelperService _menuHelperService;
        private readonly ICurrentContextService _currentContextService;
        private readonly ICategoryHelperService _httpCategoryHelperService;
        private readonly IRoleHelperService _roleHelperService;
        private readonly IInputActionConfigService _inputActionConfigService;

        public InputConfigService(AccountancyDBContext accountancyDBContext
            , IOptions<AppSetting> appSetting
            , ILogger<InputConfigService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            , ICustomGenCodeHelperService customGenCodeHelperService
            , IMenuHelperService menuHelperService
            , ICurrentContextService currentContextService
            , ICategoryHelperService httpCategoryHelperService
            , IRoleHelperService roleHelperService
            , IInputActionConfigService inputActionConfigService
            )
        {
            _accountancyDBContext = accountancyDBContext;
            _logger = logger;
            _activityLogService = activityLogService;
            _mapper = mapper;
            _customGenCodeHelperService = customGenCodeHelperService;
            _menuHelperService = menuHelperService;
            _currentContextService = currentContextService;
            _httpCategoryHelperService = httpCategoryHelperService;
            _roleHelperService = roleHelperService;
            _inputActionConfigService = inputActionConfigService;
        }

        #region InputType

        public async Task<InputTypeFullModel> GetInputType(int inputTypeId)
        {
            var globalSetting = await GetInputGlobalSetting();

            var inputType = await _accountancyDBContext.InputType
           .Where(i => i.InputTypeId == inputTypeId)
           .Include(t => t.InputArea)
           .ThenInclude(a => a.InputAreaField)
           .ThenInclude(af => af.InputField)
           .Include(t => t.InputArea)
           .ThenInclude(a => a.InputAreaField)
           .ThenInclude(af => af.InputField)
           .ProjectTo<InputTypeFullModel>(_mapper.ConfigurationProvider)
           .FirstOrDefaultAsync();
            if (inputType == null)
            {
                throw new BadRequestException(InputErrorCode.InputTypeNotFound);
            }
            inputType.InputAreas = inputType.InputAreas.OrderBy(f => f.SortOrder).ToList();
            foreach (var item in inputType.InputAreas)
            {
                item.InputAreaFields = item.InputAreaFields.OrderBy(f => f.SortOrder).ToList();
            }

            inputType.GlobalSetting = globalSetting;
            return inputType;
        }


        public async Task<IList<InputTypeFullModel>> GetAllInputTypes()
        {
            var globalSetting = await GetInputGlobalSetting();

            var lst = await _accountancyDBContext.InputType
           .Include(t => t.InputArea)
           .ThenInclude(a => a.InputAreaField)
           .ThenInclude(af => af.InputField)
           .Include(t => t.InputArea)
           .ThenInclude(a => a.InputAreaField)
           .ThenInclude(af => af.InputField)
           .ProjectTo<InputTypeFullModel>(_mapper.ConfigurationProvider)
           .ToListAsync();
            foreach (var item in lst)
            {
                item.GlobalSetting = globalSetting;
            }
            return lst;
        }

        public async Task<InputTypeFullModel> GetInputType(string inputTypeCode)
        {
            var globalSetting = await GetInputGlobalSetting();

            var inputType = await _accountancyDBContext.InputType
           .Where(i => i.InputTypeCode == inputTypeCode)
           .Include(t => t.InputArea)
           .ThenInclude(a => a.InputAreaField)
           .ThenInclude(af => af.InputField)
           .Include(t => t.InputArea)
           .ThenInclude(a => a.InputAreaField)
           .ThenInclude(af => af.InputField)
           .ProjectTo<InputTypeFullModel>(_mapper.ConfigurationProvider)
           .FirstOrDefaultAsync();
            if (inputType == null)
            {
                throw new BadRequestException(InputErrorCode.InputTypeNotFound);
            }

            inputType.GlobalSetting = globalSetting;
            return inputType;
        }

        public async Task<PageData<InputTypeModel>> GetInputTypes(string keyword, int page, int size)
        {
            var globalSetting = await GetInputGlobalSetting();
            keyword = (keyword ?? "").Trim();

            var query = _accountancyDBContext.InputType.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(i => i.InputTypeCode.Contains(keyword) || i.Title.Contains(keyword));
            }

            query = query.OrderBy(c => c.Title);
            var total = await query.CountAsync();
            if (size > 0)
            {
                query = query.Skip((page - 1) * size).Take(size);
            }
            var lst = await query.ProjectTo<InputTypeModel>(_mapper.ConfigurationProvider).OrderBy(t => t.SortOrder).ToListAsync();

            return (lst, total);
        }

        public async Task<IList<InputTypeSimpleModel>> GetInputTypeSimpleList()
        {
            var inputTypes = await _accountancyDBContext.InputType.Where(x => !x.IsHide).ProjectTo<InputTypeSimpleProjectMappingModel>(_mapper.ConfigurationProvider).OrderBy(t => t.SortOrder).ToListAsync();

            var areaFields = await (
              from a in _accountancyDBContext.InputArea
              join af in _accountancyDBContext.InputAreaField on a.InputAreaId equals af.InputAreaId
              join f in _accountancyDBContext.InputField on af.InputFieldId equals f.InputFieldId
              select new
              {
                  a.InputTypeId,
                  a.InputAreaId,
                  InputAreaTitle = a.Title,

                  af.InputAreaFieldId,
                  InputAreaFieldTitle = af.Title,
                  f.InputFieldId,
                  f.FormTypeId,
              }
              ).ToListAsync();

            var typeFields = areaFields.GroupBy(t => t.InputTypeId)
                .ToDictionary(t => t.Key, t => t.Select(f => new InputAreaFieldSimpleModel()
                {
                    InputAreaId = f.InputAreaId,
                    InputAreaTitle = f.InputAreaTitle,
                    InputAreaFieldId = f.InputAreaFieldId,
                    InputAreaFieldTitle = f.InputAreaFieldTitle,
                    InputFieldId = f.InputFieldId,
                    FormTypeId = (EnumFormType)f.FormTypeId
                }).ToList()
                );

            foreach (var item in inputTypes)
            {


                if (typeFields.TryGetValue(item.InputTypeId, out var _fields))
                {
                    item.AreaFields = _fields;
                }
            }

            return inputTypes.Cast<InputTypeSimpleModel>().ToList();
        }

        public async Task<int> AddInputType(InputTypeModel data)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockInputTypeKey(0));
            var existedInput = await _accountancyDBContext.InputType
                .FirstOrDefaultAsync(i => i.InputTypeCode == data.InputTypeCode || i.Title == data.Title);
            if (existedInput != null)
            {
                if (string.Compare(existedInput.InputTypeCode, data.InputTypeCode, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    throw new BadRequestException(InputErrorCode.InputCodeAlreadyExisted);
                }

                throw new BadRequestException(InputErrorCode.InputTitleAlreadyExisted);
            }

            using var trans = await _accountancyDBContext.Database.BeginTransactionAsync();
            try
            {
                InputType inputType = _mapper.Map<InputType>(data);
                await _accountancyDBContext.InputType.AddAsync(inputType);
                await _accountancyDBContext.SaveChangesAsync();

                trans.Commit();
                await _activityLogService.CreateLog(EnumObjectType.InputType, inputType.InputTypeId, $"Thêm chứng từ {inputType.Title}", data.JsonSerialize());

                //if (data.MenuStyle != null)
                //{
                //    var url = Utils.FormatStyle(data.MenuStyle.UrlFormat, data.InputTypeCode, inputType.InputTypeId);
                //    var param = Utils.FormatStyle(data.MenuStyle.ParamFormat, data.InputTypeCode, inputType.InputTypeId);
                //    await _menuHelperService.CreateMenu(data.MenuStyle.ParentId, false, data.MenuStyle.ModuleId, data.MenuStyle.MenuName, url, param, data.MenuStyle.Icon, data.MenuStyle.SortOrder, data.MenuStyle.IsDisabled);
                //}

                await _roleHelperService.GrantPermissionForAllRoles(EnumModule.Input, EnumObjectType.InputType, inputType.InputTypeId);
                return inputType.InputTypeId;
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
            if (_accountancyDBContext.InputType.Any(i => i.InputTypeCode == code))
            {
                suffix++;
                code = GetStringClone(source, suffix);
            }
            return code;
        }

        public async Task<int> CloneInputType(int inputTypeId)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockInputTypeKey(0));
            var sourceInput = await _accountancyDBContext.InputType
                .Include(i => i.InputArea)
                .Include(a => a.InputAreaField)
                .FirstOrDefaultAsync(i => i.InputTypeId == inputTypeId);
            if (sourceInput == null)
            {
                throw new BadRequestException(InputErrorCode.SourceInputTypeNotFound);
            }

            using var trans = await _accountancyDBContext.Database.BeginTransactionAsync();
            try
            {
                var cloneType = new InputType
                {
                    InputTypeCode = GetStringClone(sourceInput.InputTypeCode),
                    Title = GetStringClone(sourceInput.Title),
                    InputTypeGroupId = sourceInput.InputTypeGroupId,
                    SortOrder = sourceInput.SortOrder,
                    PreLoadAction = sourceInput.PreLoadAction,
                    PostLoadAction = sourceInput.PostLoadAction,
                    AfterLoadAction = sourceInput.AfterLoadAction,
                    BeforeSubmitAction = sourceInput.BeforeSubmitAction,
                    BeforeSaveAction = sourceInput.BeforeSaveAction,
                    AfterSaveAction = sourceInput.AfterSaveAction,
                    AfterUpdateRowsJsAction = sourceInput.AfterUpdateRowsJsAction,
                    IsOpenning = sourceInput.IsOpenning
                };
                await _accountancyDBContext.InputType.AddAsync(cloneType);
                await _accountancyDBContext.SaveChangesAsync();

                foreach (var area in sourceInput.InputArea)
                {
                    var cloneArea = new InputArea
                    {
                        InputTypeId = cloneType.InputTypeId,
                        InputAreaCode = area.InputAreaCode,
                        Title = area.Title,
                        Description = area.Description,
                        IsMultiRow = area.IsMultiRow,
                        IsAddition = area.IsAddition,
                        Columns = area.Columns,
                        SortOrder = area.SortOrder
                    };
                    await _accountancyDBContext.InputArea.AddAsync(cloneArea);
                    await _accountancyDBContext.SaveChangesAsync();

                    foreach (var field in sourceInput.InputAreaField.Where(f => f.InputAreaId == area.InputAreaId).ToList())
                    {
                        var cloneField = new InputAreaField
                        {
                            InputFieldId = field.InputFieldId,
                            InputTypeId = cloneType.InputTypeId,
                            InputAreaId = cloneArea.InputAreaId,
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
                            CustomButtonHtml = field.CustomButtonHtml,
                            CustomButtonOnClick = field.CustomButtonOnClick,
                            MouseEnter = field.MouseEnter,
                            MouseLeave = field.MouseLeave
                        };
                        await _accountancyDBContext.InputAreaField.AddAsync(cloneField);
                    }
                }
                await _accountancyDBContext.SaveChangesAsync();
                trans.Commit();

                //if (menuStyle != null)
                //{
                //    var url = Utils.FormatStyle(menuStyle.UrlFormat, cloneType.InputTypeCode, cloneType.InputTypeId);
                //    var param = Utils.FormatStyle(menuStyle.ParamFormat, cloneType.InputTypeCode, cloneType.InputTypeId);
                //    await _menuHelperService.CreateMenu(menuStyle.ParentId, false, menuStyle.ModuleId, menuStyle.MenuName, url, param, menuStyle.Icon, menuStyle.SortOrder, menuStyle.IsDisabled);
                //}

                await _activityLogService.CreateLog(EnumObjectType.InputType, cloneType.InputTypeId, $"Thêm chứng từ {cloneType.Title}", cloneType.JsonSerialize());
                return cloneType.InputTypeId;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "Create");
                throw;
            }
        }

        public async Task<bool> UpdateInputType(int inputTypeId, InputTypeModel data)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockInputTypeKey(inputTypeId));
            var inputType = await _accountancyDBContext.InputType.FirstOrDefaultAsync(i => i.InputTypeId == inputTypeId);
            if (inputType == null)
            {
                throw new BadRequestException(InputErrorCode.InputTypeNotFound);
            }
            if (inputType.InputTypeCode != data.InputTypeCode || inputType.Title != data.Title)
            {
                var existedInput = await _accountancyDBContext.InputType
                    .FirstOrDefaultAsync(i => i.InputTypeId != inputTypeId && (i.InputTypeCode == data.InputTypeCode || i.Title == data.Title));
                if (existedInput != null)
                {
                    if (string.Compare(existedInput.InputTypeCode, data.InputTypeCode, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        throw new BadRequestException(InputErrorCode.InputCodeAlreadyExisted);
                    }

                    throw new BadRequestException(InputErrorCode.InputTitleAlreadyExisted);
                }
            }

            using var trans = await _accountancyDBContext.Database.BeginTransactionAsync();
            try
            {
                inputType.InputTypeCode = data.InputTypeCode;
                inputType.Title = data.Title;
                inputType.SortOrder = data.SortOrder;
                inputType.InputTypeGroupId = data.InputTypeGroupId;
                inputType.PreLoadAction = data.PreLoadAction;
                inputType.PostLoadAction = data.PostLoadAction;
                inputType.AfterLoadAction = data.AfterLoadAction;
                inputType.BeforeSubmitAction = data.BeforeSubmitAction;
                inputType.BeforeSaveAction = data.BeforeSaveAction;
                inputType.AfterSaveAction = data.AfterSaveAction;
                inputType.AfterUpdateRowsJsAction = data.AfterUpdateRowsJsAction;
                inputType.IsOpenning = data.IsOpenning;
                inputType.IsHide = data.IsHide;
                await _accountancyDBContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.InputType, inputType.InputTypeId, $"Cập nhật chứng từ {inputType.Title}", data.JsonSerialize());
                return true;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "Update");
                throw;
            }
        }

        public async Task<bool> DeleteInputType(int inputTypeId)
        {
            var inputType = await _accountancyDBContext.InputType.FirstOrDefaultAsync(i => i.InputTypeId == inputTypeId);
            if (inputType == null)
            {
                throw new BadRequestException(InputErrorCode.InputTypeNotFound);
            }

            await _accountancyDBContext.ExecuteStoreProcedure("asp_InputType_Delete", new[] {
                    new SqlParameter("@InputTypeId",inputTypeId ),
                    new SqlParameter("@ResStatus",0){ Direction = ParameterDirection.Output },
                    });

            try
            {
                await _inputActionConfigService.RemoveAllByBillType(inputTypeId, inputType.Title);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"DeleteActionButtonsByType ({inputTypeId})");
            }
            await _activityLogService.CreateLog(EnumObjectType.InputType, inputType.InputTypeId, $"Xóa chứng từ {inputType.Title}", inputType.JsonSerialize());
            return true;
        }

        public async Task<InputTypeGlobalSettingModel> GetInputGlobalSetting()
        {
            var inputTypeSetting = await _accountancyDBContext.InputTypeGlobalSetting.FirstOrDefaultAsync();
            if (inputTypeSetting == null)
            {
                inputTypeSetting = new InputTypeGlobalSetting();
            }

            return _mapper.Map<InputTypeGlobalSettingModel>(inputTypeSetting);
        }

        public async Task<bool> UpdateInputGlobalSetting(InputTypeGlobalSettingModel data)
        {
            var inputTypeSetting = await _accountancyDBContext.InputTypeGlobalSetting.FirstOrDefaultAsync();
            if (inputTypeSetting == null)
            {
                inputTypeSetting = new InputTypeGlobalSetting();
            }

            using var trans = await _accountancyDBContext.Database.BeginTransactionAsync();
            try
            {
                _mapper.Map(data, inputTypeSetting);
                if (inputTypeSetting.InputTypeGlobalSettingId <= 0)
                {
                    _accountancyDBContext.InputTypeGlobalSetting.Add(inputTypeSetting);
                }

                await _accountancyDBContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.InputType, 0, $"Cập nhật cấu hình chung chứng từ kế toán", data.JsonSerialize());
                return true;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "UpdateInputSetting");
                throw;
            }
        }

        #region InputTypeView
        public async Task<IList<InputTypeViewModelList>> InputTypeViewList(int inputTypeId)
        {
            return await _accountancyDBContext.InputTypeView.Where(v => v.InputTypeId == inputTypeId).ProjectTo<InputTypeViewModelList>(_mapper.ConfigurationProvider).ToListAsync();
        }

        public async Task<InputTypeBasicOutput> GetInputTypeBasicInfo(int inputTypeId)
        {
            var inputTypeInfo = await _accountancyDBContext.InputType.AsNoTracking().Where(t => t.InputTypeId == inputTypeId).ProjectTo<InputTypeBasicOutput>(_mapper.ConfigurationProvider).FirstOrDefaultAsync();

            inputTypeInfo.Areas = await _accountancyDBContext.InputArea.AsNoTracking().Where(a => a.InputTypeId == inputTypeId).OrderBy(a => a.SortOrder).ProjectTo<InputAreaBasicOutput>(_mapper.ConfigurationProvider).ToListAsync();

            var fields = await (
                from af in _accountancyDBContext.InputAreaField
                join f in _accountancyDBContext.InputField on af.InputFieldId equals f.InputFieldId
                where af.InputTypeId == inputTypeId
                orderby af.SortOrder
                select new InputAreaFieldBasicOutput
                {
                    InputAreaId = af.InputAreaId,
                    InputAreaFieldId = af.InputAreaFieldId,
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
                    DecimalPlace = f.DecimalPlace,
                    ReferenceUrlExec = string.IsNullOrWhiteSpace(af.ReferenceUrl) ? f.ReferenceUrl : af.ReferenceUrl,
                    InputFieldId = f.InputFieldId,
                    ObjectApprovalStepId = f.ObjectApprovalStepTypeId

                }).ToListAsync();

            var views = await _accountancyDBContext.InputTypeView.AsNoTracking().Where(t => t.InputTypeId == inputTypeId).OrderByDescending(v => v.IsDefault).ProjectTo<InputTypeViewModelList>(_mapper.ConfigurationProvider).ToListAsync();

            foreach (var item in inputTypeInfo.Areas)
            {
                item.Fields = fields.Where(f => f.InputAreaId == item.InputAreaId).ToList();
            }

            inputTypeInfo.Views = views;

            return inputTypeInfo;
        }

        public async Task<InputTypeViewModel> GetInputTypeViewInfo(int inputTypeId, int inputTypeViewId)
        {
            var info = await _accountancyDBContext.InputTypeView.AsNoTracking().Where(t => t.InputTypeId == inputTypeId && t.InputTypeViewId == inputTypeViewId).ProjectTo<InputTypeViewModel>(_mapper.ConfigurationProvider).FirstOrDefaultAsync();

            if (info == null)
            {
                throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy cấu hình trong hệ thống");
            }

            var fields = await _accountancyDBContext.InputTypeViewField.AsNoTracking()
                .Where(t => t.InputTypeViewId == inputTypeViewId)
                .ProjectTo<InputTypeViewFieldModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

            info.Fields = fields;

            return info;
        }

        public async Task<int> InputTypeViewCreate(int inputTypeId, InputTypeViewModel model)
        {
            using var trans = await _accountancyDBContext.Database.BeginTransactionAsync();
            try
            {
                var inputTypeInfo = await _accountancyDBContext.InputType.FirstOrDefaultAsync(t => t.InputTypeId == inputTypeId);
                if (inputTypeInfo == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy loại chứng từ");

                var info = _mapper.Map<InputTypeView>(model);

                info.InputTypeId = inputTypeId;

                await _accountancyDBContext.InputTypeView.AddAsync(info);
                await _accountancyDBContext.SaveChangesAsync();

                await InputTypeViewFieldAddRange(info.InputTypeViewId, model.Fields);

                await _accountancyDBContext.SaveChangesAsync();

                await trans.CommitAsync();

                await _activityLogService.CreateLog(EnumObjectType.InputTypeView, info.InputTypeViewId, $"Tạo bộ lọc {info.InputTypeViewName} cho chứng từ  {inputTypeInfo.Title}", model.JsonSerialize());

                return info.InputTypeViewId;
            }
            catch (Exception ex)
            {
                await trans.TryRollbackTransactionAsync();
                _logger.LogError(ex, "InputTypeViewCreate");
                throw;
            }
        }

        public async Task<bool> InputTypeViewUpdate(int inputTypeViewId, InputTypeViewModel model)
        {
            using var trans = await _accountancyDBContext.Database.BeginTransactionAsync();
            try
            {

                var info = await _accountancyDBContext.InputTypeView.FirstOrDefaultAsync(v => v.InputTypeViewId == inputTypeViewId);

                if (info == null) throw new BadRequestException(GeneralCode.ItemNotFound, "View không tồn tại");

                var inputTypeInfo = await _accountancyDBContext.InputType.FirstOrDefaultAsync(t => t.InputTypeId == info.InputTypeId);
                if (inputTypeInfo == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy loại chứng từ");


                _mapper.Map(model, info);

                var oldFields = await _accountancyDBContext.InputTypeViewField.Where(f => f.InputTypeViewId == inputTypeViewId).ToListAsync();

                _accountancyDBContext.InputTypeViewField.RemoveRange(oldFields);

                await InputTypeViewFieldAddRange(inputTypeViewId, model.Fields);

                await _accountancyDBContext.SaveChangesAsync();

                await trans.CommitAsync();

                await _activityLogService.CreateLog(EnumObjectType.InputTypeView, info.InputTypeViewId, $"Cập nhật bộ lọc {info.InputTypeViewName} cho chứng từ  {inputTypeInfo.Title}", model.JsonSerialize());

                return true;
            }
            catch (Exception ex)
            {
                await trans.TryRollbackTransactionAsync();
                _logger.LogError(ex, "InputTypeViewUpdate");
                throw;
            }
        }

        public async Task<bool> InputTypeViewDelete(int inputTypeViewId)
        {
            var info = await _accountancyDBContext.InputTypeView.FirstOrDefaultAsync(v => v.InputTypeViewId == inputTypeViewId);

            if (info == null) throw new BadRequestException(GeneralCode.ItemNotFound, "View không tồn tại");

            info.IsDeleted = true;
            info.DeletedDatetimeUtc = DateTime.UtcNow;


            var inputTypeInfo = await _accountancyDBContext.InputType.FirstOrDefaultAsync(t => t.InputTypeId == info.InputTypeId);
            if (inputTypeInfo == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy loại chứng từ");


            await _accountancyDBContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.InputTypeView, info.InputTypeViewId, $"Xóa bộ lọc {info.InputTypeViewName} chứng từ  {inputTypeInfo.Title}", new { inputTypeViewId }.JsonSerialize());

            return true;

        }
        #endregion InputTypeView

        #region InputTypeGroup
        public async Task<int> InputTypeGroupCreate(InputTypeGroupModel model)
        {
            var info = _mapper.Map<InputTypeGroup>(model);
            await _accountancyDBContext.InputTypeGroup.AddAsync(info);
            await _accountancyDBContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.InputTypeGroup, info.InputTypeGroupId, $"Thêm nhóm chứng từ {info.InputTypeGroupName}", model.JsonSerialize());

            return info.InputTypeGroupId;
        }

        public async Task<bool> InputTypeGroupUpdate(int inputTypeGroupId, InputTypeGroupModel model)
        {
            var info = await _accountancyDBContext.InputTypeGroup.FirstOrDefaultAsync(g => g.InputTypeGroupId == inputTypeGroupId);

            if (info == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Nhóm chứng từ không tồn tại");

            _mapper.Map(model, info);

            await _accountancyDBContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.InputTypeGroup, info.InputTypeGroupId, $"Cập nhật nhóm chứng từ {info.InputTypeGroupName}", model.JsonSerialize());

            return true;
        }

        public async Task<bool> InputTypeGroupDelete(int inputTypeGroupId)
        {
            var info = await _accountancyDBContext.InputTypeGroup.FirstOrDefaultAsync(g => g.InputTypeGroupId == inputTypeGroupId);

            if (info == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Nhóm chứng từ không tồn tại");

            info.IsDeleted = true;

            await _accountancyDBContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.InputTypeGroup, info.InputTypeGroupId, $"Xóa nhóm chứng từ {info.InputTypeGroupName}", new { inputTypeGroupId }.JsonSerialize());

            return true;
        }

        public async Task<IList<InputTypeGroupList>> InputTypeGroupList()
        {
            return await _accountancyDBContext.InputTypeGroup.ProjectTo<InputTypeGroupList>(_mapper.ConfigurationProvider).OrderBy(g => g.SortOrder).ToListAsync();
        }

        #endregion


        private async Task InputTypeViewFieldAddRange(int inputTypeViewId, IList<InputTypeViewFieldModel> fieldModels)
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

            var fields = fieldModels.Select(f => _mapper.Map<InputTypeViewField>(f)).ToList();

            foreach (var f in fields)
            {
                f.InputTypeViewId = inputTypeViewId;
            }

            await _accountancyDBContext.InputTypeViewField.AddRangeAsync(fields);

        }

        #endregion

        #region Area

        public async Task<InputAreaModel> GetInputArea(int inputTypeId, int inputAreaId)
        {
            var inputArea = await _accountancyDBContext.InputArea
                .Where(i => i.InputTypeId == inputTypeId && i.InputAreaId == inputAreaId)
                .ProjectTo<InputAreaModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
            if (inputArea == null)
            {
                throw new BadRequestException(InputErrorCode.InputTypeNotFound);
            }
            return inputArea;
        }

        public async Task<PageData<InputAreaModel>> GetInputAreas(int inputTypeId, string keyword, int page, int size)
        {
            keyword = (keyword ?? "").Trim();
            var query = _accountancyDBContext.InputArea.Where(a => a.InputTypeId == inputTypeId).AsQueryable();
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(a => a.InputAreaCode.Contains(keyword) || a.Title.Contains(keyword));
            }
            query = query.OrderBy(c => c.Title);
            var total = await query.CountAsync();
            if (size > 0)
            {
                query = query.Skip((page - 1) * size).Take(size);
            }
            var lst = await query.ProjectTo<InputAreaModel>(_mapper.ConfigurationProvider).OrderBy(a => a.SortOrder).ToListAsync();
            return (lst, total);
        }

        public async Task<int> AddInputArea(int inputTypeId, InputAreaInputModel data)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockInputTypeKey(inputTypeId));
            var existedInput = await _accountancyDBContext.InputArea
                .FirstOrDefaultAsync(a => a.InputTypeId == inputTypeId && (a.InputAreaCode == data.InputAreaCode || a.Title == data.Title));
            if (existedInput != null)
            {
                if (string.Compare(existedInput.InputAreaCode, data.InputAreaCode, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    throw new BadRequestException(InputErrorCode.InputCodeAlreadyExisted);
                }

                throw new BadRequestException(InputErrorCode.InputTitleAlreadyExisted);
            }

            if (data.IsMultiRow && _accountancyDBContext.InputArea.Any(a => a.InputTypeId == inputTypeId && a.IsMultiRow))
            {
                throw new BadRequestException(InputErrorCode.MultiRowAreaAlreadyExisted);
            }


            using var trans = await _accountancyDBContext.Database.BeginTransactionAsync();
            try
            {
                InputArea inputArea = _mapper.Map<InputArea>(data);
                inputArea.InputTypeId = inputTypeId;
                await _accountancyDBContext.InputArea.AddAsync(inputArea);
                await _accountancyDBContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.InputType, inputArea.InputAreaId, $"Thêm vùng thông tin {inputArea.Title}", data.JsonSerialize());
                return inputArea.InputAreaId;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "Create");
                throw;
            }
        }

        public async Task<bool> UpdateInputArea(int inputTypeId, int inputAreaId, InputAreaInputModel data)
        {
            data.InputTypeId = inputTypeId;
            data.InputAreaId = inputAreaId;

            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockInputTypeKey(inputTypeId));
            var inputArea = await _accountancyDBContext.InputArea.FirstOrDefaultAsync(a => a.InputTypeId == inputTypeId && a.InputAreaId == inputAreaId);
            if (inputArea == null)
            {
                throw new BadRequestException(InputErrorCode.InputAreaNotFound);
            }
            if (inputArea.InputAreaCode != data.InputAreaCode || inputArea.Title != data.Title)
            {
                var existedInput = await _accountancyDBContext.InputArea
                    .FirstOrDefaultAsync(a => a.InputTypeId == inputTypeId && a.InputAreaId != inputAreaId && (a.InputAreaCode == data.InputAreaCode || a.Title == data.Title));
                if (existedInput != null)
                {
                    if (string.Compare(existedInput.InputAreaCode, data.InputAreaCode, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        throw new BadRequestException(InputErrorCode.InputAreaCodeAlreadyExisted);
                    }

                    throw new BadRequestException(InputErrorCode.InputAreaTitleAlreadyExisted);
                }
            }
            if (data.IsMultiRow && _accountancyDBContext.InputArea.Any(a => a.InputTypeId == inputTypeId && a.InputAreaId != inputAreaId && a.IsMultiRow))
            {
                throw new BadRequestException(InputErrorCode.MultiRowAreaAlreadyExisted);
            }

            using var trans = await _accountancyDBContext.Database.BeginTransactionAsync();
            try
            {
                //inputArea.InputAreaCode = data.InputAreaCode;
                //inputArea.Title = data.Title;
                //inputArea.Description = data.Description;
                //inputArea.IsMultiRow = data.IsMultiRow;
                //inputArea.IsAddition = data.IsAddition;
                //inputArea.Columns = data.Columns;
                //inputArea.ColumnStyles = data.ColumnStyles;
                //inputArea.SortOrder = data.SortOrder;
                _mapper.Map(data, inputArea);
                await _accountancyDBContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.InputType, inputArea.InputAreaId, $"Cập nhật vùng dữ liệu {inputArea.Title}", data.JsonSerialize());
                return true;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "Update");
                throw;
            }
        }

        public async Task<bool> DeleteInputArea(int inputTypeId, int inputAreaId)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockInputTypeKey(inputTypeId));
            var inputArea = await _accountancyDBContext.InputArea.FirstOrDefaultAsync(a => a.InputTypeId == inputTypeId && a.InputAreaId == inputAreaId);
            if (inputArea == null)
            {
                throw new BadRequestException(InputErrorCode.InputAreaNotFound);
            }

            await _accountancyDBContext.ExecuteStoreProcedure("asp_InputArea_Delete", new[] {
                    new SqlParameter("@InputTypeId",inputTypeId ),
                    new SqlParameter("@InputAreaId",inputAreaId ),
                    new SqlParameter("@ResStatus",0){ Direction = ParameterDirection.Output },
                    });

            inputArea.IsDeleted = true;
            await _accountancyDBContext.SaveChangesAsync();
            await _activityLogService.CreateLog(EnumObjectType.InputType, inputArea.InputTypeId, $"Xóa vùng chứng từ {inputArea.Title}", inputArea.JsonSerialize());
            return true;
        }

        #endregion

        #region Field

        public async Task<PageData<InputAreaFieldOutputFullModel>> GetInputAreaFields(int inputTypeId, int inputAreaId, string keyword, int page, int size)
        {
            keyword = (keyword ?? "").Trim();
            var query = _accountancyDBContext.InputAreaField
                .Include(f => f.InputField)
                .Where(f => f.InputTypeId == inputTypeId && f.InputAreaId == inputAreaId);
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
            var lst = await query.ProjectTo<InputAreaFieldOutputFullModel>(_mapper.ConfigurationProvider).ToListAsync();
            return (lst, total);
        }

        public async Task<PageData<InputFieldOutputModel>> GetInputFields(string keyword, int page, int size, int? objectApprovalStepTypeId)
        {
            keyword = (keyword ?? "").Trim();
            var query = _accountancyDBContext.InputField
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

            query = query.OrderBy(f => f.SortOrder);
            if (size > 0)
            {
                query = query.Skip((page - 1) * size).Take(size);
            }
            var lst = query.ProjectTo<InputFieldOutputModel>(_mapper.ConfigurationProvider).ToList();
            return (lst, total);
        }

        public async Task<InputAreaFieldOutputFullModel> GetInputAreaField(int inputTypeId, int inputAreaId, int inputAreaFieldId)
        {
            var inputAreaField = await _accountancyDBContext.InputAreaField
                .Where(f => f.InputAreaFieldId == inputAreaFieldId && f.InputTypeId == inputTypeId && f.InputAreaId == inputAreaId)
                .Include(f => f.InputField)
                .ProjectTo<InputAreaFieldOutputFullModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
            if (inputAreaField == null)
            {
                throw new BadRequestException(InputErrorCode.InputAreaFieldNotFound);
            }


            return inputAreaField;
        }

        private void ValidateInputField(InputFieldInputModel data, InputField inputField = null, int? inputFieldId = null)
        {
            if (inputFieldId.HasValue && inputFieldId.Value > 0)
            {
                if (inputField == null)
                {
                    throw new BadRequestException(InputErrorCode.InputFieldNotFound);
                }
                if (_accountancyDBContext.InputField.Any(f => f.InputFieldId != inputFieldId.Value && f.FieldName == data.FieldName))
                {
                    throw new BadRequestException(InputErrorCode.InputFieldAlreadyExisted);
                }
                if (!((EnumDataType)inputField.DataTypeId).Convertible((EnumDataType)data.DataTypeId))
                {
                    throw new BadRequestException(GeneralCode.InvalidParams, $"Không thể chuyển đổi kiểu dữ liệu từ {((EnumDataType)inputField.DataTypeId).GetEnumDescription()} sang {((EnumDataType)data.DataTypeId).GetEnumDescription()}");
                }
            }
            if (!string.IsNullOrEmpty(data.RefTableCode) && !string.IsNullOrEmpty(data.RefTableField))
            {
                var categoryCode = data.RefTableCode;
                var fieldName = data.RefTableField;
                var task = Task.Run(async () => (await _httpCategoryHelperService.GetReferFields(new List<string>() { categoryCode }, new List<string>() { fieldName })).FirstOrDefault());
                task.Wait();
                var sourceCategoryField = task.Result;
                if (sourceCategoryField == null)
                {
                    throw new BadRequestException(InputErrorCode.SourceCategoryFieldNotFound);
                }
            }
            if (data.DataTypeId == EnumDataType.Text && data.DataSize <= 0)
            {
                throw new BadRequestException(InputErrorCode.InputFieldDataSizeInValid);
            }
        }

        private void FieldDataProcess(ref InputFieldInputModel data)
        {
            if (!string.IsNullOrEmpty(data.RefTableCode) && !string.IsNullOrEmpty(data.RefTableField))
            {
                var categoryCode = data.RefTableCode;
                var fieldName = data.RefTableField;
                var task = Task.Run(async () => (await _httpCategoryHelperService.GetReferFields(new List<string>() { categoryCode }, new List<string>() { fieldName })).FirstOrDefault());
                task.Wait();
                var sourceCategoryField = task.Result;
                if (sourceCategoryField != null)
                {
                    data.DataTypeId = (EnumDataType)sourceCategoryField.DataTypeId;
                    data.DataSize = sourceCategoryField.DataSize;
                }
            }

            if (data.FormTypeId == EnumFormType.Generate)
            {
                data.DataTypeId = EnumDataType.Text;
                data.DataSize = -1;
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

        public async Task<bool> UpdateMultiField(int inputTypeId, List<InputAreaFieldInputModel> fields)
        {
            var inputTypeInfo = await _accountancyDBContext.InputType.AsNoTracking()
                .Where(t => t.InputTypeId == inputTypeId)
                .FirstOrDefaultAsync();

            if (inputTypeInfo == null)
            {
                throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy loại chứng từ!");
            }

            var areaIds = fields.Select(f => f.InputAreaId).Distinct().ToList();

            var inputAreas = await _accountancyDBContext.InputArea.Where(a => a.InputTypeId == inputTypeId).AsNoTracking().ToListAsync();

            foreach (var areaId in areaIds)
            {
                if (!inputAreas.Any(a => a.InputAreaId == areaId))
                {
                    throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy vùng dữ liệu chứng từ!");
                }
            }

            foreach (var field in fields)
            {
                field.InputTypeId = inputTypeId;
            }


            // Validate trùng trong danh sách
            if (fields.Select(f => new { f.InputTypeId, f.InputFieldId }).Distinct().Count() != fields.Count)
            {
                throw new BadRequestException(InputErrorCode.InputAreaFieldAlreadyExisted);
            }

            var curFields = _accountancyDBContext.InputAreaField
                .Include(af => af.InputField)
                .IgnoreQueryFilters()
                .Where(f => f.InputTypeId == inputTypeId)
                .ToList();

            var deleteFields = curFields
                .Where(cf => !cf.IsDeleted)
                .Where(cf => fields.All(f => f.InputFieldId != cf.InputFieldId))
                .ToList();

            List<int> singleNewFieldIds = new List<int>();

            using var trans = await _accountancyDBContext.Database.BeginTransactionAsync();
            try
            {
                // delete
                foreach (var deleteField in deleteFields)
                {
                    deleteField.IsDeleted = true;

                    await _accountancyDBContext.ExecuteStoreProcedure("asp_InputType_Clear_FieldData", new[] {
                        new SqlParameter("@InputTypeId",inputTypeId ),
                        new SqlParameter("@FieldName", deleteField.InputField.FieldName ),
                        new SqlParameter("@ResStatus", 0){ Direction = ParameterDirection.Output },
                    });
                }

                foreach (var field in fields)
                {
                    // validate
                    var curField = curFields.FirstOrDefault(f => f.InputFieldId == field.InputFieldId && f.InputTypeId == field.InputTypeId);
                    if (curField == null)
                    {
                        // create new
                        curField = _mapper.Map<InputAreaField>(field);
                        await _accountancyDBContext.InputAreaField.AddAsync(curField);
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
                        curField.InputAreaId = field.InputAreaId;
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
                        curField.InputFieldId = field.InputFieldId;
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
                        curField.ReferenceUrl = field.ReferenceUrl;
                        curField.IsBatchSelect = field.IsBatchSelect;
                        curField.OnClick = field.OnClick;

                        curField.CustomButtonHtml = field.CustomButtonHtml;
                        curField.CustomButtonOnClick = field.CustomButtonOnClick;
                        curField.MouseEnter = field.MouseEnter;
                        curField.MouseLeave = field.MouseLeave;
                    }
                }

                await _accountancyDBContext.SaveChangesAsync();

                // Get list gen code
                var genCodeConfigs = fields
                    .Where(f => f.IdGencode.HasValue)
                    .Select(f => new
                    {
                        InputAreaFieldId = f.InputAreaFieldId.Value,
                        IdGencode = f.IdGencode.Value
                    })
                    .ToDictionary(c => (long)c.InputAreaFieldId, c => c.IdGencode);

                var result = await _customGenCodeHelperService.MapObjectCustomGenCode(EnumObjectType.InputTypeRow, EnumObjectType.InputAreaField, genCodeConfigs);

                if (!result)
                {
                    trans.TryRollbackTransaction();
                    throw new BadRequestException(InputErrorCode.MapGenCodeConfigFail);
                }

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.InputType, inputTypeId, $"Cập nhật trường dữ liệu chứng từ {inputTypeInfo.Title}", fields.JsonSerialize());

                return true;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "Create");
                throw;
            }
        }

        public async Task<InputFieldInputModel> AddInputField(InputFieldInputModel data)
        {
            FieldDataProcess(ref data);
            ValidateInputField(data);

            using var trans = await _accountancyDBContext.Database.BeginTransactionAsync();
            try
            {
                var inputField = _mapper.Map<InputField>(data);

                await _accountancyDBContext.InputField.AddAsync(inputField);
                await _accountancyDBContext.SaveChangesAsync();

                if (inputField.FormTypeId != (int)EnumFormType.ViewOnly)
                {
                    await _accountancyDBContext.AddColumn(INPUTVALUEROW_TABLE, data.FieldName, data.DataTypeId, data.DataSize, data.DecimalPlace, data.DefaultValue, true);
                }
                await UpdateInputValueView();
                await UpdateInputTableType();
                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.InputType, inputField.InputFieldId, $"Thêm trường dữ liệu chung {inputField.Title}", data.JsonSerialize());
                return data;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "Create");
                throw;
            }
        }

        public async Task<InputFieldInputModel> UpdateInputField(int inputFieldId, InputFieldInputModel data)
        {
            var inputField = await _accountancyDBContext.InputField.FirstOrDefaultAsync(f => f.InputFieldId == inputFieldId);
            FieldDataProcess(ref data);
            ValidateInputField(data, inputField, inputFieldId);

            using var trans = await _accountancyDBContext.Database.BeginTransactionAsync();
            try
            {
                if (inputField.FormTypeId != (int)EnumFormType.ViewOnly)
                {
                    if (data.FieldName != inputField.FieldName)
                    {
                        await _accountancyDBContext.RenameColumn(INPUTVALUEROW_TABLE, inputField.FieldName, data.FieldName);
                    }
                    await _accountancyDBContext.UpdateColumn(INPUTVALUEROW_TABLE, data.FieldName, data.DataTypeId, data.DataSize, data.DecimalPlace, data.DefaultValue, true);
                }
                _mapper.Map(data, inputField);

                await _accountancyDBContext.SaveChangesAsync();

                await UpdateInputValueView();
                await UpdateInputTableType();
                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.InputType, inputField.InputFieldId, $"Cập nhật trường dữ liệu chung {inputField.Title}", data.JsonSerialize());
                return data;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "Update");
                throw;
            }
        }

        public async Task<bool> DeleteInputField(int inputFieldId)
        {
            var inputField = await _accountancyDBContext.InputField.FirstOrDefaultAsync(f => f.InputFieldId == inputFieldId);
            if (inputField == null)
            {

                throw new BadRequestException(InputErrorCode.InputFieldNotFound);
            }
            // Check used
            bool isUsed = _accountancyDBContext.InputAreaField.Any(af => af.InputFieldId == inputFieldId);
            if (isUsed)
            {
                throw new BadRequestException(InputErrorCode.InputFieldIsUse);
            }
            using var trans = await _accountancyDBContext.Database.BeginTransactionAsync();
            try
            {
                // Delete field
                inputField.IsDeleted = true;
                await _accountancyDBContext.SaveChangesAsync();
                if (inputField.FormTypeId != (int)EnumFormType.ViewOnly)
                {
                    await _accountancyDBContext.DropColumn(INPUTVALUEROW_TABLE, inputField.FieldName);
                }
                await UpdateInputValueView();
                await UpdateInputTableType();
                trans.Commit();
                await _activityLogService.CreateLog(EnumObjectType.InputType, inputField.InputFieldId, $"Xóa trường dữ liệu chung {inputField.Title}", inputField.JsonSerialize());
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

        private async Task UpdateInputValueView()
        {
            await _accountancyDBContext.ExecuteStoreProcedure("asp_InputValueRow_UpdateView", Array.Empty<SqlParameter>());
        }

        private async Task UpdateInputTableType()
        {
            await _accountancyDBContext.ExecuteStoreProcedure("asp_UpdateInputTableType", Array.Empty<SqlParameter>());
        }
    }
}

