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
using VErp.Commons.Library.Model;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Model.Category;
using VErp.Services.Master.Model.CategoryConfig;
using CategoryEntity = VErp.Infrastructure.EF.MasterDB.Category;
using VErp.Commons.GlobalObject.InternalDataInterface;

namespace VErp.Services.Master.Service.Category
{
    public class CategoryConfigService : ICategoryConfigService
    {
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly AppSetting _appSetting;
        private readonly IMapper _mapper;
        private readonly MasterDBContext _masterContext;

        public CategoryConfigService(MasterDBContext accountancyContext
            , IOptions<AppSetting> appSetting
            , ILogger<CategoryConfigService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            )
        {
            _logger = logger;
            _activityLogService = activityLogService;
            _masterContext = accountancyContext;
            _appSetting = appSetting.Value;
            _mapper = mapper;
        }

        #region Category
        public async Task<CategoryFullModel> GetCategory(int categoryId)
        {
            var category = await _masterContext.Category
                .Include(c => c.OutSideDataConfig)
                .ThenInclude(o => o.OutsideDataFieldConfig)
                .Include(c => c.CategoryField)
                .ProjectTo<CategoryFullModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(c => c.CategoryId == categoryId);
            if (category == null)
            {
                throw new BadRequestException(CategoryErrorCode.CategoryNotFound);
            }
            return category;
        }

        public async Task<CategoryFullModel> GetCategory(string categoryCode)
        {
            var category = await _masterContext.Category
                .Include(c => c.OutSideDataConfig)
                .ThenInclude(o => o.OutsideDataFieldConfig)
                .Include(c => c.CategoryField)
                .ProjectTo<CategoryFullModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(c => c.CategoryCode == categoryCode);
            if (category == null)
            {
                throw new BadRequestException(CategoryErrorCode.CategoryNotFound);
            }
            return category;
        }

        public async Task<PageData<CategoryModel>> GetCategories(string keyword, int page, int size)
        {
            keyword = (keyword ?? "").Trim();

            var query = _masterContext.Category.AsQueryable();

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

        public async Task<int> AddCategory(CategoryModel data)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockCategoryKey(0));
            var existedCategory = await _masterContext.Category
                .FirstOrDefaultAsync(c => c.CategoryCode == data.CategoryCode || c.Title == data.Title);
            if (existedCategory != null)
            {
                if (string.Compare(existedCategory.CategoryCode, data.CategoryCode, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    throw new BadRequestException(CategoryErrorCode.CategoryCodeAlreadyExisted);
                }

                throw new BadRequestException(CategoryErrorCode.CategoryTitleAlreadyExisted);
            }

            using var trans = await _masterContext.Database.BeginTransactionAsync();
            try
            {
                CategoryEntity category = _mapper.Map<CategoryEntity>(data);
                await _masterContext.Category.AddAsync(category);
                await _masterContext.SaveChangesAsync();

                // Thêm F_Identity
                CategoryField identityField = new CategoryField
                {
                    CategoryId = category.CategoryId,
                    CategoryFieldName = AccountantConstants.F_IDENTITY,
                    Title = AccountantConstants.F_IDENTITY,
                    FormTypeId = (int)EnumFormType.Input,
                    DataTypeId = (int)EnumDataType.Int,
                    DataSize = -1,
                    IsHidden = true,
                    IsRequired = false,
                    IsUnique = false,
                    IsShowSearchTable = false,
                    IsTreeViewKey = false,
                    IsShowList = false,
                    IsReadOnly = true
                };

                await _masterContext.CategoryField.AddAsync(identityField);
                await _masterContext.SaveChangesAsync();

                if (!category.IsOutSideData)
                {
                    // Create table
                    await _masterContext.ExecuteStoreProcedure("asp_Category_Table_Add", new[] {
                        new SqlParameter("@CategoryCode", category.CategoryCode ),
                        new SqlParameter("@IsTreeView", category.IsTreeView),
                    });
                }

                string tableName = category.IsOutSideData ? category.OutSideDataConfig.Url : category.CategoryCode;

                // Create view
                await _masterContext.ExecuteStoreProcedure("asp_Category_View_Update", new[] {
                    new SqlParameter("@CategoryCode", category.CategoryCode ),
                    new SqlParameter("@TableName", tableName ),
                    new SqlParameter("@IsTreeView", category.IsTreeView),
                    new SqlParameter("@IsOutSideData", category.IsOutSideData),
                    new SqlParameter("@Key", category.OutSideDataConfig?.Key??string.Empty),
                    new SqlParameter("@ParentKey", category.OutSideDataConfig?.ParentKey??string.Empty),
                    new SqlParameter("@UsePlace", category.UsePlace??string.Empty)
                    });

                trans.Commit();
                await _activityLogService.CreateLog(EnumObjectType.Category, category.CategoryId, $"Thêm danh mục {category.Title}", data.JsonSerialize());
                return category.CategoryId;
            }
            catch (Exception)
            {
                trans.TryRollbackTransaction();
                throw;
            }
        }

