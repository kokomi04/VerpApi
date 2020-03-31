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
    public class CategoryService : ICategoryService
    {
        private readonly AccountingDBContext _accountingContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly IMapper _mapper;

        public CategoryService(AccountingDBContext accountingContext
            , IOptions<AppSetting> appSetting
            , ILogger<CategoryService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            )
        {
            _accountingContext = accountingContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
            _mapper = mapper;
        }

        public async Task<ServiceResult<CategoryFullModel>> GetCategory(int categoryId)
        {
            var category = await _accountingContext.Category.FirstOrDefaultAsync(c => c.CategoryId == categoryId);
            if (category == null)
            {
                return CategoryErrorCode.CategoryNotFound;
            }
            CategoryFullModel categoryFullModel = _mapper.Map<CategoryFullModel>(category);
            categoryFullModel.SubCategories = GetSubCategories(category.CategoryId);
            categoryFullModel.CategoryFields = GetFields(category.CategoryId);
            return categoryFullModel;
        }

        public async Task<PageData<CategoryModel>> GetCategories(string keyword, bool? isModule, int page, int size)
        {
            keyword = (keyword ?? "").Trim();

            var query = _accountingContext.Category.Include(c => c.SubCategories).AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(c => c.Code.Contains(keyword) || c.Title.Contains(keyword));
            }
            if (isModule.HasValue)
            {
                query = query.Where(c => c.IsModule == isModule.Value);
            }

            query = query.OrderBy(c => c.Title);

            if (size > 0)
            {
                query = query.Skip((page - 1) * size).Take(size);
            }
            List<CategoryModel> lst = new List<CategoryModel>();

            foreach (var item in query)
            {
                CategoryModel categoryModel = _mapper.Map<CategoryModel>(item);
                lst.Add(categoryModel);
            }
            var total = lst.Count;
            return (lst, total);
        }

        public async Task<ServiceResult<int>> AddCategory(int updatedUserId, CategoryModel data)
        {
            var existedCategory = await _accountingContext.Category
                .FirstOrDefaultAsync(c => c.Code == data.Code || c.Title == data.Title);
            if (existedCategory != null)
            {
                if (string.Compare(existedCategory.Code, data.Code, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return CategoryErrorCode.CategoryCodeAlreadyExisted;
                }

                return CategoryErrorCode.CategoryNameAlreadyExisted;
            }
            foreach (int subId in data.SubCategories.Select(c => c.CategoryId))
            {
                var subCategory = _accountingContext.Category.FirstOrDefault(c => c.CategoryId == subId);
                if (subCategory == null)
                {
                    return CategoryErrorCode.SubCategoryNotFound;
                }
                else if (subCategory.IsModule)
                {
                    return CategoryErrorCode.SubCategoryIsModule;
                }
                else if (subCategory.ParentId.HasValue)
                {
                    return CategoryErrorCode.SubCategoryHasParent;
                }
            }
            using (var trans = await _accountingContext.Database.BeginTransactionAsync())
            {
                try
                {
                    CategoryEntity category = _mapper.Map<CategoryEntity>(data);
                    category.UpdatedUserId = updatedUserId;

                    await _accountingContext.Category.AddAsync(category);
                    await _accountingContext.SaveChangesAsync();
                    foreach (int subId in data.SubCategories.Select(c => c.CategoryId))
                    {
                        var subCategory = _accountingContext.Category.FirstOrDefault(c => c.CategoryId == subId);
                        subCategory.ParentId = category.CategoryId;
                        subCategory.UpdatedUserId = updatedUserId;
                        await _accountingContext.SaveChangesAsync();
                    }
                    trans.Commit();
                    await _activityLogService.CreateLog(EnumObjectType.Category, category.CategoryId, $"Thêm danh mục {category.Title}", data.JsonSerialize());
                    return category.CategoryId;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    _logger.LogError(ex, "Create");
                    return GeneralCode.InternalError;
                }
            }
        }

        public async Task<Enum> UpdateCategory(int updatedUserId, int categoryId, CategoryModel data)
        {
            var category = await _accountingContext.Category.Include(c => c.SubCategories).FirstOrDefaultAsync(c => c.CategoryId == categoryId);
            if (category == null)
            {
                return CategoryErrorCode.CategoryNotFound;
            }
            if (category.Code != data.Code || category.Title != data.Title)
            {
                var existedCategory = await _accountingContext.Category
                    .FirstOrDefaultAsync(c => c.CategoryId != categoryId && (c.Code == data.Code || c.Title == data.Title));
                if (existedCategory != null)
                {
                    if (string.Compare(existedCategory.Code, data.Code, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return CategoryErrorCode.CategoryCodeAlreadyExisted;
                    }

                    return CategoryErrorCode.CategoryNameAlreadyExisted;
                }
            }

            var deleteSubCategories = category.SubCategories.Where(c => !data.SubCategories.Any(s => s.CategoryId == c.CategoryId)).ToList();
            var newSubCategories = data.SubCategories.Where(c => !category.SubCategories.Any(s => s.CategoryId == c.CategoryId)).ToList();

            foreach (int subId in newSubCategories.Select(c => c.CategoryId))
            {
                var subCategory = _accountingContext.Category.FirstOrDefault(c => c.CategoryId == subId);
                if (subCategory == null)
                {
                    return CategoryErrorCode.SubCategoryNotFound;
                }
                else if (subCategory.IsModule)
                {
                    return CategoryErrorCode.SubCategoryIsModule;
                }
                else if (subCategory.ParentId.HasValue)
                {
                    return CategoryErrorCode.SubCategoryHasParent;
                }
            }

            using (var trans = await _accountingContext.Database.BeginTransactionAsync())
            {
                try
                {
                    category.Code = data.Code;
                    category.Title = data.Title;
                    category.IsModule = data.IsModule;
                    category.IsReadonly = data.IsReadonly;
                    category.UpdatedUserId = updatedUserId;
                    await _accountingContext.SaveChangesAsync();
                    foreach (var item in deleteSubCategories)
                    {
                        var subCategory = _accountingContext.Category.FirstOrDefault(c => c.CategoryId == item.CategoryId);
                        subCategory.ParentId = null;
                        subCategory.UpdatedUserId = updatedUserId;
                    }
                    foreach (var item in newSubCategories)
                    {
                        var subCategory = _accountingContext.Category.FirstOrDefault(c => c.CategoryId == item.CategoryId);
                        subCategory.ParentId = category.CategoryId;
                        subCategory.UpdatedUserId = updatedUserId;
                        await _accountingContext.SaveChangesAsync();
                    }
                    await _accountingContext.SaveChangesAsync();
                    trans.Commit();
                    await _activityLogService.CreateLog(EnumObjectType.Category, category.CategoryId, $"Cập nhật danh mục {category.Title}", data.JsonSerialize());
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

        public async Task<Enum> DeleteCategory(int categoryId, int updatedUserId)
        {
            var category = await _accountingContext.Category.Include(c => c.Parent).FirstOrDefaultAsync(c => c.CategoryId == categoryId);
            if (category == null)
            {
                return CategoryErrorCode.CategoryNotFound;
            }
            if (category.Parent != null)
            {
                return CategoryErrorCode.ParentCategoryAlreadyExisted;
            }
            using (var trans = await _accountingContext.Database.BeginTransactionAsync())
            {
                try
                {
                    category.IsDeleted = true;
                    category.UpdatedUserId = updatedUserId;

                    foreach (var item in category.SubCategories)
                    {
                        var subCategory = _accountingContext.Category.FirstOrDefault(c => c.CategoryId == item.CategoryId);
                        subCategory.ParentId = null;
                        subCategory.UpdatedUserId = updatedUserId;
                    }

                    var deleteFields = _accountingContext.CategoryField.Where(f => f.CategoryId == categoryId);
                    foreach (var item in deleteFields)
                    {
                        // Check có trường đang tham chiếu tới
                        if(_accountingContext.CategoryField.Any(f => f.ReferenceCategoryFieldId == item.CategoryFieldId))
                        {
                            trans.Rollback();
                            return CategoryErrorCode.DestCategoryFieldAlreadyExisted;
                        }
                        item.IsDeleted = true;
                        item.UpdatedUserId = updatedUserId;
                    }
                    await _accountingContext.SaveChangesAsync();
                    trans.Commit();
                    await _activityLogService.CreateLog(EnumObjectType.Category, category.CategoryId, $"Xóa danh mục {category.Title}", category.JsonSerialize());
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

        private ICollection<CategoryFullModel> GetSubCategories(int categoryId)
        {
            List<CategoryFullModel> result = new List<CategoryFullModel>();
            var query = _accountingContext.Category.Where(r => r.ParentId == categoryId).ToList();
            foreach (var item in query)
            {
                CategoryFullModel category = _mapper.Map<CategoryFullModel>(item);
                category.SubCategories = GetSubCategories(item.CategoryId);
                category.CategoryFields = GetFields(item.CategoryId);
                result.Add(category);
            }
            return result;
        }

        private ICollection<CategoryFieldOutputModel> GetFields(int categoryId)
        {
            return _accountingContext.CategoryField
                .Where(f => f.CategoryId == categoryId)
                .Include(f => f.DataType)
                .Include(f => f.FormType)
                .OrderBy(f => f.Sequence)
                .Select(f => _mapper.Map<CategoryFieldOutputModel>(f)).ToList();
        }
    }
}
