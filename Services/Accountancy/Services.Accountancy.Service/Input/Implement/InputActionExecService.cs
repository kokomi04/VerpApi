using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Verp.Resources.Accountancy.InputData;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface.DynamicBill;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.AccountancyDB;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;

namespace VErp.Services.Accountancy.Service.Input.Implement
{
    public class InputPrivateActionExecService : InputActionExecServiceBase, IInputPrivateActionExecService
    {
        public InputPrivateActionExecService(
            AccountancyDBPrivateContext accountancyDBContext, 
            IActivityLogService activityLogService, 
            IActionButtonExecHelperService actionButtonExecHelperService, 
            ICurrentContextService currentContextService) : 
            base(accountancyDBContext, activityLogService, actionButtonExecHelperService, currentContextService, EnumObjectType.InputType, EnumObjectType.InputBill)
        {
        }
    }

    public class InputPublicActionExecService : InputActionExecServiceBase, IInputPublicActionExecService
    {
        public InputPublicActionExecService(
            AccountancyDBPublicContext accountancyDBContext,
            IActivityLogService activityLogService,
            IActionButtonExecHelperService actionButtonExecHelperService,
            ICurrentContextService currentContextService) :
            base(accountancyDBContext, activityLogService, actionButtonExecHelperService, currentContextService, EnumObjectType.InputTypePublic, EnumObjectType.InputBillPublic)
        {
        }
    }

    public abstract class InputActionExecServiceBase : ActionButtonExecHelperServiceAbstract, IActionButtonExecHelper
    {

        private readonly AccountancyDBContext _accountancyDBContext;
        private readonly ICurrentContextService _currentContextService;
        private readonly ObjectActivityLogFacade _inputDataActivityLog;

        public InputActionExecServiceBase(AccountancyDBContext accountancyDBContext
            , IActivityLogService activityLogService
            , IActionButtonExecHelperService actionButtonExecHelperService
            , ICurrentContextService currentContextService
            , EnumObjectType inputTypeObjectTypeId
            , EnumObjectType InputBillObjectTypeId
            ) : base(actionButtonExecHelperService, inputTypeObjectTypeId)
        {
            _accountancyDBContext = accountancyDBContext;
            _currentContextService = currentContextService;
            _inputDataActivityLog = activityLogService.CreateObjectTypeActivityLog(inputTypeObjectTypeId);
        }

        public override async Task<List<NonCamelCaseDictionary>> ExecActionButton(int actionButtonId, int billTypeObjectId, long billId, BillInfoModel data)
        {
            var inputTypeId = billTypeObjectId;
            var inputBillId = billId;

            List<NonCamelCaseDictionary> result = null;
            var action = await ActionButtonInfo(actionButtonId, billTypeObjectId);
            if (action == null) throw new BadRequestException(InputErrorCode.InputActionNotFound);

            if (!_accountancyDBContext.InputBill.Any(b => b.InputTypeId == inputTypeId && b.FId == inputBillId))
                throw new BadRequestException(InputErrorCode.InputValueBillNotFound);

            var fields = _accountancyDBContext.InputField
                .Where(f => f.FormTypeId != (int)EnumFormType.ViewOnly && f.FormTypeId != (int)EnumFormType.SqlSelect)
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

                var resultData = await _accountancyDBContext.QueryDataTableRaw(action.SqlAction, parammeters);
                result = resultData.ConvertData();
            }
            var code = (resultParam.Value as int?).GetValueOrDefault();

            if (code != 0)
            {
                var message = messageParam.Value as string;
                throw new BadRequestException(GeneralCode.InvalidParams, message);
            }

            var billCode = data.Info.ContainsKey("so_ct") ? data.Info["so_ct"] : "";

            await _inputDataActivityLog.LogBuilder(() => InputActionExecActivityLogMessage.ExecActionButton)
                .MessageResourceFormatDatas(action.Title, billCode)
                .ObjectId(billId)
                .JsonData(data)
                .CreateLog();
            return result;
        }
    }
}

