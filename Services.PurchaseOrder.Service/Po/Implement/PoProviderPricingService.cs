using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Verp.Resources.PurchaseOrder.PoProviderPricing;
using VErp.Commons.Enums.ErrorCodes.PO;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.MasterEnum.PO;
using VErp.Commons.Enums.PO;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.EF.PurchaseOrderDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.PurchaseOrder.Model;
using VErp.Services.PurchaseOrder.Model.PoProviderPricing;
using VErp.Services.PurchaseOrder.Service.Po;
using VErp.Services.PurchaseOrder.Service.Po.Implement.Facade;
using static Verp.Resources.PurchaseOrder.PoProviderPricing.PoProviderPricingValidationMessage;
using PoProviderPricingEntity = VErp.Infrastructure.EF.PurchaseOrderDB.PoProviderPricing;

namespace VErp.Services.PoProviderPricing.Service.Implement
{


    public class PoProviderPricingService : IPoProviderPricingService
    {
        private readonly PurchaseOrderDBContext _purchaseOrderDBContext;
        private readonly ICurrentContextService _currentContext;
        private readonly ICustomGenCodeHelperService _customGenCodeHelperService;
        private readonly IProductHelperService _productHelperService;
        private readonly IMapper _mapper;
        private readonly ObjectActivityLogFacade _poActivityLog;

        public PoProviderPricingService(
            PurchaseOrderDBContext purchaseOrderDBContext
           , IActivityLogService activityLogService
           , ICurrentContextService currentContext
           , ICustomGenCodeHelperService customGenCodeHelperService
           , IProductHelperService productHelperService
           , IMapper mapper
           )
        {
            _purchaseOrderDBContext = purchaseOrderDBContext;
            _poActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.PoProviderPricing);
            _currentContext = currentContext;
            _customGenCodeHelperService = customGenCodeHelperService;
            _productHelperService = productHelperService;
            _mapper = mapper;
        }

