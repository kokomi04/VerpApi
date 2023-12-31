﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Reflection.Emit;
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
using VErp.Infrastructure.EF.AccountingDB;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Accountant.Model.Category;
using CategoryEntity = VErp.Infrastructure.EF.AccountingDB.Category;

namespace VErp.Services.Accountant.Service.Category.Implement
{
    public class CategoryRowService : AccoutantBaseService, ICategoryRowService
    {

        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;

        public CategoryRowService(AccountingDBContext accountingContext
            , IOptions<AppSetting> appSetting
            , ILogger<CategoryRowService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            ) : base(accountingContext, appSetting, mapper)
        {
            _logger = logger;
            _activityLogService = activityLogService;
        }

        public async Task<PageData<CategoryRowListOutputModel>> GetCategoryRows(int categoryId, string keyword, string filters, int page, int size)
        {
            var total = 0;
            var lst = new List<CategoryRowListOutputModel>();
            Clause filterClause = null;
            if (!string.IsNullOrEmpty(filters))
            {
                filterClause = JsonConvert.DeserializeObject<Clause>(filters);
                var fields = _accountingContext.CategoryField.Where(f => f.CategoryId == categoryId).ToList();
                filterClause = AddFieldName(filterClause, fields);
            }
            var category = await _accountingContext.Category.FirstOrDefaultAsync(c => c.CategoryId == categoryId);

            IQueryable<CategoryRow> query;
            IQueryable<CategoryRow> notFilterData;
            if (category.IsOutSideData && !category.IsTreeView)
            {
                query = GetOutSideCategoryRows(categoryId, filterClause);
                notFilterData = query;
            }
            else
            {
                if (category.IsOutSideData)
                {
                    query = GetOutSideCategoryRows(categoryId);
                }
                else
                {
                    query = _accountingContext.CategoryRow
                     .Where(r => r.CategoryId == categoryId)
                     .Include(r => r.CategoryRowValue);
                }
                notFilterData = query;
                if (filters != null)
                {
                    ParameterExpression param = Expression.Parameter(typeof(CategoryRow), "r");
                    Expression filter = FilterClauseProcess(param, filterClause, query);
                    query = query.Where(Expression.Lambda<Func<CategoryRow, bool>>(filter, param));
                }
            }
            // search
            keyword = (keyword ?? "").Trim();
            
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(r => r.CategoryRowValue.Any(rv => !string.IsNullOrEmpty(rv.Value) && rv.Value.Contains(keyword)));
            }
            total = query.Count();
            if (size > 0)
            {
                if (category.IsTreeView)
                {
                    lst = query.ProjectTo<CategoryRowListOutputModel>(_mapper.ConfigurationProvider).ToList();
                    int[] parentIds = GetParentIds(lst);
                    var parents = notFilterData
                        .ProjectTo<CategoryRowListOutputModel>(_mapper.ConfigurationProvider)
                        .Where(r => parentIds.Contains(r.CategoryRowId))
                        .ToList();
                    foreach (var parent in parents)
                    {
                        parent.IsDisabled = true;
                    }

                    lst.AddRange(parents);
                    lst = SortCategoryRows(lst).Skip((page - 1) * size).Take(size).ToList();
                }
                else
                {
                    lst = query.ProjectTo<CategoryRowListOutputModel>(_mapper.ConfigurationProvider)
                         .OrderBy(r => r.CategoryRowId).Skip((page - 1) * size).Take(size)
                         .ToList();
                }
            }
            return (lst, total);
        }


        private int[] GetParentIds(List<CategoryRowListOutputModel> categoryRows)
        {
            List<int> result = new List<int>();

            int[] parentIds = categoryRows
                .Where(r => r.ParentCategoryRowId.HasValue && !categoryRows.Any(p => p.CategoryRowId == r.ParentCategoryRowId))
                .Select(r => r.ParentCategoryRowId.Value)
                .Distinct()
                .ToArray();
            result.AddRange(parentIds);

            while (parentIds.Length > 0)
            {
                parentIds = _accountingContext.CategoryRow
                    .Where(r => parentIds.Contains(r.CategoryRowId) && r.ParentCategoryRowId.HasValue)
                    .Select(r => r.ParentCategoryRowId.Value)
                    .Distinct()
                    .ToArray();
                result.AddRange(parentIds);
            }

            return result.Distinct().ToArray();
        }

        private List<CategoryRowListOutputModel> SortCategoryRows(List<CategoryRowListOutputModel> categoryRows)
        {
            int level = 0;
            categoryRows = categoryRows.OrderBy(r => r.CategoryRowId).ToList();
            List<CategoryRowListOutputModel> nodes = new List<CategoryRowListOutputModel>();

            var items = categoryRows.Where(r => !r.ParentCategoryRowId.HasValue || !categoryRows.Any(p => p.CategoryRowId == r.ParentCategoryRowId)).ToList();
            categoryRows = categoryRows.Where(r => r.ParentCategoryRowId.HasValue && categoryRows.Any(p => p.CategoryRowId == r.ParentCategoryRowId)).ToList();

            foreach (var item in items)
            {
                item.CategoryRowLevel = level;
                nodes.Add(item);
                nodes.AddRange(GetChilds(ref categoryRows, item.CategoryRowId, level));
            }

            return nodes;
        }

        private IEnumerable<CategoryRowListOutputModel> GetChilds(ref List<CategoryRowListOutputModel> categoryRows, int categoryRowId, int level)
        {
            level++;
            List<CategoryRowListOutputModel> nodes = new List<CategoryRowListOutputModel>();
            var items = categoryRows.Where(r => r.ParentCategoryRowId == categoryRowId).ToList();
            categoryRows.RemoveAll(r => r.ParentCategoryRowId == categoryRowId);
            foreach (var item in items)
            {
                item.CategoryRowLevel = level;
                nodes.Add(item);
                nodes.AddRange(GetChilds(ref categoryRows, item.CategoryRowId, level));
            }
            return nodes;
        }

        public async Task<ServiceResult<CategoryRowOutputModel>> GetCategoryRow(int categoryId, int categoryRowId)
        {
            var category = _accountingContext.Category.FirstOrDefault(c => c.CategoryId == categoryId);
            CategoryRowOutputModel categoryRow = null;
            if (category.IsOutSideData)
            {
                GetOutSideCategoryRow(categoryId, categoryRowId, ref categoryRow);
            }
            else
            {
                categoryRow = await _accountingContext.CategoryRow
                    .Where(r => r.CategoryId == categoryId && r.CategoryRowId == categoryRowId)
                    .Include(r => r.ParentCategoryRow)
                    .ThenInclude(pr => pr.CategoryRowValue)
                    .ThenInclude(rv => rv.CategoryField)
                    .Include(r => r.ParentCategoryRow)
                    .ProjectTo<CategoryRowOutputModel>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync();

            }
            // Check row 
            if (categoryRow == null)
            {
                throw new BadRequestException(CategoryErrorCode.CategoryRowNotFound);
            }

            return categoryRow;
        }

