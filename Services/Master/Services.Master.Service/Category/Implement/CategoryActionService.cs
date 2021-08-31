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
using VErp.Infrastructure.EF.MasterDB;

namespace VErp.Services.Master.Service.Category.Implement
{
    public class CateogryActionService : ActionButtonHelperServiceAbstract, ICategoryActionService
    {
        private readonly IActivityLogService _activityLogService;
        private readonly IMapper _mapper;
        private readonly MasterDBContext _masterDBContext;
        private readonly IActionButtonHelperService _actionButtonHelperService;

        public CateogryActionService(MasterDBContext masterDBContext
            , IActivityLogService activityLogService
            , IMapper mapper
            , IRoleHelperService roleHelperService
            , IActionButtonHelperService actionButtonHelperService
            ) : base(actionButtonHelperService, EnumObjectType.Category)
        {
            _masterDBContext = masterDBContext;
            _activityLogService = activityLogService;
            _mapper = mapper;
            _actionButtonHelperService = actionButtonHelperService;
        }

        protected override async Task<string> GetObjectTitle(int objectId)
        {
            var info = await _masterDBContext.Category.FirstOrDefaultAsync(v => v.CategoryId == objectId);
            if (info == null) throw new BadRequestException(CategoryErrorCode.CategoryNotFound);
            return info.Title;
        }

        public override async Task<List<NonCamelCaseDictionary>> ExecActionButton(int objectId, int categoryActionId, NonCamelCaseDictionary data)
        {
            var categoryId = objectId;

            List<NonCamelCaseDictionary> result = null;
            var action = await _actionButtonHelperService.ActionButtonInfo(categoryActionId, EnumObjectType.Category, categoryId);
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
                    data.TryGetValue(field.Key, out var celValue);
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

        public override Task<List<NonCamelCaseDictionary>> ExecActionButton(int objectId, int inputActionId, long billId, BillInfoModel data)
        {
            throw new NotImplementedException();
        }
    }
}

