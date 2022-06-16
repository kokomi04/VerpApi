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
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;

namespace VErp.Services.Master.Service.Category.Implement
{
    public class CategoryActionExecService : ActionButtonExecHelperServiceAbstract, ICategoryActionExecService
    {
        private readonly MasterDBContext _masterDBContext;

        public CategoryActionExecService(MasterDBContext masterDBContext
            , IActionButtonExecHelperService actionButtonExecHelperService
            ) : base(actionButtonExecHelperService, EnumObjectType.Category)
        {
            _masterDBContext = masterDBContext;
        }

        public override async Task<List<NonCamelCaseDictionary>> ExecActionButton(int actionButtonId, int billTypeObjectId, long billId, BillInfoModel data)
        {
            var categoryId = billTypeObjectId;

            List<NonCamelCaseDictionary> result = null;
            var action = await ActionButtonInfo(actionButtonId, billTypeObjectId);
            if (action == null) throw new BadRequestException(InputErrorCode.InputActionNotFound);

            if (!_masterDBContext.Category.Any(b => b.CategoryId == categoryId))
                throw new BadRequestException(CategoryErrorCode.CategoryNotFound);

            var fields = _masterDBContext.CategoryField
                .Where(f => f.CategoryId == categoryId && f.FormTypeId != (int)EnumFormType.ViewOnly)
                .ToDictionary(f => f.CategoryFieldName, f => (EnumDataType)f.DataTypeId);
            // Validate permission

            var resultParam = new SqlParameter("@ResStatus", 0) { DbType = DbType.Int32, Direction = ParameterDirection.Output };
            var messageParam = new SqlParameter("@Message", DBNull.Value) { DbType = DbType.String, Direction = ParameterDirection.Output, Size = 128 };
            if (!string.IsNullOrEmpty(action.SqlAction))
            {
                var parammeters = new List<SqlParameter>() {
                    resultParam,
                    messageParam,
                    new SqlParameter("@CategoryId", categoryId),
                };

                foreach (var field in fields)
                {
                    object celValue = null;
                    data?.Info?.TryGetValue(field.Key, out celValue);
                    parammeters.Add(new SqlParameter($"@{field.Key}", (field.Value).GetSqlValue(celValue)));
                }

                var resultData = await _masterDBContext.QueryDataTable(action.SqlAction, parammeters);
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

