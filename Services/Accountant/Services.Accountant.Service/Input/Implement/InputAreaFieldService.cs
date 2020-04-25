using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.AccountingDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Accountant.Model.Category;
using VErp.Services.Accountant.Model.Input;
using CategoryEntity = VErp.Infrastructure.EF.AccountingDB.Category;

namespace VErp.Services.Accountant.Service.Input.Implement
{
    public class InputAreaFieldService : InputBaseService, IInputAreaFieldService
    {
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly IMapper _mapper;
        public InputAreaFieldService(AccountingDBContext accountingContext
            , IOptions<AppSetting> appSetting
            , ILogger<InputAreaFieldService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            ) : base(accountingContext)
        {
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
            _mapper = mapper;
        }

        public async Task<PageData<InputAreaFieldOutputFullModel>> GetInputAreaFields(int inputTypeId, int inputAreaId, string keyword, int page, int size)
        {
            keyword = (keyword ?? "").Trim();
            var query = _accountingContext.InputAreaField
                .Include(f => f.InputAreaFieldStyle)
                .Include(f => f.DataType)
                .Include(f => f.FormType)
                .Include(f => f.SourceCategoryField)
                .Include(f => f.SourceCategoryTitleField)
                .Where(f => f.InputTypeId == inputTypeId && f.InputAreaId == inputAreaId);
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(f => f.FieldName.Contains(keyword) || f.Title.Contains(keyword));
            }
            query = query.OrderBy(c => c.SortOrder);
            var total = await query.CountAsync();
            if (size > 0)
            {
                query = query.Skip((page - 1) * size).Take(size);
            }
            List<InputAreaFieldOutputFullModel> lst = await query.Select(f => _mapper.Map<InputAreaFieldOutputFullModel>(f)).ToListAsync();
            foreach (InputAreaFieldOutputFullModel item in lst)
            {
                if (item.SourceCategoryField != null)
                {
                    CategoryEntity sourceCategory = _accountingContext.Category.FirstOrDefault(c => c.CategoryId == item.SourceCategoryField.CategoryId);
                    item.SourceCategory = _mapper.Map<CategoryModel>(sourceCategory);
                }
            }
            return (lst, total);
        }

        public async Task<ServiceResult<InputAreaFieldOutputFullModel>> GetInputAreaField(int inputTypeId, int inputAreaId, int inputAreaField)
        {
            var InputAreaField = await _accountingContext.InputAreaField
                .Include(f => f.InputAreaFieldStyle)
                .Include(f => f.DataType)
                .Include(f => f.FormType)
                .Include(f => f.SourceCategoryField)
                .Include(f => f.SourceCategoryTitleField)
                .FirstOrDefaultAsync(f => f.InputAreaFieldId == inputAreaField && f.InputTypeId == inputTypeId && f.InputAreaId == inputAreaId);
            if (InputAreaField == null)
            {
                return InputErrorCode.InputAreaFieldNotFound;
            }
            InputAreaFieldOutputFullModel inputAreaFieldOutputModel = _mapper.Map<InputAreaFieldOutputFullModel>(InputAreaField);

            if (inputAreaFieldOutputModel.SourceCategoryField != null)
            {
                CategoryEntity sourceCategory = _accountingContext.Category.FirstOrDefault(c => c.CategoryId == inputAreaFieldOutputModel.SourceCategoryField.CategoryId);
                inputAreaFieldOutputModel.SourceCategory = _mapper.Map<CategoryModel>(sourceCategory);
            }

            return inputAreaFieldOutputModel;
        }

