﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
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
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.AccountingDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Accountant.Model.Category;
using VErp.Services.Accountant.Model.Input;
using CategoryEntity = VErp.Infrastructure.EF.AccountingDB.Category;

namespace VErp.Services.Accountant.Service.Input.Implement
{
    public class InputTypeService : AccoutantBaseService, IInputTypeService
    {
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;

        public InputTypeService(AccountingDBContext accountingDBContext
            , IOptions<AppSetting> appSetting
            , ILogger<InputTypeService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            ) : base(accountingDBContext, appSetting, mapper)
        {
            _logger = logger;
            _activityLogService = activityLogService;
        }

        public async Task<ServiceResult<InputTypeFullModel>> GetInputType(int inputTypeId)
        {
            var inputType = await _accountingContext.InputType
                .Include(t => t.InputAreas)
                .ThenInclude(a => a.InputAreaFields)
                .ThenInclude(f => f.SourceCategoryField)
                .Include(t => t.InputAreas)
                .ThenInclude(a => a.InputAreaFields)
                .ThenInclude(f => f.SourceCategoryTitleField)
                .Include(t => t.InputAreas)
                .ThenInclude(a => a.InputAreaFields)
                .ThenInclude(f => f.InputAreaFieldStyle)
                .FirstOrDefaultAsync(i => i.InputTypeId == inputTypeId);
            if (inputType == null)
            {
                return InputErrorCode.InputTypeNotFound;
            }
            InputTypeFullModel inputTypeFullModel = _mapper.Map<InputTypeFullModel>(inputType);

            foreach (var area in inputTypeFullModel.InputAreas)
            {
                foreach (var field in area.InputAreaFields)
                {
                    if (field.SourceCategoryField != null)
                    {
                        CategoryEntity sourceCategory = _accountingContext.Category.FirstOrDefault(c => c.CategoryId == field.SourceCategoryField.CategoryId);
                        field.SourceCategory = _mapper.Map<CategoryModel>(sourceCategory);
                    }
                }
            }

            return inputTypeFullModel;
        }

        public async Task<PageData<InputTypeModel>> GetInputTypes(string keyword, int page, int size)
        {
            keyword = (keyword ?? "").Trim();

            var query = _accountingContext.InputType.AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(i => i.InputTypeCode.Contains(keyword) || i.Title.Contains(keyword));
            }

            query = query.OrderBy(c => c.Title);
            var total = await query.CountAsync();
            if (size > 0)
            {
                query = query.Skip((page - 1) * size).Take(size);
            }
            List<InputTypeModel> lst = new List<InputTypeModel>();

            foreach (var item in query)
            {
                InputTypeModel inputModel = _mapper.Map<InputTypeModel>(item);
                lst.Add(inputModel);
            }

            return (lst, total);
        }

