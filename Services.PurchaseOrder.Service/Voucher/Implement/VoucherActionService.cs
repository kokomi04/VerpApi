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

        public VoucherActionService(PurchaseOrderDBContext purchaseOrderDBContext
            , IOptions<AppSetting> appSetting
            , ILogger<VoucherConfigService> logger
            , IActivityLogService activityLogService
            , IMapper mapper
            )
        {
            _purchaseOrderDBContext = purchaseOrderDBContext;
            _logger = logger;
            _activityLogService = activityLogService;
            _mapper = mapper;
        }

        public async Task<IList<VoucherActionListModel>> GetVoucherActions(int voucherTypeId)
        {
            return _purchaseOrderDBContext.VoucherAction
                .Where(a => a.VoucherTypeId == voucherTypeId)
                .ProjectTo<VoucherActionListModel>(_mapper.ConfigurationProvider)
                .ToList();
        }

        public async Task<VoucherActionModel> GetVoucherAction(int voucherActionId)
        {
            var action = _purchaseOrderDBContext.VoucherAction
               .Where(a => a.VoucherActionId == voucherActionId)
               .ProjectTo<VoucherActionModel>(_mapper.ConfigurationProvider)
               .FirstOrDefault();
            if (action == null) throw new BadRequestException(GeneralCode.ItemNotFound);
            return action;
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
            var action = _purchaseOrderDBContext.VoucherAction.FirstOrDefault(a => a.VoucherTypeId == voucherActionId);
            if (action == null) throw new BadRequestException(VoucherErrorCode.VoucherActionNotFound);
            try
            {
                action.Title = data.Title;
                action.VoucherActionCode = data.VoucherActionCode;
                action.JsAction = data.JsAction;
                action.SqlAction = data.SqlAction;

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
           var action = _purchaseOrderDBContext.VoucherAction.FirstOrDefault(a => a.VoucherTypeId == voucherActionId);
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
    }
}