        public async Task<bool> UpdateCategory(int categoryId, CategoryModel data)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockCategoryKey(categoryId));
            var category = await _masterContext.Category.FirstOrDefaultAsync(c => c.CategoryId == categoryId);
            if (category == null)
            {
                throw new BadRequestException(CategoryErrorCode.CategoryNotFound);
            }
            if (category.CategoryCode != data.CategoryCode || category.Title != data.Title)
            {
                var existedCategory = await _masterContext.Category
                    .FirstOrDefaultAsync(c => c.CategoryId != categoryId && (c.CategoryCode == data.CategoryCode || c.Title == data.Title));
                if (existedCategory != null)
                {
                    if (string.Compare(existedCategory.CategoryCode, data.CategoryCode, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        throw new BadRequestException(CategoryErrorCode.CategoryCodeAlreadyExisted);
                    }

                    throw new BadRequestException(CategoryErrorCode.CategoryTitleAlreadyExisted);
                }
            }

            using var trans = await _masterContext.Database.BeginTransactionAsync();
            try
            {
                // Rename table, view
                if (category.CategoryCode != data.CategoryCode)
                {
                    if (!category.IsOutSideData)
                    {
                        // Rename table
                        await _masterContext.ExecuteStoreProcedure("asp_Category_Rename", new[] {
                            new SqlParameter("@OldCategoryCode", category.CategoryCode ),
                            new SqlParameter("@NewCategoryCode", data.CategoryCode ),
                            new SqlParameter("@IsTable", true),
                        });
                    }
                    // Rename view
                    await _masterContext.ExecuteStoreProcedure("asp_Category_Rename", new[] {
                        new SqlParameter("@OldCategoryCode", category.CategoryCode ),
                        new SqlParameter("@NewCategoryCode", data.CategoryCode ),
                        new SqlParameter("@IsTable", false),
                    });
                }
                category.CategoryCode = data.CategoryCode;

                // Change IsOutSideData
                if (category.IsOutSideData != data.IsOutSideData)
                {
                    if (data.IsOutSideData)
                    {
                        // Delete category table
                        await _masterContext.ExecuteStoreProcedure("asp_Category_Delete", new[] {
                            new SqlParameter("@CategoryCode", data.CategoryCode ),
                            new SqlParameter("@IsTable", true),
                        });
                    }
                    else
                    {
                        // Create category table
                        await _masterContext.ExecuteStoreProcedure("asp_Category_Table_Add", new[] {
                            new SqlParameter("@CategoryCode", data.CategoryCode ),
                            new SqlParameter("@IsTreeView", category.IsTreeView),
                        });
                    }
                }
                category.IsOutSideData = data.IsOutSideData;

                // Change IsTreeView
                if (category.IsTreeView != data.IsTreeView)
                {
                    if (data.IsTreeView)
                    {
                        // Create ParentId Field
                        await _masterContext.AddColumn(data.CategoryCode, "ParentId", EnumDataType.Int, -1, 0, null, true);
                    }
                    else
                    {
                        // Drop ParentId Field
                        await _masterContext.DeleteColumn(data.CategoryCode, "ParentId");
                    }

                }
                category.IsTreeView = data.IsTreeView;

                // Update other info
                category.Title = data.Title;
                category.IsReadonly = data.IsReadonly;
                category.UsePlace = data.UsePlace;
                await _masterContext.SaveChangesAsync();

                //Update config outside nếu là danh mục ngoài phân hệ
                if (category.IsOutSideData)
                {
                    OutSideDataConfig config = _masterContext.OutSideDataConfig
                        .Include(o => o.OutsideDataFieldConfig)
                        .FirstOrDefault(cf => cf.CategoryId == category.CategoryId);

                    if (config == null)
                    {
                        config = _mapper.Map<OutSideDataConfig>(data.OutSideDataConfig);
                        config.CategoryId = category.CategoryId;
                        await _masterContext.OutSideDataConfig.AddAsync(config);
                    }
                    else
                    {
                        config.ModuleType = data.OutSideDataConfig.ModuleType;
                        config.Url = data.OutSideDataConfig.Url;
                        config.ParentKey = data.OutSideDataConfig.ParentKey;
                        config.Key = data.OutSideDataConfig.Key;
                        config.Description = data.OutSideDataConfig.Description;
                        config.Joins = data.OutSideDataConfig.Joins;
                        // Update config fields
                        var deletedFields = config.OutsideDataFieldConfig.Where(f => !data.OutSideDataConfig.OutsideDataFieldConfig.Any(nf => nf.OutsideDataFieldConfigId == f.OutsideDataFieldConfigId)).ToList();
                        var newFields = data.OutSideDataConfig.OutsideDataFieldConfig.Where(nf => nf.OutsideDataFieldConfigId == 0).ToList();
                        var updatedFields = data.OutSideDataConfig.OutsideDataFieldConfig.Where(nf => nf.OutsideDataFieldConfigId != 0).ToList();
                        foreach (var deletedField in deletedFields)
                        {
                            deletedField.IsDeleted = true;
                        }
                        foreach (var newField in newFields)
                        {
                            var field = _mapper.Map<OutsideDataFieldConfig>(newField);
                            config.OutsideDataFieldConfig.Add(field);
                        }
                        foreach (var updatedField in updatedFields)
                        {
                            var curField = config.OutsideDataFieldConfig.FirstOrDefault(f => f.OutsideDataFieldConfigId == updatedField.OutsideDataFieldConfigId);
                            if (curField == null) continue;
                            curField.Value = updatedField.Value;
                            curField.Alias = updatedField.Alias;
                        }

                    }
                }
                await _masterContext.SaveChangesAsync();

                string tableName = category.IsOutSideData ? category.OutSideDataConfig.Url : category.CategoryCode;

                // Update view
                await _masterContext.ExecuteStoreProcedure("asp_Category_View_Update", new[] {
                        new SqlParameter("@CategoryCode", category.CategoryCode ),
                        new SqlParameter("@TableName", tableName ),
                        new SqlParameter("@IsTreeView", category.IsTreeView),
                        new SqlParameter("@IsOutSideData", category.IsOutSideData),
                        new SqlParameter("@Key", category.OutSideDataConfig?.Key??string.Empty),
                        new SqlParameter("@ParentKey", category.OutSideDataConfig?.ParentKey??string.Empty),
                        new SqlParameter("@UsePlace", category.UsePlace??string.Empty)
                    });
                trans.Commit();
                await _activityLogService.CreateLog(EnumObjectType.Category, category.CategoryId, $"Cập nhật danh mục {category.Title}", data.JsonSerialize());
                return true;
            }
            catch (Exception)
            {
                trans.TryRollbackTransaction();
                throw;
            }
        }

