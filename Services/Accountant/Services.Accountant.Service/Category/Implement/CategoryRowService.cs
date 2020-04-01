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
            var requiredFields = categoryFields.Where(f => f.IsRequired);
            var uniqueFields = categoryFields.Where(f => f.IsUnique);

            // Check field required
            if (requiredFields.Count() > 0 && requiredFields.Any(rf => !data.Values.Any(v => v.CategoryFieldId == rf.CategoryFieldId && !string.IsNullOrWhiteSpace(v.Value))))
            {
                return CategoryErrorCode.RequiredFieldIsEmpty;
            }
            // Check unique
            foreach(var item in data.Values.Where(v => uniqueFields.Any(f => f.CategoryFieldId == v.CategoryFieldId)))
            {
                bool isExisted = _accountingContext.CategoryValue.Any(v => v.CategoryFieldId == item.CategoryFieldId && v.Value == item.Value);
                if (isExisted)
                {
                    return CategoryErrorCode.UniqueValueAlreadyExisted;
                }
            }

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
                    var autoIncrementFields = categoryFields.Where(f => f.AutoIncrement);
                    foreach(var field in autoIncrementFields)
                    {
                        // Lấy ra value lớn nhất
                        string max = _accountingContext.CategoryValue.Where(v => v.CategoryFieldId == field.CategoryFieldId).Max(v => v.Value);
                        string value = (int.Parse(max) + 1).ToString();
                        // Thêm value
                        var categoryValue = _mapper.Map<CategoryValue>(value);
                        await _accountingContext.CategoryValue.AddAsync(categoryValue);
                        await _accountingContext.SaveChangesAsync();
                        // Thêm mapping
                        var categoryRowValue = new CategoryRowValue
                        {
                            CategoryRowId = categoryRow.CategoryRowId,
                            CategoryValueId = categoryValue.CategoryValueId,
                        };
                        await _accountingContext.CategoryRowValue.AddAsync(categoryRowValue);
                        await _accountingContext.SaveChangesAsync();
                    }

                    var selectFields = categoryFields.Where(f => !f.AutoIncrement && f.ReferenceCategoryFieldId.HasValue);
                    foreach (var field in selectFields)
                    {
                        var value = data.Values.FirstOrDefault(v => v.CategoryFieldId == field.CategoryFieldId);
                        // Thêm mapping
                        var categoryRowValue = new CategoryRowValue
                        {
                            CategoryRowId = categoryRow.CategoryRowId,
                            CategoryValueId = value.CategoryValueId,
                        };
                        await _accountingContext.CategoryRowValue.AddAsync(categoryRowValue);
                        await _accountingContext.SaveChangesAsync();
                    }

                    var inputFields = categoryFields.Where(f => !f.AutoIncrement && !f.ReferenceCategoryFieldId.HasValue);
                    foreach (var field in inputFields)
                    {
                        var value = data.Values.FirstOrDefault(v => v.CategoryFieldId == field.CategoryFieldId);
                        // Thêm value
                        var categoryValue = _mapper.Map<CategoryValue>(value);
                        await _accountingContext.CategoryValue.AddAsync(categoryValue);
                        await _accountingContext.SaveChangesAsync();
                        // Thêm mapping
                        var categoryRowValue = new CategoryRowValue
                        {
                            CategoryRowId = categoryRow.CategoryRowId,
                            CategoryValueId = categoryValue.CategoryValueId,
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

        public async Task<PageData<IDictionary<string, string>>> GetCategoryRows(int categoryId, int page, int size)
        {
            var query = _accountingContext.CategoryRow
                           .Where(r => r.CategoryId == categoryId)
                           .Join(_accountingContext.CategoryRowValue, r => r.CategoryRowId, rv => rv.CategoryRowId, (r, rv) => new
                           {
                               r.CategoryRowId,
                               rv.CategoryValueId
                           })
                           .Join(_accountingContext.CategoryValue, rv => rv.CategoryValueId, v => v.CategoryValueId, (rv, v) => new
                           {
                               rv.CategoryRowId,
                               v.Value,
                               v.CategoryFieldId
                           })
                           .Join(_accountingContext.CategoryField, rv => rv.CategoryFieldId, f => f.CategoryFieldId, (rv, f) => new
                           {
                               rv.CategoryRowId,
                               rv.Value,
                               FieldName = f.Name,
                               f.IsHidden
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


            List<IDictionary<string, string>> lst = new List<IDictionary<string, string>>();
            foreach (var item in data)
            {
                IDictionary<string, string> row = new Dictionary<string, string>();
                foreach (var cell in item)
                {
                    row.Add(cell.FieldName, cell.Value);
                }
                lst.Add(row);
            }
            return (lst, total);
        }
    }
}
