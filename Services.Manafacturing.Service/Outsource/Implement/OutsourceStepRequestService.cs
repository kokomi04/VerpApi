using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.ErrorCodes;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Manafacturing.Model.Outsource.RequestStep;
using VErp.Services.Manafacturing.Model.ProductionOrder;
using VErp.Services.Manafacturing.Model.ProductionStep;
using static VErp.Commons.Enums.Manafacturing.EnumOutsourceTrack;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;

namespace VErp.Services.Manafacturing.Service.Outsource.Implement
{
    public class OutsourceStepRequestService : IOutsourceStepRequestService
    {
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly ICustomGenCodeHelperService _customGenCodeHelperService;
        private readonly ICurrentContextService _currentContextService;
        private readonly IProductHelperService _productHelperService;
        public OutsourceStepRequestService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<OutsourceStepRequestService> logger
            , IMapper mapper
            , ICustomGenCodeHelperService customGenCodeHelperService
            , ICurrentContextService currentContextService
            , IProductHelperService productHelperService)
        {
            _manufacturingDBContext = manufacturingDB;
            _activityLogService = activityLogService;
            _logger = logger;
            _mapper = mapper;
            _customGenCodeHelperService = customGenCodeHelperService;
            _currentContextService = currentContextService;
            _productHelperService = productHelperService;
        }

        public async Task<PageData<OutsourceStepRequestSearch>> SearchOutsourceStepRequest(
            string keyword,
            int page,
            int size,
            string orderByFieldName,
            bool asc,
            long fromDate,
            long toDate,
            Clause filters = null)
        {
            keyword = (keyword ?? "").Trim();

            var productionOrderAsQueryable = from o in _manufacturingDBContext.ProductionOrder
                                             join d in _manufacturingDBContext.ProductionOrderDetail on o.ProductionOrderId equals d.ProductionOrderId
                                             select new
                                             {
                                                 o.ProductionOrderId,
                                                 o.ProductionOrderCode,
                                                 d.OrderCode
                                             };

            var productionStepAsQueryable = from c in _manufacturingDBContext.ProductionStep
                                            join p in _manufacturingDBContext.ProductionStep on c.ParentId equals p.ProductionStepId
                                            join s in _manufacturingDBContext.Step on p.StepId equals s.StepId
                                            select new
                                            {
                                                c.OutsourceStepRequestId,
                                                c.ProductionStepId,
                                                InOutTitle = c.Title,
                                                s.StepName,
                                                p.StepId
                                            };

            var query = from rq in _manufacturingDBContext.OutsourceStepRequest
                        join po in productionOrderAsQueryable on rq.ProductionOrderId equals po.ProductionOrderId
                        join ps in productionStepAsQueryable on rq.OutsourceStepRequestId equals ps.OutsourceStepRequestId
                        select new
                        {
                            rq.OutsourceStepRequestId,
                            rq.OutsourceStepRequestCode,
                            rq.CreatedDatetimeUtc,
                            rq.OutsourceStepRequestStatusId,
                            rq.OutsourceStepRequestFinishDate,
                            rq.IsInvalid,
                            rq.ProductionOrderId,
                            po.ProductionOrderCode,
                            po.OrderCode,
                            ps.InOutTitle,
                            ps.StepName
                        };

            if (fromDate > 0 && toDate > 0)
            {
                query = query.Where(x => x.CreatedDatetimeUtc >= fromDate.UnixToDateTime() && x.CreatedDatetimeUtc < toDate.UnixToDateTime().Value.AddDays(1));
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(x => x.OutsourceStepRequestCode.Contains(keyword)
                        || x.ProductionOrderCode.Contains(keyword)
                        || x.OrderCode.Contains(keyword)
                        || x.StepName.Contains(keyword));
            }

            if (filters != null)
            {
                query = query.InternalFilter(filters);
            }

            if (!string.IsNullOrWhiteSpace(orderByFieldName))
            {
                query = query.InternalOrderBy(orderByFieldName, asc);
            }

            var arrOutsourceStepRequestId = (await query.Select(x => x.OutsourceStepRequestId).ToListAsync()).Distinct().ToArray();
            var arrProductionOrdertId = (await query.Select(x => x.ProductionOrderId).ToListAsync()).Distinct().ToArray();

            var mapProductionOrder = (await productionOrderAsQueryable.Where(x => arrProductionOrdertId.Contains(x.ProductionOrderId)).ToListAsync())
                                        .GroupBy(k => k.ProductionOrderId).ToDictionary(k => k.Key, v => new
                                        {
                                            ProductionOrderCode = v.First().ProductionOrderCode,
                                            OrderCode = v.Select(x => x.OrderCode).Distinct().ToArray()
                                        });

            var mapProductionStep = (await productionStepAsQueryable.Where(x => arrOutsourceStepRequestId.Contains(x.OutsourceStepRequestId.GetValueOrDefault())).ToListAsync())
                                        .GroupBy(k => k.OutsourceStepRequestId).ToDictionary(k => k.Key, v => new
                                        {
                                            ProductionStepTile = v.Select(x => x.StepName).Distinct().ToArray()
                                        });

            var mapPurchaseOrder = (await _manufacturingDBContext.RefOutsourceStepOrder.Where(x => arrOutsourceStepRequestId.Contains(x.OutsourceRequestId.GetValueOrDefault()))
                            .ToListAsync())
                            .GroupBy(x => x.OutsourceRequestId)
                            .ToDictionary(k => k.Key, v => v.Select(x => new PurchaseOrderSimple
                            {
                                PurchaseOrderCode = x.PurchaseOrderCode,
                                PurchaseOrderId = x.PurchaseOrderId
                            }));


            var dataAsQueryAble = from r in _manufacturingDBContext.OutsourceStepRequest
                                  where arrOutsourceStepRequestId.Contains(r.OutsourceStepRequestId)
                                  select new OutsourceStepRequestSearch
                                  {
                                      IsInvalid = r.IsInvalid,
                                      OutsourceStepRequestCode = r.OutsourceStepRequestCode,
                                      OutsourceStepRequestFinishDate = r.OutsourceStepRequestFinishDate.GetUnix(),
                                      OutsourceStepRequestDate = r.CreatedDatetimeUtc.GetUnix(),
                                      OutsourceStepRequestId = r.OutsourceStepRequestId,
                                      OutsourceStepRequestStatusId = r.OutsourceStepRequestStatusId,
                                      ProductionOrderId = r.ProductionOrderId,
                                  };

            var total = await dataAsQueryAble.CountAsync();
            var lst = await (size > 0 ? dataAsQueryAble.Skip((page - 1) * size).Take(size) : dataAsQueryAble).ToListAsync();

            foreach (var element in lst)
            {
                mapProductionOrder.TryGetValue(element.ProductionOrderId, out var productOrderInfo);
                mapProductionStep.TryGetValue(element.OutsourceStepRequestId, out var productionStepInfo);
                mapPurchaseOrder.TryGetValue(element.OutsourceStepRequestId, out var purchaseOrderInfo);

                element.ProductionStepCollectionTitle = string.Join(", ", productionStepInfo.ProductionStepTile);
                element.OrderCode = string.Join(", ", productOrderInfo.OrderCode);
                element.ProductionOrderCode = productOrderInfo.ProductionOrderCode;
                element.PurchaseOrder = purchaseOrderInfo;
            }

            return (lst, total);
        }

