
using System;
using System.Linq;
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
using VErp.Commons.Library;
using VErp.Services.Master.Service.Activity;
using VErp.Services.Master.Model.Activity;
using VErp.Commons.Enums.MasterEnum.PO;
using VErp.Commons.GlobalObject;
using VErp.Services.PurchaseOrder.Model;
using VErp.Services.Stock.Service.Products;

namespace VErp.Services.PurchaseOrder.Service.Implement
{
    public class PurchasingRequestService : IPurchasingRequestService
    {
        private readonly PurchaseOrderDBContext _purchaseOrderDBContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly IAsyncRunnerService _asyncRunner;
        private readonly ICurrentContextService _currentContext;
        private readonly IProductService _productService;

        public PurchasingRequestService(
            PurchaseOrderDBContext purchaseOrderDBContext
           , IOptions<AppSetting> appSetting
           , ILogger<PurchasingRequestService> logger
           , IActivityLogService activityLogService
           , IAsyncRunnerService asyncRunner
           , ICurrentContextService currentContext
            , IProductService productService
           )
        {
            _purchaseOrderDBContext = purchaseOrderDBContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
            _asyncRunner = asyncRunner;
            _currentContext = currentContext;
            _productService = productService;
        }


        public async Task<ServiceResult<PurchasingRequestOutput>> GetInfo(long purchasingRequestId)
        {
            var info = await _purchaseOrderDBContext.PurchasingRequest.AsNoTracking()
                .FirstOrDefaultAsync(r => r.PurchasingRequestId == purchasingRequestId);

            if (info == null) return PurchasingRequestErrorCode.RequestNotFound;

            var details = await _purchaseOrderDBContext.PurchasingRequestDetail.AsNoTracking()
                .Where(d => d.PurchasingRequestId == purchasingRequestId)
                .ToListAsync();

            return new PurchasingRequestOutput()
            {
                PurchasingRequestId = info.PurchasingRequestId,
                PurchasingRequestCode = info.PurchasingRequestCode,
                OrderCode = info.OrderCode,
                PurchasingRequestStatusId = (EnumPurchasingRequestStatus)info.PurchasingRequestStatusId,
                IsApproved = info.IsApproved,
                PoProcessStatusId = (EnumPoProcessStatus?)info.PoProcessStatusId,
                CreatedByUserId = info.CreatedByUserId,
                UpdatedByUserId = info.UpdatedByUserId,
                CensorByUserId = info.CensorByUserId,

                CensorDatetimeUtc = info.CensorDatetimeUtc?.GetUnix(),
                CreatedDatetimeUtc = info.CreatedDatetimeUtc.GetUnix(),
                UpdatedDatetimeUtc = info.UpdatedDatetimeUtc.GetUnix(),

                RejectCount = info.RejectCount,
                Content = info.Content,
                Details = details.Select(d => new PurchasingRequestOutputDetail()
                {
                    PurchasingRequestDetailId = d.PurchasingRequestDetailId,
                    ProductId = d.ProductId,
                    PrimaryQuantity = d.PrimaryQuantity,
                }).ToList()
            };

        }

        public async Task<PageData<PurchasingRequestOutputList>> GetList(string keyword, IList<int> productIds, EnumPurchasingRequestStatus? purchasingRequestStatusId, EnumPoProcessStatus? poProcessStatusId, bool? isApproved, long? fromDate, long? toDate, string sortBy, bool asc, int page, int size)
        {
            var query = _purchaseOrderDBContext.PurchasingRequest.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query
                    .Where(q => q.OrderCode.Contains(keyword)
                    || q.PurchasingRequestCode.Contains(keyword)
                    || q.Content.Contains(keyword));
            }

            if (productIds != null && productIds.Count > 0)
            {
                var purchasingRequestIds = (
                    from d in _purchaseOrderDBContext.PurchasingRequestDetail
                    where productIds.Contains(d.ProductId)
                    select d.PurchasingRequestId
                 ).Distinct();

                query = query.Where(q => purchasingRequestIds.Contains(q.PurchasingRequestId));
            }

            if (purchasingRequestStatusId.HasValue)
            {
                query = query.Where(q => q.PurchasingRequestStatusId == (int)purchasingRequestStatusId.Value);
            }

            if (poProcessStatusId.HasValue)
            {
                query = query.Where(q => q.PoProcessStatusId == (int)poProcessStatusId.Value);
            }

            if (isApproved.HasValue)
            {
                query = query.Where(q => q.IsApproved == isApproved);
            }

            if (fromDate.HasValue)
            {
                var time = fromDate.Value.UnixToDateTime();
                query = query.Where(q => q.CreatedDatetimeUtc >= time);
            }

            if (toDate.HasValue)
            {
                var time = toDate.Value.UnixToDateTime();
                query = query.Where(q => q.CreatedDatetimeUtc <= time);
            }

