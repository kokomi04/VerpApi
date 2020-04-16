using AutoMapper;
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
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.AccountingDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Accountant.Model.Input;

namespace VErp.Services.Accountant.Service.Input.Implement
{
    public class InputTypeService : InputBaseService, IInputTypeService
    {
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly IMapper _mapper;

        public InputTypeService(AccountingDBContext accountingDBContext
            , IOptions<AppSetting> appSetting
            , ILogger<InputTypeService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            ) : base(accountingDBContext)
        {
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
            _mapper = mapper;
        }

        public async Task<ServiceResult<InputTypeFullModel>> GetInputType(int inputTypeId)
        {
            var inputType = await _accountingContext.InputType
                .Include(t => t.InputAreas)
                .ThenInclude(a => a.InputAreaFields)
                .FirstOrDefaultAsync(i => i.InputTypeId == inputTypeId);
            if (inputType == null)
            {
                return InputErrorCode.InputTypeNotFound;
            }
            InputTypeFullModel inputTypeFullModel = _mapper.Map<InputTypeFullModel>(inputType);
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

            using (var trans = await _accountingContext.Database.BeginTransactionAsync())
            {
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

            using (var trans = await _accountingContext.Database.BeginTransactionAsync())
            {
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
        }

        public async Task<Enum> DeleteInputType(int updatedUserId, int inputTypeId)
        {
            var inputType = await _accountingContext.InputType.FirstOrDefaultAsync(i => i.InputTypeId == inputTypeId);
            if (inputType == null)
            {
                return InputErrorCode.InputTypeNotFound;
            }

            using (var trans = await _accountingContext.Database.BeginTransactionAsync())
            {
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

                                // Xóa row version number
                                List<InputValueRowVersionNumber> inputValueRowVersionNumbers = _accountingContext.InputValueRowVersionNumber.Where(rvn => rvn.InputValueRowVersionId == inputValueRowVersion.InputValueRowVersionId).ToList();
                                foreach (InputValueRowVersionNumber inputValueRowVersionNumber in inputValueRowVersionNumbers)
                                {
                                    inputValueRowVersionNumber.IsDeleted = true;
                                    inputValueRowVersionNumber.UpdatedByUserId = updatedUserId;
                                    await _accountingContext.SaveChangesAsync();
                                }
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
        }

    }
}
