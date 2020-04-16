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

        public async Task<ServiceResult<InputValueBillOutputModel>> GetInputValueBill(int inputTypeId, int inputValueBillId)
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

        public async Task<ServiceResult<int>> AddInputValueBill(int updatedUserId, int inputTypeId, InputValueBillInputModel data)
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
          
            // Check field required
           

            // Check unique
        

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


                    // Insert row version

                    // Insert row version number

                    trans.Commit();
                    //await _activityLogService.CreateLog(EnumObjectType.InputType, categoryRowId, $"Thêm chứng từ cho loại chứng từ {inputType.Title}", data.JsonSerialize());
                    return 1;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    _logger.LogError(ex, "Create");
                    return GeneralCode.InternalError;
                }
            }
        }

    }
}
