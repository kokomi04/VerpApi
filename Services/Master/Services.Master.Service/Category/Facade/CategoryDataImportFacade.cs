using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using VErp.Commons.Constants;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Model.Category;
using CategoryEntity = VErp.Infrastructure.EF.MasterDB.Category;

namespace VErp.Services.Master.Service.Category
{
    public class CategoryDataImportFacade
    {
        private readonly int _categoryId;
        private readonly MasterDBContext _accountancyContext;
        private readonly ICategoryDataService _categoryDataService;
        private readonly IActivityLogService _activityLogService;
        private readonly ICurrentContextService _currentContextService;

        private CategoryEntity _category;
        private ExcelSheetDataModel _importData;
        private List<CategoryField> _categoryFields;

        private List<NonCamelCaseDictionary> _refCategoryDataForParent;
        private Dictionary<string, List<NonCamelCaseDictionary>> _refCategoryDataForProperty;
        private Dictionary<string, IEnumerable<RefCategoryProperty>> _refCategoryFields;
        private CategoryField[] _uniqueFields;
        private List<Dictionary<string, string>> _CategoryDataRows;

        public CategoryDataImportFacade(int categoryId, MasterDBContext accountancyContext, ICategoryDataService categoryDataService, IActivityLogService activityLogService, ICurrentContextService currentContextService)
        {
            _categoryId = categoryId;
            _accountancyContext = accountancyContext;
            _categoryDataService = categoryDataService;
            _activityLogService = activityLogService;
            _currentContextService = currentContextService;
        }

        public async Task<bool> ImportData(CategoryImportExcelMapping mapping, Stream stream)
        {
            _category = _accountancyContext.Category.FirstOrDefault(c => c.CategoryId == _categoryId);

            await ValidateCategory();
            await GetCategoryFieldInfo(mapping);

            await ReadExcelData(mapping, stream);

            await RefCategoryForParent(mapping);
            await RefCategoryForProperty(mapping);
            await MappingCategoryDate(mapping);

            var existsCategoryData = (await _categoryDataService.GetCategoryRows(_categoryId, null, null, null, null, 0, 0, "", true)).List;

            var lsUpdateRow = new List<Dictionary<string, string>>();
            var lsAddRow = new List<Dictionary<string, string>>();
            foreach (var row in _CategoryDataRows)
            {
                var oldRow = existsCategoryData.FirstOrDefault(x => EqualityBetweenTwoCategory(x, row, _uniqueFields));

                if (oldRow != null && mapping.ImportDuplicateOptionId == EnumImportDuplicateOption.Denied)
                    throw new BadRequestException(GeneralCode.InvalidParams, $"Đã tồn tại \"{_uniqueFields[0].Title.ToLower()}: {row[_uniqueFields[0].CategoryFieldName]}\", giá trị mang tính định danh trong danh mục {_category.Title}.");

                if (oldRow == null)
                {
                    if (lsAddRow.Any(x => EqualityBetweenTwoCategory(x, row, _uniqueFields)))
                        throw new BadRequestException(GeneralCode.InvalidParams, $"Tồn tại nhiều \"{_uniqueFields[0].Title.ToLower()}: {row[_uniqueFields[0].CategoryFieldName]}\", giá trị mang tính định danh trong file excel.");
                    lsAddRow.Add(row);
                }
                else if (mapping.ImportDuplicateOptionId == EnumImportDuplicateOption.Update)
                {
                    if (lsUpdateRow.Any(x => EqualityBetweenTwoCategory(x, row, _uniqueFields)))
                        throw new BadRequestException(GeneralCode.InvalidParams, $"Tồn tại nhiều \"{_uniqueFields[0].Title.ToLower()}: {row[_uniqueFields[0].CategoryFieldName]}\", giá trị mang tính định danh trong file excel.");
                    
                    if(!row.ContainsKey(AccountantConstants.PARENT_ID_FIELD_NAME))
                        row.Add(AccountantConstants.PARENT_ID_FIELD_NAME, oldRow[AccountantConstants.PARENT_ID_FIELD_NAME].ToString());

                    row.Add(AccountantConstants.F_IDENTITY, oldRow[AccountantConstants.F_IDENTITY].ToString());
                    lsUpdateRow.Add(row);
                }
            }

            using (var trans = await _accountancyContext.Database.BeginTransactionAsync())
            {
                using (var logBath = _activityLogService.BeginBatchLog())
                {
                    foreach (var uRow in lsUpdateRow)
                    {
                        var rowId = Int32.Parse(uRow[AccountantConstants.F_IDENTITY]);
                        
                        await _categoryDataService.UpdateCategoryRow(_categoryId, rowId, uRow);
                    }

                    foreach (var cRow in lsAddRow)
                    {
                        await _categoryDataService.AddCategoryRow(_categoryId, cRow);
                    }

                    await trans.CommitAsync();
                    await logBath.CommitAsync();
                }
            }
            return true;
        }