        public async Task<ServiceResult<List<MapTitleOutputModel>>> MapTitle(MapTitleInputModel[] categoryValues)
        {
            var titles = new List<MapTitleOutputModel>();
            var groups = categoryValues.GroupBy(v => new { v.ReferCategoryFieldId });

            var refFieldIds = groups.Select(g => g.Key.ReferCategoryFieldId).ToList();

            var categories = (from category in _accountingContext.Category
                                               join field in _accountingContext.CategoryField on category.CategoryId equals field.CategoryId
                                               where refFieldIds.Contains(field.CategoryFieldId)
                                               select category)
                .Include(c => c.OutSideDataConfig)
                .ToList();

            var categoryIds = categories.Select(c => c.CategoryId).ToList();

            var fields = await _accountingContext.CategoryField.Where(f => categoryIds.Contains(f.CategoryId)).ToListAsync();

            var refObjects = new List<(int Index, int RefeCategoryFieldId, string FieldName, string Value)>();

            foreach (var group in groups)
            {
                CategoryField referField = fields.First(f => f.CategoryFieldId == group.Key.ReferCategoryFieldId);
                CategoryEntity category = categories.First(c => c.CategoryId == referField.CategoryId);
                bool isOutSide = category.IsOutSideData;
                IQueryable<CategoryRow> query;
                List<string> values = group.Select(g => g.Value).Distinct().ToList();
                if (isOutSide)
                {
                    string fieldName = referField.CategoryFieldName != AccountantConstants.F_IDENTITY ? referField.CategoryFieldName : category.OutSideDataConfig.Key;
                   
                    Clause clause = new SingleClause
                    {
                        DataType = (EnumDataType)referField.DataTypeId,
                        FieldName = fieldName,
                        Operator = EnumOperator.InList,
                        Value = string.Join(",", values.ToArray())
                    };
                    query = GetOutSideCategoryRows(category.CategoryId, clause);
                }
                else
                {
                    query = _accountingContext.CategoryRow
                        .Where(r => r.CategoryId == category.CategoryId)
                        .Include(r => r.CategoryRowValue);
                }
                var data = query.Where(r => r.CategoryRowValue.Any(rv => rv.CategoryFieldId == referField.CategoryFieldId && values.Contains(rv.Value))).ToList();

                foreach (var value in values)
                {
                    var row = data.FirstOrDefault(r => r.CategoryRowValue.Any(rv => rv.CategoryFieldId == referField.CategoryFieldId && value == rv.Value));
                    var referObject = new NonCamelCaseDictionary();
                    if (row != null)
                    {
                        foreach (var rowValue in row.CategoryRowValue)
                        {
                            var field = fields.First(f => f.CategoryFieldId == rowValue.CategoryFieldId);
                            bool isRef = AccountantConstants.SELECT_FORM_TYPES.Contains((EnumFormType)rowValue.CategoryField.FormTypeId);

                            if (isRef)
                            {
                                refObjects.Add((titles.Count, field.ReferenceCategoryFieldId.Value, field.CategoryFieldName, rowValue.Value));
                            }
                            else
                            {
                                referObject.Add(field.CategoryFieldName, rowValue.Value);
                            }
                        }
                    }

                    var title = new MapTitleOutputModel
                    {
                        ReferCategoryFieldId = referField.CategoryFieldId,
                        Value = value,
                        ReferObject = referObject
                    };
                    titles.Add(title);
                }
            }
            MapRefer(ref titles, refObjects);
            return titles;
        }

        private void MapRefer(ref List<MapTitleOutputModel> titles, List<(int Index, int ReferCategoryFieldId, string FieldName, string Value)> refObjects)
        {
            var groups = refObjects.GroupBy(r => r.ReferCategoryFieldId);
            var refFieldIds = groups.Select(g => g.Key).ToList();
            List<CategoryEntity> categories = (from category in _accountingContext.Category
                                               join field in _accountingContext.CategoryField on category.CategoryId equals field.CategoryId
                                               where refFieldIds.Contains(field.CategoryFieldId)
                                               select category)
                .Include(c => c.OutSideDataConfig)
                .ToList();
            var categoryIds = categories.Select(c => c.CategoryId).ToList();
            List<CategoryField> fields = _accountingContext.CategoryField.Where(f => categoryIds.Contains(f.CategoryId)).ToList();
            foreach (var group in groups)
            {
                CategoryField referField = fields.First(f => f.CategoryFieldId == group.Key);
                CategoryEntity category = categories.First(c => c.CategoryId == referField.CategoryId);
                bool isOutSide = category.IsOutSideData;
                IQueryable<CategoryRow> query;
                List<string> values = group.Select(g => g.Value).Distinct().ToList();
                if (isOutSide)
                {
                    string fieldName = referField.CategoryFieldName != AccountantConstants.F_IDENTITY ? referField.CategoryFieldName : category.OutSideDataConfig.Key;
                    Clause clause = new SingleClause
                    {
                        DataType = (EnumDataType)referField.DataTypeId,
                        FieldName = fieldName,
                        Operator = EnumOperator.InList,
                        Value = string.Join(",", values.ToArray())
                    };
                    query = GetOutSideCategoryRows(category.CategoryId, clause);
                }
                else
                {
                    query = _accountingContext.CategoryRow
                        .Where(r => r.CategoryId == category.CategoryId)
                        .Include(r => r.CategoryRowValue);
                }

                var data = query.Where(r => r.CategoryRowValue.Any(rv => rv.CategoryFieldId == referField.CategoryFieldId && values.Contains(rv.Value))).ToList();

                foreach (var value in values)
                {
                    var row = data.FirstOrDefault(r => r.CategoryRowValue.Any(rv => rv.CategoryFieldId == referField.CategoryFieldId && value == rv.Value));
                    var referObject = new NonCamelCaseDictionary();
                    if (row != null)
                    {
                        foreach (var rowValue in row.CategoryRowValue)
                        {
                            var field = fields.First(f => f.CategoryFieldId == rowValue.CategoryFieldId);
                            referObject.Add(field.CategoryFieldName, rowValue.Value);
                        }
                    }
                    foreach (var item in group.Where(g => g.Value == value))
                    {
                        titles[item.Index].ReferObject.Add(item.FieldName, new
                        {
                            item.Value,
                            ReferObject = referObject
                        });
                    }
                }
            }
        }