        public async Task<OutsourceStepRequestOutput> GetOutsourceStepRequestOutput(long outsourceStepRequestId)
        {
            var request = await _manufacturingDBContext.OutsourceStepRequest.AsNoTracking()
                .Include(x => x.ProductionStep)
                .ThenInclude(s => s.Step)
                .Include(x => x.OutsourceStepRequestData)
                .FirstOrDefaultAsync(x => x.OutsourceStepRequestId == outsourceStepRequestId);

            if (request == null)
                throw new BadRequestException(OutsourceErrorCode.NotFoundRequest);

            var productionStepParents = await _manufacturingDBContext.ProductionStep.AsNoTracking()
                .Where(x => request.ProductionStep.Select(x => x.ParentId).Distinct().Contains(x.ProductionStepId))
                .Include(x => x.Step)
                .ToListAsync();

            var roles = await _manufacturingDBContext.ProductionStepLinkDataRole.AsNoTracking()
                .Where(x => request.ProductionStep.Select(x => x.ProductionStepId).Contains(x.ProductionStepId))
                .ToListAsync();

            var purchaseOrder = await _manufacturingDBContext.RefOutsourceStepOrder.Where(x => x.OutsourceRequestId == outsourceStepRequestId).ToListAsync();

            var outsourceDetails = request.OutsourceStepRequestData.Where(x => x.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Output)
                .Select(x =>
                {

                    var role = roles.FirstOrDefault(r => r.ProductionStepLinkDataId == x.ProductionStepLinkDataId && r.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Output);
                    var productionStepInfo = request.ProductionStep.FirstOrDefault(s => s.ProductionStepId == role.ProductionStepId);
                    var productionStepParentInfo = productionStepParents.FirstOrDefault(s => s.ProductionStepId == productionStepInfo.ParentId);

                    var purchaseOrderDetail = purchaseOrder.Where(o => o.ProductionStepLinkDataId == x.ProductionStepLinkDataId).ToList();

                    return new OutsourceStepRequestDetailOutput
                    {
                        ProductionStepLinkDataId = x.ProductionStepLinkDataId,
                        Quantity = x.Quantity,
                        TotalOutsourceOrderQuantity = purchaseOrderDetail.Sum(x => x.PrimaryQuantity),
                        RoleType = (int)EnumProductionStepLinkDataRoleType.Output,
                        ProductionStepTitle = productionStepParentInfo.Step?.StepName,
                        PurchaseOrderCode = string.Join(", ", purchaseOrderDetail.Select(x => x.PurchaseOrderCode)),
                        PurchaseOrderId = string.Join(", ", purchaseOrderDetail.Select(x => x.PurchaseOrderId))
                    };
                }).ToList();

            var inputDetails = request.OutsourceStepRequestData.Where(x => x.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Input)
                .Select(x =>
                {
                    return new OutsourceStepRequestDetailOutput
                    {
                        ProductionStepLinkDataId = x.ProductionStepLinkDataId,
                        Quantity = x.Quantity,
                    };
                }).ToList();

            return new OutsourceStepRequestOutput
            {
                OutsourceStepRequestCode = request.OutsourceStepRequestCode,
                OutsourceStepRequestFinishDate = request.OutsourceStepRequestFinishDate.GetUnix(),
                ProductionOrderId = request.ProductionOrderId,
                ProductionStepIds = request.ProductionStep.Where(x => x.IsGroup == false).Select(x => x.ProductionStepId).ToArray(),
                OutsourceStepRequestId = request.OutsourceStepRequestId,
                OutsourceStepRequestDate = request.CreatedDatetimeUtc.GetUnix(),
                DetailOutputs = outsourceDetails,
                DetailInputs = inputDetails,
                IsInvalid = request.IsInvalid,
                OutsourceStepRequestStatusId = request.OutsourceStepRequestStatusId,
                Setting = request.Setting.JsonDeserialize<OutsourceStepSetting>()
            };
        }

