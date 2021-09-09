
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
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using AutoMapper.QueryableExtensions;
using AutoMapper;
using System.IO;
using VErp.Services.PurchaseOrder.Model.Request;
using VErp.Commons.GlobalObject.InternalDataInterface;
using Org.BouncyCastle.Ocsp;
using Verp.Cache.RedisCache;
using VErp.Infrastructure.ServiceCore.Facade;
using System.Linq.Expressions;
using VErp.Commons.ObjectExtensions.Extensions;
using Verp.Resources.PurchaseOrder.PurchasingRequest;
using VErp.Commons.Library.Model;
using VErp.Services.PurchaseOrder.Service.Po.Implement.Facade;

namespace VErp.Services.PurchaseOrder.Service.Implement
{
    public class PurchasingRequestService : IPurchasingRequestService
    {
        private readonly PurchaseOrderDBContext _purchaseOrderDBContext;
        private readonly AppSetting _appSetting;
        private readonly ILogger _logger;
        //private readonly IActivityLogService _activityLogService;
        private readonly IAsyncRunnerService _asyncRunner;
        private readonly ICurrentContextService _currentContext;
        private readonly IProductHelperService _productHelperService;
        private readonly IMapper _mapper;
        private readonly ICustomGenCodeHelperService _customGenCodeHelperService;
        private readonly ObjectActivityLogFacade _purchasingRequestActivityLog;

        public PurchasingRequestService(
            PurchaseOrderDBContext purchaseOrderDBContext
           , IOptions<AppSetting> appSetting
           , ILogger<PurchasingRequestService> logger
           , IActivityLogService activityLogService
           , IAsyncRunnerService asyncRunner
           , ICurrentContextService currentContext
            , IProductHelperService productHelperService
            , IMapper mapper
            , ICustomGenCodeHelperService customGenCodeHelperService
           )
        {
            _purchaseOrderDBContext = purchaseOrderDBContext;
            _appSetting = appSetting.Value;
            _logger = logger;
            //_activityLogService = activityLogService;
            _asyncRunner = asyncRunner;
            _currentContext = currentContext;
            _productHelperService = productHelperService;
            _mapper = mapper;
            _customGenCodeHelperService = customGenCodeHelperService;
            _purchasingRequestActivityLog = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.PurchasingRequest);
        }


        public async Task<PurchasingRequestOutput> GetInfo(long purchasingRequestId)
        {
            var info = await _purchaseOrderDBContext
                .PurchasingRequest
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.PurchasingRequestId == purchasingRequestId);

            if (info == null) throw PurchasingRequestErrorCode.RequestNotFound.BadRequest();

            var details = await _purchaseOrderDBContext.PurchasingRequestDetail.AsNoTracking()
                .Where(d => d.PurchasingRequestId == purchasingRequestId)
                .ToListAsync();

            var data = _mapper.Map<PurchasingRequestOutput>(info);

            data.Details = details.Select(d => _mapper.Map<PurchasingRequestOutputDetail>(d)).ToList();

