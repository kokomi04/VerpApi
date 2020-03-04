using VErp.Services.PurchaseOrder.Model.PurchasingRequest;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Infrastructure.EF.PurchaseOrderDB;
using Microsoft.Extensions.Options;
using VErp.Commons.Enums.StandardEnum;
using MasterDBContext = VErp.Infrastructure.EF.MasterDB.MasterDBContext;
using VErp.Commons.Library;
using VErp.Commons.Enums.MasterEnum;
using System.Linq;

namespace VErp.Services.PurchaseOrder.Service.PurchasingRequest.Implement
{
    class PurchasingRequestService : IPurchasingRequestService
    {
        private readonly PurchaseOrderDBContext _purchaseOrderDBContext;
        private readonly MasterDBContext _masterDBContext;
        private readonly StockDBContext _stockDbContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly IAsyncRunnerService _asyncRunner;

        public PurchasingRequestService(MasterDBContext masterDBContext, StockDBContext stockContext, PurchaseOrderDBContext purchaseOrderDBContext
           , IOptions<AppSetting> appSetting
           , ILogger<PurchasingRequestService> logger
           , IActivityLogService activityLogService
           , IAsyncRunnerService asyncRunner
           )
        {
            _purchaseOrderDBContext = purchaseOrderDBContext;
            _masterDBContext = masterDBContext;
            _stockDbContext = stockContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
            _asyncRunner = asyncRunner;
        }

        public async Task<ServiceResult<PurchasingRequestOutputModel>> Get(long purchasingRequestId)
        {
            try
            {
                var purchasingRequestObj = await _purchaseOrderDBContext.PurchasingRequest.AsNoTracking().FirstOrDefaultAsync(q => q.PurchasingRequestId == purchasingRequestId);
                if (purchasingRequestObj == null)
                {
                    return GeneralCode.InternalError;
                }
                var purchasingRequestDetailList = await _purchaseOrderDBContext.PurchasingRequestDetail.AsNoTracking().Where(q => q.PurchasingRequestId == purchasingRequestObj.PurchasingRequestId).ToListAsync();

                var result = new PurchasingRequestOutputModel
                {
                    PurchasingRequestId = purchasingRequestObj.PurchasingRequestId,
                    PurchasingRequestCode = purchasingRequestObj.PurchasingRequestCode,
                    OrderCode = purchasingRequestObj.OrderCode,
                    Date = purchasingRequestObj.Date.GetUnix(),
                    Content = purchasingRequestObj.Content,
                    IsApproved = purchasingRequestObj.IsApproved,
                    CreatedByUserId = purchasingRequestObj.CreatedByUserId,
                    UpdatedByUserId = purchasingRequestObj.UpdatedByUserId,
                    CreatedDatetime = purchasingRequestObj.CreatedDatetime != null ? ((DateTime)purchasingRequestObj.CreatedDatetime).GetUnix() : 0,
                    UpdatedDatetime = purchasingRequestObj.UpdatedDatetime != null ? ((DateTime)purchasingRequestObj.UpdatedDatetime).GetUnix() : 0,
                    DetailList = null
                };

                return GeneralCode.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ApprovePurchasingRequest");
                return GeneralCode.InternalError;
            }
        }

        public async Task<PageData<PurchasingRequestOutputModel>> GetList(string keyword, long beginTime = 0, long endTime = 0, int page = 1, int size = 10)
        {
            throw new NotImplementedException();
        }

