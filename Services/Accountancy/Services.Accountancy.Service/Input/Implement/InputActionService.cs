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

namespace VErp.Services.Accountancy.Service.Input.Implement
{
    public class InputActionService : IInputActionService
    {
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly IMapper _mapper;
        private readonly AccountancyDBContext _accountancyDBContext;
        private readonly IRoleHelperService _roleHelperService;

        public InputActionService(AccountancyDBContext accountancyDBContext
            , IOptions<AppSetting> appSetting
            , ILogger<InputActionService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            , IRoleHelperService roleHelperService
            )
        {
            _accountancyDBContext = accountancyDBContext;
            _logger = logger;
            _activityLogService = activityLogService;
            _mapper = mapper;
            _roleHelperService = roleHelperService;
        }

        public async Task<IList<InputActionModel>> GetInputActionConfigs(int inputTypeId)
        {
            return await _accountancyDBContext.InputAction
                .Where(a => a.InputTypeId == inputTypeId)
                .ProjectTo<InputActionModel>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<IList<InputActionUseModel>> GetInputActions(int inputTypeId)
        {
            return await _accountancyDBContext.InputAction
                .Where(a => a.InputTypeId == inputTypeId)
                .ProjectTo<InputActionUseModel>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<InputActionModel> AddInputAction(InputActionModel data)
        {
            if (!_accountancyDBContext.InputType.Any(v => v.InputTypeId == data.InputTypeId)) throw new BadRequestException(InputErrorCode.InputTypeNotFound);
            if (_accountancyDBContext.InputAction.Any(v => v.InputActionCode == data.InputActionCode)) throw new BadRequestException(InputErrorCode.InputActionCodeAlreadyExisted);
            var action = _mapper.Map<InputAction>(data);
            try
            {
                await _accountancyDBContext.InputAction.AddAsync(action);
                await _accountancyDBContext.SaveChangesAsync();

                await _activityLogService.CreateLog(EnumObjectType.InputAction, action.InputActionId, $"Thêm chức năng {action.Title}", data.JsonSerialize());

                await _roleHelperService.GrantActionPermissionForAllRoles(EnumModule.Input, EnumObjectType.InputType, data.InputTypeId, action.InputActionId);

                return _mapper.Map<InputActionModel>(action);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create");
                throw;
            }
        }

        public async Task<InputActionModel> UpdateInputAction(int inputActionId, InputActionModel data)
        {
            if (!_accountancyDBContext.InputType.Any(v => v.InputTypeId == data.InputTypeId)) throw new BadRequestException(InputErrorCode.InputTypeNotFound);
            if (_accountancyDBContext.InputAction.Any(v => v.InputActionId != inputActionId && v.InputActionCode == data.InputActionCode)) throw new BadRequestException(InputErrorCode.InputActionCodeAlreadyExisted);
            var action = _accountancyDBContext.InputAction.FirstOrDefault(a => a.InputActionId == inputActionId);
            if (action == null) throw new BadRequestException(InputErrorCode.InputActionNotFound);
            try
            {
                _mapper.Map(data, action);
                await _accountancyDBContext.SaveChangesAsync();

                await _activityLogService.CreateLog(EnumObjectType.InputAction, action.InputActionId, $"Cập nhật chức năng {action.Title}", data.JsonSerialize());
                return _mapper.Map<InputActionModel>(action);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update");
                throw;
            }
        }

        public async Task<bool> DeleteInputAction(int inputActionId)
        {
            var action = _accountancyDBContext.InputAction.FirstOrDefault(a => a.InputActionId == inputActionId);
            if (action == null) throw new BadRequestException(InputErrorCode.InputActionNotFound);
            try
            {
                action.IsDeleted = true;
                await _accountancyDBContext.SaveChangesAsync();
                await _activityLogService.CreateLog(EnumObjectType.InputAction, action.InputActionId, $"Xóa chức năng {action.Title}", action.JsonSerialize());
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete");
                throw;
            }
        }

        public async Task<List<NonCamelCaseDictionary>> ExecInputAction(int inputTypeId, int inputActionId, long inputBillId, BillInfoModel data)
        {
            List<NonCamelCaseDictionary> result = null;
            var action = _accountancyDBContext.InputAction.FirstOrDefault(a => a.InputTypeId == inputTypeId && a.InputActionId == inputActionId);
            if (action == null) throw new BadRequestException(InputErrorCode.InputActionNotFound);
            if (!_accountancyDBContext.InputBill.Any(b => b.InputTypeId == action.InputTypeId && b.FId == inputBillId))
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
                    new SqlParameter("@InputTypeId", action.InputTypeId),
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

        private DataTable ConvertToDataTable(BillInfoModel data, IList<InputField> fields)
        {
            var dataTable = new DataTable();
            foreach (var field in fields)
            {
                dataTable.Columns.Add(field.FieldName, ((EnumDataType)field.DataTypeId).GetColumnDataType());
            }
            foreach (var row in data.Rows)
            {
                var dataRow = dataTable.NewRow();
                foreach (var field in fields)
                {
                    row.TryGetValue(field.FieldName, out var celValue);
                    if (celValue == null) data.Info.TryGetValue(field.FieldName, out celValue);
                    var value = ((EnumDataType)field.DataTypeId).GetSqlValue(celValue);
                    dataRow[field.FieldName] = value;
                }
                dataTable.Rows.Add(dataRow);
            }
            return dataTable;
        }
    }
}