        public async Task<bool> UpdateOutsourceStepRequest(long outsourceStepRequestId, OutsourceStepRequestInput requestModel)
        {
            var request = await _manufacturingDBContext.OutsourceStepRequest.FirstOrDefaultAsync(x => x.OutsourceStepRequestId == outsourceStepRequestId);
            if (request == null)
                throw new BadRequestException(OutsourceErrorCode.NotFoundRequest);

            var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                var hasProductSemi = requestModel.ProductionProcessOutsource.ProductionStepLinkDatas.Where(x => x.ObjectTypeId == EnumProductionStepLinkDataObjectType.ProductSemi).Count() > 0;

                request.OutsourceStepRequestFinishDate = requestModel.OutsourceStepRequestFinishDate.UnixToDateTime(0);
                request.IsInvalid = hasProductSemi;
                request.Setting = requestModel.Setting.JsonSerialize();

                var oldDetail = await _manufacturingDBContext.OutsourceStepRequestData
                    .Where(d => d.OutsourceStepRequestId == outsourceStepRequestId)
                    .ToListAsync();
                _manufacturingDBContext.OutsourceStepRequestData.RemoveRange(oldDetail);
                await _manufacturingDBContext.SaveChangesAsync();

                var lsLinkDataOutput = requestModel.ProductionProcessOutsource.ProductionStepLinkDatas
                    .Where(x => requestModel.ProductionProcessOutsource.ProductionStepLinkDataOutput.Contains(x.ProductionStepLinkDataId));
                // Create outsourceStepRequestData
                foreach (var d in lsLinkDataOutput)
                {
                    if (d.OutsourceQuantity > 0)
                        _manufacturingDBContext.OutsourceStepRequestData.Add(new OutsourceStepRequestData
                        {
                            OutsourceStepRequestId = request.OutsourceStepRequestId,
                            ProductionStepLinkDataId = d.ProductionStepLinkDataId,
                            Quantity = d.OutsourceQuantity,
                            ProductionStepLinkDataRoleTypeId = 2,
                            IsImportant = d.IsImportant,
                            ProductionStepId = d.ProductionStepSourceId
                        });
                }

                var lsLinkDataInput = requestModel.ProductionProcessOutsource.ProductionStepLinkDatas
                    .Where(x => requestModel.ProductionProcessOutsource.ProductionStepLinkDataIntput.Contains(x.ProductionStepLinkDataId));
                // Create outsourceStepRequestData
                foreach (var d in lsLinkDataInput)
                {
                    if (d.ExportOutsourceQuantity > 0)
                        _manufacturingDBContext.OutsourceStepRequestData.Add(new OutsourceStepRequestData
                        {
                            OutsourceStepRequestId = request.OutsourceStepRequestId,
                            ProductionStepLinkDataId = d.ProductionStepLinkDataId,
                            Quantity = d.ExportOutsourceQuantity,
                            ProductionStepLinkDataRoleTypeId = (int)EnumProductionStepLinkDataRoleType.Input,
                            IsImportant = false,
                            ProductionStepId = d.ProductionStepReceiveId
                        });
                }

                await _manufacturingDBContext.SaveChangesAsync();

                // Update productionStep and linkData
                await SyncInfoForProductionProcess(requestModel.ProductionProcessOutsource, request.OutsourceStepRequestId);

                await trans.CommitAsync();

                await _activityLogService.CreateLog(EnumObjectType.OutsourceRequest, request.OutsourceStepRequestId,
                    $"Cập nhật yêu cầu gia công công đoạn", requestModel.JsonSerialize());

                return true;
            }
            catch (Exception ex)
            {
                await trans.TryRollbackTransactionAsync();
                _logger.LogError(ex, "UpdateOutsourceStepRequest");
                throw;
            }
        }

        private async Task SyncInfoForProductionProcess(ProductionProcessOutsourceStep processOutsourceStep, long outsourceStepRequestId)
        {
            var productionSteps = await _manufacturingDBContext.ProductionStep
                .Where(x => processOutsourceStep.ProductionSteps.Select(s => s.ProductionStepId).Contains(x.ProductionStepId) && x.IsGroup == false)
                .ToListAsync();

            var productionStepLinkDatas = await _manufacturingDBContext.ProductionStepLinkData
                .Where(x => processOutsourceStep.ProductionStepLinkDatas.Select(s => s.ProductionStepLinkDataId).Contains(x.ProductionStepLinkDataId))
                .ToListAsync();

            productionSteps.ForEach(x => x.OutsourceStepRequestId = outsourceStepRequestId);

            foreach (var ld in productionStepLinkDatas)
            {
                var source = processOutsourceStep.ProductionStepLinkDatas.FirstOrDefault(x => x.ProductionStepLinkDataId == ld.ProductionStepLinkDataId);
                if (source != null)
                {
                    ld.OutsourceQuantity = source.OutsourceQuantity;
                    ld.ExportOutsourceQuantity = source.ExportOutsourceQuantity;
                }
            }

            await _manufacturingDBContext.SaveChangesAsync();
        }

        public async Task<bool> DeleteOutsourceStepRequest(long outsourceStepRequestId)
        {
            var request = await _manufacturingDBContext.OutsourceStepRequest
                .Include(x => x.ProductionStep)
                .FirstOrDefaultAsync(x => x.OutsourceStepRequestId == outsourceStepRequestId);
            if (request == null)
                throw new BadRequestException(OutsourceErrorCode.NotFoundRequest);
            var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                request.IsDeleted = true;
                var detail = await _manufacturingDBContext.OutsourceStepRequestData
                    .Where(d => d.OutsourceStepRequestId == outsourceStepRequestId)
                    .ToListAsync();

                var hasPurchaseOrder = _manufacturingDBContext.RefOutsourceStepOrder.Any(x => x.OutsourceRequestId == outsourceStepRequestId);

                if (hasPurchaseOrder)
                    throw new BadRequestException(OutsourceErrorCode.InValidRequestOutsource, $"Đã có đơn hàng gia công cho yêu cầu {request.OutsourceStepRequestCode}");

                var arrProductionStepId = request.ProductionStep.Select(x => x.ProductionStepId);

                var productionSteps = await _manufacturingDBContext.ProductionStep
                    .Where(x => arrProductionStepId.Contains(x.ProductionStepId))
                    .ToListAsync();

                var roles = await _manufacturingDBContext.ProductionStepLinkDataRole.AsNoTracking()
                    .Where(x => arrProductionStepId.Contains(x.ProductionStepId))
                    .ToListAsync();

                var arrLinkDataId = roles.Select(x => x.ProductionStepLinkDataId).Distinct();
                var arrLinkDataInputId = roles.GroupBy(r => r.ProductionStepLinkDataId)
                    .Where(g => g.Count() == 1)
                    .Select(g => g.First())
                    .Where(l => l.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Input)
                    .Select(x => x.ProductionStepLinkDataId);

                var productionStepLinkDatas = (await _manufacturingDBContext.ProductionStepLinkData
                    .Where(x => arrLinkDataId.Contains(x.ProductionStepLinkDataId))
                    .ToListAsync()).OrderBy(x => arrLinkDataInputId.Contains(x.ProductionStepLinkDataId) ? 1 : 0);

                productionSteps.ForEach(x => x.OutsourceStepRequestId = null);

                foreach (var ld in productionStepLinkDatas)
                {
                    if (!arrLinkDataInputId.Contains(ld.ProductionStepLinkDataId))
                        ld.OutsourceQuantity = 0;
                    else
                        ld.ExportOutsourceQuantity = 0;
                }

                _manufacturingDBContext.OutsourceStepRequestData.RemoveRange(detail);

                await _manufacturingDBContext.SaveChangesAsync();

                await trans.CommitAsync();

                await _activityLogService.CreateLog(EnumObjectType.OutsourceRequest, request.OutsourceStepRequestId,
                    $"Xóa yêu cầu gia công công đoạn", request.JsonSerialize());
                return true;
            }
            catch (Exception ex)
            {
                await trans.TryRollbackTransactionAsync();
                _logger.LogError(ex, "DeleteOutsourceStepRequest");
                throw;
            }
        }

        public async Task<IList<OutsourceStepRequestDataExtraInfo>> GetOutsourceStepRequestData(long outsourceStepRequestId)
        {
            var lsOutsourceStepRequestDetail = await _manufacturingDBContext.OutsourceStepRequestData.AsNoTracking()
                .Where(x => x.OutsourceStepRequestId == outsourceStepRequestId)
                .Include(x => x.ProductionStep)
                .ThenInclude(s => s.Step)
                .ToListAsync();

            var linkDataIds = lsOutsourceStepRequestDetail.Select(x => x.ProductionStepLinkDataId);
            var quantityProcessedMap = (await _manufacturingDBContext.OutsourceOrderDetail.AsNoTracking()
                .Include(x => x.OutsourceOrder)
                .Where(x => x.OutsourceOrder.OutsourceTypeId == (int)EnumOutsourceType.OutsourceStep && linkDataIds.Contains(x.ObjectId))
                .ToListAsync())
                .GroupBy(x => x.ObjectId)
                .ToDictionary(k => k.Key, v => v.Sum(x => x.Quantity));

            var lsProductionStepLinkDataId = lsOutsourceStepRequestDetail.Select(x => x.ProductionStepLinkDataId).ToArray();
            var lsProductionStepLinkDataInfo = await GetProductionStepLinkDataByListId(lsProductionStepLinkDataId);

            var results = new List<OutsourceStepRequestDataExtraInfo>();
            foreach (var item in lsOutsourceStepRequestDetail)
            {
                var ldInfo = lsProductionStepLinkDataInfo.FirstOrDefault(x => x.ProductionStepLinkDataId == item.ProductionStepLinkDataId);
                var stepInfo = _mapper.Map<ProductionStepModel>(item.ProductionStep);
                var data = new OutsourceStepRequestDataExtraInfo
                {
                    IsImportant = item.IsImportant,
                    ProductionStepLinkDataId = item.ProductionStepLinkDataId,
                    OutsourceStepRequestDataQuantity = item.Quantity,
                    OutsourceStepRequestId = item.OutsourceStepRequestId,
                    ProductionStepId = item.ProductionStepId,
                    ProductionStepLinkDataRoleTypeId = (EnumProductionStepLinkDataRoleType)item.ProductionStepLinkDataRoleTypeId,
                    ProductionStepLinkDataTitle = ldInfo?.ObjectTitle,
                    ProductionStepTitle = stepInfo?.Title,
                    ProductionStepLinkDataUnitId = (int)(ldInfo?.UnitId),
                    ProductionStepLinkDataObjectId = (int)ldInfo.ObjectId,
                    OutsourceStepRequestDataQuantityProcessed = quantityProcessedMap.ContainsKey(item.ProductionStepLinkDataId) ? quantityProcessedMap[item.ProductionStepLinkDataId] : 0,
                    DecimalPlace = ldInfo.DecimalPlace
                };

                results.Add(data);
            }
            return results.OrderBy(x => x.IsImportant == true ? 0 : 1).ToList();
        }

        public async Task<IList<OutsourceStepRequestDataExtraInfo>> GetOutsourceStepRequestData(long[] productionStepLinkDataId)
        {
            var lsOutsourceStepRequestDetail = await _manufacturingDBContext.OutsourceStepRequestData.AsNoTracking()
                .Where(x => productionStepLinkDataId.Contains(x.ProductionStepLinkDataId))
                .Include(x => x.OutsourceStepRequest)
                .Include(x => x.ProductionStep)
                .ThenInclude(s => s.Step)
                .ToListAsync();

            var linkDataIds = lsOutsourceStepRequestDetail.Select(x => x.ProductionStepLinkDataId);
            var quantityProcessedMap = (await _manufacturingDBContext.OutsourceOrderDetail.AsNoTracking()
                .Include(x => x.OutsourceOrder)
                .Where(x => x.OutsourceOrder.OutsourceTypeId == (int)EnumOutsourceType.OutsourceStep && linkDataIds.Contains(x.ObjectId))
                .ToListAsync())
                .GroupBy(x => x.ObjectId)
                .ToDictionary(k => k.Key, v => v.Sum(x => x.Quantity));

            var lsProductionStepLinkDataId = lsOutsourceStepRequestDetail.Select(x => x.ProductionStepLinkDataId).ToArray();
            var lsProductionStepLinkDataInfo = await GetProductionStepLinkDataByListId(lsProductionStepLinkDataId);

            var results = new List<OutsourceStepRequestDataExtraInfo>();
            foreach (var item in lsOutsourceStepRequestDetail)
            {
                var ldInfo = lsProductionStepLinkDataInfo.FirstOrDefault(x => x.ProductionStepLinkDataId == item.ProductionStepLinkDataId);
                var stepInfo = _mapper.Map<ProductionStepModel>(item.ProductionStep);
                var data = new OutsourceStepRequestDataExtraInfo
                {
                    IsImportant = item.IsImportant,
                    ProductionStepLinkDataId = item.ProductionStepLinkDataId,
                    OutsourceStepRequestDataQuantity = item.Quantity,
                    OutsourceStepRequestId = item.OutsourceStepRequestId,
                    ProductionStepId = item.ProductionStepId,
                    ProductionStepLinkDataRoleTypeId = (EnumProductionStepLinkDataRoleType)item.ProductionStepLinkDataRoleTypeId,
                    ProductionStepLinkDataTitle = ldInfo?.ObjectTitle,
                    ProductionStepTitle = stepInfo?.Title,
                    ProductionStepLinkDataUnitId = (int)(ldInfo?.UnitId),
                    ProductionStepLinkDataObjectId = (int)ldInfo.ObjectId,
                    OutsourceStepRequestDataQuantityProcessed = quantityProcessedMap.ContainsKey(item.ProductionStepLinkDataId) ? quantityProcessedMap[item.ProductionStepLinkDataId] : 0,
                    OutsourceStepRequestCode = item.OutsourceStepRequest.OutsourceStepRequestCode,
                    OutsourceStepRequestFinishDate = item.OutsourceStepRequest.OutsourceStepRequestFinishDate.GetUnix(),
                    DecimalPlace = ldInfo.DecimalPlace,
                };

                results.Add(data);
            }
            return results.OrderBy(x => x.IsImportant == true ? 0 : 1).ToList();
        }

        public async Task<IList<OutsourceStepRequestModel>> GetAllOutsourceStepRequest()
        {
            var lst = await _manufacturingDBContext.OutsourceStepRequest
                .AsNoTracking()
                .Include(x => x.ProductionOrder)
                .Include(x => x.ProductionStep)
                .ProjectTo<OutsourceStepRequestModel>(_mapper.ConfigurationProvider)
                .ToListAsync();
            return lst;
        }

        public async Task<bool> UpdateOutsourceStepRequestStatus(long[] outsourceStepRequestId)
        {
            var transaction = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                var requests = await _manufacturingDBContext.OutsourceStepRequest
               .Where(x => outsourceStepRequestId.Contains(x.OutsourceStepRequestId))
               .ToListAsync();

                foreach (var rq in requests)
                {
                    var data = (from d in _manufacturingDBContext.OutsourceStepRequestData
                                join ld in _manufacturingDBContext.ProductionStepLinkData on d.ProductionStepLinkDataId equals ld.ProductionStepLinkDataId
                                select new
                                {
                                    d.ProductionStepLinkDataId,
                                    ProductId = ld.ObjectId,
                                    d.Quantity
                                }).ToList();

                    var arrPurchaseOrderId = await _manufacturingDBContext.RefOutsourceStepOrder.Where(x => x.OutsourceRequestId == rq.OutsourceStepRequestId)
                            .Select(x => x.PurchaseOrderId)
                            .Distinct()
                            .ToListAsync();

                    var totalStatus = (await _manufacturingDBContext.RefOutsourceStepTrack.AsNoTracking()
                        .Where(x => arrPurchaseOrderId.Contains(x.PurchaseOrderId) && !x.ProductId.HasValue)
                        .ToListAsync())
                        .GroupBy(x => x.PurchaseOrderId)
                        .Select(g => g.OrderByDescending(x => x.PurchaseOrderTrackedId).Take(1).FirstOrDefault()?.Status)
                        .Sum();

                    if (totalStatus.GetValueOrDefault() == 0)
                        rq.OutsourceStepRequestStatusId = (int)EnumOutsourceRequestStatusType.Unprocessed;
                    else
                    {
                        var mapQuantityProcessed = _manufacturingDBContext.RefOutsourceStepOrder
                                .Where(x => x.OutsourceRequestId == rq.OutsourceStepRequestId).ToList()
                                .GroupBy(x => x.ProductionStepLinkDataId)
                                .ToDictionary(k => k.Key, v => v.Sum(x => x.PrimaryQuantity));

                        var isCheckOrder = false;
                        foreach (var d in data)
                        {
                            mapQuantityProcessed.TryGetValue(d.ProductionStepLinkDataId, out var quantityProcessed);
                            if (d.Quantity - quantityProcessed != 0)
                            {
                                isCheckOrder = false;
                                break;
                            }

                            isCheckOrder = true;
                        }
                        if (isCheckOrder && (totalStatus.GetValueOrDefault() == ((int)EnumOutsourceTrackStatus.HandedOver * arrPurchaseOrderId.Count())))
                            rq.OutsourceStepRequestStatusId = (int)EnumOutsourceRequestStatusType.Processed;
                        else rq.OutsourceStepRequestStatusId = (int)EnumOutsourceRequestStatusType.Processing;
                    }
                }

                await _manufacturingDBContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (System.Exception ex)
            {
                transaction.TryRollbackTransaction();
                throw ex;
            }
        }

        public async Task<IList<OutsourceStepRequestDetailOutput>> GetOutsourceStepRequestDatasByProductionOrderId(long productionOrderId)
        {
            var sqlData = new StringBuilder(@$"SELECT * FROM vOutsourceStepRequestDataExtractInfo v WHERE v.ProductionOrderId = {productionOrderId}");
            var data = (await _manufacturingDBContext.QueryDataTable(sqlData.ToString(), Array.Empty<SqlParameter>())).ConvertData<OutsourceStepRequestDetailOutput>();
            return data;
        }

        private async Task<IList<ProductionStepLinkDataInput>> GetProductionStepLinkDataByListId(long[] lsProductionStepLinkDataId)
        {
            IList<ProductionStepLinkDataInput> stepLinkDatas = new List<ProductionStepLinkDataInput>();
            if (lsProductionStepLinkDataId.Length > 0)
            {
                var sql = new StringBuilder(@$"
                                    SELECT * FROM dbo.ProductionStepLinkDataExtractInfo v 
                                    WHERE v.ProductionStepLinkDataId IN (SELECT [Value] FROM @ProductionStepLinkDataIds)
                                ");
                var parammeters = new List<SqlParameter>()
                {
                    lsProductionStepLinkDataId.ToSqlParameter("@ProductionStepLinkDataIds"),
                };

                stepLinkDatas = await _manufacturingDBContext.QueryList<ProductionStepLinkDataInput>(sql.ToString(), parammeters);
            }

            return stepLinkDatas;
        }

        public async Task<OutsourceStepRequestPrivateKey> AddOutsourceStepRequest(OutsourceStepRequestInput requestModel)
        {
            using var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                CustomGenCodeOutputModel currentConfig = null;
                string outsourceStepRequestCode = string.Empty;

                currentConfig = await _customGenCodeHelperService.CurrentConfig(EnumObjectType.OutsourceRequest, EnumObjectType.OutsourceRequest, 0, null, outsourceStepRequestCode, DateTime.UtcNow.GetUnix());
                if (currentConfig == null)
                {
                    throw new BadRequestException(GeneralCode.ItemNotFound, "Chưa thiết định cấu hình sinh mã");
                }

                bool isFirst = true;
                do
                {
                    if (!isFirst) await _customGenCodeHelperService.ConfirmCode(currentConfig?.CurrentLastValue);

                    var generated = await _customGenCodeHelperService.GenerateCode(currentConfig.CustomGenCodeId,
                        currentConfig.CurrentLastValue.LastValue, null, outsourceStepRequestCode, DateTime.UtcNow.GetUnix());
                    if (generated == null)
                    {
                        throw new BadRequestException(GeneralCode.InternalError, "Không thể sinh mã ");
                    }
                    outsourceStepRequestCode = generated.CustomCode;
                    isFirst = false;
                } while (_manufacturingDBContext.ProductionMaterialsRequirement.Any(o => o.RequirementCode == outsourceStepRequestCode));

                var hasProductSemi = requestModel.ProductionProcessOutsource.ProductionStepLinkDatas.Where(x => x.ObjectTypeId == EnumProductionStepLinkDataObjectType.ProductSemi).Count() > 0;

                var entityRequest = new OutsourceStepRequest
                {
                    OutsourceStepRequestCode = outsourceStepRequestCode,
                    OutsourceStepRequestFinishDate = requestModel.OutsourceStepRequestFinishDate.UnixToDateTime(0),
                    ProductionOrderId = requestModel.ProductionOrderId,
                    IsInvalid = hasProductSemi,
                    OutsourceStepRequestStatusId = (int)EnumOutsourceRequestStatusType.Unprocessed,
                    Setting = requestModel.Setting.JsonSerialize()
                };

                _manufacturingDBContext.OutsourceStepRequest.Add(entityRequest);
                await _manufacturingDBContext.SaveChangesAsync();

                var lsLinkDataOutput = requestModel.ProductionProcessOutsource.ProductionStepLinkDatas
                    .Where(x => requestModel.ProductionProcessOutsource.ProductionStepLinkDataOutput.Contains(x.ProductionStepLinkDataId));
                // Create outsourceStepRequestData
                foreach (var d in lsLinkDataOutput)
                {
                    if (d.OutsourceQuantity > 0)
                        _manufacturingDBContext.OutsourceStepRequestData.Add(new OutsourceStepRequestData
                        {
                            OutsourceStepRequestId = entityRequest.OutsourceStepRequestId,
                            ProductionStepLinkDataId = d.ProductionStepLinkDataId,
                            Quantity = d.OutsourceQuantity,
                            ProductionStepLinkDataRoleTypeId = (int)EnumProductionStepLinkDataRoleType.Output,
                            IsImportant = d.IsImportant,
                            ProductionStepId = d.ProductionStepSourceId
                        });
                }

                var lsLinkDataInput = requestModel.ProductionProcessOutsource.ProductionStepLinkDatas
                    .Where(x => requestModel.ProductionProcessOutsource.ProductionStepLinkDataIntput.Contains(x.ProductionStepLinkDataId));
                // Create outsourceStepRequestData
                foreach (var d in lsLinkDataInput)
                {
                    if (d.ExportOutsourceQuantity > 0)
                        _manufacturingDBContext.OutsourceStepRequestData.Add(new OutsourceStepRequestData
                        {
                            OutsourceStepRequestId = entityRequest.OutsourceStepRequestId,
                            ProductionStepLinkDataId = d.ProductionStepLinkDataId,
                            Quantity = d.ExportOutsourceQuantity,
                            ProductionStepLinkDataRoleTypeId = (int)EnumProductionStepLinkDataRoleType.Input,
                            IsImportant = false,
                            ProductionStepId = d.ProductionStepReceiveId
                        });
                }
                await _manufacturingDBContext.SaveChangesAsync();

                // Update productionStep and linkData
                await SyncInfoForProductionProcess(requestModel.ProductionProcessOutsource, entityRequest.OutsourceStepRequestId);

                await _customGenCodeHelperService.ConfirmCode(currentConfig.CurrentLastValue);

                await trans.CommitAsync();

                await _activityLogService.CreateLog(EnumObjectType.OutsourceRequest, entityRequest.OutsourceStepRequestId,
                    $"Thêm mới yêu cầu gia công công đoạn", requestModel.JsonSerialize());

                return new OutsourceStepRequestPrivateKey
                {
                    OutsourceStepRequestCode = entityRequest.OutsourceStepRequestCode,
                    OutsourceStepRequestId = entityRequest.OutsourceStepRequestId
                };
            }
            catch (Exception ex)
            {
                await trans.TryRollbackTransactionAsync();
                _logger.LogError(ex, "AddOutsourceStepRequest");
                throw;
            }
        }

        public async Task<IList<OutsourceStepRequestMaterialsConsumption>> GetOutsourceStepMaterialsConsumption(long outsourceStepRequestId)
        {
            var request = await _manufacturingDBContext.OutsourceStepRequest.AsNoTracking()
                            .Include(x => x.OutsourceStepRequestData)
                            .Include(x => x.ProductionOrder)
                            .ThenInclude(x => x.ProductionOrderDetail)
                            .FirstOrDefaultAsync(x => x.OutsourceStepRequestId == outsourceStepRequestId);

            if (request == null)
                throw new BadRequestException(OutsourceErrorCode.NotFoundRequest);

            var results = new List<OutsourceStepRequestMaterialsConsumption>();

            var output = request.OutsourceStepRequestData.FirstOrDefault(x => x.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Output);
            var linkDataOutput = await _manufacturingDBContext.ProductionStepLinkData.FirstOrDefaultAsync(x => x.ProductionStepLinkDataId == output.ProductionStepLinkDataId);
            var mapProductOfProductOrder = request.ProductionOrder.ProductionOrderDetail.GroupBy(x => x.ProductId).ToDictionary(k => k.Key, v => v.Sum(x => x.Quantity));
            var productionStep = await _manufacturingDBContext.ProductionStep.FirstOrDefaultAsync(x => x.ProductionStepId == output.ProductionStepId);

            var materialsConsumptions = await _productHelperService.GetProductMaterialsConsumptions(mapProductOfProductOrder.Keys.ToArray());

            foreach (var materials in materialsConsumptions)
            {
                if (materials.StepId != productionStep.StepId) continue;

                var rate = (output.Quantity / linkDataOutput.QuantityOrigin) * mapProductOfProductOrder[materials.ProductId];

                var exists = results.FirstOrDefault(x => x.ProductId == materials.MaterialsConsumptionId);
                if (exists == null)
                {
                    var element = new OutsourceStepRequestMaterialsConsumption
                    {
                        OutsourceStepRequestId = outsourceStepRequestId,
                        ProductId = materials.MaterialsConsumptionId,
                        Quantity = (decimal)((materials.Quantity + materials.TotalQuantityInheritance) * rate)
                    };

                    results.Add(element);
                }
                else
                {
                    exists.Quantity += (decimal)((materials.Quantity + materials.TotalQuantityInheritance) * rate);
                }
            }

            return results;
        }
    
    }

}
