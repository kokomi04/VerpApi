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
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.EF.PurchaseOrderDB;

namespace VErp.Services.PurchaseOrder.Service.Voucher.Implement
{
    public class VoucherActionService : ActionButtonHelperServiceAbstract, IVoucherActionService
    {
        private readonly IActivityLogService _activityLogService;
        private readonly IMapper _mapper;
        private readonly PurchaseOrderDBContext _purchaseOrderDBContext;
        private readonly IActionButtonHelperService _actionButtonHelperService;

        public VoucherActionService(PurchaseOrderDBContext purchaseOrderDBContext
            , IActivityLogService activityLogService
            , IMapper mapper
            , IRoleHelperService roleHelperService
            , IActionButtonHelperService actionButtonHelperService
            ) : base(actionButtonHelperService, EnumObjectType.VoucherType)
        {
            _purchaseOrderDBContext = purchaseOrderDBContext;
            _activityLogService = activityLogService;
            _mapper = mapper;
            _actionButtonHelperService = actionButtonHelperService;
        }

        protected override async Task<string> GetObjectTitle(int objectId)
        {
            var info = await _purchaseOrderDBContext.VoucherType.FirstOrDefaultAsync(v => v.VoucherTypeId == objectId);
            if (info == null) throw new BadRequestException(InputErrorCode.InputTypeNotFound);
            return info.Title;
        }

        public override async Task<List<NonCamelCaseDictionary>> ExecActionButton(int objectId, int inputActionId, long billId, BillInfoModel data)
        {
            var voucherTypeId = objectId;
            var voucherActionId = inputActionId;
            var voucherBillId = billId;

            List<NonCamelCaseDictionary> result = null;
            var action = await _actionButtonHelperService.ActionButtonInfo(inputActionId, EnumObjectType.VoucherType, voucherTypeId);
            if (action == null) throw new BadRequestException(VoucherErrorCode.VoucherActionNotFound);

            if (!_purchaseOrderDBContext.VoucherBill.Any(b => b.VoucherTypeId == action.ObjectId && b.FId == voucherBillId))
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

            return result;
        }

        public override Task<List<NonCamelCaseDictionary>> ExecActionButton(int objectId, int categoryActionId, NonCamelCaseDictionary data)
        {
            throw new NotImplementedException();
        }
    }
}

