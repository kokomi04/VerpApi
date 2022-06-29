using AutoMapper;
using Microsoft.Data.SqlClient;
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
using VErp.Infrastructure.EF.PurchaseOrderDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;

namespace VErp.Services.PurchaseOrder.Service.Voucher.Implement
{
    public class VoucherActionExecService : ActionButtonExecHelperServiceAbstract, IVoucherActionExecService
    {
        private readonly PurchaseOrderDBContext _purchaseOrderDBContext;
        private readonly ICurrentContextService _currentContextService;
        private readonly ObjectActivityLogFacade _voucherDataActivityLog;

        public VoucherActionExecService(PurchaseOrderDBContext purchaseOrderDBContext
            , IActivityLogService activityLogService
            , IMapper mapper
            , IRoleHelperService roleHelperService
            , IActionButtonExecHelperService actionButtonExecHelperService
            , ICurrentContextService currentContextService
            ) : base(actionButtonExecHelperService, EnumObjectType.VoucherType)
        {
            _purchaseOrderDBContext = purchaseOrderDBContext;
            _currentContextService = currentContextService;
            _voucherDataActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.VoucherBill);
        }


        public override async Task<List<NonCamelCaseDictionary>> ExecActionButton(int actionButtonId, int billTypeObjectId, long billId, BillInfoModel data)
        {
            var voucherTypeId = billTypeObjectId;

            var voucherBillId = billId;

            List<NonCamelCaseDictionary> result = null;
            var action = await ActionButtonInfo(actionButtonId, billTypeObjectId);
            if (action == null) throw new BadRequestException(VoucherErrorCode.VoucherActionNotFound);

            if (!_purchaseOrderDBContext.VoucherBill.Any(b => b.VoucherTypeId == voucherTypeId && b.FId == voucherBillId))
                throw new BadRequestException(VoucherErrorCode.VoucherValueBillNotFound);
            var fields = _purchaseOrderDBContext.VoucherField
                .Where(f => f.FormTypeId != (int)EnumFormType.ViewOnly)
                .ToDictionary(f => f.FieldName, f => (EnumDataType)f.DataTypeId);
            // Validate permission

            var resultParam = new SqlParameter("@ResStatus", 0) { DbType = DbType.Int32, Direction = ParameterDirection.Output };
            var messageParam = new SqlParameter("@Message", DBNull.Value) { DbType = DbType.String, Direction = ParameterDirection.Output, Size = 128 };
            if (!string.IsNullOrEmpty(action.SqlAction))
            {
                DataTable rows = SqlDBHelper.ConvertToDataTable(data.Info, data.Rows, fields);
                var parammeters = new List<SqlParameter>() {
                    resultParam,
                    messageParam,
                    new SqlParameter("@VoucherTypeId", voucherTypeId),
                    new SqlParameter("@VoucherBill_F_Id", voucherBillId),
                    new SqlParameter("@UserId", _currentContextService.UserId),
                    new SqlParameter("@Rows", rows) { SqlDbType = SqlDbType.Structured, TypeName = "dbo.VoucherTableType" }
                };
                var resultData = await _purchaseOrderDBContext.QueryDataTable(action.SqlAction, parammeters);
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

            await _voucherDataActivityLog.CreateLog(billId, logMessage, data.JsonSerialize(), (EnumActionType)action.ActionTypeId, false, null, null, null, voucherTypeId);

            return result;
        }
    }
}