            return data;
        }

        public async Task<PurchasingRequestOutput> GetByOrderDetailId(long orderDetailId)
        {
            var info = await _purchaseOrderDBContext
                .PurchasingRequest
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.OrderDetailId == orderDetailId && r.PurchasingRequestTypeId == (int)EnumPurchasingRequestType.OrderMaterial);

            if (info == null) return null;

            var details = await _purchaseOrderDBContext.PurchasingRequestDetail.AsNoTracking()
                .Where(d => d.PurchasingRequestId == info.PurchasingRequestId)
                .ToListAsync();

            var data = _mapper.Map<PurchasingRequestOutput>(info);

            data.Details = details.Select(d => _mapper.Map<PurchasingRequestOutputDetail>(d)).ToList();

            return data;
        }


        public async Task<PageData<PurchasingRequestOutputList>> GetList(string keyword, IList<int> productIds, EnumPurchasingRequestStatus? purchasingRequestStatusId, EnumPoProcessStatus? poProcessStatusId, bool? isApproved, long? fromDate, long? toDate, string sortBy, bool asc, int page, int size)
        {
            keyword = keyword?.Trim();

            var query = _purchaseOrderDBContext.PurchasingRequest.AsNoTracking().AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query
                    .Where(q => q.PurchasingRequestCode.Contains(keyword)
                    || q.Content.Contains(keyword));
            }

            if (productIds != null && productIds.Count > 0)
            {
                var purchasingRequestIds = (
                    from d in _purchaseOrderDBContext.PurchasingRequestDetail
                    where productIds.Contains(d.ProductId)
                    select d.PurchasingRequestId
                 ).Distinct();

                query = query.Where(q => purchasingRequestIds.Contains(q.PurchasingRequestId));
            }

            if (purchasingRequestStatusId.HasValue)
            {
                query = query.Where(q => q.PurchasingRequestStatusId == (int)purchasingRequestStatusId.Value);
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
                query = query.Where(q => q.CreatedDatetimeUtc >= time);
            }

            if (toDate.HasValue)
            {
                var time = toDate.Value.UnixToDateTime();
                time = time.Value.AddDays(1);
                query = query.Where(q => q.CreatedDatetimeUtc < time);
            }

            var total = await query.CountAsync();
            var pagedData = await query.SortByFieldName(sortBy, asc).Skip((page - 1) * size).Take(size).ToListAsync();
            var result = new List<PurchasingRequestOutputList>();
            foreach (var info in pagedData)
            {
                result.Add(_mapper.Map<PurchasingRequestOutputList>(info));
            }

            return (result, total);

        }


        public async Task<PageData<PurchasingRequestOutputListByProduct>> GetListByProduct(string keyword, IList<int> productIds, EnumPurchasingRequestStatus? purchasingRequestStatusId, EnumPoProcessStatus? poProcessStatusId, bool? isApproved, long? fromDate, long? toDate, string sortBy, bool asc, int page, int size)
        {
            keyword = (keyword ?? "").Trim();

            var query = from r in _purchaseOrderDBContext.PurchasingRequest
                        join d in _purchaseOrderDBContext.PurchasingRequestDetail on r.PurchasingRequestId equals d.PurchasingRequestId
                        select new
                        {
                            r.PurchasingRequestId,
                            r.PurchasingRequestStatusId,
                            r.PurchasingRequestTypeId,
                            r.Date,
                            d.OrderCode,
                            d.ProductionOrderCode,
                            r.PurchasingRequestCode,
                            r.Content,
                            r.PoProcessStatusId,
                            r.IsApproved,
                            r.CreatedDatetimeUtc,
                            r.CreatedByUserId,
                            r.UpdatedByUserId,
                            r.UpdatedDatetimeUtc,
                            r.CensorByUserId,
                            r.CensorDatetimeUtc,
                            d.PurchasingRequestDetailId,
                            d.ProductId,
                            d.PrimaryQuantity,
                            d.ProductUnitConversionId,
                            d.ProductUnitConversionQuantity,
                            d.Description
                        };

            if (productIds != null && productIds.Count > 0)
            {
                query = query.Where(q => productIds.Contains(q.ProductId));
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query
                    .Where(q => q.OrderCode.Contains(keyword)
                    || q.ProductionOrderCode.Contains(keyword)
                    || q.PurchasingRequestCode.Contains(keyword)
                    || q.Content.Contains(keyword));
            }

            if (purchasingRequestStatusId.HasValue)
            {
                query = query.Where(q => q.PurchasingRequestStatusId == (int)purchasingRequestStatusId.Value);
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
                query = query.Where(q => q.CreatedDatetimeUtc >= time);
            }

            if (toDate.HasValue)
            {
                var toDateTime = toDate.Value.UnixToDateTime();
                toDateTime = toDateTime.Value.AddDays(1);
                query = query.Where(q => q.CreatedDatetimeUtc < toDateTime);
            }


            var total = await query.CountAsync();
            var pagedData = await query.SortByFieldName(sortBy, asc).Skip((page - 1) * size).Take(size).ToListAsync();
            var result = new List<PurchasingRequestOutputListByProduct>();
            foreach (var info in pagedData)
            {
                result.Add(new PurchasingRequestOutputListByProduct()
                {
                    PurchasingRequestId = info.PurchasingRequestId,
                    PurchasingRequestCode = info.PurchasingRequestCode,
                    Date = info.Date.GetUnix(),
                    OrderCode = info.OrderCode,
                    ProductionOrderCode = info.ProductionOrderCode,
                    PurchasingRequestStatusId = (EnumPurchasingRequestStatus)info.PurchasingRequestStatusId,
                    IsApproved = info.IsApproved,
                    PoProcessStatusId = (EnumPoProcessStatus?)info.PoProcessStatusId,
                    CreatedByUserId = info.CreatedByUserId,
                    UpdatedByUserId = info.UpdatedByUserId,
                    CensorByUserId = info.CensorByUserId,

                    CensorDatetimeUtc = info.CensorDatetimeUtc?.GetUnix(),
                    CreatedDatetimeUtc = info.CreatedDatetimeUtc.GetUnix(),
                    UpdatedDatetimeUtc = info.UpdatedDatetimeUtc.GetUnix(),

                    PurchasingRequestDetailId = info.PurchasingRequestDetailId,
                    ProductId = info.ProductId,
                    PrimaryQuantity = info.PrimaryQuantity,


                    ProductUnitConversionId = info.ProductUnitConversionId,
                    ProductUnitConversionQuantity = info.ProductUnitConversionQuantity,

                    Description = info.Description,
                    PurchasingRequestTypeId = (EnumPurchasingRequestType)info.PurchasingRequestTypeId
                });
            }

            return (result, total);

        }


        public async Task<IList<PurchasingRequestDetailInfo>> PurchasingRequestDetailInfo(IList<long> purchasingRequestDetailIds)
        {
            if (purchasingRequestDetailIds == null || purchasingRequestDetailIds.Count == 0)
                return new List<PurchasingRequestDetailInfo>();

            return await (
                from d in _purchaseOrderDBContext.PurchasingRequestDetail
                join r in _purchaseOrderDBContext.PurchasingRequest on d.PurchasingRequestId equals r.PurchasingRequestId
                where purchasingRequestDetailIds.Contains(d.PurchasingRequestDetailId)
                select new PurchasingRequestDetailInfo
                {
                    PurchasingRequestId = r.PurchasingRequestId,
                    PurchasingRequestCode = r.PurchasingRequestCode,
                    PurchasingRequestDetailId = d.PurchasingRequestDetailId,
                    ProductId = d.ProductId,
                    PrimaryQuantity = d.PrimaryQuantity,
                    ProductUnitConversionId = d.ProductUnitConversionId,
                    ProductUnitConversionQuantity = d.ProductUnitConversionQuantity
                })
            .ToListAsync();
        }



        public async Task<long> Create(EnumPurchasingRequestType requestType, PurchasingRequestInput model)
        {
            await ValidateProductUnitConversion(model);


            var ctx = await GeneratePurchasingRequestCode(null, model);


            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var purchasingRequest = _mapper.Map<PurchasingRequest>(model);

                purchasingRequest.RejectCount = 0;
                purchasingRequest.PurchasingRequestStatusId = (int)EnumPurchasingRequestStatus.Draff;
                purchasingRequest.IsApproved = null;
                purchasingRequest.PoProcessStatusId = null;
                purchasingRequest.IsDeleted = false;
                purchasingRequest.CreatedByUserId = _currentContext.UserId;
                purchasingRequest.UpdatedByUserId = _currentContext.UserId;
                purchasingRequest.CreatedDatetimeUtc = DateTime.UtcNow;
                purchasingRequest.UpdatedDatetimeUtc = DateTime.UtcNow;
                purchasingRequest.PurchasingRequestTypeId = (int)requestType;

                if (requestType == EnumPurchasingRequestType.OrderMaterial)
                {
                    purchasingRequest.PurchasingRequestStatusId = (int)EnumPurchasingRequestStatus.Censored;
                    purchasingRequest.IsApproved = true;
                    purchasingRequest.CensorByUserId = _currentContext.UserId;
                    purchasingRequest.CensorDatetimeUtc = DateTime.Now.Date.GetUnixUtc(_currentContext.TimeZoneOffset).UnixToDateTime();
                }

                purchasingRequest.MaterialCalcId = null;
                if (requestType == EnumPurchasingRequestType.MaterialCalc)
                {
                    if (!model.MaterialCalcId.HasValue || model.MaterialCalcId <= 0)
                    {
                        throw GeneralCode.InvalidParams.BadRequest();
                    }

                    purchasingRequest.MaterialCalcId = model.MaterialCalcId;

                    purchasingRequest.PurchasingRequestStatusId = (int)EnumPurchasingRequestStatus.WaitToCensor;
                    //purchasingRequest.IsApproved = true;
                    //purchasingRequest.CensorByUserId = _currentContext.UserId;
                    //purchasingRequest.CensorDatetimeUtc = DateTime.Now.Date.GetUnixUtc(_currentContext.TimeZoneOffset).UnixToDateTime();
                }

                if (requestType == EnumPurchasingRequestType.ProductionOrderMaterialCalc)
                {
                    if (!model.ProductionOrderId.HasValue || model.ProductionOrderId <= 0)
                    {
                        throw GeneralCode.InvalidParams.BadRequest();
                    }

                    purchasingRequest.ProductionOrderId = model.ProductionOrderId;

                    purchasingRequest.PurchasingRequestStatusId = (int)EnumPurchasingRequestStatus.WaitToCensor;
                }

                await _purchaseOrderDBContext.AddAsync(purchasingRequest);
                await _purchaseOrderDBContext.SaveChangesAsync();

                var purchasingRequestDetailList = model.Details.Select(d => _mapper.Map<PurchasingRequestDetail>(d)).ToList();

                foreach (var item in purchasingRequestDetailList)
                {
                    item.PurchasingRequestId = purchasingRequest.PurchasingRequestId;

                    item.CreatedDatetimeUtc = DateTime.UtcNow;
                    item.UpdatedDatetimeUtc = DateTime.UtcNow;
                    item.IsDeleted = false;
                    item.DeletedDatetimeUtc = null;
                }

                await _purchaseOrderDBContext.PurchasingRequestDetail.AddRangeAsync(purchasingRequestDetailList);
                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await ctx.ConfirmCode();

                await _purchasingRequestActivityLog
                     .LogBuilder(() => PurchasingRequestActivityLogMessage.PurchasingRequestCreate)
                     .MessageResourceFormatData(new[] { purchasingRequest.PurchasingRequestCode })
                     .ObjectId(purchasingRequest.PurchasingRequestId)
                     .JsonData(model.JsonSerialize())
                     .CreateLog();

                //await _activityLogService.CreateLog(EnumObjectType.PurchasingRequest, purchasingRequest.PurchasingRequestId, $"Thêm mới phiếu yêu cầu VTHH  {purchasingRequest.PurchasingRequestCode}", model.JsonSerialize());

                //await ConfirmPurchasingRequestCode(customGenCodeLastValue);


                return purchasingRequest.PurchasingRequestId;
            }
        }

        public async Task<bool> Update(EnumPurchasingRequestType purchasingRequestTypeId, long purchasingRequestId, PurchasingRequestInput model)
        {
            await ValidateProductUnitConversion(model);

            using (var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockPoRequest()))
            {

                //var customGenCodeBaseValueModel = await GeneratePurchasingRequestCode(purchasingRequestId, model);
                var ctx = await GeneratePurchasingRequestCode(purchasingRequestId, model);

                if (!string.IsNullOrEmpty(model.PurchasingRequestCode))
                {
                    var existedItem = await _purchaseOrderDBContext.PurchasingRequest.FirstOrDefaultAsync(r => r.PurchasingRequestId != purchasingRequestId && r.PurchasingRequestCode == model.PurchasingRequestCode);
                    if (existedItem != null) throw PurchasingRequestErrorCode.RequestCodeAlreadyExisted.BadRequest();
                }


                using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
                {
                    var info = await _purchaseOrderDBContext.PurchasingRequest.FirstOrDefaultAsync(d => d.PurchasingRequestId == purchasingRequestId);
                    if (info == null) throw PurchasingRequestErrorCode.RequestNotFound.BadRequest();

                    _mapper.Map(model, info);

                    if (info.PurchasingRequestTypeId != (int)purchasingRequestTypeId
                        || (purchasingRequestTypeId == EnumPurchasingRequestType.OrderMaterial && model.OrderDetailId != info.OrderDetailId))
                    {
                        throw PurchasingRequestMessage.PurchasingRequestPreventFrom.BadFormat()
                            .Add(EnumPurchasingRequestType.OrderMaterial.GetEnumDescription())
                            .Build();

                    }

                    if (info.PurchasingRequestTypeId != (int)purchasingRequestTypeId || (purchasingRequestTypeId == EnumPurchasingRequestType.MaterialCalc && model.MaterialCalcId != info.MaterialCalcId))
                    {
                        throw PurchasingRequestMessage.PurchasingRequestPreventFrom.BadFormat()
                            .Add(EnumPurchasingRequestType.MaterialCalc.GetEnumDescription())
                            .Build();
                    }

                    if (info.PurchasingRequestTypeId != (int)purchasingRequestTypeId || (purchasingRequestTypeId == EnumPurchasingRequestType.ProductionOrderMaterialCalc && model.ProductionOrderId != info.ProductionOrderId))
                    {
                        throw PurchasingRequestMessage.PurchasingRequestPreventFrom.BadFormat()
                            .Add(EnumPurchasingRequestType.ProductionOrderMaterialCalc.GetEnumDescription())
                            .Build();
                    }

                    await DeleteOldDetails(purchasingRequestId);

                    info.PurchasingRequestStatusId = (int)EnumPurchasingRequestStatus.Draff;
                    info.IsApproved = null;
                    info.UpdatedByUserId = _currentContext.UserId;
                    info.UpdatedDatetimeUtc = DateTime.UtcNow;

                    if (info.PurchasingRequestTypeId == (int)EnumPurchasingRequestType.OrderMaterial)
                    {
                        info.PurchasingRequestStatusId = (int)EnumPurchasingRequestStatus.Censored;
                        info.IsApproved = true;
                        info.CensorByUserId = _currentContext.UserId;
                        info.CensorDatetimeUtc = DateTime.UtcNow;
                    }

                    if (info.PurchasingRequestTypeId == (int)EnumPurchasingRequestType.MaterialCalc || info.PurchasingRequestTypeId == (int)EnumPurchasingRequestType.ProductionOrderMaterialCalc)
                    {
                        info.PurchasingRequestStatusId = (int)EnumPurchasingRequestStatus.WaitToCensor;
                    }


                    var purchasingRequestDetailList = model.Details.Select(d => _mapper.Map<PurchasingRequestDetail>(d)).ToList();
                    foreach (var item in purchasingRequestDetailList)
                    {
                        item.PurchasingRequestId = purchasingRequestId;

                        item.CreatedDatetimeUtc = DateTime.UtcNow;
                        item.UpdatedDatetimeUtc = DateTime.UtcNow;
                        item.IsDeleted = false;
                        item.DeletedDatetimeUtc = null;
                    }

                    await _purchaseOrderDBContext.PurchasingRequestDetail.AddRangeAsync(purchasingRequestDetailList);
                    await _purchaseOrderDBContext.SaveChangesAsync();

                    trans.Commit();

                    await ctx.ConfirmCode();


                    await _purchasingRequestActivityLog
                      .LogBuilder(() => PurchasingRequestActivityLogMessage.PurchasingRequestUpdate)
                      .MessageResourceFormatData(new[] { info.PurchasingRequestCode })
                      .ObjectId(purchasingRequestId)
                      .JsonData(model.JsonSerialize())
                      .CreateLog();

                    //await _activityLogService.CreateLog(EnumObjectType.PurchasingRequest, purchasingRequestId, $"Cập nhật phiếu yêu cầu VTHH  {info.PurchasingRequestCode}", model.JsonSerialize());

                    return true;
                }
            }
        }



        private async Task<GenerateCodeContext> GeneratePurchasingRequestCode(long? purchasingRequestId, PurchasingRequestInput model)
        {
            model.PurchasingRequestCode = (model.PurchasingRequestCode ?? "").Trim();


            var ctx = _customGenCodeHelperService.CreateGenerateCodeContext();

            var code = await ctx
                .SetConfig(EnumObjectType.PurchasingRequest)
                .SetConfigData(purchasingRequestId ?? 0, model.Date)
                .TryValidateAndGenerateCode(_purchaseOrderDBContext.PurchasingRequest, model.PurchasingRequestCode, (s, code) => s.PurchasingRequestId != purchasingRequestId && s.PurchasingRequestCode == code);

            model.PurchasingRequestCode = code;

            return ctx;

        }



        public async Task<bool> Delete(long? orderDetailId, long? materialCalcId, long? productionOrderId, long purchasingRequestId)
        {
            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var info = await _purchaseOrderDBContext.PurchasingRequest.FirstOrDefaultAsync(d => d.PurchasingRequestId == purchasingRequestId);
                if (info == null) throw PurchasingRequestErrorCode.RequestNotFound.BadRequest();
                //if (info.PurchasingRequestTypeId == (int)EnumPurchasingRequestType.OrderMaterial && info.OrderDetailId != orderDetailId)
                //{
                //    throw new BadRequestException(GeneralCode.InvalidParams);
                //}

                //if (info.PurchasingRequestTypeId == (int)EnumPurchasingRequestType.MaterialCalc && info.MaterialCalcId != materialCalcId)
                //{
                //    throw new BadRequestException(GeneralCode.InvalidParams);
                //}

                await DeleteOldDetails(purchasingRequestId);

                info.IsDeleted = true;
                info.DeletedDatetimeUtc = DateTime.UtcNow;

                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();


                await _purchasingRequestActivityLog
                  .LogBuilder(() => PurchasingRequestActivityLogMessage.PurchasingRequestDelete)
                  .MessageResourceFormatData(new[] { info.PurchasingRequestCode })
                  .ObjectId(purchasingRequestId)
                  .JsonData(info.JsonSerialize())
                  .CreateLog();

                //await _activityLogService.CreateLog(EnumObjectType.PurchasingRequest, purchasingRequestId, $"Xóa phiếu yêu cầu VTHH  {info.PurchasingRequestCode}", info.JsonSerialize());
                return true;
            }
        }


        public CategoryNameModel GetFieldDataForMapping()
        {
            var result = new CategoryNameModel()
            {
                CategoryId = 1,
                CategoryCode = "PurchasingRequest",
                CategoryTitle = "PurchasingRequest",
                IsTreeView = false,
                Fields = new List<CategoryFieldNameModel>()
            };
            var fields = Utils.GetFieldNameModels<PurchasingRequestDetailRowValue>();
            result.Fields = fields;
            return result;
        }

        public IAsyncEnumerable<PurchasingRequestInputDetail> ParseInvoiceDetails(ImportExcelMapping mapping, SingleInvoiceStaticContent extra, Stream stream)
        {
            return new PurchasingRequestParseExcelFacade(_productHelperService)
                 .ParseInvoiceDetails(mapping, extra, stream);
        }



        public async Task<bool> SendToCensor(long purchasingRequestId)
        {
            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var info = await _purchaseOrderDBContext.PurchasingRequest.FirstOrDefaultAsync(d => d.PurchasingRequestId == purchasingRequestId);
                if (info == null) throw PurchasingRequestErrorCode.RequestNotFound.BadRequest();

                if (info.PurchasingRequestStatusId != (int)EnumPurchasingRequestStatus.Draff)
                {
                    throw GeneralCode.InvalidParams.BadRequest();
                }

                info.PurchasingRequestStatusId = (int)EnumPurchasingRequestStatus.WaitToCensor;
                info.UpdatedDatetimeUtc = DateTime.UtcNow;
                info.UpdatedByUserId = _currentContext.UserId;


                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();


                await _purchasingRequestActivityLog
                  .LogBuilder(() => PurchasingRequestActivityLogMessage.PurchasingRequestSentToCensor)
                  .MessageResourceFormatData(new[] { info.PurchasingRequestCode })
                  .ObjectId(purchasingRequestId)
                  .JsonData(info.JsonSerialize())
                  .CreateLog();


                //await _activityLogService.CreateLog(EnumObjectType.PurchasingRequest, purchasingRequestId, $"Gửi duyệt yêu cầu VTHH  {info.PurchasingRequestCode}", info.JsonSerialize());

                return true;
            }
        }

        public async Task<bool> Approve(long purchasingRequestId)
        {
            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var info = await _purchaseOrderDBContext.PurchasingRequest.FirstOrDefaultAsync(d => d.PurchasingRequestId == purchasingRequestId);
                if (info == null) throw PurchasingRequestErrorCode.RequestNotFound.BadRequest();

                //allow re censored
                if (info.PurchasingRequestStatusId != (int)EnumPurchasingRequestStatus.WaitToCensor
                    && info.PurchasingRequestStatusId != (int)EnumPurchasingRequestStatus.Censored
                    )
                {
                    throw GeneralCode.InvalidParams.BadRequest();
                }

                info.IsApproved = true;
                info.PurchasingRequestStatusId = (int)EnumPurchasingRequestStatus.Censored;
                info.CensorDatetimeUtc = DateTime.Now.Date.GetUnixUtc(_currentContext.TimeZoneOffset).UnixToDateTime();
                info.CensorByUserId = _currentContext.UserId;

                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _purchasingRequestActivityLog
                   .LogBuilder(() => PurchasingRequestActivityLogMessage.PurchasingRequestApproved)
                   .MessageResourceFormatData(new[] { info.PurchasingRequestCode })
                   .ObjectId(purchasingRequestId)
                   .JsonData(info.JsonSerialize())
                   .CreateLog();

                //await _activityLogService.CreateLog(EnumObjectType.PurchasingRequest, purchasingRequestId, $"Duyệt yêu cầu VTHH  {info.PurchasingRequestCode}", info.JsonSerialize());

                return true;
            }
        }

        public async Task<bool> Reject(long purchasingRequestId)
        {
            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var info = await _purchaseOrderDBContext.PurchasingRequest.FirstOrDefaultAsync(d => d.PurchasingRequestId == purchasingRequestId);
                if (info == null) throw PurchasingRequestErrorCode.RequestNotFound.BadRequest();
                //allow re censored
                if (info.PurchasingRequestStatusId != (int)EnumPurchasingRequestStatus.WaitToCensor
                    && info.PurchasingRequestStatusId != (int)EnumPurchasingRequestStatus.Censored
                    )
                {
                    throw GeneralCode.InvalidParams.BadRequest();
                }

                info.IsApproved = false;
                info.RejectCount++;

                info.PurchasingRequestStatusId = (int)EnumPurchasingRequestStatus.Censored;
                info.CensorDatetimeUtc = DateTime.Now.Date.GetUnixUtc(_currentContext.TimeZoneOffset).UnixToDateTime();
                info.CensorByUserId = _currentContext.UserId;

                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _purchasingRequestActivityLog
                  .LogBuilder(() => PurchasingRequestActivityLogMessage.PurchasingRequestRejected)
                  .MessageResourceFormatData(new[] { info.PurchasingRequestCode })
                  .ObjectId(purchasingRequestId)
                  .JsonData(info.JsonSerialize())
                  .CreateLog();

                // await _activityLogService.CreateLog(EnumObjectType.PurchasingRequest, purchasingRequestId, $"Từ chối yêu cầu VTHH  {info.PurchasingRequestCode}", info.JsonSerialize());

                return true;
            }
        }

        public async Task<bool> UpdatePoProcessStatus(long purchasingRequestId, EnumPoProcessStatus poProcessStatusId)
        {
            using (var trans = await _purchaseOrderDBContext.Database.BeginTransactionAsync())
            {
                var info = await _purchaseOrderDBContext.PurchasingRequest.FirstOrDefaultAsync(d => d.PurchasingRequestId == purchasingRequestId);
                if (info == null) throw PurchasingRequestErrorCode.RequestNotFound.BadRequest();

                info.PoProcessStatusId = (int)poProcessStatusId;

                await _purchaseOrderDBContext.SaveChangesAsync();

                trans.Commit();

                await _purchasingRequestActivityLog
                 .LogBuilder(() => PurchasingRequestActivityLogMessage.PurchasingRequestUpdatedProgress)
                 .MessageResourceFormatData(new[] { info.PurchasingRequestCode, poProcessStatusId.GetEnumDescription() })
                 .ObjectId(purchasingRequestId)
                 .JsonData(info.JsonSerialize())
                 .CreateLog();

                //await _activityLogService.CreateLog(EnumObjectType.PurchasingRequest, purchasingRequestId, $"Cập nhật tiến trình PO yêu cầu VTHH  {info.PurchasingRequestCode}", info.JsonSerialize());

                return true;
            }
        }

        private async Task ValidateProductUnitConversion(PurchasingRequestInput model)
        {
            var productUnitConversionProductGroup = model.Details.Select(d => new { d.ProductUnitConversionId, d.ProductId })
               .GroupBy(d => d.ProductUnitConversionId);
            if (productUnitConversionProductGroup.Any(g => g.Select(p => p.ProductId).Distinct().Count() > 1))
            {
                throw PurchasingRequestMessage.PuConversionDoesNotBelongToProduct.BadRequest();
            }

            if (!await _productHelperService.ValidateProductUnitConversions(productUnitConversionProductGroup.ToDictionary(g => g.Key, g => g.First().ProductId)))
            {
                throw PurchasingRequestMessage.PuConversionDoesNotBelongToProduct.BadRequest();
            }
        }

        public async Task<PurchasingRequestOutput> GetPurchasingRequestByProductionOrderId(long productionOrderId)
        {
            var info = await _purchaseOrderDBContext
                .PurchasingRequest
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.ProductionOrderId == productionOrderId);

            if (info == null) return null;

            var details = await _purchaseOrderDBContext.PurchasingRequestDetail.AsNoTracking()
                .Where(d => d.PurchasingRequestId == info.PurchasingRequestId)
                .ToListAsync();

            var data = _mapper.Map<PurchasingRequestOutput>(info);

            data.Details = details.Select(d => _mapper.Map<PurchasingRequestOutputDetail>(d)).ToList();

            return data;
        }

        private async Task DeleteOldDetails(long purchasingRequestId)
        {
            var oldDetails = await _purchaseOrderDBContext.PurchasingRequestDetail.Where(d => d.PurchasingRequestId == purchasingRequestId).ToListAsync();

            var purchasingRequestDetailIds = oldDetails.Select(d => (long?)d.PurchasingRequestDetailId).ToList();
            var sugguests = await (
                from d in _purchaseOrderDBContext.PurchasingSuggestDetail.Where(d => purchasingRequestDetailIds.Contains(d.PurchasingRequestDetailId))
                join s in _purchaseOrderDBContext.PurchasingSuggest on d.PurchasingSuggestId equals s.PurchasingSuggestId
                select

                    s.PurchasingSuggestCode
                ).Distinct()
                .ToListAsync();
            if (sugguests.Count > 0)
            {
                throw PurchasingRequestMessage.CanNotDeletePurchasingRequestWithExistedSuggest.BadRequestFormat(string.Join(", ", sugguests));
            }

            foreach (var item in oldDetails)
            {
                item.IsDeleted = true;
                item.DeletedDatetimeUtc = DateTime.UtcNow;
            }

        }

    }
}
