using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public async Task<PageData<CategoryFieldOutputModel>> GetCategoryFields(int categoryId, string keyword, int page, int size, bool? isFull)
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
                query = query.Where(f => f.CategoryFieldName.Contains(keyword) || f.Title.Contains(keyword));
            }
            query = query.OrderBy(c => c.SortOrder);
            var total = await query.CountAsync();
            if (size > 0)
            {
                query = query.Skip((page - 1) * size).Take(size);
            }
            List<CategoryFieldOutputModel> lst = await query
                .OrderBy(f => f.SortOrder)
                .Select(f => _mapper.Map<CategoryFieldOutputModel>(f)).ToListAsync();

            return (lst, total);
        }

        public async Task<ServiceResult<CategoryFieldOutputFullModel>> GetCategoryField(int categoryId, int categoryFieldId)
        {
            var categoryField = await _accountingContext.CategoryField
                .Include(f => f.DataType)
                .Include(f => f.FormType)
                .Include(f => f.SourceCategoryField)
                .Include(f => f.SourceCategoryTitleField)
                .FirstOrDefaultAsync(c => c.CategoryFieldId == categoryFieldId && c.CategoryId == categoryId);
            if (categoryField == null)
            {
                return CategoryErrorCode.CategoryFieldNotFound;
            }
            CategoryFieldOutputFullModel categoryFieldOutputModel = _mapper.Map<CategoryFieldOutputFullModel>(categoryField);

            if (categoryFieldOutputModel.SourceCategoryField != null)
            {
                CategoryEntity sourceCategory = _accountingContext.Category.FirstOrDefault(c => c.CategoryId == categoryFieldOutputModel.SourceCategoryField.CategoryId);
                categoryFieldOutputModel.SourceCategory = _mapper.Map<CategoryModel>(sourceCategory);
            }


            return categoryFieldOutputModel;
        }

        public async Task<ServiceResult<int>> AddCategoryField(int updatedUserId, int categoryId, CategoryFieldInputModel data)
        {
            // Check category
            if (!_accountingContext.Category.Any(c => c.CategoryId == categoryId))
            {
                return CategoryErrorCode.CategoryNotFound;
            }

            if (_accountingContext.CategoryField.Any(f => f.CategoryId == data.CategoryId && f.CategoryFieldName == data.CategoryFieldName))
            {
                return CategoryErrorCode.CategoryFieldNotFound;
            }

            if (data.ReferenceCategoryFieldId.HasValue)
            {
                var sourceCategoryField = _accountingContext.CategoryField.FirstOrDefault(f => f.CategoryFieldId == data.ReferenceCategoryFieldId);
                if (sourceCategoryField == null)
                {
                    return CategoryErrorCode.SourceCategoryFieldNotFound;
                }
                data.DataTypeId = sourceCategoryField.DataTypeId;
                data.DataSize = sourceCategoryField.DataSize;
            }

            if (data.FormTypeId == (int)EnumFormType.Generate)
            {
                data.DataTypeId = (int)EnumDataType.Text;
                data.DataSize = 0;
            }

            using var trans = await _accountingContext.Database.BeginTransactionAsync();
            try
            {
                var categoryField = _mapper.Map<CategoryField>(data);
                categoryField.CategoryId = categoryId;
                categoryField.CreatedByUserId = updatedUserId;
                categoryField.UpdatedByUserId = updatedUserId;

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
            if (categoryFieldId == data.ReferenceCategoryFieldId)
            {
                return CategoryErrorCode.ReferenceFromItSelf;
            }
            var categoryField = await _accountingContext.CategoryField.FirstOrDefaultAsync(c => c.CategoryFieldId == categoryFieldId && c.CategoryId == categoryId);
            if (categoryField == null)
            {
                return CategoryErrorCode.CategoryFieldNotFound;
            }
            if (categoryField.CategoryFieldName != data.CategoryFieldName && _accountingContext.CategoryField.Any(f => f.CategoryFieldId != categoryFieldId && f.CategoryFieldName == data.CategoryFieldName))
            {
                return CategoryErrorCode.CategoryFieldNameAlreadyExisted;
            }
            if (data.ReferenceCategoryFieldId.HasValue && data.ReferenceCategoryFieldId != categoryField.ReferenceCategoryFieldId)
            {
                var sourceCategoryField = _accountingContext.CategoryField.FirstOrDefault(f => f.CategoryFieldId == data.ReferenceCategoryFieldId);
                if (sourceCategoryField == null)
                {
                    return CategoryErrorCode.SourceCategoryFieldNotFound;
                }
                data.DataTypeId = sourceCategoryField.DataTypeId;
                data.DataSize = sourceCategoryField.DataSize;
            }

            if (data.FormTypeId == (int)EnumFormType.Generate)
            {
                data.DataTypeId = (int)EnumDataType.Text;
                data.DataSize = 0;
            }

            using (var trans = await _accountingContext.Database.BeginTransactionAsync())
            {
                try
                {
                    categoryField.CategoryFieldName = data.CategoryFieldName;
                    categoryField.Title = data.Title;
                    categoryField.SortOrder = data.SortOrder;
                    categoryField.DataSize = data.DataSize;
                    categoryField.DataTypeId = data.DataTypeId;
                    categoryField.FormTypeId = data.FormTypeId;
                    categoryField.AutoIncrement = data.AutoIncrement;
                    categoryField.IsRequired = data.IsRequired;
                    categoryField.IsUnique = data.IsUnique;
                    categoryField.IsHidden = data.IsHidden;
                    categoryField.IsShowList = data.IsShowList;
                    categoryField.RegularExpression = data.RegularExpression;
                    categoryField.Filters = data.Filters;
                    categoryField.ReferenceCategoryFieldId = data.ReferenceCategoryFieldId;
                    categoryField.ReferenceCategoryTitleFieldId = data.ReferenceCategoryTitleFieldId;
                    categoryField.UpdatedByUserId = updatedUserId;
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
                foreach (var value in values)
                {
                    value.IsDeleted = true;
                    value.UpdatedByUserId = updatedUserId;
                }
                // Delete row-field-value
                var rowFieldValues = _accountingContext.CategoryRowValue.Where(rfv => rfv.CategoryFieldId == categoryFieldId);
                foreach (var rowFieldValue in rowFieldValues)
                {
                    rowFieldValue.IsDeleted = true;
                    rowFieldValue.UpdatedByUserId = updatedUserId;
                }
                // Delete field
                categoryField.IsDeleted = true;
                categoryField.UpdatedByUserId = updatedUserId;
                await _accountingContext.SaveChangesAsync();
                trans.Commit();
                await _activityLogService.CreateLog(EnumObjectType.Category, categoryField.CategoryFieldId, $"Xóa trường dữ liệu {categoryField.Title}", categoryField.JsonSerialize());
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
