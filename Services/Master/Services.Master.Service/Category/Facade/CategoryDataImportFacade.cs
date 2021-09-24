using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using static Verp.Resources.Master.Category.CategoryDataValidationMessage;
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
using Verp.Resources.Master.Category;
using VErp.Infrastructure.ServiceCore.Facade;

namespace VErp.Services.Master.Service.Category
{
    public class CategoryDataImportFacade
    {
        private readonly int _categoryId;
        private readonly MasterDBContext _masterContext;
        private readonly ICategoryDataService _categoryDataService;
        private readonly ICurrentContextService _currentContextService;

        private CategoryEntity _category;
        private ExcelSheetDataModel _importData;
        private List<CategoryField> _categoryFields;

        private List<NonCamelCaseDictionary> _refCategoryDataForParent = new List<NonCamelCaseDictionary>();
        private Dictionary<string, List<NonCamelCaseDictionary>> _refCategoryDataForProperty;
        private Dictionary<string, IEnumerable<RefCategoryProperty>> _refCategoryFields;
        private CategoryField[] _uniqueFields;
        private List<Dictionary<string, string>> _categoryDataRows;

        private readonly ObjectActivityLogFacade _categoryDataActivityLog;


        public CategoryDataImportFacade(int categoryId, MasterDBContext accountancyContext, ICategoryDataService categoryDataService, ObjectActivityLogFacade categoryDataActivityLog, ICurrentContextService currentContextService)
        {
            _categoryId = categoryId;
            _masterContext = accountancyContext;
            _categoryDataService = categoryDataService;
            _categoryDataActivityLog = categoryDataActivityLog;
            _currentContextService = currentContextService;
        }