        public async Task<PageData<PoProviderPricingOutputList>> GetList(string keyword, int? customerId, IList<int> productIds, EnumPoProviderPricingStatus? poProviderPricingStatusId, EnumPoProcessStatus? poProcessStatusId, bool? isChecked, bool? isApproved, long? fromDate, long? toDate, string sortBy, bool asc, int page, int size)
        {
            keyword = keyword?.Trim();

            var poQuery = _purchaseOrderDBContext.PoProviderPricing.AsQueryable();
            if (customerId > 0)
            {
                poQuery = poQuery.Where(po => po.CustomerId == customerId);
            }
            var query = from po in poQuery
                        join d in _purchaseOrderDBContext.PoProviderPricingDetail on po.PoProviderPricingId equals d.PoProviderPricingId
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
                            po.PoProviderPricingId,
                            po.PoProviderPricingCode,
                            po.Date,
                            po.CustomerId,
                            po.DeliveryDestination,
                            po.Content,
                            po.AdditionNote,
                            po.DeliveryFee,
                            po.OtherFee,
                            po.TotalMoney,
                            po.PoProviderPricingStatusId,
                            po.IsChecked,
                            po.IsApproved,
                            po.PoProcessStatusId,
                            po.PoProviderPricingDescription,
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
                            CensorFullName = censor.FullName
                        };
            if (!string.IsNullOrWhiteSpace(keyword))
            {

                query = query
                   .Where(q => q.PoProviderPricingCode.Contains(keyword)
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


            if (poProviderPricingStatusId.HasValue)
            {
                query = query.Where(q => q.PoProviderPricingStatusId == (int)poProviderPricingStatusId.Value);
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


            var poQueryDistinct = _purchaseOrderDBContext.PoProviderPricing.Where(po => query.Select(p => p.PoProviderPricingId).Contains(po.PoProviderPricingId));

            var total = await poQueryDistinct.CountAsync();
            var additionResult = await (from q in poQueryDistinct
                                        group q by 1 into g
                                        select new
                                        {
                                            SumTotalMoney = g.Sum(x => x.TotalMoney),
                                        }).FirstOrDefaultAsync();
            var pagedData = await poQuery.SortByFieldName(sortBy, asc).Skip((page - 1) * size).Take(size).ToListAsync();
            var result = _mapper.Map<List<PoProviderPricingOutputList>>(pagedData);

            return (result, total, additionResult);
        }

        public async Task<PageData<PoProviderPricingOutputListByProduct>> GetListByProduct(string keyword, int? customerId, IList<string> codes, IList<int> productIds, EnumPoProviderPricingStatus? poProviderPricingStatusId, EnumPoProcessStatus? poProcessStatusId, bool? isChecked, bool? isApproved, long? fromDate, long? toDate, string sortBy, bool asc, int page, int size)
        {
            keyword = (keyword ?? "").Trim();

            var poQuery = _purchaseOrderDBContext.PoProviderPricing.AsQueryable();
            if (codes?.Count > 0)
            {
                poQuery = poQuery.Where(po => codes.Contains(po.PoProviderPricingCode));

            }
            if (customerId > 0)
            {
                poQuery = poQuery.Where(po => po.CustomerId == customerId);
            }
            var query = from po in poQuery
                        join pod in _purchaseOrderDBContext.PoProviderPricingDetail on po.PoProviderPricingId equals pod.PoProviderPricingId
                        join p in _purchaseOrderDBContext.RefProduct on pod.ProductId equals p.ProductId into ps
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
                            po.PoProviderPricingId,
                            po.PoProviderPricingCode,
                            po.Date,
                            po.CustomerId,
                            po.DeliveryDestination,
                            po.Content,
                            po.AdditionNote,
                            po.DeliveryFee,
                            po.OtherFee,
                            po.TotalMoney,
                            po.PoProviderPricingStatusId,
                            po.IsChecked,
                            po.IsApproved,
                            po.PoProcessStatusId,
                            po.PoProviderPricingDescription,

                            po.CreatedByUserId,
                            po.UpdatedByUserId,
                            po.CheckedByUserId,
                            po.CensorByUserId,

                            po.CreatedDatetimeUtc,
                            po.UpdatedDatetimeUtc,
                            po.CheckedDatetimeUtc,
                            po.CensorDatetimeUtc,

                            //detail
                            pod.PoProviderPricingDetailId,


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

                            pod.OrderCode,
                            pod.ProductionOrderCode,

                            pod.SortOrder,


                            c.CustomerCode,
                            c.CustomerName,

                            p.ProductCode,
                            p.ProductName,

                            po.DeliveryDate,

                            CreatorFullName = creator.FullName,
                            CheckerFullName = checker.FullName,
                            CensorFullName = censor.FullName,

                            pod.IntoMoney,

                            po.CurrencyId,
                            pod.ExchangedMoney,
                            po.ExchangeRate
                        };

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query
                    .Where(q => q.PoProviderPricingCode.Contains(keyword)
                    || q.Content.Contains(keyword)
                    || q.AdditionNote.Contains(keyword)
                    || q.CustomerCode.Contains(keyword)
                    || q.CustomerName.Contains(keyword)
                    || q.ProductCode.Contains(keyword)
                    || q.ProductName.Contains(keyword)
                    || q.OrderCode.Contains(keyword)
                    || q.ProductionOrderCode.Contains(keyword)
                    || q.CreatorFullName.Contains(keyword)
                    || q.CheckerFullName.Contains(keyword)
                    || q.CensorFullName.Contains(keyword)
                    );
            }

            if (poProviderPricingStatusId.HasValue)
            {
                query = query.Where(q => q.PoProviderPricingStatusId == (int)poProviderPricingStatusId.Value);
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
                                        group q by q.PoProviderPricingCode into g
                                        select new
                                        {
                                            TotalMoney = g.Sum(x => x.TotalMoney) / g.Count()
                                        }).ToListAsync()).Sum(x => x.TotalMoney);



