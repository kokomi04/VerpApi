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
using VErp.Services.Accountant.Model.Input;

namespace VErp.Services.Accountant.Service.Input.Implement
{
    public class InputValueBillService : InputBaseService, IInputValueBillService
    {
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly IMapper _mapper;
        public InputValueBillService(AccountingDBContext accountingContext
            , IOptions<AppSetting> appSetting
            , ILogger<InputValueBillService> logger
            , IActivityLogService activityLogService
             , IMapper mapper
            ) : base(accountingContext)
        {
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
            _mapper = mapper;
        }

        public async Task<PageData<InputValueBillOutputModel>> GetInputValueBills(int inputTypeId, string keyword, int page, int size)
        {
            var query = _accountingContext.InputValueBill
                .Include(b => b.InputValueRows)
                .ThenInclude(r => r.InputValueRowVersions.Where(rv => rv.InputValueRowVersionId == r.LastestInputValueRowVersionId))
                .Where(b => b.InputTypeId == inputTypeId);

            // search
            if (!string.IsNullOrEmpty(keyword))
            {

                query = query.Where(b => b.InputValueRows.Any(r => r.InputValueRowVersions.Any(rv => rv.Field0.Contains(keyword)
                || rv.Field1.Contains(keyword)
                || rv.Field2.Contains(keyword)
                || rv.Field3.Contains(keyword)
                || rv.Field4.Contains(keyword)
                || rv.Field5.Contains(keyword)
                || rv.Field6.Contains(keyword)
                || rv.Field7.Contains(keyword)
                || rv.Field8.Contains(keyword)
                || rv.Field9.Contains(keyword)
                || rv.Field10.Contains(keyword)
                || rv.Field11.Contains(keyword)
                || rv.Field11.Contains(keyword)
                || rv.Field12.Contains(keyword)
                || rv.Field13.Contains(keyword)
                || rv.Field14.Contains(keyword)
                || rv.Field15.Contains(keyword)
                || rv.Field16.Contains(keyword)
                || rv.Field17.Contains(keyword)
                || rv.Field18.Contains(keyword)
                || rv.Field19.Contains(keyword)
                || rv.Field20.Contains(keyword)
                )));

            }

            var total = await query.CountAsync();
            if (size > 0)
            {
                query = query.Skip((page - 1) * size).Take(size);
            }

            var lst = query.Select(b => _mapper.Map<InputValueBillOutputModel>(b)).ToList();


            return (lst, total);
        }

        public async Task<ServiceResult<InputValueBillOutputModel>> GetInputValueBill(int inputTypeId, long inputValueBillId)
        {
            // Check exist
            var inputValueBill = await _accountingContext.InputValueBill
                .Include(b => b.InputValueRows)
                .ThenInclude(r => r.InputValueRowVersions.Where(rv => rv.InputValueRowVersionId == r.LastestInputValueRowVersionId))
                .FirstOrDefaultAsync(i => i.InputTypeId == inputTypeId && i.InputValueBillId == inputValueBillId);
            if (inputValueBill == null)
            {
                return InputErrorCode.InputValueBillNotFound;
            }

            var output = _mapper.Map<InputValueBillOutputModel>(inputValueBill);

            return output;
        }

