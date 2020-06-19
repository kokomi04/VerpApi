using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Verp.Cache.RedisCache;
using VErp.Commons.Constants;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.AccountingDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Accountant.Model.Input;

namespace VErp.Services.Accountant.Service.Input.Implement
{
    public class InputConfigService : AccoutantBaseService, IInputConfigService
    {
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;

        public InputConfigService(AccountingDBContext accountingDBContext
            , IOptions<AppSetting> appSetting
            , ILogger<InputConfigService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            ) : base(accountingDBContext, appSetting, mapper)
        {
            _logger = logger;
            _activityLogService = activityLogService;
        }

        #region InputType

        public async Task<ServiceResult<InputTypeFullModel>> GetInputType(int inputTypeId)
        {
            var inputType = await _accountingContext.InputType
                .Where(i => i.InputTypeId == inputTypeId)
                .Include(t => t.InputArea)
                .ThenInclude(a => a.InputAreaField)
                .ThenInclude(af => af.InputField)
                .ThenInclude(f => f.ReferenceCategoryField)
                .Include(t => t.InputArea)
                .ThenInclude(a => a.InputAreaField)
                .ThenInclude(af => af.InputField)
                .ThenInclude(f => f.ReferenceCategoryTitleField)
                .ProjectTo<InputTypeFullModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
            if (inputType == null)
            {
                return InputErrorCode.InputTypeNotFound;
            }
            return inputType;
        }

        public async Task<PageData<InputTypeModel>> GetInputTypes(string keyword, int page, int size)
        {
            keyword = (keyword ?? "").Trim();

            var query = _accountingContext.InputType.AsQueryable();

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

        public async Task<ServiceResult<int>> AddInputType(InputTypeModel data)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockInputTypeKey(0));
            var existedInput = await _accountingContext.InputType
                .FirstOrDefaultAsync(i => i.InputTypeCode == data.InputTypeCode || i.Title == data.Title);
            if (existedInput != null)
            {
                if (string.Compare(existedInput.InputTypeCode, data.InputTypeCode, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return InputErrorCode.InputCodeAlreadyExisted;
                }

                return InputErrorCode.InputTitleAlreadyExisted;
            }

            using var trans = await _accountingContext.Database.BeginTransactionAsync();
            try
            {
                InputType inputType = _mapper.Map<InputType>(data);
                await _accountingContext.InputType.AddAsync(inputType);
                await _accountingContext.SaveChangesAsync();

                trans.Commit();
                await _activityLogService.CreateLog(EnumObjectType.InputType, inputType.InputTypeId, $"Thêm chứng từ {inputType.Title}", data.JsonSerialize());
                return inputType.InputTypeId;
            }
            catch (Exception ex)
            {
                trans.Rollback();
                _logger.LogError(ex, "Create");
                return GeneralCode.InternalError;
            }
        }

        public string GetStringClone(string source, int suffix = 0)
        {
            string suffixText = suffix > 0 ? string.Format("({0})", suffix) : string.Empty;
            string code = string.Format("{0}_{1}{2}", source, "Copy", suffixText);
            if (_accountingContext.InputType.Any(i => i.InputTypeCode == code))
            {
                suffix++;
                code = GetStringClone(source, suffix);
            }
            return code;
        }

        public async Task<ServiceResult<int>> CloneInputType(int inputTypeId)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockInputTypeKey(0));
            var sourceInput = await _accountingContext.InputType
                .Include(i => i.InputArea)
                .Include(a => a.InputAreaField)
                .FirstOrDefaultAsync(i => i.InputTypeId == inputTypeId);
            if (sourceInput == null)
            {
                return InputErrorCode.SourceInputTypeNotFound;
            }