            var result = new List<PoProviderPricingOutputListByProduct>();
            foreach (var info in pagedData)
            {
                result.Add(new PoProviderPricingOutputListByProduct()
                {
                    PoProviderPricingId = info.PoProviderPricingId,
                    PoProviderPricingCode = info.PoProviderPricingCode,
                    Date = info.Date.GetUnix(),
                    CustomerId = info.CustomerId,
                    DeliveryDestination = info.DeliveryDestination?.JsonDeserialize<DeliveryDestinationModel>(),
                    Content = info.Content,
                    AdditionNote = info.AdditionNote,
                    DeliveryFee = info.DeliveryFee,
                    OtherFee = info.OtherFee,
                    TotalMoney = info.TotalMoney,
                    PoProviderPricingStatusId = (EnumPoProviderPricingStatus)info.PoProviderPricingStatusId,
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

                    DeliveryDate = info.DeliveryDate.GetUnix(),
                    CreatorFullName = info.CreatorFullName,
                    CheckerFullName = info.CheckerFullName,
                    CensorFullName = info.CensorFullName,
                    IntoMoney = info.IntoMoney,

                    CurrencyId = info.CurrencyId,
                    ExchangedMoney = info.ExchangedMoney,
                    ExchangeRate = info.ExchangeRate,
                    SortOrder = info.SortOrder
                });
            }
            return (result, total, new { SumTotalMoney = sumTotalMoney, additionResult.SumPrimaryQuantity, additionResult.SumTaxInMoney });
        }


        public async Task<PoProviderPricingModel> GetInfo(long poProviderPricingId)
        {
            var info = await _purchaseOrderDBContext.PoProviderPricing.AsNoTracking().Where(po => po.PoProviderPricingId == poProviderPricingId).FirstOrDefaultAsync();

            if (info == null) throw new BadRequestException(PurchaseOrderErrorCode.PoNotFound);

            var details = await _purchaseOrderDBContext.PoProviderPricingDetail.AsNoTracking().Where(d => d.PoProviderPricingId == poProviderPricingId).ToListAsync();



            var files = await _purchaseOrderDBContext.PoProviderPricingFile.AsNoTracking().Where(d => d.PoProviderPricingId == poProviderPricingId).ToListAsync();
            var result = _mapper.Map<PoProviderPricingModel>(info);

            result.FileIds = files.Select(f => f.FileId).ToList();
            result.Details = _mapper.Map<List<PoProviderPricingOutputDetail>>(details.OrderBy(d => d.SortOrder).ToList());

            return result;
        }

        public async Task<long> Create(PoProviderPricingModel model)
        {
            var validate = await ValidatePoProviderPriceModelInput(null, model);

            if (!validate.IsSuccess())
            {
                throw new BadRequestException(validate);
            }

            var ctx = await GeneratePoProviderPricingCode(null, model);

            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var po = _mapper.Map<PoProviderPricingEntity>(model);

                po.PoProviderPricingStatusId = (int)EnumPoProviderPricingStatus.Draff;
                po.IsApproved = null;
                po.IsChecked = null;
                po.PoProcessStatusId = null;
                po.CensorByUserId = null;

                if (po.DeliveryDestination?.Length > 1024)
                {
                    throw DeleveryDestinationTooLong.BadRequest();
                }

                await _purchaseOrderDBContext.AddAsync(po);
                await _purchaseOrderDBContext.SaveChangesAsync();

                var poDetails = model.Details.Select(d =>
                {
                    var detail = _mapper.Map<PoProviderPricingDetail>(d);
                    detail.PoProviderPricingId = po.PoProviderPricingId;
                    return detail;
                }).ToList();

                var sortOrder = 1;
                foreach (var item in poDetails)
                {
                    item.SortOrder = sortOrder++;
                }

                await _purchaseOrderDBContext.PoProviderPricingDetail.AddRangeAsync(poDetails);


                if (model.FileIds?.Count > 0)
                {
                    await _purchaseOrderDBContext.PoProviderPricingFile.AddRangeAsync(model.FileIds.Select(f => new PoProviderPricingFile()
                    {
                        PoProviderPricingId = po.PoProviderPricingId,
                        FileId = f
                    }));
                }


                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();


                await ctx.ConfirmCode();

                await _poActivityLog.LogBuilder(() => PoProviderPricingActivityLogMessage.Create)
                  .MessageResourceFormatDatas(po.PoProviderPricingCode)
                  .ObjectId(po.PoProviderPricingId)
                  .JsonData(model.JsonSerialize())
                  .CreateLog();
                return po.PoProviderPricingId;
            }

        }

        private async Task<GenerateCodeContext> GeneratePoProviderPricingCode(long? poProviderPricingId, PoProviderPricingModel model)
        {
            model.PoProviderPricingCode = (model.PoProviderPricingCode ?? "").Trim();

            var ctx = _customGenCodeHelperService.CreateGenerateCodeContext();

            var code = await ctx
                .SetConfig(EnumObjectType.PoProviderPricing)
                .SetConfigData(poProviderPricingId ?? 0, model.Date)
                .TryValidateAndGenerateCode(_purchaseOrderDBContext.PoProviderPricing, model.PoProviderPricingCode, (s, code) => s.PoProviderPricingId != poProviderPricingId && s.PoProviderPricingCode == code);

            model.PoProviderPricingCode = code;

            return ctx;
        }

        public async Task<bool> Update(long poProviderPricingId, PoProviderPricingModel model)
        {
            var validate = await ValidatePoProviderPriceModelInput(poProviderPricingId, model);

            if (!validate.IsSuccess())
            {
                throw new BadRequestException(validate);
            }

            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var info = await _purchaseOrderDBContext.PoProviderPricing.FirstOrDefaultAsync(d => d.PoProviderPricingId == poProviderPricingId);
                if (info == null) throw new BadRequestException(PurchaseOrderErrorCode.PoNotFound);

                _mapper.Map(model, info);

                info.PoProviderPricingStatusId = (int)EnumPoProviderPricingStatus.Draff;
                info.IsChecked = null;
                info.IsApproved = null;
                info.PoProcessStatusId = null;
                info.CensorByUserId = null;
                info.CensorDatetimeUtc = null;
                info.IsDeleted = false;
                info.DeletedDatetimeUtc = null;

                if (info.DeliveryDestination?.Length > 1024)
                {
                    throw DeleveryDestinationTooLong.BadRequest();
                }


                var details = await _purchaseOrderDBContext.PoProviderPricingDetail.Where(d => d.PoProviderPricingId == poProviderPricingId).ToListAsync();

                var newDetails = new List<PoProviderPricingDetail>();

                foreach (var item in model.Details)
                {

                    var found = false;
                    foreach (var detail in details)
                    {
                        if (item.PoProviderPricingDetailId == detail.PoProviderPricingDetailId)
                        {
                            _mapper.Map(item, detail);
                            detail.PoProviderPricingId = info.PoProviderPricingId;
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        var detail = _mapper.Map<PoProviderPricingDetail>(item);
                        detail.PoProviderPricingId = info.PoProviderPricingId;
                        newDetails.Add(detail);
                    }
                }

                var updatedIds = model.Details.Select(d => d.PoProviderPricingDetailId).ToList();

                var deleteDetails = details.Where(d => !updatedIds.Contains(d.PoProviderPricingDetailId));

                foreach (var detail in deleteDetails)
                {
                    detail.IsDeleted = true;
                    detail.DeletedDatetimeUtc = DateTime.UtcNow;
                }

                await _purchaseOrderDBContext.PoProviderPricingDetail.AddRangeAsync(newDetails);

                var oldFiles = await _purchaseOrderDBContext.PoProviderPricingFile.Where(f => f.PoProviderPricingId == info.PoProviderPricingId).ToListAsync();

                if (oldFiles.Count > 0)
                {
                    _purchaseOrderDBContext.PoProviderPricingFile.RemoveRange(oldFiles);
                }

                if (model.FileIds?.Count > 0)
                {
                    await _purchaseOrderDBContext.PoProviderPricingFile.AddRangeAsync(model.FileIds.Select(f => new PoProviderPricingFile()
                    {
                        PoProviderPricingId = info.PoProviderPricingId,
                        FileId = f
                    }));
                }

                await _purchaseOrderDBContext.SaveChangesAsync();

                var allDetails = await _purchaseOrderDBContext.PoProviderPricingDetail.Where(d => d.PoProviderPricingId == poProviderPricingId).ToListAsync();
                var sortOrder = 1;
                foreach (var item in allDetails)
                {
                    item.SortOrder = sortOrder++;
                }

                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();


                await _poActivityLog.LogBuilder(() => PoProviderPricingActivityLogMessage.Update)
                   .MessageResourceFormatDatas(info.PoProviderPricingCode)
                   .ObjectId(info.PoProviderPricingId)
                   .JsonData(model.JsonSerialize())
                   .CreateLog();
                return true;
            }
        }



        public async Task<bool> Delete(long poProviderPricingId)
        {
            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var info = await _purchaseOrderDBContext.PoProviderPricing.FirstOrDefaultAsync(d => d.PoProviderPricingId == poProviderPricingId);
                if (info == null) throw GeneralCode.ItemNotFound.BadRequest();


                var oldDetails = await _purchaseOrderDBContext.PoProviderPricingDetail.Where(d => d.PoProviderPricingId == poProviderPricingId).ToListAsync();

                info.IsDeleted = true;
                info.DeletedDatetimeUtc = DateTime.UtcNow;

                foreach (var item in oldDetails)
                {
                    item.IsDeleted = true;
                    item.DeletedDatetimeUtc = DateTime.UtcNow;
                }

                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _poActivityLog.LogBuilder(() => PoProviderPricingActivityLogMessage.Delete)
                   .MessageResourceFormatDatas(info.PoProviderPricingCode)
                   .ObjectId(info.PoProviderPricingId)
                   .JsonData(info.JsonSerialize())
                   .CreateLog();

                return true;
            }
        }

        public async Task<bool> Checked(long purchaseOrderId)
        {
            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var info = await _purchaseOrderDBContext.PoProviderPricing.FirstOrDefaultAsync(d => d.PoProviderPricingId == purchaseOrderId);
                if (info == null) throw new BadRequestException(PurchaseOrderErrorCode.PoNotFound);

                if (info.PoProviderPricingStatusId == (int)EnumPoProviderPricingStatus.Checked && info.IsChecked == true)
                {
                    throw AlreadyChecked.BadRequest();
                }

                if (info.PoProviderPricingStatusId != (int)EnumPoProviderPricingStatus.WaitToCensor
                    && info.PoProviderPricingStatusId != (int)EnumPoProviderPricingStatus.Checked)
                {
                    throw NotSentToCensorYet.BadRequest();
                }

                info.IsChecked = true;

                info.PoProviderPricingStatusId = (int)EnumPoProviderPricingStatus.Checked;
                info.CheckedDatetimeUtc = DateTime.Now.Date.GetUnixUtc(_currentContext.TimeZoneOffset).UnixToDateTime();
                info.CheckedByUserId = _currentContext.UserId;

                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _poActivityLog.LogBuilder(() => PoProviderPricingActivityLogMessage.CheckApprove)
                   .MessageResourceFormatDatas(info.PoProviderPricingCode)
                   .ObjectId(info.PoProviderPricingId)
                   .JsonData((new { purchaseOrderId }).JsonSerialize())
                   .CreateLog();

                return true;
            }
        }

        public async Task<bool> RejectCheck(long purchaseOrderId)
        {
            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var info = await _purchaseOrderDBContext.PoProviderPricing.FirstOrDefaultAsync(d => d.PoProviderPricingId == purchaseOrderId);
                if (info == null) throw new BadRequestException(PurchaseOrderErrorCode.PoNotFound);

                if (info.PoProviderPricingStatusId == (int)EnumPoProviderPricingStatus.Checked && info.IsChecked == false)
                {
                    throw HasBeenFailAtCheck.BadRequest();
                }

                if (info.PoProviderPricingStatusId != (int)EnumPoProviderPricingStatus.WaitToCensor
                    && info.PoProviderPricingStatusId != (int)EnumPoProviderPricingStatus.Checked)
                {
                    throw NotSentToCensorYet.BadRequest();
                }

                info.IsChecked = false;

                info.PoProviderPricingStatusId = (int)EnumPoProviderPricingStatus.Checked;
                info.CheckedDatetimeUtc = DateTime.Now.Date.GetUnixUtc(_currentContext.TimeZoneOffset).UnixToDateTime();
                info.CheckedByUserId = _currentContext.UserId;

                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();


                await _poActivityLog.LogBuilder(() => PoProviderPricingActivityLogMessage.CheckReject)
                  .MessageResourceFormatDatas(info.PoProviderPricingCode)
                  .ObjectId(info.PoProviderPricingId)
                  .JsonData((new { purchaseOrderId }).JsonSerialize())
                  .CreateLog();
                return true;
            }
        }

        public async Task<bool> Approve(long purchaseOrderId)
        {
            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var info = await _purchaseOrderDBContext.PoProviderPricing.FirstOrDefaultAsync(d => d.PoProviderPricingId == purchaseOrderId);
                if (info == null) throw new BadRequestException(PurchaseOrderErrorCode.PoNotFound);

                if (info.PoProviderPricingStatusId != (int)EnumPoProviderPricingStatus.Censored && info.IsApproved == true)
                {
                    throw AlreadyApproved.BadRequest();
                }

                if (info.IsChecked != true)
                {
                    throw NotPassCheckYet.BadRequest();
                }

                if (info.PoProviderPricingStatusId != (int)EnumPoProviderPricingStatus.Censored
                    && info.PoProviderPricingStatusId != (int)EnumPoProviderPricingStatus.Checked)
                {
                    throw NotSentToCensorYet.BadRequest();
                }

                info.IsApproved = true;

                info.PoProviderPricingStatusId = (int)EnumPoProviderPricingStatus.Censored;
                info.CensorDatetimeUtc = DateTime.Now.Date.GetUnixUtc(_currentContext.TimeZoneOffset).UnixToDateTime();
                info.CensorByUserId = _currentContext.UserId;

                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();


                await _poActivityLog.LogBuilder(() => PoProviderPricingActivityLogMessage.CensorApprove)
                   .MessageResourceFormatDatas(info.PoProviderPricingCode)
                   .ObjectId(info.PoProviderPricingId)
                   .JsonData((new { purchaseOrderId }).JsonSerialize())
                   .CreateLog();

                return true;
            }
        }

        public async Task<bool> Reject(long purchaseOrderId)
        {
            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var info = await _purchaseOrderDBContext.PoProviderPricing.FirstOrDefaultAsync(d => d.PoProviderPricingId == purchaseOrderId);
                if (info == null) throw new BadRequestException(PurchaseOrderErrorCode.PoNotFound);

                if (info.PoProviderPricingStatusId != (int)EnumPoProviderPricingStatus.Censored && info.IsApproved == false)
                {
                    throw AlreadyRejected.BadRequest();
                }

                if (info.IsChecked != true)
                {
                    throw NotPassCheckYet.BadRequest();
                }

                if (info.PoProviderPricingStatusId != (int)EnumPoProviderPricingStatus.Censored
                   && info.PoProviderPricingStatusId != (int)EnumPoProviderPricingStatus.Checked)
                {
                    throw NotSentToCensorYet.BadRequest();
                }


                info.IsApproved = false;

                info.PoProviderPricingStatusId = (int)EnumPoProviderPricingStatus.Censored;
                info.CensorDatetimeUtc = DateTime.Now.Date.GetUnixUtc(_currentContext.TimeZoneOffset).UnixToDateTime();
                info.CensorByUserId = _currentContext.UserId;

                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();


                await _poActivityLog.LogBuilder(() => PoProviderPricingActivityLogMessage.CensorReject)
                  .MessageResourceFormatDatas(info.PoProviderPricingCode)
                  .ObjectId(info.PoProviderPricingId)
                  .JsonData((new { purchaseOrderId }).JsonSerialize())
                  .CreateLog();
                return true;
            }
        }

        public async Task<bool> SentToCensor(long purchaseOrderId)
        {
            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var info = await _purchaseOrderDBContext.PoProviderPricing.FirstOrDefaultAsync(d => d.PoProviderPricingId == purchaseOrderId);
                if (info == null) throw new BadRequestException(PurchaseOrderErrorCode.PoNotFound);

                if (info.PoProviderPricingStatusId != (int)EnumPoProviderPricingStatus.Draff)
                {
                    throw new BadRequestException(GeneralCode.InvalidParams);
                }

                info.IsChecked = null;
                info.IsApproved = null;
                info.PoProviderPricingStatusId = (int)EnumPoProviderPricingStatus.WaitToCensor;
                info.UpdatedDatetimeUtc = DateTime.UtcNow;
                info.UpdatedByUserId = _currentContext.UserId;


                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _poActivityLog.LogBuilder(() => PoProviderPricingActivityLogMessage.SendToCensor)
                  .MessageResourceFormatDatas(info.PoProviderPricingCode)
                  .ObjectId(info.PoProviderPricingId)
                  .JsonData((new { purchaseOrderId }).JsonSerialize())
                  .CreateLog();
                return true;
            }
        }


        public async Task<bool> UpdatePoProcessStatus(long purchaseOrderId, EnumPoProcessStatus poProcessStatusId)
        {
            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var info = await _purchaseOrderDBContext.PoProviderPricing.FirstOrDefaultAsync(d => d.PoProviderPricingId == purchaseOrderId);
                if (info == null) throw new BadRequestException(PurchaseOrderErrorCode.PoNotFound);

                info.PoProcessStatusId = (int)poProcessStatusId;

                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _poActivityLog.LogBuilder(() => PoProviderPricingActivityLogMessage.UpdatePoProcessStatus)
                 .MessageResourceFormatDatas(poProcessStatusId, info.PoProviderPricingCode)
                 .ObjectId(info.PoProviderPricingId)
                 .JsonData((new { purchaseOrderId, poProcessStatusId }).JsonSerialize())
                 .CreateLog();

                return true;
            }
        }


        private async Task<Enum> ValidatePoProviderPriceModelInput(long? poId, PoProviderPricingModel model)
        {
            if (!string.IsNullOrEmpty(model.PoProviderPricingCode))
            {
                var existedItem = await _purchaseOrderDBContext.PoProviderPricing.AsNoTracking().FirstOrDefaultAsync(r => r.PoProviderPricingCode == model.PoProviderPricingCode && r.PoProviderPricingId != poId);
                if (existedItem != null) return PurchaseOrderErrorCode.PoCodeAlreadyExisted;
            }
            //else
            //{
            //    return PurchaseOrderErrorCode.PoCodeAlreadyExisted;
            //}

            PoProviderPricingEntity poInfo = null;

            if (poId.HasValue)
            {
                poInfo = await _purchaseOrderDBContext.PoProviderPricing.AsNoTracking().FirstOrDefaultAsync(r => r.PoProviderPricingId == poId.Value);
                if (poInfo == null)
                {
                    return PurchaseOrderErrorCode.PoNotFound;
                }
            }

            return GeneralCode.Success;
        }



        public CategoryNameModel GetFieldDataForMapping()
        {
            var result = new CategoryNameModel()
            {
                //CategoryId = 1,
                CategoryCode = "PoPricing",
                CategoryTitle = "PoPricing",
                IsTreeView = false,
                Fields = new List<CategoryFieldNameModel>()
            };
            var fields = ExcelUtils.GetFieldNameModels<PoPricingDetailRow>();
            result.Fields = fields;
            return result;
        }

        public IAsyncEnumerable<PoProviderPricingOutputDetail> ParseDetails(ImportExcelMapping mapping, Stream stream)
        {
            return new PoProviderPricingParseExcelFacade(_productHelperService)
                 .ParseInvoiceDetails(mapping, stream);
        }


    }
}
