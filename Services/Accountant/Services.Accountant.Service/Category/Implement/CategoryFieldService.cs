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
    public class CategoryFieldService : ICategoryFieldService
    {
        private readonly AccountingDBContext _accountingContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly IMapper _mapper;
        public CategoryFieldService(AccountingDBContext accountingContext
            , IOptions<AppSetting> appSetting
            , ILogger<CategoryFieldService> logger
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

        public async Task<PageData<CategoryFieldModel>> GetCategoryFields(int categoryId, string keyword, int page, int size, bool isFull)
        {
            keyword = (keyword ?? "").Trim();
            var query = _accountingContext.CategoryField.AsQueryable();
            int[] categoryIds;
            if (isFull)
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
            List<CategoryFieldModel> lst = await query.Include(f => f.DataType)
                .Include(f => f.FormType)
                .OrderBy(f => f.Sequence)
                .Select(f => _mapper.Map<CategoryFieldModel>(f)).ToListAsync();

            return (lst, total);
        }

        public async Task<ServiceResult<int>> AddCategoryField(int updatedUserId, CategoryFieldModel data)
        {
            // Check category
            if (!_accountingContext.Category.Any(c => c.CategoryId == data.CategoryId))
            {
                return CategoryErrorCode.CategoryNotFound;
            }

            var existedCategoryField = await _accountingContext.CategoryField
                .FirstOrDefaultAsync(f => f.CategoryId == data.CategoryId && f.Name == data.Name || f.Title == f.Title);
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
                var sourceCategoryField = _accountingContext.CategoryField.Include(f => f.Category).FirstOrDefault(f => f.CategoryFieldId == data.ReferenceCategoryFieldId);
                if (sourceCategoryField == null)
                {
                    return CategoryErrorCode.SourceCategoryFieldNotFound;
                }
            }

            using (var trans = await _accountingContext.Database.BeginTransactionAsync())
            {
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
        }

        private int[] GetAllCategoryIds(int categoryId)
        {
            List<int> ids = new List<int>(categoryId);
            foreach (int id in _accountingContext.Category.Where(r => r.ParentId == categoryId).Select(r => r.CategoryId))
            {
                ids.AddRange(GetAllCategoryIds(id));
            }

            return ids.ToArray();
        }
    }
}
