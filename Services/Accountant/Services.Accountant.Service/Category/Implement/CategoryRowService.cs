using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VErp.Commons.Constants;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.AccountingDB;
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

        public async Task<PageData<CategoryRowListOutputModel>> GetCategoryRows(int categoryId, string keyword, FilterModel[] filters, int page, int size)
        {
            var total = 0;
            List<(CategoryRow Data, int Level)> categoryRows = new List<(CategoryRow Data, int Level)>();
            List<CategoryRowListOutputModel> lst = new List<CategoryRowListOutputModel>();
            IQueryable<CategoryRow> query;
            var category = _accountingContext.Category.FirstOrDefault(c => c.CategoryId == categoryId);
            if (category.IsOutSideData)
            {
                query = GetOutSideCategoryRows(categoryId);
            }
            else
            {
                query = _accountingContext.CategoryRow
                    .Where(r => r.CategoryId == categoryId)
                    .Include(r => r.CategoryRowValues)
                    .ThenInclude(rv => rv.CategoryField)
                    .Include(r => r.CategoryRowValues)
                    .ThenInclude(rv => rv.SourceCategoryRowValue);
            }

            if (filters != null && filters.Length > 0)
            {
                FillterProcess(ref query, filters);
            }
            // search
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(r => r.CategoryRowValues
                .Any(rv => rv.CategoryField.FormTypeId == (int)EnumFormType.SearchTable || rv.CategoryField.FormTypeId == (int)EnumFormType.Select
                ? rv.SourceCategoryRowValue.Value.Contains(keyword)
                : rv.Value.Contains(keyword)));
            }
            total = query.Count();
            if (size > 0)
            {
                if (category.IsTreeView)
                {
                    var temp = query.ToList();
                    int[] parentIds = GetParentIds(temp);
                    temp.AddRange(_accountingContext.CategoryRow
                        .Include(r => r.CategoryRowValues)
                        .ThenInclude(rv => rv.CategoryField)
                        .Include(r => r.CategoryRowValues)
                        .ThenInclude(rv => rv.SourceCategoryRowValue)
                        .Where(r => parentIds.Contains(r.CategoryRowId)));
                    categoryRows = SortCategoryRows(temp).Skip((page - 1) * size).Take(size).ToList();
                }
                else
                {
                    foreach (var item in query.OrderBy(r => r.CategoryRowId).Skip((page - 1) * size).Take(size))
                    {
                        categoryRows.Add((item, 0));
                    }
                }
            }
            foreach (var (Data, Level) in categoryRows)
            {
                CategoryRowListOutputModel output = _mapper.Map<CategoryRowListOutputModel>(Data);
                output.CategoryRowLevel = Level;
                ICollection<CategoryValueModel> row = new List<CategoryValueModel>();
                foreach (var cell in Data.CategoryRowValues)
                {
                    row.Add(new CategoryValueModel
                    {
                        CategoryFieldId = cell.CategoryFieldId,
                        CategoryValueId = cell.CategoryRowValueId,
                        Value = ((EnumFormType)cell.CategoryField.FormTypeId).IsRef() ? cell.SourceCategoryRowValue.Value : cell.Value
                    });
                }
                output.CategoryRowValues = row;
                lst.Add(output);
            }
            return (lst, total);
        }

        private int[] GetParentIds(List<CategoryRow> categoryRows)
        {
            List<int> result = new List<int>();

            int[] parentIds = categoryRows
                .Where(r => r.ParentCategoryRowId.HasValue && !categoryRows.Any(p => p.CategoryRowId == r.ParentCategoryRowId))
                .Select(r => r.ParentCategoryRowId.Value)
                .Distinct()
                .ToArray();
            result.AddRange(parentIds);

            while(parentIds.Length > 0)
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

        private List<(CategoryRow Data, int Level)> SortCategoryRows(List<CategoryRow> categoryRows)
        {
            int level = 0;
            categoryRows = categoryRows.OrderBy(r => r.CategoryRowId).ToList();
            List<(CategoryRow Data, int Level)> nodes = new List<(CategoryRow Data, int Level)>();

            var items = categoryRows.Where(r => !r.ParentCategoryRowId.HasValue || !categoryRows.Any(p => p.CategoryRowId == r.ParentCategoryRowId)).ToList();
            categoryRows.RemoveAll(r => !r.ParentCategoryRowId.HasValue || !categoryRows.Any(p => p.CategoryRowId == r.ParentCategoryRowId));
            foreach (var item in items)
            {
                nodes.Add((item, level));
                nodes.AddRange(GetChilds(ref categoryRows, item.CategoryRowId, level));
            }

            return nodes;
        }

         


        private IEnumerable<(CategoryRow Data, int Level)> GetChilds(ref List<CategoryRow> categoryRows, int categoryRowId, int level)
        {
            level++;
            List<(CategoryRow Data, int Level)> nodes = new List<(CategoryRow Data, int Level)>();
            var items = categoryRows.Where(r => r.ParentCategoryRowId == categoryRowId).ToList();
            categoryRows.RemoveAll(r => r.ParentCategoryRowId == categoryRowId);
            foreach (var item in items)
            {
                nodes.Add((item, level));
                nodes.AddRange(GetChilds(ref categoryRows, item.CategoryRowId, level));
            }
            return nodes;
        }

        public async Task<ServiceResult<CategoryRowOutputModel>> GetCategoryRow(int categoryId, int categoryRowId)
        {
            var category = _accountingContext.Category.FirstOrDefault(c => c.CategoryId == categoryId);
            CategoryRow categoryRow = null;
            List<CategoryRowValue> values = new List<CategoryRowValue>();
            if (category.IsOutSideData)
            {
                var r = GetOutSideCategoryRow(categoryId, categoryRowId, ref categoryRow, ref values);
                if (!r.IsSuccess())
                {
                    return r;
                }
            }
            else
            {
                categoryRow = await _accountingContext.CategoryRow
                  .Include(r => r.ParentCategoryRow)
                  .ThenInclude(pr => pr.CategoryRowValues)
                  .FirstOrDefaultAsync(r => r.CategoryId == categoryId && r.CategoryRowId == categoryRowId);
                values = _accountingContext.CategoryRowValue
                  .Where(r => r.CategoryRowId == categoryRowId)
                  .Include(rv => rv.CategoryField)
                  .Include(rv => rv.SourceCategoryRowValue).ToList();
            }
            // Check row 
            if (categoryRow == null)
            {
                return CategoryErrorCode.CategoryRowNotFound;
            }
            CategoryRowOutputModel output = _mapper.Map<CategoryRowOutputModel>(categoryRow);
            ICollection<CategoryValueModel> row = new List<CategoryValueModel>();
            foreach (var value in values)
            {
                row.Add(new CategoryValueModel
                {
                    CategoryFieldId = value.CategoryFieldId,
                    CategoryValueId = value.CategoryRowValueId,
                    Value = ((EnumFormType)value.CategoryField.FormTypeId).IsRef() ? value.SourceCategoryRowValue.Value : value.Value
                });
            }
            output.CategoryRowValues = row;

            return output;
        }

        private Enum GetOutSideCategoryRow(int categoryId, int categoryRowId, ref CategoryRow categoryRow, ref List<CategoryRowValue> values)
        {
            try
            {
                var config = _accountingContext.OutSideDataConfig.FirstOrDefault(cf => cf.CategoryId == categoryId);
                if (!string.IsNullOrEmpty(config?.Url))
                {
                    string url = $"{config.Url}/{categoryRowId}";
                    (JObject, HttpStatusCode) result = GetFromAPI<JObject>(url, 100000);
                    if (result.Item2 == HttpStatusCode.OK)
                    {
                        int[] categoryIds = GetAllCategoryIds(categoryId);
                        List<CategoryField> fields = _accountingContext.CategoryField.Where(f => categoryIds.Contains(f.CategoryId)).ToList();

                        // Lấy thông tin row
                        Dictionary<string, string> properties = new Dictionary<string, string>();
                        foreach (var jprop in result.Item1.Properties())
                        {
                            var key = jprop.Name;
                            var value = jprop.Value.ToString();
                            properties.Add(key, value);
                        }

                        // Map row
                        categoryRow = new CategoryRow
                        {
                            CategoryRowId = categoryRowId,
                            CategoryId = categoryId,
                        };

                        // Map value cho các field
                        foreach (var field in fields)
                        {
                            var value = new CategoryRowValue
                            {
                                CategoryFieldId = field.CategoryFieldId,
                                Value = properties[field.CategoryFieldName],
                                CategoryField = field
                            };
                            values.Add(value);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return CategoryErrorCode.CategoryIsOutSideDataError;
            }

            return GeneralCode.Success;
        }

        public async Task<ServiceResult<int>> AddCategoryRow(int updatedUserId, int categoryId, CategoryRowInputModel data)
        {
            // Validate
            var category = _accountingContext.Category.FirstOrDefault(c => c.CategoryId == categoryId);
            if (category == null)
            {
                return CategoryErrorCode.CategoryNotFound;
            }
            if (category.IsReadonly)
            {
                return CategoryErrorCode.CategoryReadOnly;
            }
            if (!category.IsModule)
            {
                return CategoryErrorCode.CategoryIsNotModule;
            }
            if (category.IsOutSideData)
            {
                return CategoryErrorCode.CategoryIsOutSideData;
            }
            // Check parent row
            var r = CheckParentRow(data, category);
            if (!r.IsSuccess()) return r;

            // Lấy thông tin field
            var categoryIds = GetAllCategoryIds(categoryId);
            var categoryFields = _accountingContext.CategoryField.Include(f => f.DataType).Where(f => categoryIds.Contains(f.CategoryId)).AsEnumerable();
            var requiredFields = categoryFields.Where(f => !f.AutoIncrement && f.IsRequired);
            var uniqueFields = categoryFields.Where(f => !f.AutoIncrement && f.IsUnique);
            var selectFields = categoryFields.Where(f => !f.AutoIncrement && (f.FormTypeId == (int)EnumFormType.SearchTable || f.FormTypeId == (int)EnumFormType.Select));

            // Check field required
            r = CheckRequired(data, requiredFields);
            if (!r.IsSuccess()) return r;

            // Check unique
            r = CheckUnique(data, uniqueFields);
            if (!r.IsSuccess()) return r;

            // Check refer
            r = CheckRefer(ref data, selectFields);
            if (!r.IsSuccess()) return r;

            // Check value
            r = CheckValue(data, categoryFields);
            if (!r.IsSuccess()) return r;

            using (var trans = await _accountingContext.Database.BeginTransactionAsync())
            {
                try
                {
                    int categoryRowId = await InsertCategoryRowAsync(updatedUserId, categoryId, categoryFields, data);
                    trans.Commit();
                    await _activityLogService.CreateLog(EnumObjectType.Category, categoryRowId, $"Thêm dòng cho danh mục {category.Title}", data.JsonSerialize());
                    return categoryRowId;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    _logger.LogError(ex, "Create");
                    return GeneralCode.InternalError;
                }
            }
        }

        private async Task<int> InsertCategoryRowAsync(int updatedUserId, int categoryId, IEnumerable<CategoryField> categoryFields, CategoryRowInputModel data)
        {
            // Thêm dòng
            var categoryRow = new CategoryRow
            {
                CategoryId = categoryId,
                UpdatedByUserId = updatedUserId,
                CreatedByUserId = updatedUserId,
                ParentCategoryRowId = data.ParentCategoryRowId
            };
            await _accountingContext.CategoryRow.AddAsync(categoryRow);
            await _accountingContext.SaveChangesAsync();

            // Duyệt danh sách field
            foreach (var field in categoryFields)
            {
                bool isRef = ((EnumFormType)field.FormTypeId).IsRef();
                var valueItem = data.CategoryRowValues.FirstOrDefault(v => v.CategoryFieldId == field.CategoryFieldId);
                if ((valueItem == null || string.IsNullOrEmpty(valueItem.Value)) && !field.AutoIncrement)
                {
                    continue;
                }
                CategoryRowValue categoryRowValue = new CategoryRowValue
                {
                    CategoryRowId = categoryRow.CategoryRowId,
                    CategoryFieldId = field.CategoryFieldId,
                    UpdatedByUserId = updatedUserId,
                    CreatedByUserId = updatedUserId,
                    ValueInNumber = 0
                };

                if (isRef)
                {
                    categoryRowValue.ReferenceCategoryRowValueId = valueItem.CategoryValueId;
                }
                else
                {
                    string value = string.Empty;
                    long valueInNumber = 0;
                    if (field.AutoIncrement)
                    {
                        // Lấy ra value lớn nhất
                        long max = _accountingContext.CategoryRowValue.Where(v => v.CategoryFieldId == field.CategoryFieldId).Max(v => v.ValueInNumber);
                        valueInNumber = (max / Numbers.CONVERT_VALUE_TO_NUMBER_FACTOR) + 1;
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
                }

                await _accountingContext.CategoryRowValue.AddAsync(categoryRowValue);
                await _accountingContext.SaveChangesAsync();
            }
            return categoryRow.CategoryRowId;
        }

        public async Task<Enum> UpdateCategoryRow(int updatedUserId, int categoryId, int categoryRowId, CategoryRowInputModel data)
        {
            var categoryRow = await _accountingContext.CategoryRow.FirstOrDefaultAsync(c => c.CategoryRowId == categoryRowId && c.CategoryId == categoryId);
            if (categoryRow == null)
            {
                return CategoryErrorCode.CategoryRowNotFound;
            }

            Enum r;
            // Check parent row
            if (categoryRow.ParentCategoryRowId != data.ParentCategoryRowId)
            {
                var category = _accountingContext.Category.First(c => c.CategoryId == categoryId);
                r = CheckParentRow(data, category, categoryRowId);
                if (!r.IsSuccess()) return r;
            }

            // Lấy thông tin field
            var categoryIds = GetAllCategoryIds(categoryRow.CategoryId);
            var categoryFields = _accountingContext.CategoryField.Include(f => f.DataType).Where(f => categoryIds.Contains(f.CategoryId)).AsEnumerable();

            // Lấy thông tin value hiện tại
            var currentValues = _accountingContext.CategoryRowValue
                .Where(rv => rv.CategoryRowId == categoryRowId)
                .Include(rv => rv.CategoryField)
                .Include(rv => rv.SourceCategoryRowValue)
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
                        bool isRef = ((EnumFormType)categoryField.FormTypeId).IsRef();
                        if (isRef ? currentValue.SourceCategoryRowValue.Value != updateValue.Value : currentValue.Value != updateValue.Value)
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
            r = CheckRequired(data, requiredFields);
            if (!r.IsSuccess()) return r;

            // Check unique
            r = CheckUnique(data, uniqueFields, categoryRowId);
            if (!r.IsSuccess()) return r;

            // Check refer
            r = CheckRefer(ref data, selectFields);
            if (!r.IsSuccess()) return r;

            // Check value
            r = CheckValue(data, categoryFields);
            if (!r.IsSuccess()) return r;

            using (var trans = await _accountingContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // Update parent id
                    categoryRow.ParentCategoryRowId = data.ParentCategoryRowId;

                    // Duyệt danh sách field
                    foreach (var field in updateFields)
                    {
                        bool isRef = ((EnumFormType)field.FormTypeId).IsRef();
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
                                currentRowValue.UpdatedByUserId = updatedUserId;
                            }
                        }
                        else if (oldValue == null) // Nếu giá trị cũ là null, tạo mới, map lại
                        {
                            CategoryRowValue categoryRowValue = new CategoryRowValue
                            {
                                CategoryFieldId = field.CategoryFieldId,
                                CategoryRowId = categoryRowId,
                                UpdatedByUserId = updatedUserId,
                                CreatedByUserId = updatedUserId,
                                ValueInNumber = 0
                            };
                            if (isRef)
                            {
                                categoryRowValue.ReferenceCategoryRowValueId = valueItem.CategoryValueId;
                            }
                            else
                            {
                                // Thêm value
                                categoryRowValue.Value = valueItem.Value;
                                categoryRowValue.ValueInNumber = valueItem.Value.ConvertValueToNumber((EnumDataType)field.DataTypeId);
                            }
                            _accountingContext.CategoryRowValue.Add(categoryRowValue);
                        }
                        else if (isRef)
                        {
                            // Sửa mapping giá trị mới
                            var currentRowValue = _accountingContext.CategoryRowValue.First(rv => rv.CategoryRowId == categoryRowId && rv.CategoryFieldId == field.CategoryFieldId);
                            currentRowValue.Value = null;
                            currentRowValue.ValueInNumber = 0;
                            currentRowValue.ReferenceCategoryRowValueId = valueItem.CategoryValueId;
                            currentRowValue.UpdatedByUserId = updatedUserId;
                        }
                        else
                        {
                            // Sửa value cũ, 
                            var currentRowValue = _accountingContext.CategoryRowValue.FirstOrDefault(rv => rv.CategoryRowId == categoryRowId && rv.CategoryFieldId == field.CategoryFieldId);
                            currentRowValue.UpdatedByUserId = updatedUserId;
                            currentRowValue.ReferenceCategoryRowValueId = null;
                            string value = string.Empty;
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
        }

        public async Task<Enum> DeleteCategoryRow(int updatedUserId, int categoryId, int categoryRowId)
        {
            var categoryRow = await _accountingContext.CategoryRow.FirstOrDefaultAsync(c => c.CategoryRowId == categoryRowId && c.CategoryId == categoryId);
            if (categoryRow == null)
            {
                return CategoryErrorCode.CategoryRowNotFound;
            }
            // Lấy thông tin field
            var categoryIds = GetAllCategoryIds(categoryRow.CategoryId);
            var categoryFields = _accountingContext.CategoryField.Where(f => categoryIds.Contains(f.CategoryId)).AsEnumerable();

            // Check reference
            foreach (var field in categoryFields)
            {
                if (_accountingContext.CategoryField.Any(c => c.ReferenceCategoryFieldId == field.CategoryFieldId))
                {
                    int valueId = _accountingContext.CategoryRowValue
                        .Where(rv => rv.CategoryFieldId == field.CategoryFieldId && rv.CategoryRowId == categoryRowId)
                        .Select(rv => rv.CategoryRowValueId).FirstOrDefault();
                    bool isRefer = valueId > 0 && _accountingContext.CategoryRowValue.Any(rv => rv.ReferenceCategoryRowValueId == valueId && rv.CategoryRowId != categoryRowId);
                    if (isRefer) return CategoryErrorCode.DestCategoryFieldAlreadyExisted;
                }
            }

            using var trans = await _accountingContext.Database.BeginTransactionAsync();
            try
            {
                // Delete row
                categoryRow.IsDeleted = true;
                categoryRow.UpdatedByUserId = updatedUserId;
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
                    categoryRowValue.UpdatedByUserId = updatedUserId;
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

        private Enum CheckParentRow(CategoryRowInputModel data, CategoryEntity category, int? categoryRowId = null)
        {
            if (categoryRowId.HasValue && data.ParentCategoryRowId == categoryRowId)
            {
                return CategoryErrorCode.ParentCategoryFromItSelf;
            }
            if (category.IsTreeView && data.ParentCategoryRowId.HasValue)
            {
                bool isExist = _accountingContext.CategoryRow.Any(r => r.CategoryId == category.CategoryId && r.CategoryRowId == data.ParentCategoryRowId.Value);
                if (!isExist)
                {
                    return CategoryErrorCode.ParentCategoryRowNotExisted;
                }
            }
            return GeneralCode.Success;
        }

        private Enum CheckRefer(ref CategoryRowInputModel data, IEnumerable<CategoryField> selectFields)
        {
            // Check refer
            foreach (var field in selectFields)
            {
                var valueItem = data.CategoryRowValues.FirstOrDefault(v => v.CategoryFieldId == field.CategoryFieldId);
                if (valueItem != null && !string.IsNullOrEmpty(valueItem.Value))
                {
                    int referValueId = 0;
                    if (field.ReferenceCategoryFieldId.HasValue)
                    {
                        CategoryField referField = _accountingContext.CategoryField.First(f => f.CategoryFieldId == field.ReferenceCategoryFieldId.Value);
                        bool isRef = ((EnumFormType)referField.FormTypeId).IsRef();
                        CategoryEntity referCategory = GetReferenceCategory(referField);
                        IQueryable<CategoryRow> query = _accountingContext.CategoryRow
                            .Where(r => r.CategoryId == referCategory.CategoryId)
                            .Include(r => r.CategoryRowValues)
                            .ThenInclude(rv => rv.SourceCategoryRowValue)
                            .Include(r => r.CategoryRowValues)
                            .ThenInclude(rv => rv.CategoryField);

                        if (!string.IsNullOrEmpty(field.Filters))
                        {
                            FilterModel[] filters = JsonConvert.DeserializeObject<FilterModel[]>(field.Filters);
                            FillterProcess(ref query, filters);
                        }

                        referValueId = query
                            .Where(r => r.CategoryRowValues.Any(
                                rv => rv.CategoryFieldId == field.ReferenceCategoryFieldId.Value
                                && (isRef ? rv.SourceCategoryRowValue.Value == valueItem.Value : rv.Value == valueItem.Value)
                            ))
                            .FirstOrDefault()?
                            .CategoryRowValues
                            .Where(rv => rv.CategoryFieldId == field.ReferenceCategoryFieldId.Value)
                            .FirstOrDefault()?
                            .CategoryRowValueId ?? 0;
                    }
                    if (referValueId <= 0)
                    {
                        return CategoryErrorCode.ReferValueNotFound;
                    }
                    valueItem.CategoryValueId = referValueId;
                }
            }
            return GeneralCode.Success;
        }

        private Enum CheckUnique(CategoryRowInputModel data, IEnumerable<CategoryField> uniqueFields, int? categoryRowId = null)
        {
            // Check unique
            foreach (var item in data.CategoryRowValues.Where(v => uniqueFields.Any(f => f.CategoryFieldId == v.CategoryFieldId)))
            {
                bool isExisted = _accountingContext.CategoryRowValue
                    .Any(rv => (categoryRowId.HasValue ? rv.CategoryRowId != categoryRowId : true) && rv.CategoryFieldId == item.CategoryFieldId && rv.Value == item.Value);
                if (isExisted)
                {
                    return CategoryErrorCode.UniqueValueAlreadyExisted;
                }
            }
            return GeneralCode.Success;
        }

        private Enum CheckRequired(CategoryRowInputModel data, IEnumerable<CategoryField> requiredFields)
        {
            if (requiredFields.Count() > 0 && requiredFields.Any(rf => !data.CategoryRowValues.Any(v => v.CategoryFieldId == rf.CategoryFieldId && !string.IsNullOrWhiteSpace(v.Value))))
            {
                return CategoryErrorCode.RequiredFieldIsEmpty;
            }
            return GeneralCode.Success;
        }

        private Enum CheckValue(CategoryRowInputModel data, IEnumerable<CategoryField> categoryFields)
        {
            foreach (var field in categoryFields)
            {
                var valueItem = data.CategoryRowValues.FirstOrDefault(v => v.CategoryFieldId == field.CategoryFieldId);
                if ((field.FormTypeId == (int)EnumFormType.SearchTable || field.FormTypeId == (int)EnumFormType.Select) || field.AutoIncrement || valueItem == null || string.IsNullOrEmpty(valueItem.Value))
                {
                    continue;
                }
                var r = CheckValue(valueItem, field);
                if (!r.IsSuccess())
                {
                    return r;
                }
            }
            return GeneralCode.Success;
        }

        private Enum CheckValue(CategoryValueModel valueItem, CategoryField field)
        {
            if ((field.DataSize > 0 && valueItem.Value.Length > field.DataSize)
                || (!string.IsNullOrEmpty(field.DataType.RegularExpression) && !Regex.IsMatch(valueItem.Value, field.DataType.RegularExpression))
                || (!string.IsNullOrEmpty(field.RegularExpression) && !Regex.IsMatch(valueItem.Value, field.RegularExpression)))
            {
                return CategoryErrorCode.CategoryValueInValid;
            }
            return GeneralCode.Success;
        }

        public async Task<ServiceResult> ImportCategoryRow(int updatedUserId, int categoryId, Stream stream)
        {
            try
            {
                string errFormat = "Dòng {0} : {1}";
                var category = _accountingContext.Category.FirstOrDefault(c => c.CategoryId == categoryId);
                if (category == null)
                {
                    return CategoryErrorCode.CategoryNotFound;
                }
                if (category.IsReadonly)
                {
                    return CategoryErrorCode.CategoryReadOnly;
                }
                if (!category.IsModule)
                {
                    return CategoryErrorCode.CategoryIsNotModule;
                }
                if (category.IsOutSideData)
                {
                    return CategoryErrorCode.CategoryIsOutSideData;
                }
                var reader = new ExcelReader(stream);

                // Lấy thông tin field
                var categoryIds = GetAllCategoryIds(categoryId);
                var categoryFields = _accountingContext.CategoryField
                    .Include(f => f.DataType)
                    .Where(f => categoryIds.Contains(f.CategoryId))
                    .ToList();

                List<CategoryRowInputModel> rowInputs = new List<CategoryRowInputModel>();

                string[][] data = reader.ReadFile(categoryFields.Where(f => !f.IsHidden && !f.AutoIncrement).Count(), 0, 1, 0);
                string[] fieldNames = data[0];
                for (int rowIndx = 1; rowIndx < data.Length; rowIndx++)
                {
                    string[] row = data[rowIndx];
                    CategoryRowInputModel rowInput = new CategoryRowInputModel();
                    for (int fieldIndx = 0; fieldIndx < fieldNames.Length; fieldIndx++)
                    {
                        string fieldName = fieldNames[fieldIndx];
                        var field = categoryFields.FirstOrDefault(f => f.CategoryFieldName == fieldName);
                        if (field == null) continue;

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
                                return (CategoryErrorCode.CategoryValueInValid, string.Format(errFormat, rowIndx + 1, CategoryErrorCode.CategoryValueInValid.GetEnumDescription()));
                            }
                        }
                        else if (field.DataTypeId == (int)EnumDataType.Date)
                        {
                            if (DateTime.TryParse(row[fieldIndx], out DateTime value))
                            {
                                row[fieldIndx] = value.GetUnix().ToString();
                            }
                            else
                            {
                                return (CategoryErrorCode.CategoryValueInValid, string.Format(errFormat, rowIndx + 1, CategoryErrorCode.CategoryValueInValid.GetEnumDescription()));
                            }
                        }
                        rowInput.CategoryRowValues.Add(new CategoryValueModel
                        {
                            CategoryFieldId = field.CategoryFieldId,
                            CategoryValueId = 0,
                            Value = row[fieldIndx]
                        });
                    }

                    var requiredFields = categoryFields.Where(f => !f.AutoIncrement && f.IsRequired);
                    var uniqueFields = categoryFields.Where(f => !f.AutoIncrement && f.IsUnique);
                    var selectFields = categoryFields.Where(f => !f.AutoIncrement && (f.FormTypeId == (int)EnumFormType.SearchTable || f.FormTypeId == (int)EnumFormType.Select));

                    // Check field required
                    var r = CheckRequired(rowInput, requiredFields);
                    if (!r.IsSuccess()) return (r, string.Format(errFormat, rowIndx + 1, r.GetEnumDescription()));

                    // Check unique
                    r = CheckUnique(rowInput, uniqueFields);
                    if (!r.IsSuccess()) return (r, string.Format(errFormat, rowIndx + 1, r.GetEnumDescription()));

                    // Check refer
                    r = CheckRefer(ref rowInput, selectFields);
                    if (!r.IsSuccess()) return (r, string.Format(errFormat, rowIndx + 1, r.GetEnumDescription()));

                    // Check value
                    r = CheckValue(rowInput, categoryFields);
                    if (!r.IsSuccess()) return (r, string.Format(errFormat, rowIndx + 1, r.GetEnumDescription()));

                    rowInputs.Add(rowInput);
                }

                using (var trans = await _accountingContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        foreach (var rowInput in rowInputs)
                        {
                            int categoryRowId = await InsertCategoryRowAsync(updatedUserId, categoryId, categoryFields, rowInput);
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
            var categoryIds = GetAllCategoryIds(categoryId);
            var categoryFields = _accountingContext.CategoryField
                .Where(f => categoryIds.Contains(f.CategoryId))
                .Where(f => !f.IsHidden && !f.AutoIncrement)
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
            dataInRows.Add(titles.ToArray());
            dataInRows.Add(names.ToArray());

            var writer = new ExcelWriter();
            writer.WriteToSheet(dataInRows, "Data");

            MemoryStream stream = await writer.WriteToStream();
            return stream;
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
            var categoryIds = GetAllCategoryIds(categoryId);
            var categoryFields = _accountingContext.CategoryField
                .Where(f => categoryIds.Contains(f.CategoryId))
                .Where(f => !f.IsHidden && !f.AutoIncrement)
                .AsEnumerable();
            // Lấy thông tin row
            var categoryRows = _accountingContext.CategoryRow
                .Where(r => r.CategoryId == categoryId)
                .Include(r => r.CategoryRowValues)
                .ThenInclude(rv => rv.SourceCategoryRowValue);

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
                    bool isRef = ((EnumFormType)field.FormTypeId).IsRef();
                    var categoryValueRow = row.CategoryRowValues.FirstOrDefault(rv => rv.CategoryFieldId == field.CategoryFieldId);
                    string value = isRef ? categoryValueRow?.SourceCategoryRowValue?.Value ?? string.Empty : categoryValueRow?.Value ?? string.Empty;
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
