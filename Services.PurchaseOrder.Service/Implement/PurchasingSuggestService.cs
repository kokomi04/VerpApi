
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
using VErp.Services.Master.Service.Config;

namespace VErp.Services.PurchaseOrder.Service.Implement
{
    public class PurchasingSuggestService : IPurchasingSuggestService
    {
        private readonly PurchaseOrderDBContext _purchaseOrderDBContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly IAsyncRunnerService _asyncRunner;
        private readonly ICurrentContextService _currentContext;
        private readonly IObjectGenCodeService _objectGenCodeService;
        private readonly IPurchasingRequestService _purchasingRequestService;

        public PurchasingSuggestService(
            PurchaseOrderDBContext purchaseOrderDBContext
           , IOptions<AppSetting> appSetting
           , ILogger<PurchasingSuggestService> logger
           , IActivityLogService activityLogService
           , IAsyncRunnerService asyncRunner
           , ICurrentContextService currentContext
            , IObjectGenCodeService objectGenCodeService
            , IPurchasingRequestService purchasingRequestService
           )
        {
            _purchaseOrderDBContext = purchaseOrderDBContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
            _asyncRunner = asyncRunner;
            _currentContext = currentContext;
            _objectGenCodeService = objectGenCodeService;
            _purchasingRequestService = purchasingRequestService;
        }


        public async Task<ServiceResult<PurchasingSuggestOutput>> GetInfo(long purchasingSuggestId)
        {
            var info = await _purchaseOrderDBContext.PurchasingSuggest.AsNoTracking()
                .FirstOrDefaultAsync(r => r.PurchasingSuggestId == purchasingSuggestId);

            if (info == null) return PurchasingSuggestErrorCode.SuggestNotFound;

            var details = await _purchaseOrderDBContext.PurchasingSuggestDetail.AsNoTracking()
                .Where(d => d.PurchasingSuggestId == purchasingSuggestId)
                .ToListAsync();

            var files = await _purchaseOrderDBContext.PurchasingSuggestFile.AsNoTracking().Where(s => s.PurchasingSuggestId == purchasingSuggestId).ToListAsync();

            var requestDetailIds = details.Select(d => d.PurchasingRequestDetailId).Where(d => d.HasValue).Select(d => d.Value).ToList();

            var requestDetailInfos = (await _purchasingRequestService.PurchasingRequestDetailInfo(requestDetailIds)).ToDictionary(d => d.PurchasingRequestDetailId, d => d);

            return new PurchasingSuggestOutput()
            {
                PurchasingSuggestId = info.PurchasingSuggestId,
                PurchasingSuggestCode = info.PurchasingSuggestCode,
                OrderCode = info.OrderCode,
                PurchasingSuggestStatusId = (EnumPurchasingSuggestStatus)info.PurchasingSuggestStatusId,
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
                FileIds = files.Select(f => f.FileId).ToList(),
                Details = details.Select(d =>
                {
                    requestDetailInfos.TryGetValue(d.PurchasingRequestDetailId ?? 0, out var requestDetailInfo);
                    return new PurchasingSuggestDetailOutputModel()
                    {
                        PurchasingRequestDetailId = requestDetailInfo?.PurchasingRequestDetailId,
                        PurchasingRequestId = requestDetailInfo?.PurchasingRequestId,
                        PurchasingRequestCode = requestDetailInfo?.PurchasingRequestCode,
                        PurchasingRequestPrimaryQuantity = requestDetailInfo?.PrimaryQuantity,

                        PurchasingSuggestDetailId = d.PurchasingSuggestDetailId,
                        ProductId = d.ProductId,
                        PrimaryQuantity = d.PrimaryQuantity,

                        CustomerId = d.CustomerId,
                        PrimaryUnitPrice = d.PrimaryUnitPrice,
                        TaxInPercent = d.TaxInPercent,
                        TaxInMoney = d.TaxInMoney
                    };
                }
                ).ToList()
            };

        }

        public async Task<PageData<PurchasingSuggestOutputList>> GetList(string keyword, EnumPurchasingSuggestStatus? purchasingSuggestStatusId, EnumPoProcessStatus? poProcessStatusId, bool? isApproved, long? fromDate, long? toDate, string sortBy, bool asc, int page, int size)
        {
            var query = _purchaseOrderDBContext.PurchasingSuggest.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query
                    .Where(q => q.OrderCode.Contains(keyword)
                    || q.PurchasingSuggestCode.Contains(keyword)
                    || q.Content.Contains(keyword));
            }

