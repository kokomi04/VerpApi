using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Verp.Resources.PurchaseOrder.Po;
using VErp.Commons.Enums.ErrorCodes.PO;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.MasterEnum.PO;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.PurchaseOrderDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.PurchaseOrder.Model;
using static Verp.Resources.PurchaseOrder.Po.PurchaseOrderOutsourceValidationMessage;
using PurchaseOrderModel = VErp.Infrastructure.EF.PurchaseOrderDB.PurchaseOrder;

namespace VErp.Services.PurchaseOrder.Service.Implement
{

    public abstract class PurchaseOrderOutsourceAbstract
    {
        protected PurchaseOrderDBContext _purchaseOrderDBContext;
        protected AppSetting _appSetting;
        protected ILogger _logger;
        private readonly ObjectActivityLogFacade _poActivityLog;
        protected ICurrentContextService _currentContext;
        protected ICustomGenCodeHelperService _customGenCodeHelperService;
        protected IManufacturingHelperService _manufacturingHelperService;
        protected IMapper _mapper;

        public PurchaseOrderOutsourceAbstract(
            PurchaseOrderDBContext purchaseOrderDBContext,
            IOptions<AppSetting> appSetting,
            ILogger logger,
            IActivityLogService activityLogService,
            ICurrentContextService currentContext,
            ICustomGenCodeHelperService customGenCodeHelperService,
            IManufacturingHelperService manufacturingHelperService,
            IMapper mapper)
        {
            _purchaseOrderDBContext = purchaseOrderDBContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _poActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.PurchaseOrder);
            _currentContext = currentContext;
            _customGenCodeHelperService = customGenCodeHelperService;
            _manufacturingHelperService = manufacturingHelperService;
            _mapper = mapper;
        }