        private bool EqualityBetweenTwoCategory(Dictionary<string, string> f1, Dictionary<string, string> f2, CategoryField[] u)
        {
            bool isEqual = false;
            for (int i = 0; i < u.Length; i++)
            {
                var key = u[i].CategoryFieldName;

                var f1Value = f1[key].ToString().ToLower();
                var f2Value = f2[key].ToString().ToLower();

                isEqual = string.Compare(f1Value, f2Value, true) == 0;
            }

            return isEqual;
        }

        private bool EqualityBetweenTwoCategory(NonCamelCaseDictionary f1, Dictionary<string, string> f2, CategoryField[] u)
        {
            bool isEqual = false;
            for (int i = 0; i < u.Length; i++)
            {
                var key = u[i].CategoryFieldName;

                var f1Value = f1[key].ToString().ToLower();
                var f2Value = f2[key].ToString().ToLower();

                isEqual = string.Compare(f1Value, f2Value, true) == 0;
            }

            return isEqual;
        }

        private async Task MappingCategoryDate(CategoryImportExcelMapping mapping)
        {
            _CategoryDataRows = new List<Dictionary<string, string>>();

            var mapField = (from mf in mapping.MappingFields
                            join cf in _categoryFields on mf.FieldName equals cf.CategoryFieldName into gcf
                            from cf in gcf.DefaultIfEmpty()
                            select new
                            {
                                mf.Column,
                                mf.FieldName,
                                mf.RefFieldName,
                                mf.IsRequire,
                                cf?.RefTableCode,
                                cf?.Title,
                                cf?.DataTypeId
                            });

            foreach (var (row, i) in _importData.Rows.Select((value, i) => (value, i)))
            {
                var categoryDataRow = new Dictionary<string, string>();
                foreach (var mf in mapField)
                {
                    row.TryGetValue(mf.Column, out var value);

                    if (string.IsNullOrWhiteSpace(value) && mf.IsRequire)
                        throw new BadRequestException(GeneralCode.InvalidParams, $"Dòng {i + mapping.FromRow} cột {mf.Column}, giá trị của trường \"{mf.Title}\" không được để trống");

                    if (string.IsNullOrEmpty(value)) continue;

                    if (!string.IsNullOrWhiteSpace(mf.RefFieldName))
                    {
                        if (string.IsNullOrWhiteSpace(value))
                        {
                            value = string.Empty;
                        }
                        else if (mf.FieldName == AccountantConstants.PARENT_ID_FIELD_NAME)
                        {
                            var fieldInfo = _categoryFields.FirstOrDefault(x => x.CategoryFieldName == mf.RefFieldName);
                            value = TransformValueByDataType(mapping, i, value, fieldInfo, mf.Column);

                            var parents = _refCategoryDataForParent.Where(x => x[mf.RefFieldName].ToString().ToLower() == value.ToLower());

                            var hasGreaterThanParent = parents.Count() > 1;
                            if (hasGreaterThanParent)
                                throw new BadRequestException(GeneralCode.InvalidParams, $"Tìm thấy nhiều {_category.Title.ToLower()} cha có \"{fieldInfo.Title}: {value}\" trong danh mục \"{_category.Title}\"");

                            var notFoundParent = parents.Count() == 0;
                            if (notFoundParent)
                                throw new BadRequestException(GeneralCode.InvalidParams, $"Không tìm thấy {_category.Title.ToLower()} cha có \"{fieldInfo.Title}: {value}\" trong danh mục \"{_category.Title}\"");

                            value = parents.FirstOrDefault()[AccountantConstants.F_IDENTITY].ToString();
                        }
                        else
                        {
                            if (string.IsNullOrWhiteSpace(mf.RefTableCode)) continue;

                            _refCategoryFields.TryGetValue(mf.RefTableCode, out var refFields);

                            var refField = refFields.FirstOrDefault(x => x.Field.CategoryFieldName == mf.RefFieldName);

                            value = TransformValueByDataType(mapping, i, value, refField.Field, mf.Column);

                            _refCategoryDataForProperty.TryGetValue(mf.RefTableCode, out var _refTable);

                            var refProperty = _refTable.Where(x => x[mf.RefFieldName].ToString().ToLower() == value.ToLower());

                            var hasGreaterThanProperty = refProperty.Count() > 1;
                            if (hasGreaterThanProperty)
                                throw new BadRequestException(GeneralCode.InvalidParams, $"Tìm thấy nhiều {refField.Title.ToLower()} có \"{refField.Field.Title}: {value}\" trong danh mục \"{refField.Title}\"");

                            var notFoundProperty = _refCategoryDataForProperty.Count() == 0;
                            if (notFoundProperty)
                                throw new BadRequestException(GeneralCode.InvalidParams, $"Không tìm thấy {refField.Title.ToLower()} có \"{refField.Field.Title}: {value}\" trong danh mục \"{refField.Title}\"");

                            value = refProperty.FirstOrDefault()[AccountantConstants.F_IDENTITY].ToString();
                        }
                    }
                    else
                        value = TransformValueByDataType(mapping, i, value, new CategoryField { Title = mf.Title, DataTypeId = mf.DataTypeId ?? 1 }, mf.Column);

                    categoryDataRow.Add(mf.FieldName, value);
                }

                _CategoryDataRows.Add(categoryDataRow);

            }
        }

