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
    public class CategoryService : CategoryBaseService, ICategoryService
    {
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly IMapper _mapper;

        public CategoryService(AccountingDBContext accountingContext
            , IOptions<AppSetting> appSetting
            , ILogger<CategoryService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            ) : base(accountingContext)
        {
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

        public async Task<PageData<CategoryModel>> GetCategories(string keyword, bool? isModule, bool? hasParent, int page, int size)
        {
            keyword = (keyword ?? "").Trim();

            var query = _accountingContext.Category.Include(c => c.SubCategories).AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(c => c.CategoryCode.Contains(keyword) || c.Title.Contains(keyword));
            }
            if (isModule.HasValue)
            {
                query = query.Where(c => c.IsModule == isModule.Value);
            }
            if (hasParent.HasValue)
            {
                query = query.Where(c => c.ParentId.HasValue == hasParent.Value);
            }

            query = query.OrderBy(c => c.Title);
            var total = await query.CountAsync();
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

            return (lst, total);
        }

        public async Task<ServiceResult<int>> AddCategory(int updatedUserId, CategoryModel data)
        {
            var existedCategory = await _accountingContext.Category
                .FirstOrDefaultAsync(c => c.CategoryCode == data.CategoryCode || c.Title == data.Title);
            if (existedCategory != null)
            {
                if (string.Compare(existedCategory.CategoryCode, data.CategoryCode, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return CategoryErrorCode.CategoryCodeAlreadyExisted;
                }

                return CategoryErrorCode.CategoryTitleAlreadyExisted;
            }

            List<CategoryEntity> selectSubCategories = new List<CategoryEntity>();
            foreach (int subId in data.SubCategories.Where(s => s.CategoryId > 0).Select(s => s.CategoryId))
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
                else
                {
                    selectSubCategories.Add(subCategory);
                }
            }

            List<CategoryEntity> newSubCategories = new List<CategoryEntity>();
            foreach (var item in data.SubCategories.Where(s => s.CategoryId <= 0))
            {
                var existedsubCategory = await _accountingContext.Category
                    .FirstOrDefaultAsync(c => c.CategoryCode == item.CategoryCode || c.Title == item.Title);
                if (existedsubCategory != null)
                {
                    if (string.Compare(existedsubCategory.CategoryCode, data.CategoryCode, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return CategoryErrorCode.CategoryCodeAlreadyExisted;
                    }

                    return CategoryErrorCode.CategoryTitleAlreadyExisted;
                }
                else
                {
                    var subCategory = _mapper.Map<CategoryEntity>(item);
                    subCategory.IsModule = false;
                    subCategory.IsReadonly = false;
                    newSubCategories.Add(subCategory);
                }
            }

            using (var trans = await _accountingContext.Database.BeginTransactionAsync())
            {
                try
                {
                    CategoryEntity category = _mapper.Map<CategoryEntity>(data);
                    category.IsModule = true;
                    category.UpdatedByUserId = updatedUserId;
                    category.CreatedByUserId = updatedUserId;
                    await _accountingContext.Category.AddAsync(category);
                    await _accountingContext.SaveChangesAsync();
                    foreach (var selectSubCategory in selectSubCategories)
                    {
                        selectSubCategory.ParentId = category.CategoryId;
                        selectSubCategory.UpdatedByUserId = updatedUserId;
                        selectSubCategory.CreatedByUserId = updatedUserId;
                        await _accountingContext.SaveChangesAsync();
                    }
                    foreach (var newSubCategory in newSubCategories)
                    {
                        newSubCategory.ParentId = category.CategoryId;
                        newSubCategory.UpdatedByUserId = updatedUserId;
                        newSubCategory.CreatedByUserId = updatedUserId;
                        await _accountingContext.Category.AddAsync(newSubCategory);
                    }
                    await _accountingContext.SaveChangesAsync();
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
            if (category.CategoryCode != data.CategoryCode || category.Title != data.Title)
            {
                var existedCategory = await _accountingContext.Category
                    .FirstOrDefaultAsync(c => c.CategoryId != categoryId && (c.CategoryCode == data.CategoryCode || c.Title == data.Title));
                if (existedCategory != null)
                {
                    if (string.Compare(existedCategory.CategoryCode, data.CategoryCode, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return CategoryErrorCode.CategoryCodeAlreadyExisted;
                    }

                    return CategoryErrorCode.CategoryTitleAlreadyExisted;
                }
            }

            var deleteSubCategories = category.SubCategories.Where(c => !data.SubCategories.Any(s => s.CategoryId == c.CategoryId)).ToList();
            var newSubCategoryModels = data.SubCategories.Where(c => !category.SubCategories.Any(s => s.CategoryId == c.CategoryId)).ToList();

            List<CategoryEntity> selectSubCategories = new List<CategoryEntity>();
            foreach (int subId in newSubCategoryModels.Where(s => s.CategoryId > 0).Select(c => c.CategoryId))
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
                else
                {
                    selectSubCategories.Add(subCategory);
                }
            }

            List<CategoryEntity> newSubCategories = new List<CategoryEntity>();
            foreach (var item in data.SubCategories.Where(s => s.CategoryId <= 0))
            {
                var existedsubCategory = await _accountingContext.Category
                    .FirstOrDefaultAsync(c => c.CategoryCode == item.CategoryCode || c.Title == item.Title);
                if (existedsubCategory != null)
                {
                    if (string.Compare(existedsubCategory.CategoryCode, data.CategoryCode, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return CategoryErrorCode.SubCategoryCodeAlreadyExisted;
                    }

                    return CategoryErrorCode.SubCategoryTitleAlreadyExisted;
                }
                else
                {
                    var subCategory = _mapper.Map<CategoryEntity>(item);
                    subCategory.IsModule = false;
                    subCategory.IsReadonly = false;
                    newSubCategories.Add(subCategory);
                }
            }

            if (category.IsModule != data.IsModule && category.ParentId.HasValue)
            {
                return CategoryErrorCode.IsSubCategory;
            }

            using (var trans = await _accountingContext.Database.BeginTransactionAsync())
            {
                try
                {
                    category.CategoryCode = data.CategoryCode;
                    category.Title = data.Title;
                    category.IsModule = data.IsModule;
                    category.IsReadonly = data.IsReadonly;
                    category.IsTreeView = data.IsTreeView;
                    category.UpdatedByUserId = updatedUserId;
                    await _accountingContext.SaveChangesAsync();
                    foreach (var item in deleteSubCategories)
                    {
                        var subCategory = _accountingContext.Category.FirstOrDefault(c => c.CategoryId == item.CategoryId);
                        subCategory.ParentId = null;
                        subCategory.UpdatedByUserId = updatedUserId;
                        await _accountingContext.SaveChangesAsync();
                    }
                    foreach (var item in selectSubCategories)
                    {
                        item.ParentId = category.CategoryId;
                        item.UpdatedByUserId = updatedUserId;
                        await _accountingContext.SaveChangesAsync();
                    }
                    foreach (var newSubCategory in newSubCategories)
                    {
                        newSubCategory.ParentId = category.CategoryId;
                        newSubCategory.UpdatedByUserId = updatedUserId;
                        newSubCategory.CreatedByUserId = updatedUserId;
                        await _accountingContext.Category.AddAsync(newSubCategory);
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

        public async Task<Enum> DeleteCategory(int updatedUserId, int categoryId)
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
                    // Xóa category, field
                    var categoryIds = GetAllCategoryIds(categoryId);
                    foreach (var id in categoryIds)
                    {
                        category = _accountingContext.Category.FirstOrDefault(c => c.CategoryId == id);
                        category.IsDeleted = true;
                        category.UpdatedByUserId = updatedUserId;
                        await _accountingContext.SaveChangesAsync();
                        var deleteFields = _accountingContext.CategoryField.Where(f => f.CategoryId == category.CategoryId);
                        foreach (var field in deleteFields)
                        {
                            // Check có trường đang tham chiếu tới
                            if (_accountingContext.CategoryField.Any(f => f.ReferenceCategoryFieldId == field.CategoryFieldId))
                            {
                                trans.Rollback();
                                return CategoryErrorCode.DestCategoryFieldAlreadyExisted;
                            }
                            field.IsDeleted = true;
                            field.UpdatedByUserId = updatedUserId;
                            await _accountingContext.SaveChangesAsync();

                            // Xóa value
                            var categoryValues = _accountingContext.CategoryValue.Where(v => v.CategoryFieldId == field.CategoryFieldId);
                            foreach (var value in categoryValues)
                            {
                                value.IsDeleted = true;
                                value.UpdatedByUserId = updatedUserId;
                                await _accountingContext.SaveChangesAsync();
                            }
                        }
                    }
                    // Xóa row
                    var categoryRows = _accountingContext.CategoryRow.Where(r => r.CategoryId == categoryId);
                    foreach (var row in categoryRows)
                    {
                        row.IsDeleted = true;
                        row.UpdatedByUserId = updatedUserId;
                        await _accountingContext.SaveChangesAsync();

                        // Xóa mapping row, value
                        var categoryRowValues = _accountingContext.CategoryRowValue.Where(rv => rv.CategoryRowId == row.CategoryRowId);
                        foreach (var rowValue in categoryRowValues)
                        {
                            rowValue.IsDeleted = true;
                            rowValue.UpdatedByUserId = updatedUserId;
                            await _accountingContext.SaveChangesAsync();
                        }
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

        public async Task<PageData<DataTypeModel>> GetDataTypes(int page, int size)
        {
            var query = _accountingContext.DataType.OrderBy(d => d.Name).AsQueryable();
            var total = await query.CountAsync();
            if (size > 0)
            {
                query = query.Skip((page - 1) * size).Take(size);
            }
            List<DataTypeModel> lst = new List<DataTypeModel>();
            foreach (var item in query)
            {
                DataTypeModel dataTypeModel = _mapper.Map<DataTypeModel>(item);
                lst.Add(dataTypeModel);
            }
            return (lst, total);
        }
        public async Task<PageData<FormTypeModel>> GetFormTypes(int page, int size)
        {
            var query = _accountingContext.FormType.OrderBy(f => f.Name).AsQueryable();
            var total = await query.CountAsync();
            if (size > 0)
            {
                query = query.Skip((page - 1) * size).Take(size);
            }
            List<FormTypeModel> lst = new List<FormTypeModel>();
            foreach (var item in query)
            {
                FormTypeModel formTypeModel = _mapper.Map<FormTypeModel>(item);
                lst.Add(formTypeModel);
            }
            return (lst, total);
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
            var query = _accountingContext.CategoryField
                .Include(f => f.SourceCategoryField)
                .Where(f => f.CategoryId == categoryId)
                .Include(f => f.DataType)
                .Include(f => f.FormType)
                .OrderBy(f => f.SortOrder);
            List<CategoryFieldOutputModel> result = new List<CategoryFieldOutputModel>();
            foreach (var field in query)
            {
                var fieldModel = _mapper.Map<CategoryFieldOutputModel>(field);
                fieldModel.ReferenceCategoryId = field.SourceCategoryField?.CategoryId ?? null;

                result.Add(fieldModel);
            }

            return result;
        }

        public async Task<PageData<OperatorModel>> GetOperators(int page, int size)
        {
            List<OperatorModel> operators = new List<OperatorModel>();
            foreach (EnumOperator ope in (EnumOperator[])EnumOperator.GetValues(typeof(EnumOperator)))
            {
                operators.Add(new OperatorModel
                {
                    Value = (int)ope,
                    Title = ope.GetEnumDescription(),
                    ParamNumber = ope.GetParamNumber()
                }); ;
            }
            int total = operators.Count;
            if (size > 0)
            {
                operators = operators.Skip((page - 1) * size).Take(size).ToList();
            }
            return (operators, total);
        }
        public async Task<PageData<LogicOperatorModel>> GetLogicOperators(int page, int size)
        {
            List<LogicOperatorModel> operators = new List<LogicOperatorModel>();
            foreach (EnumLogicOperator ope in (EnumLogicOperator[])EnumLogicOperator.GetValues(typeof(EnumLogicOperator)))
            {
                operators.Add(new OperatorModel
                {
                    Value = (int)ope,
                    Title = ope.GetEnumDescription()
                }); ;
            }
            int total = operators.Count;
            if (size > 0)
            {
                operators = operators.Skip((page - 1) * size).Take(size).ToList();
            }
            return (operators, total);
        }

    }
}