        public async Task<ServiceResult<int>> AddInputType(int updatedUserId, InputTypeModel data)
        {
            var existedInput = await _accountingContext.InputType
                .FirstOrDefaultAsync(i => i.InputTypeCode == data.InputTypeCode || i.Title == data.Title);
            if (existedInput != null)
            {
                if (string.Compare(existedInput.InputTypeCode, data.InputTypeCode, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return InputErrorCode.InputCodeAlreadyExisted;
                }

                return InputErrorCode.InputTitleAlreadyExisted;
            }

            using var trans = await _accountingContext.Database.BeginTransactionAsync();
            try
            {
                InputType inputType = _mapper.Map<InputType>(data);
                inputType.UpdatedByUserId = updatedUserId;
                inputType.CreatedByUserId = updatedUserId;
                await _accountingContext.InputType.AddAsync(inputType);
                await _accountingContext.SaveChangesAsync();

                trans.Commit();
                await _activityLogService.CreateLog(EnumObjectType.InputType, inputType.InputTypeId, $"Thêm chứng từ {inputType.Title}", data.JsonSerialize());
                return inputType.InputTypeId;
            }
            catch (Exception ex)
            {
                trans.Rollback();
                _logger.LogError(ex, "Create");
                return GeneralCode.InternalError;
            }
        }

        public async Task<Enum> UpdateInputType(int updatedUserId, int inputTypeId, InputTypeModel data)
        {
            var inputType = await _accountingContext.InputType.FirstOrDefaultAsync(i => i.InputTypeId == inputTypeId);
            if (inputType == null)
            {
                return InputErrorCode.InputTypeNotFound;
            }
            if (inputType.InputTypeCode != data.InputTypeCode || inputType.Title != data.Title)
            {
                var existedInput = await _accountingContext.InputType
                    .FirstOrDefaultAsync(i => i.InputTypeId != inputTypeId && (i.InputTypeCode == data.InputTypeCode || i.Title == data.Title));
                if (existedInput != null)
                {
                    if (string.Compare(existedInput.InputTypeCode, data.InputTypeCode, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return InputErrorCode.InputCodeAlreadyExisted;
                    }

                    return InputErrorCode.InputTitleAlreadyExisted;
                }
            }

            using var trans = await _accountingContext.Database.BeginTransactionAsync();
            try
            {
                inputType.InputTypeCode = data.InputTypeCode;
                inputType.Title = data.Title;
                inputType.UpdatedByUserId = updatedUserId;
                await _accountingContext.SaveChangesAsync();

                trans.Commit();
                await _activityLogService.CreateLog(EnumObjectType.InputType, inputType.InputTypeId, $"Cập nhật chứng từ {inputType.Title}", data.JsonSerialize());
                return GeneralCode.Success;
            }
            catch (Exception ex)
            {
                trans.Rollback();
                _logger.LogError(ex, "Update");
                return GeneralCode.InternalError;
            }
        }

        public async Task<Enum> DeleteInputType(int updatedUserId, int inputTypeId)
        {
            var inputType = await _accountingContext.InputType.FirstOrDefaultAsync(i => i.InputTypeId == inputTypeId);
            if (inputType == null)
            {
                return InputErrorCode.InputTypeNotFound;
            }

            using var trans = await _accountingContext.Database.BeginTransactionAsync();
            try
            {
                // Xóa area
                List<InputArea> inputAreas = _accountingContext.InputArea.Where(a => a.InputTypeId == inputTypeId).ToList();
                foreach (InputArea inputArea in inputAreas)
                {
                    inputArea.IsDeleted = true;
                    inputArea.UpdatedByUserId = updatedUserId;
                    await _accountingContext.SaveChangesAsync();

                    // Xóa field
                    List<InputAreaField> inputAreaFields = _accountingContext.InputAreaField.Where(f => f.InputAreaId == inputArea.InputAreaId).ToList();
                    foreach (InputAreaField inputAreaField in inputAreaFields)
                    {
                        inputAreaField.IsDeleted = true;
                        inputAreaField.UpdatedByUserId = updatedUserId;
                        await _accountingContext.SaveChangesAsync();

                    }
                }

                // Xóa Bill
                List<InputValueBill> inputValueBills = _accountingContext.InputValueBill.Where(b => b.InputTypeId == inputTypeId).ToList();
                foreach (InputValueBill inputValueBill in inputValueBills)
                {
                    inputValueBill.IsDeleted = true;
                    inputValueBill.UpdatedByUserId = updatedUserId;
                    await _accountingContext.SaveChangesAsync();

                    // Xóa row
                    List<InputValueRow> inputValueRows = _accountingContext.InputValueRow.Where(r => r.InputValueBillId == inputValueBill.InputValueBillId).ToList();
                    foreach (InputValueRow inputValueRow in inputValueRows)
                    {
                        inputValueRow.IsDeleted = true;
                        inputValueRow.UpdatedByUserId = updatedUserId;
                        await _accountingContext.SaveChangesAsync();

                        // Xóa row version
                        List<InputValueRowVersion> inputValueRowVersions = _accountingContext.InputValueRowVersion.Where(rv => rv.InputValueRowId == inputValueRow.InputValueRowId).ToList();
                        foreach (InputValueRowVersion inputValueRowVersion in inputValueRowVersions)
                        {
                            inputValueRowVersion.IsDeleted = true;
                            inputValueRowVersion.UpdatedByUserId = updatedUserId;
                            await _accountingContext.SaveChangesAsync();

                            //// Xóa row version number
                            //List<InputValueRowVersionNumber> inputValueRowVersionNumbers = _accountingContext.InputValueRowVersionNumber.Where(rvn => rvn.InputValueRowVersionId == inputValueRowVersion.InputValueRowVersionId).ToList();
                            //foreach (InputValueRowVersionNumber inputValueRowVersionNumber in inputValueRowVersionNumbers)
                            //{
                            //    inputValueRowVersionNumber.IsDeleted = true;
                            //    inputValueRowVersionNumber.UpdatedByUserId = updatedUserId;
                            //    await _accountingContext.SaveChangesAsync();
                            //}
                        }
                    }
                }
                // Xóa type
                inputType.IsDeleted = true;
                inputType.UpdatedByUserId = updatedUserId;
                await _accountingContext.SaveChangesAsync();
                trans.Commit();
                await _activityLogService.CreateLog(EnumObjectType.InventoryInput, inputType.InputTypeId, $"Xóa chứng từ {inputType.Title}", inputType.JsonSerialize());
                return GeneralCode.Success;

            }
            catch (Exception ex)
            {
                trans.Rollback();
                _logger.LogError(ex, "Delete");
                return GeneralCode.InternalError;
            }
        }

      

       
        public async Task<IList<InputTypeViewModelList>> InputTypeViewList(int inputTypeId)
        {
            return await _accountingContext.InputTypeView.Where(v => v.InputTypeId == inputTypeId).ProjectTo<InputTypeViewModelList>(_mapper.ConfigurationProvider).ToListAsync();
        }


        public async Task<InputTypeBasicOutput> GetInputTypeBasicInfo(int inputTypeId)
        {
            var inputTypeInfo = await _accountingContext.InputType.AsNoTracking().Where(t => t.InputTypeId == inputTypeId).ProjectTo<InputTypeBasicOutput>(_mapper.ConfigurationProvider).FirstOrDefaultAsync();

            inputTypeInfo.Areas = await _accountingContext.InputArea.AsNoTracking().Where(a => a.InputTypeId == inputTypeId).ProjectTo<InputAreaBasicOutput>(_mapper.ConfigurationProvider).ToListAsync();

            var fields = await _accountingContext.InputAreaField.AsNoTracking().Where(a => a.InputTypeId == inputTypeId).ProjectTo<InputAreaFieldBasicOutput>(_mapper.ConfigurationProvider).ToListAsync();

            var views= await _accountingContext.InputTypeView.AsNoTracking().Where(t => t.InputTypeId == inputTypeId).ProjectTo<InputTypeViewModelList>(_mapper.ConfigurationProvider).ToListAsync();

            foreach (var item in inputTypeInfo.Areas)
            {
                item.Fields = fields.Where(f => f.InputAreaId == item.InputAreaId).ToList();
            }

            inputTypeInfo.Views = views;

            return inputTypeInfo;
        }

        public async Task<InputTypeViewModel> GetInputTypeViewInfo(int inputTypeId, int inputTypeViewId)
        {
            var info = await _accountingContext.InputTypeView.AsNoTracking().Where(t => t.InputTypeId == inputTypeId && t.InputTypeViewId == inputTypeViewId).ProjectTo<InputTypeViewModel>(_mapper.ConfigurationProvider).FirstOrDefaultAsync();

            if (info == null)
            {
                throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy cấu hình trong hệ thống");
            }

            var fields = await _accountingContext.InputTypeViewField.AsNoTracking()
                .Where(t => t.InputTypeViewId == inputTypeViewId)
                .ProjectTo<InputTypeViewFieldModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

            info.Fields = fields;

            return info;
        }

        public async Task<int> InputTypeViewCreate(int inputTypeId, InputTypeViewModel model)
        {
            using var trans = await _accountingContext.Database.BeginTransactionAsync();
            try
            {
                var info = _mapper.Map<InputTypeView>(model);

                info.InputTypeId = inputTypeId;

                await _accountingContext.InputTypeView.AddAsync(info);
                await _accountingContext.SaveChangesAsync();

                await InputTypeViewFieldAddRange(info.InputTypeViewId, model.Fields);

                await _accountingContext.SaveChangesAsync();

                await trans.CommitAsync();

                return info.InputTypeViewId;
            }
            catch (Exception ex)
            {
                await trans.RollbackAsync();
                _logger.LogError(ex, "InputTypeViewCreate");
                throw ex;
            }

        }


        public async Task<Enum> InputTypeViewUpdate(int inputTypeViewId, InputTypeViewModel model)
        {
            using var trans = await _accountingContext.Database.BeginTransactionAsync();
            try
            {
                var info = await _accountingContext.InputTypeView.FirstOrDefaultAsync(v => v.InputTypeViewId == inputTypeViewId);

                if (info == null) throw new BadRequestException(GeneralCode.ItemNotFound, "View không tồn tại");

                _mapper.Map(model, info);

                var oldFields = await _accountingContext.InputTypeViewField.Where(f => f.InputTypeViewId == inputTypeViewId).ToListAsync();

                _accountingContext.InputTypeViewField.RemoveRange(oldFields);

                await InputTypeViewFieldAddRange(inputTypeViewId, model.Fields);

                await _accountingContext.SaveChangesAsync();

                await trans.CommitAsync();

                return GeneralCode.Success;
            }
            catch (Exception ex)
            {
                await trans.RollbackAsync();
                _logger.LogError(ex, "InputTypeViewUpdate");
                throw ex;
            }
        }

        public async Task<Enum> InputTypeViewDelete(int inputTypeViewId)
        {
            var info = await _accountingContext.InputTypeView.FirstOrDefaultAsync(v => v.InputTypeViewId == inputTypeViewId);

            if (info == null) throw new BadRequestException(GeneralCode.ItemNotFound, "View không tồn tại");

            info.IsDeleted = true;
            info.DeletedDatetimeUtc = DateTime.UtcNow;

            await _accountingContext.SaveChangesAsync();

            return GeneralCode.Success;

        }

        private async Task InputTypeViewFieldAddRange(int inputTypeViewId, IList<InputTypeViewFieldModel> fieldModels)
        {
            var categoryFieldIds = fieldModels.Where(f => f.ReferenceCategoryFieldId.HasValue).Select(f => f.ReferenceCategoryFieldId.Value).ToList();
            categoryFieldIds.Union(fieldModels.Where(f => f.ReferenceCategoryTitleFieldId.HasValue).Select(f => f.ReferenceCategoryFieldId.Value).ToList());

            if (categoryFieldIds.Count > 0)
            {

                var categoryFields = (await _accountingContext.CategoryField
                    .Where(f => categoryFieldIds.Contains(f.CategoryFieldId))
                    .Select(f => new { f.CategoryFieldId, f.CategoryId })
                    .AsNoTracking()
                    .ToListAsync())
                    .ToDictionary(f => f.CategoryFieldId, f => f);

                foreach (var f in fieldModels)
                {
                    if (f.ReferenceCategoryFieldId.HasValue && categoryFields.TryGetValue(f.ReferenceCategoryFieldId.Value, out var cateField) && cateField.CategoryId != f.ReferenceCategoryId)
                    {
                        throw new BadRequestException(GeneralCode.InvalidParams, "Trường dữ liệu của danh mục không thuộc danh mục");
                    }

                    if (f.ReferenceCategoryTitleFieldId.HasValue && categoryFields.TryGetValue(f.ReferenceCategoryTitleFieldId.Value, out cateField) && cateField.CategoryId != f.ReferenceCategoryId)
                    {
                        throw new BadRequestException(GeneralCode.InvalidParams, "Trường hiển thị của danh mục không thuộc danh mục");
                    }
                }

            }

            var fields = fieldModels.Select(f => _mapper.Map<InputTypeViewField>(f)).ToList();

            foreach (var f in fields)
            {
                f.InputTypeViewId = inputTypeViewId;
            }

            await _accountingContext.InputTypeViewField.AddRangeAsync(fields);

        }

      
    }
}