        public async Task<ServiceResult<long>> AddPurchasingRequest(int currentUserId, PurchasingRequestInputModel model)
        {
            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var purchasingRequestObj = new VErp.Infrastructure.EF.PurchaseOrderDB.PurchasingRequest()
                    {
                        PurchasingRequestCode = model.PurchasingRequestCode,
                        OrderCode = model.OrderCode,
                        Date = model.Date.UnixToDateTime(),
                        Content = model.Content,
                        CreatedByUserId = currentUserId,
                        UpdatedByUserId = currentUserId,
                        IsApproved = false,
                        CreatedDatetime = DateTime.UtcNow,
                        UpdatedDatetime = DateTime.UtcNow,
                        IsDeleted = false
                    };

                    await _purchaseOrderDBContext.AddAsync(purchasingRequestObj);
                    await _purchaseOrderDBContext.SaveChangesAsync();

                    var purchasingRequestDetailList = new List<PurchasingRequestDetail>(model.DetailList.Count);

                    foreach (var details in model.DetailList)
                    {
                        purchasingRequestDetailList.Add(new PurchasingRequestDetail
                        {
                            PurchasingRequestId = purchasingRequestObj.PurchasingRequestId,
                            ProductId = details.ProductId,
                            PrimaryUnitId = details.PrimaryUnitId,
                            PrimaryQuantity = details.PrimaryQuantity,
                            IsDeleted = false,
                            CreatedDatetime = DateTime.UtcNow,
                            UpdatedDatetime = DateTime.UtcNow,
                        });
                    }
                    await _purchaseOrderDBContext.PurchasingRequestDetail.AddRangeAsync(purchasingRequestDetailList);
                    await _purchaseOrderDBContext.SaveChangesAsync();

                    trans.Commit();

                    await _activityLogService.CreateLog(EnumObjectType.PurchasingRequest, purchasingRequestObj.PurchasingRequestId, $"Thêm mới phiếu đề nghị mua  {purchasingRequestObj.PurchasingRequestCode}", model.JsonSerialize());

                    return purchasingRequestObj.PurchasingRequestId;
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    _logger.LogError(ex, "AddStock");
                    return GeneralCode.InternalError;
                }
            }
        }            

        public async Task<Enum> UpdatePurchasingRequest(long purchasingRequestId, int currentUserId, PurchasingRequestInputModel model)
        {
            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var purchasingRequestObj = await _purchaseOrderDBContext.PurchasingRequest.FirstOrDefaultAsync(q => q.PurchasingRequestId == purchasingRequestId);
                    if (purchasingRequestObj == null)
                    {
                        return GeneralCode.InternalError;
                    }
                    purchasingRequestObj.PurchasingRequestCode = model.PurchasingRequestCode;
                    purchasingRequestObj.OrderCode = model.OrderCode;
                    purchasingRequestObj.Date = model.Date.UnixToDateTime();
                    purchasingRequestObj.Content = model.Content;
                    purchasingRequestObj.UpdatedByUserId = currentUserId;
                    purchasingRequestObj.UpdatedDatetime = DateTime.UtcNow;

                    await _purchaseOrderDBContext.SaveChangesAsync();

                    var oldPurchasingRequestDetailList = await _purchaseOrderDBContext.PurchasingRequestDetail.Where(q => q.PurchasingRequestId == purchasingRequestObj.PurchasingRequestId).ToListAsync();

                    foreach (var details in oldPurchasingRequestDetailList)
                    {
                        details.IsDeleted = true;
                        details.UpdatedDatetime = DateTime.UtcNow;
                    }
                    await _purchaseOrderDBContext.SaveChangesAsync();

                    var newPurchasingRequestDetailList = new List<PurchasingRequestDetail>(model.DetailList.Count);

                    foreach (var details in model.DetailList)
                    {
                        newPurchasingRequestDetailList.Add(new PurchasingRequestDetail
                        {
                            PurchasingRequestId = purchasingRequestObj.PurchasingRequestId,
                            ProductId = details.ProductId,
                            PrimaryUnitId = details.PrimaryUnitId,
                            PrimaryQuantity = details.PrimaryQuantity,
                            IsDeleted = false,
                            CreatedDatetime = DateTime.UtcNow,
                            UpdatedDatetime = DateTime.UtcNow,
                        });
                    }
                    await _purchaseOrderDBContext.PurchasingRequestDetail.AddRangeAsync(newPurchasingRequestDetailList);
                    await _purchaseOrderDBContext.SaveChangesAsync();

                    trans.Commit();

                    await _activityLogService.CreateLog(EnumObjectType.PurchasingRequest, purchasingRequestObj.PurchasingRequestId, $"Cập nhật phiếu đề nghị mua  {purchasingRequestObj.PurchasingRequestCode}", purchasingRequestObj.JsonSerialize());

                    return GeneralCode.Success;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "UpdatePurchasingRequest");
                    return GeneralCode.InternalError;
                }
            }
        }

        public async Task<Enum> DeletePurchasingRequest(long purchasingRequestId, int currentUserId)
        {
            try
            {
                var purchasingRequestObj = await _purchaseOrderDBContext.PurchasingRequest.FirstOrDefaultAsync(q => q.PurchasingRequestId == purchasingRequestId);
                if (purchasingRequestObj == null)
                {
                    return GeneralCode.InternalError;
                }
                if (purchasingRequestObj.IsApproved)
                {
                    return GeneralCode.InternalError;
                }
                purchasingRequestObj.IsDeleted = true;
                purchasingRequestObj.UpdatedByUserId = currentUserId;
                purchasingRequestObj.UpdatedDatetime = DateTime.UtcNow;

                await _purchaseOrderDBContext.SaveChangesAsync();

                await _activityLogService.CreateLog(EnumObjectType.PurchasingRequest, purchasingRequestObj.PurchasingRequestId, $"Xóa phiếu đề nghị mua  {purchasingRequestObj.PurchasingRequestCode}", purchasingRequestObj.JsonSerialize());

                return GeneralCode.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ApprovePurchasingRequest");
                return GeneralCode.InternalError;
            }
        }

        public async Task<Enum> ApprovePurchasingRequest(long purchasingRequestId, int currentUserId)
        {
            try
            {
                var purchasingRequestObj = await _purchaseOrderDBContext.PurchasingRequest.FirstOrDefaultAsync(q => q.PurchasingRequestId == purchasingRequestId);
                if (purchasingRequestObj == null)
                {
                    return GeneralCode.InternalError;
                }
                purchasingRequestObj.IsApproved = true;
                purchasingRequestObj.UpdatedByUserId = currentUserId;
                purchasingRequestObj.UpdatedDatetime = DateTime.UtcNow;

                await _purchaseOrderDBContext.SaveChangesAsync();

                await _activityLogService.CreateLog(EnumObjectType.PurchasingRequest, purchasingRequestObj.PurchasingRequestId, $"Duyệt phiếu đề nghị mua  {purchasingRequestObj.PurchasingRequestCode}", purchasingRequestObj.JsonSerialize());

                return GeneralCode.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ApprovePurchasingRequest");
                return GeneralCode.InternalError;
            }
        }

        public async Task<Enum> RejectPurchasingRequest(long purchasingRequestId, int currentUserId)
        {
            try
            {
                var purchasingRequestObj = await _purchaseOrderDBContext.PurchasingRequest.FirstOrDefaultAsync(q => q.PurchasingRequestId == purchasingRequestId);
                if (purchasingRequestObj == null)
                {
                    return GeneralCode.InternalError;
                }
                purchasingRequestObj.IsApproved = false;
                purchasingRequestObj.UpdatedByUserId = currentUserId;
                purchasingRequestObj.UpdatedDatetime = DateTime.UtcNow;

                await _purchaseOrderDBContext.SaveChangesAsync();

                await _activityLogService.CreateLog(EnumObjectType.PurchasingRequest, purchasingRequestObj.PurchasingRequestId, $"Từ chối phiếu đề nghị mua  {purchasingRequestObj.PurchasingRequestCode}", purchasingRequestObj.JsonSerialize());

                return GeneralCode.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RejectPurchasingRequest");
                return GeneralCode.InternalError;
            }
        }
    }
}
