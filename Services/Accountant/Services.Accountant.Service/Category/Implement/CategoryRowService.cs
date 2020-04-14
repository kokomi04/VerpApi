using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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

namespace VErp.Services.Accountant.Service.Category.Implement
{
    public class CategoryRowService : CategoryBaseService, ICategoryRowService
    {
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly IMapper _mapper;
        public CategoryRowService(AccountingDBContext accountingContext
            , IOptions<AppSetting> appSetting
            , ILogger<CategoryRowService> logger
            , IActivityLogService activityLogService
             , IMapper mapper
            ) : base(accountingContext)
        {
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
            _mapper = mapper;
        }

        public async Task<PageData<CategoryRowOutputModel>> GetCategoryRows(int categoryId, string keyword, int page, int size)
        {
            var query = _accountingContext.CategoryRow
                           .Where(r => r.CategoryId == categoryId)
                           .Join(_accountingContext.CategoryRowValue, r => r.CategoryRowId, rv => rv.CategoryRowId, (r, rv) => new
                           {
                               r.CategoryRowId,
                               rv.CategoryValueId,
                               rv.CategoryFieldId
                           })
                           .Join(_accountingContext.CategoryValue, rv => rv.CategoryValueId, v => v.CategoryValueId, (rv, v) => new
                           {
                               rv.CategoryRowId,
                               v.Value,
                               v.CategoryValueId,
                               rv.CategoryFieldId
                           });

            IQueryable<int> rowIds;
            // search
            if (!string.IsNullOrEmpty(keyword))
            {
                rowIds = query.Where(v => v.Value.Contains(keyword)).GroupBy(rvf => rvf.CategoryRowId).Select(g => g.Key);
            }
            else
            {
                rowIds = query.GroupBy(rvf => rvf.CategoryRowId).Select(g => g.Key);
            }
            var total = await rowIds.CountAsync();
            if (size > 0)
            {
                rowIds = rowIds.Skip((page - 1) * size).Take(size);
            }

            var data = query.Where(rvf => rowIds.Contains(rvf.CategoryRowId))
                .AsEnumerable()
                .GroupBy(rvf => rvf.CategoryRowId);

            List<CategoryRowOutputModel> lst = new List<CategoryRowOutputModel>();
            foreach (var item in data)
            {
                CategoryRowOutputModel output = new CategoryRowOutputModel
                {
                    CategoryRowId = item.Key
                };

                ICollection<CategoryValueModel> row = new List<CategoryValueModel>();
                foreach (var cell in item)
                {
                    row.Add(new CategoryValueModel
                    {
                        CategoryFieldId = cell.CategoryFieldId,
                        CategoryValueId = cell.CategoryValueId,
                        Value = cell.Value
                    });
                }
                output.Values = row;
                lst.Add(output);
            }
            return (lst, total);
        }

        public async Task<ServiceResult<CategoryRowOutputModel>> GetCategoryRow(int categoryId, int categoryRowId)
        {
            // Check row 
            var categoryRow = await _accountingContext.CategoryRow.FirstOrDefaultAsync(r => r.CategoryId == categoryId && r.CategoryRowId == categoryRowId);
            if (categoryRow == null)
            {
                return CategoryErrorCode.CategoryRowNotFound;
            }

            var values = _accountingContext.CategoryRowValue
                           .Where(r => r.CategoryRowId == categoryRowId)
                           .Join(_accountingContext.CategoryValue, rv => rv.CategoryValueId, v => v.CategoryValueId, (rv, v) => new
                           {
                               rv.CategoryRowId,
                               v.Value,
                               v.CategoryValueId,
                               rv.CategoryFieldId
                           }).ToList();

            CategoryRowOutputModel output = new CategoryRowOutputModel
            {
                CategoryRowId = categoryRowId
            };

            ICollection<CategoryValueModel> row = new List<CategoryValueModel>();
            foreach (var value in values)
            {
                row.Add(new CategoryValueModel
                {
                    CategoryFieldId = value.CategoryFieldId,
                    CategoryValueId = value.CategoryValueId,
                    Value = value.Value
                });
            }
            output.Values = row;

            return output;
        }

