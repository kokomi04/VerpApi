using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;

namespace VErp.Services.Organization.Service.HrConfig
{
    public interface IHrActionExecService : IActionButtonExecHelper
    {

    }

    public class HrActionExecService : ActionButtonExecHelperServiceAbstract, IHrActionExecService
    {
        private readonly OrganizationDBContext _organizationDBContext;

        private readonly ObjectActivityLogFacade _hrDataActivityLog;
        public HrActionExecService(
            ILogger<HrActionExecService> logger,
            IActivityLogService activityLogService,
            IActionButtonExecHelperService actionButtonExecHelperService,
            OrganizationDBContext organizationDBContext)
            : base(actionButtonExecHelperService, EnumObjectType.HrType)
        {
            _organizationDBContext = organizationDBContext;
            _hrDataActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.HrBill);
        }


        public override async Task<List<NonCamelCaseDictionary>> ExecActionButton(int actionButtonId, int billTypeObjectId, long billId, BillInfoModel data)
        {
            var hrTypeId = billTypeObjectId;
            var hrBillId = billId;

            List<NonCamelCaseDictionary> result = null;
            var action = await ActionButtonInfo(actionButtonId, billTypeObjectId);
            if (action == null) throw new BadRequestException(HrErrorCode.HrActionNotFound);

            if (!_organizationDBContext.HrBill.Any(b => b.HrTypeId == hrTypeId && b.FId == hrBillId))
                throw new BadRequestException(HrErrorCode.HrValueBillNotFound);

            var fields = _organizationDBContext.HrField
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
                    new SqlParameter("@HrTypeId", hrTypeId),
                    new SqlParameter("@HrBill_F_Id", hrBillId),
                };

                // DataTable rows = SqlDBHelper.ConvertToDataTable(data.Info, data.Rows, fields);
                // parammeters.Add(new SqlParameter("@Rows", rows) { SqlDbType = SqlDbType.Structured, TypeName = "dbo.InputTableType" });

                var resultData = await _organizationDBContext.QueryDataTableRaw(action.SqlAction, parammeters);
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

            await _hrDataActivityLog.CreateLog(billId, logMessage, data, (EnumActionType)action.ActionTypeId, false, null, null, null, hrTypeId);

            return result;
        }

    }
}