            if (purchasingSuggestStatusId.HasValue)
            {
                query = query.Where(q => q.PurchasingSuggestStatusId == (int)purchasingSuggestStatusId.Value);
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
            var result = new List<PurchasingSuggestOutputList>();
            foreach (var info in pagedData)
            {
                result.Add(new PurchasingSuggestOutputList()
                {
                    PurchasingSuggestId = info.PurchasingSuggestId,
                    PurchasingSuggestCode = info.PurchasingSuggestCode,
                    OrderCode = info.OrderCode,
                    PurchasingSuggestStatusId = (EnumPurchasingSuggestStatus)info.PurchasingSuggestStatusId,
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

        public async Task<PageData<PurchasingSuggestOutputListByProduct>> GetListByProduct(string keyword, IList<int> productIds, EnumPurchasingSuggestStatus? purchasingSuggestStatusId, EnumPoProcessStatus? poProcessStatusId, bool? isApproved, long? fromDate, long? toDate, string sortBy, bool asc, int page, int size)
        {

            var query = from s in _purchaseOrderDBContext.PurchasingSuggest
                        join d in _purchaseOrderDBContext.PurchasingSuggestDetail on s.PurchasingSuggestId equals d.PurchasingSuggestId
                        select new
                        {
                            s.PurchasingSuggestId,
                            s.PurchasingSuggestStatusId,
                            s.OrderCode,
                            s.PurchasingSuggestCode,
                            s.Content,
                            s.PoProcessStatusId,
                            s.IsApproved,
                            s.CreatedDatetimeUtc,
                            s.CreatedByUserId,
                            s.UpdatedByUserId,
                            s.UpdatedDatetimeUtc,
                            s.CensorByUserId,
                            s.CensorDatetimeUtc,
                            s.RejectCount,

                            d.PurchasingSuggestDetailId,
                            d.CustomerId,
                            d.ProductId,
                            d.PrimaryQuantity,
                            d.PurchasingRequestIds,
                            d.PrimaryUnitPrice,
                            d.TaxInPercent,
                            d.TaxInMoney
                        };

            if (productIds != null && productIds.Count > 0)
            {
                query = query.Where(q => productIds.Contains(q.ProductId));
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query
                    .Where(q => q.OrderCode.Contains(keyword)
                    || q.PurchasingSuggestCode.Contains(keyword)
                    || q.Content.Contains(keyword));
            }

            if (purchasingSuggestStatusId.HasValue)
            {
                query = query.Where(q => q.PurchasingSuggestStatusId == (int)purchasingSuggestStatusId.Value);
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
            var result = new List<PurchasingSuggestOutputListByProduct>();
            foreach (var info in pagedData)
            {
                result.Add(new PurchasingSuggestOutputListByProduct()
                {
                    PurchasingSuggestId = info.PurchasingSuggestId,
                    PurchasingSuggestCode = info.PurchasingSuggestCode,
                    OrderCode = info.OrderCode,
                    PurchasingSuggestStatusId = (EnumPurchasingSuggestStatus)info.PurchasingSuggestStatusId,
                    IsApproved = info.IsApproved,
                    PoProcessStatusId = (EnumPoProcessStatus?)info.PoProcessStatusId,
                    CreatedByUserId = info.CreatedByUserId,
                    UpdatedByUserId = info.UpdatedByUserId,
                    CensorByUserId = info.CensorByUserId,

                    CensorDatetimeUtc = info.CensorDatetimeUtc.GetUnix(),
                    CreatedDatetimeUtc = info.CreatedDatetimeUtc.GetUnix(),
                    UpdatedDatetimeUtc = info.UpdatedDatetimeUtc.GetUnix(),

                    Content = info.Content,
                    RejectCount = info.RejectCount,
                    PurchasingSuggestDetailId = info.PurchasingSuggestDetailId,
                    CustomerId = info.CustomerId,
                    PurchasingRequestIds = info.PurchasingRequestIds?.JsonDeserialize<IList<long>>(),
                    ProductId = info.ProductId,
                    PrimaryQuantity = info.PrimaryQuantity,
                    PrimaryUnitPrice = info.PrimaryUnitPrice,
                    TaxInPercent = info.TaxInPercent,
                    TaxInMoney = info.TaxInMoney,
                });
            }

            return (result, total);

        }

        public async Task<ServiceResult<long>> Create(PurchasingSuggestInput model)
        {
            model.PurchasingSuggestCode = (model.PurchasingSuggestCode ?? "").Trim();
            if (!string.IsNullOrEmpty(model.PurchasingSuggestCode))
            {
                var existedItem = await _purchaseOrderDBContext.PurchasingSuggest.FirstOrDefaultAsync(r => r.PurchasingSuggestCode == model.PurchasingSuggestCode);
                if (existedItem != null) return PurchasingSuggestErrorCode.SuggestCodeAlreadyExisted;
            }


            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {

                var purchasingSuggest = new PurchasingSuggest()
                {
                    PurchasingSuggestCode = model.PurchasingSuggestCode,
                    OrderCode = model.OrderCode,
                    Content = model.Content,
                    RejectCount = 0,
                    PurchasingSuggestStatusId = (int)EnumPurchasingSuggestStatus.Draff,
                    IsApproved = null,
                    PoProcessStatusId = null,
                    IsDeleted = false,
                    CreatedByUserId = _currentContext.UserId,
                    UpdatedByUserId = _currentContext.UserId,
                    CreatedDatetimeUtc = DateTime.UtcNow,
                    UpdatedDatetimeUtc = DateTime.UtcNow
                };

                await _purchaseOrderDBContext.AddAsync(purchasingSuggest);
                await _purchaseOrderDBContext.SaveChangesAsync();

                var purchasingSuggestDetailList = model.Details.Select(d => PurchasingSuggestDetailObjectToEntity(purchasingSuggest.PurchasingSuggestId, d));

                if (model.FileIds?.Count > 0)
                {
                    await _purchaseOrderDBContext.PurchasingSuggestFile.AddRangeAsync(model.FileIds.Select(f => new PurchasingSuggestFile()
                    {
                        FileId = f,
                        PurchasingSuggestId = purchasingSuggest.PurchasingSuggestId
                    }));
                }

                await _purchaseOrderDBContext.PurchasingSuggestDetail.AddRangeAsync(purchasingSuggestDetailList);
                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.PurchasingSuggest, purchasingSuggest.PurchasingSuggestId, $"Thêm mới phiếu đề nghị mua hàng {purchasingSuggest.PurchasingSuggestCode}", model.JsonSerialize());

                return purchasingSuggest.PurchasingSuggestId;
            }
        }

        public async Task<Enum> Update(long purchasingSuggestId, PurchasingSuggestInput model)
        {
            model.PurchasingSuggestCode = (model.PurchasingSuggestCode ?? "").Trim();
            if (!string.IsNullOrEmpty(model.PurchasingSuggestCode))
            {
                var existedItem = await _purchaseOrderDBContext.PurchasingSuggest.FirstOrDefaultAsync(r => r.PurchasingSuggestId != purchasingSuggestId && r.PurchasingSuggestCode == model.PurchasingSuggestCode);
                if (existedItem != null) return PurchasingSuggestErrorCode.SuggestCodeAlreadyExisted;
            }

            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var info = await _purchaseOrderDBContext.PurchasingSuggest.FirstOrDefaultAsync(d => d.PurchasingSuggestId == purchasingSuggestId);
                if (info == null) return PurchasingSuggestErrorCode.SuggestNotFound;

                info.PurchasingSuggestCode = model.PurchasingSuggestCode;
                info.OrderCode = model.OrderCode;
                info.Content = model.Content;
                info.PurchasingSuggestStatusId = (int)EnumPurchasingSuggestStatus.Draff;
                info.IsApproved = null;
                info.UpdatedByUserId = _currentContext.UserId;
                info.UpdatedDatetimeUtc = DateTime.UtcNow;

                var details = await _purchaseOrderDBContext.PurchasingSuggestDetail.Where(d => d.PurchasingSuggestId == purchasingSuggestId).ToListAsync();

                var newDetails = new List<PurchasingSuggestDetail>();

                foreach (var item in model.Details)
                {
                    var found = false;
                    foreach (var detail in details)
                    {
                        if (item.PurchasingSuggestDetailId == detail.PurchasingSuggestDetailId)
                        {
                            found = true;
                            detail.ProductId = item.ProductId;
                            detail.PrimaryQuantity = item.PrimaryQuantity;
                            detail.UpdatedDatetimeUtc = DateTime.UtcNow;
                            detail.CustomerId = item.CustomerId;
                            detail.PrimaryUnitPrice = item.PrimaryUnitPrice;
                            detail.TaxInPercent = item.TaxInPercent;
                            detail.TaxInMoney = item.TaxInMoney;
                            detail.PurchasingRequestDetailId = item.PurchasingRequestDetailId;
                            break;
                        }
                    }

                    if (!found)
                    {
                        newDetails.Add(PurchasingSuggestDetailObjectToEntity(purchasingSuggestId, item));
                    }
                }

                var updatedIds = model.Details.Select(d => d.PurchasingSuggestDetailId).ToList();

                var deleteDetails = details.Where(d => !updatedIds.Contains(d.PurchasingSuggestDetailId));

                if (!await ValidateInUsePurchasingSuggestDetail(deleteDetails.Select(d => d.PurchasingSuggestDetailId).ToList()))
                {
                    trans.Rollback();
                    return PurchasingSuggestErrorCode.PoAssignmentDetailNotEmpty;
                }

                foreach (var detail in deleteDetails)
                {
                    detail.IsDeleted = true;
                    detail.DeletedDatetimeUtc = DateTime.UtcNow;
                }

                var files = await _purchaseOrderDBContext.PurchasingSuggestFile.Where(s => s.PurchasingSuggestId == purchasingSuggestId).ToListAsync();

                var dbFileIds = files.Select(f => f.FileId).ToList();

                if (model.FileIds?.Count > 0)
                {
                    var removeFiles = files.Where(f => !model.FileIds.Contains(f.FileId)).ToList();
                    foreach (var f in removeFiles)
                    {
                        f.IsDeleted = true;
                    }
                    var newFileIds = model.FileIds.Where(f => !dbFileIds.Contains(f)).ToList();
                    if (newFileIds.Count > 0)
                    {
                        await _purchaseOrderDBContext.PurchasingSuggestFile.AddRangeAsync(newFileIds.Select(f => new PurchasingSuggestFile()
                        {
                            FileId = f,
                            PurchasingSuggestId = purchasingSuggestId
                        }));
                    }
                }
                else
                {
                    foreach (var f in files)
                    {
                        f.IsDeleted = true;
                    }
                }

                await _purchaseOrderDBContext.PurchasingSuggestDetail.AddRangeAsync(newDetails);
                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.PurchasingSuggest, purchasingSuggestId, $"Cập nhật phiếu đề nghị mua hàng {info.PurchasingSuggestCode}", model.JsonSerialize());

                return GeneralCode.Success;
            }
        }

        public async Task<Enum> Delete(long purchasingSuggestId)
        {
            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var info = await _purchaseOrderDBContext.PurchasingSuggest.FirstOrDefaultAsync(d => d.PurchasingSuggestId == purchasingSuggestId);
                if (info == null) return PurchasingSuggestErrorCode.SuggestNotFound;


                var oldDetails = await _purchaseOrderDBContext.PurchasingSuggestDetail.Where(d => d.PurchasingSuggestId == purchasingSuggestId).ToListAsync();

                if (!await ValidateInUsePurchasingSuggestDetail(oldDetails.Select(d => d.PurchasingSuggestDetailId).ToList()))
                {
                    trans.Rollback();
                    return PurchasingSuggestErrorCode.PoAssignmentDetailNotEmpty;
                }

                info.IsDeleted = true;
                info.DeletedDatetimeUtc = DateTime.UtcNow;


                foreach (var item in oldDetails)
                {
                    item.IsDeleted = true;
                    item.DeletedDatetimeUtc = DateTime.UtcNow;
                }

                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.PurchasingSuggest, purchasingSuggestId, $"Xóa phiếu đề nghị mua hàng {info.PurchasingSuggestCode}", info.JsonSerialize());

                return GeneralCode.Success;
            }
        }