        protected async Task<long> CreatePurchaseOrderOutsource(PurchaseOrderInput model, EnumPurchasingOrderType purchaseOrderType)
        {
            var validate = await ValidateModelInput(null, model);

            if (!validate.IsSuccess())
            {
                throw new BadRequestException(validate);
            }

            var ctx = await GeneratePurchaseOrderCode(null, model);

            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var po = new PurchaseOrderModel()
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
                        IsApproved = null,
                        IsChecked = null,
                        PoProcessStatusId = null,
                        DeliveryFee = model.DeliveryFee,
                        OtherFee = model.OtherFee,
                        TotalMoney = model.TotalMoney,
                        CensorByUserId = null,
                        CensorDatetimeUtc = null,
                        PurchaseOrderType = (int)purchaseOrderType,
                        PropertyCalcId = model.PropertyCalcId,
                        PoDescription = model.PoDescription,
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
                    foreach (var detail in model.Details.OrderBy(d=>d.SortOrder))
                    {
                        var entityDetail = new PurchaseOrderDetail()
                        {
                            PurchaseOrderId = po.PurchaseOrderId,

                            ProductId = detail.ProductId,

                            ProviderProductName = detail.ProviderProductName,
                            PrimaryQuantity = detail.PrimaryQuantity,
                            PrimaryUnitPrice = detail.PrimaryUnitPrice,

                            ProductUnitConversionId = detail.ProductUnitConversionId,
                            ProductUnitConversionQuantity = detail.ProductUnitConversionQuantity,
                            ProductUnitConversionPrice = detail.ProductUnitConversionPrice,

                            PoProviderPricingCode = detail.PoProviderPricingCode,
                            OrderCode = detail.OrderCode,
                            ProductionOrderCode = detail.ProductionOrderCode,
                            Description = detail.Description,

                            OutsourceRequestId = detail.OutsourceRequestId,
                            ProductionStepLinkDataId = detail.ProductionStepLinkDataId,
                            IntoMoney = detail.IntoMoney,

                            ExchangedMoney = detail.ExchangedMoney,
                            SortOrder = sortOrder++,
                            IsSubCalculation = detail.IsSubCalculation
                        };

                        await _purchaseOrderDBContext.PurchaseOrderDetail.AddAsync(entityDetail);
                        await _purchaseOrderDBContext.SaveChangesAsync();

                        if (detail.OutsourceMappings.Count > 0)
                        {
                            var eOutsourceMappings = detail.OutsourceMappings.Select(x => new PurchaseOrderOutsourceMapping
                            {
                                OrderCode = x.OrderCode,
                                OutsourcePartRequestId = x.OutsourcePartRequestId,
                                ProductId = x.ProductId,
                                Quantity = x.Quantity,
                                ProductionOrderCode = x.ProductionOrderCode,
                                PurchaseOrderDetailId = entityDetail.PurchaseOrderDetailId,
                                ProductionStepLinkDataId = x.ProductionStepLinkDataId
                            });
                            await _purchaseOrderDBContext.PurchaseOrderOutsourceMapping.AddRangeAsync(eOutsourceMappings);
                            await _purchaseOrderDBContext.SaveChangesAsync();
                        }
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

                    if (model.Excess?.Count > 0)
                    {
                        foreach (var excess in model.Excess)
                            excess.PurchaseOrderId = po.PurchaseOrderId;

                        var lst = _mapper.Map<IList<PurchaseOrderExcess>>(model.Excess).OrderBy(s => s.SortOrder).ToList();
                        sortOrder = 1;
                        foreach (var item in lst)
                        {
                            item.SortOrder = sortOrder++;
                        }
                        await _purchaseOrderDBContext.PurchaseOrderExcess.AddRangeAsync(lst);
                    }

                    if (model.Materials?.Count > 0)
                    {
                        foreach (var material in model.Materials)
                            material.PurchaseOrderId = po.PurchaseOrderId;

                        var lst = _mapper.Map<IList<PurchaseOrderMaterials>>(model.Materials).OrderBy(s => s.SortOrder).ToList();
                        sortOrder = 1;
                        foreach (var item in lst)
                        {
                            item.SortOrder = sortOrder++;
                        }
                        await _purchaseOrderDBContext.PurchaseOrderMaterials.AddRangeAsync(lst);
                    }



                    await _purchaseOrderDBContext.SaveChangesAsync();

                    await CreatePurchaseOrderTracked(po.PurchaseOrderId);

                    trans.Commit();

                    await UpdateStatusForOutsourceRequestInPurcharOrder(po.PurchaseOrderId, purchaseOrderType);


                    await ctx.ConfirmCode();

                    await _poActivityLog.LogBuilder(() => PurchaseOrderActivityLogMessage.Create)
                    .MessageResourceFormatDatas(po.PurchaseOrderCode)
                    .ObjectId(po.PurchaseOrderId)
                    .JsonData((new { purchaseOrderType, model }).JsonSerialize())
                    .CreateLog();

                    return po.PurchaseOrderId;
                }
                catch (Exception)
                {
                    await trans.RollbackAsync();
                    throw;
                }
            }

        }

        protected async Task<bool> UpdatePurchaseOrderOutsource(long purchaseOrderId, PurchaseOrderInput model)
        {
            var validate = await ValidateModelInput(purchaseOrderId, model);

            if (!validate.IsSuccess())
            {
                throw new BadRequestException(validate);
            }

            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var info = await _purchaseOrderDBContext.PurchaseOrder.FirstOrDefaultAsync(d => d.PurchaseOrderId == purchaseOrderId);
                    if (info == null) throw new BadRequestException(PurchaseOrderErrorCode.PoNotFound);

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
                    info.CensorByUserId = null;
                    info.CensorDatetimeUtc = null;
                    info.PoDescription = model.PoDescription;
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
                        var found = false;
                        foreach (var detail in details)
                        {

                            if (item.PurchaseOrderDetailId == detail.PurchaseOrderDetailId)
                            {
                                found = true;

                                var allocateQuantity = (await _purchaseOrderDBContext.PurchaseOrderOutsourceMapping.Where(x => x.PurchaseOrderDetailId == detail.PurchaseOrderDetailId).ToListAsync()).Sum(x => x.Quantity);

                                if (item.PrimaryQuantity < allocateQuantity)
                                    throw new BadRequestException(PurchaseOrderErrorCode.PrimaryQuantityLessThanAllocateQuantity);

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
                                detail.IntoMoney = item.IntoMoney;

                                detail.ExchangedMoney = item.ExchangedMoney;

                                detail.SortOrder = item.SortOrder;
                                detail.IsSubCalculation = item.IsSubCalculation;

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
                                    OutsourcePartRequestId = x.OutsourcePartRequestId,
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

                                OutsourceRequestId = item.OutsourceRequestId,
                                ProductionStepLinkDataId = item.ProductionStepLinkDataId,
                                IntoMoney = item.IntoMoney,

                                ExchangedMoney = item.ExchangedMoney,
                                SortOrder = item.SortOrder,
                                IsSubCalculation = item.IsSubCalculation
                            };

                            await _purchaseOrderDBContext.PurchaseOrderDetail.AddAsync(eDetail);
                            await _purchaseOrderDBContext.SaveChangesAsync();

                            if (item.OutsourceMappings.Count > 0)
                            {
                                var eOutsourceMappings = item.OutsourceMappings.Select(x => new PurchaseOrderOutsourceMapping
                                {
                                    OrderCode = x.OrderCode,
                                    OutsourcePartRequestId = x.OutsourcePartRequestId,
                                    ProductId = x.ProductId,
                                    Quantity = x.Quantity,
                                    ProductionOrderCode = x.ProductionOrderCode,
                                    PurchaseOrderDetailId = eDetail.PurchaseOrderDetailId,
                                    ProductionStepLinkDataId = x.ProductionStepLinkDataId
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

                    //materials
                    var materials = await _purchaseOrderDBContext.PurchaseOrderMaterials
                                                            .Where(x => x.PurchaseOrderId == info.PurchaseOrderId)
                                                            .ToListAsync();

                    foreach (var m in materials)
                    {
                        var s = model.Materials.FirstOrDefault(x => x.PurchaseOrderMaterialsId == m.PurchaseOrderMaterialsId);
                        if (s != null)
                            _mapper.Map(s, m);
                        else m.IsDeleted = true;
                    }

                    var newMaterials = model.Materials
                        .AsQueryable()
                        .Where(x => x.PurchaseOrderMaterialsId <= 0)
                        .ProjectTo<PurchaseOrderMaterials>(_mapper.ConfigurationProvider)
                        .ToList();
                    newMaterials.ForEach(x => x.PurchaseOrderId = info.PurchaseOrderId);

                    await _purchaseOrderDBContext.PurchaseOrderMaterials.AddRangeAsync(newMaterials);
                    await _purchaseOrderDBContext.SaveChangesAsync();

                    //excesses
                    var excesses = await _purchaseOrderDBContext.PurchaseOrderExcess.Where(x => x.PurchaseOrderId == info.PurchaseOrderId)
                        .ToListAsync();

                    foreach (var m in excesses)
                    {
                        var s = model.Excess.FirstOrDefault(x => x.PurchaseOrderExcessId == m.PurchaseOrderExcessId);
                        if (s != null)
                            _mapper.Map(s, m);
                        else m.IsDeleted = true;
                    }

                    var newExcesses = model.Excess
                        .AsQueryable()
                        .Where(x => x.PurchaseOrderExcessId <= 0)
                        .ProjectTo<PurchaseOrderExcess>(_mapper.ConfigurationProvider)
                        .ToList();
                    newExcesses.ForEach(x => x.PurchaseOrderId = info.PurchaseOrderId);

                    await _purchaseOrderDBContext.PurchaseOrderExcess.AddRangeAsync(newExcesses);
                    await _purchaseOrderDBContext.SaveChangesAsync();

                    var allDetails = await _purchaseOrderDBContext.PurchaseOrderDetail.Where(d => d.PurchaseOrderId == purchaseOrderId).ToListAsync();
                    var allExcesses = await _purchaseOrderDBContext.PurchaseOrderExcess.Where(d => d.PurchaseOrderId == purchaseOrderId).ToListAsync();
                    var allMaterials = await _purchaseOrderDBContext.PurchaseOrderMaterials.Where(d => d.PurchaseOrderId == purchaseOrderId).ToListAsync();

                    var sortOrder = 1;
                    foreach (var item in allDetails.OrderBy(d=>d.SortOrder))
                    {
                        item.SortOrder = sortOrder++;
                    }

                    sortOrder = 1;
                    foreach (var item in allExcesses.OrderBy(d => d.SortOrder))
                    {
                        item.SortOrder = sortOrder++;
                    }

                    sortOrder = 1;
                    foreach (var item in allMaterials.OrderBy(d => d.SortOrder))
                    {
                        item.SortOrder = sortOrder++;
                    }

                    await _purchaseOrderDBContext.SaveChangesAsync();

                    trans.Commit();

                    await UpdateStatusForOutsourceRequestInPurcharOrder(purchaseOrderId, (EnumPurchasingOrderType)info.PurchaseOrderType);


                    await _poActivityLog.LogBuilder(() => PurchaseOrderActivityLogMessage.Update)
                      .MessageResourceFormatDatas(info.PurchaseOrderCode)
                      .ObjectId(info.PurchaseOrderId)
                      .JsonData((new { purchaseOrderType = info.PurchaseOrderType, model }).JsonSerialize())
                      .CreateLog();

                    return true;

                }
                catch (Exception)
                {
                    await trans.RollbackAsync();
                    throw;
                }
            }
        }

        protected async Task<bool> DeletePurchaseOrderOutsource(long purchaseOrderId)
        {
            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var info = await _purchaseOrderDBContext.PurchaseOrder.FirstOrDefaultAsync(d => d.PurchaseOrderId == purchaseOrderId);
                    if (info == null) throw new BadRequestException(PurchaseOrderErrorCode.PoNotFound);

                    var oldDetails = await _purchaseOrderDBContext.PurchaseOrderDetail.Where(d => d.PurchaseOrderId == purchaseOrderId).ToListAsync();
                    var oldExcess = await _purchaseOrderDBContext.PurchaseOrderExcess.Where(d => d.PurchaseOrderId == purchaseOrderId).ToListAsync();
                    var oldMaterials = await _purchaseOrderDBContext.PurchaseOrderMaterials.Where(d => d.PurchaseOrderId == purchaseOrderId).ToListAsync();

                    info.IsDeleted = true;

                    foreach (var item in oldDetails)
                    {
                        item.IsDeleted = true;
                    }

                    foreach (var item in oldExcess)
                    {
                        item.IsDeleted = true;
                    }

                    foreach (var item in oldMaterials)
                    {
                        item.IsDeleted = true;
                    }

                    await _purchaseOrderDBContext.SaveChangesAsync();

                    trans.Commit();

                    var outsourceRequestId = oldDetails
                    .Select(x => x.OutsourceRequestId.GetValueOrDefault())
                    .Distinct()
                    .Where(x => x > 0)
                    .ToArray();

                    if (outsourceRequestId.Length > 0 && info.PurchaseOrderType == (int)EnumPurchasingOrderType.OutsourcePart)
                        return await _manufacturingHelperService.UpdateOutsourcePartRequestStatus(outsourceRequestId);

                    if (outsourceRequestId.Length > 0 && info.PurchaseOrderType == (int)EnumPurchasingOrderType.OutsourceStep)
                        return await _manufacturingHelperService.UpdateOutsourceStepRequestStatus(outsourceRequestId);


                    await _poActivityLog.LogBuilder(() => PurchaseOrderActivityLogMessage.Delete)
                        .MessageResourceFormatDatas(info.PurchaseOrderCode)
                        .ObjectId(info.PurchaseOrderId)
                        .JsonData((new { purchaseOrderType = info.PurchaseOrderType, model = info }).JsonSerialize())
                        .CreateLog();

                    return true;

                }
                catch (Exception)
                {
                    await trans.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<PurchaseOrderOutput> GetPurchaseOrderOutsource(long purchaseOrderId)
        {
            var info = await _purchaseOrderDBContext.PurchaseOrder.AsNoTracking().Where(po => po.PurchaseOrderId == purchaseOrderId).FirstOrDefaultAsync();

            if (info == null) throw new BadRequestException(PurchaseOrderErrorCode.PoNotFound);

            var details = await _purchaseOrderDBContext.PurchaseOrderDetail.AsNoTracking().Where(d => d.PurchaseOrderId == purchaseOrderId).ToListAsync();
            var files = await _purchaseOrderDBContext.PurchaseOrderFile.AsNoTracking().Where(d => d.PurchaseOrderId == purchaseOrderId).ToListAsync();
            var excess = await _purchaseOrderDBContext.PurchaseOrderExcess.AsNoTracking().Where(d => d.PurchaseOrderId == purchaseOrderId).ProjectTo<PurchaseOrderExcessModel>(_mapper.ConfigurationProvider).ToListAsync();
            var materials = await _purchaseOrderDBContext.PurchaseOrderMaterials.AsNoTracking().Where(d => d.PurchaseOrderId == purchaseOrderId).ProjectTo<PurchaseOrderMaterialsModel>(_mapper.ConfigurationProvider).ToListAsync();

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
                CreatedByUserId = info.CreatedByUserId,
                UpdatedByUserId = info.UpdatedByUserId,
                CheckedByUserId = info.CheckedByUserId,
                CensorByUserId = info.CensorByUserId,

                CreatedDatetimeUtc = info.CreatedDatetimeUtc.GetUnix(),
                UpdatedDatetimeUtc = info.UpdatedDatetimeUtc.GetUnix(),
                CheckedDatetimeUtc = info.CheckedDatetimeUtc.GetUnix(),
                CensorDatetimeUtc = info.CensorDatetimeUtc.GetUnix(),

                PurchaseOrderType = info.PurchaseOrderType,

                PropertyCalcId = info.PropertyCalcId,

                TaxInPercent = info.TaxInPercent,
                TaxInMoney = info.TaxInMoney,

                CurrencyId = info.CurrencyId,
                ExchangeRate = info.ExchangeRate,

                FileIds = files.Select(f => f.FileId).ToList(),
                Details = details.OrderBy(d => d.SortOrder)
                .Select(d =>
                {
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

                        OutsourceRequestId = d.OutsourceRequestId,
                        ProductionStepLinkDataId = d.ProductionStepLinkDataId,
                        IntoMoney = d.IntoMoney,

                        ExchangedMoney = d.ExchangedMoney,
                        SortOrder = d.SortOrder
                    };
                }).ToList(),
                Excess = excess.OrderBy(e => e.SortOrder).ToList(),
                Materials = materials.OrderBy(m => m.SortOrder).ToList()
            };
        }

        protected async Task<long[]> GetAllOutsourceRequestIdInPurchaseOrder(long purchaseOrderId)
        {
            var info = await _purchaseOrderDBContext.PurchaseOrder.AsNoTracking().FirstOrDefaultAsync(d => d.PurchaseOrderId == purchaseOrderId);
            if (info == null) throw new BadRequestException(PurchaseOrderErrorCode.PoNotFound);

            var outsourceRequestId = _purchaseOrderDBContext.PurchaseOrderDetail.Where(x => x.PurchaseOrderId == purchaseOrderId)
                .Select(x => x.OutsourceRequestId.GetValueOrDefault())
                .Distinct()
                .Where(x => x > 0)
                .ToArray();
            return outsourceRequestId;
        }

        private async Task<bool> UpdateStatusForOutsourceRequestInPurcharOrder(long purchaseOrderId, EnumPurchasingOrderType purchaseOrderType)
        {
            var outsourceRequestId = await GetAllOutsourceRequestIdInPurchaseOrder(purchaseOrderId);

            if (outsourceRequestId.Length > 0 && purchaseOrderType == EnumPurchasingOrderType.OutsourcePart)
                return await _manufacturingHelperService.UpdateOutsourcePartRequestStatus(outsourceRequestId);

            if (outsourceRequestId.Length > 0 && purchaseOrderType == EnumPurchasingOrderType.OutsourceStep)
                return await _manufacturingHelperService.UpdateOutsourceStepRequestStatus(outsourceRequestId);

            return true;
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

        protected virtual async Task<Enum> ValidateModelInput(long? poId, PurchaseOrderInput model)
        {
            return await Task.FromResult(GeneralCode.InternalError);
        }

        private async Task<bool> CreatePurchaseOrderTracked(long purchaseOrderId)
        {
            var track = new purchaseOrderTrackedModel
            {
                Date = DateTime.UtcNow.GetUnixUtc(_currentContext.TimeZoneOffset),
                Description = "Tạo đơn hàng gia công",
                Status = EnumPurchaseOrderTrackStatus.Created,
                PurchaseOrderId = purchaseOrderId,
            };

            await _purchaseOrderDBContext.PurchaseOrderTracked.AddAsync(_mapper.Map<PurchaseOrderTracked>(track));
            await _purchaseOrderDBContext.SaveChangesAsync();

            return true;
        }

    }
}