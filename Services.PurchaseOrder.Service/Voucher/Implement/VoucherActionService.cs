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
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Verp.Cache.RedisCache;
using VErp.Commons.Constants;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.PurchaseOrderDB;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.PurchaseOrder.Model.Voucher;
using VErp.Commons.GlobalObject.InternalDataInterface;

namespace VErp.Services.PurchaseOrder.Service.Voucher.Implement
{
    public class VoucherActionService : IVoucherActionService
    {
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly IMapper _mapper;
        private readonly PurchaseOrderDBContext _purchaseOrderDBContext;
        private readonly IRoleHelperService _roleHelperService;
        public VoucherActionService(PurchaseOrderDBContext purchaseOrderDBContext
            , IOptions<AppSetting> appSetting
            , ILogger<VoucherActionService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            , IRoleHelperService roleHelperService
            )
        {
            _purchaseOrderDBContext = purchaseOrderDBContext;
            _logger = logger;
            _activityLogService = activityLogService;
            _mapper = mapper;
            _roleHelperService = roleHelperService;
        }

        public async Task<IList<VoucherActionModel>> GetVoucherActionConfigs(int voucherTypeId)
        {
            return await _purchaseOrderDBContext.VoucherAction
                .Where(a => a.VoucherTypeId == voucherTypeId)
                .ProjectTo<VoucherActionModel>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<IList<VoucherActionUseModel>> GetVoucherActions(int voucherTypeId)
        {
            return await _purchaseOrderDBContext.VoucherAction
                .Where(a => a.VoucherTypeId == voucherTypeId)
                .ProjectTo<VoucherActionUseModel>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<VoucherActionModel> AddVoucherAction(VoucherActionModel data)
        {
            if (!_purchaseOrderDBContext.VoucherType.Any(v => v.VoucherTypeId == data.VoucherTypeId)) throw new BadRequestException(VoucherErrorCode.VoucherTypeNotFound);
            if (_purchaseOrderDBContext.VoucherAction.Any(v => v.VoucherActionCode == data.VoucherActionCode)) throw new BadRequestException(VoucherErrorCode.VoucherActionCodeAlreadyExisted);
            var action = _mapper.Map<VoucherAction>(data);
            try
            {
                await _purchaseOrderDBContext.VoucherAction.AddAsync(action);
                await _purchaseOrderDBContext.SaveChangesAsync();

                await _activityLogService.CreateLog(EnumObjectType.VoucherAction, action.VoucherActionId, $"Thêm chức năng {action.Title}", data.JsonSerialize());

                await _roleHelperService.GrantActionPermissionForAllRoles(EnumModule.SalesBill, EnumObjectType.VoucherType, data.VoucherTypeId, action.VoucherActionId);

                return _mapper.Map<VoucherActionModel>(action);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Create");
                throw;
            }
        }

        public async Task<VoucherActionModel> UpdateVoucherAction(int voucherActionId, VoucherActionModel data)
        {
            if (!_purchaseOrderDBContext.VoucherType.Any(v => v.VoucherTypeId == data.VoucherTypeId)) throw new BadRequestException(VoucherErrorCode.VoucherTypeNotFound);
            if (_purchaseOrderDBContext.VoucherAction.Any(v => v.VoucherActionId != voucherActionId && v.VoucherActionCode == data.VoucherActionCode)) throw new BadRequestException(VoucherErrorCode.VoucherActionCodeAlreadyExisted);
            var action = _purchaseOrderDBContext.VoucherAction.FirstOrDefault(a => a.VoucherActionId == voucherActionId);
            if (action == null) throw new BadRequestException(VoucherErrorCode.VoucherActionNotFound);
            try
            {
                _mapper.Map(data, action);
                await _purchaseOrderDBContext.SaveChangesAsync();

                await _activityLogService.CreateLog(EnumObjectType.VoucherAction, action.VoucherActionId, $"Cập nhật chức năng {action.Title}", data.JsonSerialize());
                return _mapper.Map<VoucherActionModel>(action);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Update");
                throw;
            }
        }

        public async Task<bool> DeleteVoucherAction(int voucherActionId)
        {
            var action = _purchaseOrderDBContext.VoucherAction.FirstOrDefault(a => a.VoucherActionId == voucherActionId);
            if (action == null) throw new BadRequestException(VoucherErrorCode.VoucherActionNotFound);
            try
            {
                action.IsDeleted = true;
                await _purchaseOrderDBContext.SaveChangesAsync();
                await _activityLogService.CreateLog(EnumObjectType.VoucherAction, action.VoucherActionId, $"Xóa chức năng {action.Title}", action.JsonSerialize());
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete");
                throw;
            }
        }

        public async Task<List<NonCamelCaseDictionary>> ExecVoucherAction(int voucherTypeId, int voucherActionId, long voucherBillId, VoucherBillInfoModel data)
        {
            List<NonCamelCaseDictionary> result = null;
            var action = _purchaseOrderDBContext.VoucherAction.FirstOrDefault(a => a.VoucherTypeId == voucherTypeId && a.VoucherActionId == voucherActionId);
            if (action == null) throw new BadRequestException(VoucherErrorCode.VoucherActionNotFound);
            if (!_purchaseOrderDBContext.VoucherBill.Any(b => b.VoucherTypeId == action.VoucherTypeId && b.FId == voucherBillId))
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
                    new SqlParameter("@VoucherTypeId", action.VoucherTypeId),
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
    }
}

