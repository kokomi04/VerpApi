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
using VErp.Commons.Enums.ErrorCodes;
using VErp.Commons.Enums.MasterEnum;
using MasterDBContext = VErp.Infrastructure.EF.MasterDB.MasterDBContext;
using VErp.Commons.Library;

using System.Linq;

namespace VErp.Services.PurchaseOrder.Service.PurchasingRequest.Implement
{
    public class PurchasingRequestService : IPurchasingRequestService
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

        public async Task<ServiceResult<PurchasingSuggestOutputModel>> Get(long purchasingRequestId)
        {
            try
            {
                var purchasingRequestObj = await _purchaseOrderDBContext.PurchasingRequest.AsNoTracking().FirstOrDefaultAsync(q => q.PurchasingRequestId == purchasingRequestId);
                if (purchasingRequestObj == null)
                {
                    return PurchasingRequestErrorCode.NotFound;
                }
                var purchasingRequestDetailList = await _purchaseOrderDBContext.PurchasingRequestDetail.AsNoTracking().Where(q => q.PurchasingRequestId == purchasingRequestObj.PurchasingRequestId).ToListAsync();

                var productIdsList = purchasingRequestDetailList.Select(q => q.ProductId).ToList();
                var productModelList = await _stockDbContext.Product.Where(q => productIdsList.Contains(q.ProductId)).AsNoTracking().Select(q=> new { q.ProductId,q.ProductCode,q.ProductName,q.UnitId }).ToListAsync();

                var unitIdsList = purchasingRequestDetailList.Select(q => q.PrimaryUnitId).ToList();
                var unitModelList = await _masterDBContext.Unit.Where(q => unitIdsList.Contains(q.UnitId)).AsNoTracking().ToListAsync();

                var detailsModelOutput = purchasingRequestDetailList.Select(q => new PurchasingSuggestDetailOutputModel
                {
                    PurchasingRequestDetailId = q.PurchasingRequestDetailId,
                    PurchasingRequestId = q.PurchasingRequestId,
                    ProductId = q.ProductId,
                    PrimaryUnitId = q.PrimaryUnitId,
                    PrimaryQuantity = q.PrimaryQuantity,
                    PrimaryUnitName = unitModelList.FirstOrDefault(v=>v.UnitId == q.PrimaryUnitId)?.UnitName,
                    ProductName = productModelList.FirstOrDefault(v=> v.ProductId == q.ProductId)?.ProductName,
                    ProductCode = productModelList.FirstOrDefault(v => v.ProductId == q.ProductId)?.ProductCode
                }).ToList();

                var result = new PurchasingSuggestOutputModel
                {
                    PurchasingRequestId = purchasingRequestObj.PurchasingRequestId,
                    PurchasingRequestCode = purchasingRequestObj.PurchasingRequestCode,
                    OrderCode = purchasingRequestObj.OrderCode,
                    Date = purchasingRequestObj.Date.GetUnix(),
                    Content = purchasingRequestObj.Content,
                    Status = purchasingRequestObj.Status,
                    RejectCount  = purchasingRequestObj.RejectCount,


                    CreatedByUserId = purchasingRequestObj.CreatedByUserId,
                    UpdatedByUserId = purchasingRequestObj.UpdatedByUserId,
                    CreatedDatetimeUtc = purchasingRequestObj.CreatedDatetimeUtc != null ? ((DateTime)purchasingRequestObj.CreatedDatetimeUtc).GetUnix() : 0,
                    UpdatedDatetimeUtc = purchasingRequestObj.UpdatedDatetimeUtc != null ? ((DateTime)purchasingRequestObj.UpdatedDatetimeUtc).GetUnix() : 0,

                    CensorByUserId = purchasingRequestObj.CensorByUserId,
                    CensorDatetimeUtc = purchasingRequestObj.CensorDatetimeUtc != null ? ((DateTime)purchasingRequestObj.CensorDatetimeUtc).GetUnix() : 0,

                    DetailList = detailsModelOutput
                };

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Get PurchasingRequest");
                return GeneralCode.InternalError;
            }
        }

