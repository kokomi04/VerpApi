using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.AccountingDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Accountant.Model.Category;

namespace VErp.Services.Accountant.Service.Category.Implement
{
    public class CategoryRowService : CategoryBaseService, ICategoryRowService
    {
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly IMapper _mapper;
        public CategoryRowService(AccountingDBContext accountingContext
            , IOptions<AppSetting> appSetting
            , ILogger<CategoryRowService> logger
            , IActivityLogService activityLogService
             , IMapper mapper
            ) : base(accountingContext)
        {
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
            _mapper = mapper;
        }

        public async Task<PageData<CategoryRowOutputModel>> GetCategoryRows(int categoryId, int page, int size)
        {
            var query = _accountingContext.CategoryRow
                           .Where(r => r.CategoryId == categoryId)
                           .Join(_accountingContext.CategoryRowValue, r => r.CategoryRowId, rv => rv.CategoryRowId, (r, rv) => new
                           {
                               r.CategoryRowId,
                               rv.CategoryValueId,
                               rv.CategoryFieldId
                           })
                           .Join(_accountingContext.CategoryValue, rv => rv.CategoryValueId, v => v.CategoryValueId, (rv, v) => new
                           {
                               rv.CategoryRowId,
                               v.Value,
                               v.CategoryValueId,
                               rv.CategoryFieldId
                           });

            var rowIds = query.GroupBy(rvf => rvf.CategoryRowId).Select(g => g.Key);
            var total = await rowIds.CountAsync();
            if (size > 0)
            {
                rowIds = rowIds.Skip((page - 1) * size).Take(size);
            }

            var data = query.Where(rvf => rowIds.Contains(rvf.CategoryRowId))
                .AsEnumerable()
                .GroupBy(rvf => rvf.CategoryRowId);

            List<CategoryRowOutputModel> lst = new List<CategoryRowOutputModel>();
            foreach (var item in data)
            {
                CategoryRowOutputModel output = new CategoryRowOutputModel
                {
                    CategoryRowId = item.Key
                };

                ICollection<CategoryValueModel> row = new List<CategoryValueModel>();
                foreach (var cell in item)
                {
                    row.Add(new CategoryValueModel
                    {
                        CategoryFieldId = cell.CategoryFieldId,
                        CategoryValueId = cell.CategoryValueId,
                        Value = cell.Value
                    });
                }
                output.Values = row;
                lst.Add(output);
            }
            return (lst, total);
        }

