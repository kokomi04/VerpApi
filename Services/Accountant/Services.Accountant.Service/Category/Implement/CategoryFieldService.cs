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
using CategoryEntity = VErp.Infrastructure.EF.AccountingDB.Category;

namespace VErp.Services.Accountant.Service.Category.Implement
{
    public class CategoryFieldService : CategoryBaseService, ICategoryFieldService
    {
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly IMapper _mapper;
        public CategoryFieldService(AccountingDBContext accountingContext
            , IOptions<AppSetting> appSetting
            , ILogger<CategoryFieldService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            ) : base(accountingContext)
        {
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
            _mapper = mapper;
        }

        public async Task<PageData<CategoryFieldInputModel>> GetCategoryFields(int categoryId, string keyword, int page, int size, bool? isFull)
        {
            keyword = (keyword ?? "").Trim();
            var query = _accountingContext.CategoryField.AsQueryable();
            int[] categoryIds;
            if (isFull.HasValue && isFull.Value)
            {
                categoryIds = GetAllCategoryIds(categoryId);
            }
            else
            {
                categoryIds = new int[] { categoryId };
            }
            query = query.Where(c => categoryIds.Contains(c.CategoryId));
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(f => f.Name.Contains(keyword) || f.Title.Contains(keyword));
            }
            query = query.OrderBy(c => c.Sequence);
            var total = await query.CountAsync();
            if (size > 0)
            {
                query = query.Skip((page - 1) * size).Take(size);
            }
            List<CategoryFieldInputModel> lst = await query.Include(f => f.DataType)
                .Include(f => f.FormType)
                .OrderBy(f => f.Sequence)
                .Select(f => _mapper.Map<CategoryFieldInputModel>(f)).ToListAsync();

            return (lst, total);
        }

