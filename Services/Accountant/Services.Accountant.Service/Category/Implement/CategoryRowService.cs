using AutoMapper;
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
                    .Include(r => r.CategoryRowValue)
                    .ThenInclude(rv => rv.CategoryField)
                    .Include(r => r.CategoryRowValue)
                    .ThenInclude(rv => rv.ReferenceCategoryRowValue);
            }

            if (filters != null && filters.Length > 0)
            {
                FillterProcess(ref query, filters);
            }
            // search
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(r => r.CategoryRowValue
                .Any(rv => (rv.CategoryField.FormTypeId == (int)EnumFormType.SearchTable || rv.CategoryField.FormTypeId == (int)EnumFormType.Select) && rv.ReferenceCategoryRowValueId.HasValue
                ? rv.ReferenceCategoryRowValue.Value.Contains(keyword)
                : rv.Value.Contains(keyword)));
            }
            total = query.Count();
            if (size > 0)
            {
                if (category.IsTreeView)
                {
                    lst = query.ProjectTo<CategoryRowListOutputModel>(_mapper.ConfigurationProvider).ToList();
                    int[] parentIds = GetParentIds(lst);


                    lst.AddRange(_accountingContext.CategoryRow
                        .Include(r => r.CategoryRowValue)
                        .ThenInclude(rv => rv.CategoryField)
                        .Include(r => r.CategoryRowValue)
                        .ThenInclude(rv => rv.ReferenceCategoryRowValue)
                        .ProjectTo<CategoryRowListOutputModel>(_mapper.ConfigurationProvider)
                        .Where(r => parentIds.Contains(r.CategoryRowId)));

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
            categoryRows.RemoveAll(r => !r.ParentCategoryRowId.HasValue || !categoryRows.Any(p => p.CategoryRowId == r.ParentCategoryRowId));

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
                var r = GetOutSideCategoryRow(categoryId, categoryRowId, ref categoryRow);
                if (!r.IsSuccess())
                {
                    return r;
                }
            }
            else
            {
                categoryRow = await _accountingContext.CategoryRow
                    .Where(r => r.CategoryId == categoryId && r.CategoryRowId == categoryRowId)
                    .Include(r => r.ParentCategoryRow)
                    .ThenInclude(pr => pr.CategoryRowValue)
                    .ThenInclude(rv => rv.CategoryField)
                    .Include(r => r.ParentCategoryRow)
                    .ThenInclude(pr => pr.CategoryRowValue)
                    .ThenInclude(rv => rv.ReferenceCategoryRowValue)
                    .ProjectTo<CategoryRowOutputModel>(_mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync();

            }
            // Check row 
            if (categoryRow == null)
            {
                return CategoryErrorCode.CategoryRowNotFound;
            }

            return categoryRow;
        }

        private Enum GetOutSideCategoryRow(int categoryId, int categoryRowId, ref CategoryRowOutputModel categoryRow)
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
                        categoryRow = new CategoryRowOutputModel
                        {
                            CategoryRowId = categoryRowId
                        };

                        // Map value cho các field
                        foreach (var field in fields)
                        {
                            var value = new CategoryValueModel
                            {
                                CategoryFieldId = field.CategoryFieldId,
                                Value = properties[field.CategoryFieldName]
                            };
                            categoryRow.CategoryRowValues.Add(value);
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

                if (isRef && valueItem.CategoryValueId > 0)
                {
                    categoryRowValue.ReferenceCategoryRowValueId = valueItem.CategoryValueId;
                }
                else if (isRef)
                {
                    // Thêm value
                    categoryRowValue.Value = valueItem.Value;
                    categoryRowValue.ValueInNumber = valueItem.Value.ConvertValueToNumber((EnumDataType)field.DataTypeId);
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
                .Include(rv => rv.ReferenceCategoryRowValue)
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
                        if (isRef ? currentValue.ReferenceCategoryRowValue.Value != updateValue.Value : currentValue.Value != updateValue.Value)
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
                            if (isRef && valueItem.CategoryValueId > 0)
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
                        else //nếu có giá trị cũ
                        {
                            var currentRowValue = _accountingContext.CategoryRowValue.First(rv => rv.CategoryRowId == categoryRowId && rv.CategoryFieldId == field.CategoryFieldId);
                            if (isRef && valueItem.CategoryValueId > 0) // và là loại ref và là nội bộ phân hệ
                            {
                                // Sửa mapping giá trị mới
                                currentRowValue.Value = null;
                                currentRowValue.ValueInNumber = 0;
                                currentRowValue.ReferenceCategoryRowValueId = valueItem.CategoryValueId;
                                currentRowValue.UpdatedByUserId = updatedUserId;
                            }
                            else // sửa sang value mới
                            {
                                currentRowValue.UpdatedByUserId = updatedUserId;
                                currentRowValue.ReferenceCategoryRowValueId = null;
                                string value = string.Empty;
                                currentRowValue.Value = valueItem.Value;
                                currentRowValue.ValueInNumber = valueItem.Value.ConvertValueToNumber((EnumDataType)field.DataTypeId);
                            }
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
                    bool isOutSide = false;
                    bool isExisted = false;
                    int referValueId = 0;
                    if (field.ReferenceCategoryFieldId.HasValue)
                    {
                        CategoryField referField = _accountingContext.CategoryField.First(f => f.CategoryFieldId == field.ReferenceCategoryFieldId.Value);
                        CategoryEntity referCategory = GetReferenceCategory(referField);
                        isOutSide = referCategory.IsOutSideData;
                        bool isRef = ((EnumFormType)referField.FormTypeId).IsRef() && !isOutSide;
                        IQueryable<CategoryRow> query;
                        if (isOutSide)
                        {
                            query = GetOutSideCategoryRows(referCategory.CategoryId);
                        }
                        else
                        {
                            query = _accountingContext.CategoryRow
                               .Where(r => r.CategoryId == referCategory.CategoryId)
                               .Include(r => r.CategoryRowValue)
                               .ThenInclude(rv => rv.ReferenceCategoryRowValue)
                               .Include(r => r.CategoryRowValue)
                               .ThenInclude(rv => rv.CategoryField);
                        }

                        if (!string.IsNullOrEmpty(field.Filters))
                        {
                            FilterModel[] filters = JsonConvert.DeserializeObject<FilterModel[]>(field.Filters);
                            FillterProcess(ref query, filters);
                        }

                        isExisted = query.Any(r => r.CategoryRowValue.Any(
                                rv => rv.CategoryFieldId == field.ReferenceCategoryFieldId.Value
                                && (isRef ? rv.ReferenceCategoryRowValue.Value == valueItem.Value : rv.Value == valueItem.Value)));

                        if (isExisted && !isOutSide)
                        {
                            referValueId = query
                            .Where(r => r.CategoryRowValue.Any(
                                rv => rv.CategoryFieldId == field.ReferenceCategoryFieldId.Value
                                && (isRef ? rv.ReferenceCategoryRowValue.Value == valueItem.Value : rv.Value == valueItem.Value)
                            ))
                            .First()
                            .CategoryRowValue
                            .Where(rv => rv.CategoryFieldId == field.ReferenceCategoryFieldId.Value)
                            .First()
                            .CategoryRowValueId;
                        }

                    }
                    if (!isExisted)
                    {
                        return CategoryErrorCode.ReferValueNotFound;
                    }
                    valueItem.CategoryValueId = isOutSide ? 0 : referValueId;
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
                .Include(r => r.CategoryRowValue)
                .ThenInclude(rv => rv.ReferenceCategoryRowValue);

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
                    var categoryValueRow = row.CategoryRowValue.FirstOrDefault(rv => rv.CategoryFieldId == field.CategoryFieldId);
                    string value = string.Empty;
                    if (categoryValueRow != null)
                    {
                        value = isRef && categoryValueRow.ReferenceCategoryRowValueId.HasValue ? categoryValueRow.ReferenceCategoryRowValue?.Value ?? string.Empty : categoryValueRow?.Value ?? string.Empty;
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