        public async Task<ServiceResult<CategoryRowOutputModel>> GetCategoryRow(int categoryId, int categoryRowId)
        {
            // Check row 
            var categoryRow = await _accountingContext.CategoryRow.FirstOrDefaultAsync(r => r.CategoryId == categoryId && r.CategoryRowId == categoryRowId);
            if (categoryRow == null)
            {
                return CategoryErrorCode.CategoryRowNotFound;
            }

            var values = _accountingContext.CategoryRowValue
                           .Where(r => r.CategoryRowId == categoryRowId)
                           .Join(_accountingContext.CategoryValue, rv => rv.CategoryValueId, v => v.CategoryValueId, (rv, v) => new
                           {
                               rv.CategoryRowId,
                               v.Value,
                               v.CategoryValueId,
                               rv.CategoryFieldId
                           }).ToList();

            CategoryRowOutputModel output = new CategoryRowOutputModel
            {
                CategoryRowId = categoryRowId
            };

            ICollection<CategoryValueModel> row = new List<CategoryValueModel>();
            foreach (var value in values)
            {
                row.Add(new CategoryValueModel
                {
                    CategoryFieldId = value.CategoryFieldId,
                    CategoryValueId = value.CategoryValueId,
                    Value = value.Value
                });
            }
            output.Values = row;

            return output;
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
            // Lấy thông tin field
            var categoryIds = GetAllCategoryIds(categoryId);
            var categoryFields = _accountingContext.CategoryField.Where(f => categoryIds.Contains(f.CategoryId)).AsEnumerable();
            var requiredFields = categoryFields.Where(f => !f.AutoIncrement && f.IsRequired);
            var uniqueFields = categoryFields.Where(f => !f.AutoIncrement && f.IsUnique);
            var selectFields = categoryFields.Where(f => !f.AutoIncrement && f.ReferenceCategoryFieldId.HasValue);

            // Check field required
            var r = CheckRequired(data, requiredFields);
            if (!r.IsSuccess()) return r;

            // Check unique
            r = CheckUnique(data, uniqueFields);
            if (!r.IsSuccess()) return r;

            // Check refer
            r = CheckRefer(data, selectFields);
            if (!r.IsSuccess()) return r;

            using (var trans = await _accountingContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // Thêm dòng
                    var categoryRow = new CategoryRow
                    {
                        CategoryId = categoryId
                    };
                    await _accountingContext.CategoryRow.AddAsync(categoryRow);
                    await _accountingContext.SaveChangesAsync();

                    // Duyệt danh sách field
                    foreach (var field in categoryFields)
                    {
                        int categoryValueId = 0;
                        var valueItem = data.Values.FirstOrDefault(v => v.CategoryFieldId == field.CategoryFieldId);
                        if (valueItem == null && !field.AutoIncrement)
                        {
                            continue;
                        }

                        if (!field.ReferenceCategoryFieldId.HasValue)
                        {
                            string value = string.Empty;
                            if (field.AutoIncrement)
                            {
                                // Lấy ra value lớn nhất
                                string max = _accountingContext.CategoryValue.Where(v => v.CategoryFieldId == field.CategoryFieldId).Max(v => v.Value);
                                value = (int.Parse(max ?? "0") + 1).ToString();
                            }
                            else
                            {
                                value = valueItem.Value;
                            }
                            // Thêm value
                            CategoryValue categoryValue = new CategoryValue
                            {
                                CategoryFieldId = field.CategoryFieldId,
                                Value = value,
                                UpdatedUserId = updatedUserId
                            };
                            await _accountingContext.CategoryValue.AddAsync(categoryValue);
                            await _accountingContext.SaveChangesAsync();
                            categoryValueId = categoryValue.CategoryValueId;
                        }
                        else
                        {
                            categoryValueId = valueItem.CategoryValueId;
                        }

                        // Thêm mapping
                        var categoryRowValue = new CategoryRowValue
                        {
                            CategoryFieldId = field.CategoryFieldId,
                            CategoryRowId = categoryRow.CategoryRowId,
                            CategoryValueId = categoryValueId,
                            UpdatedUserId = updatedUserId
                        };
                        await _accountingContext.CategoryRowValue.AddAsync(categoryRowValue);
                        await _accountingContext.SaveChangesAsync();
                    }

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
        }

        public async Task<Enum> UpdateCategoryRow(int updatedUserId, int categoryId, int categoryRowId, CategoryRowInputModel data)
        {
            var categoryRow = await _accountingContext.CategoryRow.FirstOrDefaultAsync(c => c.CategoryRowId == categoryRowId && c.CategoryId == categoryId);
            if (categoryRow == null)
            {
                return CategoryErrorCode.CategoryRowNotFound;
            }
            // Lấy thông tin field
            var categoryIds = GetAllCategoryIds(categoryRow.CategoryId);
            var categoryFields = _accountingContext.CategoryField.Where(f => categoryIds.Contains(f.CategoryId)).AsEnumerable();

            // Lấy thông tin value hiện tại
            var currentValues = _accountingContext.CategoryRowValue
                .Where(rv => rv.CategoryRowId == categoryRowId)
                .Join(_accountingContext.CategoryValue, rv => rv.CategoryValueId, v => v.CategoryValueId, (rv, v) => new
                {
                    rv.CategoryValueId,
                    rv.CategoryFieldId,
                    v.Value
                }).ToList();

            var updateFields = categoryFields
                .Where(f => currentValues.FirstOrDefault(v => v.CategoryFieldId == f.CategoryFieldId).Value != data.Values.FirstOrDefault(v => v.CategoryFieldId == f.CategoryFieldId).Value)
                .ToList();

            // Lấy thông tin field
            var requiredFields = updateFields.Where(f => !f.AutoIncrement && f.IsRequired);
            var uniqueFields = updateFields.Where(f => !f.AutoIncrement && f.IsUnique);
            var selectFields = updateFields.Where(f => !f.AutoIncrement && f.ReferenceCategoryFieldId.HasValue);

            // Check field required
            var r = CheckRequired(data, requiredFields);
            if (!r.IsSuccess()) return r;

            // Check unique
            r = CheckUnique(data, uniqueFields, categoryRowId);
            if (!r.IsSuccess()) return r;

            // Check refer
            r = CheckRefer(data, selectFields);
            if (!r.IsSuccess()) return r;

            using (var trans = await _accountingContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // Duyệt danh sách field
                    foreach (var field in updateFields)
                    {
                        var valueItem = data.Values.FirstOrDefault(v => v.CategoryFieldId == field.CategoryFieldId);
                        if (valueItem == null || field.AutoIncrement)
                        {
                            continue;
                        }
                        if (!field.ReferenceCategoryFieldId.HasValue)
                        {
                            string value = valueItem.Value;
                            // Sửa value cũ
                            int currentValueId = currentValues.First(v => v.CategoryFieldId == field.CategoryFieldId).CategoryValueId;
                            var currentValue = _accountingContext.CategoryValue.First(v => v.CategoryValueId == currentValueId);
                            currentValue.Value = value;
                            currentValue.UpdatedUserId = updatedUserId;
                        }
                        else
                        {
                            // Sửa mapping giá trị mới
                            var currentRowValue = _accountingContext.CategoryRowValue.First(rv => rv.CategoryRowId == categoryRowId && rv.CategoryFieldId == field.CategoryFieldId);
                            currentRowValue.CategoryValueId = valueItem.CategoryValueId;
                            currentRowValue.UpdatedUserId = updatedUserId;
                        }
                        await _accountingContext.SaveChangesAsync();
                    }

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
                        .Select(rv => rv.CategoryValueId).FirstOrDefault();
                    bool isRefer = valueId > 0 && _accountingContext.CategoryRowValue.Any(rv => rv.CategoryValueId == valueId && rv.CategoryRowId != categoryRowId);
                    if (isRefer) return CategoryErrorCode.DestCategoryFieldAlreadyExisted;
                }
            }

            using var trans = await _accountingContext.Database.BeginTransactionAsync();
            try
            {
                // Delete row
                categoryRow.IsDeleted = true;
                categoryRow.UpdatedUserId = updatedUserId;
                foreach (var field in categoryFields)
                {
                    var categoryRowValue = _accountingContext.CategoryRowValue
                      .Where(rv => rv.CategoryFieldId == field.CategoryFieldId && rv.CategoryRowId == categoryRowId)
                      .FirstOrDefault();
                    if (!field.ReferenceCategoryFieldId.HasValue)
                    {
                        // Delete value
                        var value = _accountingContext.CategoryValue.FirstOrDefault(v => v.CategoryValueId == categoryRowValue.CategoryValueId);
                        value.IsDeleted = true;
                        value.UpdatedUserId = updatedUserId;
                    }
                    // Delete row-field-value
                    categoryRowValue.IsDeleted = true;
                    categoryRowValue.UpdatedUserId = updatedUserId;
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

        private Enum CheckRefer(CategoryRowInputModel data, IEnumerable<CategoryField> selectFields)
        {
            // Check refer
            foreach (var field in selectFields)
            {
                var valueItem = data.Values.FirstOrDefault(v => v.CategoryFieldId == field.CategoryFieldId);
                if (valueItem != null)
                {
                    bool isExisted = _accountingContext.CategoryValue
                        .Join(_accountingContext.CategoryRowValue, v => v.CategoryValueId, rv => rv.CategoryValueId, (v, rv) => new
                        {
                            v.CategoryValueId,
                            rv.CategoryFieldId,
                            v.Value
                        })
                        .Any(v => v.CategoryValueId == valueItem.CategoryValueId
                        && v.CategoryFieldId == field.ReferenceCategoryFieldId
                        && v.Value == valueItem.Value);
                    if (!isExisted)
                    {
                        return CategoryErrorCode.ReferValueNotFound;
                    }
                }
            }
            return GeneralCode.Success;
        }

        private Enum CheckUnique(CategoryRowInputModel data, IEnumerable<CategoryField> uniqueFields, int? categoryRowId = null)
        {
            // Check unique
            foreach (var item in data.Values.Where(v => uniqueFields.Any(f => f.CategoryFieldId == v.CategoryFieldId)))
            {
                bool isExisted = _accountingContext.CategoryRowValue
                    .Join(_accountingContext.CategoryValue, rv => rv.CategoryValueId, v => v.CategoryValueId, (rv, v) => new
                    {
                        rv.CategoryFieldId,
                        rv.CategoryValueId,
                        rv.CategoryRowId,
                        v.Value
                    })
                    .Any(rfv => (categoryRowId.HasValue ? rfv.CategoryRowId != categoryRowId : true) && rfv.CategoryFieldId == item.CategoryFieldId && rfv.Value == item.Value);

                if (isExisted)
                {
                    return CategoryErrorCode.UniqueValueAlreadyExisted;
                }
            }
            return GeneralCode.Success;
        }

        private Enum CheckRequired(CategoryRowInputModel data, IEnumerable<CategoryField> requiredFields)
        {
            if (requiredFields.Count() > 0 && requiredFields.Any(rf => !data.Values.Any(v => v.CategoryFieldId == rf.CategoryFieldId && !string.IsNullOrWhiteSpace(v.Value))))
            {
                return CategoryErrorCode.RequiredFieldIsEmpty;
            }
            return GeneralCode.Success;
        }

    }
}
