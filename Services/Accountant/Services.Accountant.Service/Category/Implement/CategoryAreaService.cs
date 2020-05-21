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
using VErp.Commons.Enums.AccountantEnum;
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
    public class CategoryAreaService : AccoutantBaseService, ICategoryAreaService
    {
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;

        public CategoryAreaService(AccountingDBContext accountingDBContext
            , IOptions<AppSetting> appSetting
            , ILogger<CategoryAreaService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            ) : base(accountingDBContext, appSetting, mapper)
        {
            _logger = logger;
            _activityLogService = activityLogService;
        }

        public async Task<ServiceResult<CategoryAreaModel>> GetCategoryArea(int categoryId, int categoryAreaId)
        {
            var CategoryArea = await _accountingContext.CategoryArea
                .Where(i => i.CategoryId == categoryId && i.CategoryAreaId == categoryAreaId)
                .ProjectTo<CategoryAreaModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
            if (CategoryArea == null)
            {
                return CategoryErrorCode.SubCategoryNotFound;
            }
            return CategoryArea;
        }

        public async Task<PageData<CategoryAreaModel>> GetCategoryAreas(int categoryId, string keyword, int page, int size)
        {
            keyword = (keyword ?? "").Trim();
            var query = _accountingContext.CategoryArea.Where(a => a.CategoryId == categoryId).AsQueryable();
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(a => a.CategoryAreaCode.Contains(keyword) || a.Title.Contains(keyword));
            }
            query = query.OrderBy(c => c.Title);
            var total = await query.CountAsync();
            if (size > 0)
            {
                query = query.Skip((page - 1) * size).Take(size);
            }
            var lst = await query.ProjectTo<CategoryAreaModel>(_mapper.ConfigurationProvider).OrderBy(a=>a.SortOrder).ToListAsync();
            return (lst, total);
        }

        public async Task<ServiceResult<int>> AddCategoryArea(int categoryId, CategoryAreaInputModel data)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockCategoryKey(categoryId));
            var existedCategory = await _accountingContext.CategoryArea
                .FirstOrDefaultAsync(a => a.CategoryId == categoryId && (a.CategoryAreaCode == data.CategoryAreaCode || a.Title == data.Title));
            if (existedCategory != null)
            {
                if (string.Compare(existedCategory.CategoryAreaCode, data.CategoryAreaCode, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return CategoryErrorCode.CategoryCodeAlreadyExisted;
                }

                return CategoryErrorCode.CategoryTitleAlreadyExisted;
            }

            using (var trans = await _accountingContext.Database.BeginTransactionAsync())
            {
                try
                {
                    CategoryArea CategoryArea = _mapper.Map<CategoryArea>(data);
                    CategoryArea.CategoryId = categoryId;
                    await _accountingContext.CategoryArea.AddAsync(CategoryArea);
                    await _accountingContext.SaveChangesAsync();

                    trans.Commit();
                    await _activityLogService.CreateLog(EnumObjectType.Category, CategoryArea.CategoryAreaId, $"Thêm vùng thông tin {CategoryArea.Title}", data.JsonSerialize());
                    return CategoryArea.CategoryAreaId;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    _logger.LogError(ex, "Create");
                    return GeneralCode.InternalError;
                }
            }
        }

        public async Task<Enum> UpdateCategoryArea(int categoryId, int categoryAreaId, CategoryAreaInputModel data)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockCategoryKey(categoryId));
            var CategoryArea = await _accountingContext.CategoryArea.FirstOrDefaultAsync(a => a.CategoryId == categoryId && a.CategoryAreaId == categoryAreaId);
            if (CategoryArea == null)
            {
                return CategoryErrorCode.SubCategoryNotFound;
            }
            if (CategoryArea.CategoryAreaCode != data.CategoryAreaCode || CategoryArea.Title != data.Title)
            {
                var existedCategory = await _accountingContext.CategoryArea
                    .FirstOrDefaultAsync(a => a.CategoryId == categoryId && a.CategoryAreaId != categoryAreaId && (a.CategoryAreaCode == data.CategoryAreaCode || a.Title == data.Title));
                if (existedCategory != null)
                {
                    if (string.Compare(existedCategory.CategoryAreaCode, data.CategoryAreaCode, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return CategoryErrorCode.SubCategoryCodeAlreadyExisted;
                    }

                    return CategoryErrorCode.SubCategoryTitleAlreadyExisted;
                }
            }

            using var trans = await _accountingContext.Database.BeginTransactionAsync();
            try
            {
                CategoryArea.CategoryAreaCode = data.CategoryAreaCode;
                CategoryArea.Title = data.Title;
                CategoryArea.SortOrder = data.SortOrder;
                await _accountingContext.SaveChangesAsync();

                trans.Commit();
                await _activityLogService.CreateLog(EnumObjectType.Category, CategoryArea.CategoryAreaId, $"Cập nhật vùng dữ liệu {CategoryArea.Title}", data.JsonSerialize());
                return GeneralCode.Success;
            }
            catch (Exception ex)
            {
                trans.Rollback();
                _logger.LogError(ex, "Update");
                return GeneralCode.InternalError;
            }
        }

        public async Task<Enum> DeleteCategoryArea(int categoryId, int categoryAreaId)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockCategoryKey(categoryId));
            var categoryArea = await _accountingContext.CategoryArea.FirstOrDefaultAsync(a => a.CategoryId == categoryId && a.CategoryAreaId == categoryAreaId);
            if (categoryArea == null)
            {
                return CategoryErrorCode.SubCategoryNotFound;
            }

            using var trans = await _accountingContext.Database.BeginTransactionAsync();
            try
            {
                // Xóa field
                List<CategoryField> CategoryAreaFields = _accountingContext.CategoryField.Where(f => f.CategoryAreaId == categoryAreaId).ToList();
                foreach (CategoryField categoryField in CategoryAreaFields)
                {
                    categoryField.IsDeleted = true;
                    await _accountingContext.SaveChangesAsync();

                    // Xóa rowValue
                    List<CategoryRowValue> CategoryValueRows = _accountingContext.CategoryRowValue.Where(r => r.CategoryFieldId == categoryField.CategoryFieldId).ToList();
                    foreach (CategoryRowValue categoryValueRow in CategoryValueRows)
                    {
                        categoryValueRow.IsDeleted = true;
                        await _accountingContext.SaveChangesAsync();

                    }
                }

              

                // Xóa area
                categoryArea.IsDeleted = true;
                await _accountingContext.SaveChangesAsync();
                trans.Commit();
                await _activityLogService.CreateLog(EnumObjectType.Category, categoryArea.CategoryAreaId, $"Xóa chứng từ {categoryArea.Title}", categoryArea.JsonSerialize());
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
