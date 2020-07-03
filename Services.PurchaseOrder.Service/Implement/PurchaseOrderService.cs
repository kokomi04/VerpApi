using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.ErrorCodes;
using VErp.Commons.Enums.ErrorCodes.PO;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.MasterEnum.PO;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.PurchaseOrderDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Service.Config;
using VErp.Services.PurchaseOrder.Model;
using VErp.Services.PurchaseOrder.Model.PurchaseOrder;
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
        private readonly IPurchasingSuggestService _purchasingSuggestService;
        private readonly IProductHelperService _productHelperService;
        public PurchaseOrderService(
            PurchaseOrderDBContext purchaseOrderDBContext
           , IOptions<AppSetting> appSetting
           , ILogger<PurchasingSuggestService> logger
           , IActivityLogService activityLogService
           , IAsyncRunnerService asyncRunner
           , ICurrentContextService currentContext
           , IObjectGenCodeService objectGenCodeService
           , IPurchasingSuggestService purchasingSuggestService
           , IProductHelperService productHelperService
           )
        {
            _purchaseOrderDBContext = purchaseOrderDBContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
            _asyncRunner = asyncRunner;
            _currentContext = currentContext;
            _objectGenCodeService = objectGenCodeService;
            _purchasingSuggestService = purchasingSuggestService;
            _productHelperService = productHelperService;
        }

        public async Task<PageData<PurchaseOrderOutputList>> GetList(string keyword, EnumPurchaseOrderStatus? purchaseOrderStatusId, EnumPoProcessStatus? poProcessStatusId, bool? isChecked, bool? isApproved, long? fromDate, long? toDate, string sortBy, bool asc, int page, int size)
        {
            var query = from po in _purchaseOrderDBContext.PurchaseOrder
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
                            po.IsChecked,
                            po.IsApproved,
                            po.PoProcessStatusId,
                            po.CreatedByUserId,
                            po.UpdatedByUserId,
                            po.CheckedByUserId,
                            po.CensorByUserId,

                            po.CreatedDatetimeUtc,
                            po.UpdatedDatetimeUtc,
                            po.CheckedDatetimeUtc,
                            po.CensorDatetimeUtc,

                        };
            if (!string.IsNullOrWhiteSpace(keyword))
            {

                query = query
                   .Where(q => q.PurchaseOrderCode.Contains(keyword)
                   || q.Content.Contains(keyword)
                   || q.AdditionNote.Contains(keyword)
                   );
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

            if (isChecked.HasValue)
            {
                query = query.Where(q => q.IsChecked == isChecked);
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
                    IsChecked = info.IsChecked,
                    IsApproved = info.IsApproved,
                    PoProcessStatusId = (EnumPoProcessStatus?)info.PoProcessStatusId,
                    CreatedByUserId = info.CreatedByUserId,
                    UpdatedByUserId = info.UpdatedByUserId,
                    CheckedByUserId = info.CheckedByUserId,
                    CensorByUserId = info.CensorByUserId,

                    CreatedDatetimeUtc = info.CreatedDatetimeUtc.GetUnix(),
                    UpdatedDatetimeUtc = info.UpdatedDatetimeUtc.GetUnix(),
                    CheckedDatetimeUtc = info.CheckedDatetimeUtc.GetUnix(),
                    CensorDatetimeUtc = info.CensorDatetimeUtc.GetUnix()
                });
            }

            return (result, total);
        }

        public async Task<PageData<PurchaseOrderOutputListByProduct>> GetListByProduct(string keyword, IList<int> productIds, EnumPurchaseOrderStatus? purchaseOrderStatusId, EnumPoProcessStatus? poProcessStatusId, bool? isChecked, bool? isApproved, long? fromDate, long? toDate, string sortBy, bool asc, int page, int size)
        {
            var query = from po in _purchaseOrderDBContext.PurchaseOrder
                        join pod in _purchaseOrderDBContext.PurchaseOrderDetail on po.PurchaseOrderId equals pod.PurchaseOrderId
                        join ad in _purchaseOrderDBContext.PoAssignmentDetail on pod.PoAssignmentDetailId equals ad.PoAssignmentDetailId into ads
                        from ad in ads.DefaultIfEmpty()
                        join a in _purchaseOrderDBContext.PoAssignment on ad.PoAssignmentId equals a.PoAssignmentId into aa
                        from a in aa.DefaultIfEmpty()
                        join sd in _purchaseOrderDBContext.PurchasingSuggestDetail on pod.PurchasingSuggestDetailId equals sd.PurchasingSuggestDetailId into sds
                        from sd in sds.DefaultIfEmpty()
                        join s in _purchaseOrderDBContext.PurchasingSuggest on sd.PurchasingSuggestId equals s.PurchasingSuggestId into ss
                        from s in ss.DefaultIfEmpty()
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
                            po.IsChecked,
                            po.IsApproved,
                            po.PoProcessStatusId,
                            po.CreatedByUserId,
                            po.UpdatedByUserId,
                            po.CheckedByUserId,
                            po.CensorByUserId,

                            po.CreatedDatetimeUtc,
                            po.UpdatedDatetimeUtc,
                            po.CheckedDatetimeUtc,
                            po.CensorDatetimeUtc,

                            //detail
                            pod.PurchaseOrderDetailId,
                            pod.PoAssignmentDetailId,
                            pod.PurchasingSuggestDetailId,

                            pod.ProviderProductName,

                            pod.ProductId,
                            pod.PrimaryQuantity,
                            pod.PrimaryUnitPrice,

                            pod.ProductUnitConversionId,
                            pod.ProductUnitConversionQuantity,
                            pod.ProductUnitConversionPrice,

                            pod.TaxInPercent,
                            pod.TaxInMoney,
                            pod.Description,

                            pod.OrderCode,
                            pod.ProductionOrderCode,

                            PoAssignmentId = a == null ? (long?)null : a.PoAssignmentId,
                            PoAssignmentCode = a == null ? null : a.PoAssignmentCode,

                            PurchasingSuggestId = s == null ? (long?)null : s.PurchasingSuggestId,
                            PurchasingSuggestCode = s == null ? null : s.PurchasingSuggestCode
                        };

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query
                    .Where(q => q.PurchaseOrderCode.Contains(keyword)
                    || q.Content.Contains(keyword)
                    || q.AdditionNote.Contains(keyword)
                    || q.PoAssignmentCode.Contains(keyword)
                    || q.PurchasingSuggestCode.Contains(keyword)
                    || q.OrderCode.Contains(keyword)
                    || q.ProductionOrderCode.Contains(keyword)
                    );
            }

            if (purchaseOrderStatusId.HasValue)
            {
                query = query.Where(q => q.PurchaseOrderStatusId == (int)purchaseOrderStatusId.Value);
            }

            if (poProcessStatusId.HasValue)
            {
                query = query.Where(q => q.PoProcessStatusId == (int)poProcessStatusId.Value);
            }

            if (isChecked.HasValue)
            {
                query = query.Where(q => q.IsChecked == isChecked);
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

            var poAssignmentDetailIds = pagedData.Where(d => d.PoAssignmentDetailId.HasValue).Select(d => d.PoAssignmentDetailId.Value).ToList();
            var purchasingSuggestDetailIds = pagedData.Where(d => d.PurchasingSuggestDetailId.HasValue).Select(d => d.PurchasingSuggestDetailId.Value).ToList();

            var assignmentDetails = (await _purchasingSuggestService.PoAssignmentDetailInfos(poAssignmentDetailIds))
                .ToDictionary(d => d.PoAssignmentDetailId, d => d);

            var suggestDetails = (await _purchasingSuggestService.PurchasingSuggestDetailInfo(purchasingSuggestDetailIds))
                .ToDictionary(d => d.PurchasingSuggestDetailId, d => d);

            var result = new List<PurchaseOrderOutputListByProduct>();
            foreach (var info in pagedData)
            {
                assignmentDetails.TryGetValue(info.PoAssignmentDetailId ?? 0, out var assignmentDetailInfo);

                suggestDetails.TryGetValue(info.PurchasingSuggestDetailId ?? 0, out var purchasingSuggestDetailInfo);


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
                    IsChecked = info.IsChecked,
                    IsApproved = info.IsApproved,
                    PoProcessStatusId = (EnumPoProcessStatus?)info.PoProcessStatusId,
                    CreatedByUserId = info.CreatedByUserId,
                    UpdatedByUserId = info.UpdatedByUserId,
                    CheckedByUserId = info.CheckedByUserId,
                    CensorByUserId = info.CensorByUserId,

                    CreatedDatetimeUtc = info.CreatedDatetimeUtc.GetUnix(),
                    UpdatedDatetimeUtc = info.UpdatedDatetimeUtc.GetUnix(),
                    CheckedDatetimeUtc = info.CheckedDatetimeUtc.GetUnix(),
                    CensorDatetimeUtc = info.CensorDatetimeUtc.GetUnix(),


                    //detail
                    PurchaseOrderDetailId = info.PurchaseOrderDetailId,
                    PurchasingSuggestDetailId = info.PurchasingSuggestDetailId,
                    PoAssignmentDetailId = info.PoAssignmentDetailId,
                    ProviderProductName = info.ProviderProductName,

                    ProductId = info.ProductId,
                    PrimaryQuantity = info.PrimaryQuantity,
                    PrimaryUnitPrice = info.PrimaryUnitPrice,

                    ProductUnitConversionId = info.ProductUnitConversionId,
                    ProductUnitConversionQuantity = info.ProductUnitConversionQuantity,
                    ProductUnitConversionPrice = info.ProductUnitConversionPrice,

                    TaxInPercent = info.TaxInPercent,
                    TaxInMoney = info.TaxInMoney,
                    OrderCode = info.OrderCode,
                    ProductionOrderCode = info.ProductionOrderCode,
                    Description = info.Description,

                    PoAssignmentDetail = assignmentDetailInfo,
                    PurchasingSuggestDetail = purchasingSuggestDetailInfo
                });
            }

            return (result, total);
        }


        public async Task<PurchaseOrderOutput> GetInfo(long purchaseOrderId)
        {
            var info = await _purchaseOrderDBContext.PurchaseOrder.AsNoTracking().Where(po => po.PurchaseOrderId == purchaseOrderId).FirstOrDefaultAsync();

            if (info == null) throw new BadRequestException(PurchaseOrderErrorCode.PoNotFound);

            var details = await _purchaseOrderDBContext.PurchaseOrderDetail.AsNoTracking().Where(d => d.PurchaseOrderId == purchaseOrderId).ToListAsync();

            var poAssignmentDetailIds = details.Where(d => d.PoAssignmentDetailId.HasValue).Select(d => d.PoAssignmentDetailId.Value).ToList();
            var purchasingSuggestDetailIds = details.Where(d => d.PurchasingSuggestDetailId.HasValue).Select(d => d.PurchasingSuggestDetailId.Value).ToList();

            var assignmentDetails = (await _purchasingSuggestService.PoAssignmentDetailInfos(poAssignmentDetailIds))
                .ToDictionary(d => d.PoAssignmentDetailId, d => d);

            var suggestDetails = (await _purchasingSuggestService.PurchasingSuggestDetailInfo(purchasingSuggestDetailIds))
                .ToDictionary(d => d.PurchasingSuggestDetailId, d => d);


            return new PurchaseOrderOutput()
            {
                PurchaseOrderId = info.PurchaseOrderId,
                PurchaseOrderCode = info.PurchaseOrderCode,
                Date = info.Date.GetUnix(),
                CustomerId = info.CustomerId,
                PaymentInfo = info.PaymentInfo,

                DeliveryDate = info.DeliveryDate?.GetUnix(),
                DeliveryUserId = info.DeliveryUserId,
                DeliveryCustomerId = info.DeliveryCustomerId,

                DeliveryDestination = info.DeliveryDestination?.JsonDeserialize<DeliveryDestinationModel>(),
                Content = info.Content,
                AdditionNote = info.AdditionNote,
                DeliveryFee = info.DeliveryFee,
                OtherFee = info.OtherFee,
                TotalMoney = info.TotalMoney,
                PurchaseOrderStatusId = (EnumPurchaseOrderStatus)info.PurchaseOrderStatusId,
                IsChecked = info.IsChecked,
                IsApproved = info.IsApproved,
                PoProcessStatusId = (EnumPoProcessStatus?)info.PoProcessStatusId,
                CreatedByUserId = info.CreatedByUserId,
                UpdatedByUserId = info.UpdatedByUserId,
                CheckedByUserId = info.CheckedByUserId,
                CensorByUserId = info.CensorByUserId,

                CreatedDatetimeUtc = info.CreatedDatetimeUtc.GetUnix(),
                UpdatedDatetimeUtc = info.UpdatedDatetimeUtc.GetUnix(),
                CheckedDatetimeUtc = info.CheckedDatetimeUtc.GetUnix(),
                CensorDatetimeUtc = info.CensorDatetimeUtc.GetUnix(),

                Details = details.Select(d =>
                {
                    assignmentDetails.TryGetValue(d.PoAssignmentDetailId ?? 0, out var assignmentDetailInfo);

                    suggestDetails.TryGetValue(d.PurchasingSuggestDetailId ?? 0, out var purchasingSuggestDetailInfo);

                    return new PurchaseOrderOutputDetail()
                    {
                        PurchaseOrderDetailId = d.PurchaseOrderDetailId,
                        PoAssignmentDetailId = d.PoAssignmentDetailId,
                        ProviderProductName = d.ProviderProductName,
                        ProductId = d.ProductId,
                        PrimaryQuantity = d.PrimaryQuantity,
                        PrimaryUnitPrice = d.PrimaryUnitPrice,

                        ProductUnitConversionId = d.ProductUnitConversionId,
                        ProductUnitConversionQuantity = d.ProductUnitConversionQuantity,
                        ProductUnitConversionPrice = d.ProductUnitConversionPrice,

                        TaxInPercent = d.TaxInPercent,
                        TaxInMoney = d.TaxInMoney,
                        OrderCode = d.OrderCode,
                        ProductionOrderCode = d.ProductionOrderCode,
                        Description = d.Description,

                        PoAssignmentDetail = assignmentDetailInfo,
                        PurchasingSuggestDetail = purchasingSuggestDetailInfo,

                    };
                }
                ).ToList()
            };
        }

        public async Task<long> Create(PurchaseOrderInput model)
        {
            var validate = await ValidatePoModelInput(null, model);

            if (!validate.IsSuccess())
            {
                throw new BadRequestException(validate);
            }

            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var poAssignmentDetailIds = model.Details.Where(d => d.PoAssignmentDetailId.HasValue).Select(d => d.PoAssignmentDetailId.Value).ToList();

                var poAssignmentDetails = await GetPoAssignmentDetailInfos(poAssignmentDetailIds);

                var po = new PurchaseOrderModel()
                {
                    PurchaseOrderCode = model.PurchaseOrderCode,
                    CustomerId = model.CustomerId,
                    Date = model.Date.UnixToDateTime(),
                    PaymentInfo = model.PaymentInfo,
                    DeliveryDate = model.DeliveryDate?.UnixToDateTime(),
                    DeliveryUserId = model.DeliveryUserId,
                    DeliveryCustomerId = model.DeliveryCustomerId,
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

                if (po.DeliveryDestination?.Length > 1024)
                {
                    throw new BadRequestException(GeneralCode.InvalidParams, "Thông tin liên hệ giao hàng quá dài");
                }

                await _purchaseOrderDBContext.AddAsync(po);
                await _purchaseOrderDBContext.SaveChangesAsync();

                var poDetails = model.Details.Select(d =>
                {
                    var assignmentDetail = poAssignmentDetails.FirstOrDefault(a => a.PoAssignmentDetailId == d.PoAssignmentDetailId);

                    return new PurchaseOrderDetail()
                    {
                        PurchaseOrderId = po.PurchaseOrderId,

                        PurchasingSuggestDetailId = d.PurchasingSuggestDetailId.HasValue ?
                                                    d.PurchasingSuggestDetailId :
                                                    assignmentDetail?.PurchasingSuggestDetailId,

                        PoAssignmentDetailId = d.PoAssignmentDetailId,

                        ProductId = d.ProductId,

                        ProviderProductName = d.ProviderProductName,
                        PrimaryQuantity = d.PrimaryQuantity,
                        PrimaryUnitPrice = d.PrimaryUnitPrice,

                        ProductUnitConversionId = d.ProductUnitConversionId,
                        ProductUnitConversionQuantity = d.ProductUnitConversionQuantity,
                        ProductUnitConversionPrice = d.ProductUnitConversionPrice,

                        TaxInPercent = d.TaxInPercent,
                        TaxInMoney = d.TaxInMoney,
                        OrderCode = d.OrderCode,
                        ProductionOrderCode = d.ProductionOrderCode,
                        Description = d.Description,
                        CreatedDatetimeUtc = DateTime.UtcNow,
                        UpdatedDatetimeUtc = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedDatetimeUtc = null
                    };
                }).ToList();


                await _purchaseOrderDBContext.PurchaseOrderDetail.AddRangeAsync(poDetails);
                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.PurchaseOrder, po.PurchaseOrderId, $"Tạo PO {po.PurchaseOrderCode}", model.JsonSerialize());

                return po.PurchaseOrderId;
            }

        }

        public async Task<bool> Update(long purchaseOrderId, PurchaseOrderInput model)
        {
            var validate = await ValidatePoModelInput(purchaseOrderId, model);

            if (!validate.IsSuccess())
            {
                throw new BadRequestException(validate);
            }

            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var info = await _purchaseOrderDBContext.PurchaseOrder.FirstOrDefaultAsync(d => d.PurchaseOrderId == purchaseOrderId);
                if (info == null) throw new BadRequestException(PurchaseOrderErrorCode.PoNotFound);

                var poAssignmentDetailIds = model.Details.Where(d => d.PoAssignmentDetailId.HasValue).Select(d => d.PoAssignmentDetailId).ToList();

                var poAssignmentDetails = await GetPoAssignmentDetailInfos(poAssignmentDetailIds.Select(d => d.Value).ToList());


                info.PurchaseOrderCode = model.PurchaseOrderCode;

                info.CustomerId = model.CustomerId;
                info.Date = model.Date.UnixToDateTime();
                info.PaymentInfo = model.PaymentInfo;
                info.DeliveryDate = model.DeliveryDate?.UnixToDateTime();
                info.DeliveryUserId = model.DeliveryUserId;
                info.DeliveryCustomerId = model.DeliveryCustomerId;

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

                if (info.DeliveryDestination?.Length > 1024)
                {
                    throw new BadRequestException(GeneralCode.InvalidParams, "Thông tin liên hệ giao hàng quá dài");
                }


                var details = await _purchaseOrderDBContext.PurchaseOrderDetail.Where(d => d.PurchaseOrderId == purchaseOrderId).ToListAsync();

                var newDetails = new List<PurchaseOrderDetail>();

                foreach (var item in model.Details)
                {
                    var assignmentDetail = poAssignmentDetails.FirstOrDefault(a => a.PoAssignmentDetailId == item.PoAssignmentDetailId);

                    var found = false;
                    foreach (var detail in details)
                    {
                        if (item.PurchaseOrderDetailId == detail.PurchaseOrderDetailId)
                        {
                            found = true;

                            detail.PurchasingSuggestDetailId = item.PurchasingSuggestDetailId.HasValue ?
                                                        item.PurchasingSuggestDetailId :
                                                        assignmentDetail?.PurchasingSuggestDetailId;

                            detail.PoAssignmentDetailId = item.PoAssignmentDetailId;
                            detail.ProviderProductName = item.ProviderProductName;
                            detail.PrimaryQuantity = item.PrimaryQuantity;
                            detail.PrimaryUnitPrice = item.PrimaryUnitPrice;

                            detail.ProductUnitConversionId = item.ProductUnitConversionId;
                            detail.ProductUnitConversionQuantity = item.ProductUnitConversionQuantity;
                            detail.ProductUnitConversionPrice = item.ProductUnitConversionPrice;

                            detail.TaxInPercent = item.TaxInPercent;
                            detail.TaxInMoney = item.TaxInMoney;
                            detail.OrderCode = item.OrderCode;
                            detail.ProductionOrderCode = item.ProductionOrderCode;
                            detail.Description = item.Description;
                            detail.UpdatedDatetimeUtc = DateTime.UtcNow;
                            break;
                        }
                    }

                    if (!found)
                    {
                        newDetails.Add(new PurchaseOrderDetail()
                        {
                            PurchaseOrderId = info.PurchaseOrderId,

                            PurchasingSuggestDetailId = item.PurchasingSuggestDetailId.HasValue ?
                                                    item.PurchasingSuggestDetailId :
                                                    assignmentDetail?.PurchasingSuggestDetailId,

                            PoAssignmentDetailId = item.PoAssignmentDetailId,

                            ProviderProductName = item.ProviderProductName,
                            PrimaryQuantity = item.PrimaryQuantity,
                            PrimaryUnitPrice = item.PrimaryUnitPrice,
                            ProductUnitConversionId = item.ProductUnitConversionId,
                            ProductUnitConversionQuantity = item.ProductUnitConversionQuantity,
                            ProductUnitConversionPrice = item.ProductUnitConversionPrice,
                            TaxInPercent = item.TaxInPercent,
                            TaxInMoney = item.TaxInMoney,
                            OrderCode = item.OrderCode,
                            ProductionOrderCode = item.ProductionOrderCode,
                            Description = item.Description,
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

                return true;
            }
        }



        public async Task<bool> Delete(long purchaseOrderId)
        {
            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var info = await _purchaseOrderDBContext.PurchaseOrder.FirstOrDefaultAsync(d => d.PurchaseOrderId == purchaseOrderId);
                if (info == null) throw new BadRequestException(PurchaseOrderErrorCode.PoNotFound);


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

                return true;
            }
        }


        public async IAsyncEnumerable<PurchaseOrderInputDetail> ParseInvoiceDetails(SingleInvoicePoExcelMappingModel mapping, Stream stream)
        {
            var rowDatas = SingleInvoiceParseExcel(mapping, stream);

            var productCodes = rowDatas.Select(r => r.ProductCode).ToList();
            var productInternalNames = rowDatas.Select(r => r.ProductInternalName).ToList();

            var productInfos = await _productHelperService.GetListByCodeAndInternalNames(productCodes, productInternalNames);

            var productInfoByCode = productInfos.GroupBy(p => p.ProductCode)
                .ToDictionary(p => p.Key.Trim().ToLower(), p => p.ToList());

            var productInfoByInternalName = productInfos.GroupBy(p => p.ProductName.NormalizeAsInternalName())
                .ToDictionary(p => p.Key.Trim().ToLower(), p => p.ToList());

            foreach (var item in rowDatas)
            {
                IList<IProductModel> productInfo = null;
                if (!string.IsNullOrWhiteSpace(item.ProductCode) && productInfoByCode.ContainsKey(item.ProductCode?.ToLower()))
                {
                    productInfo = productInfoByCode[item.ProductCode?.ToLower()];
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(item.ProductInternalName) && productInfoByInternalName.ContainsKey(item.ProductInternalName))
                    {
                        productInfo = productInfoByCode[item.ProductInternalName];
                    }
                }

                if (productInfo == null || productInfo.Count == 0)
                {
                    throw new BadRequestException(GeneralCode.ItemNotFound, $"Không tìm thấy mặt hàng {item.ProductCode} {item.ProductName}");
                }

                if (productInfo.Count > 1)
                {
                    productInfo = productInfo.Where(p => p.ProductName == item.ProductName).ToList();

                    if (productInfo.Count != 1)
                        throw new BadRequestException(GeneralCode.InvalidParams, $"Tìm thấy {productInfo.Count} mặt hàng {item.ProductCode} {item.ProductName}");
                }

                var productUnitConversionId = 0;
                if (string.IsNullOrWhiteSpace(item.ProductUnitConversionName))
                {
                    var pus = productInfo[0].StockInfo.UnitConversions
                            .Where(u => u.ProductUnitConversionName.NormalizeAsInternalName() == item.ProductUnitConversionName.NormalizeAsInternalName())
                            .ToList();

                    if (pus.Count != 1)
                    {
                        pus = productInfo[0].StockInfo.UnitConversions
                           .Where(u => u.ProductUnitConversionName.Contains(item.ProductUnitConversionName) || item.ProductUnitConversionName.Contains(u.ProductUnitConversionName))
                           .ToList();

                        if (pus.Count > 1)
                        {
                            pus = productInfo[0].StockInfo.UnitConversions
                             .Where(u => u.ProductUnitConversionName.Equals(item.ProductUnitConversionName, StringComparison.OrdinalIgnoreCase))
                             .ToList();
                        }
                    }

                    if (pus.Count == 0)
                    {
                        throw new BadRequestException(GeneralCode.InvalidParams, $"Không tìm thấy đơn vị chuyển đổi {item.ProductUnitConversionName} mặt hàng {item.ProductCode} {item.ProductName}");
                    }
                    if (pus.Count > 1)
                    {
                        throw new BadRequestException(GeneralCode.InvalidParams, $"Tìm thấy {pus.Count} đơn vị chuyển đổi {item.ProductUnitConversionName} mặt hàng {item.ProductCode} {item.ProductName}");
                    }

                    productUnitConversionId = pus[0].ProductUnitConversionId;

                }
                else
                {
                    var puDefault = productInfo[0].StockInfo.UnitConversions.FirstOrDefault(u => u.IsDefault);
                    if (puDefault == null)
                    {
                        throw new BadRequestException(GeneralCode.InvalidParams, $"Dữ liệu đơn vị tính default lỗi, mặt hàng {item.ProductCode} {item.ProductName}");

                    }
                    productUnitConversionId = puDefault.ProductUnitConversionId;
                }

                yield return new PurchaseOrderInputDetail()
                {
                    OrderCode = item.OrderCode,
                    ProductionOrderCode = item.ProductionOrderCode,
                    Description = item.Description,
                    ProductId = productInfo[0].ProductId.Value,
                    ProviderProductName = item.ProductProviderName,


                    PrimaryQuantity = item.PrimaryQuantity,
                    PrimaryUnitPrice = item.PrimaryQuantity > 0 ? item.PrimaryMoney / item.PrimaryQuantity : 0,

                    ProductUnitConversionId = productUnitConversionId,
                    ProductUnitConversionQuantity = item.ProductUnitConversionQuantity,
                    ProductUnitConversionPrice = item.ProductUnitConversionQuantity > 0 ? item.ProductUnitConversionMoney / item.ProductUnitConversionQuantity : 0,

                    TaxInPercent = item.TaxInPercent,
                    TaxInMoney = item.TaxInMoney
                };

            }
        }

        private IEnumerable<PoDetailRowValue> SingleInvoiceParseExcel(SingleInvoicePoExcelMappingModel mapping, Stream stream)
        {
            var reader = new ExcelReader(stream);

            var data = reader.ReadSheets(mapping.SheetName, mapping.FromRow, mapping.ToRow, null).FirstOrDefault();


            for (var rowIndx = 0; rowIndx < data.Rows.Length; rowIndx++)
            {
                var row = data.Rows[rowIndx];

                var rowData = new PoDetailRowValue();

                if (!string.IsNullOrWhiteSpace(mapping.ColumnMapping.ProductCodeColumn))
                {
                    rowData.ProductCode = row[mapping.ColumnMapping.ProductCodeColumn]?.ToString();
                }

                if (!string.IsNullOrWhiteSpace(mapping.ColumnMapping.ProductNameColumn))
                {
                    rowData.ProductName = row[mapping.ColumnMapping.ProductNameColumn]?.ToString();
                    rowData.ProductInternalName = rowData.ProductName.NormalizeAsInternalName();
                }

                if (string.IsNullOrWhiteSpace(rowData.ProductCode) || string.IsNullOrWhiteSpace(rowData.ProductName)) continue;

                if (!string.IsNullOrWhiteSpace(mapping.ColumnMapping.ProductProviderNameColumn))
                {
                    rowData.ProductProviderName = row[mapping.ColumnMapping.ProductProviderNameColumn]?.ToString();
                }

                if (!string.IsNullOrWhiteSpace(mapping.ColumnMapping.PrimaryQuantityColumn)
                                   && row[mapping.ColumnMapping.PrimaryQuantityColumn] != null
                                   && !string.IsNullOrWhiteSpace(row[mapping.ColumnMapping.PrimaryQuantityColumn].ToString())
                                   )
                {
                    try
                    {
                        rowData.PrimaryQuantity = Convert.ToDecimal(row[mapping.ColumnMapping.PrimaryQuantityColumn]);

                    }
                    catch (Exception ex)
                    {
                        throw new BadRequestException(GeneralCode.InvalidParams, $"Số lượng ở mặt hàng {rowData.ProductCode} {rowData.ProductName} {ex.Message}");
                    }

                }


                if (!string.IsNullOrWhiteSpace(mapping.ColumnMapping.PrimaryQuantityMoneyColumn)
                    && row[mapping.ColumnMapping.PrimaryQuantityMoneyColumn] != null
                    && !string.IsNullOrWhiteSpace(row[mapping.ColumnMapping.PrimaryQuantityMoneyColumn].ToString())
                    )
                {
                    try
                    {
                        rowData.PrimaryMoney = Convert.ToDecimal(row[mapping.ColumnMapping.PrimaryQuantityMoneyColumn]);
                    }
                    catch (Exception ex)
                    {
                        throw new BadRequestException(GeneralCode.InvalidParams, $"Số tiền ở mặt hàng {rowData.ProductCode} {rowData.ProductName} {ex.Message}");
                    }

                }


                if (!string.IsNullOrWhiteSpace(mapping.StaticValue.ProductUnitConversionName))
                {
                    rowData.ProductUnitConversionName = mapping.StaticValue.ProductUnitConversionName;
                }

                if (!string.IsNullOrWhiteSpace(mapping.ColumnMapping.ProductUnitConversionNameColumn))
                {
                    rowData.ProductUnitConversionName = row[mapping.ColumnMapping.ProductUnitConversionNameColumn]?.ToString();
                }

                if (!string.IsNullOrWhiteSpace(rowData.ProductUnitConversionName)
                    && !string.IsNullOrWhiteSpace(mapping.ColumnMapping.ProductUnitConversionQuantityColumn)
                    && row[mapping.ColumnMapping.ProductUnitConversionQuantityColumn] != null
                    && !string.IsNullOrWhiteSpace(row[mapping.ColumnMapping.ProductUnitConversionQuantityColumn].ToString())
                    )
                {
                    try
                    {
                        rowData.ProductUnitConversionQuantity = Convert.ToDecimal(row[mapping.ColumnMapping.ProductUnitConversionQuantityColumn]);
                    }
                    catch (Exception ex)
                    {
                        throw new BadRequestException(GeneralCode.InvalidParams, $"Số lượng ĐVCĐ ở mặt hàng {rowData.ProductCode} {rowData.ProductName} {ex.Message}");
                    }
                }


                if (!string.IsNullOrWhiteSpace(mapping.ColumnMapping.ProductUnitConversionMoneyColumn)
                    && row[mapping.ColumnMapping.ProductUnitConversionMoneyColumn] != null
                    && !string.IsNullOrWhiteSpace(row[mapping.ColumnMapping.ProductUnitConversionMoneyColumn].ToString())
                    )
                {
                    try
                    {
                        rowData.ProductUnitConversionMoney = Convert.ToDecimal(row[mapping.ColumnMapping.ProductUnitConversionMoneyColumn]);
                    }
                    catch (Exception ex)
                    {
                        throw new BadRequestException(GeneralCode.InvalidParams, $"Số tiền ĐVCĐ ở mặt hàng {rowData.ProductCode} {rowData.ProductName} {ex.Message}");
                    }

                }


                if (!string.IsNullOrWhiteSpace(mapping.ColumnMapping.TaxInPercentColumn)
                  && row[mapping.ColumnMapping.TaxInPercentColumn] != null
                  && !string.IsNullOrWhiteSpace(row[mapping.ColumnMapping.TaxInPercentColumn].ToString())
                  )
                {
                    try
                    {
                        rowData.TaxInPercent = Convert.ToDecimal(row[mapping.ColumnMapping.TaxInPercentColumn]);
                    }
                    catch (Exception ex)
                    {
                        throw new BadRequestException(GeneralCode.InvalidParams, $"Thuế % ở mặt hàng {rowData.ProductCode} {rowData.ProductName} {ex.Message}");
                    }

                }


                if (!string.IsNullOrWhiteSpace(mapping.ColumnMapping.TaxInMoneyColumn)
                  && row[mapping.ColumnMapping.TaxInMoneyColumn] != null
                  && !string.IsNullOrWhiteSpace(row[mapping.ColumnMapping.TaxInMoneyColumn].ToString())
                  )
                {
                    try
                    {
                        rowData.TaxInMoney = Convert.ToDecimal(row[mapping.ColumnMapping.TaxInMoneyColumn]);
                    }
                    catch (Exception ex)
                    {
                        throw new BadRequestException(GeneralCode.InvalidParams, $"Tiền thuế ở mặt hàng {rowData.ProductCode} {rowData.ProductName} {ex.Message}");
                    }

                }

                if (!string.IsNullOrWhiteSpace(mapping.StaticValue.OrderCode))
                {
                    rowData.OrderCode = mapping.StaticValue.OrderCode;
                }
                if (!string.IsNullOrWhiteSpace(mapping.ColumnMapping.OrderCodeColumn))
                {
                    rowData.OrderCode = row[mapping.ColumnMapping.OrderCodeColumn]?.ToString();
                }

                if (!string.IsNullOrWhiteSpace(mapping.StaticValue.ProductionOrderCode))
                {
                    rowData.ProductionOrderCode = mapping.StaticValue.ProductionOrderCode;
                }
                if (!string.IsNullOrWhiteSpace(mapping.ColumnMapping.ProductionOrderCodeColumn))
                {
                    rowData.ProductionOrderCode = row[mapping.ColumnMapping.ProductionOrderCodeColumn]?.ToString();
                }

                if (!string.IsNullOrWhiteSpace(mapping.ColumnMapping.DescriptionColumn))
                {
                    rowData.Description = row[mapping.ColumnMapping.DescriptionColumn]?.ToString();
                }

                yield return rowData;
            }
        }


        public async Task<bool> Checked(long purchaseOrderId)
        {
            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var info = await _purchaseOrderDBContext.PurchaseOrder.FirstOrDefaultAsync(d => d.PurchaseOrderId == purchaseOrderId);
                if (info == null) throw new BadRequestException(PurchaseOrderErrorCode.PoNotFound);

                if (info.PurchaseOrderStatusId == (int)EnumPurchaseOrderStatus.Checked && info.IsChecked == true)
                {
                    throw new BadRequestException(GeneralCode.InvalidParams, "PO đã được kiểm tra");
                }

                if (info.PurchaseOrderStatusId != (int)EnumPurchaseOrderStatus.WaitToCensor
                    && info.PurchaseOrderStatusId != (int)EnumPurchaseOrderStatus.Checked)
                {
                    throw new BadRequestException(GeneralCode.InvalidParams, "PO chưa được gửi để duyệt");
                }

                info.IsChecked = true;

                info.PurchaseOrderStatusId = (int)EnumPurchaseOrderStatus.Checked;
                info.CheckedDatetimeUtc = DateTime.UtcNow;
                info.CheckedByUserId = _currentContext.UserId;

                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.PurchaseOrder, purchaseOrderId, $"Đã kiểm tra PO {info.PurchaseOrderCode}", info.JsonSerialize());

                return true;
            }
        }

        public async Task<bool> RejectCheck(long purchaseOrderId)
        {
            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var info = await _purchaseOrderDBContext.PurchaseOrder.FirstOrDefaultAsync(d => d.PurchaseOrderId == purchaseOrderId);
                if (info == null) throw new BadRequestException(PurchaseOrderErrorCode.PoNotFound);

                if (info.PurchaseOrderStatusId == (int)EnumPurchaseOrderStatus.Checked && info.IsChecked == false)
                {
                    throw new BadRequestException(GeneralCode.InvalidParams, "PO đã kiểm tra từ chối");
                }

                if (info.PurchaseOrderStatusId != (int)EnumPurchaseOrderStatus.WaitToCensor
                    && info.PurchaseOrderStatusId != (int)EnumPurchaseOrderStatus.Checked)
                {
                    throw new BadRequestException(GeneralCode.InvalidParams, "PO chưa được gửi để duyệt");
                }

                info.IsChecked = false;

                info.PurchaseOrderStatusId = (int)EnumPurchaseOrderStatus.Checked;
                info.CheckedDatetimeUtc = DateTime.UtcNow;
                info.CheckedByUserId = _currentContext.UserId;

                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.PurchasingSuggest, purchaseOrderId, $"Kiểm tra từ chối  PO {info.PurchaseOrderCode}", info.JsonSerialize());

                return true;
            }
        }

        public async Task<bool> Approve(long purchaseOrderId)
        {
            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var info = await _purchaseOrderDBContext.PurchaseOrder.FirstOrDefaultAsync(d => d.PurchaseOrderId == purchaseOrderId);
                if (info == null) throw new BadRequestException(PurchaseOrderErrorCode.PoNotFound);

                if (info.PurchaseOrderStatusId != (int)EnumPurchaseOrderStatus.Censored && info.IsApproved == true)
                {
                    throw new BadRequestException(GeneralCode.InvalidParams, "PO đã được duyệt");
                }

                if (info.IsChecked != true)
                {
                    throw new BadRequestException(GeneralCode.InvalidParams, "PO chưa được qua kiểm tra kiểm soát");
                }

                if (info.PurchaseOrderStatusId != (int)EnumPurchaseOrderStatus.Censored
                    && info.PurchaseOrderStatusId != (int)EnumPurchaseOrderStatus.Checked)
                {
                    throw new BadRequestException(GeneralCode.InvalidParams, "PO chưa được gửi để duyệt");
                }

                info.IsApproved = true;

                info.PurchaseOrderStatusId = (int)EnumPurchaseOrderStatus.Censored;
                info.CensorDatetimeUtc = DateTime.UtcNow;
                info.CensorByUserId = _currentContext.UserId;

                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.PurchaseOrder, purchaseOrderId, $"Duyệt PO {info.PurchaseOrderCode}", info.JsonSerialize());

                return true;
            }
        }

        public async Task<bool> Reject(long purchaseOrderId)
        {
            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var info = await _purchaseOrderDBContext.PurchaseOrder.FirstOrDefaultAsync(d => d.PurchaseOrderId == purchaseOrderId);
                if (info == null) throw new BadRequestException(PurchaseOrderErrorCode.PoNotFound);

                if (info.PurchaseOrderStatusId != (int)EnumPurchaseOrderStatus.Censored && info.IsApproved == false)
                {
                    throw new BadRequestException(GeneralCode.InvalidParams, "PO đã từ chối");
                }

                if (info.IsChecked != true)
                {
                    throw new BadRequestException(GeneralCode.InvalidParams, "PO chưa được qua kiểm tra kiểm soát");
                }

                if (info.PurchaseOrderStatusId != (int)EnumPurchaseOrderStatus.Censored
                   && info.PurchaseOrderStatusId != (int)EnumPurchaseOrderStatus.Checked)
                {
                    throw new BadRequestException(GeneralCode.InvalidParams, "PO chưa được gửi để duyệt");
                }


                info.IsApproved = false;

                info.PurchaseOrderStatusId = (int)EnumPurchaseOrderStatus.Censored;
                info.CensorDatetimeUtc = DateTime.UtcNow;
                info.CensorByUserId = _currentContext.UserId;

                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.PurchasingSuggest, purchaseOrderId, $"Từ chối PO {info.PurchaseOrderCode}", info.JsonSerialize());

                return true;
            }
        }

        public async Task<bool> SentToCensor(long purchaseOrderId)
        {
            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var info = await _purchaseOrderDBContext.PurchaseOrder.FirstOrDefaultAsync(d => d.PurchaseOrderId == purchaseOrderId);
                if (info == null) throw new BadRequestException(PurchaseOrderErrorCode.PoNotFound);

                if (info.PurchaseOrderStatusId != (int)EnumPurchaseOrderStatus.Draff)
                {
                    throw new BadRequestException(GeneralCode.InvalidParams);
                }

                info.PurchaseOrderStatusId = (int)EnumPurchaseOrderStatus.WaitToCensor;
                info.UpdatedDatetimeUtc = DateTime.UtcNow;
                info.UpdatedByUserId = _currentContext.UserId;


                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.PurchaseOrder, purchaseOrderId, $"Gửi duyệt PO {info.PurchaseOrderCode}", info.JsonSerialize());

                return true;
            }
        }


        public async Task<bool> UpdatePoProcessStatus(long purchaseOrderId, EnumPoProcessStatus poProcessStatusId)
        {
            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var info = await _purchaseOrderDBContext.PurchaseOrder.FirstOrDefaultAsync(d => d.PurchaseOrderId == purchaseOrderId);
                if (info == null) throw new BadRequestException(PurchaseOrderErrorCode.PoNotFound);

                info.PoProcessStatusId = (int)poProcessStatusId;

                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.PurchaseOrder, purchaseOrderId, $"Cập nhật tiến trình PO {info.PurchaseOrderCode}", info.JsonSerialize());

                return true;
            }
        }

        public async Task<IDictionary<long, IList<PurchaseOrderOutputBasic>>> GetPurchaseOrderBySuggest(IList<long> purchasingSuggestIds)
        {
            var poDetail = await (
                from s in _purchaseOrderDBContext.PurchaseOrder
                join sd in _purchaseOrderDBContext.PurchaseOrderDetail on s.PurchaseOrderId equals sd.PurchaseOrderId
                join r in _purchaseOrderDBContext.PurchasingSuggestDetail on sd.PurchasingSuggestDetailId equals r.PurchasingSuggestDetailId
                where purchasingSuggestIds.Contains(r.PurchasingSuggestId)
                select new
                {
                    r.PurchasingSuggestId,
                    s.PurchaseOrderId,
                    s.PurchaseOrderCode
                }).ToListAsync();

            return purchasingSuggestIds.Distinct()
                .ToDictionary(
                r => r,
                r => (IList<PurchaseOrderOutputBasic>)poDetail.Where(d => d.PurchasingSuggestId == r).Select(d => new PurchaseOrderOutputBasic
                {
                    PurchaseOrderId = d.PurchaseOrderId,
                    PurchaseOrderCode = d.PurchaseOrderCode
                })
                    .Distinct()
                    .ToList()
                );

        }

        public async Task<IDictionary<long, IList<PurchaseOrderOutputBasic>>> GetPurchaseOrderByAssignment(IList<long> poAssignmentIds)
        {
            var poDetail = await (
                from s in _purchaseOrderDBContext.PurchaseOrder
                join sd in _purchaseOrderDBContext.PurchaseOrderDetail on s.PurchaseOrderId equals sd.PurchaseOrderId
                join r in _purchaseOrderDBContext.PoAssignmentDetail on sd.PoAssignmentDetailId equals r.PoAssignmentDetailId
                where poAssignmentIds.Contains(r.PoAssignmentId)
                select new
                {
                    r.PoAssignmentId,
                    s.PurchaseOrderId,
                    s.PurchaseOrderCode
                }).ToListAsync();

            return poAssignmentIds.Distinct()
                .ToDictionary(
                r => r,
                r => (IList<PurchaseOrderOutputBasic>)poDetail.Where(d => d.PoAssignmentId == r).Select(d => new PurchaseOrderOutputBasic
                {
                    PurchaseOrderId = d.PurchaseOrderId,
                    PurchaseOrderCode = d.PurchaseOrderCode
                })
                    .Distinct()
                    .ToList()
                );
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

            if (model.Details.Where(d => d.PoAssignmentDetailId.HasValue).GroupBy(d => d.PoAssignmentDetailId).Any(d => d.Count() > 1))
            {
                return GeneralCode.InvalidParams;
            }

            if (model.Details.Where(d => d.PurchasingSuggestDetailId.HasValue).GroupBy(d => d.PurchasingSuggestDetailId).Any(d => d.Count() > 1))
            {
                return GeneralCode.InvalidParams;
            }

            var validateAssignment = await ValidateAssignmentDetails(poId, model);

            if (!validateAssignment.IsSuccess()) return validateAssignment;

            var validateSuggest = await ValidateSuggestDetails(poId, model);

            if (!validateSuggest.IsSuccess()) return validateSuggest;

            return GeneralCode.Success;
        }

        private async Task<Enum> ValidateAssignmentDetails(long? poId, PurchaseOrderInput model)
        {
            if (model.Details.Where(d => d.PoAssignmentDetailId.HasValue).GroupBy(d => d.PoAssignmentDetailId).Any(d => d.Count() > 1))
            {
                return GeneralCode.InvalidParams;
            }

            var poAssignmentDetailIds = model.Details.Where(d => d.PoAssignmentDetailId.HasValue).Select(d => d.PoAssignmentDetailId).ToList();

            var poAssignmentDetails = await GetPoAssignmentDetailInfos(poAssignmentDetailIds.Select(a => a.Value).ToList());

            if (poAssignmentDetails.Select(d => d.PoAssignmentId).Distinct().Count() > 1)
            {
                return GeneralCode.InvalidParams;
            }

            if ((from d in poAssignmentDetails
                 join m in model.Details on d.PoAssignmentDetailId equals m.PoAssignmentDetailId
                 where d.ProductId != m.ProductId
                 select 0
            ).Any())
            {
                return GeneralCode.InvalidParams;
            }

            if ((from d in poAssignmentDetails
                 join m in model.Details on d.PoAssignmentDetailId equals m.PoAssignmentDetailId
                 where m.PurchasingSuggestDetailId.HasValue && d.PurchasingSuggestDetailId != m.PurchasingSuggestDetailId
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


            //var sameAssignmentDetails = await(
            //    from d in _purchaseOrderDBContext.PurchaseOrderDetail
            //    where poAssignmentDetailIds.Contains(d.PoAssignmentDetailId)
            //    select d
            //    ).AsNoTracking()
            //    .ToListAsync();

            //if (sameAssignmentDetails.Any(d => d.PurchaseOrderId != poId))
            //{
            //    return PurchaseOrderErrorCode.AssignmentDetailAlreadyCreatedPo;
            //}

            return GeneralCode.Success;
        }

        private async Task<Enum> ValidateSuggestDetails(long? poId, PurchaseOrderInput model)
        {
            if (model.Details.Where(d => d.PurchasingSuggestDetailId.HasValue).GroupBy(d => d.PurchasingSuggestDetailId).Any(d => d.Count() > 1))
            {
                return GeneralCode.InvalidParams;
            }

            var suggestDetailIds = model.Details.Where(d => d.PurchasingSuggestDetailId.HasValue).Select(d => d.PurchasingSuggestDetailId).ToList();

            var suggestDetails = await GetSuggestDetailInfos(suggestDetailIds.Select(a => a.Value).ToList());

            if (suggestDetails.Select(d => d.PurchasingSuggestId).Distinct().Count() > 1)
            {
                return GeneralCode.InvalidParams;
            }

            if ((from d in suggestDetails
                 join m in model.Details on d.PurchasingSuggestDetailId equals m.PurchasingSuggestDetailId
                 where d.ProductId != m.ProductId
                 select 0)
                 .Any()
            )
            {
                return GeneralCode.InvalidParams;
            }


            foreach (var suggestDetailId in suggestDetailIds)
            {
                if (!suggestDetails.Any(d => d.PurchasingSuggestDetailId == suggestDetailId))
                {
                    return PurchasingSuggestErrorCode.PoAssignmentNotfound;
                }
            }

            if (suggestDetails.Select(d => d.CustomerId).Distinct().Count() > 1)
            {
                return PurchaseOrderErrorCode.OnlyCreatePOFromOneCustomer;
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
                 PurchasingSuggestId = sd.PurchasingSuggestId,
                 PurchasingSuggestDetailId = sd.PurchasingSuggestDetailId,

                 CustomerId = sd.CustomerId,

                 PoAssignmentId = pd.PoAssignmentId,
                 PoAssignmentDetailId = pd.PoAssignmentDetailId,

                 ProductId = sd.ProductId,

                 PrimaryQuantity = pd.PrimaryQuantity,
                 PrimaryUnitPrice = pd.PrimaryUnitPrice,



                 TaxInPercent = pd.TaxInPercent,
                 TaxInMoney = pd.TaxInMoney
             }).AsNoTracking()
             .ToListAsync();
        }

        private async Task<IList<PurchasingSuggestDetailInfo>> GetSuggestDetailInfos(IList<long> suggestDetailIds)
        {
            return await (
             from pd in _purchaseOrderDBContext.PurchasingSuggestDetail
             join sd in _purchaseOrderDBContext.PurchasingSuggest on pd.PurchasingSuggestId equals sd.PurchasingSuggestId
             where suggestDetailIds.Contains(pd.PurchasingSuggestDetailId)
             select new PurchasingSuggestDetailInfo
             {
                 PurchasingSuggestId = pd.PurchasingSuggestId,
                 PurchasingSuggestDetailId = pd.PurchasingSuggestDetailId,
                 ProductId = pd.ProductId,
                 CustomerId = pd.CustomerId
             }).AsNoTracking()
             .ToListAsync();
        }



        private class PoAssignmentDetailInfo
        {
            public long PurchasingSuggestId { get; set; }

            public long PurchasingSuggestDetailId { get; set; }

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

        private class PurchasingSuggestDetailInfo
        {
            public long PurchasingSuggestId { get; set; }
            public long PurchasingSuggestDetailId { get; set; }
            public int ProductId { get; set; }
            public int CustomerId { get; set; }
        }
    }
}
