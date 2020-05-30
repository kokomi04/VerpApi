using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Verp.Cache.RedisCache;
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
    public class CategoryService : AccoutantBaseService, ICategoryService
    {
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;

        public CategoryService(AccountingDBContext accountingContext
            , IOptions<AppSetting> appSetting
            , ILogger<CategoryService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            ) : base(accountingContext, appSetting, mapper)
        {
            _logger = logger;
            _activityLogService = activityLogService;
        }

        public async Task<ServiceResult<CategoryFullModel>> GetCategory(int categoryId)
        {
            var category = await _accountingContext.Category
                .Include(c => c.OutSideDataConfig)
                .Include(c => c.CategoryArea)
                .ThenInclude(a => a.CategoryField)
                .ThenInclude(f => f.ReferenceCategoryField)
                .Include(c => c.CategoryArea)
                .ThenInclude(a => a.CategoryField)
                .ThenInclude(f => f.InverseReferenceCategoryTitleField)
                .ProjectTo<CategoryFullModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(c => c.CategoryId == categoryId);
            if (category == null)
            {
                return CategoryErrorCode.CategoryNotFound;
            }
            return category;
        }

        public async Task<PageData<CategoryModel>> GetCategories(string keyword, int page, int size)
        {
            keyword = (keyword ?? "").Trim();

            var query = _accountingContext.Category.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(c => c.CategoryCode.Contains(keyword) || c.Title.Contains(keyword));
            }
            query = query.OrderBy(c => c.Title);
            var total = await query.CountAsync();
            if (size > 0)
            {
                query = query.Skip((page - 1) * size).Take(size);
            }
            List<CategoryModel> lst = query.ProjectTo<CategoryModel>(_mapper.ConfigurationProvider).ToList();

            return (lst, total);
        }

        public async Task<ServiceResult<int>> AddCategory(CategoryModel data)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockCategoryKey(0));
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

            using var trans = await _accountingContext.Database.BeginTransactionAsync();
            try
            {
                CategoryEntity category = _mapper.Map<CategoryEntity>(data);
                await _accountingContext.Category.AddAsync(category);
                await _accountingContext.SaveChangesAsync();

                // Thêm Identity Area
                CategoryArea identityArea = new CategoryArea
                {
                    CategoryId = category.CategoryId,
                    CategoryAreaCode = AccountantConstants.IDENTITY_AREA,
                    Title = AccountantConstants.IDENTITY_AREA_TITLE,
                    SortOrder = 0,
                    CategoryAreaType = (int)EnumCategoryAreaType.Identity
                };
                await _accountingContext.CategoryArea.AddAsync(identityArea);
                await _accountingContext.SaveChangesAsync();

                // Thêm F_Identity
                CategoryField identityField = new CategoryField
                {
                    CategoryId = category.CategoryId,
                    CategoryFieldName = AccountantConstants.F_IDENTITY,
                    CategoryAreaId = identityArea.CategoryAreaId,
                    Title = AccountantConstants.F_IDENTITY,
                    FormTypeId = (int)EnumFormType.Input,
                    DataTypeId = (int)EnumDataType.Number,
                    DataSize = -1,
                    IsHidden = true,
                    IsRequired = false,
                    IsUnique = false,
                    IsShowSearchTable = false,
                    IsTreeViewKey = false,
                    IsShowList = false,
                    IsReadOnly = true
                };
                await _accountingContext.CategoryField.AddAsync(identityField);
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

        public async Task<Enum> UpdateCategory(int categoryId, CategoryModel data)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockCategoryKey(categoryId));
            var category = await _accountingContext.Category.FirstOrDefaultAsync(c => c.CategoryId == categoryId);
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

            using var trans = await _accountingContext.Database.BeginTransactionAsync();
            try
            {
                category.CategoryCode = data.CategoryCode;
                category.Title = data.Title;
                category.IsReadonly = data.IsReadonly;
                category.IsTreeView = data.IsTreeView;
                category.IsOutSideData = data.IsOutSideData;
                await _accountingContext.SaveChangesAsync();

                //Update config outside nếu là danh mục ngoài phân hệ
                if (category.IsOutSideData)
                {
                    OutSideDataConfig config = _accountingContext.OutSideDataConfig.FirstOrDefault(cf => cf.CategoryId == category.CategoryId);
                    if (config == null)
                    {
                        config = _mapper.Map<OutSideDataConfig>(data.OutSideDataConfig);
                        config.CategoryId = category.CategoryId;
                        await _accountingContext.OutSideDataConfig.AddAsync(config);
                    }
                    else
                    {
                        config.ModuleType = data.OutSideDataConfig.ModuleType;
                        config.Url = data.OutSideDataConfig.Url;
                        config.Key = data.OutSideDataConfig.Key;
                        config.Description = data.OutSideDataConfig.Description;
                    }
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

        public async Task<Enum> DeleteCategory(int categoryId)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockCategoryKey(categoryId));
            var category = await _accountingContext.Category.FirstOrDefaultAsync(c => c.CategoryId == categoryId);
            if (category == null)
            {
                return CategoryErrorCode.CategoryNotFound;
            }

            using var trans = await _accountingContext.Database.BeginTransactionAsync();
            try
            {
                // Xóa category, field

                category.IsDeleted = true;
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
                }


                // Xóa row
                var categoryRows = _accountingContext.CategoryRow.Where(r => r.CategoryId == categoryId);
                foreach (var row in categoryRows)
                {
                    row.IsDeleted = true;

                    // Xóa mapping row, value
                    var categoryRowValues = _accountingContext.CategoryRowValue.Where(rv => rv.CategoryRowId == row.CategoryRowId);
                    foreach (var rowValue in categoryRowValues)
                    {
                        rowValue.IsDeleted = true;
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

        public async Task<PageData<DataTypeModel>> GetDataTypes(int page, int size)
        {
            var query = _accountingContext.DataType.OrderBy(d => d.Name).AsQueryable();
            var total = await query.CountAsync();
            if (size > 0)
            {
                query = query.Skip((page - 1) * size).Take(size);
            }
            List<DataTypeModel> lst = query.ProjectTo<DataTypeModel>(_mapper.ConfigurationProvider).ToList();
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
            List<FormTypeModel> lst = query.ProjectTo<FormTypeModel>(_mapper.ConfigurationProvider).ToList();
            return (lst, total);
        }

        private ICollection<CategoryFieldOutputModel> GetFields(int categoryId)
        {
            var query = _accountingContext.CategoryField
                .Include(f => f.ReferenceCategoryField)
                .Where(f => f.CategoryId == categoryId)
                .OrderBy(f => f.SortOrder);
            List<CategoryFieldOutputModel> result = query.ProjectTo<CategoryFieldOutputModel>(_mapper.ConfigurationProvider).ToList();
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

        public async Task<PageData<ModuleTypeModel>> GetModuleTypes(int page, int size)
        {
            List<ModuleTypeModel> moduleTypes = new List<ModuleTypeModel>();
            foreach (EnumModuleType type in (EnumModuleType[])EnumModuleType.GetValues(typeof(EnumModuleType)))
            {
                moduleTypes.Add(new ModuleTypeModel
                {
                    ModuleTypeValue = (int)type,
                    ModuleTypeTitle = type.GetEnumDescription()
                }); ;
            }
            int total = moduleTypes.Count;
            if (size > 0)
            {
                moduleTypes = moduleTypes.Skip((page - 1) * size).Take(size).ToList();
            }
            return (moduleTypes, total);
        }
    }
}