        public async Task<ServiceResult<int>> AddInputAreaField(int updatedUserId, int inputTypeId, int inputAreaId, InputAreaFieldInputModel data)
        {
            // Check inputType
            if (!_accountingContext.InputType.Any(i => i.InputTypeId == inputTypeId))
            {
                return InputErrorCode.InputTypeNotFound;
            }

            if (!_accountingContext.InputArea.Any(f => f.InputTypeId == inputTypeId && f.InputAreaId == inputAreaId))
            {
                return InputErrorCode.InputAreaNotFound;
            }

            if (data.ReferenceCategoryFieldId.HasValue)
            {
                var sourceCategoryField = _accountingContext.CategoryField.FirstOrDefault(f => f.CategoryFieldId == data.ReferenceCategoryFieldId.Value);
                if (sourceCategoryField == null)
                {
                    return InputErrorCode.SourceCategoryFieldNotFound;
                }
                data.DataTypeId = sourceCategoryField.DataTypeId;
                data.DataSize = sourceCategoryField.DataSize;
            }

            if (data.FormTypeId == (int)EnumFormType.Generate)
            {
                data.DataTypeId = (int)EnumDataType.Text;
                data.DataSize = 0;
            }

            using var trans = await _accountingContext.Database.BeginTransactionAsync();
            try
            {
                var inputAreaField = _mapper.Map<InputAreaField>(data);
                inputAreaField.CreatedByUserId = updatedUserId;
                inputAreaField.UpdatedByUserId = updatedUserId;

                int fieldIndex = GetFieldIndex(inputAreaId);
                if (fieldIndex < 0)
                {
                    trans.Rollback();
                    return InputErrorCode.InputAreaFieldOverLoad;
                }
                inputAreaField.FieldIndex = fieldIndex;
                await _accountingContext.InputAreaField.AddAsync(inputAreaField);
                await _accountingContext.SaveChangesAsync();

                trans.Commit();
                await _activityLogService.CreateLog(EnumObjectType.InputType, inputAreaField.InputAreaFieldId, $"Thêm trường dữ liệu {inputAreaField.Title}", data.JsonSerialize());
                return inputAreaField.InputAreaFieldId;
            }
            catch (Exception ex)
            {
                trans.Rollback();
                _logger.LogError(ex, "Create");
                return GeneralCode.InternalError;
            }
        }

        private int GetFieldIndex(int inputAreaId)
        {
            int index = -1;
            var arrIndex = _accountingContext.InputAreaField.Where(f => f.InputAreaId == inputAreaId).Select(f => f.FieldIndex).ToList();
            for (int indx = 0; indx <= 20; indx++)
            {
                if (!arrIndex.Contains(indx))
                {
                    index = indx;
                    break;
                }
            }

            return index;
        }

