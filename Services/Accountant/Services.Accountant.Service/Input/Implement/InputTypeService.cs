using AutoMapper;
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
                .Where(i => i.InputTypeId == inputTypeId)
                .Include(t => t.InputArea)
                .ThenInclude(a => a.InputAreaField)
                .ThenInclude(f => f.ReferenceCategoryField)
                .Include(t => t.InputArea)
                .ThenInclude(a => a.InputAreaField)
                .ThenInclude(f => f.ReferenceCategoryTitleField)
                .Include(t => t.InputArea)
                .ThenInclude(a => a.InputAreaField)
                .ThenInclude(f => f.InputAreaFieldStyle)
                .ProjectTo<InputTypeFullModel>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();
            if (inputType == null)
            {
                return InputErrorCode.InputTypeNotFound;
            }
            return inputType;
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
            var lst = await query.ProjectTo<InputTypeModel>(_mapper.ConfigurationProvider).OrderBy(t => t.SortOrder).ToListAsync();
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
                inputType.SortOrder = data.SortOrder;
                inputType.InputTypeGroupId = data.InputTypeGroupId;

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

            inputTypeInfo.Areas = await _accountingContext.InputArea.AsNoTracking().Where(a => a.InputTypeId == inputTypeId).OrderBy(a => a.SortOrder).ProjectTo<InputAreaBasicOutput>(_mapper.ConfigurationProvider).ToListAsync();

            var fields = await _accountingContext.InputAreaField.AsNoTracking().Where(a => a.InputTypeId == inputTypeId).OrderBy(f => f.SortOrder).ProjectTo<InputAreaFieldBasicOutput>(_mapper.ConfigurationProvider).ToListAsync();

            var views = await _accountingContext.InputTypeView.AsNoTracking().Where(t => t.InputTypeId == inputTypeId).OrderByDescending(v => v.IsDefault).ProjectTo<InputTypeViewModelList>(_mapper.ConfigurationProvider).ToListAsync();

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
                var inputTypeInfo = await _accountingContext.InputType.FirstOrDefaultAsync(t => t.InputTypeId == inputTypeId);
                if (inputTypeInfo == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy loại chứng từ");

                var info = _mapper.Map<InputTypeView>(model);

                info.InputTypeId = inputTypeId;

                await _accountingContext.InputTypeView.AddAsync(info);
                await _accountingContext.SaveChangesAsync();

                await InputTypeViewFieldAddRange(info.InputTypeViewId, model.Fields);

                await _accountingContext.SaveChangesAsync();

                await trans.CommitAsync();

                await _activityLogService.CreateLog(EnumObjectType.InputTypeView, info.InputTypeViewId, $"Tạo bộ lọc {info.InputTypeViewName} cho chứng từ  {inputTypeInfo.Title}", model.JsonSerialize());

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

                var inputTypeInfo = await _accountingContext.InputType.FirstOrDefaultAsync(t => t.InputTypeId == info.InputTypeId);
                if (inputTypeInfo == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy loại chứng từ");


                _mapper.Map(model, info);

                var oldFields = await _accountingContext.InputTypeViewField.Where(f => f.InputTypeViewId == inputTypeViewId).ToListAsync();

                _accountingContext.InputTypeViewField.RemoveRange(oldFields);

                await InputTypeViewFieldAddRange(inputTypeViewId, model.Fields);

                await _accountingContext.SaveChangesAsync();

                await trans.CommitAsync();

                await _activityLogService.CreateLog(EnumObjectType.InputTypeView, info.InputTypeViewId, $"Cập nhật bộ lọc {info.InputTypeViewName} cho chứng từ  {inputTypeInfo.Title}", model.JsonSerialize());

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


            var inputTypeInfo = await _accountingContext.InputType.FirstOrDefaultAsync(t => t.InputTypeId == info.InputTypeId);
            if (inputTypeInfo == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Không tìm thấy loại chứng từ");


            await _accountingContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.InputTypeView, info.InputTypeViewId, $"Xóa bộ lọc {info.InputTypeViewName} chứng từ  {inputTypeInfo.Title}", new { inputTypeViewId }.JsonSerialize());

            return GeneralCode.Success;

        }



        #region InputTypeGroup
        public async Task<int> InputTypeGroupCreate(InputTypeGroupModel model)
        {
            var info = _mapper.Map<InputTypeGroup>(model);
            await _accountingContext.InputTypeGroup.AddAsync(info);
            await _accountingContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.InputTypeGroup, info.InputTypeGroupId, $"Thêm nhóm chứng từ {info.InputTypeGroupName}", model.JsonSerialize());

            return info.InputTypeGroupId;
        }

        public async Task<bool> InputTypeGroupUpdate(int inputTypeGroupId, InputTypeGroupModel model)
        {
            var info = await _accountingContext.InputTypeGroup.FirstOrDefaultAsync(g => g.InputTypeGroupId == inputTypeGroupId);

            if (info == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Nhóm chứng từ không tồn tại");

            _mapper.Map(model, info);

            await _accountingContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.InputTypeGroup, info.InputTypeGroupId, $"Cập nhật nhóm chứng từ {info.InputTypeGroupName}", model.JsonSerialize());

            return true;
        }

        public async Task<bool> InputTypeGroupDelete(int inputTypeGroupId)
        {
            var info = await _accountingContext.InputTypeGroup.FirstOrDefaultAsync(g => g.InputTypeGroupId == inputTypeGroupId);

            if (info == null) throw new BadRequestException(GeneralCode.ItemNotFound, "Nhóm chứng từ không tồn tại");

            info.IsDeleted = true;

            await _accountingContext.SaveChangesAsync();

            await _activityLogService.CreateLog(EnumObjectType.InputTypeGroup, info.InputTypeGroupId, $"Xóa nhóm chứng từ {info.InputTypeGroupName}", new { inputTypeGroupId }.JsonSerialize());

            return true;
        }

        public async Task<IList<InputTypeGroupList>> InputTypeGroupList()
        {
            return await _accountingContext.InputTypeGroup.ProjectTo<InputTypeGroupList>(_mapper.ConfigurationProvider).OrderBy(g => g.SortOrder).ToListAsync();
        }

        #endregion
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
