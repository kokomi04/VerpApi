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
                query = query.Where(f => f.Name.Contains(keyword) || f.Title.Contains(keyword));
            }
            query = query.OrderBy(c => c.Sequence);
            if (size > 0)
            {
                query = query.Skip((page - 1) * size).Take(size);
            }
            List<CategoryFieldOutputModel> lst = await query.Include(f => f.DataType)
                .Include(f => f.FormType)
                .OrderBy(f => f.Sequence)
                .Select(f => _mapper.Map<CategoryFieldOutputModel>(f)).ToListAsync();
            var total = lst.Count;
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
            if(!_accountingContext.DataType.Any(d => d.DataTypeId == data.DataTypeId))
            {
                return CategoryErrorCode.DataTypeNotFound;
            }

            if (!_accountingContext.FormType.Any(f => f.FormTypeId == data.FormTypeId))
            {
                return CategoryErrorCode.FormTypeNotFound;
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

        public async Task<Enum> UpdateCategoryField(int updatedUserId, int categoryFieldId, CategoryFieldInputModel data)
        {
            var categoryField = await _accountingContext.CategoryField.FirstOrDefaultAsync(f => f.CategoryFieldId == categoryFieldId);
            if (categoryField == null)
            {
                return CategoryErrorCode.CategoryFieldNotFound;
            }
            if (categoryField.Name != data.Name || categoryField.Title != data.Title || categoryField.CategoryId != data.CategoryId)
            {
                var existedCategoryField = await _accountingContext.CategoryField
                    .FirstOrDefaultAsync(f => f.CategoryFieldId != categoryFieldId && f.CategoryId == data.CategoryId && (f.Name == data.Name || f.Title == f.Title));
                if (existedCategoryField != null)
                {
                    if (string.Compare(existedCategoryField.Name, data.Name, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return CategoryErrorCode.CategoryFieldNameAlreadyExisted;
                    }

                    return CategoryErrorCode.CategoryFieldTitleAlreadyExisted;
                }
            }

            if (categoryField.ReferenceCategoryFieldId != data.ReferenceCategoryFieldId && data.ReferenceCategoryFieldId.HasValue)
            {
                var sourceCategoryField = _accountingContext.CategoryField.Include(f => f.Category).FirstOrDefault(f => f.CategoryFieldId == data.ReferenceCategoryFieldId);
                if (sourceCategoryField == null)
                {
                    return CategoryErrorCode.SourceCategoryFieldNotFound;
                }
            }

            if (categoryField.DataTypeId != data.DataTypeId && !_accountingContext.DataType.Any(d => d.DataTypeId == data.DataTypeId))
            {
                return CategoryErrorCode.DataTypeNotFound;
            }

            if (categoryField.FormTypeId != data.FormTypeId && !_accountingContext.FormType.Any(f => f.FormTypeId == data.FormTypeId))
            {
                return CategoryErrorCode.FormTypeNotFound;
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
                    categoryField.IsHidden = data.IsHidden;
                    categoryField.IsRequired = data.IsRequired;
                    categoryField.IsUnique = data.IsUnique;
                    categoryField.AutoIncrement = data.AutoIncrement;
                    data.ReferenceCategoryFieldId = data.ReferenceCategoryFieldId;
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

        public async Task<Enum> DeleteCategoryField(int updatedUserId, int categoryFieldId)
        {
            var categoryField = await _accountingContext.CategoryField.FirstOrDefaultAsync(f => f.CategoryFieldId == categoryFieldId);
            if (categoryField == null)
            {
                return CategoryErrorCode.CategoryFieldNotFound;
            }

            // Check xem có trường dữ liệu nào đang tham chiếu tới
            if(_accountingContext.CategoryField.Any(f => f.ReferenceCategoryFieldId == categoryFieldId))
            {
                return CategoryErrorCode.DestCategoryFieldAlreadyExisted;
            }

            using (var trans = await _accountingContext.Database.BeginTransactionAsync())
            {
                try
                {
                    categoryField.IsDeleted = true;
                    categoryField.UpdatedUserId = updatedUserId;
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

        private int[] GetAllCategoryIds(int categoryId)
        {
            List<int> ids = new List<int> { categoryId };
            foreach (int id in _accountingContext.Category.Where(r => r.ParentId == categoryId).Select(r => r.CategoryId))
            {
                ids.AddRange(GetAllCategoryIds(id));
            }

            return ids.ToArray();
        }
    }
}
