using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
            List<InputAreaFieldOutputFullModel> lst = await query.ProjectTo<InputAreaFieldOutputFullModel>(_mapper.ConfigurationProvider).ToListAsync();
            foreach (var field in lst)
            {
                if (field.ReferenceCategoryId.HasValue)
                {
                    CategoryField referField = _accountingContext.CategoryField.First(f => f.CategoryFieldId == field.ReferenceCategoryId.Value);
                    field.ReferenceCategoryId = referField.CategoryId;
                }
            }
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
            List<InputAreaFieldOutputFullModel> lst = query.ProjectTo<InputAreaFieldOutputFullModel>(_mapper.ConfigurationProvider).ToList();
            foreach (var field in lst)
            {
                if (field.ReferenceCategoryId.HasValue)
                {
                    CategoryField referField = _accountingContext.CategoryField.First(f => f.CategoryFieldId == field.ReferenceCategoryId.Value);
                    field.ReferenceCategoryId = referField.CategoryId;
                }
            }
            return (lst, total);
        }

        public async Task<ServiceResult<InputAreaFieldOutputFullModel>> GetInputAreaField(int inputTypeId, int inputAreaId, int inputAreaFieldId)
        {
            var inputAreaField = await _accountingContext.InputAreaField
                .Where(f => f.InputAreaFieldId == inputAreaFieldId && f.InputTypeId == inputTypeId && f.InputAreaId == inputAreaId)
                .Include(f => f.InputAreaFieldStyle)
                .Include(f => f.DataType)
                .Include(f => f.FormType)
                .Include(f => f.ReferenceCategoryField)
                .ProjectTo<InputAreaFieldOutputFullModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
            if (inputAreaField == null)
            {
                return InputErrorCode.InputAreaFieldNotFound;
            }

            if (inputAreaField.ReferenceCategoryId.HasValue)
            {
                CategoryField referField = _accountingContext.CategoryField.First(f => f.CategoryFieldId == inputAreaField.ReferenceCategoryId.Value);
                inputAreaField.ReferenceCategoryId = referField.CategoryId;
            }
            return inputAreaField;
        }

        private Enum ValidateExistedInputType(int inputTypeId, int inputAreaId)
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
            return GeneralCode.Success;
        }

        private Enum ValidateInputAreaField(InputAreaFieldInputModel data, InputAreaField inputAreaField = null, int? inputAreaFieldId = null)
        {
            bool updateFieldName = true;
            if (inputAreaFieldId.HasValue && inputAreaFieldId.Value > 0)
            {
                if (inputAreaField == null)
                {
                    return InputErrorCode.InputAreaFieldNotFound;
                }
                updateFieldName = inputAreaField.FieldName == data.FieldName;
            }
            if (updateFieldName && _accountingContext.InputAreaField.Any(f => (!inputAreaFieldId.HasValue || f.InputAreaFieldId != inputAreaFieldId.Value) && f.InputAreaFieldId == data.InputAreaFieldId && f.FieldName == data.FieldName))
            {
                return InputErrorCode.InputAreaFieldNameAlreadyExisted;
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

        private void FieldDataProcess(ref InputAreaFieldInputModel data)
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
            using var trans = await _accountingContext.Database.BeginTransactionAsync();
            try
            {
                // Validate trùng name trong danh sách
                if (fields.Select(f => new { f.InputAreaId, f.FieldName }).Distinct().Count() != fields.Count)
                {
                    return InputErrorCode.InputAreaFieldNameAlreadyExisted;
                }

                var groups = fields.GroupBy(i => new { i.InputAreaId });
                foreach (var group in groups)
                {
                    Enum r = ValidateExistedInputType(inputTypeId, group.Key.InputAreaId);
                    if (!r.IsSuccess())
                    {
                        return r;
                    }

                    for (int indx = 0; indx < group.Count(); indx++)
                    {
                        var data = group.ElementAt(indx);
                        var inputAreaField = data.InputAreaFieldId > 0 ? _accountingContext.InputAreaField.FirstOrDefault(f => f.InputAreaFieldId == data.InputAreaFieldId) : null;
                        r = ValidateInputAreaField(data, inputAreaField, data.InputAreaFieldId);
                        if (!r.IsSuccess())
                        {
                            return r;
                        }
                        FieldDataProcess(ref data);
                        if (data.InputAreaFieldId > 0)
                        {
                            // Update
                            UpdateField(ref inputAreaField, data);
                        }
                        else
                        {
                            // Create new
                            inputAreaField = _mapper.Map<InputAreaField>(data);
                            int fieldIndex = GetFieldIndex(inputAreaField.InputAreaId);
                            if (fieldIndex < 0)
                            {
                                trans.Rollback();
                                return InputErrorCode.InputAreaFieldOverLoad;
                            }
                            inputAreaField.FieldIndex = fieldIndex;
                            await _accountingContext.InputAreaField.AddAsync(inputAreaField);
                            await _accountingContext.SaveChangesAsync();
                        }
                    }
                }
                await _accountingContext.SaveChangesAsync();
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

        public async Task<ServiceResult<int>> AddInputAreaField(int inputTypeId, int inputAreaId, InputAreaFieldInputModel data)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockInputTypeKey(inputTypeId));
            Enum r = ValidateExistedInputType(inputTypeId, inputAreaId);
            if (!r.IsSuccess())
            {
                return r;
            }
            r = ValidateInputAreaField(data);
            if (!r.IsSuccess())
            {
                return r;
            }
            FieldDataProcess(ref data);

            using var trans = await _accountingContext.Database.BeginTransactionAsync();
            try
            {
                var inputAreaField = _mapper.Map<InputAreaField>(data);
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
            var arrIndex = _accountingContext.InputAreaField
                .Where(f => f.InputAreaId == inputAreaId)
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


                    isEmpty = (from row in _accountingContext.InputValueRow
                               join rowVersion in _accountingContext.InputValueRowVersion
                               on new { rowId = row.InputValueRowId, rowVersionId = row.LastestInputValueRowVersionId }
                               equals new { rowId = rowVersion.InputValueRowId, rowVersionId = rowVersion.InputValueRowVersionId }
                               where row.InputAreaId == inputAreaId
                               select rowVersion).All(Expression.Lambda<Func<InputValueRowVersion, bool>>(expression, rParam));

                    if(firstIndex == -1)
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

            if(index == -1)
            {
                index = firstIndex;
            }


            return index;
        }

        private void UpdateField(ref InputAreaField inputAreaField, InputAreaFieldInputModel data)
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
            // update style
            int inputAreaFieldId = inputAreaField.InputAreaFieldId;
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
        }

        public async Task<Enum> UpdateInputAreaField(int inputTypeId, int inputAreaId, int inputAreaFieldId, InputAreaFieldInputModel data)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockInputTypeKey(inputTypeId));
            Enum r = ValidateExistedInputType(inputTypeId, inputAreaId);
            if (!r.IsSuccess())
            {
                return r;
            }
            var inputAreaField = await _accountingContext.InputAreaField.FirstOrDefaultAsync(f => f.InputAreaFieldId == inputAreaFieldId);

            r = ValidateInputAreaField(data, inputAreaField, inputAreaFieldId);
            if (!r.IsSuccess())
            {
                return r;
            }

            FieldDataProcess(ref data);

            using var trans = await _accountingContext.Database.BeginTransactionAsync();
            try
            {
                UpdateField(ref inputAreaField, data);
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

        public async Task<Enum> DeleteInputAreaField(int inputTypeId, int inputAreaId, int inputAreaFieldId)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockInputTypeKey(inputTypeId));
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
