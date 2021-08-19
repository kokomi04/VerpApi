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
        private readonly ICustomGenCodeHelperService _customGenCodeHelperService;

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
           , ICustomGenCodeHelperService customGenCodeHelperService
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
            _customGenCodeHelperService = customGenCodeHelperService;
        }

        public async Task<PageData<PurchaseOrderOutputList>> GetList(string keyword, IList<int> purchaseOrderTypes, IList<int> productIds, EnumPurchaseOrderStatus? purchaseOrderStatusId, EnumPoProcessStatus? poProcessStatusId, bool? isChecked, bool? isApproved, long? fromDate, long? toDate, string sortBy, bool asc, int page, int size)
        {
            keyword = keyword?.Trim();

            var query = from po in _purchaseOrderDBContext.PurchaseOrder
                        join d in _purchaseOrderDBContext.PurchaseOrderDetail on po.PurchaseOrderId equals d.PurchaseOrderId
                        join p in _purchaseOrderDBContext.RefProduct on d.ProductId equals p.ProductId into ps
                        from p in ps.DefaultIfEmpty()
                        join c in _purchaseOrderDBContext.RefCustomer on po.CustomerId equals c.CustomerId into cs
                        from c in cs.DefaultIfEmpty()
                        join creator in _purchaseOrderDBContext.RefEmployee on po.CreatedByUserId equals creator.UserId into gCreator
                        from creator in gCreator.DefaultIfEmpty()
                        join checker in _purchaseOrderDBContext.RefEmployee on po.CheckedByUserId equals checker.UserId into gChecker
                        from checker in gChecker.DefaultIfEmpty()
                        join censor in _purchaseOrderDBContext.RefEmployee on po.CensorByUserId equals censor.UserId into gCensor
                        from censor in gCensor.DefaultIfEmpty()
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
                            po.PoDescription,
                            po.CreatedByUserId,
                            po.UpdatedByUserId,
                            po.CheckedByUserId,
                            po.CensorByUserId,

                            po.CreatedDatetimeUtc,
                            po.UpdatedDatetimeUtc,
                            po.CheckedDatetimeUtc,
                            po.CensorDatetimeUtc,

                            c.CustomerCode,
                            c.CustomerName,

                            p.ProductId,
                            p.ProductCode,
                            p.ProductName,
                            po.DeliveryDate,

                            CreatorFullName = creator.FullName,
                            CheckerFullName = checker.FullName,
                            CensorFullName = censor.FullName,
                            po.PurchaseOrderType,
                        };
            if (!string.IsNullOrWhiteSpace(keyword))
            {

                query = query
                   .Where(q => q.PurchaseOrderCode.Contains(keyword)
                    || q.Content.Contains(keyword)
                    || q.AdditionNote.Contains(keyword)
                    || q.CustomerCode.Contains(keyword)
                    || q.CustomerName.Contains(keyword)
                    || q.ProductCode.Contains(keyword)
                    || q.ProductName.Contains(keyword)
                    || q.CreatorFullName.Contains(keyword)
                    || q.CheckerFullName.Contains(keyword)
                    || q.CensorFullName.Contains(keyword)
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


            if (productIds != null && productIds.Count > 0)
            {
                query = query.Where(p => productIds.Contains(p.ProductId));
            }

            if (purchaseOrderTypes != null && purchaseOrderTypes.Count > 0)
            {
                query = query.Where(p => purchaseOrderTypes.Contains(p.PurchaseOrderType));
            }

            var poQuery = _purchaseOrderDBContext.PurchaseOrder.Where(po => query.Select(p => p.PurchaseOrderId).Contains(po.PurchaseOrderId));

            var total = await poQuery.CountAsync();
            var additionResult = await (from q in poQuery
                                        group q by 1 into g
                                        select new
                                        {
                                            SumTotalMoney = g.Sum(x => x.TotalMoney),
                                        }).FirstOrDefaultAsync();
            var pagedData = await poQuery.SortByFieldName(sortBy, asc).Skip((page - 1) * size).Take(size).ToListAsync();
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

                    PoDescription = info.PoDescription,

                    CreatedByUserId = info.CreatedByUserId,
                    UpdatedByUserId = info.UpdatedByUserId,
                    CheckedByUserId = info.CheckedByUserId,
                    CensorByUserId = info.CensorByUserId,

                    CreatedDatetimeUtc = info.CreatedDatetimeUtc.GetUnix(),
                    UpdatedDatetimeUtc = info.UpdatedDatetimeUtc.GetUnix(),
                    CheckedDatetimeUtc = info.CheckedDatetimeUtc.GetUnix(),
                    CensorDatetimeUtc = info.CensorDatetimeUtc.GetUnix(),
                    DeliveryDate = info.DeliveryDate.GetUnix(),
                    PurchaseOrderType = info.PurchaseOrderType
                });
            }

            return (result, total, additionResult);
        }

        public async Task<PageData<PurchaseOrderOutputListByProduct>> GetListByProduct(string keyword, IList<int> purchaseOrderTypes, IList<int> productIds, EnumPurchaseOrderStatus? purchaseOrderStatusId, EnumPoProcessStatus? poProcessStatusId, bool? isChecked, bool? isApproved, long? fromDate, long? toDate, string sortBy, bool asc, int page, int size)
        {
            keyword = (keyword ?? "").Trim();

            var query = from po in _purchaseOrderDBContext.PurchaseOrder
                        join pod in _purchaseOrderDBContext.PurchaseOrderDetail on po.PurchaseOrderId equals pod.PurchaseOrderId
                        join p in _purchaseOrderDBContext.RefProduct on pod.ProductId equals p.ProductId into ps
                        from p in ps.DefaultIfEmpty()
                        join c in _purchaseOrderDBContext.RefCustomer on po.CustomerId equals c.CustomerId into cs
                        from c in cs.DefaultIfEmpty()
                        join ad in _purchaseOrderDBContext.PoAssignmentDetail on pod.PoAssignmentDetailId equals ad.PoAssignmentDetailId into ads
                        from ad in ads.DefaultIfEmpty()
                        join a in _purchaseOrderDBContext.PoAssignment on ad.PoAssignmentId equals a.PoAssignmentId into aa
                        from a in aa.DefaultIfEmpty()
                        join sd in _purchaseOrderDBContext.PurchasingSuggestDetail on pod.PurchasingSuggestDetailId equals sd.PurchasingSuggestDetailId into sds
                        from sd in sds.DefaultIfEmpty()
                        join s in _purchaseOrderDBContext.PurchasingSuggest on sd.PurchasingSuggestId equals s.PurchasingSuggestId into ss
                        from s in ss.DefaultIfEmpty()
                        join creator in _purchaseOrderDBContext.RefEmployee on po.CreatedByUserId equals creator.UserId into gCreator
                        from creator in gCreator.DefaultIfEmpty()
                        join checker in _purchaseOrderDBContext.RefEmployee on po.CheckedByUserId equals checker.UserId into gChecker
                        from checker in gChecker.DefaultIfEmpty()
                        join censor in _purchaseOrderDBContext.RefEmployee on po.CensorByUserId equals censor.UserId into gCensor
                        from censor in gCensor.DefaultIfEmpty()
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
                            po.PoDescription,

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


                            c.CustomerCode,
                            c.CustomerName,

                            p.ProductCode,
                            p.ProductName,

                            PoAssignmentId = a == null ? (long?)null : a.PoAssignmentId,
                            PoAssignmentCode = a == null ? null : a.PoAssignmentCode,

                            PurchasingSuggestId = s == null ? (long?)null : s.PurchasingSuggestId,
                            PurchasingSuggestCode = s == null ? null : s.PurchasingSuggestCode,
                            po.DeliveryDate,

                            CreatorFullName = creator.FullName,
                            CheckerFullName = checker.FullName,
                            CensorFullName = censor.FullName,
                            po.PurchaseOrderType,
                            pod.IntoMoney,
                            pod.IntoAfterTaxMoney,
                        };

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query
                    .Where(q => q.PurchaseOrderCode.Contains(keyword)
                    || q.Content.Contains(keyword)
                    || q.AdditionNote.Contains(keyword)
                    || q.CustomerCode.Contains(keyword)
                    || q.CustomerName.Contains(keyword)
                    || q.ProductCode.Contains(keyword)
                    || q.ProductName.Contains(keyword)
                    || q.PoAssignmentCode.Contains(keyword)
                    || q.PurchasingSuggestCode.Contains(keyword)
                    || q.OrderCode.Contains(keyword)
                    || q.ProductionOrderCode.Contains(keyword)
                    || q.CreatorFullName.Contains(keyword)
                    || q.CheckerFullName.Contains(keyword)
                    || q.CensorFullName.Contains(keyword)
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

            if (purchaseOrderTypes != null && purchaseOrderTypes.Count > 0)
            {
                query = query.Where(p => purchaseOrderTypes.Contains(p.PurchaseOrderType));
            }

            var total = await query.CountAsync();
            var pagedData = await query.SortByFieldName(sortBy, asc).Skip((page - 1) * size).Take(size).ToListAsync();
            var additionResult = await (from q in query
                                        group q by 1 into g
                                        select new
                                        {
                                            SumPrimaryQuantity = g.Sum(x => x.PrimaryQuantity),
                                            SumTaxInMoney = g.Sum(x => x.TaxInMoney)
                                        }).FirstOrDefaultAsync();

            var sumTotalMoney = (await (from q in query
                                        group q by q.PurchaseOrderCode into g
                                        select new
                                        {
                                            TotalMoney = g.Sum(x => x.TotalMoney) / g.Count()
                                        }).ToListAsync()).Sum(x => x.TotalMoney);

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
                    PurchasingSuggestDetail = purchasingSuggestDetailInfo,

                    DeliveryDate = info.DeliveryDate.GetUnix(),
                    CreatorFullName = info.CreatorFullName,
                    CheckerFullName = info.CheckerFullName,
                    CensorFullName = info.CensorFullName,
                    PurchaseOrderType = info.PurchaseOrderType,
                    IntoMoney = info.IntoMoney,
                    IntoAfterTaxMoney = info.IntoAfterTaxMoney,
                });
            }
            return (result, total, new { SumTotalMoney = sumTotalMoney, additionResult.SumPrimaryQuantity, additionResult.SumTaxInMoney });
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

            var files = await _purchaseOrderDBContext.PurchaseOrderFile.AsNoTracking().Where(d => d.PurchaseOrderId == purchaseOrderId).ToListAsync();

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
                PoDescription = info.PoDescription,
                CreatedByUserId = info.CreatedByUserId,
                UpdatedByUserId = info.UpdatedByUserId,
                CheckedByUserId = info.CheckedByUserId,
                CensorByUserId = info.CensorByUserId,

                CreatedDatetimeUtc = info.CreatedDatetimeUtc.GetUnix(),
                UpdatedDatetimeUtc = info.UpdatedDatetimeUtc.GetUnix(),
                CheckedDatetimeUtc = info.CheckedDatetimeUtc.GetUnix(),
                CensorDatetimeUtc = info.CensorDatetimeUtc.GetUnix(),

                PurchaseOrderType = info.PurchaseOrderType,

                FileIds = files.Select(f => f.FileId).ToList(),
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
                        IntoMoney = d.IntoMoney,
                        IntoAfterTaxMoney = d.IntoAfterTaxMoney
                    };
                }).ToList()
            };
        }

        public async Task<long> Create(PurchaseOrderInput model)
        {
            var validate = await ValidatePoModelInput(null, model);

            if (!validate.IsSuccess())
            {
                throw new BadRequestException(validate);
            }

            var ctx = await GeneratePurchaseOrderCode(null, model);

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
                    PoDescription = model.PoDescription,
                    IsApproved = null,
                    IsChecked = null,
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
                    PurchaseOrderType = (int)EnumPurchasingOrderType.Default
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
                        DeletedDatetimeUtc = null,
                        IntoMoney = Math.Round(d.PrimaryQuantity * d.PrimaryUnitPrice),
                        IntoAfterTaxMoney = Math.Round(d.PrimaryQuantity * d.PrimaryUnitPrice) + Math.Round((decimal)((d.PrimaryQuantity * d.PrimaryUnitPrice) * (d.TaxInPercent / 100))),
                    };
                }).ToList();


                await _purchaseOrderDBContext.PurchaseOrderDetail.AddRangeAsync(poDetails);


                if (model.FileIds?.Count > 0)
                {
                    await _purchaseOrderDBContext.PurchaseOrderFile.AddRangeAsync(model.FileIds.Select(f => new PurchaseOrderFile()
                    {
                        PurchaseOrderId = po.PurchaseOrderId,
                        FileId = f,
                        CreatedDatetimeUtc = DateTime.UtcNow,
                        DeletedDatetimeUtc = null,
                        IsDeleted = false
                    }));
                }


                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.PurchaseOrder, po.PurchaseOrderId, $"Tạo PO {po.PurchaseOrderCode}", model.JsonSerialize());

                await ctx.ConfirmCode();

                return po.PurchaseOrderId;
            }

        }

        private async Task<GenerateCodeContext> GeneratePurchaseOrderCode(long? purchaseOrderId, PurchaseOrderInput model)
        {
            model.PurchaseOrderCode = (model.PurchaseOrderCode ?? "").Trim();

            var ctx = _customGenCodeHelperService.CreateGenerateCodeContext();

            var code = await ctx
                .SetConfig(EnumObjectType.PurchaseOrder)
                .SetConfigData(purchaseOrderId ?? 0, model.Date)
                .TryValidateAndGenerateCode(_purchaseOrderDBContext.PurchaseOrder, model.PurchaseOrderCode, (s, code) => s.PurchaseOrderId != purchaseOrderId && s.PurchaseOrderCode == code);

            model.PurchaseOrderCode = code;

            return ctx;
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
                info.IsChecked = null;
                info.IsApproved = null;
                info.PoProcessStatusId = null;
                info.DeliveryFee = model.DeliveryFee;
                info.OtherFee = model.OtherFee;
                info.TotalMoney = model.TotalMoney;
                info.PoDescription = model.PoDescription;

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
                            detail.ProductId = item.ProductId;
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
                            detail.IntoMoney = Math.Round(detail.PrimaryQuantity * detail.PrimaryUnitPrice);
                            detail.IntoAfterTaxMoney = detail.IntoMoney + Math.Round((decimal)(detail.IntoMoney * (detail.TaxInPercent / 100)));
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
                            ProductId = item.ProductId,
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
                            DeletedDatetimeUtc = null,
                            IntoMoney = Math.Round(item.PrimaryQuantity * item.PrimaryUnitPrice),
                            IntoAfterTaxMoney = Math.Round(item.PrimaryQuantity * item.PrimaryUnitPrice) + Math.Round((decimal)((item.PrimaryQuantity * item.PrimaryUnitPrice) * (item.TaxInPercent / 100))),
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

                var oldFiles = await _purchaseOrderDBContext.PurchaseOrderFile.Where(f => f.PurchaseOrderId == info.PurchaseOrderId).ToListAsync();

                if (oldFiles.Count > 0)
                {
                    _purchaseOrderDBContext.PurchaseOrderFile.RemoveRange(oldFiles);
                }

                if (model.FileIds?.Count > 0)
                {
                    await _purchaseOrderDBContext.PurchaseOrderFile.AddRangeAsync(model.FileIds.Select(f => new PurchaseOrderFile()
                    {
                        PurchaseOrderId = info.PurchaseOrderId,
                        FileId = f,
                        CreatedDatetimeUtc = DateTime.UtcNow,
                        DeletedDatetimeUtc = null,
                        IsDeleted = false
                    }));
                }

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


        public async IAsyncEnumerable<PurchaseOrderExcelParseDetail> ParseInvoiceDetails(SingleInvoicePoExcelMappingModel mapping, Stream stream)
        {
            var rowDatas = SingleInvoiceParseExcel(mapping, stream).ToList();

            var productCodes = rowDatas.Select(r => r.ProductCode).ToList();
            var productInternalNames = rowDatas.Select(r => r.ProductInternalName).ToList();

            var productInfos = await _productHelperService.GetListByCodeAndInternalNames(productCodes, productInternalNames);

            var productInfoByCode = productInfos.GroupBy(p => p.ProductCode)
                .ToDictionary(p => p.Key.Trim().ToLower(), p => p.ToList());

            var productInfoByInternalName = productInfos.GroupBy(p => p.ProductName.NormalizeAsInternalName())
                .ToDictionary(p => p.Key.Trim().ToLower(), p => p.ToList());

            foreach (var item in rowDatas)
            {
                IList<ProductModel> productInfo = null;
                if (!string.IsNullOrWhiteSpace(item.ProductCode) && productInfoByCode.ContainsKey(item.ProductCode?.ToLower()))
                {
                    productInfo = productInfoByCode[item.ProductCode?.ToLower()];
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(item.ProductInternalName) && productInfoByInternalName.ContainsKey(item.ProductInternalName))
                    {
                        productInfo = productInfoByInternalName[item.ProductInternalName];
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
                if (!string.IsNullOrWhiteSpace(item.ProductUnitConversionName))
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

                yield return new PurchaseOrderExcelParseDetail()
                {
                    OrderCode = item.OrderCode,
                    ProductionOrderCode = item.ProductionOrderCode,
                    Description = item.Description,
                    ProductId = productInfo[0].ProductId.Value,
                    ProviderProductName = item.ProductProviderName,


                    PrimaryQuantity = item.PrimaryQuantity ?? 0,
                    PrimaryUnitPrice = item.PrimaryPrice ?? 0,

                    ProductUnitConversionId = productUnitConversionId,
                    ProductUnitConversionQuantity = item.ProductUnitConversionQuantity ?? 0,
                    ProductUnitConversionPrice = item.ProductUnitConversionPrice ?? 0,

                    Money = item.Money ?? 0,

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
                if (row.Count == 0) continue;

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

                if (string.IsNullOrWhiteSpace(rowData.ProductCode) && string.IsNullOrWhiteSpace(rowData.ProductName)) continue;

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

                if (!string.IsNullOrWhiteSpace(mapping.ColumnMapping.PrimaryPriceColumn)
                                   && row[mapping.ColumnMapping.PrimaryPriceColumn] != null
                                   && !string.IsNullOrWhiteSpace(row[mapping.ColumnMapping.PrimaryPriceColumn].ToString())
                )
                {
                    try
                    {
                        rowData.PrimaryPrice = Convert.ToDecimal(row[mapping.ColumnMapping.PrimaryPriceColumn]);

                    }
                    catch (Exception ex)
                    {
                        throw new BadRequestException(GeneralCode.InvalidParams, $"Đơn giá ở mặt hàng {rowData.ProductCode} {rowData.ProductName} {ex.Message}");
                    }
                }


                if (!string.IsNullOrWhiteSpace(mapping.ColumnMapping.MoneyColumn)
                    && row[mapping.ColumnMapping.MoneyColumn] != null
                    && !string.IsNullOrWhiteSpace(row[mapping.ColumnMapping.MoneyColumn].ToString())
                    )
                {
                    try
                    {
                        rowData.Money = Convert.ToDecimal(row[mapping.ColumnMapping.MoneyColumn]);
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

                if (!string.IsNullOrWhiteSpace(rowData.ProductUnitConversionName)
                    && !string.IsNullOrWhiteSpace(mapping.ColumnMapping.ProductUnitConversionPriceColumn)
                    && row[mapping.ColumnMapping.ProductUnitConversionPriceColumn] != null
                    && !string.IsNullOrWhiteSpace(row[mapping.ColumnMapping.ProductUnitConversionPriceColumn].ToString())
                    )
                {
                    try
                    {
                        rowData.ProductUnitConversionPrice = Convert.ToDecimal(row[mapping.ColumnMapping.ProductUnitConversionPriceColumn]);
                    }
                    catch (Exception ex)
                    {
                        throw new BadRequestException(GeneralCode.InvalidParams, $"Đơn giá ĐVCĐ ở mặt hàng {rowData.ProductCode} {rowData.ProductName} {ex.Message}");
                    }
                }

                //if (rowData.ProductUnitConversionQuantity == 0)
                //{
                //    rowData.ProductUnitConversionName = null;
                //}


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
                info.CheckedDatetimeUtc = DateTime.Now.Date.GetUnixUtc(_currentContext.TimeZoneOffset).UnixToDateTime();
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
                info.CheckedDatetimeUtc = DateTime.Now.Date.GetUnixUtc(_currentContext.TimeZoneOffset).UnixToDateTime();
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
                info.CensorDatetimeUtc = DateTime.Now.Date.GetUnixUtc(_currentContext.TimeZoneOffset).UnixToDateTime();
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
                info.CensorDatetimeUtc = DateTime.Now.Date.GetUnixUtc(_currentContext.TimeZoneOffset).UnixToDateTime();
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

                info.IsChecked = null;
                info.IsApproved = null;
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
            //else
            //{
            //    return PurchaseOrderErrorCode.PoCodeAlreadyExisted;
            //}


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
