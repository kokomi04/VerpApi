
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


        public async Task<ServiceResult<PurchasingSuggestOutput>> GetInfo(long purchasingSuggestId)
        {
            var info = await _purchaseOrderDBContext.PurchasingSuggest.AsNoTracking()
                .FirstOrDefaultAsync(r => r.PurchasingSuggestId == purchasingSuggestId);

            if (info == null) return PurchasingSuggestErrorCode.NotFound;

            var details = await _purchaseOrderDBContext.PurchasingSuggestDetail.AsNoTracking()
                .Where(d => d.PurchasingSuggestId == purchasingSuggestId)
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

                var purchasingSuggest = new PurchasingSuggest()
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

                await _purchaseOrderDBContext.AddAsync(purchasingSuggest);
                await _purchaseOrderDBContext.SaveChangesAsync();

                var purchasingSuggestDetailList = model.Details.Select(d => new PurchasingSuggestDetail
                {
                    PurchasingSuggestId = purchasingSuggest.PurchasingSuggestId,
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
                if (existedItem != null) return PurchasingSuggestErrorCode.CodeAlreadyExisted;
            }


            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var info = await _purchaseOrderDBContext.PurchasingSuggest.FirstOrDefaultAsync(d => d.PurchasingSuggestId == purchasingSuggestId);
                if (info == null) return PurchasingSuggestErrorCode.NotFound;


                info.PurchasingSuggestCode = model.PurchasingSuggestCode;
                info.OrderCode = model.OrderCode;
                info.Date = model.Date.UnixToDateTime();
                info.Content = model.Content;
                info.PurchasingSuggestStatusId = (int)EnumPurchasingSuggestStatus.Draff;
                info.IsApproved = null;
                info.UpdatedByUserId = _currentContext.UserId;
                info.UpdatedDatetimeUtc = DateTime.UtcNow;

                var oldDetails = await _purchaseOrderDBContext.PurchasingSuggestDetail.Where(d => d.PurchasingSuggestId == purchasingSuggestId).ToListAsync();

                foreach (var item in oldDetails)
                {
                    item.IsDeleted = true;
                    item.DeletedDatetimeUtc = DateTime.UtcNow;
                }

                var PurchasingSuggestDetailList = model.Details.Select(d => new PurchasingSuggestDetail
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
                    Tax = d.Tax,
                    PurchasingRequestIds = d.PurchasingRequestIds.JsonSerialize()
                });


                await _purchaseOrderDBContext.PurchasingSuggestDetail.AddRangeAsync(PurchasingSuggestDetailList);
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
                if (info == null) return PurchasingSuggestErrorCode.NotFound;


                info.IsDeleted = true;
                info.DeletedDatetimeUtc = DateTime.UtcNow;

                var oldDetails = await _purchaseOrderDBContext.PurchasingSuggestDetail.Where(d => d.PurchasingSuggestId == purchasingSuggestId).ToListAsync();

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
                if (info == null) return PurchasingSuggestErrorCode.NotFound;

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
                if (info == null) return PurchasingSuggestErrorCode.NotFound;

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
                if (info == null) return PurchasingSuggestErrorCode.NotFound;

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
                if (info == null) return PurchasingSuggestErrorCode.NotFound;

                info.PoProcessStatusId = (int)poProcessStatusId;

                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.PurchasingSuggest, purchasingSuggestId, $"Cập nhật trạng thái PO đề nghị mua hàng {info.PurchasingSuggestCode}", info.JsonSerialize());

                return GeneralCode.Success;
            }
        }



        public async Task<PageData<PoAssignmentOutputList>> PoAssignmentList(string keyword, EnumPoAssignmentStatus? poAssignmentStatusId, int? assigneeUserId, long? purchasingSuggestId, long? fromDate, long? toDate, string sortBy, bool asc, int page, int size)
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
                    a.CreatedDatetimeUtc
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
                AssigneeUserId = a.AssigneeUserId,
                IsConfirmed = a.IsConfirmed,
                CreatedDatetimeUtc = a.CreatedDatetimeUtc.GetUnix()
            }).ToList();

            return (lst, total);

        }
        public async Task<ServiceResult<IList<PoAssignmentOutput>>> PoAssignmentListBySuggest(long purchasingSuggestId)
        {

            var suggestInfo = await _purchaseOrderDBContext.PurchasingSuggest.AsNoTracking().FirstOrDefaultAsync(s => s.PurchasingSuggestId == purchasingSuggestId);

            if (suggestInfo == null)
            {
                return PurchasingSuggestErrorCode.NotFound;
            }

            var assignments = await _purchaseOrderDBContext.PoAssignment.AsNoTracking().Where(a => a.PurchasingSuggestId == purchasingSuggestId).ToListAsync();

            var assignmentDetails = await (
                from d in _purchaseOrderDBContext.PoAssignmentDetail
                join a in _purchaseOrderDBContext.PoAssignment on d.PoAssignmentId equals a.PoAssignmentId
                where a.PurchasingSuggestId == purchasingSuggestId
                select d
                ).AsNoTracking()
                .ToListAsync();
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
                    CreatedDatetimeUtc = item.CreatedDatetimeUtc.GetUnix(),
                    Content = item.Content,
                    PoAssignmentStatusId = (EnumPoAssignmentStatus)item.PoAssignmentStatusId,
                    Details = assignmentDetails
                        .Where(d => d.PoAssignmentId == item.PoAssignmentId)
                        .Select(d => new PoAssimentDetailModel()
                        {
                            PoAssignmentDetailId = d.PoAssignmentDetailId,
                            PurchasingSuggestDetailId = d.PurchasingSuggestDetailId,
                            ProviderProductName = d.ProviderProductName,
                            PrimaryQuantity = d.PrimaryQuantity,
                            PrimaryUnitPrice = d.PrimaryUnitPrice,
                            Tax = d.Tax
                        })
                        .ToList()
                });
            }
            return data;
        }
        public async Task<ServiceResult<long>> PoAssignmentCreate(long purchasingSuggestId, PoAssignmentInput model)
        {
            model.PoAssignmentCode = (model.PoAssignmentCode ?? "").Trim();
            if (!string.IsNullOrEmpty(model.PoAssignmentCode))
            {
                var existedItem = await _purchaseOrderDBContext.PoAssignment.FirstOrDefaultAsync(r => r.PoAssignmentCode == model.PoAssignmentCode);
                if (existedItem != null) return PurchasingSuggestErrorCode.PoAssignmentCodeAlreadyExisted;
            }

            var suggestInfo = await _purchaseOrderDBContext.PurchasingSuggest.FirstOrDefaultAsync(r => r.PurchasingSuggestId == purchasingSuggestId);

            if (suggestInfo == null)
            {
                return PurchasingSuggestErrorCode.NotFound;
            }

            if (model.Details.GroupBy(d => d.PurchasingSuggestDetailId).Any(g => g.Count() > 1))
            {
                return GeneralCode.InvalidParams;
            }

            var assignDetails = await (
                from d in _purchaseOrderDBContext.PoAssignmentDetail
                join a in _purchaseOrderDBContext.PoAssignment on d.PoAssignmentId equals a.PoAssignmentId
                where a.PurchasingSuggestId == purchasingSuggestId
                select new
                {
                    d.PoAssignmentDetailId,
                    d.PurchasingSuggestDetailId,
                    d.PrimaryQuantity
                }).ToListAsync();

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
                    return PurchasingSuggestErrorCode.PurchasingSuggestDetailNotfound;
                }

                var totalSameSuggestDetail = assignDetails
                    .Where(d => d.PurchasingSuggestDetailId == detail.PurchasingSuggestDetailId
                        && d.PoAssignmentDetailId != detail.PoAssignmentDetailId
                    )
                    .Sum(d => d.PrimaryQuantity);

                if (totalSameSuggestDetail + detail.PrimaryQuantity > suggestDetail.PrimaryQuantity)
                {

                    return PurchasingSuggestErrorCode.PoAssignmentOverload;
                }
            }

            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {

                var poAssignment = new PoAssignment()
                {
                    PurchasingSuggestId = purchasingSuggestId,
                    PoAssignmentCode = model.PoAssignmentCode,
                    Date = null,
                    Content = model.Content,
                    AssigneeUserId = model.AssigneeUserId,
                    PoAssignmentStatusId = (int)EnumPoAssignmentStatus.Draff,
                    IsConfirmed = null,
                    CreatedByUsersId = _currentContext.UserId,
                    UpdatedByUserId = _currentContext.UserId,
                    CreatedDatetimeUtc = DateTime.UtcNow,
                    UpdatedDatetimeUtc = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedDatetimeUtc = null
                };

                await _purchaseOrderDBContext.AddAsync(poAssignment);
                await _purchaseOrderDBContext.SaveChangesAsync();

                var poAssignmentDetails = model.Details.Select(d => new PoAssignmentDetail
                {
                    PoAssignmentId = poAssignment.PoAssignmentId,
                    PurchasingSuggestDetailId = d.PurchasingSuggestDetailId,
                    PrimaryQuantity = d.PrimaryQuantity,
                    PrimaryUnitPrice = d.PrimaryUnitPrice,
                    Tax = d.Tax,
                    CreatedDatetimeUtc = DateTime.UtcNow,
                    UpdatedDatetimeUtc = DateTime.UtcNow,
                    IsDeleted = false,
                    DeletedDatetimeUtc = null
                });


                await _purchaseOrderDBContext.PoAssignmentDetail.AddRangeAsync(poAssignmentDetails);
                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.PoAssignment, poAssignment.PoAssignmentId, $"Thêm phân công mua hàng {poAssignment.PoAssignmentCode}", model.JsonSerialize());

                return poAssignment.PoAssignmentId;
            }
        }

    }
}