        private async Task RefCategoryForParent(CategoryImportExcelMapping mapping)
        {
            var hasParent = mapping.MappingFields.Any(m => m.FieldName == AccountantConstants.PARENT_ID_FIELD_NAME);
            if (hasParent)
            {
                var mapField = mapping.MappingFields.FirstOrDefault(x => x.FieldName == AccountantConstants.PARENT_ID_FIELD_NAME);

                var query = new CategoryQueryRequest()
                {
                    CategoryCode = _category.CategoryCode,
                    FieldQuery = new Dictionary<string, HashSet<string>>()
                };

                var refValues = new HashSet<string>();
                foreach (var (row, i) in _importData.Rows.Select((value, i) => (value, i)))
                {
                    row.TryGetValue(mapField.Column, out var value);

                    if (string.IsNullOrWhiteSpace(value)) continue;

                    var fieldInfo = _categoryFields.FirstOrDefault(x => x.CategoryFieldName == mapField.RefFieldName);

                    value = TransformValueByDataType(mapping, i, value, fieldInfo, mapField.Column);

                    refValues.Add(value);
                }

                query.FieldQuery.Add(mapField.RefFieldName, refValues);

                (await GetRefCategoryDataByMultiField(new[] { query })).TryGetValue(_category.CategoryCode, out _refCategoryDataForParent);

            }
        }

        private async Task RefCategoryForProperty(CategoryImportExcelMapping mapping)
        {
            var hasRefProperty = mapping.MappingFields.Any(m => !string.IsNullOrWhiteSpace(m.RefFieldName) && !m.FieldName.Equals(AccountantConstants.PARENT_ID_FIELD_NAME));
            if (hasRefProperty)
            {
                var groupRefTable = (from mf in mapping.MappingFields.Where(m => !string.IsNullOrWhiteSpace(m.RefFieldName) && !m.FieldName.Equals(AccountantConstants.PARENT_ID_FIELD_NAME))
                                     join cf in _categoryFields on mf.FieldName equals cf.CategoryFieldName
                                     select new
                                     {
                                         mf.Column,
                                         mf.FieldName,
                                         mf.RefFieldName,
                                         cf.RefTableCode,
                                     }).GroupBy(x => x.RefTableCode)
                                     .ToDictionary(k => k.Key, v => v);

                var queries = new List<CategoryQueryRequest>();

                foreach (var (refTableCode, mapField) in groupRefTable)
                {
                    if (string.IsNullOrWhiteSpace(refTableCode)) continue;

                    _refCategoryFields.TryGetValue(refTableCode, out var refFields);
                    foreach (var mf in mapField)
                    {
                        var refField = refFields.FirstOrDefault(x => x.Field.CategoryFieldName == mf.RefFieldName);

                        var query = new CategoryQueryRequest()
                        {
                            CategoryCode = mf.RefTableCode,
                            FieldQuery = new Dictionary<string, HashSet<string>>()
                        };

                        var refValues = new HashSet<string>();
                        foreach (var (row, i) in _importData.Rows.Select((value, i) => (value, i)))
                        {
                            row.TryGetValue(mf.Column, out var value);

                            if (string.IsNullOrWhiteSpace(value)) continue;

                            value = TransformValueByDataType(mapping, i, value, refField.Field, mf.Column);

                            refValues.Add(value);
                        }

                        queries.Add(new CategoryQueryRequest()
                        {
                            CategoryCode = refTableCode,
                            FieldQuery = new Dictionary<string, HashSet<string>>(new[] { KeyValuePair.Create(mf.RefFieldName, refValues) })
                        });
                    }
                }

                _refCategoryDataForProperty = await GetRefCategoryDataByMultiField(queries);
            }
        }

