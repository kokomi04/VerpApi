using AutoMapper;
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
using VErp.Commons.Constants;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Model.Category;
using Microsoft.AspNetCore.DataProtection;

using CategoryEntity = VErp.Infrastructure.EF.MasterDB.Category;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Services.Master.Service.Category;

namespace VErp.Services.Accountancy.Service.Category
{
    public class CategoryDataService : ICategoryDataService
    {
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly AppSetting _appSetting;
        private readonly IMapper _mapper;
        private readonly MasterDBContext _accountancyContext;
        private readonly ICurrentContextService _currentContextService;
        private readonly IDataProtectionProvider _protectionProvider;
        private readonly ICustomGenCodeHelperService _customGenCodeHelperService;
        public CategoryDataService(MasterDBContext accountancyContext
            , IOptions<AppSetting> appSetting
            , ILogger<CategoryDataService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            , ICurrentContextService currentContextService
            , IDataProtectionProvider protectionProvider
            , ICustomGenCodeHelperService customGenCodeHelperService
            )
        {
            _logger = logger;
            _activityLogService = activityLogService;
            _accountancyContext = accountancyContext;
            _appSetting = appSetting.Value;
            _mapper = mapper;
            _currentContextService = currentContextService;
            _protectionProvider = protectionProvider;
            _customGenCodeHelperService = customGenCodeHelperService;
        }

        public async Task<int> AddCategoryRow(int categoryId, Dictionary<string, string> data)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockCategoryKey(categoryId));
            var category = _accountancyContext.Category.FirstOrDefault(c => c.CategoryId == categoryId);
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
            var categoryFields = _accountancyContext.CategoryField
                .Where(f => category.CategoryId == f.CategoryId && f.FormTypeId != (int)EnumFormType.ViewOnly)
                .ToList();

            await FillGenerateColumn(categoryFields, data);

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

            // Insert data
            var dataTable = new DataTable(category.CategoryCode);

            if (category.IsTreeView)
            {
                dataTable.Columns.Add("ParentId", typeof(int));
            }
            dataTable.Columns.Add("CreatedByUserId", typeof(int));
            dataTable.Columns.Add("CreatedDatetimeUtc", typeof(DateTime));
            dataTable.Columns.Add("UpdatedByUserId", typeof(int));
            dataTable.Columns.Add("UpdatedDatetimeUtc", typeof(DateTime));
            dataTable.Columns.Add("IsDeleted", typeof(bool));
            dataTable.Columns.Add("DeletedDatetimeUtc", typeof(DateTime));

            foreach (var field in categoryFields)
            {
                if (field.CategoryFieldName == "F_Id") continue;

                dataTable.Columns.Add(field.CategoryFieldName, ((EnumDataType)field.DataTypeId).GetColumnDataType());
            }

            var dataRow = dataTable.NewRow();
            if (category.IsTreeView)
            {
                data.TryGetValue("ParentId", out string value);
                if (!string.IsNullOrEmpty(value))
                {
                    dataRow["ParentId"] = int.Parse(value);
                }
                else
                {
                    dataRow["ParentId"] = DBNull.Value;
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
                if (field.CategoryFieldName == "F_Id") continue;
                data.TryGetValue(field.CategoryFieldName, out string value);
                dataRow[field.CategoryFieldName] = ((EnumDataType)field.DataTypeId).GetSqlValue(value);

                if (field.CategoryFieldName == GlobalFieldConstants.SubsidiaryId)
                {
                    dataRow[field.CategoryFieldName] = ((EnumDataType)field.DataTypeId).GetSqlValue(_currentContextService.SubsidiaryId);
                }
            }
            dataTable.Rows.Add(dataRow);

            var id = await _accountancyContext.InsertDataTable(dataTable);
            await _activityLogService.CreateLog(EnumObjectType.Category, id, $"Thêm mới dữ liệu danh mục {id}", data.JsonSerialize());
            return (int)id;
        }