            var total = await query.CountAsync();
            var pagedData = await query.SortByFieldName(sortBy, asc).Skip((page - 1) * size).Take(size).ToListAsync();
            var result = new List<PurchasingRequestOutputList>();
            foreach (var info in pagedData)
            {
                result.Add(new PurchasingRequestOutputList()
                {
                    PurchasingRequestId = info.PurchasingRequestId,
                    PurchasingRequestCode = info.PurchasingRequestCode,
                    OrderCode = info.OrderCode,
                    PurchasingRequestStatusId = (EnumPurchasingRequestStatus)info.PurchasingRequestStatusId,
                    IsApproved = info.IsApproved,
                    PoProcessStatusId = (EnumPoProcessStatus?)info.PoProcessStatusId,
                    CreatedByUserId = info.CreatedByUserId,
                    UpdatedByUserId = info.UpdatedByUserId,
                    CensorByUserId = info.CensorByUserId,

                    CensorDatetimeUtc = info.CensorDatetimeUtc?.GetUnix(),
                    CreatedDatetimeUtc = info.CreatedDatetimeUtc.GetUnix(),
                    UpdatedDatetimeUtc = info.UpdatedDatetimeUtc.GetUnix(),
                });
            }

            return (result, total);

        }


        public async Task<PageData<PurchasingRequestOutputListByProduct>> GetListByProduct(string keyword, IList<int> productIds, EnumPurchasingRequestStatus? purchasingRequestStatusId, EnumPoProcessStatus? poProcessStatusId, bool? isApproved, long? fromDate, long? toDate, string sortBy, bool asc, int page, int size)
        {

            var query = from r in _purchaseOrderDBContext.PurchasingRequest
                        join d in _purchaseOrderDBContext.PurchasingRequestDetail on r.PurchasingRequestId equals d.PurchasingRequestId
                        select new
                        {
                            r.PurchasingRequestId,
                            r.PurchasingRequestStatusId,
                            r.OrderCode,
                            r.PurchasingRequestCode,
                            r.Content,
                            r.PoProcessStatusId,
                            r.IsApproved,
                            r.CreatedDatetimeUtc,
                            r.CreatedByUserId,
                            r.UpdatedByUserId,
                            r.UpdatedDatetimeUtc,
                            r.CensorByUserId,
                            r.CensorDatetimeUtc,
                            d.PurchasingRequestDetailId,
                            d.ProductId,
                            d.PrimaryQuantity
                        };

            if (productIds != null && productIds.Count > 0)
            {
                query = query.Where(q => productIds.Contains(q.ProductId));
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query
                    .Where(q => q.OrderCode.Contains(keyword)
                    || q.PurchasingRequestCode.Contains(keyword)
                    || q.Content.Contains(keyword));
            }

            if (purchasingRequestStatusId.HasValue)
            {
                query = query.Where(q => q.PurchasingRequestStatusId == (int)purchasingRequestStatusId.Value);
            }

            if (poProcessStatusId.HasValue)
            {
                query = query.Where(q => q.PoProcessStatusId == (int)poProcessStatusId.Value);
            }

            if (isApproved.HasValue)
            {
                query = query.Where(q => q.IsApproved == isApproved);
            }

            if (fromDate.HasValue)
            {
                var time = fromDate.Value.UnixToDateTime();
                query = query.Where(q => q.CreatedDatetimeUtc >= time);
            }

            if (toDate.HasValue)
            {
                var time = toDate.Value.UnixToDateTime();
                query = query.Where(q => q.CreatedDatetimeUtc <= time);
            }

            var total = await query.CountAsync();
            var pagedData = await query.SortByFieldName(sortBy, asc).Skip((page - 1) * size).Take(size).ToListAsync();
            var result = new List<PurchasingRequestOutputListByProduct>();
            foreach (var info in pagedData)
            {
                result.Add(new PurchasingRequestOutputListByProduct()
                {
                    PurchasingRequestId = info.PurchasingRequestId,
                    PurchasingRequestCode = info.PurchasingRequestCode,
                    OrderCode = info.OrderCode,
                    PurchasingRequestStatusId = (EnumPurchasingRequestStatus)info.PurchasingRequestStatusId,
                    IsApproved = info.IsApproved,
                    PoProcessStatusId = (EnumPoProcessStatus?)info.PoProcessStatusId,
                    CreatedByUserId = info.CreatedByUserId,
                    UpdatedByUserId = info.UpdatedByUserId,
                    CensorByUserId = info.CensorByUserId,

                    CensorDatetimeUtc = info.CensorDatetimeUtc?.GetUnix(),
                    CreatedDatetimeUtc = info.CreatedDatetimeUtc.GetUnix(),
                    UpdatedDatetimeUtc = info.UpdatedDatetimeUtc.GetUnix(),

                    PurchasingRequestDetailId = info.PurchasingRequestDetailId,
                    ProductId = info.ProductId,
                    PrimaryQuantity = info.PrimaryQuantity
                });
            }

            return (result, total);

        }