        public async Task<PageData<PurchasingSuggestOutputModel>> GetList(string keyword, IList<int> statusList, long beginTime = 0, long endTime = 0, int page = 1, int size = 10)
        {
            var purchasingRequestQuery = from pr in _purchaseOrderDBContext.PurchasingRequest
                                         select pr;
                
            if(!string.IsNullOrEmpty(keyword))
            {
                purchasingRequestQuery = purchasingRequestQuery.Where(q => q.PurchasingRequestCode.Contains(keyword) || q.OrderCode.Contains(keyword));
            }
            if(statusList.Count > 0)
            {
                var enumStatusList = statusList.Select(q => Convert.ToInt32(q)).ToList();
                purchasingRequestQuery = purchasingRequestQuery.Where(q => enumStatusList.Contains(q.Status));
            }
            var bTime = DateTime.MinValue;
            var eTime = DateTime.MinValue;

            if (beginTime > 0)
            {
                bTime = beginTime.UnixToDateTime();
            }
            if (endTime > 0)
            {
                eTime = endTime.UnixToDateTime();
                eTime = eTime.AddDays(1);
            }
            
            if (bTime != DateTime.MinValue && eTime != DateTime.MinValue)
            {
                purchasingRequestQuery = purchasingRequestQuery.Where(q => q.Date >= bTime && q.Date < eTime);
            }
            else
            {
                if (bTime != DateTime.MinValue)
                {
                    purchasingRequestQuery = purchasingRequestQuery.Where(q => q.Date >= bTime);
                }
                if (eTime != DateTime.MinValue)
                {
                    purchasingRequestQuery = purchasingRequestQuery.Where(q => q.Date < eTime);
                }
            }

            purchasingRequestQuery = purchasingRequestQuery.OrderByDescending(q => q.Date);
            var total = purchasingRequestQuery.Count();
            var purchasingRequestDataList = purchasingRequestQuery.AsNoTracking().Skip((page - 1) * size).Take(size).ToList();
            var purchasingRequestIdsList = purchasingRequestDataList.Select(q => q.PurchasingRequestId).ToList();

            var purchasingRequestDetailList = await _purchaseOrderDBContext.PurchasingRequestDetail.AsNoTracking().Where(q => purchasingRequestIdsList.Contains(q.PurchasingRequestId)).ToListAsync();

            var productIdsList = purchasingRequestDetailList.Select(q => q.ProductId).ToList();
            var productModelList = await _stockDbContext.Product.Where(q => productIdsList.Contains(q.ProductId)).AsNoTracking().Select(q => new { q.ProductId, q.ProductCode, q.ProductName, q.UnitId }).ToListAsync();

            var unitIdsList = purchasingRequestDetailList.Select(q => q.PrimaryUnitId).ToList();
            var unitModelList = await _masterDBContext.Unit.Where(q => unitIdsList.Contains(q.UnitId)).AsNoTracking().ToListAsync();

            var detailsModelOutputList = purchasingRequestDetailList.Select(q => new PurchasingSuggestDetailOutputModel
            {
                PurchasingRequestDetailId = q.PurchasingRequestDetailId,
                PurchasingRequestId = q.PurchasingRequestId,
                ProductId = q.ProductId,
                PrimaryUnitId = q.PrimaryUnitId,
                PrimaryQuantity = q.PrimaryQuantity,
                PrimaryUnitName = unitModelList.FirstOrDefault(v => v.UnitId == q.PrimaryUnitId)?.UnitName,
                ProductName = productModelList.FirstOrDefault(v => v.ProductId == q.ProductId)?.ProductName,
                ProductCode = productModelList.FirstOrDefault(v => v.ProductId == q.ProductId)?.ProductCode
            }).ToList();


            var pagedData = new List<PurchasingSuggestOutputModel>(purchasingRequestDataList.Count);
            foreach (var purchasingRequestObj in purchasingRequestDataList)
            {
                var prItem = new PurchasingSuggestOutputModel
                {
                    PurchasingRequestId = purchasingRequestObj.PurchasingRequestId,
                    PurchasingRequestCode = purchasingRequestObj.PurchasingRequestCode,
                    OrderCode = purchasingRequestObj.OrderCode,
                    Date = purchasingRequestObj.Date.GetUnix(),
                    Content = purchasingRequestObj.Content,
                    Status = purchasingRequestObj.Status,
                    RejectCount = purchasingRequestObj.RejectCount,

                    CreatedByUserId = purchasingRequestObj.CreatedByUserId,
                    UpdatedByUserId = purchasingRequestObj.UpdatedByUserId,
                    CreatedDatetimeUtc = purchasingRequestObj.CreatedDatetimeUtc != null ? ((DateTime)purchasingRequestObj.CreatedDatetimeUtc).GetUnix() : 0,
                    UpdatedDatetimeUtc = purchasingRequestObj.UpdatedDatetimeUtc != null ? ((DateTime)purchasingRequestObj.UpdatedDatetimeUtc).GetUnix() : 0,

                    CensorByUserId = purchasingRequestObj.CensorByUserId,
                    CensorDatetimeUtc = purchasingRequestObj.CensorDatetimeUtc != null ? ((DateTime)purchasingRequestObj.CensorDatetimeUtc).GetUnix() : 0,

                    DetailList = detailsModelOutputList.Where(q=>q.PurchasingRequestId == purchasingRequestObj.PurchasingRequestId).ToList()
                };
                pagedData.Add(prItem);
            }
            return (pagedData, total);
        }