        public async Task<int> UpdateCategoryRow(int categoryId, int fId, Dictionary<string, string> data)
        {
            var category = _accountancyContext.Category.FirstOrDefault(c => c.CategoryId == categoryId);
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

            var categoryFields = _accountancyContext.CategoryField.Where(f => f.CategoryId == category.CategoryId && f.FormTypeId != (int)EnumFormType.ViewOnly && f.CategoryFieldName != "F_Id").ToList();

            var categoryRow = await GetCategoryRowInfo(category, categoryFields, fId);

            bool isParentChange = false;
            // Check parent row
            if (category.IsTreeView)
            {
                categoryRow.TryGetValue("ParentId", out object oParent);
                string cParent = oParent?.ToString() ?? string.Empty;
                data.TryGetValue("ParentId", out string uParent);
                uParent ??= string.Empty;
                if (isParentChange = cParent != uParent)
                {
                    await CheckParentRowAsync(data, category, fId);
                }
            }

            // Lấy các trường thay đổi
            List<CategoryField> updateFields = new List<CategoryField>();
            foreach (CategoryField categoryField in categoryFields)
            {
                var currentValue = categoryRow[categoryField.CategoryFieldName].ToString();
                var updateValue = data[categoryField.CategoryFieldName];
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

            // Update data
            var dataTable = new DataTable(category.CategoryCode);

            if (isParentChange)
            {
                dataTable.Columns.Add("ParentId", typeof(int));
            }
            dataTable.Columns.Add("UpdatedByUserId", typeof(int));
            dataTable.Columns.Add("UpdatedDatetimeUtc", typeof(DateTime));

            foreach (var field in updateFields)
            {
                if (field.CategoryFieldName == "F_Id") continue;
                dataTable.Columns.Add(field.CategoryFieldName, ((EnumDataType)field.DataTypeId).GetColumnDataType());
            }

            var dataRow = dataTable.NewRow();

            if (isParentChange)
            {
                data.TryGetValue("ParentId", out string value);
                if (!string.IsNullOrEmpty(value))
                {
                    dataRow["ParentId"] = int.Parse(value);
                }
                else
                {
                    dataRow["ParentId"] = DBNull.Value;
                }
            }

            dataRow["UpdatedByUserId"] = _currentContextService.UserId;
            dataRow["UpdatedDatetimeUtc"] = DateTime.UtcNow;

            foreach (var field in updateFields)
            {
                if (field.CategoryFieldName == "F_Id") continue;
                data.TryGetValue(field.CategoryFieldName, out string value);
                dataRow[field.CategoryFieldName] = ((EnumDataType)field.DataTypeId).GetSqlValue(value);

                if (field.CategoryFieldName == GlobalFieldConstants.SubsidiaryId)
                {
                    dataRow[field.CategoryFieldName] = ((EnumDataType)field.DataTypeId).GetSqlValue(_currentContextService.SubsidiaryId);
                }
            }
            dataTable.Rows.Add(dataRow);

            int numberChange = await _accountancyContext.UpdateCategoryData(dataTable, fId);
            await _activityLogService.CreateLog(EnumObjectType.Category, fId, $"Cập nhật dữ liệu danh mục {fId}", data.JsonSerialize());
            return numberChange;
        }



        private async Task FillGenerateColumn(ICollection<CategoryField> fields, Dictionary<string, string> data)
        {
            foreach (var field in fields.Where(f => f.FormTypeId == (int)EnumFormType.Generate))
            {
                if ((!data.TryGetValue(field.CategoryFieldName, out var value) || value.IsNullObject()))
                {
                    try
                    {
                        CustomGenCodeOutputModelOut currentConfig = await _customGenCodeHelperService.CurrentConfig(EnumObjectType.Category, field.CategoryFieldId);

                        if (currentConfig == null)
                        {
                            throw new BadRequestException(GeneralCode.ItemNotFound, "Thiết định cấu hình sinh mã null " + field.Title);
                        }

                        var generated = await _customGenCodeHelperService.GenerateCode(currentConfig.CustomGenCodeId, currentConfig.LastValue);

                        if (generated == null)
                        {
                            throw new BadRequestException(GeneralCode.InternalError, "Không thể sinh mã " + field.Title);
                        }

                        value = generated.CustomCode;

                        if (!data.ContainsKey(field.CategoryFieldName))
                        {
                            data.Add(field.CategoryFieldName, value);
                        }
                        else
                        {
                            data[field.CategoryFieldName] = value;
                        }
                    }
                    catch (BadRequestException badRequest)
                    {
                        throw new BadRequestException(badRequest.Code, "Cấu hình sinh mã " + field.Title + " => " + badRequest.Message);
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
            var category = _accountancyContext.Category.FirstOrDefault(c => c.CategoryId == categoryId);
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
                var result = await _accountancyContext.QueryDataTable(existSql, Array.Empty<SqlParameter>());
                bool isExisted = result != null && result.Rows.Count > 0;
                if (isExisted)
                {
                    throw new BadRequestException(CategoryErrorCode.RelationshipAlreadyExisted);
                }
            }

            // Get row
            var categoryFields = _accountancyContext.CategoryField.Where(f => f.CategoryId == categoryId && f.FormTypeId != (int)EnumFormType.ViewOnly).ToList();

            var categoryRow = await GetCategoryRowInfo(category, categoryFields, fId);

            var fieldNames = categoryFields.Select(f => f.CategoryFieldName).ToList();

            // Category refer fields
            var referToFields = _accountancyContext.CategoryField.Where(f => f.RefTableCode == category.CategoryCode && fieldNames.Contains(f.RefTableField)).ToList();
            var referToCategoryIds = referToFields.Select(f => f.CategoryId).Distinct().ToList();
            var referToCategories = _accountancyContext.Category.Where(c => referToCategoryIds.Contains(c.CategoryId)).ToList();

            // Bill refer fields

            // TODO
            //var inputReferToFields = _accountancyContext.InputField.Where(f => f.RefTableCode == category.CategoryCode && fieldNames.Contains(f.RefTableField)).ToList();

            // Check reference
            foreach (var field in categoryFields)
            {
                categoryRow.TryGetValue(field.CategoryFieldName, out object value);
                if (value == null) continue;
                foreach (var referToField in referToFields.Where(c => c.RefTableField == field.CategoryFieldName))
                {
                    var referToCategory = referToCategories.First(c => c.CategoryId == referToField.CategoryId);
                    var referToTable = $"v{referToCategory.CategoryCode}";

                    var existSql = $"SELECT F_Id FROM [dbo].v{referToTable} WHERE {referToField.CategoryFieldName} = {value.ToString()};";
                    var result = await _accountancyContext.QueryDataTable(existSql, Array.Empty<SqlParameter>());
                    bool isExisted = result != null && result.Rows.Count > 0;
                    if (isExisted)
                    {
                        throw new BadRequestException(CategoryErrorCode.RelationshipAlreadyExisted);
                    }
                }

                // TODO
                // check bill refer
                //foreach (var referToField in inputReferToFields.Where(f => f.RefTableField == field.CategoryFieldName))
                //{
                //    var existSql = $"SELECT tk.F_Id FROM [dbo]._tk tk WHERE tk.{referToField.FieldName} = {value.ToString()};";
                //    var result = await _accountancyContext.QueryDataTable(existSql, Array.Empty<SqlParameter>());
                //    bool isExisted = result != null && result.Rows.Count > 0;
                //    if (isExisted)
                //    {
                //        throw new BadRequestException(CategoryErrorCode.RelationshipAlreadyExisted);
                //    }
                //}

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
            int numberChange = await _accountancyContext.UpdateCategoryData(dataTable, fId);
            await _activityLogService.CreateLog(EnumObjectType.Category, fId, $"Xóa dòng dữ liệu {fId}", categoryRow.JsonSerialize());
            return numberChange;
        }

        private void CheckRequired(Dictionary<string, string> data, IEnumerable<CategoryField> requiredFields)
        {
            foreach (var field in requiredFields)
            {
                // ignore auto generate field
                if (field.FormTypeId == (int)EnumFormType.Generate) continue;

                if (!data.Any(v => v.Key == field.CategoryFieldName && !string.IsNullOrEmpty(v.Value)))
                {
                    throw new BadRequestException(CategoryErrorCode.RequiredFieldIsEmpty, new string[] { field.Title });
                }
            }
        }

        private async Task CheckRefer(Dictionary<string, string> data, IEnumerable<CategoryField> selectFields)
        {
            // Check refer
            foreach (var field in selectFields)
            {
                string tableName = $"v{field.RefTableCode}";
                data.TryGetValue(field.CategoryFieldName, out string valueItem);

                if (!string.IsNullOrEmpty(valueItem))
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
                            filters = filters.Replace(match[i].Value, data[fieldName]);
                        }

                        Clause filterClause = JsonConvert.DeserializeObject<Clause>(filters);
                        if (filterClause != null)
                        {
                            filterClause.FilterClauseProcess(tableName, tableName, ref whereCondition, ref sqlParams, ref suffix);
                        }
                    }
                    var paramName = $"@{field.RefTableField}_{suffix}";
                    var existSql = $"SELECT F_Id FROM {tableName} WHERE {field.RefTableField} = {paramName}";
                    if (whereCondition.Length > 0)
                    {
                        existSql += $" AND {whereCondition.ToString()}";
                    }
                    sqlParams.Add(new SqlParameter(paramName, valueItem));
                    var result = await _accountancyContext.QueryDataTable(existSql, sqlParams.ToArray());
                    bool isExisted = result != null && result.Rows.Count > 0;
                    if (!isExisted)
                    {
                        throw new BadRequestException(CategoryErrorCode.ReferValueNotFound, new string[] { field.Title });
                    }
                }
            }
        }

        private async Task CheckUnique(Dictionary<string, string> data, IEnumerable<CategoryField> uniqueFields, string categoryCode, int? fId = null)
        {
            foreach (var field in uniqueFields)
            {
                data.TryGetValue(field.CategoryFieldName, out string valueItem);
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
                    var result = await _accountancyContext.QueryDataTable(existSql, sqlParams.ToArray());
                    bool isExisted = result != null && result.Rows.Count > 0;
                    if (isExisted)
                    {
                        throw new BadRequestException(CategoryErrorCode.UniqueValueAlreadyExisted, new string[] { field.Title });
                    }
                }
            }
        }

