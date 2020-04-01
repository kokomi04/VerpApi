using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.ErrorCodes;
using VErp.Commons.Enums.ErrorCodes.PO;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.MasterEnum.PO;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.PurchaseOrderDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Service.Config;
using VErp.Services.PurchaseOrder.Model;
using PurchaseOrderModel = VErp.Infrastructure.EF.PurchaseOrderDB.PurchaseOrder;

namespace VErp.Services.PurchaseOrder.Service.Implement
{
    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly PurchaseOrderDBContext _purchaseOrderDBContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        private readonly IActivityLogService _activityLogService;
        private readonly IAsyncRunnerService _asyncRunner;
        private readonly ICurrentContextService _currentContext;
        private readonly IObjectGenCodeService _objectGenCodeService;

        public PurchaseOrderService(
            PurchaseOrderDBContext purchaseOrderDBContext
           , IOptions<AppSetting> appSetting
           , ILogger<PurchasingSuggestService> logger
           , IActivityLogService activityLogService
           , IAsyncRunnerService asyncRunner
           , ICurrentContextService currentContext
            , IObjectGenCodeService objectGenCodeService
           )
        {
            _purchaseOrderDBContext = purchaseOrderDBContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
            _asyncRunner = asyncRunner;
            _currentContext = currentContext;
            _objectGenCodeService = objectGenCodeService;
        }

        public async Task<PageData<PurchaseOrderOutputList>> GetList(string keyword, EnumPurchaseOrderStatus? purchaseOrderStatusId, EnumPoProcessStatus? poProcessStatusId, bool? isApproved, long? fromDate, long? toDate, string sortBy, bool asc, int page, int size)
        {
            var query = _purchaseOrderDBContext.PurchaseOrder.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query
                    .Where(q => q.PurchaseOrderCode.Contains(keyword)
                    || q.Content.Contains(keyword)
                    || q.AdditionNote.Contains(keyword));
            }

            if (purchaseOrderStatusId.HasValue)
            {
                query = query.Where(q => q.PurchaseOrderStatusId == (int)purchaseOrderStatusId.Value);
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
            var result = new List<PurchaseOrderOutputList>();
            foreach (var info in pagedData)
            {
                result.Add(new PurchaseOrderOutputList()
                {
                    PurchaseOrderId = info.PurchaseOrderId,
                    PurchaseOrderCode = info.PurchaseOrderCode,
                    Date = info.Date.GetUnix(),
                    CustomerId = info.CustomerId,
                    DeliveryDestination = info.DeliveryDestination?.JsonDeserialize<DeliveryDestinationModel>(),
                    Content = info.Content,
                    AdditionNote = info.AdditionNote,
                    DeliveryFee = info.DeliveryFee,
                    OtherFee = info.OtherFee,
                    TotalMoney = info.TotalMoney,
                    PurchaseOrderStatusId = (EnumPurchaseOrderStatus)info.PurchaseOrderStatusId,
                    IsApproved = info.IsApproved,
                    PoProcessStatusId = (EnumPoProcessStatus?)info.PoProcessStatusId,
                    CreatedByUserId = info.CreatedByUserId,
                    UpdatedByUserId = info.UpdatedByUserId,
                    CensorByUserId = info.CensorByUserId,

                    CensorDatetimeUtc = info.CensorDatetimeUtc.GetUnix(),
                    CreatedDatetimeUtc = info.CreatedDatetimeUtc.GetUnix(),
                    UpdatedDatetimeUtc = info.UpdatedDatetimeUtc.GetUnix()
                });
            }

            return (result, total);
        }

        public async Task<PageData<PurchaseOrderOutputListByProduct>> GetListByProduct(string keyword, IList<int> productIds, EnumPurchaseOrderStatus? purchaseOrderStatusId, EnumPoProcessStatus? poProcessStatusId, bool? isApproved, long? fromDate, long? toDate, string sortBy, bool asc, int page, int size)
        {
            var query = from po in _purchaseOrderDBContext.PurchaseOrder
                        join pod in _purchaseOrderDBContext.PurchaseOrderDetail on po.PurchaseOrderId equals pod.PurchaseOrderId
                        select new
                        {
                            po.PurchaseOrderId,
                            po.PurchaseOrderCode,
                            po.Date,
                            po.CustomerId,
                            po.DeliveryDestination,
                            po.Content,
                            po.AdditionNote,
                            po.DeliveryFee,
                            po.OtherFee,
                            po.TotalMoney,
                            po.PurchaseOrderStatusId,
                            po.IsApproved,
                            po.PoProcessStatusId,
                            po.CreatedByUserId,
                            po.UpdatedByUserId,
                            po.CensorByUserId,

                            po.CensorDatetimeUtc,
                            po.CreatedDatetimeUtc,
                            po.UpdatedDatetimeUtc,

                            //detail
                            pod.PurchaseOrderDetailId,
                            pod.PoAssignmentDetailId,
                            pod.ProviderProductName,

                            pod.ProductId,
                            pod.PrimaryQuantity,
                            pod.PrimaryUnitPrice,
                            pod.TaxInPercent,
                            pod.TaxInMoney,
                        };

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query
                    .Where(q => q.PurchaseOrderCode.Contains(keyword)
                    || q.Content.Contains(keyword)
                    || q.AdditionNote.Contains(keyword));
            }

