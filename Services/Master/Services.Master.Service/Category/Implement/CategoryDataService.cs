using AutoMapper;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Verp.Cache.RedisCache;
using Verp.Resources.Master.Category;
using VErp.Commons.Constants;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface.Category;
using VErp.Commons.GlobalObject.InternalDataInterface.System;
using VErp.Commons.Library;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Model.Category;
using VErp.Services.Master.Service.Category;
using static Verp.Resources.Master.Category.CategoryDataValidationMessage;
using CategoryEntity = VErp.Infrastructure.EF.MasterDB.Category;

namespace VErp.Services.Accountancy.Service.Category
{
    public class CategoryDataService : ICategoryDataService
    {
        private readonly ILogger _logger;
        private readonly AppSetting _appSetting;
        private readonly IMapper _mapper;
        private readonly MasterDBContext _masterContext;
        private readonly ICurrentContextService _currentContextService;
        private readonly IDataProtectionProvider _protectionProvider;
        private readonly ICustomGenCodeHelperService _customGenCodeHelperService;
        private readonly ICategoryHelperService _httpCategoryHelperService;
        private readonly ILongTaskResourceLockService longTaskResourceLockService;
        private readonly ObjectActivityLogFacade _categoryDataActivityLog;

        public CategoryDataService(MasterDBContext masterContext
            , IOptions<AppSetting> appSetting
            , ILogger<CategoryDataService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            , ICurrentContextService currentContextService
            , IDataProtectionProvider protectionProvider
            , ICustomGenCodeHelperService customGenCodeHelperService
            , ICategoryHelperService httpCategoryHelperService
            , ILongTaskResourceLockService longTaskResourceLockService
            )
        {
            _logger = logger;
            _masterContext = masterContext;
            _appSetting = appSetting.Value;
            _mapper = mapper;
            _currentContextService = currentContextService;
            _protectionProvider = protectionProvider;
            _customGenCodeHelperService = customGenCodeHelperService;
            _httpCategoryHelperService = httpCategoryHelperService;
            this.longTaskResourceLockService = longTaskResourceLockService;
            _categoryDataActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.CategoryData);
        }