        private void GetOutSideCategoryRow(int categoryId, int categoryRowId, ref CategoryRowOutputModel categoryRow)
        {
            try
            {
                var config = _accountingContext.OutSideDataConfig.FirstOrDefault(cf => cf.CategoryId == categoryId);
                if (!string.IsNullOrEmpty(config?.Url))
                {
                    string url = $"{config.Url}/{categoryRowId}";
                    (JObject, HttpStatusCode) result = GetFromAPI<JObject>(url, 100000, HttpMethod.Get);
                    if (result.Item2 == HttpStatusCode.OK)
                    {
                        List<CategoryField> fields = _accountingContext.CategoryField.Where(f => categoryId == f.CategoryId).ToList();

                        // Lấy thông tin row
                        Dictionary<string, string> properties = new Dictionary<string, string>();
                        foreach (var jprop in result.Item1.Properties())
                        {
                            var key = jprop.Name;
                            var value = jprop.Value.ToString();
                            properties.Add(key, value);
                        }

                        // Map row
                        categoryRow = new CategoryRowOutputModel
                        {
                            CategoryRowId = categoryRowId
                        };

                        // Map value cho các field
                        foreach (var field in fields)
                        {
                            CategoryValueInputModel rowValue;
                            if (field.CategoryFieldName == AccountantConstants.F_IDENTITY)
                            {
                                rowValue = new CategoryValueInputModel
                                {
                                    CategoryFieldId = field.CategoryFieldId,
                                    Value = categoryRowId.ToString()
                                };
                            }
                            else
                            {
                                bool bValue = properties.TryGetValue(field.CategoryFieldName, out string value);
                                rowValue = new CategoryValueInputModel
                                {
                                    CategoryFieldId = field.CategoryFieldId,
                                    Value = bValue ? value : string.Empty,
                                };
                            }
                            categoryRow.CategoryRowValues.Add(rowValue);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetOutSideCategoryRow");
                throw new BadRequestException(CategoryErrorCode.CategoryIsOutSideDataError);
            }
        }

        public async Task<ServiceResult<int>> AddCategoryRow(int categoryId, CategoryRowInputModel data)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockCategoryKey(categoryId));
            // Validate
            var category = _accountingContext.Category.FirstOrDefault(c => c.CategoryId == categoryId);
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
            CheckParentRow(data, category);

            // Lấy thông tin field
            var categoryFields = _accountingContext.CategoryField
                .Include(f => f.DataType)
                .Where(f => categoryId == f.CategoryId)
                .AsEnumerable();
            var requiredFields = categoryFields.Where(f => !f.AutoIncrement && f.IsRequired);
            var uniqueFields = categoryFields.Where(f => !f.AutoIncrement && f.IsUnique);
            var selectFields = categoryFields.Where(f => !f.AutoIncrement && (f.FormTypeId == (int)EnumFormType.SearchTable || f.FormTypeId == (int)EnumFormType.Select));

            // Check field required
            CheckRequired(data, requiredFields);
            // Check refer
            CheckRefer(ref data, selectFields);
            // Check unique
            CheckUnique(data, uniqueFields);
            // Check value
            CheckValue(data, categoryFields);

            using var trans = await _accountingContext.Database.BeginTransactionAsync();
            try
            {
                var categoryRow = await InsertCategoryRowAsync(categoryId, categoryFields, data);
                trans.Commit();
                await _activityLogService.CreateLog(EnumObjectType.Category, categoryRow.CategoryRowId, $"Thêm dòng cho danh mục {category.Title}", data.JsonSerialize());
                return categoryRow.CategoryRowId;
            }
            catch (Exception ex)
            {
                trans.Rollback();
                _logger.LogError(ex, "Create");
                return GeneralCode.InternalError;
            }
        }

        private async Task<CategoryRow> InsertCategoryRowAsync(int categoryId, IEnumerable<CategoryField> categoryFields, CategoryRowInputModel data)
        {
            // Thêm dòng
            var categoryRow = new CategoryRow
            {
                CategoryId = categoryId,
                ParentCategoryRowId = data.ParentCategoryRowId
            };
            await _accountingContext.CategoryRow.AddAsync(categoryRow);
            await _accountingContext.SaveChangesAsync();

            // Duyệt danh sách field
            foreach (var field in categoryFields.Where(f => f.CategoryFieldName != AccountantConstants.F_IDENTITY))
            {
                bool isRef = AccountantConstants.SELECT_FORM_TYPES.Contains((EnumFormType)field.FormTypeId);
                var valueItem = data.CategoryRowValues.FirstOrDefault(v => v.CategoryFieldId == field.CategoryFieldId);
                if ((valueItem == null || (string.IsNullOrEmpty(valueItem.Value) && !field.AutoIncrement)))
                {
                    continue;
                }
                CategoryRowValue categoryRowValue = new CategoryRowValue
                {
                    CategoryRowId = categoryRow.CategoryRowId,
                    CategoryFieldId = field.CategoryFieldId,
                };
                string value = string.Empty;
                long valueInNumber = 0;
                if (field.AutoIncrement)
                {
                    // Lấy ra value lớn nhất
                    long max = _accountingContext.CategoryRowValue.Where(v => v.CategoryFieldId == field.CategoryFieldId).Max(v => v.ValueInNumber);
                    valueInNumber = (max / AccountantConstants.CONVERT_VALUE_TO_NUMBER_FACTOR) + 1;
                    value = valueInNumber.ToString();
                }
                else
                {
                    value = valueItem.Value;
                    valueInNumber = value.ConvertValueToNumber((EnumDataType)field.DataTypeId);
                }
                // Thêm value
                categoryRowValue.Value = value;
                categoryRowValue.ValueInNumber = valueInNumber;
                await _accountingContext.CategoryRowValue.AddAsync(categoryRowValue);
            }

            var identityField = categoryFields.First(f => f.CategoryFieldName == AccountantConstants.F_IDENTITY);
            // Insert F_Identity Value
            CategoryRowValue identityValue = new CategoryRowValue
            {
                CategoryRowId = categoryRow.CategoryRowId,
                CategoryFieldId = identityField.CategoryFieldId,
                ValueInNumber = categoryRow.CategoryRowId,
                Value = categoryRow.CategoryRowId.ToString()
            };
            await _accountingContext.CategoryRowValue.AddAsync(identityValue);
            await _accountingContext.SaveChangesAsync();
            return categoryRow;
        }

        public async Task<Enum> UpdateCategoryRow(int categoryId, int categoryRowId, CategoryRowInputModel data)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockCategoryKey(categoryId));
            var categoryRow = await _accountingContext.CategoryRow.FirstOrDefaultAsync(c => c.CategoryRowId == categoryRowId && c.CategoryId == categoryId);
            if (categoryRow == null)
            {
                return CategoryErrorCode.CategoryRowNotFound;
            }

            // Check parent row
            if (categoryRow.ParentCategoryRowId != data.ParentCategoryRowId)
            {
                var category = _accountingContext.Category.First(c => c.CategoryId == categoryId);
                CheckParentRow(data, category, categoryRowId);
            }