        public async Task<ServiceResult<int>> AddCategoryField(int updatedUserId, CategoryFieldInputModel data)
        {
            // Check category
            if (!_accountingContext.Category.Any(c => c.CategoryId == data.CategoryId))
            {
                return CategoryErrorCode.CategoryNotFound;
            }

            var existedCategoryField = await _accountingContext.CategoryField
                .FirstOrDefaultAsync(f => f.CategoryId == data.CategoryId && f.Name == data.Name || f.Title == data.Title);
            if (existedCategoryField != null)
            {
                if (string.Compare(existedCategoryField.Name, data.Name, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return CategoryErrorCode.CategoryFieldNameAlreadyExisted;
                }

                return CategoryErrorCode.CategoryFieldTitleAlreadyExisted;
            }

            if (data.ReferenceCategoryFieldId.HasValue)
            {
                var sourceCategoryField = _accountingContext.CategoryField.FirstOrDefault(f => f.CategoryFieldId == data.ReferenceCategoryFieldId);
                if (sourceCategoryField == null)
                {
                    return CategoryErrorCode.SourceCategoryFieldNotFound;
                }
            }

            using var trans = await _accountingContext.Database.BeginTransactionAsync();
            try
            {
                var categoryField = _mapper.Map<CategoryField>(data);

                categoryField.UpdatedUserId = updatedUserId;

                await _accountingContext.CategoryField.AddAsync(categoryField);
                await _accountingContext.SaveChangesAsync();

                trans.Commit();
                await _activityLogService.CreateLog(EnumObjectType.Category, categoryField.CategoryFieldId, $"Thêm trường danh mục {categoryField.Title}", data.JsonSerialize());
                return categoryField.CategoryFieldId;
            }
            catch (Exception ex)
            {
                trans.Rollback();
                _logger.LogError(ex, "Create");
                return GeneralCode.InternalError;
            }
        }

        public async Task<Enum> UpdateCategoryField(int updatedUserId, int categoryId, int categoryFieldId, CategoryFieldInputModel data)
        {
            var categoryField = await _accountingContext.CategoryField.FirstOrDefaultAsync(c => c.CategoryFieldId == categoryFieldId && c.CategoryId == categoryId);
            if (categoryField == null)
            {
                return CategoryErrorCode.CategoryFieldNotFound;
            }
            if (categoryField.Name != data.Name || categoryField.Title != data.Title)
            {
                var existedCategoryField = await _accountingContext.CategoryField
                    .FirstOrDefaultAsync(f => f.CategoryFieldId != categoryFieldId && (f.Name == data.Name || f.Title == data.Title));
                if (existedCategoryField != null)
                {
                    if (string.Compare(existedCategoryField.Name, data.Name, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return CategoryErrorCode.CategoryFieldNameAlreadyExisted;
                    }

                    return CategoryErrorCode.CategoryFieldTitleAlreadyExisted;
                }
            }

            if (data.ReferenceCategoryFieldId.HasValue && data.ReferenceCategoryFieldId != categoryField.ReferenceCategoryFieldId)
            {
                var sourceCategoryField = _accountingContext.CategoryField.FirstOrDefault(f => f.CategoryFieldId == data.ReferenceCategoryFieldId);
                if (sourceCategoryField == null)
                {
                    return CategoryErrorCode.SourceCategoryFieldNotFound;
                }
            }

            using (var trans = await _accountingContext.Database.BeginTransactionAsync())
            {
                try
                {
                    categoryField.Name = data.Name;
                    categoryField.Title = data.Title;
                    categoryField.Sequence = data.Sequence;
                    categoryField.DataSize = data.DataSize;
                    categoryField.DataTypeId = data.DataTypeId;
                    categoryField.FormTypeId = data.FormTypeId;
                    categoryField.AutoIncrement = data.AutoIncrement;
                    categoryField.IsRequired = data.IsRequired;
                    categoryField.IsUnique = data.IsUnique;
                    categoryField.ReferenceCategoryFieldId = data.ReferenceCategoryFieldId;
                    categoryField.UpdatedUserId = updatedUserId;
                    await _accountingContext.SaveChangesAsync();
                  
                    trans.Commit();
                    await _activityLogService.CreateLog(EnumObjectType.Category, categoryField.CategoryFieldId, $"Cập nhật trường dữ liệu {categoryField.Title}", data.JsonSerialize());
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

        public async Task<Enum> DeleteCategoryField(int updatedUserId, int categoryId, int categoryFieldId)
        {
            var categoryField = await _accountingContext.CategoryField.FirstOrDefaultAsync(c => c.CategoryFieldId == categoryFieldId && c.CategoryId == categoryId);
            if (categoryField == null)
            {
                return CategoryErrorCode.CategoryFieldNotFound;
            }
       
            // Check reference
            bool isRefer = await _accountingContext.CategoryField.AnyAsync(c => c.ReferenceCategoryFieldId == categoryFieldId);
            if (isRefer)
            {
                return CategoryErrorCode.DestCategoryFieldAlreadyExisted;
            }

            using var trans = await _accountingContext.Database.BeginTransactionAsync();
            try
            {
                // Delete value
                var values = _accountingContext.CategoryValue.Where(v => v.CategoryFieldId == categoryFieldId);
                foreach(var value in values)
                {
                    value.IsDeleted = true;
                    value.UpdatedUserId = updatedUserId;
                }
                // Delete row-field-value
                var rowFieldValues = _accountingContext.CategoryRowValue.Where(rfv => rfv.CategoryFieldId == categoryFieldId);
                foreach (var rowFieldValue in rowFieldValues)
                {
                    rowFieldValue.IsDeleted = true;
                    rowFieldValue.UpdatedUserId = updatedUserId;
                }
                // Delete field
                categoryField.IsDeleted = true;
                categoryField.UpdatedUserId = updatedUserId;
                await _accountingContext.SaveChangesAsync();
                trans.Commit();
                await _activityLogService.CreateLog(EnumObjectType.Category, categoryField.CategoryFieldId, $"Xóa trường thư mục {categoryField.Title}", categoryField.JsonSerialize());
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
