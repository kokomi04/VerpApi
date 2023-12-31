﻿using AutoMapper;
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
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.AccountingDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Accountant.Model.Category;
using CategoryEntity = VErp.Infrastructure.EF.AccountingDB.Category;

namespace VErp.Services.Accountant.Service.Category.Implement
{
    public class CategoryFieldService : AccoutantBaseService, ICategoryFieldService
    {
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        public CategoryFieldService(AccountingDBContext accountingContext
            , IOptions<AppSetting> appSetting
            , ILogger<CategoryFieldService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            ) : base(accountingContext, appSetting, mapper)
        {
            _logger = logger;
            _activityLogService = activityLogService;
        }

        public async Task<PageData<CategoryFieldOutputModel>> GetCategoryFields(int categoryId, string keyword, int page, int size)
        {
            keyword = (keyword ?? "").Trim();
            var query = _accountingContext.CategoryField
                .Include(f => f.ReferenceCategoryField)
                .Include(f => f.InverseReferenceCategoryTitleField)
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

        public async Task<PageData<CategoryFieldOutputModel>> GetCategoryFieldsByCode(string categoryCode, string keyword, int page, int size)
        {
            var categoryId = (await _accountingContext.Category.FirstOrDefaultAsync(c => c.CategoryCode == categoryCode))?.CategoryId;

            keyword = (keyword ?? "").Trim();
            var query = _accountingContext.CategoryField
                .Include(f => f.ReferenceCategoryField)
                .Include(f => f.InverseReferenceCategoryTitleField)
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
            var query = _accountingContext.CategoryField
                .Include(f => f.ReferenceCategoryField)
                .Include(f => f.InverseReferenceCategoryTitleField)
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
            var categoryField = await _accountingContext.CategoryField
                .Include(f => f.ReferenceCategoryField)
                .Include(f => f.ReferenceCategoryTitleField)
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
            using var trans = await _accountingContext.Database.BeginTransactionAsync();
            try
            {
                var categoryField = _mapper.Map<CategoryField>(data);
                categoryField.CategoryId = categoryId;

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

        public async Task<Enum> UpdateCategoryField(int categoryId, int categoryFieldId, CategoryFieldInputModel data)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockCategoryKey(categoryId));
            ValidateExistedCategory(categoryId, data.CategoryAreaId);
            var categoryField = await _accountingContext.CategoryField.FirstOrDefaultAsync(f => f.CategoryFieldId == categoryFieldId);
            ValidateCategoryField(data, categoryField, categoryFieldId);
            FieldDataProcess(ref data);
            using var trans = await _accountingContext.Database.BeginTransactionAsync();
            try
            {
                UpdateField(ref categoryField, data);
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

        private void UpdateField(ref CategoryField categoryField, CategoryFieldInputModel data)
        {
            categoryField.CategoryFieldName = data.CategoryFieldName;
            categoryField.CategoryAreaId = data.CategoryAreaId;
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
            categoryField.ReferenceCategoryFieldId = data.ReferenceCategoryFieldId;
            categoryField.ReferenceCategoryTitleFieldId = data.ReferenceCategoryTitleFieldId;
        }

        private void ValidateExistedCategory(int categoryId, int categoryAreaId)
        {
            // Check category
            if (!_accountingContext.Category.Any(c => c.CategoryId == categoryId))
            {
                throw new BadRequestException(CategoryErrorCode.CategoryNotFound);
            }
            if (!_accountingContext.CategoryArea.Any(a => a.CategoryId == categoryId && a.CategoryAreaId == categoryAreaId))
            {
                throw new BadRequestException(CategoryErrorCode.SubCategoryNotFound);
            }
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
            if (updateFieldName && _accountingContext.CategoryField.Any(f => (!categoryFieldId.HasValue || f.CategoryFieldId != categoryFieldId.Value) && f.CategoryFieldId == data.CategoryFieldId && f.CategoryFieldName == data.CategoryFieldName))
            {
                throw new BadRequestException(CategoryErrorCode.CategoryFieldNameAlreadyExisted);
            }
            if (data.ReferenceCategoryFieldId.HasValue)
            {
                var sourceCategoryField = _accountingContext.CategoryField.FirstOrDefault(f => f.CategoryFieldId == data.ReferenceCategoryFieldId.Value);
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
                var sourceCategoryField = _accountingContext.CategoryField.FirstOrDefault(f => f.CategoryFieldId == referId);
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
            using var trans = await _accountingContext.Database.BeginTransactionAsync();
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
                        var categoryAreaField = data.CategoryFieldId > 0 ? _accountingContext.CategoryField.FirstOrDefault(f => f.CategoryFieldId == data.CategoryFieldId) : null;
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
                            await _accountingContext.CategoryField.AddAsync(categoryField);
                            await _accountingContext.SaveChangesAsync();
                        }
                    }
                }
                await _accountingContext.SaveChangesAsync();
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
            var categoryField = await _accountingContext.CategoryField.FirstOrDefaultAsync(c => c.CategoryFieldId == categoryFieldId && c.CategoryId == categoryId);
            if (categoryField == null)
            {
                throw new BadRequestException(CategoryErrorCode.CategoryFieldNotFound);
            }

            // Check reference
            bool isRefer = await _accountingContext.CategoryField.AnyAsync(c => c.ReferenceCategoryFieldId == categoryFieldId);
            if (isRefer)
            {
                throw new BadRequestException(CategoryErrorCode.DestCategoryFieldAlreadyExisted);
            }

            using var trans = await _accountingContext.Database.BeginTransactionAsync();
            try
            {
                // Delete row-field-value
                var rowFieldValues = _accountingContext.CategoryRowValue.Where(rfv => rfv.CategoryFieldId == categoryFieldId);
                foreach (var rowFieldValue in rowFieldValues)
                {
                    rowFieldValue.IsDeleted = true;
                }
                // Delete field
                categoryField.IsDeleted = true;
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
