using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.EF.PurchaseOrderDB;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.PurchaseOrder.Model;
using VErp.Services.PurchaseOrder.Model.PurchaseOrder;
using VErp.Services.PurchaseOrder.Model.Request;
using VErp.Services.PurchaseOrder.Service.Po.Implement.Facade;
using static Verp.Resources.PurchaseOrder.Po.PurchaseOrderOutsourceValidationMessage;
using PurchaseOrderEntity = VErp.Infrastructure.EF.PurchaseOrderDB.PurchaseOrder;
using VErp.Infrastructure.EF.EFExtensions;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Office2010.Excel;
using VErp.Commons.GlobalObject.InternalDataInterface.Category;
using DocumentFormat.OpenXml.InkML;
using Verp.Resources.Enums.ErrorCodes.PO;
using VErp.Commons.GlobalObject.InternalDataInterface.System;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper.General;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper.System;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper.Hr;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper.Manufacture;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper.Product;

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
        private readonly ICategoryHelperService _categoryHelperService;
        private readonly IPurchaseOrderImportExcelFacadeService _purchaseOrderImportExcelFacadeService;

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
           , INotificationFactoryService notificationFactoryService, ICategoryHelperService categoryHelperService
           , IPurchaseOrderImportExcelFacadeService purchaseOrderImportExcelFacadeService)
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
            _categoryHelperService = categoryHelperService;
            _purchaseOrderImportExcelFacadeService = purchaseOrderImportExcelFacadeService;
        }

        public async Task<bool> SendMailNotifyCheckAndCensor(long purchaseOrderId, string mailTemplateCode, string[] mailTo)
        {
            var purchaseOrder = await GetInfo(purchaseOrderId);
            var userIds = new[] { purchaseOrder.CreatedByUserId, purchaseOrder.CheckedByUserId.GetValueOrDefault(), purchaseOrder.UpdatedByUserId, purchaseOrder.CensorByUserId.GetValueOrDefault() };
            var users = await _userHelperService.GetByIds(userIds);

            var createdUser = users.FirstOrDefault(x => x.UserId == purchaseOrder.CreatedByUserId)?.FullName;
            var updatedUser = users.FirstOrDefault(x => x.UserId == purchaseOrder.UpdatedByUserId)?.FullName;
            var checkedUser = users.FirstOrDefault(x => x.UserId == purchaseOrder.CheckedByUserId)?.FullName;
            var censortUser = users.FirstOrDefault(x => x.UserId == purchaseOrder.CensorByUserId)?.FullName;

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

        public async Task<PageData<PurchaseOrderOutputList>> GetList(PurchaseOrderFilterRequestModel req)
        {
            var (keyword, poCodes, purchaseOrderTypes, productIds, purchaseOrderStatusId, poProcessStatusId, isChecked, isApproved, fromDate, toDate, sortBy, asc, page, size, filters) = req;

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
                            po.Requirement,
                            po.DeliveryPolicy,
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

                            po.DeliveryMethod,
                            po.PaymentMethod,
                            po.AttachmentBill,
                            po.InputTypeSelectedState,
                            po.InputUnitTypeSelectedState,

                        };



            if (!string.IsNullOrWhiteSpace(keyword))
            {

                query = query
                   .Where(q => q.PurchaseOrderCode.Contains(keyword)
                    || q.Requirement.Contains(keyword)
                    || q.DeliveryPolicy.Contains(keyword)
                    || q.DeliveryMethod.Contains(keyword)
                    || q.PaymentMethod.Contains(keyword)
                    || q.AttachmentBill.Contains(keyword)
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

            query = query.InternalFilter(filters);

            var poQuery = _purchaseOrderDBContext.PurchaseOrder.Where(po => query.Select(p => p.PurchaseOrderId).Contains(po.PurchaseOrderId));

            var total = await poQuery.CountAsync();
            var additionResult = await (from q in poQuery
                                        group q by 1 into g
                                        select new
                                        {
                                            SumTotalMoney = g.Sum(x => x.TotalMoney),
                                        }).FirstOrDefaultAsync();
            var pagedData = await poQuery.SortByFieldName(sortBy, asc)
                .ThenBy(q => q.PurchaseOrderCode)
                .Skip((page - 1) * size).Take(size).ToListAsync();
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
                    Requirement = info.Requirement,
                    DeliveryPolicy = info.DeliveryPolicy,
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
                    PurchaseOrderType = info.PurchaseOrderType,

                    DeliveryMethod = info.DeliveryMethod,
                    PaymentMethod = info.DeliveryMethod,
                    AttachmentBill = info.AttachmentBill,

                    InputTypeSelectedState = info.InputTypeSelectedState.HasValue ? (EnumPurchaseOrderInputType)info.InputTypeSelectedState : EnumPurchaseOrderInputType.InputDefault,
                    InputUnitTypeSelectedState = info.InputUnitTypeSelectedState.HasValue ? (EnumPurchaseOrderInputUnitType)info.InputUnitTypeSelectedState : null,
                });
            }

            return (result, total, additionResult);
        }

        public async Task<PageData<PurchaseOrderOutputListByProduct>> GetListByProduct(PurchaseOrderFilterRequestModel req)
        {
            var (keyword, poCodes, purchaseOrderTypes, productIds, purchaseOrderStatusId, poProcessStatusId, isChecked, isApproved, fromDate, toDate, sortBy, asc, page, size, filters) = req;

            keyword = (keyword ?? "").Trim();

            var poQuery = _purchaseOrderDBContext.PurchaseOrder.AsQueryable();
            if (poCodes?.Count > 0)
            {
                poQuery = poQuery.Where(po => poCodes.Contains(po.PurchaseOrderCode));

            }

            var poDetails = _purchaseOrderDBContext.PurchaseOrderDetail.AsQueryable();
            if (req.IgnoreDetailIds?.Count > 0)
            {
                poDetails = poDetails.Where(d => !req.IgnoreDetailIds.Contains(d.PurchaseOrderDetailId));
            }

            var query = from po in poQuery
                        join pod in poDetails on po.PurchaseOrderId equals pod.PurchaseOrderId
                        join p in _purchaseOrderDBContext.RefProduct on pod.ProductId equals p.ProductId into ps
                        from p in ps.DefaultIfEmpty()
                        join c in _purchaseOrderDBContext.RefCustomer on po.CustomerId equals c.CustomerId into cs
                        from c in cs.DefaultIfEmpty()
                        join ad in _purchaseOrderDBContext.PoAssignment on pod.RefPoAssignmentId equals ad.PoAssignmentId into ads
                        from ad in ads.DefaultIfEmpty()
                        join a in _purchaseOrderDBContext.PoAssignment on ad.PoAssignmentId equals a.PoAssignmentId into aa
                        from a in aa.DefaultIfEmpty()
                        join sd in _purchaseOrderDBContext.PurchasingSuggest on pod.RefPurchasingSuggestId equals sd.PurchasingSuggestId into sds
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
                            po.Requirement,
                            po.DeliveryPolicy,
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
                            pod.RefPoAssignmentId,
                            pod.RefPurchasingSuggestId,

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

                            PoAssignmentCode = a == null ? null : a.PoAssignmentCode,

                            PurchasingSuggestCode = s == null ? null : s.PurchasingSuggestCode,
                            po.DeliveryDate,

                            CreatorFullName = creator.FullName,
                            CheckerFullName = checker.FullName,
                            CensorFullName = censor.FullName,
                            po.PurchaseOrderType,
                            pod.IntoMoney,

                            po.CurrencyId,
                            pod.ExchangedMoney,
                            po.ExchangeRate,

                            po.DeliveryMethod,
                            po.PaymentMethod,
                            po.AttachmentBill,

                            po.InputTypeSelectedState,
                            po.InputUnitTypeSelectedState,
                        };

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query
                    .Where(q => q.PurchaseOrderCode.Contains(keyword)
                    || q.Requirement.Contains(keyword)
                    || q.DeliveryPolicy.Contains(keyword)
                    || q.DeliveryMethod.Contains(keyword)
                    || q.PaymentMethod.Contains(keyword)
                    || q.AttachmentBill.Contains(keyword)
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

            query = query.InternalFilter(filters);

            var total = await query.CountAsync();

            var additionResult = await (from q in query
                                        group q by 1 into g
                                        select new
                                        {
                                            SumPrimaryQuantity = g.Sum(x => x.PrimaryQuantity),

                                        }).FirstOrDefaultAsync();

            var sumTotalMoney = await (from q in query
                                       group q by q.PurchaseOrderId into g
                                       select new
                                       {
                                           TotalMoney = g.Sum(x => x.TotalMoney) / g.Count(),
                                           SumTaxInMoney = g.Sum(x => x.TaxInMoney) / g.Count()
                                       }).ToListAsync();


            query = query.SortByFieldName(sortBy, asc)
                .ThenBy(q => q.PurchaseOrderCode)
                .ThenBy(q => q.SortOrder);

            if (size > 0)
            {
                query = query.Skip((page - 1) * size).Take(size);
            }
            var pagedData = await query.ToListAsync();


            var assignmentIds = pagedData.Select(d => d.RefPoAssignmentId).ToList();

            var suggestIds = pagedData.Where(d => d.RefPurchasingSuggestId.HasValue).Select(d => d.RefPurchasingSuggestId.Value).ToList();

            var assignmentDetails = await GetAssignementDetailInfos(assignmentIds);

            var suggestDetails = await GetSuggestDetailInfos(suggestIds);


            var result = new List<PurchaseOrderOutputListByProduct>();
            foreach (var item in pagedData)
            {
                var assignmentDetailInfo = assignmentDetails.FirstOrDefault(sd => sd.ProductId == item.ProductId && sd.PoAssignmentId == item.RefPoAssignmentId);


                var purchasingSuggestDetailInfo = suggestDetails
                            .Where(sd => sd.PurchasingSuggestId == item.RefPurchasingSuggestId && sd.ProductId == item.ProductId)
                            .OrderByDescending(d => d.CustomerId == item.CustomerId)
                            .FirstOrDefault();


                result.Add(new PurchaseOrderOutputListByProduct()
                {
                    PurchaseOrderId = item.PurchaseOrderId,
                    PurchaseOrderCode = item.PurchaseOrderCode,
                    Date = item.Date.GetUnix(),
                    CustomerId = item.CustomerId,
                    DeliveryDestination = item.DeliveryDestination?.JsonDeserialize<DeliveryDestinationModel>(),
                    Requirement = item.Requirement,
                    DeliveryPolicy = item.DeliveryPolicy,

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
                    PurchasingSuggestDetailId = purchasingSuggestDetailInfo?.PurchasingSuggestDetailId,
                    PoAssignmentDetailId = assignmentDetailInfo?.PoAssignmentDetailId,
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
                    SortOrder = item.SortOrder,

                    DeliveryMethod = item.DeliveryMethod,
                    PaymentMethod = item.DeliveryMethod,
                    AttachmentBill = item.AttachmentBill,

                    InputTypeSelectedState = item.InputTypeSelectedState.HasValue ? (EnumPurchaseOrderInputType)item.InputTypeSelectedState : EnumPurchaseOrderInputType.InputDefault,
                    InputUnitTypeSelectedState = item.InputUnitTypeSelectedState.HasValue ? (EnumPurchaseOrderInputUnitType)item.InputUnitTypeSelectedState : null,
                });
            }
            return (result, total, new { SumTotalMoney = sumTotalMoney.Sum(t => t.TotalMoney), additionResult?.SumPrimaryQuantity, SumTaxInMoney = sumTotalMoney.Sum(t => t.SumTaxInMoney) });
        }


        private async Task<IList<Model.PoAssignmentDetailInfo>> GetAssignementDetailInfos(IList<long?> assignmentIds)
        {
            var assignmentDetails = await _purchaseOrderDBContext.PoAssignmentDetail
               .Include(d => d.PurchasingSuggestDetail)
               .Include(d => d.PoAssignment)
               .Where(d => assignmentIds.Contains(d.PoAssignmentId))
               .ToListAsync();

            return assignmentDetails.Select(d =>
            {
                return new Model.PoAssignmentDetailInfo
                {
                    PoAssignmentId = d.PoAssignmentId,
                    PoAssignmentCode = d.PoAssignment?.PoAssignmentCode,
                    PoAssignmentDetailId = d.PoAssignmentDetailId,
                    ProductId = d.PurchasingSuggestDetail?.ProductId ?? 0,


                    ProductUnitConversionId = d.PurchasingSuggestDetail?.ProductUnitConversionId ?? 0,
                    ProductUnitConversionQuantity = d.PurchasingSuggestDetail?.ProductUnitConversionQuantity ?? 0,

                };
            }).ToIList();

        }

        private async Task<IList<Model.PurchasingSuggestDetailInfo>> GetSuggestDetailsInfos(IList<long?> suggestIds)
        {

            var suggestDetails = await _purchaseOrderDBContext.PurchasingSuggestDetail
                .Include(d => d.PurchasingSuggest)
                .Where(d => suggestIds.Contains(d.PurchasingSuggestId))
                .ToListAsync();

            return suggestDetails.Select(d =>
            {
                return new Model.PurchasingSuggestDetailInfo
                {
                    PurchasingSuggestId = d.PurchasingSuggestId,
                    PurchasingSuggestCode = d.PurchasingSuggest.PurchasingSuggestCode,
                    PurchasingSuggestDetailId = d.PurchasingSuggestDetailId,
                    ProductId = d.ProductId,
                    PrimaryQuantity = d.PrimaryQuantity,

                    ProductUnitConversionId = d.ProductUnitConversionId,
                    ProductUnitConversionQuantity = d.ProductUnitConversionQuantity,

                    OrderCode = d.OrderCode,
                    ProductionOrderCode = d.ProductionOrderCode,
                    SortOrder = d.SortOrder
                };
            }).ToList();

        }




        public async Task<PurchaseOrderOutput> GetInfo(long purchaseOrderId)
        {
            var info = await _purchaseOrderDBContext.PurchaseOrder.AsNoTracking().Where(po => po.PurchaseOrderId == purchaseOrderId).FirstOrDefaultAsync();

            if (info == null) throw new BadRequestException(PurchaseOrderErrorCode.PoNotFound);

            var details = await _purchaseOrderDBContext.PurchaseOrderDetail.AsNoTracking().Where(d => d.PurchaseOrderId == purchaseOrderId)
            .Include(x => x.PurchaseOrderDetailSubCalculation)
            .ToListAsync();


            var purchaseOrderDetailIds = details.Select(d => d.PurchaseOrderDetailId).ToList();


            var assignmentIds = details.Select(d => d.RefPoAssignmentId).ToList();

            var suggestIds = details.Where(d=>d.RefPurchasingSuggestId.HasValue).Select(d => d.RefPurchasingSuggestId.Value).ToList();


            var assignmentDetails = await GetAssignementDetailInfos(assignmentIds);

            var suggestDetails = await GetSuggestDetailInfos(suggestIds);


            var files = await _purchaseOrderDBContext.PurchaseOrderFile.AsNoTracking().Where(d => d.PurchaseOrderId == purchaseOrderId).ToListAsync();

            var excess = await _purchaseOrderDBContext.PurchaseOrderExcess.AsNoTracking().Where(d => d.PurchaseOrderId == purchaseOrderId).ProjectTo<PurchaseOrderExcessModel>(_mapper.ConfigurationProvider).ToListAsync();
            var materials = await _purchaseOrderDBContext.PurchaseOrderMaterials.AsNoTracking().Where(d => d.PurchaseOrderId == purchaseOrderId).ProjectTo<PurchaseOrderMaterialsModel>(_mapper.ConfigurationProvider).ToListAsync();

            var allocates = await _purchaseOrderDBContext.PurchaseOrderOutsourceMapping.Where(x => purchaseOrderDetailIds.Contains(x.PurchaseOrderDetailId)).ToListAsync();


            return new PurchaseOrderOutput()
            {
                PurchaseOrderId = info.PurchaseOrderId,
                PurchaseOrderCode = info.PurchaseOrderCode,
                Date = info.Date.GetUnix(),
                CustomerId = info.CustomerId,
                OtherPolicy = info.OtherPolicy,

                DeliveryDate = info.DeliveryDate?.GetUnix(),
                DeliveryUserId = info.DeliveryUserId,
                DeliveryCustomerId = info.DeliveryCustomerId,

                DeliveryDestination = info.DeliveryDestination?.JsonDeserialize<DeliveryDestinationModel>(),
                Requirement = info.Requirement,
                DeliveryPolicy = info.DeliveryPolicy,
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

                DeliveryMethod = info.DeliveryMethod,
                PaymentMethod = info.PaymentMethod,
                AttachmentBill = info.AttachmentBill,

                InputTypeSelectedState = info.InputTypeSelectedState.HasValue ? (EnumPurchaseOrderInputType)info.InputTypeSelectedState : EnumPurchaseOrderInputType.InputDefault,
                InputUnitTypeSelectedState = info.InputUnitTypeSelectedState.HasValue ? (EnumPurchaseOrderInputUnitType)info.InputUnitTypeSelectedState : null,

                FileIds = files.Select(f => f.FileId).ToList(),
                Details = details.OrderBy(d => d.SortOrder)
                .Select(d =>
                {
                    var assignmentDetailInfo = assignmentDetails.FirstOrDefault(sd => sd.ProductId == d.ProductId && sd.PoAssignmentId == d.RefPoAssignmentId);


                    var purchasingSuggestDetailInfo = suggestDetails
                                .Where(sd => sd.PurchasingSuggestId == d.RefPurchasingSuggestId && sd.ProductId == d.ProductId)
                                .OrderByDescending(sd => sd.CustomerId == info.CustomerId)
                                .FirstOrDefault();

                    return new PurchaseOrderOutputDetail()
                    {
                        PurchaseOrderDetailId = d.PurchaseOrderDetailId,
                        PoAssignmentDetailId = assignmentDetailInfo?.PoAssignmentDetailId,
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
                        IsSubCalculation = d.IsSubCalculation,
                        OutsourceMappings = allocates.Where(x => x.PurchaseOrderDetailId == d.PurchaseOrderDetailId).Select(x => new PurchaseOrderOutsourceMappingModel
                        {
                            OrderCode = x.OrderCode,
                            OutsourcePartRequestId = x.OutsourcePartRequestId,
                            ProductId = x.ProductId,
                            ProductionOrderCode = x.ProductionOrderCode,
                            ProductionStepLinkDataId = x.ProductionStepLinkDataId,
                            PurchaseOrderOutsourceMappingId = x.PurchaseOrderOutsourceMappingId,
                            PurchaseOrderDetailId = x.PurchaseOrderDetailId,
                        }).ToList(),
                    };
                }).ToList(),
                Excess = excess.OrderBy(e => e.SortOrder).ToList(),
                Materials = materials.OrderBy(m => m.SortOrder).ToList(),

            };
        }

        public async Task<long> Create(PurchaseOrderInput model)
        {

            var ctx = await GeneratePurchaseOrderCode(null, model);

            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var po = await CreateToDb(model);
                trans.Commit();

                await ctx.ConfirmCode();

                await _poActivityLog.LogBuilder(() => PurchaseOrderActivityLogMessage.Create)
                .MessageResourceFormatDatas(po.PurchaseOrderCode)
                .ObjectId(po.PurchaseOrderId)
                .JsonData(new { purchaseOrderType = EnumPurchasingOrderType.Default, model })
                .CreateLog();

                return po.PurchaseOrderId;
            }
        }

        public async Task<PurchaseOrderEntity> CreateToDb(PurchaseOrderInput model)
        {
            await ValidatePoModelInput(null, model);

            var poAssignmentDetailIds = model.Details.Where(d => d.PoAssignmentDetailId.HasValue).Select(d => d.PoAssignmentDetailId.Value).ToList();

            var poAssignmentDetails = await GetPoAssignmentDetailInfos(poAssignmentDetailIds);

            var suguestDetailIds = model.Details.Where(d => d.PurchasingSuggestDetailId.HasValue).Select(d => d.PurchasingSuggestDetailId.Value).ToList();

            var suggestDetails = await _purchaseOrderDBContext.PurchasingSuggestDetail.Where(d => suguestDetailIds.Contains(d.PurchasingSuggestDetailId)).ToListAsync();

            var po = new PurchaseOrderEntity()
            {
                PurchaseOrderCode = model.PurchaseOrderCode,
                CustomerId = model.CustomerId,
                Date = model.Date.UnixToDateTime(),
                OtherPolicy = model.OtherPolicy,
                DeliveryDate = model.DeliveryDate?.UnixToDateTime(),
                DeliveryUserId = model.DeliveryUserId,
                DeliveryCustomerId = model.DeliveryCustomerId,
                DeliveryDestination = model.DeliveryDestination.JsonSerialize(),
                Requirement = model.Requirement,
                DeliveryPolicy = model.DeliveryPolicy,
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
                ExchangeRate = model.ExchangeRate,

                DeliveryMethod = model.DeliveryMethod,
                PaymentMethod = model.PaymentMethod,
                AttachmentBill = model.AttachmentBill,
                InputTypeSelectedState = model.InputTypeSelectedState.HasValue ? (int)model.InputTypeSelectedState : (int)EnumPurchaseOrderInputType.InputDefault,
                InputUnitTypeSelectedState = model.InputUnitTypeSelectedState.HasValue ? (int)model.InputUnitTypeSelectedState : null,

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
                var suggestDetail = suggestDetails.FirstOrDefault(a => a.PurchasingSuggestDetailId == item.PurchasingSuggestDetailId);
                var eDetail = new PurchaseOrderDetail()
                {
                    PurchaseOrderId = po.PurchaseOrderId,

                    //PurchasingSuggestDetailId = item.PurchasingSuggestDetailId.HasValue ?
                    //                            item.PurchasingSuggestDetailId :
                    //                            assignmentDetail?.PurchasingSuggestDetailId,
                    RefPoAssignmentId = assignmentDetail?.PoAssignmentId,
                    //PoAssignmentDetailId = item.PoAssignmentDetailId,
                    RefPurchasingSuggestId = suggestDetail?.PurchasingSuggestId,

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

                if (item.SubCalculations != null)
                {
                    var arrEntitySubCalculation = item.SubCalculations.Select(x => new PurchaseOrderDetailSubCalculation
                    {
                        PrimaryQuantity = x.PrimaryQuantity,
                        ProductBomId = x.ProductBomId,
                        PurchaseOrderDetailId = eDetail.PurchaseOrderDetailId,
                        PrimaryUnitPrice = x.PrimaryUnitPrice,
                        UnitConversionId = x.UnitConversionId
                    });

                    await _purchaseOrderDBContext.PurchaseOrderDetailSubCalculation.AddRangeAsync(arrEntitySubCalculation);
                }

                await _purchaseOrderDBContext.SaveChangesAsync();

                if (item.OutsourceMappings.Count > 0)
                {
                    var eOutsourceMappings = item.OutsourceMappings.Select(x => new PurchaseOrderOutsourceMapping
                    {
                        OrderCode = x.OrderCode,
                        OutsourcePartRequestId = 0,
                        ProductId = x.ProductId,
                        Quantity = x.Quantity,
                        ProductionOrderCode = x.ProductionOrderCode,
                        PurchaseOrderDetailId = eDetail.PurchaseOrderDetailId
                    });
                    await _purchaseOrderDBContext.PurchaseOrderOutsourceMapping.AddRangeAsync(eOutsourceMappings);
                    await _purchaseOrderDBContext.SaveChangesAsync();
                }

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

            return po;

        }

        private async Task<IGenerateCodeContext> GeneratePurchaseOrderCode(long? purchaseOrderId, PurchaseOrderInput model)
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
            await ValidatePoModelInput(purchaseOrderId, model);

            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var info = await _purchaseOrderDBContext.PurchaseOrder.FirstOrDefaultAsync(d => d.PurchaseOrderId == purchaseOrderId);
                if (info == null) throw new BadRequestException(PurchaseOrderErrorCode.PoNotFound);

                if (model.UpdatedDatetimeUtc != info.UpdatedDatetimeUtc.GetUnix())
                {
                    throw GeneralCode.DataIsOld.BadRequest();
                }

                var poAssignmentDetailIds = model.Details.Where(d => d.PoAssignmentDetailId.HasValue).Select(d => d.PoAssignmentDetailId).ToList();

                var poAssignmentDetails = await GetPoAssignmentDetailInfos(poAssignmentDetailIds.Select(d => d.Value).ToList());

                var suguestDetailIds = model.Details.Where(d => d.PurchasingSuggestDetailId.HasValue).Select(d => d.PurchasingSuggestDetailId.Value).ToList();

                var suggestDetails = await _purchaseOrderDBContext.PurchasingSuggestDetail.Where(d => suguestDetailIds.Contains(d.PurchasingSuggestDetailId)).ToListAsync();


                info.PurchaseOrderCode = model.PurchaseOrderCode;

                info.CustomerId = model.CustomerId;
                info.Date = model.Date.UnixToDateTime();
                info.OtherPolicy = model.OtherPolicy;
                info.DeliveryDate = model.DeliveryDate?.UnixToDateTime();
                info.DeliveryUserId = model.DeliveryUserId;
                info.DeliveryCustomerId = model.DeliveryCustomerId;

                info.DeliveryDestination = model.DeliveryDestination.JsonSerialize();
                info.Requirement = model.Requirement;
                info.DeliveryPolicy = model.DeliveryPolicy;
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

                info.DeliveryMethod = model.DeliveryMethod;
                info.PaymentMethod = model.PaymentMethod;
                info.AttachmentBill = model.AttachmentBill;
                info.InputUnitTypeSelectedState = model.InputUnitTypeSelectedState.HasValue ? (int)model.InputUnitTypeSelectedState : null;
                info.InputTypeSelectedState = model.InputTypeSelectedState.HasValue ? (int)model.InputTypeSelectedState : (int)EnumPurchaseOrderInputType.InputDefault;

                if (info.DeliveryDestination?.Length > 1024)
                {
                    throw DeleveryDestinationTooLong.BadRequest();
                }


                var details = await _purchaseOrderDBContext.PurchaseOrderDetail.Where(d => d.PurchaseOrderId == purchaseOrderId).ToListAsync();


                foreach (var item in model.Details)
                {
                    var assignmentDetail = poAssignmentDetails.FirstOrDefault(a => a.PoAssignmentDetailId == item.PoAssignmentDetailId);

                    var suggestDetail = suggestDetails.FirstOrDefault(a => a.PurchasingSuggestDetailId == item.PurchasingSuggestDetailId);

                    var found = false;
                    foreach (var detail in details)
                    {

                        if (item.PurchaseOrderDetailId == detail.PurchaseOrderDetailId)
                        {
                            found = true;

                            var allocateQuantity = (await _purchaseOrderDBContext.PurchaseOrderOutsourceMapping.Where(x => x.PurchaseOrderDetailId == detail.PurchaseOrderDetailId).ToListAsync()).Sum(x => x.Quantity);

                            if (item.PrimaryQuantity < allocateQuantity)
                                throw new BadRequestException(PurchaseOrderErrorCode.PrimaryQuantityLessThanAllocateQuantity);

                            //detail.PurchasingSuggestDetailId = item.PurchasingSuggestDetailId.HasValue ?
                            //                            item.PurchasingSuggestDetailId :
                            //                            assignmentDetail?.PurchasingSuggestDetailId;

                            //detail.PoAssignmentDetailId = item.PoAssignmentDetailId;

                            detail.RefPoAssignmentId = assignmentDetail?.PoAssignmentId;
                            detail.RefPurchasingSuggestId = suggestDetail?.PurchasingSuggestId;

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

                            if (item.SubCalculations == null)
                            {
                                item.SubCalculations = new List<PurchaseOrderDetailSubCalculationModel>();
                            }

                            var arrEntitySubCalculation = _purchaseOrderDBContext.PurchaseOrderDetailSubCalculation.Where(x => x.PurchaseOrderDetailId == detail.PurchaseOrderDetailId).ToList();
                            foreach (var sub in arrEntitySubCalculation)
                            {
                                var mSub = item.SubCalculations.FirstOrDefault(x => x.PurchaseOrderDetailSubCalculationId == sub.PurchaseOrderDetailSubCalculationId);
                                if (mSub != null)
                                    _mapper.Map(mSub, sub);
                                else sub.IsDeleted = true;
                            }
                            var arrNewEntitySubCalculation = item.SubCalculations.Where(x => x.PurchaseOrderDetailSubCalculationId <= 0)
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


                            var arrAllocate = _purchaseOrderDBContext.PurchaseOrderOutsourceMapping.Where(x => x.PurchaseOrderDetailId == detail.PurchaseOrderDetailId).ToList();
                            foreach (var allocate in arrAllocate)
                            {
                                var mAllocate = item.OutsourceMappings.FirstOrDefault(x => x.PurchaseOrderOutsourceMappingId == allocate.PurchaseOrderOutsourceMappingId);
                                if (mAllocate != null)
                                    _mapper.Map(mAllocate, allocate);
                                else allocate.IsDeleted = true;
                            }
                            var arrNewEntityAllocate = item.OutsourceMappings.Where(x => x.PurchaseOrderOutsourceMappingId <= 0)
                            .Select(x => new PurchaseOrderOutsourceMapping
                            {
                                OrderCode = x.OrderCode,
                                OutsourcePartRequestId = 0,
                                ProductId = x.ProductId,
                                Quantity = x.Quantity,
                                ProductionOrderCode = x.ProductionOrderCode,
                                PurchaseOrderDetailId = detail.PurchaseOrderDetailId
                            });
                            await _purchaseOrderDBContext.PurchaseOrderOutsourceMapping.AddRangeAsync(arrNewEntityAllocate);
                            await _purchaseOrderDBContext.SaveChangesAsync();
                            break;
                        }
                    }

                    if (!found)
                    {
                        var allocateQuantity = item.OutsourceMappings.Sum(x => x.Quantity);

                        if (item.PrimaryQuantity < allocateQuantity)
                            throw new BadRequestException(PurchaseOrderErrorCode.PrimaryQuantityLessThanAllocateQuantity);

                        var eDetail = new PurchaseOrderDetail()
                        {
                            PurchaseOrderId = info.PurchaseOrderId,

                            //PurchasingSuggestDetailId = item.PurchasingSuggestDetailId.HasValue ?
                            //                        item.PurchasingSuggestDetailId :
                            //                        assignmentDetail?.PurchasingSuggestDetailId,

                            //PoAssignmentDetailId = item.PoAssignmentDetailId,

                            RefPoAssignmentId = assignmentDetail?.PoAssignmentId,
                            RefPurchasingSuggestId = suggestDetail?.PurchasingSuggestId,

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

                        if (item.OutsourceMappings.Count > 0)
                        {
                            var eOutsourceMappings = item.OutsourceMappings.Select(x => new PurchaseOrderOutsourceMapping
                            {
                                OrderCode = x.OrderCode,
                                OutsourcePartRequestId = 0,
                                ProductId = x.ProductId,
                                Quantity = x.Quantity,
                                ProductionOrderCode = x.ProductionOrderCode,
                                PurchaseOrderDetailId = eDetail.PurchaseOrderDetailId
                            });
                            await _purchaseOrderDBContext.PurchaseOrderOutsourceMapping.AddRangeAsync(eOutsourceMappings);
                            await _purchaseOrderDBContext.SaveChangesAsync();
                        }
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
                foreach (var item in allDetails.OrderBy(d => d.SortOrder))
                {
                    item.SortOrder = sortOrder++;
                }

                if (_purchaseOrderDBContext.HasChanges())
                    info.UpdatedDatetimeUtc = DateTime.UtcNow;

                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();


                await _poActivityLog.LogBuilder(() => PurchaseOrderActivityLogMessage.Update)
                   .MessageResourceFormatDatas(info.PurchaseOrderCode)
                   .ObjectId(info.PurchaseOrderId)
                   .JsonData(new { purchaseOrderType = EnumPurchasingOrderType.Default, model })
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
                   .JsonData(new { purchaseOrderType = EnumPurchasingOrderType.Default, model = info })
                   .CreateLog();



                return true;
            }
        }

        public CategoryNameModel GetFieldDataForParseMapping()
        {
            var result = new CategoryNameModel()
            {
                //CategoryId = 1,
                CategoryCode = "PurchaseOrder",
                CategoryTitle = "PurchaseOrder",
                IsTreeView = false,
                Fields = new List<CategoryFieldNameModel>()
            };
            var fields = ExcelUtils.GetFieldNameModels<PoDetailRowValue>();
            result.Fields = fields;
            return result;
        }

        public IAsyncEnumerable<PurchaseOrderInputDetail> ParseDetails(ImportExcelMapping mapping, SingleInvoiceStaticContent extra, Stream stream)
        {
            return new PurchaseOrderParseExcelFacade(_productHelperService)
                 .ParseInvoiceDetails(mapping, extra, stream);
        }


        public CategoryNameModel GetFieldDataForImportMapping()
        {
            var result = new CategoryNameModel()
            {
                //CategoryId = 1,
                CategoryCode = "PurchaseOrder",
                CategoryTitle = "PurchaseOrder",
                IsTreeView = false,
                Fields = new List<CategoryFieldNameModel>()
            };
            var fields = ExcelUtils.GetFieldNameModels<PurchaseOrderImportModel>(categoryHelper: _categoryHelperService);
            var detailProps = typeof(PoDetailRowValue).GetProperties().Select(p => p.Name).ToList();
            const string detailPrefix = "--- ";
            foreach (var f in fields)
            {
                if (detailProps.Contains(f.FieldName))
                {
                    f.GroupName = detailPrefix + f.GroupName;
                    f.FieldTitle = detailPrefix + f.FieldTitle;
                }
            }
            result.Fields = fields;
            return result;
        }

        public async Task<bool> Import(ImportExcelMapping mapping, Stream stream)
        {
            return await _purchaseOrderImportExcelFacadeService.Import(mapping, stream, this);
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
                   .JsonData((new { purchaseOrderId }))
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
                  .JsonData((new { purchaseOrderId }))
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
                   .JsonData((new { purchaseOrderId }))
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
                  .JsonData((new { purchaseOrderId }))
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
                  .JsonData((new { purchaseOrderId }))
                  .CreateLog();

                await _notificationFactoryService.AddSubscription(new SubscriptionSimpleModel
                {
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
                 .JsonData((new { purchaseOrderId, poProcessStatusId }))
                 .CreateLog();



                return true;
            }
        }

        public async Task<IDictionary<long, IList<PurchaseOrderOutputBasic>>> GetPurchaseOrderBySuggest(IList<long> purchasingSuggestIds)
        {
            var refPurchasingSuggestIds = purchasingSuggestIds?.Select(s => (long?)s).ToList();

            var poDetail = await (
                from s in _purchaseOrderDBContext.PurchaseOrder
                join sd in _purchaseOrderDBContext.PurchaseOrderDetail on s.PurchaseOrderId equals sd.PurchaseOrderId
                //join r in _purchaseOrderDBContext.PurchasingSuggestDetail on sd.PurchasingSuggestDetailId equals r.PurchasingSuggestDetailId
                where refPurchasingSuggestIds.Contains(sd.RefPurchasingSuggestId)
                orderby sd.SortOrder
                select new
                {
                    sd.RefPurchasingSuggestId,
                    s.PurchaseOrderId,
                    s.PurchaseOrderCode
                }).ToListAsync();

            return purchasingSuggestIds.Distinct()
                .ToDictionary(
                r => r,
                r => (IList<PurchaseOrderOutputBasic>)poDetail.Where(d => d.RefPurchasingSuggestId == r).Select(d => new PurchaseOrderOutputBasic
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
            var refPoAssignmentIds = poAssignmentIds?.Select(s => (long?)s).ToList();

            var poDetail = await (
                from s in _purchaseOrderDBContext.PurchaseOrder
                join sd in _purchaseOrderDBContext.PurchaseOrderDetail on s.PurchaseOrderId equals sd.PurchaseOrderId
                //join r in _purchaseOrderDBContext.PoAssignmentDetail on sd.PoAssignmentDetailId equals r.PoAssignmentDetailId
                where refPoAssignmentIds.Contains(sd.RefPoAssignmentId)
                orderby sd.SortOrder
                select new
                {
                    sd.RefPoAssignmentId,
                    s.PurchaseOrderId,
                    s.PurchaseOrderCode
                }).ToListAsync();

            return poAssignmentIds.Distinct()
                .ToDictionary(
                r => r,
                r => (IList<PurchaseOrderOutputBasic>)poDetail.Where(d => d.RefPoAssignmentId == r).Select(d => new PurchaseOrderOutputBasic
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
            catch (System.Exception)
            {
                await trans.RollbackAsync();
                throw;
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

        public async Task<IList<EnrichDataPurchaseOrderAllocate>> EnrichDataForPurchaseOrderAllocate(long purchaseOrderId)
        {
            var purchaseOrder = await _purchaseOrderDBContext.PurchaseOrder.FirstOrDefaultAsync(x => x.PurchaseOrderId == purchaseOrderId);
            if (purchaseOrder == null) return new List<EnrichDataPurchaseOrderAllocate>();

            var queryRefPurchaseOrderOutsource = from pd in _purchaseOrderDBContext.PurchaseOrderDetail
                                                 join m in _purchaseOrderDBContext.PurchaseOrderOutsourceMapping on pd.PurchaseOrderDetailId equals m.PurchaseOrderDetailId
                                                 where pd.PurchaseOrderId == purchaseOrderId
                                                 select new
                                                 {
                                                     m.PurchaseOrderOutsourceMappingId,
                                                     pd.PurchaseOrderId,
                                                     pd.PurchaseOrderDetailId,
                                                     OutsourceRequestId = m.OutsourcePartRequestId,
                                                     ProductionOrderCode = m.ProductionOrderCode,
                                                     m.ProductionStepLinkDataId
                                                 };

            var data = new List<EnrichDataPurchaseOrderAllocate>();

            if (purchaseOrder.PurchaseOrderType == (int)EnumPurchasingOrderType.OutsourcePart)
            {
                var queryRefOutsourcePart = _purchaseOrderDBContext.RefOutsourcePartRequest.AsQueryable();

                var query = from v in queryRefPurchaseOrderOutsource
                            join r in queryRefOutsourcePart on v.OutsourceRequestId equals r.OutsourcePartRequestId
                            select new EnrichDataPurchaseOrderAllocate
                            {
                                PurchaseOrderOutsourceMappingId = v.PurchaseOrderOutsourceMappingId,
                                PurchaseOrderId = v.PurchaseOrderId,
                                PurchaseOrderDetailId = v.PurchaseOrderDetailId,
                                OutsourceRequestId = v.OutsourceRequestId,
                                ProductionOrderCode = v.ProductionOrderCode,
                                OutsourceRequestCode = r.OutsourcePartRequestCode,
                                ProductionOrderId = r.ProductionOrderId
                            };
                data = await query.ToListAsync();
            }
            else if (purchaseOrder.PurchaseOrderType == (int)EnumPurchasingOrderType.OutsourceStep)
            {
                var queryRefOutsourceStep = _purchaseOrderDBContext.RefOutsourceStepRequest.AsQueryable();

                var query = from v in queryRefPurchaseOrderOutsource
                            join r in queryRefOutsourceStep on new { v.OutsourceRequestId, ProductionStepLinkDataId = v.ProductionStepLinkDataId.GetValueOrDefault() } equals new { OutsourceRequestId = r.OutsourceStepRequestId, r.ProductionStepLinkDataId }
                            select new EnrichDataPurchaseOrderAllocate
                            {
                                PurchaseOrderOutsourceMappingId = v.PurchaseOrderOutsourceMappingId,
                                PurchaseOrderId = v.PurchaseOrderId,
                                PurchaseOrderDetailId = v.PurchaseOrderDetailId,
                                OutsourceRequestId = v.OutsourceRequestId,
                                ProductionOrderCode = v.ProductionOrderCode,
                                OutsourceRequestCode = r.OutsourceStepRequestCode,
                                ProductionOrderId = r.ProductionOrderId
                            };
                data = await query.ToListAsync();
            }
            else
            {
                var queryRefProductionOrder = _purchaseOrderDBContext.RefProductionOrder.AsQueryable();

                var query = from v in queryRefPurchaseOrderOutsource
                            join r in queryRefProductionOrder on v.ProductionOrderCode equals r.ProductionOrderCode into gr
                            from r in gr.DefaultIfEmpty()
                            select new EnrichDataPurchaseOrderAllocate
                            {
                                PurchaseOrderOutsourceMappingId = v.PurchaseOrderOutsourceMappingId,
                                PurchaseOrderId = v.PurchaseOrderId,
                                PurchaseOrderDetailId = v.PurchaseOrderDetailId,
                                ProductionOrderCode = v.ProductionOrderCode,
                                ProductionOrderId = r.ProductionOrderId
                            };
                data = await query.ToListAsync();
            }

            return data;
        }

        private async Task ValidatePoModelInput(long? poId, PurchaseOrderInput model)
        {
            if (!string.IsNullOrEmpty(model.PurchaseOrderCode))
            {
                var existedItem = await _purchaseOrderDBContext.PurchaseOrder.AsNoTracking().FirstOrDefaultAsync(r => r.PurchaseOrderCode == model.PurchaseOrderCode && r.PurchaseOrderId != poId);
                if (existedItem != null)
                {
                    throw PurchaseOrderErrorCode.PoCodeAlreadyExisted.BadRequest(PurchaseOrderErrorCodeDescription.PoCodeAlreadyExisted + " " + model.PurchaseOrderCode);
                }
            }
            //else
            //{
            //    return PurchaseOrderErrorCode.PoCodeAlreadyExisted;
            //}


            PurchaseOrderEntity poInfo = null;

            if (poId.HasValue)
            {
                poInfo = await _purchaseOrderDBContext.PurchaseOrder.AsNoTracking().FirstOrDefaultAsync(r => r.PurchaseOrderId == poId.Value);
                if (poInfo == null)
                {
                    throw PurchaseOrderErrorCode.PoNotFound.BadRequest();
                }
            }

            if (model.Details.Where(d => d.PoAssignmentDetailId.HasValue).GroupBy(d => d.PoAssignmentDetailId).Any(d => d.Count() > 1))
            {
                throw GeneralCode.InvalidParams.BadRequest();
            }

            if (model.Details.Where(d => d.PurchasingSuggestDetailId.HasValue).GroupBy(d => d.PurchasingSuggestDetailId).Any(d => d.Count() > 1))
            {
                throw GeneralCode.InvalidParams.BadRequest();
            }

            var validateAssignment = await ValidateAssignmentDetails(poId, model);

            if (!validateAssignment.IsSuccess()) throw validateAssignment.BadRequest();

            var validateSuggest = await ValidateSuggestDetails(poId, model);

            if (!validateSuggest.IsSuccess()) throw validateSuggest.BadRequest();

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

            var suggestDetailIds = model.Details.Where(d => d.PurchasingSuggestDetailId.HasValue).Select(d => d.PurchasingSuggestDetailId.Value).ToList();

            var suggestDetails = await GetSuggestDetailInfos(suggestDetailIds.ToList());

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

        private async Task<IList<VErp.Services.PurchaseOrder.Model.PurchasingSuggestDetailInfo>> GetSuggestDetailInfos(IList<long> suggestDetailIds)
        {
            return await (
             from pd in _purchaseOrderDBContext.PurchasingSuggestDetail
             join sd in _purchaseOrderDBContext.PurchasingSuggest on pd.PurchasingSuggestId equals sd.PurchasingSuggestId
             where suggestDetailIds.Contains(pd.PurchasingSuggestDetailId)
             select new VErp.Services.PurchaseOrder.Model.PurchasingSuggestDetailInfo
             {
                 PurchasingSuggestId = pd.PurchasingSuggestId,
                 PurchasingSuggestCode = pd.PurchasingSuggest.PurchasingSuggestCode,
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