        private async Task<(int Code, string Message, List<NonCamelCaseDictionary> ResultData)> ProcessActionAsync(string script, NonCamelCaseDictionary data, Dictionary<string, EnumDataType> fields, EnumActionType action)
        {
            List<NonCamelCaseDictionary> resultData = null;
            var resultParam = new SqlParameter("@ResStatus", 0) { DbType = DbType.Int32, Direction = ParameterDirection.Output };
            var messageParam = new SqlParameter("@Message", DBNull.Value) { DbType = DbType.String, Direction = ParameterDirection.Output, Size = 128 };
            if (!string.IsNullOrEmpty(script))
            {
                var parammeters = new List<SqlParameter>() {
                    new SqlParameter("@Action", (int)action),
                    resultParam,
                    messageParam
                };
                foreach (var field in fields)
                {
                    data.TryGetStringValue(field.Key, out var celValue);
                    parammeters.Add(new SqlParameter($"@{field.Key}", (field.Value).GetSqlValue(celValue)));
                }
                resultData = (await _masterContext.QueryDataTableRaw(script, parammeters)).ConvertData();
            }
            return ((resultParam.Value as int?).GetValueOrDefault(), messageParam.Value as string, resultData);
        }
        public async Task<int> AddCategoryRow(int categoryId, NonCamelCaseDictionary data)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockCategoryKey(categoryId));
            using var trans = await _masterContext.Database.BeginTransactionAsync();
            var r = await AddCategoryRowToDb(categoryId, data);
            await trans.CommitAsync();
            return r;
        }

        public async Task<int> AddCategoryRowToDb(int categoryId, NonCamelCaseDictionary data)
        {
            var category = _masterContext.Category.FirstOrDefault(c => c.CategoryId == categoryId);
            if (category == null)
            {
                throw new BadRequestException(CategoryErrorCode.CategoryNotFound);
            }
            //var tableName = $"v{category.CategoryCode}";

            if (category.IsReadonly)
            {
                throw new BadRequestException(CategoryErrorCode.CategoryReadOnly);
            }
            if (category.IsOutSideData)
            {
                throw new BadRequestException(CategoryErrorCode.CategoryIsOutSideData);
            }

            // Check parent row
            await CheckParentRowAsync(data, category);

            // Lấy thông tin field
            var categoryFields = _masterContext.CategoryField
                .Where(f => category.CategoryId == f.CategoryId && f.FormTypeId != (int)EnumFormType.ViewOnly && f.FormTypeId != (int)EnumFormType.SqlSelect)
                .ToList();

            var genCodeContexts = new List<IGenerateCodeContext>();
            var baseValueChains = new Dictionary<string, int>();

            await FillGenerateColumn(genCodeContexts, baseValueChains, categoryId, category.CategoryCode, categoryFields, data);

            var requiredFields = categoryFields.Where(f => !f.AutoIncrement && f.IsRequired);
            var uniqueFields = categoryFields.Where(f => !f.AutoIncrement && f.IsUnique);
            var selectFields = categoryFields.Where(f => !f.AutoIncrement && (f.FormTypeId == (int)EnumFormType.SearchTable || f.FormTypeId == (int)EnumFormType.Select));

            // Check field required
            CheckRequired(data, requiredFields);
            // Check refer
            await CheckRefer(data, selectFields);
            // Check unique
            await CheckUnique(data, uniqueFields, category.CategoryCode);
            // Check value
            CheckValue(data, categoryFields);

            // Before saving action (SQL)


            var fieldParam = _masterContext.CategoryField
                .Where(f => category.CategoryId == f.CategoryId)
                .ToDictionary(f => f.CategoryFieldName, f => (EnumDataType)f.DataTypeId);
            var result = await ProcessActionAsync(category.BeforeSaveAction, data, fieldParam, EnumActionType.Add);

            if (result.Code != 0)
            {
                if (string.IsNullOrWhiteSpace(result.Message))
                    throw ProcessActionResultErrorCode.BadRequestFormat(result.Code);

                throw result.Message.BadRequest();
            }

            // Insert data
            var dataTable = new DataTable(category.CategoryCode);

            if (category.IsTreeView)
            {
                dataTable.Columns.Add(CategoryFieldConstants.ParentId, typeof(int));
            }
            dataTable.Columns.Add("CreatedByUserId", typeof(int));
            dataTable.Columns.Add("CreatedDatetimeUtc", typeof(DateTime));
            dataTable.Columns.Add("UpdatedByUserId", typeof(int));
            dataTable.Columns.Add("UpdatedDatetimeUtc", typeof(DateTime));
            dataTable.Columns.Add("IsDeleted", typeof(bool));
            dataTable.Columns.Add("DeletedDatetimeUtc", typeof(DateTime));

            foreach (var field in categoryFields)
            {
                if (field.CategoryFieldName == CategoryFieldConstants.F_Id) continue;

                dataTable.Columns.Add(field.CategoryFieldName, ((EnumDataType)field.DataTypeId).GetColumnDataType());
            }

            var dataRow = dataTable.NewRow();
            if (category.IsTreeView)
            {
                data.TryGetStringValue(CategoryFieldConstants.ParentId, out string value);
                if (!string.IsNullOrEmpty(value))
                {
                    dataRow[CategoryFieldConstants.ParentId] = int.Parse(value);
                }
                else
                {
                    dataRow[CategoryFieldConstants.ParentId] = DBNull.Value;
                }
            }
            dataRow["CreatedByUserId"] = _currentContextService.UserId;
            dataRow["CreatedDatetimeUtc"] = DateTime.UtcNow;
            dataRow["UpdatedByUserId"] = _currentContextService.UserId;
            dataRow["UpdatedDatetimeUtc"] = DateTime.UtcNow;
            dataRow["IsDeleted"] = false;
            dataRow["DeletedDatetimeUtc"] = DBNull.Value;

            foreach (var field in categoryFields)
            {
                if (field.CategoryFieldName == CategoryFieldConstants.F_Id) continue;
                data.TryGetStringValue(field.CategoryFieldName, out string value);
                dataRow[field.CategoryFieldName] = ((EnumDataType)field.DataTypeId).GetSqlValue(value);

                if (field.CategoryFieldName == GlobalFieldConstants.SubsidiaryId)
                {
                    dataRow[field.CategoryFieldName] = ((EnumDataType)field.DataTypeId).GetSqlValue(_currentContextService.SubsidiaryId);
                }
            }
            dataTable.Rows.Add(dataRow);

            var id = await _masterContext.InsertDataTable(dataTable);

            // After saving action (SQL)
            await ProcessActionAsync(category.AfterSaveAction, data, fieldParam, EnumActionType.Add);

            await _customGenCodeHelperService.ConfirmCode(CustomGenCodeBaseValue);


            foreach (var item in genCodeContexts)
            {
                await item.ConfirmCode();
            }


            await _categoryDataActivityLog.LogBuilder(() => CategoryDataActivityLogMessage.Create)
              .MessageResourceFormatDatas(id)
              .BillTypeId(category.CategoryId)
              .ObjectId(id)
              .JsonData(data)
              .CreateLog();

            return (int)id;
        }

        public async Task<int> UpdateCategoryRow(int categoryId, int fId, NonCamelCaseDictionary data)
        {
            var category = _masterContext.Category.FirstOrDefault(c => c.CategoryId == categoryId);
            if (category == null)
            {
                throw new BadRequestException(CategoryErrorCode.CategoryNotFound);
            }
            if (category.IsReadonly)
            {
                throw new BadRequestException(CategoryErrorCode.CategoryReadOnly);
            }
            if (category.IsOutSideData)
            {
                throw new BadRequestException(CategoryErrorCode.CategoryIsOutSideData);
            }
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockCategoryKey(categoryId));

            var categoryFields = _masterContext.CategoryField.Where(f => f.CategoryId == category.CategoryId && f.FormTypeId != (int)EnumFormType.ViewOnly && f.FormTypeId != (int)EnumFormType.SqlSelect && f.CategoryFieldName != CategoryFieldConstants.F_Id).ToList();

            var categoryRow = await GetCategoryRowInfo(category, categoryFields, fId);

            data.TryGetValue(GlobalFieldConstants.UpdatedDatetimeUtc, out object modelUpdatedDatetimeUtc);

            categoryRow.TryGetValue(GlobalFieldConstants.UpdatedDatetimeUtc, out object entityUpdatedDatetimeUtc);

            if (modelUpdatedDatetimeUtc?.ToString() != entityUpdatedDatetimeUtc?.ToString())
            {
                throw GeneralCode.DataIsOld.BadRequest();
            }

            bool isParentChange = false;
            // Check parent row
            if (category.IsTreeView)
            {
                categoryRow.TryGetValue(CategoryFieldConstants.ParentId, out object oParent);
                string cParent = oParent?.ToString() ?? string.Empty;
                data.TryGetStringValue(CategoryFieldConstants.ParentId, out string uParent);
                uParent ??= string.Empty;

                isParentChange = cParent != uParent;

                if (!string.IsNullOrWhiteSpace(uParent) && isParentChange)
                {
                    await CheckParentRowAsync(data, category, fId);
                }
            }

            // Lấy các trường thay đổi
            List<CategoryField> updateFields = new List<CategoryField>();
            foreach (CategoryField categoryField in categoryFields)
            {
                if (!data.ContainsKey(categoryField.CategoryFieldName)) continue;

                categoryRow.TryGetStringValue(categoryField.CategoryFieldName, out var currentValue);
                data.TryGetStringValue(categoryField.CategoryFieldName, out var updateValue);
                if (currentValue != updateValue)
                {
                    updateFields.Add(categoryField);
                }
            }

            // Lấy thông tin field
            var requiredFields = updateFields.Where(f => !f.AutoIncrement && f.IsRequired);
            var uniqueFields = updateFields.Where(f => !f.AutoIncrement && f.IsUnique);
            var selectFields = updateFields.Where(f => !f.AutoIncrement && (f.FormTypeId == (int)EnumFormType.SearchTable || f.FormTypeId == (int)EnumFormType.Select));

            // Check field required
            CheckRequired(data, requiredFields);

            // Check refer
            await CheckRefer(data, selectFields);

            // Check unique
            await CheckUnique(data, uniqueFields, category.CategoryCode);

            // Check value
            CheckValue(data, categoryFields);

            var fieldParam = _masterContext.CategoryField
                .Where(f => category.CategoryId == f.CategoryId)
                .ToDictionary(f => f.CategoryFieldName, f => (EnumDataType)f.DataTypeId);
            var result = await ProcessActionAsync(category.BeforeSaveAction, data, fieldParam, EnumActionType.Update);

            if (result.Code != 0)
            {
                if (string.IsNullOrWhiteSpace(result.Message))
                    throw ProcessActionResultErrorCode.BadRequestFormat(result.Code);

                throw result.Message.BadRequest();
            }

            // Update data
            var dataTable = new DataTable(category.CategoryCode);

            if (isParentChange)
            {
                dataTable.Columns.Add(CategoryFieldConstants.ParentId, typeof(int));
            }
            dataTable.Columns.Add("UpdatedByUserId", typeof(int));
            dataTable.Columns.Add("UpdatedDatetimeUtc", typeof(DateTime));

            foreach (var field in updateFields)
            {
                if (field.CategoryFieldName == CategoryFieldConstants.F_Id) continue;
                dataTable.Columns.Add(field.CategoryFieldName, ((EnumDataType)field.DataTypeId).GetColumnDataType());
            }

            var dataRow = dataTable.NewRow();

            if (isParentChange)
            {
                data.TryGetStringValue(CategoryFieldConstants.ParentId, out string value);
                if (!string.IsNullOrEmpty(value))
                {
                    dataRow[CategoryFieldConstants.ParentId] = int.Parse(value);
                }
                else
                {
                    dataRow[CategoryFieldConstants.ParentId] = DBNull.Value;
                }
            }

            dataRow["UpdatedByUserId"] = _currentContextService.UserId;
            dataRow["UpdatedDatetimeUtc"] = DateTime.UtcNow;

            foreach (var field in updateFields)
            {
                if (field.CategoryFieldName == CategoryFieldConstants.F_Id) continue;
                data.TryGetStringValue(field.CategoryFieldName, out string value);
                dataRow[field.CategoryFieldName] = ((EnumDataType)field.DataTypeId).GetSqlValue(value);

                if (field.CategoryFieldName == GlobalFieldConstants.SubsidiaryId)
                {
                    dataRow[field.CategoryFieldName] = ((EnumDataType)field.DataTypeId).GetSqlValue(_currentContextService.SubsidiaryId);
                }
            }
            dataTable.Rows.Add(dataRow);

            int numberChange = await _masterContext.UpdateCategoryData(dataTable, fId);

            await _customGenCodeHelperService.ConfirmCode(CustomGenCodeBaseValue);

            // After saving action (SQL)
            await ProcessActionAsync(category.AfterSaveAction, data, fieldParam, EnumActionType.Update);

            await _categoryDataActivityLog.LogBuilder(() => CategoryDataActivityLogMessage.Update)
             .MessageResourceFormatDatas(fId)
             .BillTypeId(category.CategoryId)
             .ObjectId(fId)
             .JsonData(data)
             .CreateLog();

            return numberChange;
        }

        private CustomGenCodeBaseValueModel CustomGenCodeBaseValue = null;

        private async Task FillGenerateColumn(List<IGenerateCodeContext> ctxs, Dictionary<string, int> baseValueChains, int categoryId, string categoryCode, ICollection<CategoryField> fields, NonCamelCaseDictionary data)
        {
            var ngayCt = data.ContainsKey(AccountantConstants.BILL_DATE) ? data[AccountantConstants.BILL_DATE] : null;

            long? ngayCtValue = null;
            if (long.TryParse(ngayCt?.ToString(), out var v))
            {
                ngayCtValue = v;
            }

            foreach (var field in fields.Where(f => f.FormTypeId == (int)EnumFormType.Generate))
            {
                if ((!data.TryGetStringValue(field.CategoryFieldName, out var value) || value.IsNullOrEmptyObject()))
                {
                    try
                    {

                        var ctx = _customGenCodeHelperService.CreateGenerateCodeContext(baseValueChains);

                        var code = await ctx
                            .SetConfig(EnumObjectType.Category, EnumObjectType.CategoryField, field.CategoryFieldId, field.Title)
                            .SetConfigData(categoryId, ngayCtValue, categoryCode)
                            .TryValidateAndGenerateCode(value, (code) =>
                            {
                                return Task.FromResult(true);
                            });

                        value = code;
                        ctxs.Add(ctx);

                        if (!data.ContainsKey(field.CategoryFieldName))
                        {
                            data.Add(field.CategoryFieldName, value);
                        }
                        else
                        {
                            data[field.CategoryFieldName] = value;
                        }
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
        }

        public async Task<int> DeleteCategoryRow(int categoryId, int fId)
        {
            var category = _masterContext.Category.FirstOrDefault(c => c.CategoryId == categoryId);
            if (category == null)
            {
                throw new BadRequestException(CategoryErrorCode.CategoryNotFound);
            }
            var tableName = $"v{category.CategoryCode}";

            if (category.IsReadonly)
            {
                throw new BadRequestException(CategoryErrorCode.CategoryReadOnly);
            }
            if (category.IsOutSideData)
            {
                throw new BadRequestException(CategoryErrorCode.CategoryIsOutSideData);
            }
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockCategoryKey(0));
            // Validate child-parent relationship
            if (category.IsTreeView)
            {
                var existSql = $"SELECT [{tableName}].F_Id as Total FROM {tableName} WHERE [{tableName}].ParentId = {fId};";
                var result = await _masterContext.QueryDataTableRaw(existSql, Array.Empty<SqlParameter>());
                bool isExisted = result != null && result.Rows.Count > 0;
                if (isExisted)
                {
                    throw new BadRequestException(CategoryErrorCode.HadSomeDataRelatedToThisValue);
                }
            }

            // Get row
            var categoryFields = _masterContext.CategoryField.Where(f => f.CategoryId == categoryId && f.FormTypeId != (int)EnumFormType.ViewOnly).ToList();

            var categoryRow = await GetCategoryRowInfo(category, categoryFields, fId);

            var fieldNames = categoryFields.Select(f => f.CategoryFieldName).ToList();

            // Category refer fields
            var referToFields = _masterContext.CategoryField.Where(f => f.RefTableCode == category.CategoryCode && fieldNames.Contains(f.RefTableField)).ToList();
            var referToCategoryIds = referToFields.Select(f => f.CategoryId).Distinct().ToList();
            var referToCategories = _masterContext.Category.Where(c => referToCategoryIds.Contains(c.CategoryId)).ToList();

            // Check reference
            foreach (var field in categoryFields)
            {
                categoryRow.TryGetValue(field.CategoryFieldName, out object value);
                if (value == null) continue;
                bool isExisted = false;
                foreach (var referToField in referToFields.Where(c => c.RefTableField == field.CategoryFieldName))
                {
                    if (!((EnumFormType)referToField.FormTypeId).IsJoinForm()) continue;

                    var referToCategory = referToCategories.First(c => c.CategoryId == referToField.CategoryId);
                    var referToTable = $"v{referToCategory.CategoryCode}";

                    var referToValue = new SqlParameter("@RefValue", value?.ToString());
                    var existSql = $"SELECT F_Id FROM [dbo].{referToTable} WHERE {referToField.CategoryFieldName} = @RefValue;";
                    var result = await _masterContext.QueryDataTableRaw(existSql, new[] { referToValue });
                    isExisted = result != null && result.Rows.Count > 0;
                    if (isExisted)
                    {
                        throw new BadRequestException(CategoryErrorCode.HadSomeDataRelatedToThisValue);
                    }
                }

                // TODO
                isExisted = await _httpCategoryHelperService.CheckReferFromCategory(category.CategoryCode, fieldNames, categoryRow);
                if (isExisted) throw new BadRequestException(CategoryErrorCode.HadSomeDataRelatedToThisValue);

            }
            // Delete data
            var dataTable = new DataTable(category.CategoryCode);
            dataTable.Columns.Add("IsDeleted", typeof(bool));
            dataTable.Columns.Add("UpdatedByUserId", typeof(int));
            dataTable.Columns.Add("DeletedDatetimeUtc", typeof(DateTime));

            var dataRow = dataTable.NewRow();
            dataRow["IsDeleted"] = true;
            dataRow["UpdatedByUserId"] = _currentContextService.UserId;
            dataRow["DeletedDatetimeUtc"] = DateTime.UtcNow;

            dataTable.Rows.Add(dataRow);
            int numberChange = await _masterContext.UpdateCategoryData(dataTable, fId);


            await _categoryDataActivityLog.LogBuilder(() => CategoryDataActivityLogMessage.Delete)
             .MessageResourceFormatDatas(fId)
             .BillTypeId(category.CategoryId)
             .ObjectId(fId)
             .JsonData(categoryRow)
             .CreateLog();

            return numberChange;
        }

        private void CheckRequired(NonCamelCaseDictionary data, IEnumerable<CategoryField> requiredFields)
        {
            foreach (var field in requiredFields)
            {
                // ignore auto generate field
                //if (field.FormTypeId == (int)EnumFormType.Generate) continue;

                if (!data.Any(v => v.Key == field.CategoryFieldName && !v.Value.IsNullOrEmptyObject()))
                {
                    throw new BadRequestException(CategoryErrorCode.RequiredFieldIsEmpty, new string[] { field.Title });
                }
            }
        }

        private async Task CheckRefer(NonCamelCaseDictionary data, IEnumerable<CategoryField> selectFields)
        {
            // Check refer
            foreach (var field in selectFields)
            {
                string tableName = $"v{field.RefTableCode}";
                data.TryGetValue(field.CategoryFieldName, out object valueItem);

                if (!string.IsNullOrEmpty(valueItem?.ToString()))
                {
                    var whereCondition = new StringBuilder();
                    var sqlParams = new List<SqlParameter>();
                    int suffix = 0;
                    if (!string.IsNullOrEmpty(field.Filters))
                    {
                        var filters = field.Filters;
                        var pattern = @"@{(?<word>\w+)}";
                        Regex rx = new Regex(pattern);
                        MatchCollection match = rx.Matches(field.Filters);
                        for (int i = 0; i < match.Count; i++)
                        {
                            var fieldName = match[i].Groups["word"].Value;
                            filters = filters.Replace(match[i].Value, data[fieldName]?.ToString());
                        }

                        Clause filterClause = JsonConvert.DeserializeObject<Clause>(filters);
                        if (filterClause != null)
                        {
                            suffix = filterClause.FilterClauseProcess(tableName, tableName, whereCondition, sqlParams, suffix, refValues: data);
                        }
                    }
                    var paramName = $"@{field.RefTableField}_{suffix}";
                    var existSql = $"SELECT F_Id FROM {tableName} WHERE {field.RefTableField} = {paramName}";
                    if (whereCondition.Length > 0)
                    {
                        existSql += $" AND {whereCondition}";
                    }
                    sqlParams.Add(new SqlParameter(paramName, valueItem));
                    var result = await _masterContext.QueryDataTableRaw(existSql, sqlParams.ToArray());
                    bool isExisted = result != null && result.Rows.Count > 0;
                    if (!isExisted)
                    {
                        throw new BadRequestException(CategoryErrorCode.ReferValueNotFound, new string[] { field.Title + ": " + valueItem });
                    }
                }
            }
        }

        private async Task CheckUnique(NonCamelCaseDictionary data, IEnumerable<CategoryField> uniqueFields, string categoryCode, int? fId = null)
        {
            foreach (var field in uniqueFields)
            {
                data.TryGetStringValue(field.CategoryFieldName, out string valueItem);
                if (!string.IsNullOrEmpty(valueItem))
                {
                    var sqlParams = new List<SqlParameter>();
                    var paramName = $"@{field.CategoryFieldName}";
                    var existSql = $"SELECT F_Id FROM v{categoryCode} WHERE {field.CategoryFieldName} = {paramName}";
                    sqlParams.Add(new SqlParameter(paramName, valueItem));
                    if (fId.HasValue)
                    {
                        existSql += $" AND F_Id != {fId}";
                    }
                    var result = await _masterContext.QueryDataTableRaw(existSql, sqlParams.ToArray());
                    bool isExisted = result != null && result.Rows.Count > 0;
                    if (isExisted)
                    {
                        throw new BadRequestException(CategoryErrorCode.UniqueValueAlreadyExisted, new string[] { field.Title });
                    }
                }
            }
        }

        private void CheckValue(NonCamelCaseDictionary data, IEnumerable<CategoryField> categoryFields)
        {
            foreach (var field in categoryFields)
            {
                data.TryGetStringValue(field.CategoryFieldName, out string valueItem);
                if ((field.FormTypeId == (int)EnumFormType.SearchTable
                    || field.FormTypeId == (int)EnumFormType.Select)
                    || field.AutoIncrement
                    || valueItem == null
                    || string.IsNullOrEmpty(valueItem))
                {
                    continue;
                }
                else
                {
                    string regex = ((EnumDataType)field.DataTypeId).GetRegex();
                    if ((field.DataSize > 0 && valueItem.Length > field.DataSize)
                        || (!string.IsNullOrEmpty(regex) && !Regex.IsMatch(valueItem.NormalizeAsInternalName(), regex))
                        || (!string.IsNullOrEmpty(field.RegularExpression) && !Regex.IsMatch(valueItem.NormalizeAsInternalName(), field.RegularExpression)))
                    {
                        throw new BadRequestException(CategoryErrorCode.CategoryValueInValid, new string[] { field.Title });
                    }
                }
            }
        }

        private async Task CheckParentRowAsync(NonCamelCaseDictionary data, CategoryEntity category, int? categoryRowId = null)
        {
            data.TryGetStringValue(CategoryFieldConstants.ParentId, out string value);
            int parentId = 0;
            if (!string.IsNullOrEmpty(value))
            {
                parentId = int.Parse(value);
            }
            if (categoryRowId.HasValue && parentId == categoryRowId)
            {
                throw new BadRequestException(CategoryErrorCode.ParentCategoryFromItSelf);
            }
            if (category.IsTreeView && value != null)
            {
                var existSql = $"SELECT F_Id FROM v{category.CategoryCode} WHERE F_Id = {parentId}";

                var result = await _masterContext.QueryDataTableRaw(existSql, Array.Empty<SqlParameter>());
                bool isExist = result != null && result.Rows.Count > 0;
                if (!isExist)
                {
                    throw new BadRequestException(CategoryErrorCode.ParentCategoryRowNotExisted);
                }
            }
        }

        public async Task<NonCamelCaseDictionary> GetCategoryRow(int categoryId, int fId)
        {
            var category = _masterContext.Category.FirstOrDefault(c => c.CategoryId == categoryId);
            if (category == null)
            {
                throw new BadRequestException(CategoryErrorCode.CategoryNotFound);
            }

            var fields = (from f in _masterContext.CategoryField
                          join c in _masterContext.Category on f.CategoryId equals c.CategoryId
                          where c.CategoryId == category.CategoryId && f.FormTypeId != (int)EnumFormType.ViewOnly && f.FormTypeId != (int)EnumFormType.SqlSelect
                          select f).ToList();
            return await GetCategoryRowInfo(category, fields, fId);
        }

        public async Task<NonCamelCaseDictionary> GetCategoryRow(string categoryCode, int fId)
        {
            var category = _masterContext.Category.FirstOrDefault(c => c.CategoryCode == categoryCode);
            if (category == null)
            {
                throw new BadRequestException(CategoryErrorCode.CategoryNotFound);
            }

            var fields = (from f in _masterContext.CategoryField
                          join c in _masterContext.Category on f.CategoryId equals c.CategoryId
                          where c.CategoryId == category.CategoryId && f.FormTypeId != (int)EnumFormType.ViewOnly && f.FormTypeId != (int)EnumFormType.SqlSelect
                          select f).ToList();
            return await GetCategoryRowInfo(category, fields, fId);
        }

        private async Task<NonCamelCaseDictionary> GetCategoryRowInfo(CategoryEntity category, List<CategoryField> categoryFields, long fId)
        {
            if (category.IsOutSideData)
            {
                categoryFields = categoryFields.Where(f => !f.IsJoinField.HasValue || f.IsJoinField.Value).ToList();
            }

            var tableName = $"v{category.CategoryCode}";
            var dataSql = new StringBuilder();
            dataSql.Append(GetSelect(tableName, categoryFields, category.IsTreeView, category.IsOutSideData));
            dataSql.Append($" FROM {tableName} WHERE [{tableName}].F_Id = {fId}");

            var currentData = await _masterContext.QueryDataTableRaw(dataSql.ToString(), Array.Empty<SqlParameter>());
            var categoryRow = currentData.ConvertData().FirstOrDefault();
            if (categoryRow == null)
            {
                throw new BadRequestException(CategoryErrorCode.CategoryRowNotFound);
            }

            if (categoryRow.ContainsKey(GlobalFieldConstants.SubsidiaryId)
                && Convert.ToInt32(categoryRow[GlobalFieldConstants.SubsidiaryId]) != _currentContextService.SubsidiaryId)
            {
                throw new BadRequestException(CategoryErrorCode.InvalidSubsidiary);
            }
            return categoryRow;
        }

        private string GetSelect(string tableName, List<CategoryField> fields, bool isTreeView, bool isOutSide)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append($"SELECT [{tableName}].F_Id,");
            if (!isOutSide)
            {
                sql.Append($"[{tableName}].UpdatedDatetimeUtc,");
            }
            foreach (var field in fields.Where(f => f.CategoryFieldName != CategoryFieldConstants.F_Id && f.CategoryFieldName != CategoryFieldConstants.ParentId))
            {
                sql.Append($"[{tableName}].{field.CategoryFieldName},");
                if (((EnumFormType)field.FormTypeId).IsJoinForm()
                    && !string.IsNullOrEmpty(field.RefTableCode)
                    && !string.IsNullOrEmpty(field.RefTableTitle))
                {
                    foreach (var item in field.RefTableTitle.Split(","))
                    {
                        var title = item.Trim();
                        sql.Append($"[{tableName}].{field.CategoryFieldName}_{title},");
                    }
                }
            }
            if (isTreeView)
            {
                sql.Append($"[{tableName}].ParentId,");
            }
            sql.Remove(sql.Length - 1, 1);
            return sql.ToString();
        }

        public async Task<PageData<NonCamelCaseDictionary>> GetCategoryRows(string categoryCode, string keyword, Dictionary<int, object> filters, Clause columnsFilters, NonCamelCaseDictionary filterData, string extraFilter, ExtraFilterParam[] extraFilterParams, int page, int size, string orderBy, bool asc)
        {
            var category = _masterContext.Category.FirstOrDefault(c => c.CategoryCode == categoryCode);
            if (category == null)
            {
                throw new BadRequestException(CategoryErrorCode.CategoryNotFound);
            }
            return await GetCategoryRows(category, keyword, filters, columnsFilters, filterData, extraFilter, extraFilterParams, page, size, orderBy, asc);
        }


        public async Task<PageData<NonCamelCaseDictionary>> GetCategoryRows(int categoryId, string keyword, Dictionary<int, object> filters, Clause columnsFilters, NonCamelCaseDictionary filterData, string extraFilter, ExtraFilterParam[] extraFilterParams, int page, int size, string orderBy, bool asc)
        {
            var category = _masterContext.Category.FirstOrDefault(c => c.CategoryId == categoryId);
            if (category == null)
            {
                throw new BadRequestException(CategoryErrorCode.CategoryNotFound);
            }
            return await GetCategoryRows(category, keyword, filters, columnsFilters, filterData, extraFilter, extraFilterParams, page, size, orderBy, asc);
        }

        private async Task<PageData<NonCamelCaseDictionary>> GetCategoryRows(CategoryEntity category, string keyword, Dictionary<int, object> filters, Clause columnsFilters, NonCamelCaseDictionary filterData, string extraFilter, ExtraFilterParam[] extraFilterParams, int page, int size, string orderBy, bool asc)
        {
            keyword = (keyword ?? "").Trim();

            var fields = (from f in _masterContext.CategoryField
                          join c in _masterContext.Category on f.CategoryId equals c.CategoryId
                          where c.CategoryId == category.CategoryId && f.FormTypeId != (int)EnumFormType.ViewOnly && f.FormTypeId != (int)EnumFormType.SqlSelect
                          select f).ToList();

            var viewAlias = $"v";
            var categoryView = $"{GetCategoryView(category, fields, viewAlias)}";

            //fields = fields.Where(f => f.IsShowList).ToList();

            var dataSql = new StringBuilder();
            var sqlParams = new List<SqlParameter>();
            var allDataSql = new StringBuilder();
            dataSql.Append(GetSelect(viewAlias, fields, category.IsTreeView, category.IsOutSideData));
            dataSql.Append($" FROM {categoryView}");
            allDataSql.Append(dataSql.ToString());
            var whereCondition = new StringBuilder();
            if (!string.IsNullOrEmpty(keyword))
            {
                whereCondition.Append("(");
                var idx = 0;
                foreach (var field in fields)
                {
                    if (idx > 0)
                    {
                        whereCondition.Append(" OR ");
                    }

                    if (string.IsNullOrEmpty(field.RefTableCode))
                    {
                        var paramName = $"@{field.CategoryFieldName}_{idx}";
                        sqlParams.Add(new SqlParameter(paramName, $"%{keyword}%"));
                        whereCondition.Append($"[{viewAlias}].{field.CategoryFieldName} LIKE {paramName}");
                    }
                    else
                    {
                        foreach (var item in field.RefTableTitle.Split(","))
                        {
                            var title = item.Trim();
                            var paramName = $"@{field.CategoryFieldName}_{title}_{idx}";
                            sqlParams.Add(new SqlParameter(paramName, $"%{keyword}%"));
                            whereCondition.Append($"[{viewAlias}].{field.CategoryFieldName}_{title} LIKE {paramName}");
                        }
                    }
                    idx++;
                }
                whereCondition.Append(")");
            }

            int suffix = 0;

            var filterClause = new ArrayClause() { Condition = EnumLogicOperator.And, Rules = new List<Clause>() };
            if (filters != null)
            {
                var viewInfo = await _masterContext.CategoryView.Where(c => c.CategoryId == category.CategoryId).OrderByDescending(v => v.IsDefault).FirstOrDefaultAsync();

                var categoryViewId = viewInfo?.CategoryViewId;

                var viewFields = await (
                     from f in _masterContext.CategoryViewField
                     where f.CategoryViewId == categoryViewId
                     select f
                 ).ToListAsync();

                foreach (var filter in filters)
                {
                    var viewField = viewFields.FirstOrDefault(f => f.CategoryViewFieldId == filter.Key);
                    if (viewField == null) continue;

                    var field = fields.FirstOrDefault(f => f.CategoryFieldName?.ToLower() == viewField.ParamerterName?.ToLower());
                    if (field == null) continue;

                    var value = filter.Value;

                    if (value.IsNullOrEmptyObject()) continue;

                    if (new[] { EnumDataType.Date, EnumDataType.Month, EnumDataType.QuarterOfYear, EnumDataType.Year }.Contains((EnumDataType)viewField.DataTypeId))
                    {
                        value = Convert.ToInt64(value);
                    }

                    var dataTypeId = (EnumDataType)viewField.DataTypeId;

                    switch (dataTypeId)
                    {
                        case EnumDataType.Text:
                            filterClause.Rules.Add(new SingleClause()
                            {
                                FieldName = field.CategoryFieldName,
                                DataType = dataTypeId,
                                Operator = EnumOperator.Contains,
                                Value = value
                            });
                            break;

                        case EnumDataType.DateRange:
                            var type = value.GetType();
                            IList<object> values = new List<object>();
                            if (type.IsArray || typeof(System.Collections.IEnumerable).IsAssignableFrom(type))
                            {
                                foreach (object v in (dynamic)value)
                                {
                                    if (!values.Contains(v))
                                        values.Add(v);
                                }
                            }

                            filterClause.Rules.Add(new SingleClause()
                            {
                                FieldName = field.CategoryFieldName,
                                DataType = dataTypeId,
                                Operator = EnumOperator.GreaterOrEqual,
                                Value = values[0]
                            });

                            filterClause.Rules.Add(new SingleClause()
                            {
                                FieldName = field.CategoryFieldName,
                                DataType = dataTypeId,
                                Operator = EnumOperator.LessThanOrEqual,
                                Value = values[1]
                            });
                            break;
                        default:
                            filterClause.Rules.Add(new SingleClause()
                            {
                                FieldName = field.CategoryFieldName,
                                DataType = dataTypeId,
                                Operator = EnumOperator.Equal,
                                Value = value
                            });
                            break;
                    }



                }

                if (filterClause != null && filterClause.Rules.Count > 0)
                {
                    if (whereCondition.Length > 0)
                    {
                        whereCondition.Append(" AND ");
                    }

                    suffix = filterClause.FilterClauseProcess(GetCategoryViewName(category), viewAlias, whereCondition, sqlParams, suffix, false);
                }

            }

            if (columnsFilters != null)
            {
                if (whereCondition.Length > 0) whereCondition.Append(" AND ");

                suffix = columnsFilters.FilterClauseProcess(GetCategoryViewName(category), viewAlias, whereCondition, sqlParams, suffix, refValues: filterData);
            }

            if (!string.IsNullOrEmpty(extraFilter))
            {
                if (whereCondition.Length > 0) whereCondition.Append(" AND ");
                var protector = _protectionProvider.CreateProtector(_appSetting.ExtraFilterEncryptPepper);
                extraFilter = protector.Unprotect(extraFilter);
                whereCondition.Append(extraFilter);
                var pattern = @"@(?<word>\w+)";
                Regex rx = new Regex(pattern);
                MatchCollection match = rx.Matches(extraFilter);
                for (int i = 0; i < match.Count; i++)
                {
                    var word = match[i].Groups["word"].Value;
                    var paramName = $"@{word}";
                    if (sqlParams.Any(p => p.ParameterName == paramName) || paramName == "@SubId") continue;
                    var param = extraFilterParams.FirstOrDefault(p => p.ParamName == word);
                    object value = param != null ? param.DataType.GetSqlValue(param.Value) : DBNull.Value;
                    sqlParams.Add(new SqlParameter(paramName, value));
                }
            }

            var sumColumns = fields.Where(f => f.IsCalcSum).ToList();

            var sumExpColumns = sumColumns.Select(f => $"SUM({f.CategoryFieldName}) AS {f.CategoryFieldName}").ToArray();

            var totalSql = new StringBuilder($"SELECT COUNT(F_Id) as Total");
            if (sumExpColumns.Length > 0)
            {
                totalSql.Append($", {string.Join(",", sumExpColumns)} ");
            }
            totalSql.Append($" FROM {categoryView} ");

            if (whereCondition.Length > 2)
            {
                dataSql.Append($" WHERE {whereCondition}");
                totalSql.Append($" WHERE {whereCondition}");
            }

            var countTable = await _masterContext.QueryDataTableRaw(totalSql.ToString(), sqlParams.Select(p => p.CloneSqlParam()).ToArray());
            var total = 0;
            var additionResult = new NonCamelCaseDictionary();
            if (countTable != null && countTable.Rows.Count > 0)
            {
                total = (countTable.Rows[0]["Total"] as int?).GetValueOrDefault();
                foreach (var c in sumColumns)
                {
                    additionResult.Add(c.CategoryFieldName, countTable.Rows[0][c.CategoryFieldName]);
                }
            }

            if (!category.IsTreeView)
            {
                dataSql.Append(string.IsNullOrEmpty(orderBy) ? string.IsNullOrEmpty(category.DefaultOrder) ? $" ORDER BY [{viewAlias}].F_Id" : $" ORDER BY {category.DefaultOrder}" : $" ORDER BY [{viewAlias}].{orderBy} {(asc ? "" : "DESC")}");
                if (size > 0)
                {
                    dataSql.Append($" OFFSET {(page - 1) * size} ROWS FETCH NEXT {size} ROWS ONLY;");
                }
            }
            var data = await _masterContext.QueryDataTableRaw(dataSql.ToString(), sqlParams.Select(p => p.CloneSqlParam()).ToArray());
            var lstData = data.ConvertData();

            if (category.IsTreeView)
            {
                var allData = await _masterContext.QueryDataTableRaw(allDataSql.ToString(), Array.Empty<SqlParameter>());
                var lstAll = allData.ConvertData();

                AddParents(ref lstData, lstAll);

                lstData = SortCategoryRows(lstData);
                if (size > 0)
                {
                    lstData = lstData.Skip((page - 1) * size).Take(size).ToList();
                }
            }

            return (lstData, total, additionResult);
        }

        private void AddParents(ref List<NonCamelCaseDictionary> categoryRows, List<NonCamelCaseDictionary> lstAll)
        {

            var ids = categoryRows.Select(r => (int)r[CategoryFieldConstants.F_Id]).ToList();
            var parentIds = categoryRows.Where(r => !r[CategoryFieldConstants.ParentId].IsNullOrEmptyObject()).Select(r => (int)r[CategoryFieldConstants.ParentId]).Where(id => !ids.Contains(id)).ToList();
            while (parentIds.Count > 0)
            {
                var parents = lstAll.Where(r => parentIds.Contains((int)r[CategoryFieldConstants.F_Id])).ToList();
                foreach (var parent in parents)
                {
                    parent["IsDisable"] = true;
                    categoryRows.Add(parent);
                    ids.Add((int)parent[CategoryFieldConstants.F_Id]);
                }
                parentIds = parents.Where(r => !r[CategoryFieldConstants.ParentId].IsNullOrEmptyObject()).Select(r => (int)r[CategoryFieldConstants.ParentId]).Where(id => !ids.Contains(id)).ToList();
            }
        }

        private List<NonCamelCaseDictionary> SortCategoryRows(List<NonCamelCaseDictionary> categoryRows)
        {
            int level = 0;
            categoryRows = categoryRows.OrderBy(r => (int)r[CategoryFieldConstants.F_Id]).ToList();
            List<NonCamelCaseDictionary> nodes = new List<NonCamelCaseDictionary>();

            var items = categoryRows.Where(r => r[CategoryFieldConstants.ParentId].IsNullOrEmptyObject() || !categoryRows.Any(p => (int)p[CategoryFieldConstants.F_Id] == (int)r[CategoryFieldConstants.ParentId])).ToList();
            categoryRows = categoryRows.Where(r => !r[CategoryFieldConstants.ParentId].IsNullOrEmptyObject() && categoryRows.Any(p => (int)p[CategoryFieldConstants.F_Id] == (int)r[CategoryFieldConstants.ParentId])).ToList();

            foreach (var item in items)
            {
                item["CategoryRowLevel"] = level;
                nodes.Add(item);
                nodes.AddRange(GetChilds(ref categoryRows, (int)item[CategoryFieldConstants.F_Id], level));
            }

            return nodes;
        }

        private IEnumerable<NonCamelCaseDictionary> GetChilds(ref List<NonCamelCaseDictionary> categoryRows, int categoryRowId, int level)
        {
            level++;
            List<NonCamelCaseDictionary> nodes = new List<NonCamelCaseDictionary>();
            var items = categoryRows.Where(r => !r[CategoryFieldConstants.ParentId].IsNullOrEmptyObject() && (int)r[CategoryFieldConstants.ParentId] == categoryRowId).ToList();
            categoryRows.RemoveAll(r => !r[CategoryFieldConstants.ParentId].IsNullOrEmptyObject() && (int)r[CategoryFieldConstants.ParentId] == categoryRowId);
            foreach (var item in items)
            {
                item["CategoryRowLevel"] = level;
                nodes.Add(item);
                nodes.AddRange(GetChilds(ref categoryRows, (int)item[CategoryFieldConstants.F_Id], level));
            }
            return nodes;
        }

        private string GetCategoryView(CategoryEntity category, List<CategoryField> fields, string viewAlias = "")
        {
            var categoryView = GetCategoryViewName(category);

            if (string.IsNullOrWhiteSpace(viewAlias))
            {
                viewAlias = categoryView;
            }

            var select = $"{GetSelect(categoryView, fields, category.IsTreeView, category.IsOutSideData)}";

            if (fields.Any(f => f.CategoryFieldName == GlobalFieldConstants.SubsidiaryId))
            {
                return $"(SELECT * FROM {categoryView} WHERE {categoryView}.[{GlobalFieldConstants.SubsidiaryId}]={_currentContextService.SubsidiaryId}) as {viewAlias}";
            }
            else
            {
                if (categoryView != viewAlias)
                {
                    return $"{categoryView} {viewAlias}";
                }
                else
                {
                    return GetCategoryViewName(category);
                }
            }
        }

        private string GetCategoryViewName(CategoryEntity category)
        {
            if (category.IsOutSideData && !string.IsNullOrWhiteSpace(category.SearchSqlRaw))
            {
                return $"v{category.CategoryCode}_Search";
            }

            return $"v{category.CategoryCode}";
        }

        public async Task<List<MapObjectOutputModel>> MapToObject(MapObjectInputModel[] categoryValues)
        {
            List<MapObjectOutputModel> titles = new List<MapObjectOutputModel>();
            var groupByCodes = categoryValues.Where(v => !string.IsNullOrEmpty(v.Value)).Distinct().GroupBy(v => new { v.CategoryCode });

            foreach (var group in groupByCodes)
            {
                var category = _masterContext.Category.First(c => c.CategoryCode == group.Key.CategoryCode);

                var fields = (from f in _masterContext.CategoryField
                              join c in _masterContext.Category on f.CategoryId equals c.CategoryId
                              where c.CategoryCode == category.CategoryCode
                              select f).ToList();

                var viewAlias = $"v";
                var categoryView = $"{GetCategoryView(category, fields, viewAlias)}";

                var selectCondition = $"{GetSelect(viewAlias, fields, category.IsTreeView, category.IsOutSideData)} FROM {categoryView} ";
                var groupByFilters = group.GroupBy(v => new { v.Filters });
                foreach (var groupByFilter in groupByFilters)
                {
                    var dataSql = new StringBuilder(selectCondition);
                    var sqlParams = new List<SqlParameter>();
                    int suffix = 0;
                    dataSql.Append("WHERE (");
                    var groupByFields = group.GroupBy(v => new { v.CategoryFieldName });
                    foreach (var groupByField in groupByFields)
                    {
                        var inputField = fields.First(f => f.CategoryFieldName == groupByField.Key.CategoryFieldName);
                        var values = groupByField.Select(v => ((EnumDataType)inputField.DataTypeId).GetSqlValue(v.Value)).Distinct().ToList();

                        if (values.Count() > 0)
                        {
                            if (suffix > 0)
                            {
                                dataSql.Append(" OR ");
                            }

                            var pName = $"@{inputField.CategoryFieldName}_in_{suffix}";

                            SqlParameter sqlParam;

                            dataSql.Append($" [{viewAlias}].{inputField.CategoryFieldName} IN (");

                            switch ((EnumDataType)inputField.DataTypeId)
                            {
                                case EnumDataType.BigInt:
                                    dataSql.Append($"SELECT [Value] FROM {pName}");
                                    sqlParam = values.Select(v => (long)v).ToList().ToSqlParameter(pName);
                                    break;
                                default:
                                    dataSql.Append($"SELECT [NValue] FROM {pName}");
                                    sqlParam = values.Select(v => v?.ToString()).ToList().ToSqlParameter(pName);
                                    break;

                            }

                            dataSql.Append(")");

                            sqlParams.Add(sqlParam);
                            suffix++;
                        }
                    }
                    dataSql.Append(")");
                    if (!string.IsNullOrEmpty(groupByFilter.Key.Filters))
                    {
                        Clause filterClause = JsonConvert.DeserializeObject<Clause>(groupByFilter.Key.Filters);
                        if (filterClause != null)
                        {
                            dataSql.Append(" AND ");
                            suffix = filterClause.FilterClauseProcess(GetCategoryViewName(category), viewAlias, dataSql, sqlParams, suffix);
                        }
                    }

                    var data = await _masterContext.QueryDataTableRaw(dataSql.ToString(), sqlParams.ToArray());
                    var lst = data.ConvertData();

                    foreach (var item in groupByFilter)
                    {
                        var referObject = lst.FirstOrDefault(o => o[item.CategoryFieldName].ToString() == item.Value);
                        if (referObject != null)
                        {
                            titles.Add(new MapObjectOutputModel
                            {
                                CategoryTitle = item.CategoryTitle,
                                CategoryCode = item.CategoryCode,
                                Filters = item.Filters,
                                CategoryFieldName = item.CategoryFieldName,
                                Value = item.Value,
                                ReferObject = referObject
                            });
                        }
                    }
                }
            }

            return titles;
        }

        public async Task<bool> ImportCategoryRowFromMapping(int categoryId, ImportExcelMapping mapping, Stream stream)
        {
            var facade = new CategoryDataImportFacade(categoryId, _masterContext, this, _categoryDataActivityLog, _currentContextService);
            await facade.ImportData(longTaskResourceLockService, mapping, stream);

            return true;

        }

    }
}
