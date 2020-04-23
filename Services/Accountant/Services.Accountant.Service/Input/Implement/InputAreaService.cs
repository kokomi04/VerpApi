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
    public class InputAreaService : InputBaseService, IInputAreaService
    {
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly IMapper _mapper;

        public InputAreaService(AccountingDBContext accountingDBContext
            , IOptions<AppSetting> appSetting
            , ILogger<InputAreaService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            ) : base(accountingDBContext)
        {
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
            _mapper = mapper;
        }

        public async Task<ServiceResult<InputAreaOutputModel>> GetInputArea(int inputTypeId, int inputAreaId)
        {
            var inputArea = await _accountingContext.InputArea
                .FirstOrDefaultAsync(i => i.InputTypeId == inputTypeId && i.InputAreaId == inputAreaId);
            if (inputArea == null)
            {
                return InputErrorCode.InputTypeNotFound;
            }
            InputAreaOutputModel inputAreaOuputModel = _mapper.Map<InputAreaOutputModel>(inputArea);
            return inputAreaOuputModel;
        }

        public async Task<PageData<InputAreaOutputModel>> GetInputAreas(int inputTypeId, string keyword, int page, int size)
        {
            keyword = (keyword ?? "").Trim();

            var query = _accountingContext.InputArea.Where(a => a.InputTypeId == inputTypeId).AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(a => a.InputAreaCode.Contains(keyword) || a.Title.Contains(keyword));
            }

            query = query.OrderBy(c => c.Title);
            var total = await query.CountAsync();
            if (size > 0)
            {
                query = query.Skip((page - 1) * size).Take(size);
            }
            List<InputAreaOutputModel> lst = new List<InputAreaOutputModel>();

            foreach (var item in query)
            {
                InputAreaOutputModel inputAreaModel = _mapper.Map<InputAreaOutputModel>(item);
                lst.Add(inputAreaModel);
            }

            return (lst, total);
        }

        public async Task<ServiceResult<int>> AddInputArea(int updatedUserId, int inputTypeId, InputAreaInputModel data)
        {
            var existedInput = await _accountingContext.InputArea
                .FirstOrDefaultAsync(a => a.InputTypeId == inputTypeId && (a.InputAreaCode == data.InputAreaCode || a.Title == data.Title));
            if (existedInput != null)
            {
                if (string.Compare(existedInput.InputAreaCode, data.InputAreaCode, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return InputErrorCode.InputCodeAlreadyExisted;
                }

                return InputErrorCode.InputTitleAlreadyExisted;
            }

            using (var trans = await _accountingContext.Database.BeginTransactionAsync())
            {
                try
                {
                    InputArea inputArea = _mapper.Map<InputArea>(data);
                    inputArea.InputTypeId = inputTypeId;
                    inputArea.UpdatedByUserId = updatedUserId;
                    inputArea.CreatedByUserId = updatedUserId;
                    await _accountingContext.InputArea.AddAsync(inputArea);
                    await _accountingContext.SaveChangesAsync();

                    trans.Commit();
                    await _activityLogService.CreateLog(EnumObjectType.InputType, inputArea.InputAreaId, $"Thêm vùng thông tin {inputArea.Title}", data.JsonSerialize());
                    return inputArea.InputAreaId;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    _logger.LogError(ex, "Create");
                    return GeneralCode.InternalError;
                }
            }
        }

        public async Task<Enum> UpdateInputArea(int updatedUserId, int inputTypeId, int inputAreaId, InputAreaInputModel data)
        {
            var inputArea = await _accountingContext.InputArea.FirstOrDefaultAsync(a => a.InputTypeId == inputTypeId && a.InputAreaId == inputAreaId);
            if (inputArea == null)
            {
                return InputErrorCode.InputAreaNotFound;
            }
            if (inputArea.InputAreaCode != data.InputAreaCode || inputArea.Title != data.Title)
            {
                var existedInput = await _accountingContext.InputArea
                    .FirstOrDefaultAsync(a => a.InputTypeId == inputTypeId && a.InputAreaId != inputAreaId && (a.InputAreaCode == data.InputAreaCode || a.Title == data.Title));
                if (existedInput != null)
                {
                    if (string.Compare(existedInput.InputAreaCode, data.InputAreaCode, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return InputErrorCode.InputAreaCodeAlreadyExisted;
                    }

                    return InputErrorCode.InputAreaTitleAlreadyExisted;
                }
            }

            using (var trans = await _accountingContext.Database.BeginTransactionAsync())
            {
                try
                {
                    inputArea.InputAreaCode = data.InputAreaCode;
                    inputArea.Title = data.Title;
                    inputArea.IsMultiRow = data.IsMultiRow;
                    inputArea.Columns = data.Columns;
                    inputArea.UpdatedByUserId = updatedUserId;
                    await _accountingContext.SaveChangesAsync();

                    trans.Commit();
                    await _activityLogService.CreateLog(EnumObjectType.InputType, inputArea.InputAreaId, $"Cập nhật vùng dữ liệu {inputArea.Title}", data.JsonSerialize());
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

        public async Task<Enum> DeleteInputArea(int updatedUserId, int inputTypeId, int inputAreaId)
        {
            var inputArea = await _accountingContext.InputArea.FirstOrDefaultAsync(a => a.InputTypeId == inputTypeId && a.InputAreaId == inputAreaId);
            if (inputArea == null)
            {
                return InputErrorCode.InputAreaNotFound;
            }

            using (var trans = await _accountingContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // Xóa field
                    List<InputAreaField> inputAreaFields = _accountingContext.InputAreaField.Where(f => f.InputAreaId == inputAreaId).ToList();
                    foreach (InputAreaField inputAreaField in inputAreaFields)
                    {
                        inputAreaField.IsDeleted = true;
                        inputAreaField.UpdatedByUserId = updatedUserId;
                        await _accountingContext.SaveChangesAsync();
                    }

                    // Xóa row
                    List<InputValueRow> inputValueRows = _accountingContext.InputValueRow.Where(r => r.InputAreaId == inputAreaId).ToList();
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

                    // Xóa area
                    inputArea.IsDeleted = true;
                    inputArea.UpdatedByUserId = updatedUserId;
                    await _accountingContext.SaveChangesAsync();
                    trans.Commit();
                    await _activityLogService.CreateLog(EnumObjectType.InventoryInput, inputArea.InputTypeId, $"Xóa chứng từ {inputArea.Title}", inputArea.JsonSerialize());
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