        public async Task<bool> ImportData(ImportExcelMapping mapping, Stream stream)
        {
            _category = _masterContext.Category.FirstOrDefault(c => c.CategoryId == _categoryId);

            await ValidateCategory();
            await GetCategoryFieldInfo(mapping);

            await ReadExcelData(mapping, stream);

            await RefCategoryForParent(mapping);
            await RefCategoryForProperty(mapping);
            await MappingCategoryDate(mapping);

            var existsCategoryData = (await _categoryDataService.GetCategoryRows(_categoryId, null, null, null, null, 0, 0, "", true)).List;

            var lsUpdateRow = new List<Dictionary<string, string>>();
            var lsAddRow = new List<Dictionary<string, string>>();
            foreach (var row in _categoryDataRows)
            {
                var oldRow = existsCategoryData.FirstOrDefault(x => EqualityBetweenTwoCategory(x, row, _uniqueFields));

                var uniqueFieldMessage = $"{_uniqueFields[0].Title.ToLower()}: {row[_uniqueFields[0].CategoryFieldName]}";

                if (oldRow != null && mapping.ImportDuplicateOptionId == EnumImportDuplicateOption.Denied)
                    throw ImportExistedRowInDatabase.BadRequestFormat(uniqueFieldMessage, _category.Title);


                if (oldRow == null)
                {
                    if (lsAddRow.Any(x => EqualityBetweenTwoCategory(x, row, _uniqueFields)))
                        throw ImportExistedRowInDatabase.BadRequestFormat(uniqueFieldMessage);

                    lsAddRow.Add(row);
                }
                else if (mapping.ImportDuplicateOptionId == EnumImportDuplicateOption.Update)
                {
                    if (lsUpdateRow.Any(x => EqualityBetweenTwoCategory(x, row, _uniqueFields)))
                        throw ImportExistedRowInDatabase.BadRequestFormat(uniqueFieldMessage);

                    if (!row.ContainsKey(CategoryFieldConstants.ParentId))
                        row.Add(CategoryFieldConstants.ParentId, oldRow[CategoryFieldConstants.ParentId].ToString());

                    row.Add(CategoryFieldConstants.F_Id, oldRow[CategoryFieldConstants.F_Id].ToString());
                    lsUpdateRow.Add(row);
                }
            }

            using (var trans = await _masterContext.Database.BeginTransactionAsync())
            {
                using (var logBath = _categoryDataActivityLog.BeginBatchLog())
                {
                    var parentField = mapping.MappingFields.FirstOrDefault(mf => mf.FieldName == CategoryFieldConstants.ParentId);

                    string GetParentId(Dictionary<string, string> row)
                    {
                        if (parentField != null && row.ContainsKey(CategoryFieldConstants.ParentId) && !string.IsNullOrWhiteSpace(row[CategoryFieldConstants.ParentId]))
                        {
                            var refValue = row[CategoryFieldConstants.ParentId].ToString();

                            return _refCategoryDataForParent.FirstOrDefault(x => x[parentField.RefFieldName].ToString().ToLower() == refValue.ToLower())?[CategoryFieldConstants.F_Id]?.ToString();
                        }
                        return "";
                    }

                    IEnumerable<Dictionary<string, string>> GetElementByTreeView(IList<Dictionary<string, string>> data)
                    {
                        var loopData = new List<Dictionary<string, string>>();

                        foreach (var row in data)
                        {
                            if (!row.ContainsKey(CategoryFieldConstants.ParentId))
                                yield return row;
                            else if (!string.IsNullOrWhiteSpace(GetParentId(row)))
                                yield return row;
                            else
                                loopData.Add(row);
                        }

                        if (loopData.Count > 0)
                            foreach (var l in GetElementByTreeView(loopData)) yield return l;
                    }

                    foreach (var cRow in GetElementByTreeView(lsAddRow))
                    {
                        var parentId = GetParentId(cRow);
                        if (!string.IsNullOrWhiteSpace(parentId))
                            cRow[CategoryFieldConstants.ParentId] = GetParentId(cRow);

                        var categoryId = await _categoryDataService.AddCategoryRowToDb(_categoryId, cRow);

                        cRow.Add(CategoryFieldConstants.F_Id, categoryId.ToString());

                        _refCategoryDataForParent.Add(cRow.ToNonCamelCaseDictionary(k => k.Key, v => v.Value));
                    }

                    foreach (var uRow in lsUpdateRow)
                    {
                        var parentId = GetParentId(uRow);
                        if (!string.IsNullOrWhiteSpace(parentId))
                            uRow[CategoryFieldConstants.ParentId] = GetParentId(uRow);

                        var rowId = int.Parse(uRow[CategoryFieldConstants.F_Id]);

                        await _categoryDataService.UpdateCategoryRow(_categoryId, rowId, uRow);
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

        private async Task MappingCategoryDate(ImportExcelMapping mapping)
        {
            _categoryDataRows = new List<Dictionary<string, string>>();

            var mapField = (from mf in mapping.MappingFields
                            join cf in _categoryFields on mf.FieldName equals cf.CategoryFieldName into gcf
                            from cf in gcf.DefaultIfEmpty()
                            select new
                            {
                                mf.Column,
                                mf.FieldName,
                                mf.RefFieldName,
                                mf.IsIgnoredIfEmpty,
                                cf?.RefTableCode,
                                cf?.Title,
                                cf?.DataTypeId
                            });

            foreach (var (row, i) in _importData.Rows.Where(x => x.Values.Where(v => !string.IsNullOrWhiteSpace(v)).Count() > 0).Select((value, i) => (value, i)))
            {
                var categoryDataRow = new Dictionary<string, string>();
                foreach (var mf in mapField)
                {
                    row.TryGetValue(mf.Column, out var value);

                    if (string.IsNullOrWhiteSpace(value) && mf.IsIgnoredIfEmpty)
                    {
                        categoryDataRow = new Dictionary<string, string>();
                        break;
                    }

                    if (string.IsNullOrWhiteSpace(value) && _uniqueFields.Any(x => x.CategoryFieldName == mf.FieldName))
                        throw ImportRequiredFieldInRowEmpty.BadRequestFormat(i + mapping.FromRow, mf.Column, mf.Title, _category.Title);

                    if (string.IsNullOrWhiteSpace(value) || mf.FieldName == ImportStaticFieldConsants.CheckImportRowEmpty)
                        continue;

                    if (!string.IsNullOrWhiteSpace(mf.RefFieldName))
                    {
                        if (string.IsNullOrWhiteSpace(value))
                        {
                            value = string.Empty;
                        }
                        else if (mf.FieldName == CategoryFieldConstants.ParentId)
                        {
                            var fieldInfo = _categoryFields.FirstOrDefault(x => x.CategoryFieldName == mf.RefFieldName);
                            var refColumn = mapField.FirstOrDefault(x => x.FieldName == mf.RefFieldName);

                            value = TransformValueByDataType(mapping, i, value, fieldInfo, mf.Column);

                            if (!_importData.Rows.Any(x => x != row && x[refColumn.Column]?.ToString().ToLower() == value.ToLower()))
                            {
                                var parents = _refCategoryDataForParent.Where(x => x[mf.RefFieldName].ToString().ToLower() == value.ToLower());

                                var hasGreaterThanParent = parents.Count() > 1;
                                if (hasGreaterThanParent)
                                    throw ImportFoundMoreThanOneParent.BadRequestFormat(_category.Title.ToLower(), $"{fieldInfo.Title}: {value}", _category.Title);

                                var notFoundParent = parents.Count() == 0;
                                if (notFoundParent)
                                    throw ImportFoundNoParent.BadRequestFormat(_category.Title.ToLower(), $"{fieldInfo.Title}: {value}", _category.Title);

                            }

                            // value = parents.FirstOrDefault()[CategoryFieldConstants.F_Id].ToString();
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
                                throw ImportFoundMoreThanOneRef.BadRequestFormat(refField.Title.ToLower(), $"{refField.Field.Title}: {value}", refField.Title);

                            var notFoundProperty = _refCategoryDataForProperty.Count() == 0;
                            if (notFoundProperty)
                                throw ImportFoundNoRefValue.BadRequestFormat(refField.Title.ToLower(), $"{refField.Field.Title}: {value}", refField.Title);

                            value = refProperty.FirstOrDefault()[CategoryFieldConstants.F_Id].ToString();
                        }
                    }
                    else
                        value = TransformValueByDataType(mapping, i, value, new CategoryField { Title = mf.Title, DataTypeId = mf.DataTypeId ?? 1 }, mf.Column);

                    categoryDataRow.Add(mf.FieldName, value);
                }

                if (categoryDataRow.Count > 0)
                    _categoryDataRows.Add(categoryDataRow);

            }

            await Task.CompletedTask;
        }

        private async Task RefCategoryForParent(ImportExcelMapping mapping)
        {
            var hasParent = mapping.MappingFields.Any(m => m.FieldName == CategoryFieldConstants.ParentId);
            if (hasParent)
            {
                var mapField = mapping.MappingFields.FirstOrDefault(x => x.FieldName == CategoryFieldConstants.ParentId);

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

        private async Task RefCategoryForProperty(ImportExcelMapping mapping)
        {
            var hasRefProperty = mapping.MappingFields.Any(m => !string.IsNullOrWhiteSpace(m.RefFieldName) && !m.FieldName.Equals(CategoryFieldConstants.ParentId));
            if (hasRefProperty)
            {
                var groupRefTable = (from mf in mapping.MappingFields.Where(m => !string.IsNullOrWhiteSpace(m.RefFieldName) && !m.FieldName.Equals(CategoryFieldConstants.ParentId))
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

        private string TransformValueByDataType(ImportExcelMapping mapping, int i, string value, CategoryField fieldInfo, string column)
        {

            if (((EnumDataType?)fieldInfo?.DataTypeId).GetValueOrDefault().IsTimeType())
            {
                if (!DateTime.TryParse(value, out DateTime date))
                    throw ImportCannotConvertToDateTime.BadRequestFormat(value, i + mapping.FromRow, column, fieldInfo.Title);

                value = date.AddMinutes(_currentContextService.TimeZoneOffset.Value).GetUnix().ToString();
            }

            if ((EnumDataType)fieldInfo?.DataTypeId == EnumDataType.Boolean)
            {
                if (!value.HasValueInRangeOfAllowValueForBoolean())
                    throw ImportOnlyAllowRangeValue.BadRequestFormat(i + mapping.FromRow, column, fieldInfo.Title, string.Join(", ", Utils.GetRangeOfAllowValueForBoolean()));

                value = value.IsRangeOfAllowValueForBooleanTrueValue() ? "true" : "false";
            }

            return value;
        }

        private async Task GetCategoryFieldInfo(ImportExcelMapping mapping)
        {
            _categoryFields = _masterContext.CategoryField
                            .AsNoTracking()
                            .Where(f => _categoryId == f.CategoryId)
                            .ToList();

            ValidateCategoryFieldRequirement(mapping);

            var refCategoryCodes = _categoryFields.Select(f => f.RefTableCode).ToList();

            _refCategoryFields = (await (from f in _masterContext.CategoryField
                                         join c in _masterContext.Category on f.CategoryId equals c.CategoryId
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

        private void ValidateCategoryFieldRequirement(ImportExcelMapping mapping)
        {
            _uniqueFields = _categoryFields.Where(x => x.IsUnique).ToArray();
            // var requiredFields = _categoryFields.Where(x => x.IsRequired || x.IsUnique).ToList();

            var mappingFields = _categoryFields.Where(x => mapping.MappingFields.Any(y => y.FieldName == x.CategoryFieldName)).ToList();

            if (mappingFields.Where(x => x.IsUnique).Count() != _uniqueFields.Length)
                throw ImportRequiredFieldNotMapped.BadRequestFormat(string.Join(", ", _uniqueFields.Select(x => x.Title)));
        }

        private async Task ReadExcelData(ImportExcelMapping mapping, Stream stream)
        {
            var reader = new ExcelReader(stream);
            _importData = reader.ReadSheets(mapping.SheetName, mapping.FromRow, mapping.ToRow, null).FirstOrDefault();

            await Task.CompletedTask;
        }

        private async Task<Dictionary<string, List<NonCamelCaseDictionary>>> GetRefCategoryDataByMultiField(IList<CategoryQueryRequest> categoryQueryRequests)
        {
            var categoryCodes = categoryQueryRequests.Select(c => c.CategoryCode).ToList();
            var refCategoryFields = (await (
                from f in _masterContext.CategoryField
                join c in _masterContext.Category on f.CategoryId equals c.CategoryId
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

                var data = await _masterContext.QueryDataTable(dataSql.ToString(), sqlParams.ToArray());

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