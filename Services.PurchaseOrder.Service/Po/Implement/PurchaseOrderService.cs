using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Verp.Resources.PurchaseOrder.Po;
using VErp.Commons.Enums.ErrorCodes;
using VErp.Commons.Enums.ErrorCodes.PO;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.MasterEnum.PO;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.PurchaseOrderDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.PurchaseOrder.Model;
using VErp.Services.PurchaseOrder.Model.PurchaseOrder;
using VErp.Services.PurchaseOrder.Model.Request;
using VErp.Services.PurchaseOrder.Service.Po.Implement.Facade;
using PurchaseOrderModel = VErp.Infrastructure.EF.PurchaseOrderDB.PurchaseOrder;
using static Verp.Resources.PurchaseOrder.Po.PurchaseOrderOutsourceValidationMessage;
using AutoMapper.QueryableExtensions;
using AutoMapper;
using VErp.Commons.GlobalObject.InternalDataInterface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using VErp.Infrastructure.ServiceCore.SignalR;

namespace VErp.Services.PurchaseOrder.Service.Implement
{
    public class PurchaseOrderService : IPurchaseOrderService
    {
        private readonly PurchaseOrderDBContext _purchaseOrderDBContext;
        private readonly ICurrentContextService _currentContext;
        private readonly IPurchasingSuggestService _purchasingSuggestService;
        private readonly IProductHelperService _productHelperService;
        private readonly ICustomGenCodeHelperService _customGenCodeHelperService;
        private readonly IManufacturingHelperService _manufacturingHelperService;

        private readonly ObjectActivityLogFacade _poActivityLog;
        private readonly IMapper _mapper;
        private readonly IMailFactoryService _mailFactoryService;
        private readonly IUserHelperService _userHelperService;
        private readonly IOrganizationHelperService _organizationHelperService;
        private readonly INotificationFactoryService _notificationFactoryService;


        public PurchaseOrderService(
            PurchaseOrderDBContext purchaseOrderDBContext
           , ILogger<PurchasingSuggestService> logger
           , IActivityLogService activityLogService
           , IAsyncRunnerService asyncRunner
           , ICurrentContextService currentContext
           , IPurchasingSuggestService purchasingSuggestService
           , IProductHelperService productHelperService
           , ICustomGenCodeHelperService customGenCodeHelperService
           , IManufacturingHelperService manufacturingHelperService
           , IMapper mapper, IMailFactoryService mailFactoryService
           , IUserHelperService userHelperService, IOrganizationHelperService organizationHelperService
           , INotificationFactoryService notificationFactoryService)
        {
            _purchaseOrderDBContext = purchaseOrderDBContext;
            _poActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.PurchaseOrder);
            _currentContext = currentContext;
            _purchasingSuggestService = purchasingSuggestService;
            _productHelperService = productHelperService;
            _customGenCodeHelperService = customGenCodeHelperService;
            _manufacturingHelperService = manufacturingHelperService;
            _mapper = mapper;
            _mailFactoryService = mailFactoryService;
            _userHelperService = userHelperService;
            _organizationHelperService = organizationHelperService;
            _notificationFactoryService = notificationFactoryService;
        }

