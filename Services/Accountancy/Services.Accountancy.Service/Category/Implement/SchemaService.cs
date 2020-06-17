using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verp.Cache.RedisCache;
using VErp.Commons.Constants;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.AccountancyDB;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Accountancy.Model.Category;
using CategoryEntity = VErp.Infrastructure.EF.AccountancyDB.Category;

namespace VErp.Services.Accountancy.Service.Category
{
    public class SchemaService : ISchemaService
    {
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly AppSetting _appSetting;
        private readonly IMapper _mapper;
        private readonly AccountancyDBContext _accountancyContext;

        public SchemaService(AccountancyDBContext accountancyContext
            , IOptions<AppSetting> appSetting
            , ILogger<SchemaService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            )
        {
            _logger = logger;
            _activityLogService = activityLogService;
            _accountancyContext = accountancyContext;
            _appSetting = appSetting.Value;
            _mapper = mapper;
        }

        #region Category
        public async Task<ServiceResult<CategoryFullModel>> GetCategory(int categoryId)
        {
            var category = await _accountancyContext.Category
                .Include(c => c.OutSideDataConfig)
                .Include(c => c.CategoryField)
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

            var query = _accountancyContext.Category.AsQueryable();

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
            var existedCategory = await _accountancyContext.Category
                .FirstOrDefaultAsync(c => c.CategoryCode == data.CategoryCode || c.Title == data.Title);
            if (existedCategory != null)
            {
                if (string.Compare(existedCategory.CategoryCode, data.CategoryCode, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return CategoryErrorCode.CategoryCodeAlreadyExisted;
                }

                return CategoryErrorCode.CategoryTitleAlreadyExisted;
            }

            using var trans = await _accountancyContext.Database.BeginTransactionAsync();
            try
            {
                CategoryEntity category = _mapper.Map<CategoryEntity>(data);
                await _accountancyContext.Category.AddAsync(category);
                await _accountancyContext.SaveChangesAsync();

                // Thêm F_Identity
                CategoryField identityField = new CategoryField
                {
                    CategoryId = category.CategoryId,
                    CategoryFieldName = AccountantConstants.F_IDENTITY,
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
                await _accountancyContext.CategoryField.AddAsync(identityField);
                await _accountancyContext.SaveChangesAsync();

                if (!category.IsOutSideData)
                {
                    // Create table
                    await _accountancyContext.ExecuteStoreProcedure("asp_Category_Table_Add", new[] {
                    new SqlParameter("@CategoryCode", category.CategoryCode ),
                    new SqlParameter("@IsTreeView", category.IsTreeView),
                    });
                }

                string tableName = category.IsOutSideData ? category.OutSideDataConfig.Url : string.Format("_{0}", category.CategoryCode);

                // Create view
                await _accountancyContext.ExecuteStoreProcedure("asp_Category_View_Update", new[] {
                    new SqlParameter("@CategoryCode", category.CategoryCode ),
                    new SqlParameter("@TableName", tableName ),
                    new SqlParameter("@IsTreeView", category.IsTreeView),
                    new SqlParameter("@IsOutSideData", category.IsOutSideData),
                    new SqlParameter("@Key", category.OutSideDataConfig?.Key??string.Empty),
                    new SqlParameter("@ParentKey", category.OutSideDataConfig?.ParentKey??string.Empty),
                    });

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
            var category = await _accountancyContext.Category.FirstOrDefaultAsync(c => c.CategoryId == categoryId);
            if (category == null)
            {
                return CategoryErrorCode.CategoryNotFound;
            }
            if (category.CategoryCode != data.CategoryCode || category.Title != data.Title)
            {
                var existedCategory = await _accountancyContext.Category
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

            using var trans = await _accountancyContext.Database.BeginTransactionAsync();
            try
            {
                // Rename table, view
                if(category.CategoryCode != data.CategoryCode)
                {
                    if (!category.IsOutSideData)
                    {
                        // Rename table
                        await _accountancyContext.ExecuteStoreProcedure("asp_Category_Rename", new[] {
                            new SqlParameter("@OldCategoryCode", category.CategoryCode ),
                            new SqlParameter("@NewCategoryCode", data.CategoryCode ),
                            new SqlParameter("@IsTable", true),
                        });
                    }
                    // Rename view
                    await _accountancyContext.ExecuteStoreProcedure("asp_Category_Rename", new[] {
                        new SqlParameter("@OldCategoryCode", category.CategoryCode ),
                        new SqlParameter("@NewCategoryCode", data.CategoryCode ),
                        new SqlParameter("@IsTable", false),
                    });
                }

                category.CategoryCode = data.CategoryCode;
                category.Title = data.Title;
                category.IsReadonly = data.IsReadonly;
                category.IsTreeView = data.IsTreeView;
                category.IsOutSideData = data.IsOutSideData;
                await _accountancyContext.SaveChangesAsync();

                //Update config outside nếu là danh mục ngoài phân hệ
                if (category.IsOutSideData)
                {
                    OutSideDataConfig config = _accountancyContext.OutSideDataConfig.FirstOrDefault(cf => cf.CategoryId == category.CategoryId);
       
                    if (config == null)
                    {
                        config = _mapper.Map<OutSideDataConfig>(data.OutSideDataConfig);
                        config.CategoryId = category.CategoryId;
                        await _accountancyContext.OutSideDataConfig.AddAsync(config);
                    }
                    else
                    {
                        config.ModuleType = data.OutSideDataConfig.ModuleType;
                        config.Url = data.OutSideDataConfig.Url;
                        config.Key = data.OutSideDataConfig.Key;
                        config.Description = data.OutSideDataConfig.Description;
                    }
                }

                string tableName = category.IsOutSideData ? category.OutSideDataConfig.Url : string.Format("_{0}", category.CategoryCode);

                // Update view
                await _accountancyContext.ExecuteStoreProcedure("asp_Category_View_Update", new[] {
                        new SqlParameter("@CategoryCode", category.CategoryCode ),
                        new SqlParameter("@TableName", tableName ),
                        new SqlParameter("@IsTreeView", category.IsTreeView),
                        new SqlParameter("@IsOutSideData", category.IsOutSideData),
                        new SqlParameter("@Key", category.OutSideDataConfig?.Key??string.Empty),
                        new SqlParameter("@ParentKey", category.OutSideDataConfig?.ParentKey??string.Empty),
                    });

                await _accountancyContext.SaveChangesAsync();
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
            var category = await _accountancyContext.Category.FirstOrDefaultAsync(c => c.CategoryId == categoryId);
            if (category == null)
            {
                return CategoryErrorCode.CategoryNotFound;
            }

            using var trans = await _accountancyContext.Database.BeginTransactionAsync();
            try
            {
                // Xóa category
                category.IsDeleted = true;

                // Xóa field
                var deleteFields = _accountancyContext.CategoryField.Where(f => f.CategoryId == category.CategoryId);
                foreach (var field in deleteFields)
                {
                    // Check có trường đang tham chiếu tới

                    field.IsDeleted = true;
                }

                // Delete table
                if (!category.IsOutSideData)
                {
                    // Create table
                    await _accountancyContext.ExecuteStoreProcedure("asp_Category_Delete", new[] {
                        new SqlParameter("@CategoryCode", category.CategoryCode),
                        new SqlParameter("@IsTable", true )
                    });
                }

                // Delete view
                await _accountancyContext.ExecuteStoreProcedure("asp_Category_Delete", new[] {
                        new SqlParameter("@CategoryCode", category.CategoryCode),
                        new SqlParameter("@IsTable", false )
                    });

                await _accountancyContext.SaveChangesAsync();
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

        #endregion

        #region Area

        //public async Task<ServiceResult<CategoryAreaModel>> GetCategoryArea(int categoryId, int categoryAreaId)
        //{
        //    var CategoryArea = await _accountancyContext.CategoryArea
        //        .Where(i => i.CategoryId == categoryId && i.CategoryAreaId == categoryAreaId)
        //        .ProjectTo<CategoryAreaModel>(_mapper.ConfigurationProvider)
        //        .FirstOrDefaultAsync();
        //    if (CategoryArea == null)
        //    {
        //        throw new BadRequestException(CategoryErrorCode.SubCategoryNotFound);
        //    }
        //    return CategoryArea;
        //}

        //public async Task<PageData<CategoryAreaModel>> GetCategoryAreas(int categoryId, string keyword, int page, int size)
        //{
        //    keyword = (keyword ?? "").Trim();
        //    var query = _accountancyContext.CategoryArea.Where(a => a.CategoryId == categoryId).AsQueryable();
        //    if (!string.IsNullOrEmpty(keyword))
        //    {
        //        query = query.Where(a => a.CategoryAreaCode.Contains(keyword) || a.Title.Contains(keyword));
        //    }
        //    query = query.OrderBy(c => c.Title);
        //    var total = await query.CountAsync();
        //    if (size > 0)
        //    {
        //        query = query.Skip((page - 1) * size).Take(size);
        //    }
        //    var lst = await query.ProjectTo<CategoryAreaModel>(_mapper.ConfigurationProvider).OrderBy(a => a.SortOrder).ToListAsync();
        //    return (lst, total);
        //}

        //public async Task<ServiceResult<int>> AddCategoryArea(int categoryId, CategoryAreaInputModel data)
        //{
        //    using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockCategoryKey(categoryId));
        //    var existedCategory = await _accountancyContext.CategoryArea
        //        .FirstOrDefaultAsync(a => a.CategoryId == categoryId && (a.CategoryAreaCode == data.CategoryAreaCode || a.Title == data.Title));
        //    if (existedCategory != null)
        //    {
        //        if (string.Compare(existedCategory.CategoryAreaCode, data.CategoryAreaCode, StringComparison.OrdinalIgnoreCase) == 0)
        //        {
        //            throw new BadRequestException(CategoryErrorCode.CategoryCodeAlreadyExisted);
        //        }
        //        throw new BadRequestException(CategoryErrorCode.CategoryTitleAlreadyExisted);
        //    }

        //    using (var trans = await _accountancyContext.Database.BeginTransactionAsync())
        //    {
        //        try
        //        {
        //            CategoryArea CategoryArea = _mapper.Map<CategoryArea>(data);
        //            CategoryArea.CategoryId = categoryId;
        //            await _accountancyContext.CategoryArea.AddAsync(CategoryArea);
        //            await _accountancyContext.SaveChangesAsync();

        //            trans.Commit();
        //            await _activityLogService.CreateLog(EnumObjectType.Category, CategoryArea.CategoryAreaId, $"Thêm vùng thông tin {CategoryArea.Title}", data.JsonSerialize());
        //            return CategoryArea.CategoryAreaId;
        //        }
        //        catch (Exception ex)
        //        {
        //            trans.Rollback();
        //            _logger.LogError(ex, "Create");
        //            return GeneralCode.InternalError;
        //        }
        //    }
        //}

        //public async Task<Enum> UpdateCategoryArea(int categoryId, int categoryAreaId, CategoryAreaInputModel data)
        //{
        //    using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockCategoryKey(categoryId));
        //    var categoryArea = await _accountancyContext.CategoryArea.FirstOrDefaultAsync(a => a.CategoryId == categoryId && a.CategoryAreaId == categoryAreaId);
        //    if (categoryArea == null)
        //    {
        //        throw new BadRequestException(CategoryErrorCode.SubCategoryNotFound);
        //    }
        //    if (categoryArea.CategoryAreaCode != data.CategoryAreaCode || categoryArea.Title != data.Title)
        //    {
        //        var existedCategory = await _accountancyContext.CategoryArea
        //            .FirstOrDefaultAsync(a => a.CategoryId == categoryId && a.CategoryAreaId != categoryAreaId && (a.CategoryAreaCode == data.CategoryAreaCode || a.Title == data.Title));
        //        if (existedCategory != null)
        //        {
        //            if (string.Compare(existedCategory.CategoryAreaCode, data.CategoryAreaCode, StringComparison.OrdinalIgnoreCase) == 0)
        //            {
        //                throw new BadRequestException(CategoryErrorCode.SubCategoryCodeAlreadyExisted);
        //            }

        //            throw new BadRequestException(CategoryErrorCode.SubCategoryTitleAlreadyExisted);
        //        }
        //    }

        //    using var trans = await _accountancyContext.Database.BeginTransactionAsync();
        //    try
        //    {
        //        categoryArea.CategoryAreaCode = data.CategoryAreaCode;
        //        categoryArea.Title = data.Title;
        //        categoryArea.SortOrder = data.SortOrder;
        //        categoryArea.CategoryAreaType = (int)data.CategoryAreaType;
        //        await _accountancyContext.SaveChangesAsync();

        //        trans.Commit();
        //        await _activityLogService.CreateLog(EnumObjectType.Category, categoryArea.CategoryAreaId, $"Cập nhật vùng dữ liệu {categoryArea.Title}", data.JsonSerialize());
        //        return GeneralCode.Success;
        //    }
        //    catch (Exception ex)
        //    {
        //        trans.Rollback();
        //        _logger.LogError(ex, "Update");
        //        return GeneralCode.InternalError;
        //    }
        //}

        //public async Task<Enum> DeleteCategoryArea(int categoryId, int categoryAreaId)
        //{
        //    using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockCategoryKey(categoryId));
        //    var categoryArea = await _accountancyContext.CategoryArea.FirstOrDefaultAsync(a => a.CategoryId == categoryId && a.CategoryAreaId == categoryAreaId);
        //    if (categoryArea == null)
        //    {
        //        throw new BadRequestException(CategoryErrorCode.SubCategoryNotFound);
        //    }

        //    using var trans = await _accountancyContext.Database.BeginTransactionAsync();
        //    try
        //    {
        //        // Xóa field
        //        List<CategoryField> CategoryAreaFields = _accountancyContext.CategoryField.Where(f => f.CategoryAreaId == categoryAreaId).ToList();
        //        foreach (CategoryField categoryField in CategoryAreaFields)
        //        {
        //            categoryField.IsDeleted = true;
        //            await _accountancyContext.SaveChangesAsync();
        //        }
        //        // Xóa area
        //        categoryArea.IsDeleted = true;
        //        await _accountancyContext.SaveChangesAsync();
        //        trans.Commit();
        //        await _activityLogService.CreateLog(EnumObjectType.Category, categoryArea.CategoryAreaId, $"Xóa chứng từ {categoryArea.Title}", categoryArea.JsonSerialize());
        //        return GeneralCode.Success;

        //    }
        //    catch (Exception ex)
        //    {
        //        trans.Rollback();
        //        _logger.LogError(ex, "Delete");
        //        return GeneralCode.InternalError;
        //    }
        //}

        #endregion

        #region Field
        public async Task<PageData<CategoryFieldOutputModel>> GetCategoryFields(int categoryId, string keyword, int page, int size)
        {
            keyword = (keyword ?? "").Trim();
            var query = _accountancyContext.CategoryField
                //.Include(f => f.ReferenceCategoryField)
                //.Include(f => f.InverseReferenceCategoryTitleField)
                .AsQueryable();

            query = query.Where(c => categoryId == c.CategoryId);
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
                .ProjectTo<CategoryFieldOutputModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return (lst, total);
        }

        public async Task<List<CategoryFieldOutputModel>> GetCategoryFields(IList<int> categoryIds)
        {
            var query = _accountancyContext.CategoryField
                //.Include(f => f.ReferenceCategoryField)
                //.Include(f => f.InverseReferenceCategoryTitleField)
                .AsQueryable();
            query = query.Where(c => categoryIds.Contains(c.CategoryId));
            query = query.OrderBy(c => c.SortOrder);
            List<CategoryFieldOutputModel> lst = await query
                .OrderBy(f => f.SortOrder)
                .ProjectTo<CategoryFieldOutputModel>(_mapper.ConfigurationProvider)
                .ToListAsync();
            return lst;
        }

        public async Task<ServiceResult<CategoryFieldOutputModel>> GetCategoryField(int categoryId, int categoryFieldId)
        {
            var categoryField = await _accountancyContext.CategoryField
                //.Include(f => f.ReferenceCategoryField)
                //.Include(f => f.ReferenceCategoryTitleField)
                .ProjectTo<CategoryFieldOutputModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(c => c.CategoryFieldId == categoryFieldId && c.CategoryId == categoryId);
            if (categoryField == null)
            {
                throw new BadRequestException(CategoryErrorCode.CategoryFieldNotFound);
            }
            return categoryField;
        }

        public async Task<ServiceResult<int>> AddCategoryField(int categoryId, CategoryFieldInputModel data)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockCategoryKey(categoryId));
            ValidateExistedCategory(categoryId, data.CategoryAreaId);
            ValidateCategoryField(data);
            FieldDataProcess(ref data);
            using var trans = await _accountancyContext.Database.BeginTransactionAsync();
            try
            {
                var categoryField = _mapper.Map<CategoryField>(data);
                categoryField.CategoryId = categoryId;

                await _accountancyContext.CategoryField.AddAsync(categoryField);
                await _accountancyContext.SaveChangesAsync();

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

        public async Task<Enum> UpdateCategoryField(int categoryId, int categoryFieldId, CategoryFieldInputModel data)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockCategoryKey(categoryId));
            ValidateExistedCategory(categoryId, data.CategoryAreaId);
            var categoryField = await _accountancyContext.CategoryField.FirstOrDefaultAsync(f => f.CategoryFieldId == categoryFieldId);
            ValidateCategoryField(data, categoryField, categoryFieldId);
            FieldDataProcess(ref data);
            using var trans = await _accountancyContext.Database.BeginTransactionAsync();
            try
            {
                UpdateField(ref categoryField, data);
                await _accountancyContext.SaveChangesAsync();
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

        private void UpdateField(ref CategoryField categoryField, CategoryFieldInputModel data)
        {
            categoryField.CategoryFieldName = data.CategoryFieldName;
            //categoryField.CategoryAreaId = data.CategoryAreaId;
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
            categoryField.IsShowSearchTable = data.IsShowSearchTable;
            categoryField.IsTreeViewKey = data.IsTreeViewKey;
            categoryField.RegularExpression = data.RegularExpression;
            categoryField.Filters = data.Filters;
            //categoryField.ReferenceCategoryFieldId = data.ReferenceCategoryFieldId;
            //categoryField.ReferenceCategoryTitleFieldId = data.ReferenceCategoryTitleFieldId;
        }

        private void ValidateExistedCategory(int categoryId, int categoryAreaId)
        {
            // Check category
            if (!_accountancyContext.Category.Any(c => c.CategoryId == categoryId))
            {
                throw new BadRequestException(CategoryErrorCode.CategoryNotFound);
            }
            //if (!_accountancyContext.CategoryArea.Any(a => a.CategoryId == categoryId && a.CategoryAreaId == categoryAreaId))
            //{
            //    throw new BadRequestException(CategoryErrorCode.SubCategoryNotFound);
            //}
        }

        private void ValidateCategoryField(CategoryFieldInputModel data, CategoryField categoryField = null, int? categoryFieldId = null)
        {
            bool updateFieldName = true;
            if (categoryFieldId.HasValue && categoryFieldId.Value > 0)
            {
                if (categoryField == null)
                {
                    throw new BadRequestException(CategoryErrorCode.CategoryFieldNotFound);
                }
                updateFieldName = categoryField.CategoryFieldName == data.CategoryFieldName;
            }
            if (updateFieldName && _accountancyContext.CategoryField.Any(f => (!categoryFieldId.HasValue || f.CategoryFieldId != categoryFieldId.Value) && f.CategoryFieldId == data.CategoryFieldId && f.CategoryFieldName == data.CategoryFieldName))
            {
                throw new BadRequestException(CategoryErrorCode.CategoryFieldNameAlreadyExisted);
            }
            if (data.ReferenceCategoryFieldId.HasValue)
            {
                var sourceCategoryField = _accountancyContext.CategoryField.FirstOrDefault(f => f.CategoryFieldId == data.ReferenceCategoryFieldId.Value);
                if (sourceCategoryField == null)
                {
                    throw new BadRequestException(CategoryErrorCode.SourceCategoryFieldNotFound);
                }
            }
        }

        private void FieldDataProcess(ref CategoryFieldInputModel data)
        {
            if (data.ReferenceCategoryFieldId.HasValue)
            {
                int referId = data.ReferenceCategoryFieldId.Value;
                var sourceCategoryField = _accountancyContext.CategoryField.FirstOrDefault(f => f.CategoryFieldId == referId);
                data.DataTypeId = sourceCategoryField.DataTypeId;
                data.DataSize = sourceCategoryField.DataSize;
            }
            if (data.FormTypeId == (int)EnumFormType.Generate)
            {
                data.DataTypeId = (int)EnumDataType.Text;
                data.DataSize = 0;
            }
            if (!AccountantConstants.SELECT_FORM_TYPES.Contains((EnumFormType)data.FormTypeId))
            {
                data.ReferenceCategoryFieldId = null;
                data.ReferenceCategoryTitleFieldId = null;
            }
        }

        public async Task<ServiceResult<int>> UpdateMultiField(int categoryId, List<CategoryFieldInputModel> fields)
        {
            using var trans = await _accountancyContext.Database.BeginTransactionAsync();
            try
            {
                // Validate trùng name trong danh sách
                if (fields.Select(f => new { f.CategoryAreaId, f.CategoryFieldName }).Distinct().Count() != fields.Count)
                {
                    throw new BadRequestException(CategoryErrorCode.CategoryFieldNameAlreadyExisted);
                }

                var groups = fields.GroupBy(f => new { f.CategoryAreaId, f.CategoryFieldName });
                foreach (var group in groups)
                {
                    ValidateExistedCategory(categoryId, group.Key.CategoryAreaId);

                    for (int indx = 0; indx < group.Count(); indx++)
                    {
                        var data = group.ElementAt(indx);
                        var categoryAreaField = data.CategoryFieldId > 0 ? _accountancyContext.CategoryField.FirstOrDefault(f => f.CategoryFieldId == data.CategoryFieldId) : null;
                        ValidateCategoryField(data, categoryAreaField, data.CategoryFieldId);

                        FieldDataProcess(ref data);
                        if (data.CategoryFieldId > 0)
                        {
                            // Update
                            UpdateField(ref categoryAreaField, data);
                        }
                        else
                        {
                            // Create new
                            var categoryField = _mapper.Map<CategoryField>(data);
                            categoryField.CategoryId = categoryId;
                            await _accountancyContext.CategoryField.AddAsync(categoryField);
                            await _accountancyContext.SaveChangesAsync();
                        }
                    }
                }
                await _accountancyContext.SaveChangesAsync();
                trans.Commit();
                await _activityLogService.CreateLog(EnumObjectType.Category, categoryId, $"Cập nhật nhiều trường dữ liệu", fields.JsonSerialize());
                return categoryId;
            }
            catch (Exception ex)
            {
                trans.Rollback();
                _logger.LogError(ex, "Create");
                return GeneralCode.InternalError;
            }
        }


        public async Task<Enum> DeleteCategoryField(int categoryId, int categoryFieldId)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockCategoryKey(categoryId));
            var categoryField = await _accountancyContext.CategoryField.FirstOrDefaultAsync(c => c.CategoryFieldId == categoryFieldId && c.CategoryId == categoryId);
            if (categoryField == null)
            {
                throw new BadRequestException(CategoryErrorCode.CategoryFieldNotFound);
            }

            // Check reference
            //bool isRefer = await _accountancyContext.CategoryField.AnyAsync(c => c.ReferenceCategoryFieldId == categoryFieldId);
            //if (isRefer)
            //{
            //    throw new BadRequestException(CategoryErrorCode.DestCategoryFieldAlreadyExisted);
            //}

            using var trans = await _accountancyContext.Database.BeginTransactionAsync();
            try
            {
                // Delete row-field-value
                //var rowFieldValues = _accountancyContext.CategoryRowValue.Where(rfv => rfv.CategoryFieldId == categoryFieldId);
                //foreach (var rowFieldValue in rowFieldValues)
                //{
                //    rowFieldValue.IsDeleted = true;
                //}
                // Delete field
                categoryField.IsDeleted = true;
                await _accountancyContext.SaveChangesAsync();
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
        #endregion
    }
}