        public async Task<ServiceResult<int>> AddCategoryRow(int updatedUserId, int categoryId, CategoryRowInputModel data)
        {
            // Validate
            var category = _accountingContext.Category.FirstOrDefault(c => c.CategoryId == categoryId);
            if (category == null)
            {
                return CategoryErrorCode.CategoryNotFound;
            }
            if (category.IsReadonly)
            {
                return CategoryErrorCode.CategoryReadOnly;
            }
            if (!category.IsModule)
            {
                return CategoryErrorCode.CategoryIsNotModule;
            }
            // Lấy thông tin field
            var categoryIds = GetAllCategoryIds(categoryId);
            var categoryFields = _accountingContext.CategoryField.Include(f => f.DataType).Where(f => categoryIds.Contains(f.CategoryId)).AsEnumerable();
            var requiredFields = categoryFields.Where(f => !f.AutoIncrement && f.IsRequired);
            var uniqueFields = categoryFields.Where(f => !f.AutoIncrement && f.IsUnique);
            var selectFields = categoryFields.Where(f => !f.AutoIncrement && (f.FormTypeId == (int)EnumFormType.Select || f.FormTypeId == (int)EnumFormType.SearchTable));

            // Check field required
            var r = CheckRequired(data, requiredFields);
            if (!r.IsSuccess()) return r;

            // Check unique
            r = CheckUnique(data, uniqueFields);
            if (!r.IsSuccess()) return r;

            // Check refer
            r = CheckRefer(ref data, selectFields);
            if (!r.IsSuccess()) return r;

            // Check value
            r = CheckValue(data, categoryFields);
            if (!r.IsSuccess()) return r;

            using (var trans = await _accountingContext.Database.BeginTransactionAsync())
            {
                try
                {
                    int categoryRowId = await InsertCategoryRowAsync(updatedUserId, categoryId, categoryFields, data);
                    trans.Commit();
                    await _activityLogService.CreateLog(EnumObjectType.Category, categoryRowId, $"Thêm dòng cho danh mục {category.Title}", data.JsonSerialize());
                    return categoryRowId;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    _logger.LogError(ex, "Create");
                    return GeneralCode.InternalError;
                }
            }
        }

        private async Task<int> InsertCategoryRowAsync(int updatedUserId, int categoryId, IEnumerable<CategoryField> categoryFields, CategoryRowInputModel data)
        { // Thêm dòng
            var categoryRow = new CategoryRow
            {
                CategoryId = categoryId
            };
            await _accountingContext.CategoryRow.AddAsync(categoryRow);
            await _accountingContext.SaveChangesAsync();

            // Duyệt danh sách field
            foreach (var field in categoryFields)
            {
                int categoryValueId = 0;
                var valueItem = data.Values.FirstOrDefault(v => v.CategoryFieldId == field.CategoryFieldId);
                if ((valueItem == null || string.IsNullOrEmpty(valueItem.Value)) && !field.AutoIncrement)
                {
                    continue;
                }

                if (field.FormTypeId != (int)EnumFormType.Select && field.FormTypeId != (int)EnumFormType.SearchTable)
                {
                    string value = string.Empty;
                    if (field.AutoIncrement)
                    {
                        // Lấy ra value lớn nhất
                        string max = _accountingContext.CategoryValue.Where(v => v.CategoryFieldId == field.CategoryFieldId).Max(v => v.Value);
                        value = (int.Parse(max ?? "0") + 1).ToString();
                    }
                    else
                    {
                        value = valueItem.Value;
                    }
                    // Thêm value
                    CategoryValue categoryValue = new CategoryValue
                    {
                        CategoryFieldId = field.CategoryFieldId,
                        Value = value,
                        UpdatedUserId = updatedUserId
                    };
                    await _accountingContext.CategoryValue.AddAsync(categoryValue);
                    await _accountingContext.SaveChangesAsync();
                    categoryValueId = categoryValue.CategoryValueId;
                }
                else
                {
                    categoryValueId = valueItem.CategoryValueId;
                }

                // Thêm mapping
                var categoryRowValue = new CategoryRowValue
                {
                    CategoryFieldId = field.CategoryFieldId,
                    CategoryRowId = categoryRow.CategoryRowId,
                    CategoryValueId = categoryValueId,
                    UpdatedUserId = updatedUserId
                };
                await _accountingContext.CategoryRowValue.AddAsync(categoryRowValue);
                await _accountingContext.SaveChangesAsync();
            }
            return categoryRow.CategoryId;
        }