        public async Task<bool> SendMailNotifyCheckAndCensor(long purchaseOrderId, string mailTemplateCode, string[] mailTo)
        {
            var purchaseOrder = await GetInfo(purchaseOrderId);
            var userIds = new [] {purchaseOrder.CreatedByUserId, purchaseOrder.CheckedByUserId.GetValueOrDefault(), purchaseOrder.UpdatedByUserId, purchaseOrder.CensorByUserId.GetValueOrDefault()};
            var users = await _userHelperService.GetByIds(userIds);

            var createdUser = users.FirstOrDefault(x=>x.UserId == purchaseOrder.CreatedByUserId)?.FullName;
            var updatedUser = users.FirstOrDefault(x=>x.UserId == purchaseOrder.UpdatedByUserId)?.FullName;
            var checkedUser = users.FirstOrDefault(x=>x.UserId == purchaseOrder.CheckedByUserId)?.FullName;
            var censortUser = users.FirstOrDefault(x=>x.UserId == purchaseOrder.CensorByUserId)?.FullName;

            var businessInfo = await _organizationHelperService.BusinessInfo();
            
            return await _mailFactoryService.Dispatch(mailTo, mailTemplateCode, new ObjectDataTemplateMail()
            {
                CensoredByUser = censortUser,
                CheckedByUser = checkedUser,
                CreatedByUser = createdUser,
                UpdatedByUser = updatedUser,
                CompanyName = businessInfo.CompanyName,
                F_Id = purchaseOrderId,
                Code = purchaseOrder.PurchaseOrderCode,
                TotalMoney = purchaseOrder.TotalMoney.ToString("#,##0.##"),
                Domain = _currentContext.Domain
            });
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

        public async Task<PageData<PurchaseOrderOutputListByProduct>> GetListByProduct(string keyword, IList<string> poCodes, IList<int> purchaseOrderTypes, IList<int> productIds, EnumPurchaseOrderStatus? purchaseOrderStatusId, EnumPoProcessStatus? poProcessStatusId, bool? isChecked, bool? isApproved, long? fromDate, long? toDate, string sortBy, bool asc, int page, int size)
        {
            keyword = (keyword ?? "").Trim();

            var poQuery = _purchaseOrderDBContext.PurchaseOrder.AsQueryable();
            if (poCodes?.Count > 0)
            {
                poQuery = poQuery.Where(po => poCodes.Contains(po.PurchaseOrderCode));

            }
            var query = from po in poQuery
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

                            po.TaxInPercent,
                            po.TaxInMoney,
                            pod.Description,

                            pod.PoProviderPricingCode,
                            pod.OrderCode,
                            pod.ProductionOrderCode,

                            pod.SortOrder,


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

                            po.CurrencyId,
                            pod.ExchangedMoney,
                            po.ExchangeRate
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
                    || q.PoProviderPricingCode.Contains(keyword)
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
            query = query.SortByFieldName(sortBy, asc).ThenBy(q => q.SortOrder);
            if (size > 0)
            {
                query = query.Skip((page - 1) * size).Take(size);
            }
            var pagedData = await query.ToListAsync();
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
            foreach (var item in pagedData)
            {
                assignmentDetails.TryGetValue(item.PoAssignmentDetailId ?? 0, out var assignmentDetailInfo);

                suggestDetails.TryGetValue(item.PurchasingSuggestDetailId ?? 0, out var purchasingSuggestDetailInfo);


                result.Add(new PurchaseOrderOutputListByProduct()
                {
                    PurchaseOrderId = item.PurchaseOrderId,
                    PurchaseOrderCode = item.PurchaseOrderCode,
                    Date = item.Date.GetUnix(),
                    CustomerId = item.CustomerId,
                    DeliveryDestination = item.DeliveryDestination?.JsonDeserialize<DeliveryDestinationModel>(),
                    Content = item.Content,
                    AdditionNote = item.AdditionNote,
                    DeliveryFee = item.DeliveryFee,
                    OtherFee = item.OtherFee,
                    TotalMoney = item.TotalMoney,
                    PurchaseOrderStatusId = (EnumPurchaseOrderStatus)item.PurchaseOrderStatusId,
                    IsChecked = item.IsChecked,
                    IsApproved = item.IsApproved,
                    PoProcessStatusId = (EnumPoProcessStatus?)item.PoProcessStatusId,
                    CreatedByUserId = item.CreatedByUserId,
                    UpdatedByUserId = item.UpdatedByUserId,
                    CheckedByUserId = item.CheckedByUserId,
                    CensorByUserId = item.CensorByUserId,

                    CreatedDatetimeUtc = item.CreatedDatetimeUtc.GetUnix(),
                    UpdatedDatetimeUtc = item.UpdatedDatetimeUtc.GetUnix(),
                    CheckedDatetimeUtc = item.CheckedDatetimeUtc.GetUnix(),
                    CensorDatetimeUtc = item.CensorDatetimeUtc.GetUnix(),


                    //detail
                    PurchaseOrderDetailId = item.PurchaseOrderDetailId,
                    PurchasingSuggestDetailId = item.PurchasingSuggestDetailId,
                    PoAssignmentDetailId = item.PoAssignmentDetailId,
                    ProviderProductName = item.ProviderProductName,

                    ProductId = item.ProductId,
                    PrimaryQuantity = item.PrimaryQuantity,
                    PrimaryUnitPrice = item.PrimaryUnitPrice,

                    ProductUnitConversionId = item.ProductUnitConversionId,
                    ProductUnitConversionQuantity = item.ProductUnitConversionQuantity,
                    ProductUnitConversionPrice = item.ProductUnitConversionPrice,

                    TaxInPercent = item.TaxInPercent,
                    TaxInMoney = item.TaxInMoney,
                    PoProviderPricingCode = item.PoProviderPricingCode,
                    OrderCode = item.OrderCode,
                    ProductionOrderCode = item.ProductionOrderCode,
                    Description = item.Description,

                    PoAssignmentDetail = assignmentDetailInfo,
                    PurchasingSuggestDetail = purchasingSuggestDetailInfo,

                    DeliveryDate = item.DeliveryDate.GetUnix(),
                    CreatorFullName = item.CreatorFullName,
                    CheckerFullName = item.CheckerFullName,
                    CensorFullName = item.CensorFullName,
                    PurchaseOrderType = item.PurchaseOrderType,
                    IntoMoney = item.IntoMoney,

                    CurrencyId = item.CurrencyId,
                    ExchangedMoney = item.ExchangedMoney,
                    ExchangeRate = item.ExchangeRate,
                    SortOrder = item.SortOrder
                });
            }
            return (result, total, new { SumTotalMoney = sumTotalMoney, additionResult.SumPrimaryQuantity, additionResult.SumTaxInMoney });
        }


        public async Task<PurchaseOrderOutput> GetInfo(long purchaseOrderId)
        {
            var info = await _purchaseOrderDBContext.PurchaseOrder.AsNoTracking().Where(po => po.PurchaseOrderId == purchaseOrderId).FirstOrDefaultAsync();

            if (info == null) throw new BadRequestException(PurchaseOrderErrorCode.PoNotFound);

            var details = await _purchaseOrderDBContext.PurchaseOrderDetail.AsNoTracking().Where(d => d.PurchaseOrderId == purchaseOrderId)
            .Include(x=>x.PurchaseOrderDetailSubCalculation)
            .ToListAsync();

            var poAssignmentDetailIds = details.Where(d => d.PoAssignmentDetailId.HasValue).Select(d => d.PoAssignmentDetailId.Value).ToList();
            var purchasingSuggestDetailIds = details.Where(d => d.PurchasingSuggestDetailId.HasValue).Select(d => d.PurchasingSuggestDetailId.Value).ToList();

            var assignmentDetails = (await _purchasingSuggestService.PoAssignmentDetailInfos(poAssignmentDetailIds))
                .ToDictionary(d => d.PoAssignmentDetailId, d => d);

            var suggestDetails = (await _purchasingSuggestService.PurchasingSuggestDetailInfo(purchasingSuggestDetailIds))
                .ToDictionary(d => d.PurchasingSuggestDetailId, d => d);

            var files = await _purchaseOrderDBContext.PurchaseOrderFile.AsNoTracking().Where(d => d.PurchaseOrderId == purchaseOrderId).ToListAsync();
            
            var excess = await _purchaseOrderDBContext.PurchaseOrderExcess.AsNoTracking().Where(d => d.PurchaseOrderId == purchaseOrderId).ProjectTo<PurchaseOrderExcessModel>(_mapper.ConfigurationProvider).ToListAsync();
            var materials = await _purchaseOrderDBContext.PurchaseOrderMaterials.AsNoTracking().Where(d => d.PurchaseOrderId == purchaseOrderId).ProjectTo<PurchaseOrderMaterialsModel>(_mapper.ConfigurationProvider).ToListAsync();

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

                TaxInPercent = info.TaxInPercent,
                TaxInMoney = info.TaxInMoney,
                CurrencyId = info.CurrencyId,
                ExchangeRate = info.ExchangeRate,

                FileIds = files.Select(f => f.FileId).ToList(),
                Details = details.OrderBy(d => d.SortOrder)
                .Select(d =>
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

                        PoProviderPricingCode = d.PoProviderPricingCode,
                        OrderCode = d.OrderCode,
                        ProductionOrderCode = d.ProductionOrderCode,
                        Description = d.Description,

                        PoAssignmentDetail = assignmentDetailInfo,
                        PurchasingSuggestDetail = purchasingSuggestDetailInfo,
                        IntoMoney = d.IntoMoney,

                        ExchangedMoney = d.ExchangedMoney,
                        SortOrder = d.SortOrder,
                        OutsourceRequestId = d.OutsourceRequestId,
                        ProductionStepLinkDataId = d.ProductionStepLinkDataId,
                        SubCalculations = d.PurchaseOrderDetailSubCalculation.Select(s => new PurchaseOrderDetailSubCalculationModel
                        {
                            PrimaryQuantity = s.PrimaryQuantity,
                            ProductBomId = s.ProductBomId,
                            PurchaseOrderDetailId = s.PurchaseOrderDetailId,
                            PrimaryUnitPrice = s.PrimaryUnitPrice,
                            PurchaseOrderDetailSubCalculationId = s.PurchaseOrderDetailSubCalculationId,
                            UnitConversionId = s.UnitConversionId
                        }).ToList(),
                        IsSubCalculation = d.IsSubCalculation
                    };
                }).ToList(),
                Excess = excess.OrderBy(e => e.SortOrder).ToList(),
                Materials = materials.OrderBy(m => m.SortOrder).ToList(),
                
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
                    PurchaseOrderType = (int)EnumPurchasingOrderType.Default,
                    TaxInPercent = model.TaxInPercent,
                    TaxInMoney = model.TaxInMoney,

