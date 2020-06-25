using AutoMapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
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
using VErp.Infrastructure.EF.AccountancyDB;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Accountancy.Model.Data;
using CategoryEntity = VErp.Infrastructure.EF.AccountancyDB.Category;
namespace VErp.Services.Accountancy.Service.Category
{
    public class CategoryDataService : ICategoryDataService
    {
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly AppSetting _appSetting;
        private readonly IMapper _mapper;
        private readonly AccountancyDBContext _accountancyContext;
        private readonly ICurrentContextService _currentContextService;
        public CategoryDataService(AccountancyDBContext accountancyContext
            , IOptions<AppSetting> appSetting
            , ILogger<CategoryConfigService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            , ICurrentContextService currentContextService
            )
        {
            _logger = logger;
            _activityLogService = activityLogService;
            _accountancyContext = accountancyContext;
            _appSetting = appSetting.Value;
            _mapper = mapper;
            _currentContextService = currentContextService;
        }

        public async Task<ServiceResult<int>> AddCategoryRow(string categoryCode, Dictionary<string, string> data)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockCategoryKey(0));
            var category = _accountancyContext.Category.FirstOrDefault(c => c.CategoryCode == categoryCode);
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

            // Check parent row
            await CheckParentRowAsync(data, category);

            // Lấy thông tin field
            var categoryFields = _accountancyContext.CategoryField
                .Where(f => category.CategoryId == f.CategoryId && f.FormTypeId != (int)EnumFormType.ViewOnly)
                .AsEnumerable();
            var requiredFields = categoryFields.Where(f => !f.AutoIncrement && f.IsRequired);
            var uniqueFields = categoryFields.Where(f => !f.AutoIncrement && f.IsUnique);
            var selectFields = categoryFields.Where(f => !f.AutoIncrement && (f.FormTypeId == (int)EnumFormType.SearchTable || f.FormTypeId == (int)EnumFormType.Select));

