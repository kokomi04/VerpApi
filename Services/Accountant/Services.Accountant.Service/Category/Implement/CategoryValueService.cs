using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
    public class CategoryValueService : CategoryBaseService, ICategoryValueService
    {
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly IMapper _mapper;
        public CategoryValueService(AccountingDBContext accountingContext
            , IOptions<AppSetting> appSetting
            , ILogger<CategoryValueService> logger
            , IActivityLogService activityLogService
             , IMapper mapper
            ) : base(accountingContext)
        {
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
            _mapper = mapper;
        }

        public async Task<PageData<CategoryValueModel>> GetDefaultCategoryValues(int categoryId, int categoryFieldId, string keyword, int page, int size)
        {
            var query = _accountingContext.CategoryValue
                        .Where(v => v.CategoryFieldId == categoryFieldId && v.IsDefault);

            if (string.IsNullOrEmpty(keyword))
            {
                query = query.Where(v => v.Value.Contains(keyword));
            }
            int total = await query.CountAsync();
            if (size > 0)
            {
                query = query.Skip((page - 1) * size).Take(size);
            }

            List<CategoryValueModel> lst = query.Select(v => _mapper.Map<CategoryValueModel>(v)).ToList();
            return (lst, total);
        }

        public async Task<ServiceResult<CategoryValueModel>> GetDefaultCategoryValue(int categoryId, int categoryFieldId, int categoryValueId)
        {
            // Check row 
            var categoryValue = await _accountingContext.CategoryValue.FirstOrDefaultAsync(v => v.CategoryFieldId == categoryFieldId && v.CategoryValueId == categoryValueId && v.IsDefault);
            if (categoryValue == null)
            {
                return CategoryErrorCode.CategoryValueNotFound;
            }

            var values = _accountingContext.CategoryValue
                           .Where(v => v.CategoryValueId == categoryValueId)
                           .FirstOrDefault();

            return _mapper.Map<CategoryValueModel>(values);
        }

        public async Task<ServiceResult<int>> AddDefaultCategoryValue(int updatedUserId, int categoryId, int categoryFieldId, CategoryValueModel data)
        {
            // Validate
            var categoryField = _accountingContext.CategoryField
                .Include(f => f.DataType)
                .FirstOrDefault(f => f.CategoryId == categoryId && f.CategoryFieldId == categoryFieldId);
            if (categoryField == null)
            {
                return CategoryErrorCode.CategoryFieldNotFound;
            }

            if (categoryField.FormTypeId != 2 || categoryField.ReferenceCategoryFieldId.HasValue)
            {
                return CategoryErrorCode.CategoryFieldNotDefaultValue;
            }

            if (categoryField.DataSize > 0 && data.Value.Length > categoryField.DataSize)
            {
                return CategoryErrorCode.CategoryValueInValid;
            }

            if (!string.IsNullOrEmpty(categoryField.DataType.RegularExpression) && !Regex.IsMatch(data.Value, categoryField.DataType.RegularExpression))
            {
                return CategoryErrorCode.CategoryValueInValid;
            }

            using (var trans = await _accountingContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // Thêm value
                    CategoryValue categoryValue = new CategoryValue
                    {
                        CategoryFieldId = categoryFieldId,
                        Value = data.Value,
                        IsDefault = true,
                        UpdatedUserId = updatedUserId
                    };

                    await _accountingContext.CategoryValue.AddAsync(categoryValue);
                    await _accountingContext.SaveChangesAsync();
                    trans.Commit();
                    await _activityLogService.CreateLog(EnumObjectType.Category, categoryValue.CategoryValueId, $"Thêm giá trị {categoryValue.Value}", categoryValue.JsonSerialize());
                    return categoryValue.CategoryValueId;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    _logger.LogError(ex, "Create");
                    return GeneralCode.InternalError;
                }
            }
        }

        public async Task<Enum> UpdateDefaultCategoryValue(int updatedUserId, int categoryId, int categoryFieldId, int categoryValueId, CategoryValueModel data)
        {

            var categoryValue = await _accountingContext.CategoryValue.FirstOrDefaultAsync(v => v.CategoryValueId == categoryValueId && v.CategoryFieldId == categoryFieldId);
            if (categoryValue == null)
            {
                return CategoryErrorCode.CategoryValueNotFound;
            }

            var categoryField = _accountingContext.CategoryField
                .Include(f => f.DataType)
                .FirstOrDefault(f => f.CategoryId == categoryId && f.CategoryFieldId == categoryFieldId);
            if (categoryField == null)
            {
                return CategoryErrorCode.CategoryFieldNotFound;
            }

            if (categoryField.DataSize > 0 && data.Value.Length > categoryField.DataSize)
            {
                return CategoryErrorCode.CategoryValueInValid;
            }

            if (!string.IsNullOrEmpty(categoryField.DataType.RegularExpression) && !Regex.IsMatch(data.Value, categoryField.DataType.RegularExpression))
            {
                return CategoryErrorCode.CategoryValueInValid;
            }

            using (var trans = await _accountingContext.Database.BeginTransactionAsync())
            {
                try
                {
                    categoryValue.Value = data.Value;
                    categoryValue.UpdatedUserId = updatedUserId;
                    await _accountingContext.SaveChangesAsync();
                    trans.Commit();
                    await _activityLogService.CreateLog(EnumObjectType.Category, categoryValue.CategoryValueId, $"Cập nhật giá trị {categoryValue.Value}", data.JsonSerialize());
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

        public async Task<Enum> DeleteDefaultCategoryValue(int updatedUserId, int categoryId, int categoryFieldId, int categoryValueId)
        {
            var categoryValue = await _accountingContext.CategoryValue.FirstOrDefaultAsync(v => v.CategoryValueId == categoryValueId && v.CategoryFieldId == categoryFieldId);
            if (categoryValue == null)
            {
                return CategoryErrorCode.CategoryValueNotFound;
            }
            // Check reference
            if (_accountingContext.CategoryRowValue.Any(rv => rv.CategoryFieldId == categoryFieldId && rv.CategoryValueId == categoryValueId))
            {
                return CategoryErrorCode.CategoryRowAlreadyExisted;
            }

            using var trans = await _accountingContext.Database.BeginTransactionAsync();
            try
            {
                // Delete row
                categoryValue.IsDeleted = true;
                categoryValue.UpdatedUserId = updatedUserId;
                await _accountingContext.SaveChangesAsync();
                trans.Commit();
                await _activityLogService.CreateLog(EnumObjectType.Category, categoryValueId, $"Xóa giá trị {categoryValueId}", categoryValue.JsonSerialize());
                return GeneralCode.Success;
            }
            catch (Exception ex)
            {
                trans.Rollback();
                _logger.LogError(ex, "Delete");
                return GeneralCode.InternalError;
            }
        }
    }
}
