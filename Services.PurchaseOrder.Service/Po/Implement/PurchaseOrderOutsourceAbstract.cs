using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.ErrorCodes.PO;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.MasterEnum.PO;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.PurchaseOrderDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Master.Service.Config;
using VErp.Services.PurchaseOrder.Model;
using PurchaseOrderModel = VErp.Infrastructure.EF.PurchaseOrderDB.PurchaseOrder;

namespace VErp.Services.PurchaseOrder.Service.Implement
{
   
    public abstract class PurchaseOrderOutsourceAbstract
    {
        protected PurchaseOrderDBContext _purchaseOrderDBContext;
        protected AppSetting _appSetting;
        protected ILogger _logger;
        protected IActivityLogService _activityLogService;
        protected ICurrentContextService _currentContext;
        protected IObjectGenCodeService _objectGenCodeService;
        protected ICustomGenCodeHelperService _customGenCodeHelperService;
        protected IManufacturingHelperService _manufacturingHelperService;
        protected IMapper _mapper;

        public PurchaseOrderOutsourceAbstract(
            PurchaseOrderDBContext purchaseOrderDBContext,
            IOptions<AppSetting> appSetting,
            ILogger logger,
            IActivityLogService activityLogService,
            ICurrentContextService currentContext,
            IObjectGenCodeService objectGenCodeService,
            ICustomGenCodeHelperService customGenCodeHelperService,
            IManufacturingHelperService manufacturingHelperService,
            IMapper mapper)
        {
            _purchaseOrderDBContext = purchaseOrderDBContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            _activityLogService = activityLogService;
            _currentContext = currentContext;
            _objectGenCodeService = objectGenCodeService;
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
                        PaymentInfo = model.PaymentInfo,
                        DeliveryDate = model.DeliveryDate?.UnixToDateTime(),
                        DeliveryUserId = model.DeliveryUserId,
                        DeliveryCustomerId = model.DeliveryCustomerId,
                        DeliveryDestination = model.DeliveryDestination.JsonSerialize(),
                        Content = model.Content,
                        AdditionNote = model.AdditionNote,
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
                        PoDescription = model.PoDescription
                    };

                    if (po.DeliveryDestination?.Length > 1024)
                    {
                        throw new BadRequestException(GeneralCode.InvalidParams, "Thông tin liên hệ giao hàng quá dài");
                    }

                    await _purchaseOrderDBContext.AddAsync(po);
                    await _purchaseOrderDBContext.SaveChangesAsync();

                    var poDetails = model.Details.Select(d =>
                    {

                        return new PurchaseOrderDetail()
                        {
                            PurchaseOrderId = po.PurchaseOrderId,

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

                            OutsourceRequestId = d.OutsourceRequestId,
                            ProductionStepLinkDataId = d.ProductionStepLinkDataId,
                            IntoMoney = d.IntoMoney,
                            IntoAfterTaxMoney = d.IntoAfterTaxMoney,
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

                    if (model.Excess?.Count > 0)
                    {
                        foreach (var excess in model.Excess)
                            excess.PurchaseOrderId = po.PurchaseOrderId;

                        await _purchaseOrderDBContext.PurchaseOrderExcess.AddRangeAsync(_mapper.Map<IList<PurchaseOrderExcess>>(model.Excess));
                    }

                    if (model.Materials?.Count > 0)
                    {
                        foreach (var material in model.Materials)
                            material.PurchaseOrderId = po.PurchaseOrderId;

                        await _purchaseOrderDBContext.PurchaseOrderMaterials.AddRangeAsync(_mapper.Map<IList<PurchaseOrderMaterials>>(model.Materials));
                    }


                    await _purchaseOrderDBContext.SaveChangesAsync();

                    await CreatePurchaseOrderTracked(po.PurchaseOrderId);

                    trans.Commit();

                    await UpdateStatusForOutsourceRequestInPurcharOrder(po.PurchaseOrderId, purchaseOrderType);

                    await _activityLogService.CreateLog(EnumObjectType.PurchaseOrder, po.PurchaseOrderId, $"Tạo PO {po.PurchaseOrderCode}", model.JsonSerialize());

                    await ctx.ConfirmCode();

                    return po.PurchaseOrderId;
                }
                catch (Exception ex)
                {
                    await trans.RollbackAsync();
                    throw ex;
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
                    info.CensorByUserId = null;
                    info.CensorDatetimeUtc = null;
                    info.PoDescription = model.PoDescription;

                    if (info.DeliveryDestination?.Length > 1024)
                    {
                        throw new BadRequestException(GeneralCode.InvalidParams, "Thông tin liên hệ giao hàng quá dài");
                    }


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
                                detail.IntoMoney = item.IntoMoney;
                                detail.IntoAfterTaxMoney = item.IntoAfterTaxMoney;

                                break;
                            }
                        }

                        if (!found)
                        {
                            newDetails.Add(new PurchaseOrderDetail()
                            {
                                PurchaseOrderId = info.PurchaseOrderId,

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

                                OutsourceRequestId = item.OutsourceRequestId,
                                ProductionStepLinkDataId = item.ProductionStepLinkDataId,
                                IntoMoney = item.IntoMoney,
                                IntoAfterTaxMoney = item.IntoAfterTaxMoney,
                            });
                        }
                    }

                    var updatedIds = model.Details.Select(d => d.PurchaseOrderDetailId).ToList();

                    var deleteDetails = details.Where(d => !updatedIds.Contains(d.PurchaseOrderDetailId));

                    foreach (var detail in deleteDetails)
                    {
                        detail.IsDeleted = true;
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

                    trans.Commit();
                    
                    await UpdateStatusForOutsourceRequestInPurcharOrder(purchaseOrderId, (EnumPurchasingOrderType)info.PurchaseOrderType);

                    await _activityLogService.CreateLog(EnumObjectType.PurchaseOrder, purchaseOrderId, $"Cập nhật PO {info.PurchaseOrderCode}", info.JsonSerialize());

                    return true;

                }
                catch (Exception ex)
                {
                    await trans.RollbackAsync();
                    throw ex;
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

                    await _activityLogService.CreateLog(EnumObjectType.PurchaseOrder, purchaseOrderId, $"Xóa PO {info.PurchaseOrderCode}", info.JsonSerialize());

                    return true;

                }
                catch (Exception ex)
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

                PurchaseOrderType = info.PurchaseOrderType,

                PropertyCalcId = info.PropertyCalcId,

                FileIds = files.Select(f => f.FileId).ToList(),
                Details = details.Select(d =>
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

                        TaxInPercent = d.TaxInPercent,
                        TaxInMoney = d.TaxInMoney,
                        OrderCode = d.OrderCode,
                        ProductionOrderCode = d.ProductionOrderCode,
                        Description = d.Description,

                        OutsourceRequestId = d.OutsourceRequestId,
                        ProductionStepLinkDataId = d.ProductionStepLinkDataId,
                        IntoAfterTaxMoney = d.IntoAfterTaxMoney,
                        IntoMoney = d.IntoMoney
                    };
                }).ToList(),
                Excess = excess,
                Materials = materials
            };
        }

        protected async Task<long[]> GetAllOutsourceRequestIdInPurchaseOrder(long purchaseOrderId){
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

        protected virtual async Task<Enum> ValidateModelInput(long? poId, PurchaseOrderInput model)
        {
            return GeneralCode.InternalError;
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