            using var trans = await _accountingContext.Database.BeginTransactionAsync();
            try
            {
                InputType cloneType = new InputType
                {
                    InputTypeCode = GetStringClone(sourceInput.InputTypeCode),
                    Title = GetStringClone(sourceInput.Title),
                    InputTypeGroupId = sourceInput.InputTypeGroupId,
                    SortOrder = sourceInput.SortOrder,
                    PreLoadAction = sourceInput.PreLoadAction,
                    PostLoadAction = sourceInput.PostLoadAction
                };
                await _accountingContext.InputType.AddAsync(cloneType);
                await _accountingContext.SaveChangesAsync();

                foreach (var area in sourceInput.InputArea)
                {
                    InputArea cloneArea = new InputArea
                    {
                        InputTypeId = cloneType.InputTypeId,
                        InputAreaCode = area.InputAreaCode,
                        Title = area.Title,
                        IsMultiRow = area.IsMultiRow,
                        Columns = area.Columns,
                        SortOrder = area.SortOrder
                    };
                    await _accountingContext.InputArea.AddAsync(cloneArea);
                    await _accountingContext.SaveChangesAsync();

                    foreach (var field in sourceInput.InputAreaField.Where(f => f.InputAreaId == area.InputAreaId).ToList())
                    {
                        InputAreaField cloneField = new InputAreaField
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
                            Column = field.Column
                        };
                        await _accountingContext.InputAreaField.AddAsync(cloneField);
                    }
                }
                await _accountingContext.SaveChangesAsync();
                trans.Commit();
                UpdateView();
                await _activityLogService.CreateLog(EnumObjectType.InputType, cloneType.InputTypeId, $"Thêm chứng từ {cloneType.Title}", cloneType.JsonSerialize());
                return cloneType.InputTypeId;
            }
            catch (Exception ex)
            {
                trans.Rollback();
                _logger.LogError(ex, "Create");
                return GeneralCode.InternalError;
            }
        }

        public async Task<Enum> UpdateInputType(int inputTypeId, InputTypeModel data)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockInputTypeKey(inputTypeId));
            var inputType = await _accountingContext.InputType.FirstOrDefaultAsync(i => i.InputTypeId == inputTypeId);
            if (inputType == null)
            {
                return InputErrorCode.InputTypeNotFound;
            }
            if (inputType.InputTypeCode != data.InputTypeCode || inputType.Title != data.Title)
            {
                var existedInput = await _accountingContext.InputType
                    .FirstOrDefaultAsync(i => i.InputTypeId != inputTypeId && (i.InputTypeCode == data.InputTypeCode || i.Title == data.Title));
                if (existedInput != null)
                {
                    if (string.Compare(existedInput.InputTypeCode, data.InputTypeCode, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return InputErrorCode.InputCodeAlreadyExisted;
                    }

                    return InputErrorCode.InputTitleAlreadyExisted;
                }
            }

            using var trans = await _accountingContext.Database.BeginTransactionAsync();
            try
            {
                inputType.InputTypeCode = data.InputTypeCode;
                inputType.Title = data.Title;
                inputType.SortOrder = data.SortOrder;
                inputType.InputTypeGroupId = data.InputTypeGroupId;
                inputType.PreLoadAction = data.PreLoadAction;
                inputType.PostLoadAction = data.PostLoadAction;

                await _accountingContext.SaveChangesAsync();

                trans.Commit();
                UpdateView();
                await _activityLogService.CreateLog(EnumObjectType.InputType, inputType.InputTypeId, $"Cập nhật chứng từ {inputType.Title}", data.JsonSerialize());
                return GeneralCode.Success;
            }
            catch (Exception ex)
            {
                trans.Rollback();
                _logger.LogError(ex, "Update");
                return GeneralCode.InternalError;
            }
        }

        public async Task<Enum> DeleteInputType(int inputTypeId)
        {
            var inputType = await _accountingContext.InputType.FirstOrDefaultAsync(i => i.InputTypeId == inputTypeId);
            if (inputType == null)
            {
                return InputErrorCode.InputTypeNotFound;
            }

            using var trans = await _accountingContext.Database.BeginTransactionAsync();
            try
            {
                // Xóa area
                List<InputArea> inputAreas = _accountingContext.InputArea.Where(a => a.InputTypeId == inputTypeId).ToList();
                foreach (InputArea inputArea in inputAreas)
                {
                    inputArea.IsDeleted = true;
                    // Xóa field
                    List<InputAreaField> inputAreaFields = _accountingContext.InputAreaField.Where(f => f.InputAreaId == inputArea.InputAreaId).ToList();
                    foreach (InputAreaField inputAreaField in inputAreaFields)
                    {
                        inputAreaField.IsDeleted = true;
                    }
                }

                // Xóa Bill
                List<InputValueBill> inputValueBills = _accountingContext.InputValueBill.Where(b => b.InputTypeId == inputTypeId).ToList();
                foreach (InputValueBill inputValueBill in inputValueBills)
                {
                    inputValueBill.IsDeleted = true;
                    // Xóa row
                    List<InputValueRow> inputValueRows = _accountingContext.InputValueRow.Where(r => r.InputValueBillId == inputValueBill.InputValueBillId).ToList();
                    foreach (InputValueRow inputValueRow in inputValueRows)
                    {
                        inputValueRow.IsDeleted = true;
                        // Xóa row version
                        List<InputValueRowVersion> inputValueRowVersions = _accountingContext.InputValueRowVersion.Where(rv => rv.InputValueRowId == inputValueRow.InputValueRowId).ToList();
                        foreach (InputValueRowVersion inputValueRowVersion in inputValueRowVersions)
                        {
                            inputValueRowVersion.IsDeleted = true;
                        }
                    }
                }
                // Xóa type
                inputType.IsDeleted = true;
                await _accountingContext.SaveChangesAsync();
                trans.Commit();
                UpdateView();
                await _activityLogService.CreateLog(EnumObjectType.InventoryInput, inputType.InputTypeId, $"Xóa chứng từ {inputType.Title}", inputType.JsonSerialize());
                return GeneralCode.Success;
            }
            catch (Exception ex)
            {
                trans.Rollback();
                _logger.LogError(ex, "Delete");
                return GeneralCode.InternalError;
            }
        }

        public async Task<IList<InputTypeViewModelList>> InputTypeViewList(int inputTypeId)
        {
            return await _accountingContext.InputTypeView.Where(v => v.InputTypeId == inputTypeId).ProjectTo<InputTypeViewModelList>(_mapper.ConfigurationProvider).ToListAsync();
        }

        public async Task<InputTypeBasicOutput> GetInputTypeBasicInfo(int inputTypeId)
        {
            var inputTypeInfo = await _accountingContext.InputType.AsNoTracking().Where(t => t.InputTypeId == inputTypeId).ProjectTo<InputTypeBasicOutput>(_mapper.ConfigurationProvider).FirstOrDefaultAsync();

            inputTypeInfo.Areas = await _accountingContext.InputArea.AsNoTracking().Where(a => a.InputTypeId == inputTypeId).OrderBy(a => a.SortOrder).ProjectTo<InputAreaBasicOutput>(_mapper.ConfigurationProvider).ToListAsync();

            var fields = await (
                 from af in _accountingContext.InputAreaField
                 join f in _accountingContext.InputField on af.InputFieldId equals f.InputFieldId
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
                     FormTypeId = (EnumFormType)f.FormTypeId
                 }).ToListAsync();

            var views = await _accountingContext.InputTypeView.AsNoTracking().Where(t => t.InputTypeId == inputTypeId).OrderByDescending(v => v.IsDefault).ProjectTo<InputTypeViewModelList>(_mapper.ConfigurationProvider).ToListAsync();

            foreach (var item in inputTypeInfo.Areas)
            {
                item.Fields = fields.Where(f => f.InputAreaId == item.InputAreaId).ToList();
            }

            inputTypeInfo.Views = views;

            return inputTypeInfo;
        }

        public async Task<InputTypeViewModel> GetInputTypeViewInfo(int inputTypeId, int inputTypeViewId)
        {
            var info = await _accountingContext.InputTypeView.AsNoTracking().Where(t => t.InputTypeId == inputTypeId && t.InputTypeViewId == inputTypeViewId).ProjectTo<InputTypeViewModel>(_mapper.ConfigurationProvider).FirstOrDefaultAsync();

            if (info == null)
            {
                throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy cấu hình trong hệ thống");
            }

            var fields = await _accountingContext.InputTypeViewField.AsNoTracking()
                .Where(t => t.InputTypeViewId == inputTypeViewId)
                .ProjectTo<InputTypeViewFieldModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

            info.Fields = fields;

            return info;
        }

        public async Task<int> InputTypeViewCreate(int inputTypeId, InputTypeViewModel model)
        {
            using var trans = await _accountingContext.Database.BeginTransactionAsync();
            try
            {
                var inputTypeInfo = await _accountingContext.InputType.FirstOrDefaultAsync(t => t.InputTypeId == inputTypeId);
                if (inputTypeInfo == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy loại chứng từ");

                var info = _mapper.Map<InputTypeView>(model);

                info.InputTypeId = inputTypeId;

                await _accountingContext.InputTypeView.AddAsync(info);
                await _accountingContext.SaveChangesAsync();

                await InputTypeViewFieldAddRange(info.InputTypeViewId, model.Fields);

                await _accountingContext.SaveChangesAsync();

                await trans.CommitAsync();

                await _activityLogService.CreateLog(EnumObjectType.InputTypeView, info.InputTypeViewId, $"Tạo bộ lọc {info.InputTypeViewName} cho chứng từ  {inputTypeInfo.Title}", model.JsonSerialize());

                return info.InputTypeViewId;
            }
            catch (Exception ex)
            {
                await trans.RollbackAsync();
                _logger.LogError(ex, "InputTypeViewCreate");
                throw ex;
            }

        }

        public async Task<Enum> InputTypeViewUpdate(int inputTypeViewId, InputTypeViewModel model)
        {
            using var trans = await _accountingContext.Database.BeginTransactionAsync();
            try
            {

                var info = await _accountingContext.InputTypeView.FirstOrDefaultAsync(v => v.InputTypeViewId == inputTypeViewId);

                if (info == null) throw new BadRequestException(GeneralCode.ItemNotFound, "View không tồn tại");

                var inputTypeInfo = await _accountingContext.InputType.FirstOrDefaultAsync(t => t.InputTypeId == info.InputTypeId);
                if (inputTypeInfo == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy loại chứng từ");


                _mapper.Map(model, info);

                var oldFields = await _accountingContext.InputTypeViewField.Where(f => f.InputTypeViewId == inputTypeViewId).ToListAsync();

                _accountingContext.InputTypeViewField.RemoveRange(oldFields);

                await InputTypeViewFieldAddRange(inputTypeViewId, model.Fields);

                await _accountingContext.SaveChangesAsync();

                await trans.CommitAsync();

                await _activityLogService.CreateLog(EnumObjectType.InputTypeView, info.InputTypeViewId, $"Cập nhật bộ lọc {info.InputTypeViewName} cho chứng từ  {inputTypeInfo.Title}", model.JsonSerialize());

                return GeneralCode.Success;
            }
            catch (Exception ex)
            {
                await trans.RollbackAsync();
                _logger.LogError(ex, "InputTypeViewUpdate");
                throw ex;
            }
        }

        public async Task<Enum> InputTypeViewDelete(int inputTypeViewId)
        {
            var info = await _accountingContext.InputTypeView.FirstOrDefaultAsync(v => v.InputTypeViewId == inputTypeViewId);

            if (info == null) throw new BadRequestException(GeneralCode.ItemNotFound, "View không tồn tại");

            info.IsDeleted = true;
            info.DeletedDatetimeUtc = DateTime.UtcNow;


            var inputTypeInfo = await _accountingContext.InputType.FirstOrDefaultAsync(t => t.InputTypeId == info.InputTypeId);
            if (inputTypeInfo == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy loại chứng từ");


            await _accountingContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.InputTypeView, info.InputTypeViewId, $"Xóa bộ lọc {info.InputTypeViewName} chứng từ  {inputTypeInfo.Title}", new { inputTypeViewId }.JsonSerialize());

            return GeneralCode.Success;

        }

        #region InputTypeGroup
        public async Task<int> InputTypeGroupCreate(InputTypeGroupModel model)
        {
            var info = _mapper.Map<InputTypeGroup>(model);
            await _accountingContext.InputTypeGroup.AddAsync(info);
            await _accountingContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.InputTypeGroup, info.InputTypeGroupId, $"Thêm nhóm chứng từ {info.InputTypeGroupName}", model.JsonSerialize());

            return info.InputTypeGroupId;
        }

        public async Task<bool> InputTypeGroupUpdate(int inputTypeGroupId, InputTypeGroupModel model)
        {
            var info = await _accountingContext.InputTypeGroup.FirstOrDefaultAsync(g => g.InputTypeGroupId == inputTypeGroupId);

            if (info == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Nhóm chứng từ không tồn tại");

            _mapper.Map(model, info);

            await _accountingContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.InputTypeGroup, info.InputTypeGroupId, $"Cập nhật nhóm chứng từ {info.InputTypeGroupName}", model.JsonSerialize());

            return true;
        }

        public async Task<bool> InputTypeGroupDelete(int inputTypeGroupId)
        {
            var info = await _accountingContext.InputTypeGroup.FirstOrDefaultAsync(g => g.InputTypeGroupId == inputTypeGroupId);

            if (info == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Nhóm chứng từ không tồn tại");

            info.IsDeleted = true;

            await _accountingContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.InputTypeGroup, info.InputTypeGroupId, $"Xóa nhóm chứng từ {info.InputTypeGroupName}", new { inputTypeGroupId }.JsonSerialize());

            return true;
        }

        public async Task<IList<InputTypeGroupList>> InputTypeGroupList()
        {
            return await _accountingContext.InputTypeGroup.ProjectTo<InputTypeGroupList>(_mapper.ConfigurationProvider).OrderBy(g => g.SortOrder).ToListAsync();
        }

        #endregion
        private async Task InputTypeViewFieldAddRange(int inputTypeViewId, IList<InputTypeViewFieldModel> fieldModels)
        {
            var categoryFieldIds = fieldModels.Where(f => f.ReferenceCategoryFieldId.HasValue).Select(f => f.ReferenceCategoryFieldId.Value).ToList();
            categoryFieldIds.Union(fieldModels.Where(f => f.ReferenceCategoryTitleFieldId.HasValue).Select(f => f.ReferenceCategoryFieldId.Value).ToList());

            if (categoryFieldIds.Count > 0)
            {

                var categoryFields = (await _accountingContext.CategoryField
                    .Where(f => categoryFieldIds.Contains(f.CategoryFieldId))
                    .Select(f => new { f.CategoryFieldId, f.CategoryId })
                    .AsNoTracking()
                    .ToListAsync())
                    .ToDictionary(f => f.CategoryFieldId, f => f);

                foreach (var f in fieldModels)
                {
                    if (f.ReferenceCategoryFieldId.HasValue && categoryFields.TryGetValue(f.ReferenceCategoryFieldId.Value, out var cateField) && cateField.CategoryId != f.ReferenceCategoryId)
                    {
                        throw new BadRequestException(GeneralCode.InvalidParams, "Trường dữ liệu của danh mục không thuộc danh mục");
                    }

                    if (f.ReferenceCategoryTitleFieldId.HasValue && categoryFields.TryGetValue(f.ReferenceCategoryTitleFieldId.Value, out cateField) && cateField.CategoryId != f.ReferenceCategoryId)
                    {
                        throw new BadRequestException(GeneralCode.InvalidParams, "Trường hiển thị của danh mục không thuộc danh mục");
                    }
                }

            }

            var fields = fieldModels.Select(f => _mapper.Map<InputTypeViewField>(f)).ToList();

            foreach (var f in fields)
            {
                f.InputTypeViewId = inputTypeViewId;
            }

            await _accountingContext.InputTypeViewField.AddRangeAsync(fields);

        }

        #endregion

        #region Area

        public async Task<ServiceResult<InputAreaModel>> GetInputArea(int inputTypeId, int inputAreaId)
        {
            var inputArea = await _accountingContext.InputArea
                .Where(i => i.InputTypeId == inputTypeId && i.InputAreaId == inputAreaId)
                .ProjectTo<InputAreaModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
            if (inputArea == null)
            {
                return InputErrorCode.InputTypeNotFound;
            }
            return inputArea;
        }

        public async Task<PageData<InputAreaModel>> GetInputAreas(int inputTypeId, string keyword, int page, int size)
        {
            keyword = (keyword ?? "").Trim();
            var query = _accountingContext.InputArea.Where(a => a.InputTypeId == inputTypeId).AsQueryable();
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

        public async Task<ServiceResult<int>> AddInputArea(int inputTypeId, InputAreaInputModel data)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockInputTypeKey(inputTypeId));
            var existedInput = await _accountingContext.InputArea
                .FirstOrDefaultAsync(a => a.InputTypeId == inputTypeId && (a.InputAreaCode == data.InputAreaCode || a.Title == data.Title));
            if (existedInput != null)
            {
                if (string.Compare(existedInput.InputAreaCode, data.InputAreaCode, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return InputErrorCode.InputCodeAlreadyExisted;
                }

                return InputErrorCode.InputTitleAlreadyExisted;
            }

            if (data.IsMultiRow && _accountingContext.InputArea.Any(a => a.InputTypeId == inputTypeId && a.IsMultiRow))
            {
                return InputErrorCode.MultiRowAreaAlreadyExisted;
            }


            using var trans = await _accountingContext.Database.BeginTransactionAsync();
            try
            {
                InputArea inputArea = _mapper.Map<InputArea>(data);
                inputArea.InputTypeId = inputTypeId;
                await _accountingContext.InputArea.AddAsync(inputArea);
                await _accountingContext.SaveChangesAsync();

                trans.Commit();
                UpdateView();
                await _activityLogService.CreateLog(EnumObjectType.InputType, inputArea.InputAreaId, $"Thêm vùng thông tin {inputArea.Title}", data.JsonSerialize());
                return inputArea.InputAreaId;
            }
            catch (Exception ex)
            {
                trans.Rollback();
                _logger.LogError(ex, "Create");
                return GeneralCode.InternalError;
            }
        }

        public async Task<Enum> UpdateInputArea(int inputTypeId, int inputAreaId, InputAreaInputModel data)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockInputTypeKey(inputTypeId));
            var inputArea = await _accountingContext.InputArea.FirstOrDefaultAsync(a => a.InputTypeId == inputTypeId && a.InputAreaId == inputAreaId);
            if (inputArea == null)
            {
                return InputErrorCode.InputAreaNotFound;
            }
            if (inputArea.InputAreaCode != data.InputAreaCode || inputArea.Title != data.Title)
            {
                var existedInput = await _accountingContext.InputArea
                    .FirstOrDefaultAsync(a => a.InputTypeId == inputTypeId && a.InputAreaId != inputAreaId && (a.InputAreaCode == data.InputAreaCode || a.Title == data.Title));
                if (existedInput != null)
                {
                    if (string.Compare(existedInput.InputAreaCode, data.InputAreaCode, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return InputErrorCode.InputAreaCodeAlreadyExisted;
                    }

                    return InputErrorCode.InputAreaTitleAlreadyExisted;
                }
            }
            if (data.IsMultiRow && _accountingContext.InputArea.Any(a => a.InputTypeId == inputTypeId && a.InputAreaId != inputAreaId && a.IsMultiRow))
            {
                return InputErrorCode.MultiRowAreaAlreadyExisted;
            }
            using var trans = await _accountingContext.Database.BeginTransactionAsync();
            try
            {
                inputArea.InputAreaCode = data.InputAreaCode;
                inputArea.Title = data.Title;
                inputArea.IsMultiRow = data.IsMultiRow;
                inputArea.Columns = data.Columns;
                inputArea.SortOrder = data.SortOrder;
                await _accountingContext.SaveChangesAsync();

                trans.Commit();
                UpdateView();
                await _activityLogService.CreateLog(EnumObjectType.InputType, inputArea.InputAreaId, $"Cập nhật vùng dữ liệu {inputArea.Title}", data.JsonSerialize());
                return GeneralCode.Success;
            }
            catch (Exception ex)
            {
                trans.Rollback();
                _logger.LogError(ex, "Update");
                return GeneralCode.InternalError;
            }
        }

        public async Task<Enum> DeleteInputArea(int inputTypeId, int inputAreaId)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockInputTypeKey(inputTypeId));
            var inputArea = await _accountingContext.InputArea.FirstOrDefaultAsync(a => a.InputTypeId == inputTypeId && a.InputAreaId == inputAreaId);
            if (inputArea == null)
            {
                return InputErrorCode.InputAreaNotFound;
            }

            using var trans = await _accountingContext.Database.BeginTransactionAsync();
            try
            {
                // Xóa field
                List<InputAreaField> inputAreaFields = _accountingContext.InputAreaField.Where(f => f.InputAreaId == inputAreaId).ToList();
                foreach (InputAreaField inputAreaField in inputAreaFields)
                {
                    inputAreaField.IsDeleted = true;
                    await _accountingContext.SaveChangesAsync();
                }

                // Xóa row
                if (inputArea.IsMultiRow)
                {
                    List<InputValueRow> inputValueRows = (from vr in _accountingContext.InputValueRow
                                                          join b in _accountingContext.InputValueBill on vr.InputValueBillId equals b.InputValueBillId
                                                          where b.InputTypeId == inputTypeId && vr.IsMultiRow == true
                                                          select vr).ToList();
                    foreach (InputValueRow inputValueRow in inputValueRows)
                    {
                        inputValueRow.IsDeleted = true;
                        await _accountingContext.SaveChangesAsync();

                        // Xóa row version
                        List<InputValueRowVersion> inputValueRowVersions = _accountingContext.InputValueRowVersion.Where(rv => rv.InputValueRowId == inputValueRow.InputValueRowId).ToList();
                        foreach (InputValueRowVersion inputValueRowVersion in inputValueRowVersions)
                        {
                            inputValueRowVersion.IsDeleted = true;
                            await _accountingContext.SaveChangesAsync();
                        }
                    }
                }


                // Xóa area
                inputArea.IsDeleted = true;
                await _accountingContext.SaveChangesAsync();
                trans.Commit();
                UpdateView();
                await _activityLogService.CreateLog(EnumObjectType.InventoryInput, inputArea.InputTypeId, $"Xóa chứng từ {inputArea.Title}", inputArea.JsonSerialize());
                return GeneralCode.Success;

            }
            catch (Exception ex)
            {
                trans.Rollback();
                _logger.LogError(ex, "Delete");
                return GeneralCode.InternalError;
            }
        }

        #endregion

        #region Field

        public async Task<PageData<InputAreaFieldOutputFullModel>> GetInputAreaFields(int inputTypeId, int inputAreaId, string keyword, int page, int size)
        {
            keyword = (keyword ?? "").Trim();
            var query = _accountingContext.InputAreaField
                .Include(f => f.InputField)
                .ThenInclude(f => f.ReferenceCategoryField)
                .ThenInclude(f => f.ReferenceCategoryTitleField)
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
            List<InputAreaFieldOutputFullModel> lst = await query.ProjectTo<InputAreaFieldOutputFullModel>(_mapper.ConfigurationProvider).ToListAsync();
            return (lst, total);
        }

        public async Task<PageData<InputFieldOutputModel>> GetInputFields(string keyword, int page, int size)
        {
            keyword = (keyword ?? "").Trim();
            var query = _accountingContext.InputField
                .Include(f => f.ReferenceCategoryField)
                .Include(f => f.ReferenceCategoryTitleField)
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
            List<InputFieldOutputModel> lst = query.ProjectTo<InputFieldOutputModel>(_mapper.ConfigurationProvider).ToList();
            return (lst, total);
        }

        public async Task<ServiceResult<InputAreaFieldOutputFullModel>> GetInputAreaField(int inputTypeId, int inputAreaId, int inputAreaFieldId)
        {
            var inputAreaField = await _accountingContext.InputAreaField
                .Where(f => f.InputAreaFieldId == inputAreaFieldId && f.InputTypeId == inputTypeId && f.InputAreaId == inputAreaId)
                .Include(f => f.InputField)
                .ThenInclude(f => f.ReferenceCategoryField)
                .ThenInclude(f => f.ReferenceCategoryTitleField)
                .ProjectTo<InputAreaFieldOutputFullModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
            if (inputAreaField == null)
            {
                return InputErrorCode.InputAreaFieldNotFound;
            }

            return inputAreaField;
        }

        private void ValidateExistedInputType(int inputTypeId, int inputAreaId)
        {
            // Check inputType
            if (!_accountingContext.InputType.Any(i => i.InputTypeId == inputTypeId))
            {
                throw new BadRequestException(InputErrorCode.InputTypeNotFound);
            }
            if (!_accountingContext.InputArea.Any(f => f.InputTypeId == inputTypeId && f.InputAreaId == inputAreaId))
            {
                throw new BadRequestException(InputErrorCode.InputAreaNotFound);
            }
        }

        private Enum ValidateInputField(InputFieldInputModel data, InputField inputField = null, int? inputFieldId = null)
        {
            if (inputFieldId.HasValue && inputFieldId.Value > 0)
            {
                if (inputField == null)
                {
                    return InputErrorCode.InputFieldNotFound;
                }
                if (_accountingContext.InputField.Any(f => f.InputFieldId != inputFieldId.Value && f.FieldName == data.FieldName))
                {
                    return InputErrorCode.InputFieldAlreadyExisted;
                }
            }
            if (data.ReferenceCategoryFieldId.HasValue)
            {
                var sourceCategoryField = _accountingContext.CategoryField.FirstOrDefault(f => f.CategoryFieldId == data.ReferenceCategoryFieldId.Value);
                if (sourceCategoryField == null)
                {
                    return InputErrorCode.SourceCategoryFieldNotFound;
                }
            }
            return GeneralCode.Success;
        }

        private void FieldDataProcess(ref InputFieldInputModel data)
        {
            if (data.ReferenceCategoryFieldId.HasValue)
            {
                int referId = data.ReferenceCategoryFieldId.Value;
                var sourceCategoryField = _accountingContext.CategoryField.FirstOrDefault(f => f.CategoryFieldId == referId);
                data.DataTypeId = sourceCategoryField.DataTypeId;
                data.DataSize = sourceCategoryField.DataSize;
            }
            if (data.FormTypeId == (int)EnumFormType.Generate)
            {
                data.DataTypeId = (int)EnumDataType.Text;
                data.DataSize = 0;
            }
            if (!AccountantConstants.SELECT_FORM_TYPES.Contains((EnumFormType)data.FormTypeId))
            {
                data.ReferenceCategoryFieldId = null;
                data.ReferenceCategoryTitleFieldId = null;
            }
        }

        public async Task<ServiceResult<int>> UpdateMultiField(int inputTypeId, List<InputAreaFieldInputModel> fields)
        {
            // Validate trùng trong danh sách
            if (fields.Select(f => new { f.InputTypeId, f.InputFieldId }).Distinct().Count() != fields.Count)
            {
                return InputErrorCode.InputAreaFieldAlreadyExisted;
            }

            var areas = _accountingContext.InputArea.Where(a => fields.Select(f => f.InputAreaId).Contains(a.InputAreaId)).Select(a => new
            {
                a.InputAreaId,
                a.IsMultiRow
            }).Distinct().ToList();

            List<InputAreaField> curFields = _accountingContext.InputAreaField
                .IgnoreQueryFilters()
                .Where(f => f.InputTypeId == inputTypeId)
                .ToList();

            List<InputAreaField> deleteFields = curFields
                .Where(cf => !cf.IsDeleted)
                .Where(cf => fields.All(f => f.InputFieldId != cf.InputFieldId || f.InputTypeId != cf.InputTypeId))
                .ToList();

            List<int> singleNewFieldIds = new List<int>();

            using var trans = await _accountingContext.Database.BeginTransactionAsync();
            try
            {
                // delete
                foreach (var deleteField in deleteFields)
                {
                    deleteField.IsDeleted = true;
                }
                foreach (var field in fields)
                {
                    // validate
                    ValidateExistedInputType(inputTypeId, field.InputAreaId);
                    var curField = curFields.FirstOrDefault(f => f.InputFieldId == field.InputFieldId && f.InputTypeId == field.InputTypeId);
                    if (curField == null)
                    {
                        // create new
                        curField = _mapper.Map<InputAreaField>(field);
                        await _accountingContext.InputAreaField.AddAsync(curField);
                        await _accountingContext.SaveChangesAsync();
                        field.InputAreaFieldId = curField.InputAreaFieldId;
                        // Add field need clear old data
                        var area = areas.First(a => a.InputAreaId == curField.InputAreaId);
                        if (!area.IsMultiRow)
                        {
                            singleNewFieldIds.Add(curField.InputAreaFieldId);
                        }
                    }
                    else if (Comparer(field, curField))
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
                    }
                }

                await _accountingContext.SaveChangesAsync();
                // Clear old data

                var singleNewFields = _accountingContext.InputAreaField.Include(f => f.InputField).Where(f => singleNewFieldIds.Contains(f.InputAreaFieldId)).ToList();

                if (singleNewFieldIds.Count > 0)
                {
                    Expression ex = Expression.Constant(false);
                    var param = Expression.Parameter(typeof(InputValueRowVersion), "vrv");
                    foreach (var field in singleNewFields)
                    {
                        var method = typeof(string).GetMethod(nameof(string.IsNullOrEmpty), new[] { typeof(string) });
                        var prop = Expression.Property(param, GetFieldName(field.InputField.FieldIndex));
                        var isEmptyFunc = Expression.Lambda<Func<InputValueRowVersion, bool>>(Expression.Call(method, prop), param);
                        var notEmptyFunc = Expression.Lambda<Func<InputValueRowVersion, bool>>(Expression.Not(isEmptyFunc.Body), isEmptyFunc.Parameters[0]);
                        ex = Expression.OrElse(ex, notEmptyFunc.Body);
                    }

                    // Get rows
                    List<InputValueRowVersion> inputValueRowVersions = (from vrv in _accountingContext.InputValueRowVersion
                                                                        join vr in _accountingContext.InputValueRow on new { vrv.InputValueRowId, vrv.InputValueRowVersionId } equals new { vr.InputValueRowId, InputValueRowVersionId = vr.LastestInputValueRowVersionId }
                                                                        join b in _accountingContext.InputValueBill on vr.InputValueBillId equals b.InputValueBillId
                                                                        where b.InputTypeId == inputTypeId && vr.IsMultiRow == false
                                                                        select vrv).Where(Expression.Lambda<Func<InputValueRowVersion, bool>>(ex, param)).ToList();

                    List<InputValueRowVersionNumber> inputValueRowVersionNumbers = _accountingContext.InputValueRowVersionNumber
                        .Where(vn => inputValueRowVersions.Select(v => v.InputValueRowVersionId).Contains(vn.InputValueRowVersionId))
                        .ToList();

                    foreach (var inputValueRowVersion in inputValueRowVersions)
                    {
                        var inputValueRowVersionNumber = inputValueRowVersionNumbers.First(vn => vn.InputValueRowVersionId == inputValueRowVersion.InputValueRowVersionId);
                        foreach (var field in singleNewFields)
                        {
                            var fieldName = GetFieldName(field.InputField.FieldIndex);
                            long valueInNumber = 0;
                            typeof(InputValueRowVersion).GetProperty(fieldName).SetValue(inputValueRowVersion, valueInNumber);
                            typeof(InputValueRowVersionNumber).GetProperty(fieldName).SetValue(inputValueRowVersionNumber, valueInNumber);
                        }
                    }
                }
                await _accountingContext.SaveChangesAsync();

                // Get list gen code
                Dictionary<int, int> genCodeConfigs = fields
                    .Where(f => f.IdGencode.HasValue)
                    .Select(f => new
                    {
                        InputAreaFieldId = f.InputAreaFieldId.Value,
                        IdGencode = f.IdGencode.Value
                    })
                    .ToDictionary(c => c.InputAreaFieldId, c => c.IdGencode);

                string url = $"api/internal/InternalCustomGenCode/{(int)EnumObjectType.InputType}/multiconfigs";
                (bool, HttpStatusCode) result = GetFromAPI<bool>(url, 100000, HttpMethod.Post, genCodeConfigs);
                if (result.Item2 != HttpStatusCode.OK)
                {
                    trans.Rollback();
                    throw new BadRequestException(InputErrorCode.MapGenCodeConfigFail);
                }
                trans.Commit();
                UpdateView();
                await _activityLogService.CreateLog(EnumObjectType.InputType, inputTypeId, $"Cập nhật nhiều trường dữ liệu", fields.JsonSerialize());
                return inputTypeId;
            }
            catch (Exception ex)
            {
                trans.Rollback();
                _logger.LogError(ex, "Create");
                return GeneralCode.InternalError;
            }
        }

        private bool Comparer(InputAreaFieldInputModel updateField, InputAreaField curField)
        {
            return curField.IsDeleted ||
                updateField.InputAreaId != curField.InputAreaId ||
                updateField.InputFieldId != curField.InputFieldId ||
                updateField.InputTypeId != curField.InputTypeId ||
                updateField.Title != curField.Title ||
                updateField.Placeholder != curField.Placeholder ||
                updateField.SortOrder != curField.SortOrder ||
                updateField.IsAutoIncrement != curField.IsAutoIncrement ||
                updateField.IsRequire != curField.IsRequire ||
                updateField.IsUnique != curField.IsUnique ||
                updateField.IsHidden != curField.IsHidden ||
                updateField.IsCalcSum != curField.IsCalcSum ||
                updateField.RegularExpression != curField.RegularExpression ||
                updateField.DefaultValue != curField.DefaultValue ||
                updateField.Filters != curField.Filters ||
                updateField.Width != curField.Width ||
                updateField.Height != curField.Height ||
                updateField.TitleStyleJson != curField.TitleStyleJson ||
                updateField.InputStyleJson != curField.InputStyleJson ||
                updateField.OnFocus != curField.OnFocus ||
                updateField.OnKeydown != curField.OnKeydown ||
                updateField.OnKeypress != curField.OnKeypress ||
                updateField.OnBlur != curField.OnBlur ||
                updateField.OnChange != curField.OnChange ||
                updateField.AutoFocus != curField.AutoFocus ||
                updateField.Column != curField.Column;
        }

        public async Task<ServiceResult<int>> AddInputField(InputFieldInputModel data)
        {
            var r = ValidateInputField(data);
            if (!r.IsSuccess())
            {
                return r;
            }
            FieldDataProcess(ref data);

            using var trans = await _accountingContext.Database.BeginTransactionAsync();
            try
            {
                var inputField = _mapper.Map<InputField>(data);

                inputField.FieldIndex = GetFieldIndex();
                await _accountingContext.InputField.AddAsync(inputField);
                await _accountingContext.SaveChangesAsync();

                trans.Commit();
                await _activityLogService.CreateLog(EnumObjectType.InputType, inputField.InputFieldId, $"Thêm trường dữ liệu chung {inputField.Title}", data.JsonSerialize());
                return inputField.InputFieldId;
            }
            catch (Exception ex)
            {
                trans.Rollback();
                _logger.LogError(ex, "Create");
                return GeneralCode.InternalError;
            }
        }

        public async Task<Enum> UpdateInputField(int inputFieldId, InputFieldInputModel data)
        {
            var inputField = await _accountingContext.InputField.FirstOrDefaultAsync(f => f.InputFieldId == inputFieldId);

            var r = ValidateInputField(data, inputField, inputFieldId);
            if (!r.IsSuccess())
            {
                return r;
            }

            FieldDataProcess(ref data);

            using var trans = await _accountingContext.Database.BeginTransactionAsync();
            try
            {
                inputField.Title = data.Title;
                inputField.DataTypeId = data.DataTypeId;
                inputField.DataSize = data.DataSize;
                inputField.FormTypeId = data.FormTypeId;
                inputField.FieldName = data.FieldName;
                inputField.ReferenceCategoryFieldId = data.ReferenceCategoryFieldId;
                inputField.ReferenceCategoryTitleFieldId = data.ReferenceCategoryTitleFieldId;
                inputField.SortOrder = data.SortOrder;
                inputField.Placeholder = data.Placeholder;
                inputField.DefaultValue = data.DefaultValue;

                await _accountingContext.SaveChangesAsync();
                trans.Commit();
                await _activityLogService.CreateLog(EnumObjectType.InputType, inputField.InputFieldId, $"Cập nhật trường dữ liệu chung {inputField.Title}", data.JsonSerialize());
                return GeneralCode.Success;
            }
            catch (Exception ex)
            {
                trans.Rollback();
                _logger.LogError(ex, "Update");
                return GeneralCode.InternalError;
            }
        }

        public async Task<Enum> DeleteInputField(int inputFieldId)
        {
            var inputField = await _accountingContext.InputField.FirstOrDefaultAsync(f => f.InputFieldId == inputFieldId);
            if (inputField == null)
            {
                return InputErrorCode.InputFieldNotFound;
            }
            // Check used
            bool isUsed = _accountingContext.InputAreaField.Any(af => af.InputFieldId == inputFieldId);
            if (isUsed)
            {
                return InputErrorCode.InputFieldIsUsed;
            }
            using var trans = await _accountingContext.Database.BeginTransactionAsync();
            try
            {
                // Delete field
                inputField.IsDeleted = true;
                await _accountingContext.SaveChangesAsync();
                trans.Commit();
                await _activityLogService.CreateLog(EnumObjectType.InputType, inputField.InputFieldId, $"Xóa trường dữ liệu chung {inputField.Title}", inputField.JsonSerialize());
                return GeneralCode.Success;
            }
            catch (Exception ex)
            {
                trans.Rollback();
                _logger.LogError(ex, "Delete");
                return GeneralCode.InternalError;
            }
        }

        private int GetFieldIndex()
        {
            int index = -1;
            var arrIndex = _accountingContext.InputField
                .Select(f => f.FieldIndex).ToList();
            int firstIndex = -1;
            // Lấy ra index bị xóa và data null hoặc empty hoặc chưa được sử dụng 
            for (int indx = 0; indx <= AccountantConstants.INPUT_TYPE_FIELD_NUMBER; indx++)
            {
                // Check bị xóa hoặc chưa sử dụng
                bool isUsedYet = !arrIndex.Contains(indx);

                // Check data null hoặc empty
                bool isEmpty = false;
                if (isUsedYet)
                {
                    var rParam = Expression.Parameter(typeof(InputValueRowVersion), "rv");
                    string fieldName = string.Format(AccountantConstants.INPUT_TYPE_FIELDNAME_FORMAT, indx);
                    var methodInfo = typeof(string).GetMethod(nameof(string.IsNullOrEmpty), new[] { typeof(string) });
                    var prop = Expression.Property(rParam, fieldName);

                    Expression expression = Expression.Call(methodInfo, prop);


                    isEmpty = _accountingContext.InputValueRowVersion.All(Expression.Lambda<Func<InputValueRowVersion, bool>>(expression, rParam));

                    if (firstIndex == -1)
                    {
                        firstIndex = indx;
                    }
                }

                if (isUsedYet && isEmpty)
                {
                    index = indx;
                    break;
                }
            }

            if (index == -1 && firstIndex > 0)
            {
                index = firstIndex;
            }
            else if (index == -1)
            {
                throw new BadRequestException(InputErrorCode.InputAreaFieldOverLoad);
            }

            return index;
        }

        #endregion

        private void UpdateView()
        {
            using var connection = _accountingContext.Database.GetDbConnection();
            connection.Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "usp_InputType_UpdateView";
            cmd.ExecuteNonQuery();
        }
    }
}