        public async Task<bool> DeleteCategory(int categoryId)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockCategoryKey(categoryId));
            var category = await _masterContext.Category.FirstOrDefaultAsync(c => c.CategoryId == categoryId);
            if (category == null)
            {
                throw new BadRequestException(CategoryErrorCode.CategoryNotFound);
            }

            using var trans = await _masterContext.Database.BeginTransactionAsync();
            try
            {
                // Xóa category
                category.IsDeleted = true;

                // Xóa field
                var deleteFields = _masterContext.CategoryField.Where(f => f.CategoryId == category.CategoryId);
                foreach (var field in deleteFields)
                {
                    // Check có trường đang tham chiếu tới

                    field.IsDeleted = true;
                }

                // Delete table
                if (!category.IsOutSideData)
                {
                    // Create table
                    await _masterContext.ExecuteStoreProcedure("asp_Category_Delete", new[] {
                        new SqlParameter("@CategoryCode", category.CategoryCode),
                        new SqlParameter("@IsTable", true )
                    });
                }

                // Delete view
                await _masterContext.ExecuteStoreProcedure("asp_Category_Delete", new[] {
                        new SqlParameter("@CategoryCode", category.CategoryCode),
                        new SqlParameter("@IsTable", false )
                    });

                await _masterContext.SaveChangesAsync();
                trans.Commit();
                await _activityLogService.CreateLog(EnumObjectType.Category, category.CategoryId, $"Xóa danh mục {category.Title}", category.JsonSerialize());
                return true;
            }
            catch (Exception)
            {
                trans.TryRollbackTransaction();
                throw;
            }
        }

        #endregion

        #region Field
        public async Task<PageData<CategoryFieldModel>> GetCategoryFields(int categoryId, string keyword, int page, int size)
        {
            keyword = (keyword ?? "").Trim();
            var query = _masterContext.CategoryField
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
            List<CategoryFieldModel> lst = await query
                .OrderBy(f => f.SortOrder)
                .ProjectTo<CategoryFieldModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return (lst, total);
        }

        public async Task<List<CategoryFieldModel>> GetCategoryFields(IList<int> categoryIds)
        {
            var query = _masterContext.CategoryField
                .AsQueryable();
            query = query.Where(c => categoryIds.Contains(c.CategoryId));
            query = query.OrderBy(c => c.SortOrder);
            List<CategoryFieldModel> lst = await query
                .OrderBy(f => f.SortOrder)
                .ProjectTo<CategoryFieldModel>(_mapper.ConfigurationProvider)
                .ToListAsync();
            return lst;
        }

        public async Task<CategoryFieldModel> GetCategoryField(int categoryId, int categoryFieldId)
        {
            var categoryField = await _masterContext.CategoryField
                .ProjectTo<CategoryFieldModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(c => c.CategoryFieldId == categoryFieldId && c.CategoryId == categoryId);
            if (categoryField == null)
            {
                throw new BadRequestException(CategoryErrorCode.CategoryFieldNotFound);
            }
            return categoryField;
        }

        private void UpdateField(ref CategoryField categoryField, CategoryFieldModel data)
        {
            if (!((EnumDataType)categoryField.DataTypeId).Convertible((EnumDataType)data.DataTypeId))
            {
                throw new BadRequestException(GeneralCode.InvalidParams, $"Không thể chuyển đổi kiểu dữ liệu từ {((EnumDataType)categoryField.DataTypeId).GetEnumDescription()} sang {((EnumDataType)data.DataTypeId).GetEnumDescription()}");
            }
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
            categoryField.IsShowSearchTable = data.IsShowSearchTable;
            categoryField.IsTreeViewKey = data.IsTreeViewKey;
            categoryField.RegularExpression = data.RegularExpression;
            categoryField.Filters = data.Filters;
            categoryField.DecimalPlace = data.DecimalPlace;
            categoryField.DefaultValue = data.DefaultValue;
            categoryField.RefTableCode = data.RefTableCode;
            categoryField.RefTableField = data.RefTableField;
            categoryField.RefTableTitle = data.RefTableTitle;
        }

        private void ValidateCategoryField(CategoryFieldModel data, CategoryField categoryField = null, int? categoryFieldId = null)
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
            if (updateFieldName && _masterContext.CategoryField.Any(f => (!categoryFieldId.HasValue || f.CategoryFieldId != categoryFieldId.Value) && f.CategoryFieldId == data.CategoryFieldId && f.CategoryFieldName == data.CategoryFieldName))
            {
                throw new BadRequestException(CategoryErrorCode.CategoryFieldNameAlreadyExisted);
            }
            if (!string.IsNullOrEmpty(data.RefTableCode) && ((EnumFormType)data.FormTypeId).IsSelectForm())
            {
                string refTable = data.RefTableCode;
                string refField = data.RefTableField;
                var sourceCategoryField = (from f in _masterContext.CategoryField
                                           join c in _masterContext.Category on f.CategoryId equals c.CategoryId
                                           where f.CategoryFieldName == refField && c.CategoryCode == refTable
                                           select f).FirstOrDefault();

                if (sourceCategoryField == null)
                {
                    throw new BadRequestException(CategoryErrorCode.SourceCategoryFieldNotFound);
                }
            }
        }

        private void FieldDataProcess(ref CategoryFieldModel data)
        {
            string refTable = data.RefTableCode;
            string refField = data.RefTableField;
            if (!string.IsNullOrEmpty(data.RefTableCode) && ((EnumFormType)data.FormTypeId).IsJoinForm())
            {
                var sourceCategoryField = (from f in _masterContext.CategoryField
                                           join c in _masterContext.Category on f.CategoryId equals c.CategoryId
                                           where f.CategoryFieldName == refField && c.CategoryCode == refTable
                                           select f).FirstOrDefault();
                if (sourceCategoryField == null)
                {
                    throw new BadRequestException(GeneralCode.ItemNotFound, $"Không tìm thấy trường dữ liệu liên kết {refTable}.{refField}");
                }
                data.DataTypeId = sourceCategoryField.DataTypeId;
                data.DataSize = sourceCategoryField.DataSize;
            }
            if (data.FormTypeId == (int)EnumFormType.Generate)
            {
                data.DataTypeId = (int)EnumDataType.Text;
            }
            if (data.DataTypeId != (int)EnumDataType.Text && data.DataTypeId != (int)EnumDataType.Decimal)
            {
                data.DataSize = -1;
            }

            //allow suggestion set  RefTableField = null

            if (!((EnumFormType)data.FormTypeId).IsSelectForm())
            {
                data.RefTableField = null;
            }
        }

        public async Task<List<CategoryFieldReferModel>> GetCategoryFieldsByCodes(string[] categoryCodes)
        {

            List<CategoryFieldReferModel> lst = await (from f in _masterContext.CategoryField
                                                       join c in _masterContext.Category on f.CategoryId equals c.CategoryId
                                                       where categoryCodes.Contains(c.CategoryCode)
                                                       select new CategoryFieldReferModel
                                                       {
                                                           CategoryCode = c.CategoryCode,
                                                           CategoryFieldName = f.CategoryFieldName,
                                                           Title = f.Title
                                                       }).ToListAsync();

            return lst;
        }

        public async Task<PageData<CategoryFieldModel>> GetCategoryFieldsByCode(string categoryCode, string keyword, int page, int size)
        {
            var categoryId = (await _masterContext.Category.FirstOrDefaultAsync(c => c.CategoryCode == categoryCode))?.CategoryId;

            keyword = (keyword ?? "").Trim();
            var query = _masterContext.CategoryField
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
            List<CategoryFieldModel> lst = await query
                .OrderBy(f => f.SortOrder)
                .ProjectTo<CategoryFieldModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return (lst, total);
        }

        public async Task<bool> UpdateMultiField(int categoryId, List<CategoryFieldModel> fields)
        {
            using var trans = await _masterContext.Database.BeginTransactionAsync();
            try
            {
                // Validate trùng name trong danh sách
                if (fields.Select(f => new { f.CategoryFieldName }).Distinct().Count() != fields.Count)
                {
                    throw new BadRequestException(CategoryErrorCode.CategoryFieldNameAlreadyExisted);
                }

                var category = _masterContext.Category.Include(c => c.OutSideDataConfig).FirstOrDefault(c => c.CategoryId == categoryId);

                for (int indx = 0; indx < fields.Count; indx++)
                {
                    var data = fields[indx];
                    if (category == null)
                    {
                        throw new BadRequestException(CategoryErrorCode.CategoryNotFound);
                    }

                    var categoryAreaField = data.CategoryFieldId > 0 ? _masterContext.CategoryField.FirstOrDefault(f => f.CategoryFieldId == data.CategoryFieldId) : null;
                    ValidateCategoryField(data, categoryAreaField, data.CategoryFieldId);
                    FieldDataProcess(ref data);

                    int dataSize = data.DataTypeId == (int)EnumDataType.Email || data.DataTypeId == (int)EnumDataType.PhoneNumber ? 64 : data.DataSize;

                    if (data.CategoryFieldId > 0 && !data.Compare(categoryAreaField))
                    {
                        // rename field
                        if (!category.IsOutSideData && categoryAreaField.CategoryFieldName != data.CategoryFieldName)
                        {
                            await _masterContext.RenameColumn(category.CategoryCode, categoryAreaField.CategoryFieldName, data.CategoryFieldName);
                        }
                        // Update
                        UpdateField(ref categoryAreaField, data);
                        int decimalPlace = data.DataTypeId == (int)EnumDataType.Decimal ? data.DecimalPlace : 0;
                        // update field 
                        if (!category.IsOutSideData && data.FormTypeId != (int)EnumFormType.ViewOnly)
                        {
                            await _masterContext.UpdateColumn(category.CategoryCode, categoryAreaField.CategoryFieldName, (EnumDataType)categoryAreaField.DataTypeId, dataSize, decimalPlace, data.DefaultValue, !categoryAreaField.IsRequired);
                        }
                    }
                    else if (data.CategoryFieldId == 0)
                    {
                        // Create new
                        var categoryField = _mapper.Map<CategoryField>(data);
                        categoryField.CategoryId = categoryId;
                        await _masterContext.CategoryField.AddAsync(categoryField);
                        await _masterContext.SaveChangesAsync();
                        int decimalPlace = data.DataTypeId == (int)EnumDataType.Decimal ? data.DecimalPlace : 0;
                        // Add field into table
                        if (!category.IsOutSideData && data.FormTypeId != (int)EnumFormType.ViewOnly)
                        {
                            await _masterContext.AddColumn(category.CategoryCode, categoryField.CategoryFieldName, (EnumDataType)categoryField.DataTypeId, dataSize, decimalPlace, data.DefaultValue, !categoryField.IsRequired);
                        }
                    }
                }

                await _masterContext.SaveChangesAsync();
                // Update view
                string tableName = category.IsOutSideData ? category.OutSideDataConfig.Url : category.CategoryCode;
                await _masterContext.ExecuteStoreProcedure("asp_Category_View_Update", new[] {
                        new SqlParameter("@CategoryCode", category.CategoryCode ),
                        new SqlParameter("@TableName", tableName ),
                        new SqlParameter("@IsTreeView", category.IsTreeView),
                        new SqlParameter("@IsOutSideData", category.IsOutSideData),
                        new SqlParameter("@Key", category.OutSideDataConfig?.Key??string.Empty),
                        new SqlParameter("@ParentKey", category.OutSideDataConfig?.ParentKey??string.Empty),
                        new SqlParameter("@UsePlace", category.UsePlace??string.Empty)
                    });

                trans.Commit();
                await _activityLogService.CreateLog(EnumObjectType.Category, categoryId, $"Cập nhật nhiều trường dữ liệu", fields.JsonSerialize());
                return true;
            }
            catch (Exception)
            {
                trans.TryRollbackTransaction();
                throw;
            }
        }

        public async Task<bool> DeleteCategoryField(int categoryId, int categoryFieldId)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockCategoryKey(categoryId));
            var categoryField = await _masterContext.CategoryField.FirstOrDefaultAsync(c => c.CategoryFieldId == categoryFieldId && c.CategoryId == categoryId);
            if (categoryField == null)
            {
                throw new BadRequestException(CategoryErrorCode.CategoryFieldNotFound);
            }

            var category = _masterContext.Category.Include(c => c.OutSideDataConfig).First(c => c.CategoryId == categoryField.CategoryId);
            if (category == null)
            {
                throw new BadRequestException(CategoryErrorCode.CategoryNotFound);
            }

            // Check reference
            bool isRefer = await _masterContext.CategoryField.AnyAsync(c => c.RefTableCode == category.CategoryCode
            && (c.RefTableField == categoryField.CategoryFieldName || c.RefTableTitle.Contains(categoryField.CategoryFieldName)));
            if (isRefer)
            {
                throw new BadRequestException(CategoryErrorCode.DestCategoryFieldAlreadyExisted);
            }

            using var trans = await _masterContext.Database.BeginTransactionAsync();
            try
            {
                // Delete field
                categoryField.IsDeleted = true;
                await _masterContext.SaveChangesAsync();

                //
                if (!category.IsOutSideData)
                {
                    await _masterContext.DeleteColumn(category.CategoryCode, categoryField.CategoryFieldName);
                }

                // Update view
                string tableName = category.IsOutSideData ? category.OutSideDataConfig.Url : category.CategoryCode;
                await _masterContext.ExecuteStoreProcedure("asp_Category_View_Update", new[] {
                        new SqlParameter("@CategoryCode", category.CategoryCode ),
                        new SqlParameter("@TableName", tableName ),
                        new SqlParameter("@IsTreeView", category.IsTreeView),
                        new SqlParameter("@IsOutSideData", category.IsOutSideData),
                        new SqlParameter("@Key", category.OutSideDataConfig?.Key??string.Empty),
                        new SqlParameter("@ParentKey", category.OutSideDataConfig?.ParentKey??string.Empty),
                        new SqlParameter("@UsePlace", category.UsePlace??string.Empty)
                    });

                trans.Commit();
                await _activityLogService.CreateLog(EnumObjectType.Category, categoryField.CategoryFieldId, $"Xóa trường dữ liệu {categoryField.Title}", categoryField.JsonSerialize());
                return true;
            }
            catch (Exception)
            {
                trans.TryRollbackTransaction();
                throw;
            }
        }
        #endregion


        public async Task<List<ReferFieldModel>> GetReferFields(IList<string> categoryCodes, IList<string> fieldNames)
        {
            return await (from f in _masterContext.CategoryField
                   join c in _masterContext.Category on f.CategoryId equals c.CategoryId
                   where categoryCodes.Contains(c.CategoryCode) && fieldNames.Contains(f.CategoryFieldName)
                   select new ReferFieldModel
                   {
                       CategoryCode = c.CategoryCode,
                       CategoryFieldName = f.CategoryFieldName,
                       DataTypeId = f.DataTypeId
                   }).ToListAsync();
        }

        public async Task<CategoryNameModel> GetFieldDataForMapping(int categoryId)
        {
            var category = _masterContext.Category.AsNoTracking().FirstOrDefault(c => c.CategoryId == categoryId);
            if (category == null)
            {
                throw new BadRequestException(CategoryErrorCode.CategoryNotFound);
            }
            //if (category.IsReadonly)
            //{
            //    throw new BadRequestException(CategoryErrorCode.CategoryReadOnly);
            //}
            //if (category.IsOutSideData)
            //{
            //    throw new BadRequestException(CategoryErrorCode.CategoryIsOutSideData);
            //}


            var result = new CategoryNameModel()
            {
                CategoryId = category.CategoryId,
                CategoryCode = category.CategoryCode,
                CategoryTitle = category.Title,
                IsTreeView = category.IsTreeView,
                Fields = new List<CategoryFieldNameModel>()
            };

            var fields = await _masterContext.CategoryField
                .AsNoTracking()
                .Where(f => category.CategoryId == f.CategoryId && !f.IsHidden && !f.AutoIncrement && f.CategoryFieldName != AccountantConstants.F_IDENTITY)
                .ToListAsync();

            var refCategoryCodes = fields.Where(f => !string.IsNullOrWhiteSpace(f.RefTableCode))
                .Select(f => f.RefTableCode).Distinct().ToList();

            var refCategoryFields = (await (
                from f in _masterContext.CategoryField
                join c in _masterContext.Category on f.CategoryId equals c.CategoryId
                where refCategoryCodes.Contains(c.CategoryCode)
                select new
                {
                    c.CategoryId,
                    c.CategoryCode,
                    CategoryTitle = c.Title,
                    c.IsTreeView,
                    Field = f
                }).ToListAsync())
                .GroupBy(c => c.CategoryCode)
                .ToDictionary(
                    c => c.Key,
                    c => c.GroupBy(i => new { i.CategoryId, i.CategoryCode, i.CategoryTitle, i.IsTreeView })
                    .Select(i => new
                    {
                        CategoryInfo = new
                        {
                            i.FirstOrDefault().CategoryId,
                            i.FirstOrDefault().CategoryCode,
                            i.FirstOrDefault().CategoryTitle,
                            i.FirstOrDefault().IsTreeView
                        },
                        Fields = i.Select(f => f.Field).ToList()
                    })
                    .First()
                );



            foreach (var field in fields)
            {
                var fileData = new CategoryFieldNameModel()
                {
                    CategoryFieldId = field.CategoryFieldId,
                    FieldName = field.CategoryFieldName,
                    FieldTitle = field.Title,
                    RefCategory = null
                };

                if (!string.IsNullOrWhiteSpace(field.RefTableCode))
                {
                    if (!refCategoryFields.TryGetValue(field.RefTableCode, out var refCategory))
                    {
                        throw new BadRequestException(GeneralCode.ItemNotFound, $"Danh mục liên kết {field.RefTableCode} không tìm thấy!");
                    }

                    fileData.RefCategory = new CategoryNameModel()
                    {
                        CategoryId = refCategory.CategoryInfo.CategoryId,
                        CategoryCode = refCategory.CategoryInfo.CategoryCode,
                        CategoryTitle = refCategory.CategoryInfo.CategoryTitle,
                        IsTreeView = refCategory.CategoryInfo.IsTreeView,

                        Fields = refCategory.Fields
                        .Select(f => new CategoryFieldNameModel()
                        {
                            CategoryFieldId = f.CategoryFieldId,
                            FieldName = f.CategoryFieldName,
                            FieldTitle = f.Title,
                            RefCategory = null
                        }).ToList()
                    };
                }

                result.Fields.Add(fileData);
            }

            return result;
        }

        public PageData<DataTypeModel> GetDataTypes(int page, int size)
        {
            var dataTypes = EnumExtensions.GetEnumMembers<EnumDataType>().Select(m => new DataTypeModel
            {
                DataTypeId = (int)m.Enum,
                DataSizeDefault = m.Enum.GetDataSize(),
                RegularExpression = m.Enum.GetRegex(),
                Title = m.Description,
                Name = m.ToString()
            }).ToList();

            var total = dataTypes.Count();
            if (size > 0)
            {
                dataTypes = dataTypes.Skip((page - 1) * size).Take(size).ToList();
            }
            return (dataTypes, total);
        }

        public PageData<FormTypeModel> GetFormTypes(int page, int size)
        {
            var formTypes = EnumExtensions.GetEnumMembers<EnumFormType>().Select(m => new FormTypeModel
            {
                FormTypeId = (int)m.Enum,
                Title = m.Description,
                Name = m.ToString()
            }).ToList();

            var total = formTypes.Count();
            if (size > 0)
            {
                formTypes = formTypes.Skip((page - 1) * size).Take(size).ToList();
            }
            return (formTypes, total);
        }

        public PageData<OperatorModel> GetOperators(int page, int size)
        {
            var operators = EnumExtensions.GetEnumMembers<EnumOperator>().Select(m => new OperatorModel
            {
                Value = (int)m.Enum,
                Title = m.Description,
                ParamNumber = m.Enum.GetParamNumber()
            }).ToList();
            int total = operators.Count;
            if (size > 0)
            {
                operators = operators.Skip((page - 1) * size).Take(size).ToList();
            }
            return (operators, total);
        }

        public PageData<LogicOperatorModel> GetLogicOperators(int page, int size)
        {
            var operators = EnumExtensions.GetEnumMembers<EnumLogicOperator>().Select(m => new LogicOperatorModel
            {
                Value = (int)m.Enum,
                Title = m.Description
            }).ToList();
            int total = operators.Count;
            if (size > 0)
            {
                operators = operators.Skip((page - 1) * size).Take(size).ToList();
            }
            return (operators, total);
        }

        public PageData<ModuleTypeModel> GetModuleTypes(int page, int size)
        {
            var moduleTypes = EnumExtensions.GetEnumMembers<EnumModuleType>().Select(m => new ModuleTypeModel
            {
                ModuleTypeValue = (int)m.Enum,
                ModuleTypeTitle = m.Description
            }).ToList();
            int total = moduleTypes.Count;
            if (size > 0)
            {
                moduleTypes = moduleTypes.Skip((page - 1) * size).Take(size).ToList();
            }
            return (moduleTypes, total);
        }

        public async Task<int> GetCategoryIdByCode(string categoryCode)
        {
            var category = await _masterContext.Category.FirstOrDefaultAsync(c => c.CategoryCode == categoryCode);
            if (category == null)
            {
                throw new BadRequestException(CategoryErrorCode.CategoryNotFound);
            }
            return category.CategoryId;
        }
    }
}
