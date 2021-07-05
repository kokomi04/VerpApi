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

namespace VErp.Services.Accountancy.Service.Input.Implement
{
    public class InputActionService : ActionButtonHelperServiceAbstract, IInputActionService
    {
        private readonly IActivityLogService _activityLogService;
        private readonly IMapper _mapper;
        private readonly AccountancyDBContext _accountancyDBContext;
        private readonly IActionButtonHelperService _actionButtonHelperService;

        public InputActionService(AccountancyDBContext accountancyDBContext
            , IActivityLogService activityLogService
            , IMapper mapper
            , IRoleHelperService roleHelperService
            , IActionButtonHelperService actionButtonHelperService
            ) : base(actionButtonHelperService, EnumObjectType.InputType)
        {
            _accountancyDBContext = accountancyDBContext;
            _activityLogService = activityLogService;
            _mapper = mapper;
            _actionButtonHelperService = actionButtonHelperService;
        }

        protected override async Task<string> GetObjectTitle(int objectId)
        {
            var info = await _accountancyDBContext.InputType.FirstOrDefaultAsync(v => v.InputTypeId == objectId);
            if (info == null) throw new BadRequestException(InputErrorCode.InputTypeNotFound);
            return info.Title;
        }

        public override async Task<List<NonCamelCaseDictionary>> ExecActionButton(int objectId, int inputActionId, long billId, BillInfoModel data)
        {
            var inputTypeId = objectId;
            var inputBillId = billId;

            List<NonCamelCaseDictionary> result = null;
            var action = await _actionButtonHelperService.ActionButtonInfo(inputActionId, EnumObjectType.InputType, inputTypeId);
            if (action == null) throw new BadRequestException(InputErrorCode.InputActionNotFound);

            if (!_accountancyDBContext.InputBill.Any(b => b.InputTypeId == action.ObjectId && b.FId == inputBillId))
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
                    new SqlParameter("@InputTypeId", action.ObjectId),
                    new SqlParameter("@InputBill_F_Id", inputBillId)
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

            return result;
        }


    }
}