        public async Task<ServiceResult<long>> AddPurchasingRequest(int currentUserId, PurchasingSuggestInputModel model)
        {
            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var purchasingRequestObj = new VErp.Infrastructure.EF.PurchaseOrderDB.PurchasingRequest()
                    {
                        PurchasingRequestCode = model.PurchasingRequestCode,
                        OrderCode = model.OrderCode,
                        Date = model.Date > 0 ? model.Date.UnixToDateTime() : DateTime.UtcNow,
                        Content = model.Content,
                        Status = (int)EnumPurchasingRequestStatus.Editing,
                        RejectCount = 0,

                        CreatedByUserId = currentUserId,
                        UpdatedByUserId = currentUserId,

                        CreatedDatetimeUtc = DateTime.UtcNow,
                        UpdatedDatetimeUtc = DateTime.UtcNow,
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
                            CreatedDatetimeUtc = DateTime.UtcNow,
                            UpdatedDatetimeUtc = DateTime.UtcNow,
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
                    _logger.LogError(ex, "AddPurchasingRequest");
                    return GeneralCode.InternalError;
                }
            }
        }            

        public async Task<Enum> UpdatePurchasingRequest(long purchasingRequestId, int currentUserId, PurchasingSuggestInputModel model)
        {
            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var purchasingRequestObj = await _purchaseOrderDBContext.PurchasingRequest.FirstOrDefaultAsync(q => q.PurchasingRequestId == purchasingRequestId);
                    if (purchasingRequestObj == null)
                    {
                        return PurchasingRequestErrorCode.NotFound;
                    }
                    //purchasingRequestObj.PurchasingRequestCode = model.PurchasingRequestCode;
                    purchasingRequestObj.OrderCode = model.OrderCode;
                    purchasingRequestObj.Date = model.Date.UnixToDateTime();
                    purchasingRequestObj.Content = model.Content;
                    purchasingRequestObj.UpdatedByUserId = currentUserId;
                    purchasingRequestObj.UpdatedDatetimeUtc = DateTime.UtcNow;

                    await _purchaseOrderDBContext.SaveChangesAsync();

                    var oldPurchasingRequestDetailList = await _purchaseOrderDBContext.PurchasingRequestDetail.Where(q => q.PurchasingRequestId == purchasingRequestObj.PurchasingRequestId).ToListAsync();

                    foreach (var details in oldPurchasingRequestDetailList)
                    {
                        details.IsDeleted = true;
                        details.UpdatedDatetimeUtc = DateTime.UtcNow;
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
                            CreatedDatetimeUtc = DateTime.UtcNow,
                            UpdatedDatetimeUtc = DateTime.UtcNow,
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
                    return PurchasingRequestErrorCode.NotFound;
                }
                if (purchasingRequestObj.Status == (int)EnumPurchasingRequestStatus.Approved)
                {
                    return PurchasingRequestErrorCode.AlreadyApproved;
                }
                purchasingRequestObj.IsDeleted = true;
                purchasingRequestObj.UpdatedByUserId = currentUserId;
                purchasingRequestObj.UpdatedDatetimeUtc = DateTime.UtcNow;

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

        public async Task<Enum> SendToApprove(long purchasingRequestId, int currentUserId)
        {
            try
            {
                var purchasingRequestObj = await _purchaseOrderDBContext.PurchasingRequest.FirstOrDefaultAsync(q => q.PurchasingRequestId == purchasingRequestId);
                if (purchasingRequestObj == null)
                {
                    return GeneralCode.InternalError;
                }
                purchasingRequestObj.Status = (int)EnumPurchasingRequestStatus.WaitingApproved;
                purchasingRequestObj.UpdatedByUserId = currentUserId;
                purchasingRequestObj.UpdatedDatetimeUtc = DateTime.UtcNow;

                await _purchaseOrderDBContext.SaveChangesAsync();

                await _activityLogService.CreateLog(EnumObjectType.PurchasingRequest, purchasingRequestObj.PurchasingRequestId, $"Gửi duyệt phiếu đề nghị mua  {purchasingRequestObj.PurchasingRequestCode}", purchasingRequestObj.JsonSerialize());

                return GeneralCode.Success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SendToApprove");
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
                purchasingRequestObj.Status = (int)EnumPurchasingRequestStatus.Approved;
                purchasingRequestObj.UpdatedByUserId = currentUserId;
                purchasingRequestObj.UpdatedDatetimeUtc = DateTime.UtcNow;

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
                    return PurchasingRequestErrorCode.NotFound;
                }
                if(purchasingRequestObj.IsDeleted || purchasingRequestObj.Status == (int)EnumPurchasingRequestStatus.Approved)
                {
                    return PurchasingRequestErrorCode.AlreadyApproved;
                }


                purchasingRequestObj.Status = (int)EnumPurchasingRequestStatus.Rejected;
                purchasingRequestObj.RejectCount += 1;
                purchasingRequestObj.CensorByUserId = currentUserId;
                purchasingRequestObj.CensorDatetimeUtc = DateTime.UtcNow;

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