        public async Task<Enum> SendToCensor(long purchasingSuggestId)
        {
            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var info = await _purchaseOrderDBContext.PurchasingSuggest.FirstOrDefaultAsync(d => d.PurchasingSuggestId == purchasingSuggestId);
                if (info == null) return PurchasingSuggestErrorCode.SuggestNotFound;

                if (info.PurchasingSuggestStatusId != (int)EnumPurchasingSuggestStatus.Draff)
                {
                    return GeneralCode.InvalidParams;
                }

                info.PurchasingSuggestStatusId = (int)EnumPurchasingSuggestStatus.WaitToCensor;
                info.UpdatedDatetimeUtc = DateTime.UtcNow;
                info.UpdatedByUserId = _currentContext.UserId;


                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.PurchasingSuggest, purchasingSuggestId, $"Gửi duyệt đề nghị mua hàng {info.PurchasingSuggestCode}", info.JsonSerialize());

                return GeneralCode.Success;
            }
        }

        public async Task<Enum> Approve(long purchasingSuggestId)
        {
            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var info = await _purchaseOrderDBContext.PurchasingSuggest.FirstOrDefaultAsync(d => d.PurchasingSuggestId == purchasingSuggestId);
                if (info == null) return PurchasingSuggestErrorCode.SuggestNotFound;

                if (info.PurchasingSuggestStatusId != (int)EnumPurchasingSuggestStatus.WaitToCensor
                    && info.PurchasingSuggestStatusId != (int)EnumPurchasingSuggestStatus.Censored
                    )
                {
                    return GeneralCode.InvalidParams;
                }

                info.IsApproved = true;
                info.PurchasingSuggestStatusId = (int)EnumPurchasingSuggestStatus.Censored;
                info.CensorDatetimeUtc = DateTime.UtcNow;
                info.CensorByUserId = _currentContext.UserId;

                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.PurchasingSuggest, purchasingSuggestId, $"Duyệt đề nghị mua hàng {info.PurchasingSuggestCode}", info.JsonSerialize());

                return GeneralCode.Success;
            }
        }

        public async Task<Enum> Reject(long purchasingSuggestId)
        {
            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var info = await _purchaseOrderDBContext.PurchasingSuggest.FirstOrDefaultAsync(d => d.PurchasingSuggestId == purchasingSuggestId);
                if (info == null) return PurchasingSuggestErrorCode.SuggestNotFound;

                if (info.PurchasingSuggestStatusId == (int)EnumPurchasingSuggestStatus.Censored)
                {
                    var details = await _purchaseOrderDBContext.PurchasingSuggestDetail.Where(s => s.PurchasingSuggestId == purchasingSuggestId).ToListAsync();
                    var detailIds = details.Select(d => d.PurchasingSuggestDetailId).ToList();
                    if (!await ValidateInUsePurchasingSuggestDetail(detailIds))
                    {
                        return PurchasingSuggestErrorCode.CanNotRejectSuggestInUse;
                    }
                    return GeneralCode.InvalidParams;
                }

                if (info.PurchasingSuggestStatusId != (int)EnumPurchasingSuggestStatus.WaitToCensor
                  && info.PurchasingSuggestStatusId != (int)EnumPurchasingSuggestStatus.Censored
                  )
                {
                    return GeneralCode.InvalidParams;
                }

                info.IsApproved = false;
                info.RejectCount++;

                info.PurchasingSuggestStatusId = (int)EnumPurchasingSuggestStatus.Censored;
                info.CensorDatetimeUtc = DateTime.UtcNow;
                info.CensorByUserId = _currentContext.UserId;

                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.PurchasingSuggest, purchasingSuggestId, $"Từ chối đề nghị mua hàng {info.PurchasingSuggestCode}", info.JsonSerialize());

                return GeneralCode.Success;
            }
        }

        public async Task<Enum> UpdatePoProcessStatus(long purchasingSuggestId, EnumPoProcessStatus poProcessStatusId)
        {
            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var info = await _purchaseOrderDBContext.PurchasingSuggest.FirstOrDefaultAsync(d => d.PurchasingSuggestId == purchasingSuggestId);
                if (info == null) return PurchasingSuggestErrorCode.SuggestNotFound;

                info.PoProcessStatusId = (int)poProcessStatusId;

                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.PurchasingSuggest, purchasingSuggestId, $"Cập nhật tiến trình PO đề nghị mua hàng {info.PurchasingSuggestCode}", info.JsonSerialize());

                return GeneralCode.Success;
            }
        }

        public async Task<PageData<PoAssignmentOutputList>> PoAssignmentListByUser(string keyword, EnumPoAssignmentStatus? poAssignmentStatusId, int? assigneeUserId, long? purchasingSuggestId, long? fromDate, long? toDate, string sortBy, bool asc, int page, int size)
        {
            var query = (
                from s in _purchaseOrderDBContext.PurchasingSuggest
                join a in _purchaseOrderDBContext.PoAssignment on s.PurchasingSuggestId equals a.PurchasingSuggestId
                select new
                {
                    a.PoAssignmentId,
                    a.PurchasingSuggestId,
                    s.PurchasingSuggestCode,
                    s.OrderCode,
                    a.PoAssignmentCode,
                    a.AssigneeUserId,
                    a.PoAssignmentStatusId,
                    a.IsConfirmed,
                    a.CreatedByUserId,
                    a.CreatedDatetimeUtc,
                    a.Content
                });

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = from q in query
                        where q.PurchasingSuggestCode.Contains(keyword)
                        || q.PoAssignmentCode.Contains(keyword)
                        || q.OrderCode.Contains(keyword)
                        select q;
            }
            if (poAssignmentStatusId.HasValue)
            {
                query = from q in query
                        where q.PoAssignmentStatusId == (int)poAssignmentStatusId.Value
                        select q;
            }

            if (assigneeUserId.HasValue)
            {
                query = from q in query
                        where q.AssigneeUserId == assigneeUserId.Value
                        select q;
            }

            if (purchasingSuggestId.HasValue)
            {
                query = from q in query
                        where q.PurchasingSuggestId == purchasingSuggestId.Value
                        select q;
            }

            if (fromDate.HasValue)
            {
                var time = fromDate.Value.UnixToDateTime();
                query = from q in query
                        where q.CreatedDatetimeUtc >= time
                        select q;
            }

            if (toDate.HasValue)
            {
                var time = toDate.Value.UnixToDateTime();
                query = from q in query
                        where q.CreatedDatetimeUtc <= time
                        select q;
            }

            var total = await query.CountAsync();

            query = query.SortByFieldName(sortBy, asc);
            var pagedData = await query.Skip((page - 1) * size).Take(size).ToListAsync();

            var lst = pagedData.Select(a => new PoAssignmentOutputList
            {
                PoAssignmentId = a.PoAssignmentId,
                PurchasingSuggestId = a.PurchasingSuggestId,
                PurchasingSuggestCode = a.PurchasingSuggestCode,
                OrderCode = a.OrderCode,
                PoAssignmentCode = a.PoAssignmentCode,
                CreatedByUserId = a.CreatedByUserId,
                AssigneeUserId = a.AssigneeUserId,
                IsConfirmed = a.IsConfirmed,
                CreatedDatetimeUtc = a.CreatedDatetimeUtc.GetUnix(),
                Content = a.Content,
                PoAssignmentStatusId = (EnumPoAssignmentStatus)a.PoAssignmentStatusId,
            }).ToList();

            return (lst, total);

        }


        public async Task<PageData<PoAssignmentOutputListByProduct>> PoAssignmentListByProduct(string keyword, IList<int> productIds, EnumPoAssignmentStatus? poAssignmentStatusId, int? assigneeUserId, long? purchasingSuggestId, long? fromDate, long? toDate, string sortBy, bool asc, int page, int size)
        {
            var query = (
                from s in _purchaseOrderDBContext.PurchasingSuggest
                join a in _purchaseOrderDBContext.PoAssignment on s.PurchasingSuggestId equals a.PurchasingSuggestId
                join ad in _purchaseOrderDBContext.PoAssignmentDetail on a.PoAssignmentId equals ad.PoAssignmentId
                join sd in _purchaseOrderDBContext.PurchasingSuggestDetail on ad.PurchasingSuggestDetailId equals sd.PurchasingSuggestDetailId
                select new
                {
                    a.PoAssignmentId,
                    a.PurchasingSuggestId,
                    s.PurchasingSuggestCode,
                    s.OrderCode,
                    a.PoAssignmentCode,
                    a.AssigneeUserId,
                    a.PoAssignmentStatusId,
                    a.IsConfirmed,
                    a.CreatedByUserId,
                    a.CreatedDatetimeUtc,
                    a.Content,
                    ad.PoAssignmentDetailId,
                    ad.PurchasingSuggestDetailId,
                    sd.ProductId,
                    sd.CustomerId,
                    ad.PrimaryQuantity,
                    ad.PrimaryUnitPrice,
                    ad.TaxInPercent,
                    ad.TaxInMoney
                });

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = from q in query
                        where q.PurchasingSuggestCode.Contains(keyword)
                        || q.PoAssignmentCode.Contains(keyword)
                        || q.OrderCode.Contains(keyword)
                        select q;
            }
            if (poAssignmentStatusId.HasValue)
            {
                query = from q in query
                        where q.PoAssignmentStatusId == (int)poAssignmentStatusId.Value
                        select q;
            }

            if (assigneeUserId.HasValue)
            {
                query = from q in query
                        where q.AssigneeUserId == assigneeUserId.Value
                        select q;
            }

            if (purchasingSuggestId.HasValue)
            {
                query = from q in query
                        where q.PurchasingSuggestId == purchasingSuggestId.Value
                        select q;
            }

            if (fromDate.HasValue)
            {
                var time = fromDate.Value.UnixToDateTime();
                query = from q in query
                        where q.CreatedDatetimeUtc >= time
                        select q;
            }

            if (toDate.HasValue)
            {
                var time = toDate.Value.UnixToDateTime();
                query = from q in query
                        where q.CreatedDatetimeUtc <= time
                        select q;
            }

            if (productIds != null && productIds.Count > 0)
            {
                query = from q in query
                        where productIds.Contains(q.ProductId)
                        select q;
            }

            var total = await query.CountAsync();

            query = query.SortByFieldName(sortBy, asc);
            var pagedData = await query.Skip((page - 1) * size).Take(size).ToListAsync();

            var customerIds = pagedData.Select(d => d.CustomerId).ToList();
            var pagedProductIds = pagedData.Select(d => d.ProductId).ToList();

            var providerProductInfos = await (
              from p in _purchaseOrderDBContext.ProviderProductInfo.AsNoTracking()
              where customerIds.Contains(p.CustomerId) && pagedProductIds.Contains(p.ProductId)
              select p
              ).ToListAsync();

            var lst = pagedData.Select(a => new PoAssignmentOutputListByProduct
            {
                PoAssignmentId = a.PoAssignmentId,
                PurchasingSuggestId = a.PurchasingSuggestId,
                PurchasingSuggestCode = a.PurchasingSuggestCode,
                OrderCode = a.OrderCode,
                PoAssignmentCode = a.PoAssignmentCode,
                CreatedByUserId = a.CreatedByUserId,
                AssigneeUserId = a.AssigneeUserId,
                IsConfirmed = a.IsConfirmed,
                CreatedDatetimeUtc = a.CreatedDatetimeUtc.GetUnix(),
                Content = a.Content,
                PoAssignmentStatusId = (EnumPoAssignmentStatus)a.PoAssignmentStatusId,

                PoAssignmentDetailId = a.PoAssignmentDetailId,
                PurchasingSuggestDetailId = a.PurchasingSuggestDetailId,
                PrimaryQuantity = a.PrimaryQuantity,
                PrimaryUnitPrice = a.PrimaryUnitPrice,
                TaxInPercent = a.TaxInPercent,
                TaxInMoney = a.TaxInMoney,


                ProductId = a.ProductId,
                ProviderProductName = providerProductInfos.FirstOrDefault(p => p.CustomerId == a.CustomerId && p.ProductId == a.ProductId)?.ProviderProductName,
                CustomerId = a.CustomerId,

            }).ToList();

            return (lst, total);

        }

        public async Task<ServiceResult<IList<PoAssignmentOutput>>> PoAssignmentListBySuggest(long purchasingSuggestId)
        {

            var suggestInfo = await _purchaseOrderDBContext.PurchasingSuggest.AsNoTracking().FirstOrDefaultAsync(s => s.PurchasingSuggestId == purchasingSuggestId);

            if (suggestInfo == null)
            {
                return PurchasingSuggestErrorCode.SuggestNotFound;
            }

            var assignments = await _purchaseOrderDBContext.PoAssignment.AsNoTracking().Where(a => a.PurchasingSuggestId == purchasingSuggestId).ToListAsync();

            var assignmentDetails = await (
                from d in _purchaseOrderDBContext.PoAssignmentDetail
                join a in _purchaseOrderDBContext.PoAssignment on d.PoAssignmentId equals a.PoAssignmentId
                join s in _purchaseOrderDBContext.PurchasingSuggestDetail on d.PurchasingSuggestDetailId equals s.PurchasingSuggestDetailId
                where a.PurchasingSuggestId == purchasingSuggestId
                select new
                {
                    d.PoAssignmentId,
                    d.PoAssignmentDetailId,
                    d.PurchasingSuggestDetailId,
                    s.ProductId,
                    s.CustomerId,
                    d.PrimaryQuantity,
                    d.PrimaryUnitPrice,
                    d.TaxInPercent,
                    d.TaxInMoney
                }
                ).AsNoTracking()
                .ToListAsync();

            var customerIds = assignmentDetails.Select(d => d.CustomerId).ToList();
            var productIds = assignmentDetails.Select(d => d.ProductId).ToList();

            var providerProductInfos = await (
                from p in _purchaseOrderDBContext.ProviderProductInfo.AsNoTracking()
                where customerIds.Contains(p.CustomerId) && productIds.Contains(p.ProductId)
                select p
                ).ToListAsync();

            var data = new List<PoAssignmentOutput>();

            foreach (var item in assignments)
            {
                data.Add(new PoAssignmentOutput()
                {
                    PoAssignmentId = item.PoAssignmentId,
                    PurchasingSuggestId = item.PurchasingSuggestId,
                    PurchasingSuggestCode = suggestInfo.PurchasingSuggestCode,
                    OrderCode = suggestInfo.OrderCode,
                    PoAssignmentCode = item.PoAssignmentCode,
                    AssigneeUserId = item.AssigneeUserId,
                    IsConfirmed = item.IsConfirmed,
                    CreatedByUserId = item.CreatedByUserId,
                    CreatedDatetimeUtc = item.CreatedDatetimeUtc.GetUnix(),
                    Content = item.Content,
                    PoAssignmentStatusId = (EnumPoAssignmentStatus)item.PoAssignmentStatusId,
                    Details = assignmentDetails
                        .Where(d => d.PoAssignmentId == item.PoAssignmentId)
                        .Select(d => new PoAssimentDetailModel()
                        {
                            PoAssignmentDetailId = d.PoAssignmentDetailId,
                            PurchasingSuggestDetailId = d.PurchasingSuggestDetailId,
                            ProductId = d.ProductId,
                            CustomerId = d.CustomerId,
                            ProviderProductName = providerProductInfos.FirstOrDefault(p => p.CustomerId == d.CustomerId && p.ProductId == d.ProductId)?.ProviderProductName,
                            PrimaryQuantity = d.PrimaryQuantity,
                            PrimaryUnitPrice = d.PrimaryUnitPrice,
                            TaxInPercent = d.TaxInPercent,
                            TaxInMoney = d.TaxInMoney
                        })
                        .ToList()
                });
            }
            return data;
        }

        public async Task<ServiceResult<PoAssignmentOutput>> PoAssignmentInfo(long poAssignmentId, int? assigneeUserId)
        {
            var assignmentInfo = await _purchaseOrderDBContext.PoAssignment.AsNoTracking().Where(a => a.PoAssignmentId == poAssignmentId).FirstOrDefaultAsync();

            if (assignmentInfo == null)
            {
                return PurchasingSuggestErrorCode.PoAssignmentNotfound;
            }

            if (assigneeUserId.HasValue && assignmentInfo.AssigneeUserId != assigneeUserId)
            {
                return PurchasingSuggestErrorCode.PoAssignmentConfirmInvalidCurrentUser;
            }

            var suggestInfo = await _purchaseOrderDBContext.PurchasingSuggest.AsNoTracking().FirstOrDefaultAsync(s => s.PurchasingSuggestId == assignmentInfo.PurchasingSuggestId);

            if (suggestInfo == null)
            {
                return PurchasingSuggestErrorCode.SuggestNotFound;
            }

            var assignmentDetails = await (
                from d in _purchaseOrderDBContext.PoAssignmentDetail
                join a in _purchaseOrderDBContext.PoAssignment on d.PoAssignmentId equals a.PoAssignmentId
                join s in _purchaseOrderDBContext.PurchasingSuggestDetail on d.PurchasingSuggestDetailId equals s.PurchasingSuggestDetailId
                where a.PoAssignmentId == poAssignmentId
                select new
                {
                    d.PoAssignmentId,
                    d.PoAssignmentDetailId,
                    d.PurchasingSuggestDetailId,
                    s.ProductId,
                    s.CustomerId,
                    d.PrimaryQuantity,
                    d.PrimaryUnitPrice,
                    d.TaxInPercent,
                    d.TaxInMoney
                }
                ).AsNoTracking()
                .ToListAsync();

            var customerIds = assignmentDetails.Select(d => d.CustomerId).ToList();
            var productIds = assignmentDetails.Select(d => d.ProductId).ToList();

            var providerProductInfos = await (
                from p in _purchaseOrderDBContext.ProviderProductInfo.AsNoTracking()
                where customerIds.Contains(p.CustomerId) && productIds.Contains(p.ProductId)
                select p
                ).ToListAsync();

            var assignmentOutput = new PoAssignmentOutput()
            {
                PoAssignmentId = assignmentInfo.PoAssignmentId,
                PurchasingSuggestId = assignmentInfo.PurchasingSuggestId,
                PurchasingSuggestCode = suggestInfo.PurchasingSuggestCode,
                OrderCode = suggestInfo.OrderCode,
                PoAssignmentCode = assignmentInfo.PoAssignmentCode,
                AssigneeUserId = assignmentInfo.AssigneeUserId,
                IsConfirmed = assignmentInfo.IsConfirmed,
                CreatedByUserId = assignmentInfo.CreatedByUserId,
                CreatedDatetimeUtc = assignmentInfo.CreatedDatetimeUtc.GetUnix(),
                Content = assignmentInfo.Content,
                PoAssignmentStatusId = (EnumPoAssignmentStatus)assignmentInfo.PoAssignmentStatusId,
                Details = assignmentDetails
                        .Select(d => new PoAssimentDetailModel()
                        {
                            PoAssignmentDetailId = d.PoAssignmentDetailId,
                            PurchasingSuggestDetailId = d.PurchasingSuggestDetailId,
                            ProductId = d.ProductId,
                            CustomerId = d.CustomerId,
                            ProviderProductName = providerProductInfos.FirstOrDefault(p => p.CustomerId == d.CustomerId && p.ProductId == d.ProductId)?.ProviderProductName,
                            PrimaryQuantity = d.PrimaryQuantity,
                            PrimaryUnitPrice = d.PrimaryUnitPrice,
                            TaxInPercent = d.TaxInPercent,
                            TaxInMoney = d.TaxInMoney
                        })
                        .ToList()
            };

            return assignmentOutput;
        }


        public async Task<ServiceResult<long>> PoAssignmentCreate(long purchasingSuggestId, PoAssignmentInput model)
        {

            var validate = await ValidatePoAssignmentInput(purchasingSuggestId, null, model);

            if (!validate.IsSuccess())
            {
                return validate;
            }

            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var poAssignment = new PoAssignment()
                {
                    PurchasingSuggestId = purchasingSuggestId,
                    PoAssignmentCode = string.Empty,
                    Date = null,
                    Content = model.Content,
                    AssigneeUserId = model.AssigneeUserId,
                    PoAssignmentStatusId = (int)EnumPoAssignmentStatus.Draff,
                    IsConfirmed = null,
                    CreatedByUserId = _currentContext.UserId,
                    UpdatedByUserId = _currentContext.UserId,
                    CreatedDatetimeUtc = DateTime.UtcNow,
                    UpdatedDatetimeUtc = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedDatetimeUtc = null
                };

                await _purchaseOrderDBContext.AddAsync(poAssignment);
                await _purchaseOrderDBContext.SaveChangesAsync();

                var poAssignmentDetails = model.Details.Select(d => PoAssimentDetailObjectToEntity(poAssignment.PoAssignmentId, d));


                await _purchaseOrderDBContext.PoAssignmentDetail.AddRangeAsync(poAssignmentDetails);
                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.PoAssignment, poAssignment.PoAssignmentId, $"Thêm phân công mua hàng {poAssignment.PoAssignmentCode}", model.JsonSerialize());

                return poAssignment.PoAssignmentId;
            }
        }


        public async Task<ServiceResult> PoAssignmentUpdate(long purchasingSuggestId, long poAssignmentId, PoAssignmentInput model)
        {

            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var validate = await ValidatePoAssignmentInput(purchasingSuggestId, null, model);

                if (!validate.IsSuccess())
                {
                    trans.Rollback();
                    return validate;
                }

                var assignmentInfo = await _purchaseOrderDBContext.PoAssignment.FirstOrDefaultAsync(r => r.PoAssignmentId == poAssignmentId);

                if (assignmentInfo == null)
                {
                    return PurchasingSuggestErrorCode.PoAssignmentNotfound;
                }

                assignmentInfo.Date = null;
                assignmentInfo.Content = model.Content;
                assignmentInfo.AssigneeUserId = model.AssigneeUserId;
                assignmentInfo.PoAssignmentStatusId = (int)EnumPoAssignmentStatus.Draff;
                assignmentInfo.IsConfirmed = null;
                assignmentInfo.UpdatedByUserId = _currentContext.UserId;
                assignmentInfo.UpdatedDatetimeUtc = DateTime.UtcNow;

                var poAssignmentDetails = await _purchaseOrderDBContext.PoAssignmentDetail.Where(d => d.PoAssignmentId == poAssignmentId).ToListAsync();

                var newDetails = new List<PoAssignmentDetail>();

                foreach (var item in model.Details)
                {
                    var found = false;
                    foreach (var detail in poAssignmentDetails)
                    {
                        if (item.PoAssignmentDetailId == detail.PoAssignmentDetailId)
                        {
                            found = true;
                            detail.PurchasingSuggestDetailId = item.PurchasingSuggestDetailId;
                            detail.PrimaryQuantity = item.PrimaryQuantity;
                            detail.PrimaryUnitPrice = item.PrimaryUnitPrice;
                            detail.TaxInPercent = item.TaxInPercent;
                            detail.TaxInMoney = item.TaxInMoney;
                            detail.UpdatedDatetimeUtc = DateTime.UtcNow;
                            break;
                        }
                    }

                    if (!found)
                    {
                        newDetails.Add(PoAssimentDetailObjectToEntity(poAssignmentId, item));
                    }
                }

                var updatedIds = model.Details.Select(d => d.PoAssignmentDetailId).ToList();

                foreach (var detail in poAssignmentDetails.Where(d => !updatedIds.Contains(d.PoAssignmentDetailId)))
                {
                    detail.IsDeleted = true;
                    detail.DeletedDatetimeUtc = DateTime.UtcNow;
                }

                await _purchaseOrderDBContext.PoAssignmentDetail.AddRangeAsync(newDetails);
                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.PoAssignment, poAssignmentId, $"Cập nhật phân công mua hàng {assignmentInfo.PoAssignmentCode}", model.JsonSerialize());

                return GeneralCode.Success;
            }
        }

        public async Task<ServiceResult> PoAssignmentSendToUser(long purchasingSuggestId, long poAssignmentId)
        {
            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {

                var assignmentInfo = await _purchaseOrderDBContext.PoAssignment.FirstOrDefaultAsync(r => r.PoAssignmentId == poAssignmentId);

                if (assignmentInfo == null)
                {
                    return PurchasingSuggestErrorCode.PoAssignmentNotfound;
                }

                if (assignmentInfo.PurchasingSuggestId != purchasingSuggestId)
                {
                    return GeneralCode.InvalidParams;
                }

                if (assignmentInfo.PoAssignmentStatusId != (int)EnumPoAssignmentStatus.Draff)
                {
                    return GeneralCode.InvalidParams;
                }

                var code = await _objectGenCodeService.GenerateCode(EnumObjectType.PoAssignment);
                if (!code.Code.IsSuccess())
                {
                    return code.Code;
                }
                assignmentInfo.PoAssignmentCode = code.Data;
                assignmentInfo.PoAssignmentStatusId = (int)EnumPoAssignmentStatus.WaitToConfirm;
                assignmentInfo.UpdatedByUserId = _currentContext.UserId;
                assignmentInfo.UpdatedDatetimeUtc = DateTime.UtcNow;

                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.PoAssignment, poAssignmentId, $"Phát lệnh phân công mua hàng {assignmentInfo.PoAssignmentCode}", poAssignmentId.JsonSerialize());

                return GeneralCode.Success;
            }
        }

        public async Task<ServiceResult> PoAssignmentUserConfirm(long poAssignmentId)
        {
            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var assignmentInfo = await _purchaseOrderDBContext.PoAssignment.FirstOrDefaultAsync(r => r.PoAssignmentId == poAssignmentId);

                if (assignmentInfo == null)
                {
                    return PurchasingSuggestErrorCode.PoAssignmentNotfound;
                }

                if (assignmentInfo.AssigneeUserId != _currentContext.UserId)
                {
                    return PurchasingSuggestErrorCode.PoAssignmentConfirmInvalidCurrentUser;
                }


                if (assignmentInfo.PoAssignmentStatusId != (int)EnumPoAssignmentStatus.WaitToConfirm)
                {
                    return GeneralCode.InvalidParams;
                }


                assignmentInfo.PoAssignmentStatusId = (int)EnumPoAssignmentStatus.Confirmed;
                //assignmentInfo.UpdatedByUserId = _currentContext.UserId;
                assignmentInfo.UpdatedDatetimeUtc = DateTime.UtcNow;

                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.PoAssignment, poAssignmentId, $"Xác nhận phân công mua hàng {assignmentInfo.PoAssignmentCode}", poAssignmentId.JsonSerialize());

                return GeneralCode.Success;
            }
        }


        public async Task<ServiceResult> PoAssignmentDelete(long purchasingSuggestId, long poAssignmentId)
        {
            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var assignmentInfo = await _purchaseOrderDBContext.PoAssignment.FirstOrDefaultAsync(r => r.PoAssignmentId == poAssignmentId);

                if (assignmentInfo == null)
                {
                    return PurchasingSuggestErrorCode.PoAssignmentNotfound;
                }

                if (assignmentInfo.PurchasingSuggestId != purchasingSuggestId)
                {
                    return GeneralCode.InvalidParams;
                }

                var poAssignmentDetails = await _purchaseOrderDBContext.PoAssignmentDetail.Where(d => d.PoAssignmentId == poAssignmentId).ToListAsync();

                var deleteDetailIds = poAssignmentDetails.Select(d => d.PoAssignmentDetailId).ToList();

                if (!await ValidateDeletePoAssignmentDetail(deleteDetailIds))
                {
                    trans.Rollback();
                    return PurchasingSuggestErrorCode.PurchaseOrderDetailNotEmpty;
                }

                assignmentInfo.IsDeleted = true;
                assignmentInfo.DeletedDatetimeUtc = DateTime.UtcNow;


                foreach (var detail in poAssignmentDetails)
                {
                    detail.IsDeleted = true;
                    detail.DeletedDatetimeUtc = DateTime.UtcNow;
                }

                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.PoAssignment, poAssignmentId, $"Xóa phân công mua hàng {assignmentInfo.PoAssignmentCode}", assignmentInfo.JsonSerialize());

                return GeneralCode.Success;
            }
        }

        public async Task<IDictionary<long, IList<long>>> GetSuggestByRequest(IList<long> purchasingRequestIds)
        {
            var suggestDetail = await (
                from s in _purchaseOrderDBContext.PurchasingSuggestDetail
                join r in _purchaseOrderDBContext.PurchasingRequestDetail on s.PurchasingRequestDetailId equals r.PurchasingRequestDetailId
                where purchasingRequestIds.Contains(r.PurchasingRequestId)
                select new
                {
                    r.PurchasingRequestId,
                    s.PurchasingSuggestId
                }).ToListAsync();

            return purchasingRequestIds.Distinct()
                .ToDictionary(
                r => r,
                r => (IList<long>)suggestDetail.Where(d => d.PurchasingRequestId == r).Select(d => d.PurchasingSuggestId).Distinct().ToList()
                );
        }


        private PurchasingSuggestDetail PurchasingSuggestDetailObjectToEntity(long purchasingSuggestId, PurchasingSuggestDetailInputModel d)
        {
            return new PurchasingSuggestDetail
            {
                PurchasingSuggestId = purchasingSuggestId,
                ProductId = d.ProductId,
                PrimaryQuantity = d.PrimaryQuantity,
                CreatedDatetimeUtc = DateTime.UtcNow,
                UpdatedDatetimeUtc = DateTime.UtcNow,
                IsDeleted = false,
                DeletedDatetimeUtc = null,
                CustomerId = d.CustomerId,
                PrimaryUnitPrice = d.PrimaryUnitPrice,
                TaxInPercent = d.TaxInPercent,
                TaxInMoney = d.TaxInMoney,
                PurchasingRequestDetailId = d.PurchasingRequestDetailId
            };
        }

        private PoAssignmentDetail PoAssimentDetailObjectToEntity(long poAssignmentId, PoAssimentDetailModel d)
        {
            return new PoAssignmentDetail
            {
                PoAssignmentId = poAssignmentId,
                PurchasingSuggestDetailId = d.PurchasingSuggestDetailId,
                PrimaryQuantity = d.PrimaryQuantity,
                PrimaryUnitPrice = d.PrimaryUnitPrice,
                TaxInPercent = d.TaxInPercent,
                TaxInMoney = d.TaxInMoney,
                CreatedDatetimeUtc = DateTime.UtcNow,
                UpdatedDatetimeUtc = DateTime.UtcNow,
                IsDeleted = false,
                DeletedDatetimeUtc = null
            };
        }

        private async Task<Enum> ValidatePoAssignmentInput(long purchasingSuggestId, long? poAssignmentId, PoAssignmentInput model)
        {
            //if (!string.IsNullOrEmpty(model.PoAssignmentCode))
            //{
            //    var existedItem = await _purchaseOrderDBContext.PoAssignment.AsNoTracking().FirstOrDefaultAsync(r => r.PoAssignmentCode == model.PoAssignmentCode && r.PoAssignmentId != poAssignmentId);
            //    if (existedItem != null) return PurchasingSuggestErrorCode.PoAssignmentCodeAlreadyExisted;
            //}

            PoAssignment assignmentInfo = null;

            if (poAssignmentId.HasValue)
            {
                assignmentInfo = await _purchaseOrderDBContext.PoAssignment.AsNoTracking().FirstOrDefaultAsync(r => r.PoAssignmentId == poAssignmentId.Value);
                if (assignmentInfo == null)
                {
                    return PurchasingSuggestErrorCode.PoAssignmentNotfound;
                }

                if (assignmentInfo.PurchasingSuggestId != purchasingSuggestId)
                {
                    return GeneralCode.InvalidParams;
                }
            }

            var suggestInfo = await _purchaseOrderDBContext.PurchasingSuggest.FirstOrDefaultAsync(r => r.PurchasingSuggestId == purchasingSuggestId);

            if (suggestInfo == null)
            {
                return PurchasingSuggestErrorCode.SuggestNotFound;
            }

            if (suggestInfo.IsApproved != true)
            {
                return PurchasingSuggestErrorCode.PurchasingSuggestIsNotApprovedYet;
            }

            if (model.Details.GroupBy(d => d.PurchasingSuggestDetailId).Any(g => g.Count() > 1))
            {
                return GeneralCode.InvalidParams;
            }

            var sameSuggestAssignDetails = await (
                from d in _purchaseOrderDBContext.PoAssignmentDetail
                join a in _purchaseOrderDBContext.PoAssignment on d.PoAssignmentId equals a.PoAssignmentId
                where a.PurchasingSuggestId == purchasingSuggestId
                select d
                ).ToListAsync();

            var suggestDetails = (
                await _purchaseOrderDBContext.PurchasingSuggestDetail
                .AsNoTracking()
                .Where(r => r.PurchasingSuggestId == purchasingSuggestId)
                .ToListAsync()
                ).ToDictionary(d => d.PurchasingSuggestDetailId, d => d);

            foreach (var detail in model.Details)
            {
                if (!suggestDetails.TryGetValue(detail.PurchasingSuggestDetailId, out var suggestDetail))
                {
                    return PurchasingSuggestErrorCode.SuggestDetailNotfound;
                }

                var totalSameSuggestDetail = sameSuggestAssignDetails
                    .Where(d => d.PurchasingSuggestDetailId == detail.PurchasingSuggestDetailId
                        && d.PoAssignmentDetailId != detail.PoAssignmentDetailId
                    )
                    .Sum(d => d.PrimaryQuantity);

                if (totalSameSuggestDetail + detail.PrimaryQuantity > suggestDetail.PrimaryQuantity)
                {
                    return PurchasingSuggestErrorCode.PoAssignmentOverload;
                }
            }

            var updatedIds = model.Details.Select(d => d.PoAssignmentDetailId).ToList();

            var deleteDetailIds = sameSuggestAssignDetails
                .Where(d => d.PoAssignmentId == poAssignmentId && !updatedIds.Contains(d.PoAssignmentDetailId))
                .Select(d => d.PoAssignmentDetailId).ToList();

            if (!await ValidateDeletePoAssignmentDetail(deleteDetailIds))
            {
                return PurchasingSuggestErrorCode.PurchaseOrderDetailNotEmpty;
            }
            return GeneralCode.Success;
        }

        private async Task<bool> ValidateDeletePoAssignmentDetail(IList<long> deletePoAssignmentDetailIds)
        {
            if (deletePoAssignmentDetailIds == null || deletePoAssignmentDetailIds.Count == 0) return true;

            var poAssignmentDetailIds = deletePoAssignmentDetailIds.Cast<long?>();
            var poDetails = await _purchaseOrderDBContext.PurchaseOrderDetail.AsNoTracking().Where(d => poAssignmentDetailIds.Contains(d.PoAssignmentDetailId)).ToListAsync();
            foreach (var detail in deletePoAssignmentDetailIds)
            {
                if (poDetails.Any(d => d.PoAssignmentDetailId == detail))
                {
                    return false;
                }
            }
            return true;
        }


        private async Task<bool> ValidateInUsePurchasingSuggestDetail(IList<long> deletePurchasingSuggestDetailIds)
        {
            if (deletePurchasingSuggestDetailIds == null || deletePurchasingSuggestDetailIds.Count == 0) return true;

            var poDetails = await _purchaseOrderDBContext.PoAssignmentDetail.AsNoTracking().Where(d => deletePurchasingSuggestDetailIds.Contains(d.PurchasingSuggestDetailId)).ToListAsync();
            foreach (var detail in deletePurchasingSuggestDetailIds)
            {
                if (poDetails.Any(d => d.PurchasingSuggestDetailId == detail))
                {
                    return false;
                }
            }
            return true;
        }


    }
}
