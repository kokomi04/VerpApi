using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.AccountancyDB;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Accountancy.Model.Input;
using VErp.Services.Accountancy.Model.Data;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ServiceCore.Facade;
using Verp.Resources.Accountancy.InputData;

namespace VErp.Services.Accountancy.Service.Input.Implement
{
    public class InputActionExecService : ActionButtonExecHelperServiceAbstract, IInputActionExecService
    {
        private readonly AccountancyDBContext _accountancyDBContext;
        private readonly ICurrentContextService _currentContextService;
        private readonly ObjectActivityLogFacade _inputDataActivityLog;

        public InputActionExecService(AccountancyDBContext accountancyDBContext
            , IActivityLogService activityLogService
            , IActionButtonExecHelperService actionButtonExecHelperService
            , ICurrentContextService currentContextService
            ) : base(actionButtonExecHelperService, EnumObjectType.InputType)
        {
            _accountancyDBContext = accountancyDBContext;
            _currentContextService = currentContextService;
            _inputDataActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.InputBill);
        }

        public override async Task<List<NonCamelCaseDictionary>> ExecActionButton(int actionButtonId, int billTypeObjectId, long billId, BillInfoModel data)
        {
            var inputTypeId = billTypeObjectId;
            var inputBillId = billId;

            List<NonCamelCaseDictionary> result = null;
            var action = await ActionButtonInfo(billTypeObjectId, actionButtonId);
            if (action == null) throw new BadRequestException(InputErrorCode.InputActionNotFound);

            if (!_accountancyDBContext.InputBill.Any(b => b.InputTypeId == inputTypeId && b.FId == inputBillId))
                throw new BadRequestException(InputErrorCode.InputValueBillNotFound);

            var fields = _accountancyDBContext.InputField
                .Where(f => f.FormTypeId != (int)EnumFormType.ViewOnly)
                .ToDictionary(f => f.FieldName, f => (EnumDataType)f.DataTypeId);
            // Validate permission

            var resultParam = new SqlParameter("@ResStatus", 0) { DbType = DbType.Int32, Direction = ParameterDirection.Output };
            var messageParam = new SqlParameter("@Message", DBNull.Value) { DbType = DbType.String, Direction = ParameterDirection.Output, Size = 128 };
            if (!string.IsNullOrEmpty(action.SqlAction))
            {
                var parammeters = new List<SqlParameter>() {
                    resultParam,
                    messageParam,
                    new SqlParameter("@InputTypeId", inputTypeId),
                    new SqlParameter("@InputBill_F_Id", inputBillId),
                    new SqlParameter("@UserId", _currentContextService.UserId)
                };

                DataTable rows = SqlDBHelper.ConvertToDataTable(data.Info, data.Rows, fields);
                parammeters.Add(new SqlParameter("@Rows", rows) { SqlDbType = SqlDbType.Structured, TypeName = "dbo.InputTableType" });

                var resultData = await _accountancyDBContext.QueryDataTable(action.SqlAction, parammeters);
                result = resultData.ConvertData();
            }
            var code = (resultParam.Value as int?).GetValueOrDefault();

            if (code != 0)
            {
                var message = messageParam.Value as string;
                throw new BadRequestException(GeneralCode.InvalidParams, message);
            }

            var billCode = data.Info.ContainsKey("so_ct") ? data.Info["so_ct"] : "";
            var logMessage = $"{action.Title} {billCode}. ";

            await _inputDataActivityLog.CreateLog(billId, logMessage, data.JsonSerialize(), (EnumActionType)action.ActionTypeId, false, null, null, null, inputTypeId);

            return result;
        }
    }
}