        private string TransformValueByDataType(CategoryImportExcelMapping mapping, int i, string value, CategoryField fieldInfo, string column)
        {
            if (new[] { EnumDataType.Date, EnumDataType.Month, EnumDataType.QuarterOfYear, EnumDataType.Year }.Contains(((EnumDataType?)fieldInfo?.DataTypeId).GetValueOrDefault()))
            {
                if (!DateTime.TryParse(value.ToString(), out DateTime date))
                    throw new BadRequestException(GeneralCode.InvalidParams, $"Không thể chuyển giá trị {value}, dòng {i + mapping.FromRow} cột {column}, trường \"{fieldInfo.Title}\" sang kiểu ngày tháng");
                value = date.AddMinutes(_currentContextService.TimeZoneOffset.Value).GetUnix().ToString();
            }

            if ((EnumDataType)fieldInfo?.DataTypeId == EnumDataType.Boolean)
            {
                if (!value.HasValueInRangeOfAllowValueForBoolean())
                    throw new BadRequestException(GeneralCode.InvalidParams, $"Dòng {i + mapping.FromRow} cột {column}, trường \"{fieldInfo.Title}\" chỉ nhận các giá trị sau: {string.Join(", ", Utils.GetRangeOfAllowValueForBoolean())}");

                value = value.IsRangeOfAllowValueForBooleanTrueValue() ? "true" : "false";
            }

            return value;
        }

        private async Task GetCategoryFieldInfo(CategoryImportExcelMapping mapping)
        {
            _categoryFields = _accountancyContext.CategoryField
                            .AsNoTracking()
                            .Where(f => _categoryId == f.CategoryId)
                            .ToList();

            ValidateCategoryFieldRequirement(mapping);

            var refCategoryCodes = _categoryFields.Select(f => f.RefTableCode).ToList();

            _refCategoryFields = (await (from f in _accountancyContext.CategoryField
                                         join c in _accountancyContext.Category on f.CategoryId equals c.CategoryId
                                         where refCategoryCodes.Contains(c.CategoryCode)
                                         select new RefCategoryProperty
                                         {
                                             CategoryCode = c.CategoryCode,
                                             Title = c.Title,
                                             Field = f
                                         }
                                    ).ToListAsync())
                                    .GroupBy(f => f.CategoryCode)
                                    .ToDictionary(k => k.Key, v => v.Select(x => x));

        }

        private void ValidateCategoryFieldRequirement(CategoryImportExcelMapping mapping)
        {
            _uniqueFields = _categoryFields.Where(x => x.IsUnique).ToArray();
            var requiredFields = _categoryFields.Where(x => x.IsRequired || x.IsUnique).ToList();

            var mappingFields = _categoryFields.Where(x => mapping.MappingFields.Any(y => y.FieldName == x.CategoryFieldName)).ToList();

            if (mappingFields.Where(x => x.IsRequired || x.IsUnique).Count() != requiredFields.Count)
                throw new BadRequestException(GeneralCode.InvalidParams, $"Thiếu dữ liệu của các trường bắt buộc: {string.Join(", ", requiredFields.Select(x => x.Title))} trong file excel");
        }

        private async Task ReadExcelData(CategoryImportExcelMapping mapping, Stream stream)
        {
            var reader = new ExcelReader(stream);
            _importData = reader.ReadSheets(mapping.SheetName, mapping.FromRow, mapping.ToRow, null).FirstOrDefault();

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

        private async Task<bool> ValidateCategory()
        {
            if (_category == null)
            {
                throw new BadRequestException(CategoryErrorCode.CategoryNotFound);
            }

            if (_category.IsReadonly)
            {
                throw new BadRequestException(CategoryErrorCode.CategoryReadOnly);
            }

            if (_category.IsOutSideData)
            {
                throw new BadRequestException(CategoryErrorCode.CategoryIsOutSideData);
            }

            return await Task.FromResult(true);
        }

        private class CategoryQueryRequest
        {
            public string CategoryCode { get; set; }
            public Dictionary<string, HashSet<string>> FieldQuery { get; set; }
        }

        private class RefCategoryProperty
        {
            public string CategoryCode { get; set; }
            public string Title { get; set; }

            public CategoryField Field { get; set; }
        }
    }
}