            // Check field required
            CheckRequired(data, requiredFields);
            // Check refer
            await CheckRefer(data, selectFields, tableName);
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
            }
            dataTable.Rows.Add(dataRow);

            var id = await _accountancyContext.InsertDataTable(dataTable);
            await _activityLogService.CreateLog(EnumObjectType.Category, id, $"Thêm mới dữ liệu danh mục {id}", data.JsonSerialize());
            return (int)id;
        }

        public async Task<int> UpdateCategoryRow(string categoryCode, int fId, Dictionary<string, string> data)
        {
            var category = _accountancyContext.Category.FirstOrDefault(c => c.CategoryCode == categoryCode);
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
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockCategoryKey(category.CategoryId));
            var tableName = $"v{category.CategoryCode}";
            var categoryFields = _accountancyContext.CategoryField.Where(f => f.CategoryId == category.CategoryId && f.FormTypeId != (int)EnumFormType.ViewOnly && f.CategoryFieldName != "F_Id").ToList();
            var dataSql = new StringBuilder();
            dataSql.Append(GetSelect(tableName, categoryFields, category.IsTreeView));
            dataSql.Append($" FROM {tableName} WHERE [{tableName}].F_Id = {fId}");

            var currentData = await _accountancyContext.QueryDataTable(dataSql.ToString(), Array.Empty<SqlParameter>());
            var lst = ConvertData(currentData);
            if (lst.Count == 0)
            {
                throw new BadRequestException(CategoryErrorCode.CategoryRowNotFound);
            }
            NonCamelCaseDictionary categoryRow = lst[0];
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
            await CheckRefer(data, selectFields, tableName);

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
            }
            dataTable.Rows.Add(dataRow);

            int numberChange = await _accountancyContext.UpdateCategoryData(dataTable, fId);
            await _activityLogService.CreateLog(EnumObjectType.Category, fId, $"Cập nhật dữ liệu danh mục {fId}", data.JsonSerialize());
            return numberChange;
        }

        public async Task<int> DeleteCategoryRow(string categoryCode, int fId)
        {
            var category = _accountancyContext.Category.FirstOrDefault(c => c.CategoryCode == categoryCode);
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
            var categoryFields = _accountancyContext.CategoryField.Where(f => f.CategoryId == category.CategoryId && f.FormTypeId != (int)EnumFormType.ViewOnly && f.CategoryFieldName != "F_Id").ToList();
            var dataSql = new StringBuilder();
            dataSql.Append(GetSelect(tableName, categoryFields, category.IsTreeView));
            dataSql.Append($" FROM {tableName} WHERE [{tableName}].F_Id = {fId}");

            var currentData = await _accountancyContext.QueryDataTable(dataSql.ToString(), Array.Empty<SqlParameter>());
            var lst = ConvertData(currentData);
            if (lst.Count == 0)
            {
                throw new BadRequestException(CategoryErrorCode.CategoryRowNotFound);
            }
            NonCamelCaseDictionary categoryRow = lst[0];
            var fieldNames = categoryFields.Select(f => f.CategoryFieldName).ToList();
            var referToFields = _accountancyContext.CategoryField.Where(f => f.RefTableCode == category.CategoryCode && fieldNames.Contains(f.RefTableField)).ToList();
            var referToCategoryIds = referToFields.Select(f => f.CategoryId).Distinct().ToList();
            var referToCategories = _accountancyContext.Category.Where(c => referToCategoryIds.Contains(c.CategoryId)).ToList();

            // Check reference
            foreach (var field in categoryFields)
            {
                categoryRow.TryGetValue(field.CategoryFieldName, out object value);
                if (value == null) continue;
                foreach (var referToField in referToFields.Where(c => c.RefTableField == field.CategoryFieldName))
                {
                    var referToCategory = referToCategories.First(c => c.CategoryId == referToField.CategoryId);
                    var referToTable = $"v{referToCategory.CategoryCode}";

                    var existSql = $"SELECT [{referToTable}].F_Id as Total FROM {referToTable} WHERE [{referToTable}].IsDeleted = 0 AND [{referToTable}].{referToField.CategoryFieldName} = {value.ToString()};";
                    var result = await _accountancyContext.QueryDataTable(existSql, Array.Empty<SqlParameter>());
                    bool isExisted = result != null && result.Rows.Count > 0;
                    if (isExisted)
                    {
                        throw new BadRequestException(CategoryErrorCode.RelationshipAlreadyExisted);
                    }
                }
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
                if (!data.Any(v => v.Key == field.CategoryFieldName && !string.IsNullOrEmpty(v.Value)))
                {
                    throw new BadRequestException(CategoryErrorCode.RequiredFieldIsEmpty, new string[] { field.Title });
                }
            }
        }

        private async Task CheckRefer(Dictionary<string, string> data, IEnumerable<CategoryField> selectFields, string tableName)
        {
            // Check refer
            foreach (var field in selectFields)
            {
                data.TryGetValue(field.CategoryFieldName, out string valueItem);

                if (valueItem != null && !string.IsNullOrEmpty(valueItem))
                {
                    Clause filters = null;
                    if (!string.IsNullOrEmpty(field.Filters))
                    {
                        filters = JsonConvert.DeserializeObject<Clause>(field.Filters);
                        // Filter
                    }
                    var whereCondition = new StringBuilder();
                    var sqlParams = new List<SqlParameter>();
                    int suffix = 0;
                    if (!string.IsNullOrEmpty(field.Filters))
                    {
                        Clause filterClause = JsonConvert.DeserializeObject<Clause>(field.Filters);
                        if (filterClause != null)
                        {
                            FilterClauseProcess(filterClause, tableName, ref whereCondition, ref sqlParams, ref suffix);
                        }
                    }
                    var paramName = $"@{field.RefTableField}_{suffix}";
                    var existSql = $"SELECT F_Id FROM v{field.RefTableCode} WHERE {field.RefTableField} = {paramName}";
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

        public async Task<ServiceResult<NonCamelCaseDictionary>> GetCategoryRow(string categoryCode, int fId)
        {
            var category = _accountancyContext.Category.FirstOrDefault(c => c.CategoryCode == categoryCode);
            if (category == null)
            {
                throw new BadRequestException(CategoryErrorCode.CategoryNotFound);
            }
            return await GetCategoryRow(category, fId);
        }

        private async Task<ServiceResult<NonCamelCaseDictionary>> GetCategoryRow(CategoryEntity category, int fId)
        {
            var tableName = $"v{category.CategoryCode}";
            var fields = (from f in _accountancyContext.CategoryField
                          join c in _accountancyContext.Category on f.CategoryId equals c.CategoryId
                          where c.CategoryId == category.CategoryId && f.CategoryFieldName != "F_Id" && f.FormTypeId != (int)EnumFormType.ViewOnly
                          select f).ToList();

            var dataSql = new StringBuilder();
            dataSql.Append(GetSelect(tableName, fields, category.IsTreeView));
            dataSql.Append($" FROM {tableName} WHERE [{tableName}].F_Id = {fId}");

            var data = await _accountancyContext.QueryDataTable(dataSql.ToString(), Array.Empty<SqlParameter>());
            var lst = ConvertData(data);
            if (lst.Count == 0)
            {
                throw new BadRequestException(CategoryErrorCode.CategoryRowNotFound);
            }
            return lst[0];
        }

        private string GetSelect(string tableName, List<CategoryField> fields, bool isTreeView)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append($"SELECT [{tableName}].F_Id, ");
            foreach (var field in fields)
            {
                if (string.IsNullOrEmpty(field.RefTableCode))
                {
                    sql.Append($"[{tableName}].{field.CategoryFieldName},");
                }
                else
                {
                    sql.Append($"[{tableName}].{field.CategoryFieldName},");
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
            if (fields.Count > 0)
            {
                sql.Remove(sql.Length - 1, 1);
            }
            return sql.ToString();
        }

        private List<NonCamelCaseDictionary> ConvertData(DataTable data)
        {
            var lst = new List<NonCamelCaseDictionary>();
            for (var i = 0; i < data.Rows.Count; i++)
            {
                var row = data.Rows[i];
                var dic = new NonCamelCaseDictionary();
                foreach (DataColumn c in data.Columns)
                {
                    var v = row[c];
                    if (v != null && v.GetType() == typeof(DateTime) || v.GetType() == typeof(DateTime?))
                    {
                        var vInDateTime = (v as DateTime?).GetUnix();
                        dic.Add(c.ColumnName, vInDateTime);
                    }
                    else
                    {
                        dic.Add(c.ColumnName, row[c]);
                    }
                }
                lst.Add(dic);
            }
            return lst;
        }

        public async Task<PageData<NonCamelCaseDictionary>> GetCategoryRows(string categoryCode, string keyword, string filters, int page, int size)
        {
            var category = _accountancyContext.Category.FirstOrDefault(c => c.CategoryCode == categoryCode);
            if (category == null)
            {
                throw new BadRequestException(CategoryErrorCode.CategoryNotFound);
            }
            return await GetCategoryRows(category, keyword, filters, page, size);
        }

        private async Task<PageData<NonCamelCaseDictionary>> GetCategoryRows(CategoryEntity category, string keyword, string filters, int page, int size)
        {
            var tableName = $"v{category.CategoryCode}";
            var fields = (from f in _accountancyContext.CategoryField
                          join c in _accountancyContext.Category on f.CategoryId equals c.CategoryId
                          where c.CategoryId == category.CategoryId && f.CategoryFieldName != "F_Id" && f.FormTypeId != (int)EnumFormType.ViewOnly && f.IsShowList == true
                          select f).ToList();

            var dataSql = new StringBuilder();
            var sqlParams = new List<SqlParameter>();
            var allDataSql = new StringBuilder();
            dataSql.Append(GetSelect(tableName, fields, category.IsTreeView));
            dataSql.Append($" FROM {tableName}");
            allDataSql.Append(dataSql.ToString());
            var whereCondition = new StringBuilder();
            if (!string.IsNullOrEmpty(keyword))
            {
                whereCondition.Append("(");
                var idx = 0;
                foreach (var field in fields)
                {
                    if (whereCondition.Length > 0)
                    {
                        whereCondition.Append(" OR ");
                    }

                    if (string.IsNullOrEmpty(field.RefTableCode))
                    {
                        var paramName = $"@{field.CategoryFieldName}_{idx}";
                        sqlParams.Add(new SqlParameter(paramName, $"%{keyword}%"));
                        whereCondition.Append($"[{tableName}].{field.CategoryFieldName} LIKE {paramName}");

                    }
                    else
                    {
                        foreach (var item in field.RefTableTitle.Split(","))
                        {
                            var title = item.Trim();
                            var paramName = $"@{field.CategoryFieldName}_{title}_{idx}";
                            sqlParams.Add(new SqlParameter(paramName, $"%{keyword}%"));
                            whereCondition.Append($"[{tableName}].{field.CategoryFieldName}_{title} LIKE {paramName}");
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
                    if (whereCondition.Length > 0)
                    {
                        whereCondition.Append(" AND ");
                    }

                    int suffix = 0;
                    FilterClauseProcess(filterClause, tableName, ref whereCondition, ref sqlParams, ref suffix);
                }
            }

            var totalSql = new StringBuilder($"SELECT COUNT(F_Id) as Total FROM {tableName}");

            if (whereCondition.Length > 0)
            {
                dataSql.Append($" WHERE {whereCondition.ToString()}");
                totalSql.Append($" WHERE {whereCondition.ToString()}");
            }

            var countTable = await _accountancyContext.QueryDataTable(totalSql.ToString(), sqlParams.Select(p => p.CloneSqlParam()).ToArray());
            var total = 0;
            if (countTable != null && countTable.Rows.Count > 0)
            {
                total = (countTable.Rows[0]["Total"] as int?).GetValueOrDefault();
            }
            if (!category.IsTreeView)
            {
                dataSql.Append($" ORDER BY [{tableName}].F_Id");
                if (size > 0)
                {
                    dataSql.Append($" OFFSET {(page - 1) * size} ROWS FETCH NEXT {size} ROWS ONLY;");
                }
            }
            var data = await _accountancyContext.QueryDataTable(dataSql.ToString(), sqlParams.Select(p => p.CloneSqlParam()).ToArray());
            var lstData = ConvertData(data);

            if (category.IsTreeView)
            {
                var allData = await _accountancyContext.QueryDataTable(allDataSql.ToString(), Array.Empty<SqlParameter>());
                var lstAll = ConvertData(allData);

                AddParents(ref lstData, lstAll);

                lstData = SortCategoryRows(lstData);
                if (size > 0)
                {
                    lstData = lstData.Skip((page - 1) * size).Take(size).ToList();
                }
            }

            return (lstData, total);
        }

        public void FilterClauseProcess(Clause clause, string tableName, ref StringBuilder condition, ref List<SqlParameter> sqlParams, ref int suffix, bool not = false)
        {
            if (clause != null)
            {
                condition.Append("( ");
                if (clause is SingleClause)
                {
                    var singleClause = clause as SingleClause;
                    BuildExpression(singleClause, tableName, ref condition, ref sqlParams, ref suffix, not);
                }
                else if (clause is ArrayClause)
                {
                    var arrClause = clause as ArrayClause;
                    bool isNot = not ^ arrClause.Not;
                    bool isOr = (!isNot && arrClause.Condition == EnumLogicOperator.Or) || (isNot && arrClause.Condition == EnumLogicOperator.And);
                    for (int indx = 0; indx < arrClause.Rules.Count; indx++)
                    {
                        if (indx != 0)
                        {
                            condition.Append(isOr ? " OR " : " AND ");
                        }
                        FilterClauseProcess(arrClause.Rules.ElementAt(indx), tableName, ref condition, ref sqlParams, ref suffix, isNot);
                    }
                }
                condition.Append(" )");
            }
        }

        private void BuildExpression(SingleClause clause, string tableName, ref StringBuilder condition, ref List<SqlParameter> sqlParams, ref int suffix, bool not)
        {
            if (clause != null)
            {
                var paramName = $"@{clause.FieldName}_filter_{suffix}";
                string ope;
                switch (clause.Operator)
                {
                    case EnumOperator.Equal:
                        ope = not ? "!=" : "==";
                        condition.Append($"[{tableName}].{clause.FieldName} {ope} {paramName}");
                        sqlParams.Add(new SqlParameter(paramName, (string)clause.Value));
                        break;
                    case EnumOperator.NotEqual:
                        ope = not ? "==" : "!=";
                        condition.Append($"[{tableName}].{clause.FieldName} {ope} {paramName}");
                        sqlParams.Add(new SqlParameter(paramName, (string)clause.Value));
                        break;
                    case EnumOperator.Contains:
                        ope = not ? "NOT LIKE" : "LIKE";
                        condition.Append($"[{tableName}].{clause.FieldName} {ope} {paramName}");
                        sqlParams.Add(new SqlParameter(paramName, $"%{(string)clause.Value}%"));
                        break;
                    case EnumOperator.InList:
                        ope = not ? "NOT IN" : "IN";
                        condition.Append($"[{tableName}].{clause.FieldName} {ope} {paramName}");
                        sqlParams.Add(new SqlParameter(paramName, $"({(string)clause.Value})"));
                        break;
                    case EnumOperator.IsLeafNode:
                        ope = not ? "EXISTS" : "NOT EXISTS";
                        var alias = $"{tableName}_{suffix}";
                        condition.Append($"{ope}(SELECT {alias}.F_Id FROM {tableName} {alias} WHERE {alias}.ParentId = [{tableName}].F_Id)");
                        break;
                    case EnumOperator.StartsWith:
                        ope = not ? "NOT LIKE" : "LIKE";
                        condition.Append($"[{tableName}].{clause.FieldName} {ope} {paramName}");
                        sqlParams.Add(new SqlParameter(paramName, $"{(string)clause.Value}%"));
                        break;
                    case EnumOperator.EndsWith:
                        ope = not ? "NOT LIKE" : "LIKE";
                        condition.Append($"[{tableName}].{clause.FieldName} {ope} {paramName}");
                        sqlParams.Add(new SqlParameter(paramName, $"%{(string)clause.Value}"));
                        break;
                    default:
                        break;
                }
                suffix++;
            }
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

        public async Task<List<MapObjectOutputModel>> MapToObject(MapObjectInputModel[] categoryValues)
        {
            List<MapObjectOutputModel> titles = new List<MapObjectOutputModel>();
            var groups = categoryValues.GroupBy(v => new { v.CategoryCode, v.CategoryFieldName });

            foreach (var group in groups)
            {
                var category = _accountancyContext.Category.First(c => c.CategoryCode == group.Key.CategoryCode);
                var values = group.Select(g => g.Value).ToList();

                var tableName = $"v{category.CategoryCode}";
                var fields = (from f in _accountancyContext.CategoryField
                              join c in _accountancyContext.Category on f.CategoryId equals c.CategoryId
                              where c.CategoryCode == category.CategoryCode && f.CategoryFieldName != "F_Id" && f.FormTypeId != (int)EnumFormType.ViewOnly
                              select f).ToList();

                var dataSql = new StringBuilder();
                var sqlParams = new List<SqlParameter>();
                dataSql.Append(GetSelect(tableName, fields, category.IsTreeView));
                var paramName = $"@{group.Key.CategoryFieldName}";
                dataSql.Append($" FROM {tableName} WHERE [{tableName}].{group.Key.CategoryFieldName} IN ({paramName})");
                sqlParams.Add(new SqlParameter(paramName, string.Join(",", values.ToArray())));

                var data = await _accountancyContext.QueryDataTable(dataSql.ToString(), sqlParams.ToArray());
                var lst = ConvertData(data);


                titles.AddRange(lst.Select(r => new MapObjectOutputModel
                {
                    CategoryCode = category.CategoryCode,
                    CategoryFieldName = group.Key.CategoryFieldName,
                    Value = r[group.Key.CategoryFieldName].ToString(),
                    ReferObject = r
                }));
            }

            return titles;
        }
    }
}