        public async Task<Enum> UpdateCategoryRow(int updatedUserId, int categoryId, int categoryRowId, CategoryRowInputModel data)
        {
            var categoryRow = await _accountingContext.CategoryRow.FirstOrDefaultAsync(c => c.CategoryRowId == categoryRowId && c.CategoryId == categoryId);
            if (categoryRow == null)
            {
                return CategoryErrorCode.CategoryRowNotFound;
            }
            // Lấy thông tin field
            var categoryIds = GetAllCategoryIds(categoryRow.CategoryId);
            var categoryFields = _accountingContext.CategoryField.Include(f => f.DataType).Where(f => categoryIds.Contains(f.CategoryId)).AsEnumerable();

            // Lấy thông tin value hiện tại
            var currentValues = _accountingContext.CategoryRowValue
                .Where(rv => rv.CategoryRowId == categoryRowId)
                .Join(_accountingContext.CategoryValue, rv => rv.CategoryValueId, v => v.CategoryValueId, (rv, v) => new
                {
                    rv.CategoryValueId,
                    rv.CategoryFieldId,
                    v.Value
                }).ToList();

            var updateFields = categoryFields
                .Where(f => currentValues.FirstOrDefault(v => v.CategoryFieldId == f.CategoryFieldId)?.Value != data.Values.FirstOrDefault(v => v.CategoryFieldId == f.CategoryFieldId)?.Value)
                .ToList();

            // Lấy thông tin field
            var requiredFields = updateFields.Where(f => !f.AutoIncrement && f.IsRequired);
            var uniqueFields = updateFields.Where(f => !f.AutoIncrement && f.IsUnique);
            var selectFields = updateFields.Where(f => !f.AutoIncrement && (f.FormTypeId == (int)EnumFormType.Select || f.FormTypeId == (int)EnumFormType.SearchTable));

            // Check field required
            var r = CheckRequired(data, requiredFields);
            if (!r.IsSuccess()) return r;

            // Check unique
            r = CheckUnique(data, uniqueFields, categoryRowId);
            if (!r.IsSuccess()) return r;

            // Check refer
            r = CheckRefer(ref data, selectFields);
            if (!r.IsSuccess()) return r;

            // Check value
            r = CheckValue(data, categoryFields);
            if (!r.IsSuccess()) return r;

            using (var trans = await _accountingContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // Duyệt danh sách field
                    foreach (var field in updateFields)
                    {
                        var oldValue = currentValues.FirstOrDefault(v => v.CategoryFieldId == field.CategoryFieldId);
                        var valueItem = data.Values.FirstOrDefault(v => v.CategoryFieldId == field.CategoryFieldId);

                        if (field.AutoIncrement)
                        {
                            continue;
                        }
                        else if (valueItem == null)  // Xóa giá trị cũ
                        {
                            if (field.FormTypeId != (int)EnumFormType.Select && field.FormTypeId != (int)EnumFormType.SearchTable)
                            {
                                // Xóa value cũ
                                var currentValue = _accountingContext.CategoryValue.First(v => v.CategoryValueId == oldValue.CategoryFieldId);
                                currentValue.IsDeleted = true;
                                currentValue.UpdatedUserId = updatedUserId;
                                await _accountingContext.SaveChangesAsync();

                            }
                            // Xóa mapping
                            var currentRowValue = _accountingContext.CategoryRowValue.First(rv => rv.CategoryRowId == categoryRowId && rv.CategoryFieldId == field.CategoryFieldId);
                            currentRowValue.IsDeleted = true;
                            currentRowValue.UpdatedUserId = updatedUserId;
                            await _accountingContext.SaveChangesAsync();
                        }
                        else if (oldValue == null) // Nếu giá trị cũ là null, tạo mới, map lại
                        {
                            if (field.FormTypeId != (int)EnumFormType.Select && field.FormTypeId != (int)EnumFormType.SearchTable)
                            {
                                CategoryValue newValue = new CategoryValue
                                {
                                    CategoryFieldId = field.CategoryFieldId,
                                    Value = valueItem.Value,
                                    UpdatedUserId = updatedUserId
                                };

                                _accountingContext.CategoryValue.Add(newValue);
                                await _accountingContext.SaveChangesAsync();
                                _accountingContext.CategoryRowValue.Add(new CategoryRowValue
                                {
                                    CategoryFieldId = field.CategoryFieldId,
                                    CategoryRowId = categoryRowId,
                                    CategoryValueId = newValue.CategoryValueId,
                                    UpdatedUserId = updatedUserId
                                });
                                await _accountingContext.SaveChangesAsync();
                            }
                            else
                            {
                                _accountingContext.CategoryRowValue.Add(new CategoryRowValue
                                {
                                    CategoryFieldId = field.CategoryFieldId,
                                    CategoryRowId = categoryRowId,
                                    CategoryValueId = valueItem.CategoryValueId,
                                    UpdatedUserId = updatedUserId
                                });
                                await _accountingContext.SaveChangesAsync();
                            }
                        }
                        else if (field.FormTypeId != (int)EnumFormType.Select && field.FormTypeId != (int)EnumFormType.SearchTable)
                        {
                            // Sửa value cũ
                            var currentValue = _accountingContext.CategoryValue.First(v => v.CategoryValueId == oldValue.CategoryFieldId);
                            currentValue.Value = valueItem.Value;
                            currentValue.UpdatedUserId = updatedUserId;
                            await _accountingContext.SaveChangesAsync();
                        }
                        else
                        {
                            // Sửa mapping giá trị mới
                            var currentRowValue = _accountingContext.CategoryRowValue.First(rv => rv.CategoryRowId == categoryRowId && rv.CategoryFieldId == field.CategoryFieldId);
                            currentRowValue.CategoryValueId = valueItem.CategoryValueId;
                            currentRowValue.UpdatedUserId = updatedUserId;
                            await _accountingContext.SaveChangesAsync();
                        }
                    }

                    trans.Commit();
                    await _activityLogService.CreateLog(EnumObjectType.Category, categoryRow.CategoryRowId, $"Cập nhật dòng dữ liệu {categoryRow.CategoryRowId}", data.JsonSerialize());
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

        public async Task<Enum> DeleteCategoryRow(int updatedUserId, int categoryId, int categoryRowId)
        {
            var categoryRow = await _accountingContext.CategoryRow.FirstOrDefaultAsync(c => c.CategoryRowId == categoryRowId && c.CategoryId == categoryId);
            if (categoryRow == null)
            {
                return CategoryErrorCode.CategoryRowNotFound;
            }
            // Lấy thông tin field
            var categoryIds = GetAllCategoryIds(categoryRow.CategoryId);
            var categoryFields = _accountingContext.CategoryField.Where(f => categoryIds.Contains(f.CategoryId)).AsEnumerable();

            // Check reference
            foreach (var field in categoryFields)
            {
                if (_accountingContext.CategoryField.Any(c => c.ReferenceCategoryFieldId == field.CategoryFieldId))
                {
                    int valueId = _accountingContext.CategoryRowValue
                        .Where(rv => rv.CategoryFieldId == field.CategoryFieldId && rv.CategoryRowId == categoryRowId)
                        .Select(rv => rv.CategoryValueId).FirstOrDefault();
                    bool isRefer = valueId > 0 && _accountingContext.CategoryRowValue.Any(rv => rv.CategoryValueId == valueId && rv.CategoryRowId != categoryRowId);
                    if (isRefer) return CategoryErrorCode.DestCategoryFieldAlreadyExisted;
                }
            }

            using var trans = await _accountingContext.Database.BeginTransactionAsync();
            try
            {
                // Delete row
                categoryRow.IsDeleted = true;
                categoryRow.UpdatedUserId = updatedUserId;
                foreach (var field in categoryFields)
                {
                    var categoryRowValue = _accountingContext.CategoryRowValue
                      .Where(rv => rv.CategoryFieldId == field.CategoryFieldId && rv.CategoryRowId == categoryRowId)
                      .FirstOrDefault();

                    if (categoryRowValue == null)
                    {
                        continue;
                    }

                    if (field.FormTypeId != (int)EnumFormType.Select && field.FormTypeId != (int)EnumFormType.SearchTable)
                    {
                        // Delete value
                        var value = _accountingContext.CategoryValue.FirstOrDefault(v => v.CategoryValueId == categoryRowValue.CategoryValueId);
                        value.IsDeleted = true;
                        value.UpdatedUserId = updatedUserId;
                    }
                    // Delete row-field-value
                    categoryRowValue.IsDeleted = true;
                    categoryRowValue.UpdatedUserId = updatedUserId;
                }
                await _accountingContext.SaveChangesAsync();
                trans.Commit();
                await _activityLogService.CreateLog(EnumObjectType.Category, categoryRowId, $"Xóa dòng dữ liệu {categoryRowId}", categoryRow.JsonSerialize());
                return GeneralCode.Success;
            }
            catch (Exception ex)
            {
                trans.Rollback();
                _logger.LogError(ex, "Delete");
                return GeneralCode.InternalError;
            }
        }

        private Enum CheckRefer(ref CategoryRowInputModel data, IEnumerable<CategoryField> selectFields)
        {
            // Check refer
            foreach (var field in selectFields)
            {
                var valueItem = data.Values.FirstOrDefault(v => v.CategoryFieldId == field.CategoryFieldId);
                if (valueItem != null)
                {

                    IQueryable<CategoryValueModel> query;
                    if (field.ReferenceCategoryFieldId.HasValue)
                    {
                        query = _accountingContext.CategoryValue
                            .Join(_accountingContext.CategoryRowValue, v => v.CategoryValueId, rv => rv.CategoryValueId, (v, rv) => new
                            {
                                v.CategoryValueId,
                                rv.CategoryFieldId,
                                v.Value
                            })
                            .Where(v => v.CategoryFieldId == field.ReferenceCategoryFieldId.Value)
                            .Select(v => new CategoryValueModel
                            {
                                CategoryFieldId = v.CategoryFieldId,
                                CategoryValueId = v.CategoryValueId,
                                Value = v.Value
                            });
                    }
                    else
                    {
                        query = _accountingContext.CategoryValue
                            .Where(v => v.CategoryFieldId == field.CategoryFieldId && v.IsDefault)
                            .Select(v => new CategoryValueModel
                            {
                                CategoryFieldId = v.CategoryFieldId,
                                CategoryValueId = v.CategoryValueId,
                                Value = v.Value
                            });
                    }
                    int referValueId = query.Where(v => v.Value == valueItem.Value).Select(v => v.CategoryValueId).FirstOrDefault();

                    valueItem.CategoryValueId = referValueId;


                    if (referValueId <= 0)
                    {
                        return CategoryErrorCode.ReferValueNotFound;
                    }
                }
            }
            return GeneralCode.Success;
        }

        private Enum CheckUnique(CategoryRowInputModel data, IEnumerable<CategoryField> uniqueFields, int? categoryRowId = null)
        {
            // Check unique
            foreach (var item in data.Values.Where(v => uniqueFields.Any(f => f.CategoryFieldId == v.CategoryFieldId)))
            {
                bool isExisted = _accountingContext.CategoryRowValue
                    .Join(_accountingContext.CategoryValue, rv => rv.CategoryValueId, v => v.CategoryValueId, (rv, v) => new
                    {
                        rv.CategoryFieldId,
                        rv.CategoryValueId,
                        rv.CategoryRowId,
                        v.Value
                    })
                    .Any(rfv => (categoryRowId.HasValue ? rfv.CategoryRowId != categoryRowId : true) && rfv.CategoryFieldId == item.CategoryFieldId && rfv.Value == item.Value);

                if (isExisted)
                {
                    return CategoryErrorCode.UniqueValueAlreadyExisted;
                }
            }
            return GeneralCode.Success;
        }

        private Enum CheckRequired(CategoryRowInputModel data, IEnumerable<CategoryField> requiredFields)
        {
            if (requiredFields.Count() > 0 && requiredFields.Any(rf => !data.Values.Any(v => v.CategoryFieldId == rf.CategoryFieldId && !string.IsNullOrWhiteSpace(v.Value))))
            {
                return CategoryErrorCode.RequiredFieldIsEmpty;
            }
            return GeneralCode.Success;
        }

        private Enum CheckValue(CategoryRowInputModel data, IEnumerable<CategoryField> categoryFields)
        {
            foreach (var field in categoryFields)
            {
                var valueItem = data.Values.FirstOrDefault(v => v.CategoryFieldId == field.CategoryFieldId);
                if (field.FormTypeId == (int)EnumFormType.Select || field.FormTypeId == (int)EnumFormType.SearchTable || field.AutoIncrement || valueItem == null || string.IsNullOrEmpty(valueItem.Value))
                {
                    continue;
                }

                var r = CheckValue(valueItem, field);
                if (!r.IsSuccess())
                {
                    return r;
                }
            }

            return GeneralCode.Success;
        }

        public async Task<ServiceResult> ImportCategoryRow(int updatedUserId, int categoryId, Stream stream)
        {
            try
            {
                string errFormat = "Dòng {0} : {1}";
                var category = _accountingContext.Category.FirstOrDefault(c => c.CategoryId == categoryId);
                if (category == null)
                {
                    return CategoryErrorCode.CategoryNotFound;
                }
                if (category.IsReadonly)
                {
                    return CategoryErrorCode.CategoryReadOnly;
                }
                if (!category.IsModule)
                {
                    return CategoryErrorCode.CategoryIsNotModule;
                }
                var reader = new ExcelReader(stream);

                // Lấy thông tin field
                var categoryIds = GetAllCategoryIds(categoryId);
                var categoryFields = _accountingContext.CategoryField
                    .Include(f => f.DataType)
                    .Where(f => categoryIds.Contains(f.CategoryId))
                    .ToList();

                List<CategoryRowInputModel> rowInputs = new List<CategoryRowInputModel>();

                string[][] data = reader.ReadFile(categoryFields.Where(f => !f.IsHidden && !f.AutoIncrement).Count(), 0, 1, 0);
                string[] fieldNames = data[0];
                for (int rowIndx = 1; rowIndx < data.Length; rowIndx++)
                {
                    string[] row = data[rowIndx];
                    CategoryRowInputModel rowInput = new CategoryRowInputModel();
                    for (int fieldIndx = 0; fieldIndx < fieldNames.Length; fieldIndx++)
                    {
                        string fieldName = fieldNames[fieldIndx];
                        var field = categoryFields.FirstOrDefault(f => f.Name == fieldName);
                        if (field == null) continue;

                        if (field.DataTypeId == (int)EnumDataType.Boolean)
                        {
                            bool value;

                            bool isBoolean = int.TryParse(row[fieldIndx], out int intValue) ? (value = intValue == 1 || intValue == 0) : bool.TryParse(row[fieldIndx], out value);

                            if (isBoolean)
                            {
                                row[fieldIndx] = value.ToString().ToLower();
                            }
                            else
                            {
                                return (CategoryErrorCode.CategoryValueInValid, string.Format(errFormat, rowIndx + 1, CategoryErrorCode.CategoryValueInValid.GetEnumDescription()));
                            }
                        }

                        rowInput.Values.Add(new CategoryValueModel
                        {
                            CategoryFieldId = field.CategoryFieldId,
                            CategoryValueId = 0,
                            Value = row[fieldIndx]
                        });
                    }

                    var requiredFields = categoryFields.Where(f => !f.AutoIncrement && f.IsRequired);
                    var uniqueFields = categoryFields.Where(f => !f.AutoIncrement && f.IsUnique);
                    var selectFields = categoryFields.Where(f => !f.AutoIncrement && (f.FormTypeId == (int)EnumFormType.Select || f.FormTypeId == (int)EnumFormType.SearchTable));

                    // Check field required
                    var r = CheckRequired(rowInput, requiredFields);
                    if (!r.IsSuccess()) return (r, string.Format(errFormat, rowIndx + 1, r.GetEnumDescription()));

                    // Check unique
                    r = CheckUnique(rowInput, uniqueFields);
                    if (!r.IsSuccess()) return (r, string.Format(errFormat, rowIndx + 1, r.GetEnumDescription()));

                    // Check refer
                    r = CheckRefer(ref rowInput, categoryFields);
                    if (!r.IsSuccess()) return (r, string.Format(errFormat, rowIndx + 1, r.GetEnumDescription()));

                    // Check value
                    r = CheckValue(rowInput, categoryFields);
                    if (!r.IsSuccess()) return (r, string.Format(errFormat, rowIndx + 1, r.GetEnumDescription()));

                    rowInputs.Add(rowInput);
                }

                using (var trans = await _accountingContext.Database.BeginTransactionAsync())
                {
                    try
                    {
                        foreach (var rowInput in rowInputs)
                        {
                            int categoryRowId = await InsertCategoryRowAsync(updatedUserId, categoryId, categoryFields, rowInput);
                        }
                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        _logger.LogError(ex, "Import");
                        return GeneralCode.InternalError;
                    }
                }

                return GeneralCode.Success;
            }
            catch (Exception ex)
            {
                return CategoryErrorCode.FormatFileInvalid;
            }
        }

        public async Task<ServiceResult<MemoryStream>> GetImportTemplateCategoryRow(int categoryId)
        {
            var category = _accountingContext.Category.FirstOrDefault(c => c.CategoryId == categoryId);
            if (category == null)
            {
                return CategoryErrorCode.CategoryNotFound;
            }
            if (category.IsReadonly)
            {
                return CategoryErrorCode.CategoryReadOnly;
            }
            // Lấy thông tin field
            var categoryIds = GetAllCategoryIds(categoryId);
            var categoryFields = _accountingContext.CategoryField
                .Where(f => categoryIds.Contains(f.CategoryId))
                .Where(f => !f.IsHidden && !f.AutoIncrement)
                .AsEnumerable();
            List<(string, byte[])[]> dataInRows = new List<(string, byte[])[]>();
            List<(string, byte[])> titles = new List<(string, byte[])>();
            List<(string, byte[])> names = new List<(string, byte[])>();
            byte[] titleRgb = new byte[3] { 60, 120, 216 };
            byte[] nameRgb = new byte[3] { 147, 196, 125 };
            foreach (var field in categoryFields)
            {
                titles.Add((field.Title, titleRgb));
                names.Add((field.Name, nameRgb));
            }
            dataInRows.Add(titles.ToArray());
            dataInRows.Add(names.ToArray());

            var writer = new ExcelWriter();
            writer.WriteToSheet(dataInRows, "Data");

            MemoryStream stream = await writer.WriteToStream();
            return stream;
        }
    }
}
