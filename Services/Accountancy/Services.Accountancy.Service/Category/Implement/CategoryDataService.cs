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

        public async Task<ServiceResult<int>> AddCategoryRow(int categoryId, Dictionary<string, string> data)
        {

            var category = _accountancyContext.Category.FirstOrDefault(c => c.CategoryId == categoryId);
            if (category == null)
            {
                throw new BadRequestException(CategoryErrorCode.CategoryNotFound);
            }
            var tableName = $"v{category.CategoryCode}";
            var fields = _accountancyContext.CategoryField.Where(f => f.CategoryId == categoryId).ToList();

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

            // Check parent row
            await CheckParentRowAsync(data, category);

            // Lấy thông tin field
            var categoryFields = _accountancyContext.CategoryField
                .Where(f => categoryId == f.CategoryId)
                .AsEnumerable();
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
            data.TryGetValue("ParentId", out string value);
            int parentId = 0;
            if (!string.IsNullOrEmpty(value))
            {
                parentId = int.Parse(value);
            }
            dataRow["ParentId"] = parentId;
            dataRow["CreatedByUserId"] = _currentContextService.UserId;
            dataRow["CreatedDatetimeUtc"] = DateTime.UtcNow;
            dataRow["UpdatedByUserId"] = _currentContextService.UserId;
            dataRow["UpdatedDatetimeUtc"] = DateTime.UtcNow;
            dataRow["IsDeleted"] = false;
            dataRow["DeletedDatetimeUtc"] = DBNull.Value;

            foreach (var field in categoryFields)
            {
                if (field.CategoryFieldName == "F_Id") continue;
                data.TryGetValue(field.CategoryFieldName, out value);
                dataRow[field.CategoryFieldName] = ((EnumDataType)field.DataTypeId).GetSqlValue(value);
            }
            dataTable.Rows.Add(dataRow);

            var id = await _accountancyContext.InsertDataTable(dataTable);
            return id;
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

        private async Task CheckRefer(Dictionary<string, string> data, IEnumerable<CategoryField> selectFields)
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
                    var existSql = $"SELECT F_Id FROM v{field.RefTableCode} WHERE {field.RefTableField} = {valueItem}";
                    var result = await _accountancyContext.QueryDataTable(existSql, Array.Empty<SqlParameter>());
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
                if (valueItem != null && !string.IsNullOrEmpty(valueItem))
                {
                    var existSql = $"SELECT F_Id FROM v{categoryCode} WHERE {field.CategoryFieldName} = {valueItem}";
                    if (fId.HasValue)
                    {
                        existSql += $" AND F_Id != {fId}";
                    }
                    var result = await _accountancyContext.QueryDataTable(existSql, Array.Empty<SqlParameter>());
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
                if ((field.FormTypeId == (int)EnumFormType.SearchTable || field.FormTypeId == (int)EnumFormType.Select) || field.AutoIncrement || valueItem == null || string.IsNullOrEmpty(valueItem))
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



        public async Task<ServiceResult<NonCamelCaseDictionary>> GetCategoryRow(int categoryId, int fId)
        {
            var category = _accountancyContext.Category.FirstOrDefault(c => c.CategoryId == categoryId);
            if (category == null)
            {
                throw new BadRequestException(CategoryErrorCode.CategoryNotFound);
            }
            var tableName = $"v{category.CategoryCode}";
            var fields = (from f in _accountancyContext.CategoryField
                          join c in _accountancyContext.Category on f.CategoryId equals c.CategoryId
                          where c.CategoryId == categoryId && f.FormTypeId != (int)EnumFormType.ViewOnly
                          select new SelectField
                          {
                              CategoryFieldName = f.CategoryFieldName,
                              RefTableCode = f.RefTableCode,
                              RefTableField = f.RefTableField,
                              RefTableTitle = f.RefTableTitle,
                              DataTypeId = f.DataTypeId
                          }).ToList();

            var dataSql = new StringBuilder();
            dataSql.Append(GetSelect(tableName, fields, category.IsTreeView));
            dataSql.Append($" FROM {tableName} WHERE [{tableName}].F_Id = {fId}");

            var data = await _accountancyContext.QueryDataTable(dataSql.ToString(), Array.Empty<SqlParameter>());
            var lst = ConvertData(data);
            NonCamelCaseDictionary row;
            if (lst.Count > 0)
            {
                row = lst[0];
            }
            else
            {
                throw new BadRequestException(CategoryErrorCode.CategoryRowNotFound);
            }
            return row;
        }

        private string GetSelect(string tableName, List<SelectField> fields, bool isTreeView)
        {
            StringBuilder sql = new StringBuilder();
            sql.Append($"SELECT ");
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
                        sql.Append($"[{tableName}].{field.RefTableCode}_{title},");
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

        public async Task<PageData<NonCamelCaseDictionary>> GetCategoryRows(int categoryId, string keyword, string filters, int page, int size)
        {
            var category = _accountancyContext.Category.FirstOrDefault(c => c.CategoryId == categoryId);
            if (category == null)
            {
                throw new BadRequestException(CategoryErrorCode.CategoryNotFound);
            }
            var tableName = $"v{category.CategoryCode}";
            var fields = (from f in _accountancyContext.CategoryField
                          join c in _accountancyContext.Category on f.CategoryId equals c.CategoryId
                          where c.CategoryId == categoryId && f.FormTypeId != (int)EnumFormType.ViewOnly && f.IsShowList == true
                          select new SelectField
                          {
                              CategoryFieldName = f.CategoryFieldName,
                              RefTableCode = f.RefTableCode,
                              RefTableField = f.RefTableField,
                              RefTableTitle = f.RefTableTitle,
                              DataTypeId = f.DataTypeId
                          }).ToList();

            var dataSql = new StringBuilder();
            dataSql.Append(GetSelect(tableName, fields, category.IsTreeView));
            dataSql.Append($" FROM {tableName}");
            var serchCondition = new StringBuilder();
            if (!string.IsNullOrEmpty(keyword))
            {
                foreach (var field in fields)
                {
                    if (serchCondition.Length > 0)
                    {
                        serchCondition.Append(" OR ");
                    }

                    if (string.IsNullOrEmpty(field.RefTableCode))
                    {
                        serchCondition.Append($"[{tableName}].{field.CategoryFieldName} LIKE %{keyword}%");
                    }
                    else
                    {
                        foreach (var item in field.RefTableTitle.Split(","))
                        {
                            var title = item.Trim();
                            serchCondition.Append($"[{tableName}].{field.RefTableCode}_{title} LIKE %{keyword}%");
                        }
                    }
                }
            }
            var totalSql = new StringBuilder($"SELECT COUNT(F_Id) as Total FROM {tableName}");
            if (serchCondition.Length > 0)
            {
                dataSql.Append($" WHERE {serchCondition.ToString()}");
                totalSql.Append($" WHERE {serchCondition.ToString()}");
            }

            var countTable = await _accountancyContext.QueryDataTable(totalSql.ToString(), Array.Empty<SqlParameter>());
            var total = 0;
            if (countTable != null && countTable.Rows.Count > 0)
            {
                total = (countTable.Rows[0]["Total"] as int?).GetValueOrDefault();
            }
            dataSql.Append($" ORDER BY [{tableName}].F_Id");
            if (size > 0)
            {
                dataSql.Append($" OFFSET {(page - 1) * size} ROWS FETCH NEXT {size} ROWS ONLY;");
            }

            var data = await _accountancyContext.QueryDataTable(dataSql.ToString(), Array.Empty<SqlParameter>());
            var lst = ConvertData(data);

            return (lst, total);
        }



        private class SelectField
        {
            public string CategoryFieldName { get; set; }
            public string RefTableCode { get; set; }
            public string RefTableField { get; set; }
            public string RefTableTitle { get; set; }
            public int DataTypeId { get; set; }
        }
    }
}