                    CurrencyId = model.CurrencyId,
                    ExchangeRate = model.ExchangeRate
                };

                if (po.DeliveryDestination?.Length > 1024)
                {
                    throw DeleveryDestinationTooLong.BadRequest();
                }

                await _purchaseOrderDBContext.AddAsync(po);
                await _purchaseOrderDBContext.SaveChangesAsync();

                var sortOrder = 1;
                foreach (var item in model.Details)
                {
                    var assignmentDetail = poAssignmentDetails.FirstOrDefault(a => a.PoAssignmentDetailId == item.PoAssignmentDetailId);

                    var eDetail =  new PurchaseOrderDetail()
                    {
                        PurchaseOrderId = po.PurchaseOrderId,

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

                        PoProviderPricingCode = item.PoProviderPricingCode,

                        OrderCode = item.OrderCode,
                        ProductionOrderCode = item.ProductionOrderCode,
                        Description = item.Description,
                        CreatedDatetimeUtc = DateTime.UtcNow,
                        UpdatedDatetimeUtc = DateTime.UtcNow,
                        IsDeleted = false,
                        DeletedDatetimeUtc = null,
                        IntoMoney = item.IntoMoney,

                        ExchangedMoney = item.ExchangedMoney,
                        SortOrder = sortOrder,
                        IsSubCalculation = item.IsSubCalculation
                    };

                    await _purchaseOrderDBContext.PurchaseOrderDetail.AddAsync(eDetail);

                    await _purchaseOrderDBContext.SaveChangesAsync();

                    var arrEntitySubCalculation = item.SubCalculations.Select(x=> new PurchaseOrderDetailSubCalculation
                    {
                        PrimaryQuantity = x.PrimaryQuantity,
                        ProductBomId = x.ProductBomId,
                        PurchaseOrderDetailId = eDetail.PurchaseOrderDetailId,
                        PrimaryUnitPrice = x.PrimaryUnitPrice,
                        UnitConversionId = x.UnitConversionId
                    });

                    await _purchaseOrderDBContext.PurchaseOrderDetailSubCalculation.AddRangeAsync(arrEntitySubCalculation);
                    await _purchaseOrderDBContext.SaveChangesAsync();

                    sortOrder++;
                }

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


                await ctx.ConfirmCode();

                await _poActivityLog.LogBuilder(() => PurchaseOrderActivityLogMessage.Create)
                  .MessageResourceFormatDatas(po.PurchaseOrderCode)
                  .ObjectId(po.PurchaseOrderId)
                  .JsonData((new { purchaseOrderType = EnumPurchasingOrderType.Default, model }).JsonSerialize())
                  .CreateLog();
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
                info.TaxInPercent = model.TaxInPercent;
                info.TaxInMoney = model.TaxInMoney;

                info.CurrencyId = model.CurrencyId;
                info.ExchangeRate = model.ExchangeRate;

                if (info.DeliveryDestination?.Length > 1024)
                {
                    throw DeleveryDestinationTooLong.BadRequest();
                }


                var details = await _purchaseOrderDBContext.PurchaseOrderDetail.Where(d => d.PurchaseOrderId == purchaseOrderId).ToListAsync();


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

                            detail.PoProviderPricingCode = item.PoProviderPricingCode;
                            detail.OrderCode = item.OrderCode;
                            detail.ProductionOrderCode = item.ProductionOrderCode;
                            detail.Description = item.Description;
                            detail.UpdatedDatetimeUtc = DateTime.UtcNow;
                            detail.IntoMoney = item.IntoMoney;
                            detail.ExchangedMoney = item.ExchangedMoney;
                            detail.SortOrder = item.SortOrder;
                            detail.IsSubCalculation = item.IsSubCalculation;

                            var arrEntitySubCalculation = _purchaseOrderDBContext.PurchaseOrderDetailSubCalculation.Where(x=>x.PurchaseOrderDetailId == detail.PurchaseOrderDetailId).ToList();
                            foreach (var sub in arrEntitySubCalculation)
                            {
                                var mSub = item.SubCalculations.FirstOrDefault(x=>x.PurchaseOrderDetailSubCalculationId == sub.PurchaseOrderDetailSubCalculationId);
                                if(mSub != null)
                                    _mapper.Map(mSub, sub);
                                else sub.IsDeleted = true;
                            }
                            var arrNewEntitySubCalculation = item.SubCalculations.Where(x=>x.PurchaseOrderDetailSubCalculationId <= 0)
                            .Select(x => new PurchaseOrderDetailSubCalculation
                            {
                                PrimaryQuantity = x.PrimaryQuantity,
                                ProductBomId = x.ProductBomId,
                                PurchaseOrderDetailId = detail.PurchaseOrderDetailId,
                                PrimaryUnitPrice = x.PrimaryUnitPrice,
                                UnitConversionId = x.UnitConversionId
                            });
                            await _purchaseOrderDBContext.PurchaseOrderDetailSubCalculation.AddRangeAsync(arrNewEntitySubCalculation);
                            await _purchaseOrderDBContext.SaveChangesAsync();
                            
                            break;
                        }
                    }

                    if (!found)
                    {
                        var eDetail = new PurchaseOrderDetail()
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

                            PoProviderPricingCode = item.PoProviderPricingCode,
                            OrderCode = item.OrderCode,
                            ProductionOrderCode = item.ProductionOrderCode,
                            Description = item.Description,
                            CreatedDatetimeUtc = DateTime.UtcNow,
                            UpdatedDatetimeUtc = DateTime.UtcNow,
                            IsDeleted = false,
                            DeletedDatetimeUtc = null,
                            IntoMoney = item.IntoMoney,
                            ExchangedMoney = item.ExchangedMoney,
                            SortOrder = item.SortOrder,
                            IsSubCalculation = item.IsSubCalculation
                        };

                        await _purchaseOrderDBContext.PurchaseOrderDetail.AddAsync(eDetail);
                        await _purchaseOrderDBContext.SaveChangesAsync();

                        var arrEntitySubCalculation = item.SubCalculations.Select(x => new PurchaseOrderDetailSubCalculation
                        {
                            PrimaryQuantity = x.PrimaryQuantity,
                            ProductBomId = x.ProductBomId,
                            PurchaseOrderDetailId = eDetail.PurchaseOrderDetailId,
                            PrimaryUnitPrice = x.PrimaryUnitPrice,
                            UnitConversionId = x.UnitConversionId
                        });

                        await _purchaseOrderDBContext.PurchaseOrderDetailSubCalculation.AddRangeAsync(arrEntitySubCalculation);
                        await _purchaseOrderDBContext.SaveChangesAsync();
                    }
                }

                var updatedIds = model.Details.Select(d => d.PurchaseOrderDetailId).ToList();

                var deleteDetails = details.Where(d => !updatedIds.Contains(d.PurchaseOrderDetailId));

                foreach (var detail in deleteDetails)
                {
                    detail.IsDeleted = true;
                    detail.DeletedDatetimeUtc = DateTime.UtcNow;
                }

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

                var allDetails = await _purchaseOrderDBContext.PurchaseOrderDetail.Where(d => d.PurchaseOrderId == purchaseOrderId).ToListAsync();
                var sortOrder = 1;
                foreach (var item in allDetails)
                {
                    item.SortOrder = sortOrder++;
                }

                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();


                await _poActivityLog.LogBuilder(() => PurchaseOrderActivityLogMessage.Update)
                   .MessageResourceFormatDatas(info.PurchaseOrderCode)
                   .ObjectId(info.PurchaseOrderId)
                   .JsonData((new { purchaseOrderType = EnumPurchasingOrderType.Default, model }).JsonSerialize())
                   .CreateLog();

            
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

                    var SubCalculations = await _purchaseOrderDBContext.PurchaseOrderDetailSubCalculation.Where(d => d.PurchaseOrderDetailId == item.PurchaseOrderDetailId).ToListAsync();
                    SubCalculations.ForEach(x => x.IsDeleted = true);
                }

                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();


                await _poActivityLog.LogBuilder(() => PurchaseOrderActivityLogMessage.Delete)
                   .MessageResourceFormatDatas(info.PurchaseOrderCode)
                   .ObjectId(info.PurchaseOrderId)
                   .JsonData((new { purchaseOrderType = EnumPurchasingOrderType.Default, model = info }).JsonSerialize())
                   .CreateLog();

            

                return true;
            }
        }

        public CategoryNameModel GetFieldDataForMapping()
        {
            var result = new CategoryNameModel()
            {
                //CategoryId = 1,
                CategoryCode = "PurchaseOrder",
                CategoryTitle = "PurchaseOrder",
                IsTreeView = false,
                Fields = new List<CategoryFieldNameModel>()
            };
            var fields = Utils.GetFieldNameModels<PoDetailRowValue>();
            result.Fields = fields;
            return result;
        }

        public IAsyncEnumerable<PurchaseOrderInputDetail> ParseInvoiceDetails(ImportExcelMapping mapping, SingleInvoiceStaticContent extra, Stream stream)
        {
            return new PurchaseOrderParseExcelFacade(_productHelperService)
                 .ParseInvoiceDetails(mapping, extra, stream);
        }



        public async Task<bool> Checked(long purchaseOrderId)
        {
            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var info = await _purchaseOrderDBContext.PurchaseOrder.FirstOrDefaultAsync(d => d.PurchaseOrderId == purchaseOrderId);
                if (info == null) throw new BadRequestException(PurchaseOrderErrorCode.PoNotFound);

                if (info.PurchaseOrderStatusId == (int)EnumPurchaseOrderStatus.Checked && info.IsChecked == true)
                {
                    throw PoAlreadyChecked.BadRequest();
                }

                if (info.PurchaseOrderStatusId != (int)EnumPurchaseOrderStatus.WaitToCensor
                    && info.PurchaseOrderStatusId != (int)EnumPurchaseOrderStatus.Checked)
                {
                    throw PoNotSentToCensorYet.BadRequest();
                }

                info.IsChecked = true;

                info.PurchaseOrderStatusId = (int)EnumPurchaseOrderStatus.Checked;
                info.CheckedDatetimeUtc = DateTime.Now.Date.GetUnixUtc(_currentContext.TimeZoneOffset).UnixToDateTime();
                info.CheckedByUserId = _currentContext.UserId;

                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await UpdateStatusForOutsourceRequestInPurcharOrder(purchaseOrderId, (EnumPurchasingOrderType)info.PurchaseOrderType);

                await _notificationFactoryService.AddSubscriptionToThePermissionPerson(new SubscriptionToThePermissionPersonSimpleModel
                {
                    ObjectId = info.PurchaseOrderId,
                    ObjectTypeId = (int)EnumObjectType.PurchaseOrder,
                    ModuleId = _currentContext.ModuleId,
                    PermissionId = (int)EnumActionType.Censor
                });

                await _poActivityLog.LogBuilder(() => PurchaseOrderActivityLogMessage.CheckApprove)
                   .MessageResourceFormatDatas(info.PurchaseOrderCode)
                   .ObjectId(info.PurchaseOrderId)
                   .JsonData((new { purchaseOrderId }).JsonSerialize())
                   .CreateLog();

                await _notificationFactoryService.AddSubscription(new SubscriptionSimpleModel
                {
                    ObjectId = purchaseOrderId,
                    UserId = _currentContext.UserId,
                    ObjectTypeId = (int)EnumObjectType.PurchaseOrder
                });

            

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
                    throw PoHasBeenFailAtCheck.BadRequest();
                }

                if (info.PurchaseOrderStatusId != (int)EnumPurchaseOrderStatus.WaitToCensor
                    && info.PurchaseOrderStatusId != (int)EnumPurchaseOrderStatus.Checked)
                {
                    throw PoNotSentToCensorYet.BadRequest();
                }

                info.IsChecked = false;

                info.PurchaseOrderStatusId = (int)EnumPurchaseOrderStatus.Checked;
                info.CheckedDatetimeUtc = DateTime.Now.Date.GetUnixUtc(_currentContext.TimeZoneOffset).UnixToDateTime();
                info.CheckedByUserId = _currentContext.UserId;

                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await UpdateStatusForOutsourceRequestInPurcharOrder(purchaseOrderId, (EnumPurchasingOrderType)info.PurchaseOrderType);


                await _poActivityLog.LogBuilder(() => PurchaseOrderActivityLogMessage.CheckReject)
                  .MessageResourceFormatDatas(info.PurchaseOrderCode)
                  .ObjectId(info.PurchaseOrderId)
                  .JsonData((new { purchaseOrderId }).JsonSerialize())
                  .CreateLog();

            
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
                    throw PoAlreadyApproved.BadRequest();
                }

                if (info.IsChecked != true)
                {
                    throw PoNotPassCheckYet.BadRequest();
                }

                if (info.PurchaseOrderStatusId != (int)EnumPurchaseOrderStatus.Censored
                    && info.PurchaseOrderStatusId != (int)EnumPurchaseOrderStatus.Checked)
                {
                    throw PoNotSentToCensorYet.BadRequest();
                }

                info.IsApproved = true;

                info.PurchaseOrderStatusId = (int)EnumPurchaseOrderStatus.Censored;
                info.CensorDatetimeUtc = DateTime.Now.Date.GetUnixUtc(_currentContext.TimeZoneOffset).UnixToDateTime();
                info.CensorByUserId = _currentContext.UserId;

                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await UpdateStatusForOutsourceRequestInPurcharOrder(purchaseOrderId, (EnumPurchasingOrderType)info.PurchaseOrderType);

                await _poActivityLog.LogBuilder(() => PurchaseOrderActivityLogMessage.CensorApprove)
                   .MessageResourceFormatDatas(info.PurchaseOrderCode)
                   .ObjectId(info.PurchaseOrderId)
                   .JsonData((new { purchaseOrderId }).JsonSerialize())
                   .CreateLog();

            
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
                    throw PoAlreadyRejected.BadRequest();
                }

                if (info.IsChecked != true)
                {
                    throw PoNotPassCheckYet.BadRequest();
                }

                if (info.PurchaseOrderStatusId != (int)EnumPurchaseOrderStatus.Censored
                   && info.PurchaseOrderStatusId != (int)EnumPurchaseOrderStatus.Checked)
                {
                    throw PoNotSentToCensorYet.BadRequest();
                }


                info.IsApproved = false;

                info.PurchaseOrderStatusId = (int)EnumPurchaseOrderStatus.Censored;
                info.CensorDatetimeUtc = DateTime.Now.Date.GetUnixUtc(_currentContext.TimeZoneOffset).UnixToDateTime();
                info.CensorByUserId = _currentContext.UserId;

                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await UpdateStatusForOutsourceRequestInPurcharOrder(purchaseOrderId, (EnumPurchasingOrderType)info.PurchaseOrderType);

                await _poActivityLog.LogBuilder(() => PurchaseOrderActivityLogMessage.CensorReject)
                  .MessageResourceFormatDatas(info.PurchaseOrderCode)
                  .ObjectId(info.PurchaseOrderId)
                  .JsonData((new { purchaseOrderId }).JsonSerialize())
                  .CreateLog();

            
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

                await UpdateStatusForOutsourceRequestInPurcharOrder(purchaseOrderId, (EnumPurchasingOrderType)info.PurchaseOrderType);

                await _notificationFactoryService.AddSubscriptionToThePermissionPerson(new SubscriptionToThePermissionPersonSimpleModel
                {
                    ObjectId = info.PurchaseOrderId,
                    ObjectTypeId = (int)EnumObjectType.PurchaseOrder,
                    ModuleId = _currentContext.ModuleId,
                    PermissionId = (int)EnumActionType.Check
                });

                await _poActivityLog.LogBuilder(() => PurchaseOrderActivityLogMessage.SendToCensor)
                  .MessageResourceFormatDatas(info.PurchaseOrderCode)
                  .ObjectId(info.PurchaseOrderId)
                  .JsonData((new { purchaseOrderId }).JsonSerialize())
                  .CreateLog();

                await _notificationFactoryService.AddSubscription(new SubscriptionSimpleModel{
                    ObjectId = purchaseOrderId,
                    UserId = _currentContext.UserId,
                    ObjectTypeId = (int)EnumObjectType.PurchaseOrder, 
                });

            

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

                await _poActivityLog.LogBuilder(() => PurchaseOrderActivityLogMessage.UpdatePoProcessStatus)
                 .MessageResourceFormatDatas(poProcessStatusId, info.PurchaseOrderCode)
                 .ObjectId(info.PurchaseOrderId)
                 .JsonData((new { purchaseOrderId, poProcessStatusId }).JsonSerialize())
                 .CreateLog();

            
                
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
                orderby sd.SortOrder
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
                orderby sd.SortOrder
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

        public async Task<bool> RemoveOutsourcePart(long[] arrPurchaseOrderId, long outsourcePartRequestId)
        {
            var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync();
            try
            {
                foreach (var purchaseOrderId in arrPurchaseOrderId)
                {
                    var po = await _purchaseOrderDBContext.PurchaseOrder.FirstOrDefaultAsync(x => x.PurchaseOrderId == purchaseOrderId);
                    if (po == null || po.PurchaseOrderType != (int)EnumPurchasingOrderType.OutsourcePart)
                        throw new BadRequestException(PurchaseOrderErrorCode.PoNotFound);


                    var poDetails = await _purchaseOrderDBContext.PurchaseOrderDetail.Where(x => x.PurchaseOrderId == purchaseOrderId).ToListAsync();

                    var arrPoDetailIdWithOutOutsourceRequestId = poDetails.Where(x => x.OutsourceRequestId.HasValue == false).Select(x => x.PurchaseOrderDetailId).ToList();
                    var arrMapping = await _purchaseOrderDBContext.PurchaseOrderOutsourceMapping.Where(x => arrPoDetailIdWithOutOutsourceRequestId.Contains(x.PurchaseOrderDetailId) && x.OutsourcePartRequestId == outsourcePartRequestId).ToListAsync();
                    
                    arrMapping.ForEach(x => x.IsDeleted = true);

                    foreach (var d in poDetails.Where(x => x.OutsourceRequestId == outsourcePartRequestId))
                    {
                        d.OutsourceRequestId = null;
                        d.ProductionOrderCode = string.Empty;
                    }

                    await _purchaseOrderDBContext.SaveChangesAsync();
                }
                
                await trans.CommitAsync();
                return true;
            }
            catch (System.Exception ex)
            {
                await trans.RollbackAsync();
                throw ex;
            }
        }
        

        public async Task<IList<PurchaseOrderOutsourcePartAllocate>> GetAllPurchaseOrderOutsourcePart()
        {
            var query1 = from p in _purchaseOrderDBContext.PurchaseOrder
                         join pd in _purchaseOrderDBContext.PurchaseOrderDetail on p.PurchaseOrderId equals pd.PurchaseOrderId
                         where p.PurchaseOrderType == (int)EnumPurchasingOrderType.OutsourcePart && pd.OutsourceRequestId.HasValue == false
                         select new
                         {
                             p.PurchaseOrderId,
                             p.PurchaseOrderCode,
                             pd.ProductId,
                             pd.PrimaryQuantity,
                             pd.PurchaseOrderDetailId
                         };
            var query2 = _purchaseOrderDBContext.PurchaseOrderOutsourceMapping.GroupBy(x => new { x.PurchaseOrderDetailId, x.ProductId })
                        .Select(x => new
                        {
                            PurchaseOrderDetailId = x.Key.PurchaseOrderDetailId,
                            ProductId = x.Key.ProductId,
                            PrimaryQuantityAllocated = x.Sum(x => x.Quantity)
                        });

            var query = from q1 in query1
                        join q2 in query2 on q1.PurchaseOrderDetailId equals q2.PurchaseOrderDetailId into g
                        from q2 in g.DefaultIfEmpty()
                        select new PurchaseOrderOutsourcePartAllocate
                        {
                            PurchaseOrderId = q1.PurchaseOrderId,
                            PurchaseOrderCode = q1.PurchaseOrderCode,
                            ProductId = q1.ProductId,
                            PrimaryQuantity = q1.PrimaryQuantity,
                            PurchaseOrderDetailId = q1.PurchaseOrderDetailId,
                            PrimaryQuantityAllocated = q2.PrimaryQuantityAllocated
                        };
            return await query.Where(x => x.PrimaryQuantity > x.PrimaryQuantityAllocated.GetValueOrDefault()).ToListAsync();
        }

        public async Task<IList<EnrichDataPurchaseOrderOutsourcePart>> EnrichDataForPurchaseOrderOutsourcePart(long purchaseOrderId)
        {
            var queryRefPurchaseOrderOutsource = from p in _purchaseOrderDBContext.PurchaseOrder
                        join pd in _purchaseOrderDBContext.PurchaseOrderDetail on p.PurchaseOrderId equals pd.PurchaseOrderId
                        join m in _purchaseOrderDBContext.PurchaseOrderOutsourceMapping on pd.PurchaseOrderDetailId equals m.PurchaseOrderDetailId into gm
                        from m in gm.DefaultIfEmpty()
                        where p.PurchaseOrderType == (int)EnumPurchasingOrderType.OutsourcePart && p.PurchaseOrderId == purchaseOrderId
                        select new {
                            p.PurchaseOrderId,
                            pd.PurchaseOrderDetailId,
                            OutsourceRequestId = pd.OutsourceRequestId.HasValue ? pd.OutsourceRequestId.GetValueOrDefault() : m == null ? 0 : m.OutsourcePartRequestId,
                            ProductionOrderCode = pd.OutsourceRequestId.HasValue ? pd.ProductionOrderCode : m == null ? string.Empty : m.ProductionOrderCode,
                        };

            var queryRefOutsourcePart = _purchaseOrderDBContext.RefOutsourcePartRequest.AsQueryable();

            var query = from v in queryRefPurchaseOrderOutsource
                        join r in queryRefOutsourcePart on v.OutsourceRequestId equals r.OutsourcePartRequestId
                        select new EnrichDataPurchaseOrderOutsourcePart
                        {

                            PurchaseOrderId = v.PurchaseOrderId,
                            PurchaseOrderDetailId = v.PurchaseOrderDetailId,
                            OutsourceRequestId = v.OutsourceRequestId,
                            ProductionOrderCode = v.ProductionOrderCode,
                            OutsourceRequestCode = r.OutsourcePartRequestCode,
                            ProductionOrderId = r.ProductionOrderId 
                        };
            var data = await query.ToListAsync();
            return data;
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




        protected async Task<long[]> GetAllOutsourceRequestIdInPurchaseOrder(long purchaseOrderId)
        {
            var outsourceRequestId = _purchaseOrderDBContext.PurchaseOrderDetail.Where(x => x.PurchaseOrderId == purchaseOrderId)
                .Select(x => x.OutsourceRequestId.GetValueOrDefault())
                .Distinct()
                .ToArray();
            return await Task.FromResult(outsourceRequestId);
        }
        private async Task<bool> UpdateStatusForOutsourceRequestInPurcharOrder(long purchaseOrderId, EnumPurchasingOrderType purchaseOrderType)
        {
            var outsourceRequestId = await GetAllOutsourceRequestIdInPurchaseOrder(purchaseOrderId);

            if (purchaseOrderType == EnumPurchasingOrderType.OutsourcePart)
                return await _manufacturingHelperService.UpdateOutsourcePartRequestStatus(outsourceRequestId);

            if (purchaseOrderType == EnumPurchasingOrderType.OutsourceStep)
                return await _manufacturingHelperService.UpdateOutsourceStepRequestStatus(outsourceRequestId);

            return true;
        }
    }
}
