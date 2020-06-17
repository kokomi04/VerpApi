using AutoMapper;
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
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.AccountancyDB;
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

                // Thêm Identity Area
                CategoryArea identityArea = new CategoryArea
                {
                    CategoryId = category.CategoryId,
                    CategoryAreaCode = AccountantConstants.IDENTITY_AREA,
                    Title = AccountantConstants.IDENTITY_AREA_TITLE,
                    SortOrder = 0,
                    CategoryAreaType = (int)EnumCategoryAreaType.Identity
                };
                await _accountancyContext.CategoryArea.AddAsync(identityArea);
                await _accountancyContext.SaveChangesAsync();

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
                await _accountancyContext.CategoryField.AddAsync(identityField);
                await _accountancyContext.SaveChangesAsync();

                // Create table
                using var connection = _accountancyContext.Database.GetDbConnection();
                connection.Open();
                using var cmd = connection.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "asp_Category_Table_Add";
                var codeParam = new SqlParameter("@CategoryCode", category.CategoryCode)
                {
                    Direction = ParameterDirection.Input,
                    DbType = DbType.String
                };
                var isTreeViewParam = new SqlParameter("@IsTreeView", category.IsTreeView)
                {
                    Direction = ParameterDirection.Input,
                    DbType = DbType.Boolean
                };
                cmd.Parameters.Add(isTreeViewParam);
                cmd.ExecuteNonQuery();

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

                // Xóa area
                var deleteAreas = _accountancyContext.CategoryArea.Where(a => a.CategoryId == category.CategoryId);
                foreach (var area in deleteAreas)
                {
                    area.IsDeleted = true;
                }

                // Xóa field
                var deleteFields = _accountancyContext.CategoryField.Where(f => f.CategoryId == category.CategoryId);
                foreach (var field in deleteFields)
                {
                    // Check có trường đang tham chiếu tới
                   
                    field.IsDeleted = true;
                }

                // Delete table
                using var connection = _accountancyContext.Database.GetDbConnection();
                connection.Open();
                using var cmd = connection.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "asp_Category_Table_Delete";
                var param = new SqlParameter("@CategoryCode", category.CategoryCode)
                {
                    Direction = ParameterDirection.Input,
                    DbType = DbType.String
                };
                cmd.Parameters.Add(param);
                cmd.ExecuteNonQuery();

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

  
    }
}