            if (purchaseOrderStatusId.HasValue)
            {
                query = query.Where(q => q.PurchaseOrderStatusId == (int)purchaseOrderStatusId.Value);
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

            if (productIds != null && productIds.Count > 0)
            {
                query = query.Where(q => productIds.Contains(q.ProductId));
            }

            var total = await query.CountAsync();
            var pagedData = await query.SortByFieldName(sortBy, asc).Skip((page - 1) * size).Take(size).ToListAsync();
           
            var result = new List<PurchaseOrderOutputListByProduct>();
            foreach (var info in pagedData)
            {
                result.Add(new PurchaseOrderOutputListByProduct()
                {
                    PurchaseOrderId = info.PurchaseOrderId,
                    PurchaseOrderCode = info.PurchaseOrderCode,
                    Date = info.Date.GetUnix(),
                    CustomerId = info.CustomerId,
                    DeliveryDestination = info.DeliveryDestination?.JsonDeserialize<DeliveryDestinationModel>(),
                    Content = info.Content,
                    AdditionNote = info.AdditionNote,
                    DeliveryFee = info.DeliveryFee,
                    OtherFee = info.OtherFee,
                    TotalMoney = info.TotalMoney,
                    PurchaseOrderStatusId = (EnumPurchaseOrderStatus)info.PurchaseOrderStatusId,
                    IsApproved = info.IsApproved,
                    PoProcessStatusId = (EnumPoProcessStatus?)info.PoProcessStatusId,
                    CreatedByUserId = info.CreatedByUserId,
                    UpdatedByUserId = info.UpdatedByUserId,
                    CensorByUserId = info.CensorByUserId,

                    CensorDatetimeUtc = info.CensorDatetimeUtc.GetUnix(),
                    CreatedDatetimeUtc = info.CreatedDatetimeUtc.GetUnix(),
                    UpdatedDatetimeUtc = info.UpdatedDatetimeUtc.GetUnix(),


                    //detail
                    PurchaseOrderDetailId = info.PurchaseOrderDetailId,
                    PoAssignmentDetailId = info.PoAssignmentDetailId,
                    ProviderProductName = info.ProviderProductName,

                    ProductId = info.ProductId,
                    PrimaryQuantity = info.PrimaryQuantity,
                    PrimaryUnitPrice = info.PrimaryUnitPrice,
                    TaxInPercent = info.TaxInPercent,
                    TaxInMoney = info.TaxInMoney,
                });
            }

            return (result, total);
        }


