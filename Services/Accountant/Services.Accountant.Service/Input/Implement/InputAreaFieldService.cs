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
using VErp.Commons.GlobalObject;
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
    public class InputAreaFieldService : AccoutantBaseService, IInputAreaFieldService
    {
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        public InputAreaFieldService(AccountingDBContext accountingContext
            , IOptions<AppSetting> appSetting
            , ILogger<InputAreaFieldService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            ) : base(accountingContext, appSetting, mapper)
        {
            _logger = logger;
            _activityLogService = activityLogService;
        }

        public async Task<PageData<InputAreaFieldOutputFullModel>> GetInputAreaFields(int inputTypeId, int inputAreaId, string keyword, int page, int size)
        {
            keyword = (keyword ?? "").Trim();
            var query = _accountingContext.InputAreaField
                .Include(f => f.InputAreaFieldStyle)
                .Include(f => f.DataType)
                .Include(f => f.FormType)
                .Include(f => f.ReferenceCategoryField)
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
            return (lst, total);
        }

        public async Task<PageData<InputAreaFieldOutputFullModel>> GetAll(string keyword, int page, int size)
        {
            keyword = (keyword ?? "").Trim();
            var query = _accountingContext.InputAreaField
                .Include(f => f.InputAreaFieldStyle)
                .Include(f => f.DataType)
                .Include(f => f.FormType)
                .Include(f => f.ReferenceCategoryField)
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(f => f.FieldName.Contains(keyword) || f.Title.Contains(keyword));
            }

            var groups = from field in query
                         group field by new
                         {
                             field.FieldName,
                             field.Title,
                             field.Filters,
                             field.ReferenceCategoryFieldId,
                             field.ReferenceCategoryTitleFieldId,
                             field.DataTypeId,
                             field.FormTypeId,
                             field.IsAutoIncrement,
                             field.IsHidden,
                             field.IsRequire,
                             field.IsUnique,
                             field.Placeholder,
                             field.RegularExpression

                         } into fieldGroup
                         select new
                         {
                             fieldGroup.Key,
                             InputAreaFieldId = fieldGroup.Max(f => f.InputAreaFieldId)
                         };

            var total = await groups.Select(g => g.Key).CountAsync();
            var fieldIds = groups.Select(g => g.InputAreaFieldId);

            if (size > 0)
            {
                query = query.Where(f => fieldIds.Contains(f.InputAreaFieldId)).Skip((page - 1) * size).Take(size);
            }
            List<InputAreaFieldOutputFullModel> lst = query.Select(f => _mapper.Map<InputAreaFieldOutputFullModel>(f)).ToList();
            return (lst, total);
        }


        public async Task<ServiceResult<InputAreaFieldOutputFullModel>> GetInputAreaField(int inputTypeId, int inputAreaId, int inputAreaFieldId)
        {
            var inputAreaField = await _accountingContext.InputAreaField
                .Include(f => f.InputAreaFieldStyle)
                .Include(f => f.DataType)
                .Include(f => f.FormType)
                .Include(f => f.ReferenceCategoryField)
                .FirstOrDefaultAsync(f => f.InputAreaFieldId == inputAreaFieldId && f.InputTypeId == inputTypeId && f.InputAreaId == inputAreaId);
            if (inputAreaField == null)
            {
                return InputErrorCode.InputAreaFieldNotFound;
            }
            InputAreaFieldOutputFullModel inputAreaFieldOutputModel = _mapper.Map<InputAreaFieldOutputFullModel>((object)inputAreaField);
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
                    inputAreaField.RegularExpression = data.RegularExpression;
                    inputAreaField.DefaultValue = data.DefaultValue;
                    inputAreaField.Filters = data.Filters;
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
