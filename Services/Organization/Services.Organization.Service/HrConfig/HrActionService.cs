using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.OrganizationDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Service;

namespace VErp.Services.Organization.Service.HrConfig
{
    public interface IHrActionService : IActionButtonHelper
    {

    }

    public class HrActionService: ActionButtonHelperServiceAbstract, IHrActionService
    {
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly IMapper _mapper;
        private readonly OrganizationDBContext _organizationDBContext;

        private readonly IActionButtonHelperService _actionButtonHelperService;

        public HrActionService(
            ILogger<HrActionService> logger,
            IActivityLogService activityLogService,
            IMapper mapper,
            IActionButtonHelperService actionButtonHelperService)
            : base(actionButtonHelperService, EnumObjectType.HrType)
        {
            _logger = logger;
            _activityLogService = activityLogService;
            _mapper = mapper;
            _actionButtonHelperService = actionButtonHelperService;
        }

        protected override async Task<string> GetObjectTitle(int objectId)
        {
            var info = await _organizationDBContext.HrType.FirstOrDefaultAsync(v => v.HrTypeId == objectId);
            if (info == null) throw new BadRequestException(HrErrorCode.HrTypeNotFound);
            return info.Title;
        }

        public override async Task<List<NonCamelCaseDictionary>> ExecActionButton(int objectId, int hrActionId, long billId, BillInfoModel data)
        {
            var hrTypeId = objectId;
            var hrBillId = billId;

            List<NonCamelCaseDictionary> result = null;
            var action = await _actionButtonHelperService.ActionButtonInfo(hrActionId, EnumObjectType.HrType, hrTypeId);
            if (action == null) throw new BadRequestException(HrErrorCode.HrActionNotFound);

            if (!_organizationDBContext.HrBill.Any(b => b.HrTypeId == action.ObjectId && b.FId == hrBillId))
                throw new BadRequestException(HrErrorCode.HrValueBillNotFound);

            var fields = _organizationDBContext.HrField
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
                    new SqlParameter("@HrTypeId", action.ObjectId),
                    new SqlParameter("@HrBill_F_Id", hrBillId)
                };

                // DataTable rows = SqlDBHelper.ConvertToDataTable(data.Info, data.Rows, fields);
                // parammeters.Add(new SqlParameter("@Rows", rows) { SqlDbType = SqlDbType.Structured, TypeName = "dbo.InputTableType" });

                var resultData = await _organizationDBContext.QueryDataTable(action.SqlAction, parammeters);
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