            // Lấy thông tin field
            var categoryFields = _accountingContext.CategoryField
                .Include(f => f.DataType)
                .Where(f => categoryId == f.CategoryId)
                .Where(f => f.CategoryFieldName != AccountantConstants.F_IDENTITY)
                .AsEnumerable();

            // Lấy thông tin value hiện tại
            var currentValues = (from rowValue in _accountingContext.CategoryRowValue
                                 where rowValue.CategoryRowId == categoryRowId
                                 //join field in _accountingContext.CategoryField on rowValue.CategoryFieldId equals field.CategoryFieldId
                                 //join referRowValue in _accountingContext.CategoryRowValue
                                 //on new { categoryRowId = (int)rowValue.ValueInNumber, categoryFieldId = field.ReferenceCategoryTitleFieldId.Value }
                                 //equals new { categoryRowId = referRowValue.CategoryRowId, categoryFieldId = referRowValue.CategoryFieldId } into g
                                 //from subRefer in g.DefaultIfEmpty()
                                 //select new
                                 //{
                                 //    Data = rowValue,
                                 //    //ReferData = subRefer
                                 //})
                                 select rowValue)
                                 .ToList();

            // Lấy các trường thay đổi
            List<CategoryField> updateFields = new List<CategoryField>();
            foreach (CategoryField categoryField in categoryFields)
            {
                var currentValue = currentValues.FirstOrDefault(v => v.CategoryFieldId == categoryField.CategoryFieldId);
                var updateValue = data.CategoryRowValues.FirstOrDefault(v => v.CategoryFieldId == categoryField.CategoryFieldId);

                if (currentValue != null || updateValue != null)
                {
                    if (currentValue == null || updateValue == null)
                    {
                        updateFields.Add(categoryField);
                    }
                    else
                    {
                        bool isRef = AccountantConstants.SELECT_FORM_TYPES.Contains((EnumFormType)categoryField.FormTypeId);
                        if (currentValue.Value != updateValue.Value)
                        {
                            updateFields.Add(categoryField);
                        }
                    }
                }
            }

            // Lấy thông tin field
            var requiredFields = updateFields.Where(f => !f.AutoIncrement && f.IsRequired);
            var uniqueFields = updateFields.Where(f => !f.AutoIncrement && f.IsUnique);
            var selectFields = updateFields.Where(f => !f.AutoIncrement && (f.FormTypeId == (int)EnumFormType.SearchTable || f.FormTypeId == (int)EnumFormType.Select));

            // Check field required
            CheckRequired(data, requiredFields);

            // Check refer
            CheckRefer(ref data, selectFields);

            // Check unique
            CheckUnique(data, uniqueFields, categoryRowId);

            // Check value
            CheckValue(data, categoryFields);

