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

            List<InputAreaField> curFields = _accountingContext.InputAreaField
                .IgnoreQueryFilters()
                .Where(f => f.InputTypeId == inputTypeId)
                .ToList();

            List<InputAreaField> deleteFields = curFields
                .Where(cf => !cf.IsDeleted)
                .Where(cf => fields.All(f => f.InputFieldId != cf.InputFieldId || f.InputTypeId != cf.InputTypeId))
                .ToList();

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
                        var inputAreaField = _mapper.Map<InputAreaField>(field);
                        await _accountingContext.InputAreaField.AddAsync(inputAreaField);
                        field.InputAreaFieldId = inputAreaField.InputAreaFieldId;
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
                int fieldIndex = GetFieldIndex();
                if (fieldIndex < 0)
                {
                    trans.Rollback();
                    return InputErrorCode.InputAreaFieldOverLoad;
                }
                inputField.FieldIndex = fieldIndex;
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

            if (index == -1)
            {
                index = firstIndex;
            }


            return index;
        }
    }
}