        private void CheckValue(Dictionary<string, string> data, IEnumerable<CategoryField> categoryFields)
        {
            foreach (var field in categoryFields)
            {
                data.TryGetValue(field.CategoryFieldName, out string valueItem);
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
                        || (!string.IsNullOrEmpty(regex) && !Regex.IsMatch(valueItem, regex))
                        || (!string.IsNullOrEmpty(field.RegularExpression) && !Regex.IsMatch(valueItem, field.RegularExpression)))
                    {
                        throw new BadRequestException(CategoryErrorCode.CategoryValueInValid, new string[] { field.Title });
                    }
                }
            }
        }

        private async Task CheckParentRowAsync(Dictionary<string, string> data, CategoryEntity category, int? categoryRowId = null)
        {
            data.TryGetValue("ParentId", out string value);
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

                var result = await _accountancyContext.QueryDataTable(existSql, Array.Empty<SqlParameter>());
                bool isExist = result != null && result.Rows.Count > 0;
                if (!isExist)
                {
                    throw new BadRequestException(CategoryErrorCode.ParentCategoryRowNotExisted);
                }
            }
        }

        public async Task<NonCamelCaseDictionary> GetCategoryRow(int categoryId, int fId)
        {
            var category = _accountancyContext.Category.FirstOrDefault(c => c.CategoryId == categoryId);
            if (category == null)
            {
                throw new BadRequestException(CategoryErrorCode.CategoryNotFound);
            }

            var fields = (from f in _accountancyContext.CategoryField
                          join c in _accountancyContext.Category on f.CategoryId equals c.CategoryId
                          where c.CategoryId == category.CategoryId && f.FormTypeId != (int)EnumFormType.ViewOnly
                          select f).ToList();
            return await GetCategoryRowInfo(category, fields, fId);
        }

        private async Task<NonCamelCaseDictionary> GetCategoryRowInfo(CategoryEntity category, List<CategoryField> categoryFields, long fId)
        {
            var tableName = $"v{category.CategoryCode}";
            var dataSql = new StringBuilder();
            dataSql.Append(GetSelect(tableName, categoryFields, category.IsTreeView));
            dataSql.Append($" FROM {tableName} WHERE [{tableName}].F_Id = {fId}");

            var currentData = await _accountancyContext.QueryDataTable(dataSql.ToString(), Array.Empty<SqlParameter>());
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


        private string GetSelect(string tableName, List<CategoryField> fields, bool isTreeView)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append($"SELECT [{tableName}].F_Id,");
            foreach (var field in fields.Where(f => f.CategoryFieldName != "F_Id" && f.CategoryFieldName != "ParentId"))
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

        public async Task<PageData<NonCamelCaseDictionary>> GetCategoryRows(int categoryId, string keyword, string filters, string extraFilter, ExtraFilterParam[] extraFilterParams, int page, int size)
        {
            var category = _accountancyContext.Category.FirstOrDefault(c => c.CategoryId == categoryId);
            if (category == null)
            {
                throw new BadRequestException(CategoryErrorCode.CategoryNotFound);
            }
            return await GetCategoryRows(category, keyword, filters, extraFilter, extraFilterParams, page, size);
        }

        private async Task<PageData<NonCamelCaseDictionary>> GetCategoryRows(CategoryEntity category, string keyword, string filters, string extraFilter, ExtraFilterParam[] extraFilterParams, int page, int size)
        {

            var fields = (from f in _accountancyContext.CategoryField
                          join c in _accountancyContext.Category on f.CategoryId equals c.CategoryId
                          where c.CategoryId == category.CategoryId && f.FormTypeId != (int)EnumFormType.ViewOnly && f.IsShowList == true
                          select f).ToList();

            var viewAlias = $"v";
            var categoryView = $"{GetCategoryView(category, fields, viewAlias)}";

            var dataSql = new StringBuilder();
            var sqlParams = new List<SqlParameter>();
            var allDataSql = new StringBuilder();
            dataSql.Append(GetSelect(viewAlias, fields, category.IsTreeView));
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

            if (!string.IsNullOrEmpty(filters))
            {
                Clause filterClause = JsonConvert.DeserializeObject<Clause>(filters);
                if (filterClause != null)
                {
                    if (whereCondition.Length > 0) whereCondition.Append(" AND ");
                    int suffix = 0;
                    filterClause.FilterClauseProcess(GetCategoryViewName(category.CategoryCode), viewAlias, ref whereCondition, ref sqlParams, ref suffix);
                }
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
                    if (sqlParams.Any(p => p.ParameterName == paramName)) continue;
                    var param = extraFilterParams.FirstOrDefault(p => p.ParamName == word);
                    object value = param != null ? param.DataType.GetSqlValue(param.Value) : DBNull.Value;
                    sqlParams.Add(new SqlParameter(paramName, value));
                }
            }

            var totalSql = new StringBuilder($"SELECT COUNT(F_Id) as Total FROM {categoryView}");

            if (whereCondition.Length > 0)
            {
                dataSql.Append($" WHERE {whereCondition}");
                totalSql.Append($" WHERE {whereCondition}");
            }

            var countTable = await _accountancyContext.QueryDataTable(totalSql.ToString(), sqlParams.Select(p => p.CloneSqlParam()).ToArray());
            var total = 0;
            if (countTable != null && countTable.Rows.Count > 0)
            {
                total = (countTable.Rows[0]["Total"] as int?).GetValueOrDefault();
            }

            if (!category.IsTreeView)
            {
                dataSql.Append($" ORDER BY [{viewAlias}].F_Id");
                if (size > 0)
                {
                    dataSql.Append($" OFFSET {(page - 1) * size} ROWS FETCH NEXT {size} ROWS ONLY;");
                }
            }
            var data = await _accountancyContext.QueryDataTable(dataSql.ToString(), sqlParams.Select(p => p.CloneSqlParam()).ToArray());
            var lstData = data.ConvertData();

            if (category.IsTreeView)
            {
                var allData = await _accountancyContext.QueryDataTable(allDataSql.ToString(), Array.Empty<SqlParameter>());
                var lstAll = allData.ConvertData();

                AddParents(ref lstData, lstAll);

                lstData = SortCategoryRows(lstData);
                if (size > 0)
                {
                    lstData = lstData.Skip((page - 1) * size).Take(size).ToList();
                }
            }

            return (lstData, total);
        }

        private void AddParents(ref List<NonCamelCaseDictionary> categoryRows, List<NonCamelCaseDictionary> lstAll)
        {
            List<NonCamelCaseDictionary> result = new List<NonCamelCaseDictionary>();

            var ids = categoryRows.Select(r => (int)r["F_Id"]).ToList();
            var parentIds = categoryRows.Where(r => r["ParentId"] != DBNull.Value).Select(r => (int)r["ParentId"]).Where(id => !ids.Contains(id)).ToList();
            while (parentIds.Count > 0)
            {
                var parents = lstAll.Where(r => parentIds.Contains((int)r["F_Id"])).ToList();
                foreach (var parent in parents)
                {
                    parent["IsDisable"] = true;
                    categoryRows.Add(parent);
                    ids.Add((int)parent["F_Id"]);
                }
                parentIds = parents.Where(r => r["ParentId"] != DBNull.Value).Select(r => (int)r["ParentId"]).Where(id => !ids.Contains(id)).ToList();
            }
        }

        private List<NonCamelCaseDictionary> SortCategoryRows(List<NonCamelCaseDictionary> categoryRows)
        {
            int level = 0;
            categoryRows = categoryRows.OrderBy(r => (int)r["F_Id"]).ToList();
            List<NonCamelCaseDictionary> nodes = new List<NonCamelCaseDictionary>();

            var items = categoryRows.Where(r => r["ParentId"] == DBNull.Value || !categoryRows.Any(p => (int)p["F_Id"] == (int)r["ParentId"])).ToList();
            categoryRows = categoryRows.Where(r => r["ParentId"] != DBNull.Value && categoryRows.Any(p => (int)p["F_Id"] == (int)r["ParentId"])).ToList();

            foreach (var item in items)
            {
                item["CategoryRowLevel"] = level;
                nodes.Add(item);
                nodes.AddRange(GetChilds(ref categoryRows, (int)item["F_Id"], level));
            }

            return nodes;
        }

        private IEnumerable<NonCamelCaseDictionary> GetChilds(ref List<NonCamelCaseDictionary> categoryRows, int categoryRowId, int level)
        {
            level++;
            List<NonCamelCaseDictionary> nodes = new List<NonCamelCaseDictionary>();
            var items = categoryRows.Where(r => r["ParentId"] != DBNull.Value && (int)r["ParentId"] == categoryRowId).ToList();
            categoryRows.RemoveAll(r => r["ParentId"] != DBNull.Value && (int)r["ParentId"] == categoryRowId);
            foreach (var item in items)
            {
                item["CategoryRowLevel"] = level;
                nodes.Add(item);
                nodes.AddRange(GetChilds(ref categoryRows, (int)item["F_Id"], level));
            }
            return nodes;
        }

        private string GetCategoryView(CategoryEntity category, List<CategoryField> fields, string viewAlias = "")
        {
            var categoryView = GetCategoryViewName(category.CategoryCode);

            if (string.IsNullOrWhiteSpace(viewAlias))
            {
                viewAlias = categoryView;
            }

            var select = $"{GetSelect(categoryView, fields, category.IsTreeView)}";

            if (fields.Any(f => f.CategoryFieldName == GlobalFieldConstants.SubsidiaryId))
            {
                return $"(SELECT * FROM {categoryView} WHERE {categoryView}.[{GlobalFieldConstants.SubsidiaryId}]={_currentContextService.SubsidiaryId} as {viewAlias}";
            }
            else
            {
                if (categoryView != viewAlias)
                {
                    return $"{categoryView} {viewAlias}";
                }
                else
                {
                    return GetCategoryViewName(category.CategoryCode);
                }
            }
        }

        private string GetCategoryViewName(string categoryCode)
        {
            return $"v{categoryCode}";
        }

        public async Task<List<MapObjectOutputModel>> MapToObject(MapObjectInputModel[] categoryValues)
        {
            List<MapObjectOutputModel> titles = new List<MapObjectOutputModel>();
            var groupByCodes = categoryValues.Where(v => !string.IsNullOrEmpty(v.Value)).Distinct().GroupBy(v => new { v.CategoryCode });

            foreach (var group in groupByCodes)
            {
                var category = _accountancyContext.Category.First(c => c.CategoryCode == group.Key.CategoryCode);

                var fields = (from f in _accountancyContext.CategoryField
                              join c in _accountancyContext.Category on f.CategoryId equals c.CategoryId
                              where c.CategoryCode == category.CategoryCode
                              select f).ToList();

                var viewAlias = $"v";
                var categoryView = $"{GetCategoryView(category, fields, viewAlias)}";

                var selectCondition = $"{GetSelect(viewAlias, fields, category.IsTreeView)} FROM {categoryView} ";
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
                        var values = groupByField.Select(v => ((EnumDataType)inputField.DataTypeId).GetSqlValue(v.Value)).ToList();
                        if (values.Count() > 0)
                        {
                            if (suffix > 0)
                            {
                                dataSql.Append(" OR ");
                            }
                            dataSql.Append($" [{viewAlias}].{inputField.CategoryFieldName} IN (");
                            var paramNames = new List<string>();
                            foreach (var value in values)
                            {
                                var paramName = $"@{inputField.CategoryFieldName}_in_{suffix}";
                                paramNames.Add(paramName);
                                sqlParams.Add(new SqlParameter(paramName, value));
                                suffix++;
                            }
                            dataSql.Append(string.Join(",", paramNames));
                            dataSql.Append(")");
                        }
                    }
                    dataSql.Append(")");
                    if (!string.IsNullOrEmpty(groupByFilter.Key.Filters))
                    {
                        Clause filterClause = JsonConvert.DeserializeObject<Clause>(groupByFilter.Key.Filters);
                        if (filterClause != null)
                        {
                            dataSql.Append(" AND ");
                            filterClause.FilterClauseProcess(GetCategoryViewName(category.CategoryCode), viewAlias, ref dataSql, ref sqlParams, ref suffix);
                        }
                    }

                    var data = await _accountancyContext.QueryDataTable(dataSql.ToString(), sqlParams.ToArray());
                    var lst = data.ConvertData();

                    foreach (var item in groupByFilter)
                    {
                        var referObject = lst.FirstOrDefault(o => o[item.CategoryFieldName].ToString() == item.Value);
                        if (referObject != null)
                        {
                            titles.Add(new MapObjectOutputModel
                            {
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

        public async Task<bool> ImportCategoryRowFromMapping(int categoryId, CategoryImportExelMapping mapping, Stream stream)
        {
            var category = _accountancyContext.Category.FirstOrDefault(c => c.CategoryId == categoryId);

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

            var reader = new ExcelReader(stream);

            // Lấy thông tin field
            var categoryFields = _accountancyContext.CategoryField
                .AsNoTracking()
                .Where(f => categoryId == f.CategoryId)
                .ToList();


            var refCategoryCodes = categoryFields.Select(f => f.RefTableCode).ToList();

            var refCategoryFields = (await (from f in _accountancyContext.CategoryField
                                            join c in _accountancyContext.Category on f.CategoryId equals c.CategoryId
                                            where refCategoryCodes.Contains(c.CategoryCode)
                                            select new
                                            {
                                                f.CategoryFieldName,
                                                c.CategoryCode
                                            }
                                    ).ToListAsync())
                                    .GroupBy(f => f.CategoryCode)
                                    .ToDictionary(f => f.Key, f => f.ToList());


            var inputFieldCount = categoryFields
                .Where(f => !f.IsHidden && !f.AutoIncrement)
                .Where(f => f.CategoryFieldName != AccountantConstants.F_IDENTITY).Count();


            var data = reader.ReadSheets(mapping.SheetName, mapping.FromRow, mapping.ToRow, null).FirstOrDefault();

            var rowDatas = new List<List<CategoryImportExcelRowData>>();

            var refCategoryDatasToQuery = new List<CategoryQueryRequest>();
            for (var rowIndx = 0; rowIndx < data.Rows.Length; rowIndx++)
            {
                var row = data.Rows[rowIndx];

                var rowData = new List<CategoryImportExcelRowData>();
                bool isIgnoreRow = false;
                for (int fieldIndx = 0; fieldIndx < mapping.MappingFields.Count && !isIgnoreRow; fieldIndx++)
                {
                    var mappingField = mapping.MappingFields[fieldIndx];

                    string value = null;
                    if (row.ContainsKey(mappingField.Column))
                        value = row[mappingField.Column]?.ToString();

                    if (string.IsNullOrWhiteSpace(value) && mappingField.IsRequire)
                    {
                        isIgnoreRow = true;
                        continue;
                    }

                    var field = categoryFields.FirstOrDefault(f => f.CategoryFieldName == mappingField.FieldName);

                    if (field == null && mappingField.FieldName != AccountantConstants.PARENT_ID_FIELD_NAME && !string.IsNullOrWhiteSpace(mappingField.FieldName)) throw new BadRequestException(GeneralCode.ItemNotFound, $"Trường dữ liệu {mappingField.FieldName} không tìm thấy");

                    if (new[] { EnumDataType.Date, EnumDataType.Month, EnumDataType.QuarterOfYear, EnumDataType.Year }.Contains((EnumDataType)field.DataTypeId))
                    {
                        if (!DateTime.TryParse(value.ToString(), out DateTime date))
                            throw new BadRequestException(GeneralCode.InvalidParams, $"Không thể chuyển giá trị {value}, dòng {rowIndx + mapping.FromRow}, trường {field.Title} sang kiểu ngày tháng");
                        value = date.AddMinutes(_currentContextService.TimeZoneOffset.Value).GetUnix().ToString();
                    }


                    rowData.Add(new CategoryImportExcelRowData()
                    {
                        FieldMapping = mappingField,
                        FieldConfig = field,
                        CellValue = value
                    });


                    //collect ref data by category and it 's field data for query

                    if (!string.IsNullOrWhiteSpace(field?.RefTableCode) && !string.IsNullOrWhiteSpace(mappingField.RefFieldName))
                    {
                        if (!refCategoryFields.ContainsKey(field.RefTableCode))
                        {
                            throw new BadRequestException(GeneralCode.ItemNotFound, $"{field.RefTableCode}");
                        }

                        if (refCategoryFields[field.RefTableCode].FirstOrDefault(f => f.CategoryFieldName == mappingField.RefFieldName) == null)
                        {
                            throw new BadRequestException(GeneralCode.ItemNotFound, $"{mappingField.RefFieldName}");
                        }

                        var refCategoryToQuery = refCategoryDatasToQuery.FirstOrDefault(r => r.CategoryCode == field.RefTableCode);
                        if (refCategoryToQuery == null)
                        {
                            refCategoryToQuery = new CategoryQueryRequest()
                            {
                                CategoryCode = field.RefTableCode,
                                FieldQuery = new Dictionary<string, HashSet<string>>()
                            };

                            refCategoryDatasToQuery.Add(refCategoryToQuery);
                        }

                        if (!refCategoryToQuery.FieldQuery.TryGetValue(mappingField.RefFieldName, out var refValue))
                        {
                            refValue = new HashSet<string>();
                            refCategoryToQuery.FieldQuery.Add(mappingField.RefFieldName, refValue);
                        }

                        if (!refValue.Contains(value))
                            refValue.Add(value);
                    }
                }

                if (!isIgnoreRow)
                    rowDatas.Add(rowData);
            }


            var refCategoriesData = await GetRefCategoryDataByMultiField(refCategoryDatasToQuery);

            using (var trans = await _accountancyContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var insertedData = new Dictionary<int, Dictionary<string, object>>();

                    var parentMappingData = new Dictionary<int, CategoryParentData>();

                    // Insert data
                    foreach (var rowData in rowDatas)
                    {
                        var rowInput = new Dictionary<string, string>();

                        var parentValue = "";
                        var parentRefFieldName = "";

                        foreach (var cellData in rowData)
                        {
                            if (string.IsNullOrWhiteSpace(cellData.FieldMapping.FieldName) || string.IsNullOrWhiteSpace(cellData.CellValue)) continue;

                            var isParentField = cellData.FieldMapping.FieldName == AccountantConstants.PARENT_ID_FIELD_NAME;
                            var isRefFieldMapping = !string.IsNullOrWhiteSpace(cellData.FieldMapping.RefFieldName);

                            if (isParentField)
                            {
                                if (isRefFieldMapping)
                                {
                                    parentValue = cellData.CellValue;
                                    parentRefFieldName = cellData.FieldMapping.RefFieldName;
                                }
                                else
                                {
                                    rowInput.Add(cellData.FieldMapping.FieldName, cellData.CellValue);
                                }
                            }
                            else
                            {
                                var isRefField = !string.IsNullOrWhiteSpace(cellData.FieldConfig.RefTableCode);

                                if (isRefField)
                                {
                                    if (string.IsNullOrWhiteSpace(cellData.FieldMapping.RefFieldName))
                                    {
                                        cellData.FieldMapping.RefFieldName = cellData.FieldConfig.RefTableField;
                                    }

                                    var refCategoryRow = refCategoriesData[cellData.FieldConfig.RefTableCode]
                                        .FirstOrDefault(c => c[cellData.FieldMapping.RefFieldName].ToString() == cellData.CellValue);

                                    if (refCategoryRow != null)
                                    {
                                        var refValue = refCategoryRow[cellData.FieldConfig.RefTableField];

                                        rowInput.Add(cellData.FieldMapping.FieldName, ((EnumDataType)cellData.FieldConfig.DataTypeId).GetSqlValue(refValue).ToString());
                                    }
                                    else
                                    {
                                        throw new BadRequestException(GeneralCode.InvalidParams, $"Giá trị {cellData.CellValue} không tìm thấy trong danh mục");
                                    }
                                }
                                else
                                {
                                    rowInput.Add(cellData.FieldMapping.FieldName, cellData.CellValue);
                                }

                            }
                        }

                        var result = await AddCategoryRow(categoryId, rowInput);

                        if (!string.IsNullOrWhiteSpace(parentValue))
                        {
                            parentMappingData.Add(result, new CategoryParentData()
                            {
                                ParentFieldName = parentRefFieldName,
                                ParentValue = parentValue
                            });
                        }
                    }


                    // Insert relationship
                    if (category.IsTreeView)
                    {

                        var parentDataQuery = new CategoryQueryRequest()
                        {
                            CategoryCode = category.CategoryCode,
                            FieldQuery = new Dictionary<string, HashSet<string>>()
                        };

                        foreach (var parentMapping in parentMappingData)
                        {
                            if (!parentDataQuery.FieldQuery.TryGetValue(parentMapping.Value.ParentFieldName, out var refValues))
                            {
                                refValues = new HashSet<string>();
                                parentDataQuery.FieldQuery.Add(parentMapping.Value.ParentFieldName, refValues);
                            }

                            if (!refValues.Contains(parentMapping.Value.ParentValue))
                            {
                                refValues.Add(parentMapping.Value.ParentValue.ToString());
                            }
                        }

                        var parentData = await GetRefCategoryDataByMultiField(new[] { parentDataQuery });

                        var sqlUpdateParent = new StringBuilder();
                        foreach (var row in parentMappingData)
                        {
                            var parentRow = parentData.First().Value.FirstOrDefault(f => f[row.Value.ParentFieldName]?.ToString() == row.Value.ParentValue);
                            if (parentRow == null)
                            {
                                throw new BadRequestException(GeneralCode.InvalidParams, $"{category.Title} cha {row.Value.ParentValue} không tìm thấy");
                            }
                            if ((int)parentRow[AccountantConstants.F_IDENTITY] != row.Key)
                            {
                                sqlUpdateParent.AppendLine($"UPDATE {category.CategoryCode} SET {AccountantConstants.PARENT_ID_FIELD_NAME} = {parentRow[AccountantConstants.F_IDENTITY]} WHERE {AccountantConstants.F_IDENTITY} = {row.Key} ;");
                            }

                        }
                        if (sqlUpdateParent.Length > 0)
                        {
                            await _accountancyContext.Database.ExecuteSqlRawAsync(sqlUpdateParent.ToString());
                        }
                    }


                    trans.Commit();

                }
                catch (Exception ex)
                {
                    trans.TryRollbackTransaction();
                    _logger.LogError(ex, "Import");
                    throw;
                }
            }

            return true;

        }

        private async Task<Dictionary<string, List<NonCamelCaseDictionary>>> GetRefCategoryDataByMultiField(IList<CategoryQueryRequest> categoryQueryRequests)
        {
            var categoryCodes = categoryQueryRequests.Select(c => c.CategoryCode).ToList();
            var refCategoryFields = (await (
                from f in _accountancyContext.CategoryField
                join c in _accountancyContext.Category on f.CategoryId equals c.CategoryId
                where categoryCodes.Contains(c.CategoryCode)
                select new
                {
                    c.CategoryId,
                    c.CategoryCode,
                    CategoryTitle = c.Title,
                    c.IsTreeView,
                    Field = f
                }).ToListAsync())
                .GroupBy(c => c.CategoryCode)
                .ToDictionary(
                    c => c.Key,
                    c => c.GroupBy(i => new { i.CategoryId, i.CategoryCode, i.CategoryTitle, i.IsTreeView })
                    .Select(i => new
                    {
                        CategoryInfo = new
                        {
                            i.FirstOrDefault().CategoryId,
                            i.FirstOrDefault().CategoryCode,
                            i.FirstOrDefault().CategoryTitle,
                            i.FirstOrDefault().IsTreeView
                        },
                        Fields = i.Select(f => f.Field).ToList()
                    })
                    .First()
                );

            var categoriesData = new Dictionary<string, List<NonCamelCaseDictionary>>();
            foreach (var categoryQuery in categoryQueryRequests)
            {
                var categoryCode = categoryQuery.CategoryCode;
                var view = $"v{categoryCode}";

                refCategoryFields.TryGetValue(categoryCode, out var categoryInfo);


                var dataSql = new StringBuilder();
                var sqlParams = new List<SqlParameter>();
                dataSql.Append(GetSelect(view, categoryInfo.Fields, categoryInfo.CategoryInfo.IsTreeView));

                dataSql.Append($" FROM {view} WHERE ");

                dataSql.Append($"0 = 1");

                var idx = 0;

                var fieldsFilter = categoryQuery.FieldQuery;
                foreach (var fieldFilter in fieldsFilter)
                {
                    dataSql.Append(" OR ");

                    var field = categoryInfo.Fields.FirstOrDefault(f => f.CategoryFieldName == fieldFilter.Key);

                    var queryValues = fieldFilter.Value.Select(v => ((EnumDataType)field.DataTypeId).GetSqlValue(v)).ToList();

                    if (queryValues.Count > 0)
                    {
                        if (queryValues.Count == 1)
                        {
                            idx++;

                            var paramName = "@" + fieldFilter.Key + "" + idx;

                            dataSql.Append($"[{fieldFilter.Key}] = {paramName}");

                            sqlParams.Add(new SqlParameter(paramName, fieldFilter.Value.FirstOrDefault()));
                        }
                        else
                        {
                            var whereIn = new List<string>();
                            foreach (var v in queryValues)
                            {
                                idx++;

                                var paramName = "@" + fieldFilter.Key + "" + idx;
                                whereIn.Add(paramName);
                                sqlParams.Add(new SqlParameter(paramName, v));
                            }

                            dataSql.Append($"[{fieldFilter.Key}] IN ({string.Join(",", whereIn)})");
                        }
                    }
                }

                var data = await _accountancyContext.QueryDataTable(dataSql.ToString(), sqlParams.ToArray());

                categoriesData.Add(categoryQuery.CategoryCode, data.ConvertData());
            }

            return categoriesData;
        }

        private class CategoryParentData
        {
            public string ParentFieldName { get; set; }
            public string ParentValue { get; set; }
        }

        private class CategoryQueryRequest
        {
            public string CategoryCode { get; set; }
            public Dictionary<string, HashSet<string>> FieldQuery { get; set; }
        }
    }
}