        public async Task<Enum> UpdateInputAreaField(int updatedUserId, int inputTypeId, int inputAreaId, int inputAreaFieldId, InputAreaFieldInputModel data)
        {
            var inputAreaField = await _accountingContext.InputAreaField.FirstOrDefaultAsync(f => f.InputAreaFieldId == inputAreaFieldId && f.InputAreaId == inputAreaId && f.InputTypeId == inputTypeId);
            if (inputAreaField == null)
            {
                return InputErrorCode.InputAreaFieldNotFound;
            }
            if (inputAreaField.FieldName != data.FieldName && _accountingContext.InputAreaField.Any(f => f.InputAreaFieldId != inputAreaFieldId && f.InputAreaId != inputAreaId && f.FieldName == data.FieldName))
            {
                return InputErrorCode.InputAreaFieldNameAlreadyExisted;
            }
            if (data.ReferenceCategoryFieldId.HasValue && data.ReferenceCategoryFieldId != inputAreaField.ReferenceCategoryFieldId)
            {
                var sourceCategoryField = _accountingContext.CategoryField.FirstOrDefault(f => f.CategoryFieldId == data.ReferenceCategoryFieldId.Value);
                if (sourceCategoryField == null)
                {
                    return InputErrorCode.SourceCategoryFieldNotFound;
                }
                data.DataTypeId = sourceCategoryField.DataTypeId;
                data.DataSize = sourceCategoryField.DataSize;
            }

            if (data.FormTypeId == (int)EnumFormType.Generate)
            {
                data.DataTypeId = (int)EnumDataType.Text;
                data.DataSize = 0;
            }

            using (var trans = await _accountingContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // update field
                    inputAreaField.InputAreaId = data.InputAreaId;
                    inputAreaField.InputTypeId = data.InputTypeId;
                    inputAreaField.FieldName = data.FieldName;
                    inputAreaField.Title = data.Title;
                    inputAreaField.Placeholder = data.Placeholder;
                    inputAreaField.SortOrder = data.SortOrder;
                    inputAreaField.DataTypeId = data.DataTypeId;
                    inputAreaField.DataSize = data.DataSize;
                    inputAreaField.FormTypeId = data.FormTypeId;
                    inputAreaField.IsAutoIncrement = data.IsAutoIncrement;
                    inputAreaField.IsRequire = data.IsRequire;
                    inputAreaField.IsUnique = data.IsUnique;
                    inputAreaField.IsHidden = data.IsHidden;
                    inputAreaField.RegularExpresion = data.RegularExpresion;
                    inputAreaField.DefaultValue = data.DefaultValue;
                    inputAreaField.IsDeleted = false;
                    inputAreaField.ReferenceCategoryFieldId = data.ReferenceCategoryFieldId;
                    inputAreaField.ReferenceCategoryTitleFieldId = data.ReferenceCategoryTitleFieldId;
                    inputAreaField.UpdatedByUserId = updatedUserId;
                    await _accountingContext.SaveChangesAsync();

                    // update style
                    var inputAreaFieldStyle = _accountingContext.InputAreaFieldStyle.First(fs => fs.InputAreaFieldId == inputAreaFieldId);
                    inputAreaFieldStyle.Width = data.InputAreaFieldStyle.Width;
                    inputAreaFieldStyle.Height = data.InputAreaFieldStyle.Height;
                    inputAreaFieldStyle.TitleStyleJson = data.InputAreaFieldStyle.TitleStyleJson;
                    inputAreaFieldStyle.InputStyleJson = data.InputAreaFieldStyle.InputStyleJson;
                    inputAreaFieldStyle.OnFocus = data.InputAreaFieldStyle.OnFocus;
                    inputAreaFieldStyle.OnKeydown = data.InputAreaFieldStyle.OnKeydown;
                    inputAreaFieldStyle.OnKeypress = data.InputAreaFieldStyle.OnKeypress;
                    inputAreaFieldStyle.OnBlur = data.InputAreaFieldStyle.OnBlur;
                    inputAreaFieldStyle.OnChange = data.InputAreaFieldStyle.OnChange;
                    inputAreaFieldStyle.AutoFocus = data.InputAreaFieldStyle.AutoFocus;
                    inputAreaFieldStyle.Column = data.InputAreaFieldStyle.Column;
                    await _accountingContext.SaveChangesAsync();

                    trans.Commit();
                    await _activityLogService.CreateLog(EnumObjectType.InputType, inputAreaField.FieldIndex, $"Cập nhật trường dữ liệu {inputAreaField.Title}", data.JsonSerialize());
                    return GeneralCode.Success;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    _logger.LogError(ex, "Update");
                    return GeneralCode.InternalError;
                }
            }
        }

        public async Task<Enum> DeleteInputAreaField(int updatedUserId, int inputTypeId, int inputAreaId, int inputAreaFieldId)
        {
            var inputAreaField = await _accountingContext.InputAreaField.FirstOrDefaultAsync(f => f.InputAreaFieldId == inputAreaFieldId && f.InputAreaId == inputAreaId && f.InputTypeId == inputTypeId);
            if (inputAreaField == null)
            {
                return InputErrorCode.InputAreaFieldNotFound;
            }

            using var trans = await _accountingContext.Database.BeginTransactionAsync();
            try
            {
                // Delete field
                inputAreaField.IsDeleted = true;
                inputAreaField.UpdatedByUserId = updatedUserId;
                await _accountingContext.SaveChangesAsync();
                trans.Commit();
                await _activityLogService.CreateLog(EnumObjectType.InputType, inputAreaField.FieldIndex, $"Xóa trường dữ liệu {inputAreaField.Title}", inputAreaField.JsonSerialize());
                return GeneralCode.Success;
            }
            catch (Exception ex)
            {
                trans.Rollback();
                _logger.LogError(ex, "Delete");
                return GeneralCode.InternalError;
            }
        }
    }
}