        public async Task<ServiceResult<PurchaseOrderOutput>> GetInfo(long purchaseOrderId)
        {
            var info = await _purchaseOrderDBContext.PurchaseOrder.AsNoTracking()
               .FirstOrDefaultAsync(r => r.PurchaseOrderId == purchaseOrderId);

            if (info == null) return PurchaseOrderErrorCode.PoNotFound;

            var details = await (
                from d in _purchaseOrderDBContext.PurchaseOrderDetail.AsNoTracking()
                join pd in _purchaseOrderDBContext.PoAssignmentDetail on d.PoAssignmentDetailId equals pd.PoAssignmentDetailId
                join sd in _purchaseOrderDBContext.PurchasingSuggestDetail on pd.PurchasingSuggestDetailId equals sd.PurchasingSuggestDetailId
                where d.PurchaseOrderId == purchaseOrderId
                select new
                {
                    d.PurchaseOrderDetailId,
                    d.PoAssignmentDetailId,
                    d.ProviderProductName,
                    sd.ProductId,
                    d.PrimaryQuantity,
                    d.PrimaryUnitPrice,
                    d.TaxInPercent,
                    d.TaxInMoney
                }).ToListAsync();

            return new PurchaseOrderOutput()
            {
                PurchaseOrderId = info.PurchaseOrderId,
                PurchaseOrderCode = info.PurchaseOrderCode,
                Date = info.Date.GetUnix(),
                CustomerId = info.CustomerId,
                DeliveryDestination = info.DeliveryDestination?.JsonDeserialize<DeliveryDestinationModel>(),
                Content = info.Content,
                AdditionNote = info.AdditionNote,
                DeliveryFee = info.DeliveryFee,
                OtherFee = info.OtherFee,
                TotalMoney = info.TotalMoney,
                PurchaseOrderStatusId = (EnumPurchaseOrderStatus)info.PurchaseOrderStatusId,
                IsApproved = info.IsApproved,
                PoProcessStatusId = (EnumPoProcessStatus?)info.PoProcessStatusId,
                CreatedByUserId = info.CreatedByUserId,
                UpdatedByUserId = info.UpdatedByUserId,
                CensorByUserId = info.CensorByUserId,

                CensorDatetimeUtc = info.CensorDatetimeUtc.GetUnix(),
                CreatedDatetimeUtc = info.CreatedDatetimeUtc.GetUnix(),
                UpdatedDatetimeUtc = info.UpdatedDatetimeUtc.GetUnix(),

                Details = details.Select(d => new PurchaseOrderOutputDetail()
                {
                    PurchaseOrderDetailId = d.PurchaseOrderDetailId,
                    PoAssignmentDetailId = d.PoAssignmentDetailId,
                    ProviderProductName = d.ProviderProductName,
                    ProductId = d.ProductId,
                    PrimaryQuantity = d.PrimaryQuantity,
                    PrimaryUnitPrice = d.PrimaryUnitPrice,
                    TaxInPercent = d.TaxInPercent,
                    TaxInMoney = d.TaxInMoney,
                }).ToList()
            };
        }

        public async Task<ServiceResult<long>> Create(PurchaseOrderInput model)
        {
            var validate = await ValidatePoModelInput(null, model);

            if (!validate.IsSuccess())
            {
                return validate;
            }

            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var poAssignmentDetailIds = model.Details.Where(d => d.PoAssignmentDetailId.HasValue).Select(d => d.PoAssignmentDetailId.Value).ToList();

                var poAssignmentDetails = await GetPoAssignmentDetailInfos(poAssignmentDetailIds);

                var po = new PurchaseOrderModel()
                {
                    PurchaseOrderCode = model.PurchaseOrderCode,
                    PoAssignmentId = poAssignmentDetails.FirstOrDefault()?.PoAssignmentId,
                    CustomerId = model.CustomerId,
                    Date = model.Date.UnixToDateTime(),
                    DeliveryDestination = model.DeliveryDestination.JsonSerialize(),
                    Content = model.Content,
                    AdditionNote = model.AdditionNote,
                    PurchaseOrderStatusId = (int)EnumPurchaseOrderStatus.Draff,
                    IsApproved = null,
                    PoProcessStatusId = null,
                    DeliveryFee = model.DeliveryFee,
                    OtherFee = model.OtherFee,
                    TotalMoney = model.TotalMoney,
                    CreatedByUserId = _currentContext.UserId,
                    UpdatedByUserId = _currentContext.UserId,
                    CensorByUserId = null,
                    CreatedDatetimeUtc = DateTime.UtcNow,
                    UpdatedDatetimeUtc = DateTime.UtcNow,
                    CensorDatetimeUtc = null,
                    IsDeleted = false,
                    DeletedDatetimeUtc = null,

                };

                await _purchaseOrderDBContext.AddAsync(po);
                await _purchaseOrderDBContext.SaveChangesAsync();

                var poDetails = model.Details.Select(d =>
                {
                    return new PurchaseOrderDetail()
                    {
                        PurchaseOrderId = po.PurchaseOrderId,
                        PoAssignmentDetailId = d.PoAssignmentDetailId,
                        ProviderProductName = d.ProviderProductName,
                        PrimaryQuantity = d.PrimaryQuantity,
                        PrimaryUnitPrice = d.PrimaryUnitPrice,
                        TaxInPercent = d.TaxInPercent,
                        TaxInMoney = d.TaxInMoney,
                        CreatedDatetimeUtc = DateTime.UtcNow,
                        UpdatedDatetimeUtc = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedDatetimeUtc = null
                    };
                });


                await _purchaseOrderDBContext.PurchaseOrderDetail.AddRangeAsync(poDetails);
                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.PurchaseOrder, po.PurchaseOrderId, $"Tạo PO {po.PurchaseOrderCode}", model.JsonSerialize());