        public async Task<ServiceResult<long>> AddInputValueBill(int updatedUserId, int inputTypeId, InputValueBillInputModel data)
        {
            // Validate
            var inputType = _accountingContext.InputType.FirstOrDefault(i => i.InputTypeId == inputTypeId);
            if (inputType == null)
            {
                return InputErrorCode.InputTypeNotFound;
            }

            // Lấy thông tin field
            var inputAreaFields = _accountingContext.InputAreaField
                .Include(f => f.DataType)
                .Where(f => f.InputTypeId == inputTypeId).AsEnumerable();

            var requiredFields = inputAreaFields.Where(f => !f.IsAutoIncrement && f.IsRequire);
            var uniqueFields = inputAreaFields.Where(f => !f.IsAutoIncrement && f.IsUnique);
            var selectFields = inputAreaFields.Where(f => !f.IsAutoIncrement && ((EnumFormType)f.FormTypeId).IsRef());


            // Check field required
            var r = CheckRequired(data, requiredFields);
            if (!r.IsSuccess()) return r;

            // Check unique
            r = CheckUnique(data, uniqueFields);
            if (!r.IsSuccess()) return r;

            // Check refer


            // Check value


            using (var trans = await _accountingContext.Database.BeginTransactionAsync())
            {
                try
                {
                    // Insert bill
                    var inputValueBill = _mapper.Map<InputValueBill>(data);
                    inputValueBill.UpdatedByUserId = updatedUserId;
                    inputValueBill.CreatedByUserId = updatedUserId;
                    await _accountingContext.InputValueBill.AddAsync(inputValueBill);
                    await _accountingContext.SaveChangesAsync();

                    // Insert row
                    foreach (var rowModel in data.InputValueRows)
                    {

                        var inputValueRow = _mapper.Map<InputValueRow>(rowModel);
                        inputValueRow.UpdatedByUserId = updatedUserId;
                        inputValueRow.CreatedByUserId = updatedUserId;
                        inputValueRow.InputValueBillId = inputValueBill.InputValueBillId;
                        await _accountingContext.InputValueRow.AddAsync(inputValueRow);
                        await _accountingContext.SaveChangesAsync();

                        long lastedRowVersionId = 0;
                        // Insert row version
                        foreach (var rowVersion in rowModel.InputValueRowVersions)
                        {
                            var inputValueRowVersion = _mapper.Map<InputValueRowVersion>(rowVersion);
                            inputValueRowVersion.UpdatedByUserId = updatedUserId;
                            inputValueRowVersion.CreatedByUserId = updatedUserId;
                            inputValueRowVersion.InputValueRowId = inputValueRow.InputValueRowId;
                            await _accountingContext.InputValueRowVersion.AddAsync(inputValueRowVersion);
                            await _accountingContext.SaveChangesAsync();
                            lastedRowVersionId = inputValueRowVersion.InputValueRowVersionId;

                            // Insert row version number
                            var inputValueRowVersionNumber = new InputValueRowVersionNumber
                            {
                                InputValueRowVersionId = inputValueRowVersion.InputValueRowVersionId
                            };
                            for (int fieldIndx = 0; fieldIndx < Numbers.INPUT_TYPE_FIELD_NUMBER; fieldIndx++)
                            {
                                string fieldName = string.Format(StringFormats.INPUT_TYPE_FIELDNAME_FORMAT, fieldIndx);
                                long valueInNumber = 0;
                                typeof(InputValueRowVersionNumber).GetProperty(fieldName).SetValue(inputValueRowVersionNumber, valueInNumber);
                            }
                            await _accountingContext.InputValueRowVersionNumber.AddAsync(inputValueRowVersionNumber);
                            await _accountingContext.SaveChangesAsync();
                        }

                        if (lastedRowVersionId == 0)
                        {
                            trans.Rollback();
                            return InputErrorCode.InputValueRowVersionEmpty;
                        }

                        // Update lasted version
                        inputValueRow.LastestInputValueRowVersionId = lastedRowVersionId;
                        await _accountingContext.SaveChangesAsync();
                    }

                    trans.Commit();
                    await _activityLogService.CreateLog(EnumObjectType.InputType, inputValueBill.InputValueBillId, $"Thêm chứng từ cho loại chứng từ {inputType.Title}", data.JsonSerialize());
                    return inputValueBill.InputValueBillId;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    _logger.LogError(ex, "Create");
                    return GeneralCode.InternalError;
                }
            }
        }

        private Enum CheckRequired(InputValueBillInputModel data, IEnumerable<InputAreaField> requiredFields)
        {
            if (requiredFields.Count() > 0)
            {
                foreach (var field in requiredFields)
                {
                    string fieldName = string.Format(StringFormats.INPUT_TYPE_FIELDNAME_FORMAT, field.FieldIndex);

                    var valueRows = data.InputValueRows.Where(r => r.InputAreaId == field.InputAreaId).ToList();
                    if (valueRows.Count == 0)
                    {
                        return InputErrorCode.RequiredFieldIsEmpty;
                    }

                    foreach (var valueRow in valueRows)
                    {
                        if (valueRow.InputValueRowVersions.Count == 0 || valueRow.InputValueRowVersions.Any(rv => string.IsNullOrEmpty((string)rv.GetType().GetProperty(fieldName).GetValue(rv))))
                        {
                            return InputErrorCode.RequiredFieldIsEmpty;
                        }
                    }
                }
            }
            return GeneralCode.Success;
        }

        private Enum CheckUnique(InputValueBillInputModel data, IEnumerable<InputAreaField> uniqueFields, int? categoryBillId = null)
        {
            // Check unique
            if (uniqueFields.Count() > 0)
            {
                foreach (var field in uniqueFields)
                {
                    string fieldName = string.Format(StringFormats.INPUT_TYPE_FIELDNAME_FORMAT, field.FieldIndex);
                    var valueRows = data.InputValueRows.Where(rv => rv.InputAreaId == field.InputAreaId).ToList();
                    if (valueRows.Count > 0)
                    {
                        foreach (var valueRow in valueRows)
                        {
                            if (valueRow.InputValueRowVersions.Count > 0)
                            {
                                foreach (var valueRowVersion in valueRow.InputValueRowVersions)
                                {
                                    string value = (string)valueRowVersion.GetType().GetProperty(fieldName).GetValue(valueRowVersion);

                                    bool isExisted = _accountingContext.InputValueRow
                                        .Where(r => r.InputAreaId == field.InputAreaId)
                                        .Include(r => r.InputValueRowVersions.Where(rv => rv.InputValueRowVersionId == r.LastestInputValueRowVersionId))
                                        .Any(r => r.InputValueRowVersions.Any(rv => (string)rv.GetType().GetProperty(fieldName).GetValue(rv) == value));

                                    if (isExisted)
                                    {
                                        return InputErrorCode.UniqueValueAlreadyExisted;
                                    }
                                }
                            }
                        }
                    }

                }
            }
            return GeneralCode.Success;
        }
    }
}