            using var trans = await _accountingContext.Database.BeginTransactionAsync();
            try
            {
                // Update parent id
                categoryRow.ParentCategoryRowId = data.ParentCategoryRowId;

                // Duyệt danh sách field
                foreach (var field in updateFields)
                {
                    var oldValue = currentValues.FirstOrDefault(v => v.CategoryFieldId == field.CategoryFieldId);
                    var valueItem = data.CategoryRowValues.FirstOrDefault(v => v.CategoryFieldId == field.CategoryFieldId);

                    if (field.AutoIncrement || ((valueItem == null || string.IsNullOrEmpty(valueItem.Value)) && oldValue == null))
                    {
                        continue;
                    }
                    else if (valueItem == null || string.IsNullOrEmpty(valueItem.Value))  // Xóa giá trị cũ
                    {
                        var currentRowValue = _accountingContext.CategoryRowValue.FirstOrDefault(rv => rv.CategoryRowId == categoryRowId && rv.CategoryFieldId == field.CategoryFieldId);
                        if (currentRowValue != null)
                        {
                            currentRowValue.IsDeleted = true;
                        }
                    }
                    else if (oldValue == null) // Nếu giá trị cũ là null, tạo mới, map lại
                    {
                        CategoryRowValue categoryRowValue = new CategoryRowValue
                        {
                            CategoryFieldId = field.CategoryFieldId,
                            CategoryRowId = categoryRowId,
                            Value = valueItem.Value,
                            ValueInNumber = valueItem.Value.ConvertValueToNumber((EnumDataType)field.DataTypeId)
                        };
                        _accountingContext.CategoryRowValue.Add(categoryRowValue);
                    }
                    else //nếu có giá trị cũ
                    {
                        var currentRowValue = _accountingContext.CategoryRowValue.First(rv => rv.CategoryRowId == categoryRowId && rv.CategoryFieldId == field.CategoryFieldId);
                        currentRowValue.Value = valueItem.Value;
                        currentRowValue.ValueInNumber = valueItem.Value.ConvertValueToNumber((EnumDataType)field.DataTypeId);
                    }
                }
                await _accountingContext.SaveChangesAsync();
                trans.Commit();
                await _activityLogService.CreateLog(EnumObjectType.Category, categoryRow.CategoryRowId, $"Cập nhật dòng dữ liệu {categoryRow.CategoryRowId}", data.JsonSerialize());
                return GeneralCode.Success;
            }
            catch (Exception ex)
            {
                trans.Rollback();
                _logger.LogError(ex, "Update");
                return GeneralCode.InternalError;
            }
        }

        public async Task<Enum> DeleteCategoryRow(int categoryId, int categoryRowId)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockCategoryKey(categoryId));
            var categoryRow = await _accountingContext.CategoryRow.FirstOrDefaultAsync(c => c.CategoryRowId == categoryRowId && c.CategoryId == categoryId);
            if (categoryRow == null)
            {
                return CategoryErrorCode.CategoryRowNotFound;
            }
            // Lấy thông tin field

            var categoryFields = _accountingContext.CategoryField.Where(f => categoryId == f.CategoryId).AsEnumerable();

            // Check reference
            foreach (var field in categoryFields)
            {
                if (_accountingContext.CategoryField.Any(c => c.ReferenceCategoryFieldId == field.CategoryFieldId))
                {
                    int valueId = _accountingContext.CategoryRowValue
                        .FirstOrDefault(rv => rv.CategoryFieldId == field.CategoryFieldId && rv.CategoryRowId == categoryRowId)
                        .CategoryRowId;
                    bool isRefer = valueId > 0 && _accountingContext.CategoryRowValue
                        .Include(rv => rv.CategoryField)
                        .Any(rv => AccountantConstants.SELECT_FORM_TYPES.Contains((EnumFormType)rv.CategoryField.FormTypeId) && (int)rv.ValueInNumber == valueId && rv.CategoryRowId != categoryRowId);
                    if (isRefer) return CategoryErrorCode.DestCategoryFieldAlreadyExisted;
                }
            }

            using var trans = await _accountingContext.Database.BeginTransactionAsync();
            try
            {
                // Delete row
                categoryRow.IsDeleted = true;
                foreach (var field in categoryFields)
                {
                    var categoryRowValue = _accountingContext.CategoryRowValue
                      .Where(rv => rv.CategoryFieldId == field.CategoryFieldId && rv.CategoryRowId == categoryRowId)
                      .FirstOrDefault();
                    if (categoryRowValue == null)
                    {
                        continue;
                    }
                    // Delete row-field-value
                    categoryRowValue.IsDeleted = true;
                }
                await _accountingContext.SaveChangesAsync();
                trans.Commit();
                await _activityLogService.CreateLog(EnumObjectType.Category, categoryRowId, $"Xóa dòng dữ liệu {categoryRowId}", categoryRow.JsonSerialize());
                return GeneralCode.Success;
            }
            catch (Exception ex)
            {
                trans.Rollback();
                _logger.LogError(ex, "Delete");
                return GeneralCode.InternalError;
            }
        }

        private void CheckParentRow(CategoryRowInputModel data, CategoryEntity category, int? categoryRowId = null)
        {
            if (categoryRowId.HasValue && data.ParentCategoryRowId == categoryRowId)
            {
                throw new BadRequestException(CategoryErrorCode.ParentCategoryFromItSelf);
            }
            if (category.IsTreeView && data.ParentCategoryRowId.HasValue)
            {
                bool isExist = _accountingContext.CategoryRow.Any(r => r.CategoryId == category.CategoryId && r.CategoryRowId == data.ParentCategoryRowId.Value);
                if (!isExist)
                {
                    throw new BadRequestException(CategoryErrorCode.ParentCategoryRowNotExisted);
                }
            }
        }

        private void CheckRefer(ref CategoryRowInputModel data, IEnumerable<CategoryField> selectFields)
        {
            // Check refer
            foreach (var field in selectFields)
            {
                var valueItem = data.CategoryRowValues.FirstOrDefault(v => v.CategoryFieldId == field.CategoryFieldId);
                if (valueItem != null && !string.IsNullOrEmpty(valueItem.Value))
                {
                    bool isExisted = false;

                    if (field.ReferenceCategoryFieldId.HasValue)
                    {
                        CategoryField referField = _accountingContext.CategoryField.First(f => f.CategoryFieldId == field.ReferenceCategoryFieldId.Value);
                        CategoryEntity referCategory = _accountingContext.Category.First(c => c.CategoryId == referField.CategoryId);
                        IQueryable<CategoryRow> query;
                        Clause filters = null;
                        if (!string.IsNullOrEmpty(field.Filters))
                        {
                            filters = JsonConvert.DeserializeObject<Clause>(field.Filters);
                            var fields = _accountingContext.CategoryField.Where(f => f.CategoryId == referCategory.CategoryId).ToList();
                            filters = AddFieldName(filters, fields);
                        }
                        if (referCategory.IsOutSideData && !referCategory.IsTreeView)
                        {
                            query = GetOutSideCategoryRows(referCategory.CategoryId, filters);
                        }
                        else
                        {
                            if (referCategory.IsOutSideData)
                            {
                                query = GetOutSideCategoryRows(referCategory.CategoryId);
                            }
                            else
                            {
                                query = _accountingContext.CategoryRow
                                 .Where(r => r.CategoryId == referCategory.CategoryId)
                                 .Include(r => r.CategoryRowValue);
                            }
                            if (filters != null)
                            {
                                ParameterExpression param = Expression.Parameter(typeof(CategoryRow), "r");
                                Expression filter = FilterClauseProcess(param, filters, query);
                                query = query.Where(Expression.Lambda<Func<CategoryRow, bool>>(filter, param));
                            }
                        }
                        CategoryRow selectedItem = query.FirstOrDefault(r => r.CategoryRowValue.Any(rv => rv.CategoryFieldId == referField.CategoryFieldId && rv.Value == valueItem.Value));
                        if (isExisted = selectedItem != null)
                        {
                            valueItem.Value = selectedItem.CategoryRowValue.FirstOrDefault(rv => rv.CategoryFieldId == referField.CategoryFieldId)?.Value ?? null;
                        }
                    }
                    if (!isExisted)
                    {
                        throw new BadRequestException(CategoryErrorCode.ReferValueNotFound, new string[] { field.Title });
                    }
                }
            }
        }

        private void CheckUnique(CategoryRowInputModel data, IEnumerable<CategoryField> uniqueFields, int? categoryRowId = null)
        {

            foreach (var field in uniqueFields)
            {
                string[] values = data.CategoryRowValues.Where(v => v.CategoryFieldId == field.CategoryFieldId).Select(v => v.Value).ToArray();

                bool isExisted = (from rowValue in _accountingContext.CategoryRowValue
                                  where (!categoryRowId.HasValue || rowValue.CategoryRowId != categoryRowId) && rowValue.CategoryFieldId == field.CategoryFieldId
                                  select rowValue)
                                  .Any(r => values.Contains(r.Value));
                if (isExisted)
                {
                    throw new BadRequestException(CategoryErrorCode.UniqueValueAlreadyExisted, new string[] { field.Title });
                }
            }
        }

        private void CheckRequired(CategoryRowInputModel data, IEnumerable<CategoryField> requiredFields)
        {
            foreach (var field in requiredFields)
            {
                if (!data.CategoryRowValues.Any(v => v.CategoryFieldId == field.CategoryFieldId && !string.IsNullOrEmpty(v.Value)))
                {
                    throw new BadRequestException(CategoryErrorCode.RequiredFieldIsEmpty, new string[] { field.Title });
                }
            }
        }

        private void CheckValue(CategoryRowInputModel data, IEnumerable<CategoryField> categoryFields)
        {
            foreach (var field in categoryFields)
            {
                var valueItem = data.CategoryRowValues.FirstOrDefault(v => v.CategoryFieldId == field.CategoryFieldId);
                if ((field.FormTypeId == (int)EnumFormType.SearchTable || field.FormTypeId == (int)EnumFormType.Select) || field.AutoIncrement || valueItem == null || string.IsNullOrEmpty(valueItem.Value))
                {
                    continue;
                }
                CheckValue(valueItem, field);
            }
        }

        private void CheckValue(CategoryValueInputModel valueItem, CategoryField field)
        {
            if ((field.DataSize > 0 && valueItem.Value.Length > field.DataSize)
                || (!string.IsNullOrEmpty(field.DataType.RegularExpression) && !Regex.IsMatch(valueItem.Value, field.DataType.RegularExpression))
                || (!string.IsNullOrEmpty(field.RegularExpression) && !Regex.IsMatch(valueItem.Value, field.RegularExpression)))
            {
                throw new BadRequestException(CategoryErrorCode.CategoryValueInValid, new string[] { field.Title });
            }
        }

        public async Task<ServiceResult> ImportCategoryRow(int categoryId, Stream stream)
        {
            try
            {
                var category = _accountingContext.Category.FirstOrDefault(c => c.CategoryId == categoryId);
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
                var categoryFields = _accountingContext.CategoryField
                    .Include(f => f.DataType)
                    .Where(f => categoryId == f.CategoryId)
                    .ToList();

                var inputFieldCount = categoryFields
                    .Where(f => !f.IsHidden && !f.AutoIncrement)
                    .Where(f => f.CategoryFieldName != AccountantConstants.F_IDENTITY).Count();

                List<(CategoryRowInputModel Data, string Key, string ParentKey)> rowInputs = new List<(CategoryRowInputModel Data, string Key, string ParentKey)>();

                string[][] data = reader.ReadFile(inputFieldCount, 0, 1, 0);
                string[] fieldNames = data[0];

                string[][] relationshipData = reader.ReadFile(1, 0, 1, inputFieldCount);

                var requiredFields = categoryFields.Where(f => !f.AutoIncrement && f.IsRequired);
                var uniqueFields = categoryFields.Where(f => !f.AutoIncrement && f.IsUnique);
                var selectFields = categoryFields.Where(f => !f.AutoIncrement && (f.FormTypeId == (int)EnumFormType.SearchTable || f.FormTypeId == (int)EnumFormType.Select));

                for (int rowIndx = 1; rowIndx < data.Length; rowIndx++)
                {
                    string[] row = data[rowIndx];
                    string key = string.Empty;
                    CategoryRowInputModel rowInput = new CategoryRowInputModel();
                    for (int fieldIndx = 0; fieldIndx < fieldNames.Length; fieldIndx++)
                    {
                        string fieldName = fieldNames[fieldIndx];
                        var field = categoryFields.FirstOrDefault(f => f.CategoryFieldName == fieldName);
                        if (field == null) continue;
                        if (field.IsTreeViewKey)
                        {
                            key = row[fieldIndx];
                        }
                        if (field.DataTypeId == (int)EnumDataType.Boolean)
                        {
                            bool value;
                            bool isBoolean = int.TryParse(row[fieldIndx], out int intValue) ? (value = intValue == 1 || intValue == 0) : bool.TryParse(row[fieldIndx], out value);

                            if (isBoolean)
                            {
                                row[fieldIndx] = value.ToString().ToLower();
                            }
                            else
                            {
                                throw new BadRequestException(CategoryErrorCode.CategoryValueInValid, new string[] { field.Title });
                            }
                        }
                        else if (field.DataTypeId == (int)EnumDataType.Date && !string.IsNullOrWhiteSpace(row[fieldIndx]))
                        {
                            if (DateTime.TryParse(row[fieldIndx], out DateTime value))
                            {
                                row[fieldIndx] = value.GetUnix().ToString();
                            }
                            else
                            {
                                throw new BadRequestException(CategoryErrorCode.CategoryValueInValid, new string[] { field.Title });
                            }
                        }

                        CategoryValueInputModel rowValue = new CategoryValueInputModel
                        {
                            CategoryFieldId = field.CategoryFieldId,
                        };
                        rowValue.Value = row[fieldIndx];
                        rowInput.CategoryRowValues.Add(rowValue);
                    }

                    // Check field required
                    CheckRequired(rowInput, requiredFields);
                    // Check unique
                    CheckUnique(rowInput, uniqueFields);
                    // Check refer
                    CheckRefer(ref rowInput, selectFields);
                    // Check value
                    CheckValue(rowInput, categoryFields);

                    rowInputs.Add((rowInput, key, relationshipData[rowIndx][0]));
                }

                using (var trans = await _accountingContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        List<(CategoryRow Data, string Key, string ParentKey)> categoryRows = new List<(CategoryRow Data, string Key, string ParentKey)>();
                        // Insert data
                        foreach (var rowInput in rowInputs)
                        {
                            var categoryRow = await InsertCategoryRowAsync(categoryId, categoryFields, rowInput.Data);
                            categoryRows.Add((categoryRow, rowInput.Key, rowInput.ParentKey));
                        }
                        // Insert relationship
                        if (category.IsTreeView)
                        {
                            foreach (var categoryRow in categoryRows)
                            {
                                if (!string.IsNullOrEmpty(categoryRow.ParentKey))
                                {
                                    int parentId = categoryRows.Where(r => r.Key == categoryRow.ParentKey).Select(r => r.Data.CategoryRowId).FirstOrDefault();
                                    categoryRow.Data.ParentCategoryRowId = parentId;
                                }
                            }
                            _accountingContext.SaveChanges();
                        }
                        trans.Commit();

                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        _logger.LogError(ex, "Import");
                        return GeneralCode.InternalError;
                    }
                }

                return GeneralCode.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Import");
                return CategoryErrorCode.FormatFileInvalid;
            }
        }

        public async Task<ServiceResult<MemoryStream>> GetImportTemplateCategory(int categoryId)
        {
            var category = _accountingContext.Category.FirstOrDefault(c => c.CategoryId == categoryId);
            if (category == null)
            {
                return CategoryErrorCode.CategoryNotFound;
            }
            if (category.IsReadonly)
            {
                return CategoryErrorCode.CategoryReadOnly;
            }
            if (category.IsOutSideData)
            {
                return CategoryErrorCode.CategoryIsOutSideData;
            }
            // Lấy thông tin field
            var categoryFields = _accountingContext.CategoryField
                .Where(f => categoryId == f.CategoryId)
                .Where(f => !f.IsHidden && !f.AutoIncrement)
                .Where(f => f.CategoryFieldName != AccountantConstants.F_IDENTITY)
                .AsEnumerable();
            List<(string, byte[])[]> dataInRows = new List<(string, byte[])[]>();
            List<(string, byte[])> titles = new List<(string, byte[])>();
            List<(string, byte[])> names = new List<(string, byte[])>();
            byte[] titleRgb = new byte[3] { 60, 120, 216 };
            byte[] nameRgb = new byte[3] { 147, 196, 125 };
            foreach (var field in categoryFields)
            {
                titles.Add((field.Title, titleRgb));
                names.Add((field.CategoryFieldName, nameRgb));
            }
            if (category.IsTreeView)
            {
                titles.Add(("ParentKey", titleRgb));
                names.Add(("ParentKey", nameRgb));
            }
            dataInRows.Add(titles.ToArray());
            dataInRows.Add(names.ToArray());

            var writer = new ExcelWriter();
            writer.WriteToSheet(dataInRows, "Data");

            MemoryStream stream = await writer.WriteToStream();
            return stream;
        }


        public async Task<CategoryNameModel> GetFieldDataForMapping(int categoryId)
        {
            var category = _accountingContext.Category.AsNoTracking().FirstOrDefault(c => c.CategoryId == categoryId);
            if (category == null)
            {
                throw new BadRequestException(CategoryErrorCode.CategoryNotFound);
            }
            //if (category.IsReadonly)
            //{
            //    throw new BadRequestException(CategoryErrorCode.CategoryReadOnly);
            //}
            //if (category.IsOutSideData)
            //{
            //    throw new BadRequestException(CategoryErrorCode.CategoryIsOutSideData);
            //}


            var result = new CategoryNameModel()
            {
                CategoryId = category.CategoryId,
                CategoryCode = category.CategoryCode,
                CategoryTitle = category.Title,
                IsTreeView = category.IsTreeView,
                Fields = new List<CategoryFieldNameModel>()
            };

            var fields = await _accountingContext.CategoryField
                .Where(f => categoryId == f.CategoryId && !f.IsHidden && !f.AutoIncrement && f.CategoryFieldName != AccountantConstants.F_IDENTITY)
                .ToListAsync();

            var refCategoryFieldIds = fields.Where(f => f.ReferenceCategoryFieldId.HasValue).Select(f => f.ReferenceCategoryFieldId.Value).Distinct().ToList();

            var refFieldCategoryIds = (await _accountingContext.CategoryField.Where(f => refCategoryFieldIds.Contains(f.CategoryFieldId)).Select(f => new { f.CategoryFieldId, f.CategoryId }).ToListAsync())
                .ToDictionary(f => f.CategoryFieldId, f => f.CategoryId);

            var refCategoryIds = refFieldCategoryIds.Select(f => f.Value).ToList();

            var refCategoryFields = (await (
                from f in _accountingContext.CategoryField
                join c in _accountingContext.Category on f.CategoryId equals c.CategoryId
                where refCategoryIds.Contains(f.CategoryId)
                select new
                {
                    c.CategoryId,
                    c.CategoryCode,
                    CategoryTitle = c.Title,
                    c.IsTreeView,
                    Field = f
                })
                .AsNoTracking()
                .ToListAsync()
                )
                .GroupBy(c => c.CategoryId)
                .ToDictionary(
                    c => c.Key,
                    c => c
                    .GroupBy(i => new { i.CategoryId, i.CategoryCode, i.CategoryTitle, i.IsTreeView })
                    .Select(i => new
                    {
                        CategoryInfo = i.Key,
                        Fields = i.Select(f => f.Field).ToList()
                    })
                    .First()
                );



            foreach (var field in fields)
            {
                var fileData = new CategoryFieldNameModel()
                {
                    CategoryFieldId = field.CategoryFieldId,
                    FieldName = field.CategoryFieldName,
                    FieldTitle = field.Title,
                    RefCategory = null
                };

                if (field.ReferenceCategoryFieldId.HasValue)
                {
                    var refCategoryId = refFieldCategoryIds[field.ReferenceCategoryFieldId.Value];
                    var refCategory = refCategoryFields[refCategoryId];

                    fileData.RefCategory = new CategoryNameModel()
                    {
                        CategoryId = refCategory.CategoryInfo.CategoryId,
                        CategoryCode = refCategory.CategoryInfo.CategoryCode,
                        CategoryTitle = refCategory.CategoryInfo.CategoryTitle,
                        IsTreeView = refCategory.CategoryInfo.IsTreeView,

                        Fields = refCategory.Fields
                        .Select(f => new CategoryFieldNameModel()
                        {
                            CategoryFieldId = f.CategoryFieldId,
                            FieldName = f.CategoryFieldName,
                            FieldTitle = f.Title,
                            RefCategory = null
                        }).ToList()
                    };
                }

                result.Fields.Add(fileData);
            }

            return result;
        }


        public async Task<bool> ImportCategoryRowFromMapping(int categoryId, ImportExelMapping mapping, Stream stream)
        {

            var category = _accountingContext.Category.FirstOrDefault(c => c.CategoryId == categoryId);
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
            var categoryFields = _accountingContext.CategoryField
                .Include(f => f.DataType)
                .Where(f => categoryId == f.CategoryId)
                .ToList();

            var inputFieldCount = categoryFields
                .Where(f => !f.IsHidden && !f.AutoIncrement)
                .Where(f => f.CategoryFieldName != AccountantConstants.F_IDENTITY).Count();


            var data = reader.ReadSheets(mapping.SheetName, mapping.FromRow, mapping.ToRow, null).FirstOrDefault();

            var rowDatas = new List<List<ImportExcelRowData>>();

            var refCategoryFieldDatas = new Dictionary<int, HashSet<string>>();
            for (var rowIndx = 1; rowIndx < data.Rows.Length; rowIndx++)
            {
                var row = data.Rows[rowIndx];

                var rowData = new List<ImportExcelRowData>();
                for (int fieldIndx = 0; fieldIndx < mapping.MappingFields.Count; fieldIndx++)
                {
                    var mappingField = mapping.MappingFields[fieldIndx];
                    var field = categoryFields.FirstOrDefault(f => f.CategoryFieldId == mappingField.FieldId);

                    if (field == null && mappingField.FieldId.HasValue) throw new BadRequestException(GeneralCode.ItemNotFound, "Trường dữ liệu không tìm thấy");

                    string value = null;
                    if (row.ContainsKey(mappingField.Column))
                        value = row[mappingField.Column]?.ToString();

                    if (string.IsNullOrWhiteSpace(value) && mappingField.IsRequire) continue;

                    rowData.Add(new ImportExcelRowData()
                    {
                        FieldMapping = mappingField,
                        FieldConfig = field,
                        CellValue = value
                    });

                    if (mappingField.RefCategoryFieldId.HasValue)
                    {
                        if (!refCategoryFieldDatas.ContainsKey(mappingField.RefCategoryFieldId.Value))
                        {
                            refCategoryFieldDatas.Add(mappingField.RefCategoryFieldId.Value, new HashSet<string>());
                        }

                        if (!refCategoryFieldDatas[mappingField.RefCategoryFieldId.Value].Contains(value))
                            refCategoryFieldDatas[mappingField.RefCategoryFieldId.Value].Add(value);
                    }
                }

                rowDatas.Add(rowData);
            }


            var categoryQueryData = await GetRefCategoryData(refCategoryFieldDatas);


            using (var trans = await _accountingContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var categoryRows = new List<(CategoryRow Data, string Key, string ParentKey)>();
                    // Insert data

                    foreach (var rowData in rowDatas)
                    {
                        var rowInput = new CategoryRowInputModel();

                        var key = "";
                        var parentKey = "";
                        var parentRefFieldId = (int?)null;

                        foreach (var cellData in rowData)
                        {
                            if (cellData.FieldMapping.FieldId.HasValue)
                            {
                                if (cellData.FieldConfig.ReferenceCategoryFieldId.HasValue)
                                {
                                    var refCategoryRow = categoryQueryData.FirstOrDefault(c => c.CategoryFieldId == cellData.FieldMapping.RefCategoryFieldId.Value && c.Value == cellData.CellValue);

                                    if (refCategoryRow != null)
                                    {
                                        var refValue = refCategoryRow.RowData?.CategoryRowValues?.FirstOrDefault(f => f.CategoryFieldId == cellData.FieldConfig.ReferenceCategoryFieldId)?.Value;

                                        rowInput.CategoryRowValues.Add(new CategoryValueInputModel()
                                        {
                                            CategoryFieldId = cellData.FieldMapping.FieldId.Value,
                                            Value = refValue
                                        });
                                    }
                                }
                                else
                                {
                                    rowInput.CategoryRowValues.Add(new CategoryValueInputModel()
                                    {
                                        CategoryFieldId = cellData.FieldMapping.FieldId.Value,
                                        Value = cellData.CellValue
                                    });
                                }
                            }

                            if (categoryFields.Any(f => f.CategoryFieldId == cellData.FieldMapping.RefCategoryFieldId))
                            {
                                parentKey = cellData.CellValue;
                                parentRefFieldId = cellData.FieldMapping.RefCategoryFieldId;
                            }
                        }

                        key = rowData.FirstOrDefault(c => c.FieldMapping.FieldId == parentRefFieldId)?.CellValue;


                        var categoryRow = await InsertCategoryRowAsync(categoryId, categoryFields, rowInput);

                        categoryRows.Add((categoryRow, key, parentKey));

                    }

                    // Insert relationship
                    if (category.IsTreeView)
                    {
                        foreach (var categoryRow in categoryRows)
                        {
                            if (!string.IsNullOrEmpty(categoryRow.ParentKey))
                            {
                                int parentId = categoryRows.Where(r => r.Key == categoryRow.ParentKey).Select(r => r.Data.CategoryRowId).FirstOrDefault();
                                categoryRow.Data.ParentCategoryRowId = parentId;
                            }
                        }
                        _accountingContext.SaveChanges();
                    }
                    trans.Commit();

                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    _logger.LogError(ex, "Import");
                    throw ex;
                }
            }

            return true;

        }


        private async Task<List<CategoryQueryData>> GetRefCategoryData(Dictionary<int, HashSet<string>> refCategoryFieldDatas)
        {
            var categoryFieldInfos = (await (
                    from f in _accountingContext.CategoryField
                    join c in _accountingContext.Category on f.CategoryId equals c.CategoryId
                    where refCategoryFieldDatas.Keys.Contains(f.CategoryFieldId)
                    select new
                    {
                        f.CategoryFieldId,
                        CategoryInfo = c
                    }
                    )
                    .AsNoTracking()
                    .ToListAsync()
                )
                .ToDictionary(f => f.CategoryFieldId, f => f);

            var categoryQueryData = new List<CategoryQueryData>();

            var categoryConfigs = new Dictionary<int, CategoryEntity>();

            foreach (var fieldData in refCategoryFieldDatas)
            {
                if (!categoryFieldInfos.TryGetValue(fieldData.Key, out var fieldInfo))
                {
                    throw new BadRequestException(GeneralCode.InvalidParams, "Không tìm thấy trường dữ liệu " + fieldData.Key);
                }

                categoryQueryData.AddRange(fieldData.Value.Select(v => new CategoryQueryData()
                {
                    CategoryId = fieldInfo.CategoryInfo.CategoryId,
                    CategoryFieldId = fieldData.Key,
                    Value = v,
                    RowData = null

                }));


                if (!categoryConfigs.ContainsKey(fieldInfo.CategoryInfo.CategoryId))
                {
                    categoryConfigs.Add(fieldInfo.CategoryInfo.CategoryId, fieldInfo.CategoryInfo);

                }

            }


            foreach (var refCategoryId in categoryQueryData.Select(c => c.CategoryId).Distinct().ToList())
            {

                categoryConfigs.TryGetValue(refCategoryId, out var categoryConfig);
                IQueryable<CategoryRow> query;

                if (categoryConfig.IsOutSideData)
                {
                    query = GetOutSideCategoryRows(refCategoryId);
                }
                else
                {
                    query = _accountingContext.CategoryRow
                        .Where(r => r.CategoryId == refCategoryId)
                        .Include(r => r.CategoryRowValue);
                }

                var lst = query.ProjectTo<CategoryRowListOutputModel>(_mapper.ConfigurationProvider).ToList();

                foreach (var queryData in categoryQueryData.Where(c => c.CategoryId == refCategoryId))
                {
                    var rowData = lst.FirstOrDefault(r => r.CategoryRowValues.Any(v => v.CategoryFieldId == queryData.CategoryFieldId && v.Value?.Trim()?.Equals(queryData.Value, StringComparison.OrdinalIgnoreCase) == true));
                    queryData.RowData = rowData;
                }
            }



            return categoryQueryData;
        }

        public async Task<ServiceResult<MemoryStream>> ExportCategory(int categoryId)
        {
            var category = _accountingContext.Category.FirstOrDefault(c => c.CategoryId == categoryId);
            if (category == null)
            {
                return CategoryErrorCode.CategoryNotFound;
            }
            if (category.IsReadonly)
            {
                return CategoryErrorCode.CategoryReadOnly;
            }
            if (category.IsOutSideData)
            {
                return CategoryErrorCode.CategoryIsOutSideData;
            }
            // Lấy thông tin field
            var categoryFields = _accountingContext.CategoryField
                .Where(f => categoryId == f.CategoryId)
                .Where(f => !f.IsHidden && !f.AutoIncrement)
                .Where(f => f.CategoryFieldName != AccountantConstants.F_IDENTITY)
                .AsEnumerable();
            // Lấy thông tin row
            var categoryRows = _accountingContext.CategoryRow
                .Where(r => r.CategoryId == categoryId)
                .Include(r => r.CategoryRowValue);

            List<(string, byte[])[]> dataInRows = new List<(string, byte[])[]>();
            List<(string, byte[])> dataInRow = new List<(string, byte[])>();
            byte[] titleRgb = new byte[3] { 60, 120, 216 };
            foreach (var field in categoryFields)
            {
                dataInRow.Add((field.Title, titleRgb));
            }
            dataInRows.Add(dataInRow.ToArray());
            foreach (var row in categoryRows)
            {
                dataInRow.Clear();
                foreach (var field in categoryFields)
                {
                    bool isRef = AccountantConstants.SELECT_FORM_TYPES.Contains((EnumFormType)field.FormTypeId);
                    var categoryValueRow = row.CategoryRowValue.FirstOrDefault(rv => rv.CategoryFieldId == field.CategoryFieldId);
                    string value = string.Empty;
                    if (categoryValueRow != null)
                    {
                        value = categoryValueRow?.Value ?? string.Empty;
                    }
                    value = value.ConvertValueToData((EnumDataType)field.DataTypeId);
                    dataInRow.Add((value, null));
                }
                dataInRows.Add(dataInRow.ToArray());
            }

            var writer = new ExcelWriter();
            writer.WriteToSheet(dataInRows, "Data");
            MemoryStream stream = await writer.WriteToStream();
            return stream;
        }
    }
}
