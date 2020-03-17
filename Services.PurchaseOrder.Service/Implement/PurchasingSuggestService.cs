
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

        public PurchasingSuggestService(
            PurchaseOrderDBContext purchaseOrderDBContext
           , IOptions<AppSetting> appSetting
           , ILogger<PurchasingSuggestService> logger
           , IActivityLogService activityLogService
           , IAsyncRunnerService asyncRunner
           , ICurrentContextService currentContext
           )
        {
            _purchaseOrderDBContext = purchaseOrderDBContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
            _asyncRunner = asyncRunner;
            _currentContext = currentContext;
        }


        public async Task<ServiceResult<PurchasingSuggestOutput>> GetInfo(long PurchasingSuggestId)
        {
            var info = await _purchaseOrderDBContext.PurchasingSuggest.AsNoTracking()
                .FirstOrDefaultAsync(r => r.PurchasingSuggestId == PurchasingSuggestId);

            if (info == null) return PurchasingSuggestErrorCode.NotFound;

            var details = await _purchaseOrderDBContext.PurchasingSuggestDetail.AsNoTracking()
                .Where(d => d.PurchasingSuggestId == PurchasingSuggestId)
                .ToListAsync();

            return new PurchasingSuggestOutput()
            {
                PurchasingSuggestId = info.PurchasingSuggestId,
                PurchasingSuggestCode = info.PurchasingSuggestCode,
                OrderCode = info.OrderCode,
                Date = info.Date.GetUnix(),
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
                Details = details.Select(d => new PurchasingSuggestOutputDetail()
                {
                    PurchasingSuggestDetailId = d.PurchasingSuggestDetailId,
                    ProductId = d.ProductId,
                    PrimaryQuantity = d.PrimaryQuantity,

                    CustomerId = d.CustomerId,
                    PurchasingRequestIds = d.PurchasingRequestIds.JsonDeserialize<long[]>(),
                    PrimaryUnitPrice = d.PrimaryUnitPrice,
                    Tax = d.Tax
                }).ToList()
            };

        }

        public async Task<PageData<PurchasingSuggestOutputList>> GetList(string keyword, EnumPurchasingSuggestStatus? PurchasingSuggestStatusId, EnumPoProcessStatus? poProcessStatusId, bool? isApproved, long? fromDate, long? toDate, string sortBy, bool asc, int page, int size)
        {
            var query = _purchaseOrderDBContext.PurchasingSuggest.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query
                    .Where(q => q.OrderCode.Contains(keyword)
                    || q.PurchasingSuggestCode.Contains(keyword)
                    || q.Content.Contains(keyword));
            }

            if (PurchasingSuggestStatusId.HasValue)
            {
                query = query.Where(q => q.PurchasingSuggestStatusId == (int)PurchasingSuggestStatusId.Value);
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
                query = query.Where(q => q.Date >= time);
            }

            if (toDate.HasValue)
            {
                var time = toDate.Value.UnixToDateTime();
                query = query.Where(q => q.Date <= time);
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
                    Date = info.Date.GetUnix(),
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

        public async Task<ServiceResult<long>> Create(PurchasingSuggestInput model)
        {
            model.PurchasingSuggestCode = (model.PurchasingSuggestCode ?? "").Trim();
            if (!string.IsNullOrEmpty(model.PurchasingSuggestCode))
            {
                var existedItem = await _purchaseOrderDBContext.PurchasingSuggest.FirstOrDefaultAsync(r => r.PurchasingSuggestCode == model.PurchasingSuggestCode);
                if (existedItem != null) return PurchasingSuggestErrorCode.CodeAlreadyExisted;
            }


            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {

                var PurchasingSuggest = new PurchasingSuggest()
                {
                    PurchasingSuggestCode = model.PurchasingSuggestCode,
                    OrderCode = model.OrderCode,
                    Date = model.Date.UnixToDateTime(),
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

                await _purchaseOrderDBContext.AddAsync(PurchasingSuggest);
                await _purchaseOrderDBContext.SaveChangesAsync();

                var PurchasingSuggestDetailList = model.Details.Select(d => new PurchasingSuggestDetail
                {
                    PurchasingSuggestId = PurchasingSuggest.PurchasingSuggestId,
                    ProductId = d.ProductId,
                    PrimaryQuantity = d.PrimaryQuantity,
                    CreatedDatetimeUtc = DateTime.UtcNow,
                    UpdatedDatetimeUtc = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedDatetimeUtc = null,
                    CustomerId = d.CustomerId,
                    PrimaryUnitPrice = d.PrimaryUnitPrice,
                    Tax = d.Tax,
                    PurchasingRequestIds = d.PurchasingRequestIds.JsonSerialize()
                });


                await _purchaseOrderDBContext.PurchasingSuggestDetail.AddRangeAsync(PurchasingSuggestDetailList);
                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.PurchasingSuggest, PurchasingSuggest.PurchasingSuggestId, $"Thêm mới phiếu yêu cầu VTHH  {PurchasingSuggest.PurchasingSuggestCode}", model.JsonSerialize());

                return PurchasingSuggest.PurchasingSuggestId;
            }
        }

        public async Task<Enum> Update(long PurchasingSuggestId, PurchasingSuggestInput model)
        {
            model.PurchasingSuggestCode = (model.PurchasingSuggestCode ?? "").Trim();
            if (!string.IsNullOrEmpty(model.PurchasingSuggestCode))
            {
                var existedItem = await _purchaseOrderDBContext.PurchasingSuggest.FirstOrDefaultAsync(r => r.PurchasingSuggestId != PurchasingSuggestId && r.PurchasingSuggestCode == model.PurchasingSuggestCode);
                if (existedItem != null) return PurchasingSuggestErrorCode.CodeAlreadyExisted;
            }


            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var info = await _purchaseOrderDBContext.PurchasingSuggest.FirstOrDefaultAsync(d => d.PurchasingSuggestId == PurchasingSuggestId);
                if (info == null) return PurchasingSuggestErrorCode.NotFound;


                info.PurchasingSuggestCode = model.PurchasingSuggestCode;
                info.OrderCode = model.OrderCode;
                info.Date = model.Date.UnixToDateTime();
                info.Content = model.Content;
                info.PurchasingSuggestStatusId = (int)EnumPurchasingSuggestStatus.Draff;
                info.IsApproved = null;
                info.UpdatedByUserId = _currentContext.UserId;
                info.UpdatedDatetimeUtc = DateTime.UtcNow;

                var oldDetails = await _purchaseOrderDBContext.PurchasingSuggestDetail.Where(d => d.PurchasingSuggestId == PurchasingSuggestId).ToListAsync();

                foreach (var item in oldDetails)
                {
                    item.IsDeleted = true;
                    item.DeletedDatetimeUtc = DateTime.UtcNow;
                }

                var PurchasingSuggestDetailList = model.Details.Select(d => new PurchasingSuggestDetail
                {
                    PurchasingSuggestId = PurchasingSuggestId,
                    ProductId = d.ProductId,
                    PrimaryQuantity = d.PrimaryQuantity,
                    CreatedDatetimeUtc = DateTime.UtcNow,
                    UpdatedDatetimeUtc = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedDatetimeUtc = null,
                    CustomerId = d.CustomerId,
                    PrimaryUnitPrice = d.PrimaryUnitPrice,
                    Tax = d.Tax,
                    PurchasingRequestIds = d.PurchasingRequestIds.JsonSerialize()
                });


                await _purchaseOrderDBContext.PurchasingSuggestDetail.AddRangeAsync(PurchasingSuggestDetailList);
                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.PurchasingSuggest, PurchasingSuggestId, $"Cập nhật phiếu yêu cầu VTHH  {info.PurchasingSuggestCode}", model.JsonSerialize());

                return GeneralCode.Success;
            }
        }

        public async Task<Enum> Delete(long PurchasingSuggestId)
        {
            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var info = await _purchaseOrderDBContext.PurchasingSuggest.FirstOrDefaultAsync(d => d.PurchasingSuggestId == PurchasingSuggestId);
                if (info == null) return PurchasingSuggestErrorCode.NotFound;


                info.IsDeleted = true;
                info.DeletedDatetimeUtc = DateTime.UtcNow;

                var oldDetails = await _purchaseOrderDBContext.PurchasingSuggestDetail.Where(d => d.PurchasingSuggestId == PurchasingSuggestId).ToListAsync();

                foreach (var item in oldDetails)
                {
                    item.IsDeleted = true;
                    item.DeletedDatetimeUtc = DateTime.UtcNow;
                }


                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.PurchasingSuggest, PurchasingSuggestId, $"Xóa phiếu yêu cầu VTHH  {info.PurchasingSuggestCode}", info.JsonSerialize());

                return GeneralCode.Success;
            }
        }

        public async Task<Enum> SendToCensor(long PurchasingSuggestId)
        {
            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var info = await _purchaseOrderDBContext.PurchasingSuggest.FirstOrDefaultAsync(d => d.PurchasingSuggestId == PurchasingSuggestId);
                if (info == null) return PurchasingSuggestErrorCode.NotFound;

                info.PurchasingSuggestStatusId = (int)EnumPurchasingSuggestStatus.WaitToCensor;
                info.UpdatedDatetimeUtc = DateTime.UtcNow;
                info.UpdatedByUserId = _currentContext.UserId;


                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.PurchasingSuggest, PurchasingSuggestId, $"Gửi duyệt yêu cầu VTHH  {info.PurchasingSuggestCode}", info.JsonSerialize());

                return GeneralCode.Success;
            }
        }

        public async Task<Enum> Approve(long PurchasingSuggestId)
        {
            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var info = await _purchaseOrderDBContext.PurchasingSuggest.FirstOrDefaultAsync(d => d.PurchasingSuggestId == PurchasingSuggestId);
                if (info == null) return PurchasingSuggestErrorCode.NotFound;

                info.IsApproved = true;
                info.PurchasingSuggestStatusId = (int)EnumPurchasingSuggestStatus.Censored;
                info.CensorDatetimeUtc = DateTime.UtcNow;
                info.CensorByUserId = _currentContext.UserId;

                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.PurchasingSuggest, PurchasingSuggestId, $"Gửi duyệt yêu cầu VTHH  {info.PurchasingSuggestCode}", info.JsonSerialize());

                return GeneralCode.Success;
            }
        }

        public async Task<Enum> Reject(long PurchasingSuggestId)
        {
            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var info = await _purchaseOrderDBContext.PurchasingSuggest.FirstOrDefaultAsync(d => d.PurchasingSuggestId == PurchasingSuggestId);
                if (info == null) return PurchasingSuggestErrorCode.NotFound;

                info.IsApproved = false;
                info.RejectCount++;

                info.PurchasingSuggestStatusId = (int)EnumPurchasingSuggestStatus.Censored;
                info.CensorDatetimeUtc = DateTime.UtcNow;
                info.CensorByUserId = _currentContext.UserId;

                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.PurchasingSuggest, PurchasingSuggestId, $"Gửi duyệt yêu cầu VTHH  {info.PurchasingSuggestCode}", info.JsonSerialize());

                return GeneralCode.Success;
            }
        }

        public async Task<Enum> UpdatePoProcessStatus(long PurchasingSuggestId, EnumPoProcessStatus poProcessStatusId)
        {
            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var info = await _purchaseOrderDBContext.PurchasingSuggest.FirstOrDefaultAsync(d => d.PurchasingSuggestId == PurchasingSuggestId);
                if (info == null) return PurchasingSuggestErrorCode.NotFound;

                info.PoProcessStatusId = (int)poProcessStatusId;

                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.PurchasingSuggest, PurchasingSuggestId, $"Cập nhật trạng thái PO yêu cầu VTHH  {info.PurchasingSuggestCode}", info.JsonSerialize());

                return GeneralCode.Success;
            }
        }
    }
}