                return po.PurchaseOrderId;
            }

        }

        public async Task<ServiceResult> Update(long purchaseOrderId, PurchaseOrderInput model)
        {
            var validate = await ValidatePoModelInput(null, model);

            if (!validate.IsSuccess())
            {
                return validate;
            }

            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var info = await _purchaseOrderDBContext.PurchaseOrder.FirstOrDefaultAsync(d => d.PurchaseOrderId == purchaseOrderId);
                if (info == null) return PurchaseOrderErrorCode.PoNotFound;

                var poAssignmentDetailIds = model.Details.Where(d => d.PoAssignmentDetailId.HasValue).Select(d => d.PoAssignmentDetailId).ToList();

                var poAssignmentDetails = await GetPoAssignmentDetailInfos(poAssignmentDetailIds.Select(d => d.Value).ToList());


                info.PurchaseOrderCode = model.PurchaseOrderCode;
                info.PoAssignmentId = poAssignmentDetails.FirstOrDefault()?.PoAssignmentId;
                info.CustomerId = model.CustomerId;
                info.Date = model.Date.UnixToDateTime();
                info.DeliveryDestination = model.DeliveryDestination.JsonSerialize();
                info.Content = model.Content;
                info.AdditionNote = model.AdditionNote;
                info.PurchaseOrderStatusId = (int)EnumPurchaseOrderStatus.Draff;
                info.IsApproved = null;
                info.PoProcessStatusId = null;
                info.DeliveryFee = model.DeliveryFee;
                info.OtherFee = model.OtherFee;
                info.TotalMoney = model.TotalMoney;
                info.UpdatedByUserId = _currentContext.UserId;
                info.CensorByUserId = null;
                info.UpdatedDatetimeUtc = DateTime.UtcNow;
                info.CensorDatetimeUtc = null;
                info.IsDeleted = false;
                info.DeletedDatetimeUtc = null;



                var details = await _purchaseOrderDBContext.PurchaseOrderDetail.Where(d => d.PurchaseOrderId == purchaseOrderId).ToListAsync();

                var newDetails = new List<PurchaseOrderDetail>();

                foreach (var item in model.Details)
                {
                    var found = false;
                    foreach (var detail in details)
                    {
                        if (item.PurchaseOrderDetailId == detail.PurchaseOrderDetailId)
                        {
                            found = true;

                            detail.PoAssignmentDetailId = item.PoAssignmentDetailId;
                            detail.ProviderProductName = item.ProviderProductName;
                            detail.PrimaryQuantity = item.PrimaryQuantity;
                            detail.PrimaryUnitPrice = item.PrimaryUnitPrice;
                            detail.TaxInPercent = item.TaxInPercent;
                            detail.UpdatedDatetimeUtc = DateTime.UtcNow;
                            break;
                        }
                    }

                    if (!found)
                    {
                        newDetails.Add(new PurchaseOrderDetail()
                        {
                            PurchaseOrderId = info.PurchaseOrderId,
                            PoAssignmentDetailId = item.PoAssignmentDetailId,
                            ProviderProductName = item.ProviderProductName,
                            PrimaryQuantity = item.PrimaryQuantity,
                            PrimaryUnitPrice = item.PrimaryUnitPrice,
                            TaxInPercent = item.TaxInPercent,
                            TaxInMoney = item.TaxInMoney,
                            CreatedDatetimeUtc = DateTime.UtcNow,
                            UpdatedDatetimeUtc = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedDatetimeUtc = null
                        });
                    }
                }

                var updatedIds = model.Details.Select(d => d.PurchaseOrderDetailId).ToList();

                var deleteDetails = details.Where(d => !updatedIds.Contains(d.PurchaseOrderDetailId));

                foreach (var detail in deleteDetails)
                {
                    detail.IsDeleted = true;
                    detail.DeletedDatetimeUtc = DateTime.UtcNow;
                }

                await _purchaseOrderDBContext.PurchaseOrderDetail.AddRangeAsync(newDetails);

                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.PurchaseOrder, purchaseOrderId, $"Cập nhật PO {info.PurchaseOrderCode}", info.JsonSerialize());

                return GeneralCode.Success;
            }
        }

        public async Task<ServiceResult> Approve(long purchaseOrderId)
        {
            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var info = await _purchaseOrderDBContext.PurchaseOrder.FirstOrDefaultAsync(d => d.PurchaseOrderId == purchaseOrderId);
                if (info == null) return PurchaseOrderErrorCode.PoNotFound;

                if (info.PurchaseOrderStatusId != (int)EnumPurchaseOrderStatus.WaitToCensor
                  && info.PurchaseOrderStatusId != (int)EnumPurchaseOrderStatus.Censored
                  )
                {
                    return GeneralCode.InvalidParams;
                }

                info.IsApproved = true;

                info.PurchaseOrderStatusId = (int)EnumPurchaseOrderStatus.Censored;
                info.CensorDatetimeUtc = DateTime.UtcNow;
                info.CensorByUserId = _currentContext.UserId;

                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.PurchaseOrder, purchaseOrderId, $"Duyệt PO {info.PurchaseOrderCode}", info.JsonSerialize());

                return GeneralCode.Success;
            }
        }

        public async Task<ServiceResult> Delete(long purchaseOrderId)
        {
            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var info = await _purchaseOrderDBContext.PurchaseOrder.FirstOrDefaultAsync(d => d.PurchaseOrderId == purchaseOrderId);
                if (info == null) return PurchaseOrderErrorCode.PoNotFound;


                var oldDetails = await _purchaseOrderDBContext.PurchaseOrderDetail.Where(d => d.PurchaseOrderId == purchaseOrderId).ToListAsync();

                info.IsDeleted = true;
                info.DeletedDatetimeUtc = DateTime.UtcNow;

                foreach (var item in oldDetails)
                {
                    item.IsDeleted = true;
                    item.DeletedDatetimeUtc = DateTime.UtcNow;
                }

                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.PurchaseOrder, purchaseOrderId, $"Xóa PO {info.PurchaseOrderCode}", info.JsonSerialize());

                return GeneralCode.Success;
            }
        }


        public async Task<ServiceResult> Reject(long purchaseOrderId)
        {
            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var info = await _purchaseOrderDBContext.PurchaseOrder.FirstOrDefaultAsync(d => d.PurchaseOrderId == purchaseOrderId);
                if (info == null) return PurchaseOrderErrorCode.PoNotFound;

                if (info.PurchaseOrderStatusId != (int)EnumPurchaseOrderStatus.WaitToCensor
                  && info.PurchaseOrderStatusId != (int)EnumPurchaseOrderStatus.Censored
                  )
                {
                    return GeneralCode.InvalidParams;
                }

                info.IsApproved = false;

                info.PurchaseOrderStatusId = (int)EnumPurchaseOrderStatus.Censored;
                info.CensorDatetimeUtc = DateTime.UtcNow;
                info.CensorByUserId = _currentContext.UserId;

                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.PurchasingSuggest, purchaseOrderId, $"Từ chối PO {info.PurchaseOrderCode}", info.JsonSerialize());

                return GeneralCode.Success;
            }
        }

        public async Task<ServiceResult> SentToCensor(long purchaseOrderId)
        {
            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var info = await _purchaseOrderDBContext.PurchaseOrder.FirstOrDefaultAsync(d => d.PurchaseOrderId == purchaseOrderId);
                if (info == null) return PurchaseOrderErrorCode.PoNotFound;

                if (info.PurchaseOrderStatusId != (int)EnumPurchaseOrderStatus.Draff)
                {
                    return GeneralCode.InvalidParams;
                }

                info.PurchaseOrderStatusId = (int)EnumPurchaseOrderStatus.WaitToCensor;
                info.UpdatedDatetimeUtc = DateTime.UtcNow;
                info.UpdatedByUserId = _currentContext.UserId;


                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.PurchaseOrder, purchaseOrderId, $"Gửi duyệt PO {info.PurchaseOrderCode}", info.JsonSerialize());

                return GeneralCode.Success;
            }
        }


        public async Task<ServiceResult> UpdatePoProcessStatus(long purchaseOrderId, EnumPoProcessStatus poProcessStatusId)
        {
            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var info = await _purchaseOrderDBContext.PurchaseOrder.FirstOrDefaultAsync(d => d.PurchaseOrderId == purchaseOrderId);
                if (info == null) return PurchaseOrderErrorCode.PoNotFound;

                info.PoProcessStatusId = (int)poProcessStatusId;

                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.PurchaseOrder, purchaseOrderId, $"Cập nhật tiến trình PO {info.PurchaseOrderCode}", info.JsonSerialize());

                return GeneralCode.Success;
            }
        }

        private async Task<Enum> ValidatePoModelInput(long? poId, PurchaseOrderInput model)
        {
            if (!string.IsNullOrEmpty(model.PurchaseOrderCode))
            {
                var existedItem = await _purchaseOrderDBContext.PurchaseOrder.AsNoTracking().FirstOrDefaultAsync(r => r.PurchaseOrderCode == model.PurchaseOrderCode && r.PurchaseOrderId != poId);
                if (existedItem != null) return PurchaseOrderErrorCode.PoCodeAlreadyExisted;
            }
            else
            {
                return PurchaseOrderErrorCode.PoCodeAlreadyExisted;
            }

            PurchaseOrderModel poInfo = null;

            if (poId.HasValue)
            {
                poInfo = await _purchaseOrderDBContext.PurchaseOrder.AsNoTracking().FirstOrDefaultAsync(r => r.PurchaseOrderId == poId.Value);
                if (poInfo == null)
                {
                    return PurchaseOrderErrorCode.PoNotFound;
                }
            }

            if (model.Details.GroupBy(d => d.PoAssignmentDetailId).Any(d => d.Count() > 1))
            {
                return GeneralCode.InvalidParams;
            }

            var poAssignmentDetailIds = model.Details.Where(d => d.PoAssignmentDetailId.HasValue).Select(d => d.PoAssignmentDetailId).ToList();

            var poAssignmentDetails = await GetPoAssignmentDetailInfos(poAssignmentDetailIds.Select(a => a.Value).ToList());

            if (poAssignmentDetails.Select(d => d.PoAssignmentId).Distinct().Count() > 1 || poAssignmentDetails.Count == 0)
            {
                return GeneralCode.InvalidParams;
            }

            if (
                (from d in poAssignmentDetails
                 join m in model.Details on d.PoAssignmentDetailId equals m.PoAssignmentDetailId
                 where d.ProductId != m.ProductId
                 select 0
            ).Any())
            {
                return GeneralCode.InvalidParams;
            }


            foreach (var poAssignmentDetailId in poAssignmentDetailIds)
            {
                if (!poAssignmentDetails.Any(d => d.PoAssignmentDetailId == poAssignmentDetailId))
                {
                    return PurchasingSuggestErrorCode.PoAssignmentNotfound;
                }
            }

            if (poAssignmentDetails.Select(d => d.CustomerId).Distinct().Count() > 1)
            {
                return PurchaseOrderErrorCode.OnlyCreatePOFromOneCustomer;
            }


            var sameAssignmentDetails = await (
                from d in _purchaseOrderDBContext.PurchaseOrderDetail
                where poAssignmentDetailIds.Contains(d.PoAssignmentDetailId)
                select d
                ).AsNoTracking()
                .ToListAsync();

            if (sameAssignmentDetails.Any(d => d.PurchaseOrderId != poId))
            {
                return PurchaseOrderErrorCode.AssignmentDetailAlreadyCreatedPo;
            }

            return GeneralCode.Success;
        }


        private async Task<IList<PoAssignmentDetailInfo>> GetPoAssignmentDetailInfos(IList<long> poAssignmentDetailIds)
        {
            return await (
             from pd in _purchaseOrderDBContext.PoAssignmentDetail
             join sd in _purchaseOrderDBContext.PurchasingSuggestDetail on pd.PurchasingSuggestDetailId equals sd.PurchasingSuggestDetailId
             where poAssignmentDetailIds.Contains(pd.PoAssignmentDetailId)
             select new PoAssignmentDetailInfo
             {
                 PoAssignmentId = pd.PoAssignmentId,
                 PoAssignmentDetailId = pd.PoAssignmentDetailId,
                 ProductId = sd.ProductId,
                 CustomerId = sd.CustomerId,
                 PrimaryQuantity = pd.PrimaryQuantity,
                 PrimaryUnitPrice = pd.PrimaryUnitPrice,
                 TaxInPercent = pd.TaxInPercent,
                 TaxInMoney = pd.TaxInMoney
             }).AsNoTracking()
             .ToListAsync();
        }

        private class PoAssignmentDetailInfo
        {
            public long PoAssignmentId { get; set; }
            public long PoAssignmentDetailId { get; set; }
            public string ProviderProductName { get; set; }
            public int ProductId { get; set; }
            public int CustomerId { get; set; }
            public decimal PrimaryQuantity { get; set; }
            public decimal? PrimaryUnitPrice { get; set; }
            public decimal? TaxInPercent { get; set; }
            public decimal? TaxInMoney { get; set; }
        }
    }
}