        public async Task<ServiceResult<long>> Create(PurchasingRequestInput model)
        {
            model.PurchasingRequestCode = (model.PurchasingRequestCode ?? "").Trim();
            if (!string.IsNullOrEmpty(model.PurchasingRequestCode))
            {
                var existedItem = await _purchaseOrderDBContext.PurchasingRequest.FirstOrDefaultAsync(r => r.PurchasingRequestCode == model.PurchasingRequestCode);
                if (existedItem != null) return PurchasingRequestErrorCode.RequestCodeAlreadyExisted;
            }


            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {

                var purchasingRequest = new PurchasingRequest()
                {
                    PurchasingRequestCode = model.PurchasingRequestCode,
                    OrderCode = model.OrderCode,
                    Content = model.Content,
                    RejectCount = 0,
                    PurchasingRequestStatusId = (int)EnumPurchasingRequestStatus.Draff,
                    IsApproved = null,
                    PoProcessStatusId = null,
                    IsDeleted = false,
                    CreatedByUserId = _currentContext.UserId,
                    UpdatedByUserId = _currentContext.UserId,
                    CreatedDatetimeUtc = DateTime.UtcNow,
                    UpdatedDatetimeUtc = DateTime.UtcNow
                };

                await _purchaseOrderDBContext.AddAsync(purchasingRequest);
                await _purchaseOrderDBContext.SaveChangesAsync();

                var purchasingRequestDetailList = model.Details.Select(d => new PurchasingRequestDetail
                {
                    PurchasingRequestId = purchasingRequest.PurchasingRequestId,
                    ProductId = d.ProductId,
                    PrimaryQuantity = d.PrimaryQuantity,
                    CreatedDatetimeUtc = DateTime.UtcNow,
                    UpdatedDatetimeUtc = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedDatetimeUtc = null
                });


                await _purchaseOrderDBContext.PurchasingRequestDetail.AddRangeAsync(purchasingRequestDetailList);
                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.PurchasingRequest, purchasingRequest.PurchasingRequestId, $"Thêm mới phiếu yêu cầu VTHH  {purchasingRequest.PurchasingRequestCode}", model.JsonSerialize());

                return purchasingRequest.PurchasingRequestId;
            }
        }

        public async Task<Enum> Update(long purchasingRequestId, PurchasingRequestInput model)
        {
            model.PurchasingRequestCode = (model.PurchasingRequestCode ?? "").Trim();
            if (!string.IsNullOrEmpty(model.PurchasingRequestCode))
            {
                var existedItem = await _purchaseOrderDBContext.PurchasingRequest.FirstOrDefaultAsync(r => r.PurchasingRequestId != purchasingRequestId && r.PurchasingRequestCode == model.PurchasingRequestCode);
                if (existedItem != null) return PurchasingRequestErrorCode.RequestCodeAlreadyExisted;
            }


            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var info = await _purchaseOrderDBContext.PurchasingRequest.FirstOrDefaultAsync(d => d.PurchasingRequestId == purchasingRequestId);
                if (info == null) return PurchasingRequestErrorCode.RequestNotFound;


                info.PurchasingRequestCode = model.PurchasingRequestCode;
                info.OrderCode = model.OrderCode;
                info.Content = model.Content;
                info.PurchasingRequestStatusId = (int)EnumPurchasingRequestStatus.Draff;
                info.IsApproved = null;
                info.UpdatedByUserId = _currentContext.UserId;
                info.UpdatedDatetimeUtc = DateTime.UtcNow;

                var oldDetails = await _purchaseOrderDBContext.PurchasingRequestDetail.Where(d => d.PurchasingRequestId == purchasingRequestId).ToListAsync();

                foreach (var item in oldDetails)
                {
                    item.IsDeleted = true;
                    item.DeletedDatetimeUtc = DateTime.UtcNow;
                }

                var purchasingRequestDetailList = model.Details.Select(d => new PurchasingRequestDetail
                {
                    PurchasingRequestId = purchasingRequestId,
                    ProductId = d.ProductId,
                    PrimaryQuantity = d.PrimaryQuantity,
                    CreatedDatetimeUtc = DateTime.UtcNow,
                    UpdatedDatetimeUtc = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedDatetimeUtc = null
                });


                await _purchaseOrderDBContext.PurchasingRequestDetail.AddRangeAsync(purchasingRequestDetailList);
                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.PurchasingRequest, purchasingRequestId, $"Cập nhật phiếu yêu cầu VTHH  {info.PurchasingRequestCode}", model.JsonSerialize());

                return GeneralCode.Success;
            }
        }

        public async Task<Enum> Delete(long purchasingRequestId)
        {
            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var info = await _purchaseOrderDBContext.PurchasingRequest.FirstOrDefaultAsync(d => d.PurchasingRequestId == purchasingRequestId);
                if (info == null) return PurchasingRequestErrorCode.RequestNotFound;


                info.IsDeleted = true;
                info.DeletedDatetimeUtc = DateTime.UtcNow;

                var oldDetails = await _purchaseOrderDBContext.PurchasingRequestDetail.Where(d => d.PurchasingRequestId == purchasingRequestId).ToListAsync();

                foreach (var item in oldDetails)
                {
                    item.IsDeleted = true;
                    item.DeletedDatetimeUtc = DateTime.UtcNow;
                }


                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.PurchasingRequest, purchasingRequestId, $"Xóa phiếu yêu cầu VTHH  {info.PurchasingRequestCode}", info.JsonSerialize());

                return GeneralCode.Success;
            }
        }

        public async Task<Enum> SendToCensor(long purchasingRequestId)
        {
            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var info = await _purchaseOrderDBContext.PurchasingRequest.FirstOrDefaultAsync(d => d.PurchasingRequestId == purchasingRequestId);
                if (info == null) return PurchasingRequestErrorCode.RequestNotFound;

                if (info.PurchasingRequestStatusId != (int)EnumPurchasingRequestStatus.Draff)
                {
                    return GeneralCode.InvalidParams;
                }

                info.PurchasingRequestStatusId = (int)EnumPurchasingRequestStatus.WaitToCensor;
                info.UpdatedDatetimeUtc = DateTime.UtcNow;
                info.UpdatedByUserId = _currentContext.UserId;


                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.PurchasingRequest, purchasingRequestId, $"Gửi duyệt yêu cầu VTHH  {info.PurchasingRequestCode}", info.JsonSerialize());

                return GeneralCode.Success;
            }
        }

        public async Task<Enum> Approve(long purchasingRequestId)
        {
            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var info = await _purchaseOrderDBContext.PurchasingRequest.FirstOrDefaultAsync(d => d.PurchasingRequestId == purchasingRequestId);
                if (info == null) return PurchasingRequestErrorCode.RequestNotFound;

                //allow re censored
                if (info.PurchasingRequestStatusId != (int)EnumPurchasingRequestStatus.WaitToCensor
                    && info.PurchasingRequestStatusId != (int)EnumPurchasingRequestStatus.Censored
                    )
                {
                    return GeneralCode.InvalidParams;
                }

                info.IsApproved = true;
                info.PurchasingRequestStatusId = (int)EnumPurchasingRequestStatus.Censored;
                info.CensorDatetimeUtc = DateTime.UtcNow;
                info.CensorByUserId = _currentContext.UserId;

                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.PurchasingRequest, purchasingRequestId, $"Duyệt yêu cầu VTHH  {info.PurchasingRequestCode}", info.JsonSerialize());

                return GeneralCode.Success;
            }
        }

        public async Task<Enum> Reject(long purchasingRequestId)
        {
            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var info = await _purchaseOrderDBContext.PurchasingRequest.FirstOrDefaultAsync(d => d.PurchasingRequestId == purchasingRequestId);
                if (info == null) return PurchasingRequestErrorCode.RequestNotFound;
                //allow re censored
                if (info.PurchasingRequestStatusId != (int)EnumPurchasingRequestStatus.WaitToCensor
                    && info.PurchasingRequestStatusId != (int)EnumPurchasingRequestStatus.Censored
                    )
                {
                    return GeneralCode.InvalidParams;
                }

                info.IsApproved = false;
                info.RejectCount++;

                info.PurchasingRequestStatusId = (int)EnumPurchasingRequestStatus.Censored;
                info.CensorDatetimeUtc = DateTime.UtcNow;
                info.CensorByUserId = _currentContext.UserId;

                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.PurchasingRequest, purchasingRequestId, $"Từ chối yêu cầu VTHH  {info.PurchasingRequestCode}", info.JsonSerialize());

                return GeneralCode.Success;
            }
        }

        public async Task<Enum> UpdatePoProcessStatus(long purchasingRequestId, EnumPoProcessStatus poProcessStatusId)
        {
            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var info = await _purchaseOrderDBContext.PurchasingRequest.FirstOrDefaultAsync(d => d.PurchasingRequestId == purchasingRequestId);
                if (info == null) return PurchasingRequestErrorCode.RequestNotFound;

                info.PoProcessStatusId = (int)poProcessStatusId;

                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.PurchasingRequest, purchasingRequestId, $"Cập nhật tiến trình PO yêu cầu VTHH  {info.PurchasingRequestCode}", info.JsonSerialize());

                return GeneralCode.Success;
            }
        }
    }
}
