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

        public OutsourceStepRequestService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<OutsourceStepRequestService> logger
            , IMapper mapper
            , ICustomGenCodeHelperService customGenCodeHelperService)
        {
            _manufacturingDBContext = manufacturingDB;
            _activityLogService = activityLogService;
            _logger = logger;
            _mapper = mapper;
            _customGenCodeHelperService = customGenCodeHelperService;
        }

        public async Task<PageData<OutsourceStepRequestSearch>> SearchOutsourceStepRequest(string keyword, int page, int size, string orderByFieldName, bool asc, Clause filters = null)
        {
            var data = (await _manufacturingDBContext.OutsourceStepRequest.AsNoTracking()
                .Include(s => s.ProductionStep)
                .ThenInclude(p=>p.Step)
                .ToListAsync()).Select(x => new OutsourceStepRequestSearch
                {
                    IsInvalid = x.IsInvalid,
                    OutsourceStepRequestCode = x.OutsourceStepRequestCode,
                    OutsourceStepRequestFinishDate = x.OutsourceStepRequestFinishDate.GetUnix(),
                    OutsourceStepRequestId = x.OutsourceStepRequestId,
                    OutsourceStepRequestStatusId = x.OutsourceStepRequestStatusId,
                    ProductionOrderId = x.ProductionOrderId,
                    ProductionStepCollectionTitle = string.Join(", ", x.ProductionStep.AsQueryable().ProjectTo<ProductionStepInfo>(_mapper.ConfigurationProvider).Select(x => x.Title))
                }).ToList();

            var arrProductionOrderId = data.Select(x => x.ProductionOrderId).ToArray();
            if(arrProductionOrderId.Length > 0)
            {
                var parammeters = new List<SqlParameter>();
                var whereCondition = new StringBuilder();

                whereCondition.Append(" v.ProductionOrderId IN ( ");
                for (int i = 0; i < arrProductionOrderId.Length; i++)
                {
                    var value = arrProductionOrderId[i];
                    var pName = $"@ProductionOrderId_{i + 1}";
                    if (i == arrProductionOrderId.Length - 1)
                        whereCondition.Append($"{pName} ");
                    else
                        whereCondition.Append($"{pName}, ");
                    parammeters.Add(new SqlParameter(pName, value));
                }
                whereCondition.Append(") ");

                var sql = new StringBuilder($@"Select * from vProductionOrderDetail v");

                if (whereCondition.Length > 0)
                {
                    sql.Append(" WHERE ");
                    sql.Append(whereCondition);
                }

                var arrProductionOrderDetailInfo = (await _manufacturingDBContext.QueryDataTable(sql.ToString(), parammeters.Select(p => p.CloneSqlParam()).ToArray()))
                    .ConvertData<ProductionOrderListEntity>().AsQueryable().ProjectTo<ProductionOrderListModel>(_mapper.ConfigurationProvider).ToList();

                if(arrProductionOrderDetailInfo.Count > 0)
                {
                    foreach(var q in data)
                    {
                        var p = arrProductionOrderDetailInfo.Where(x => x.ProductionOrderId == q.ProductionOrderId);
                        if (p.Count() == 0) continue;

                        q.ProductionOrderCode = p.FirstOrDefault().ProductionOrderCode;
                        q.OrderCode = string.Join(", ", p.Select(x => x.OrderCode));
                    }
                }
            }

            var query = data.AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(x =>
                        x.OutsourceStepRequestCode.Contains(keyword)
                        || x.ProductionOrderCode.Contains(keyword)
                        || x.OrderCode.Contains(keyword)
                        || x.OrderCode.Contains(keyword));
            }

            if(filters != null)
            {
                query = query.InternalFilter(filters);
            }

            if (!string.IsNullOrWhiteSpace(orderByFieldName))
            {
                query = query.InternalOrderBy(orderByFieldName, asc);
            }

            var total = query.Count();
            var lst = (size > 0 ? query.Skip((page - 1) * size).Take(size) : query).ToList();

            return (lst, total);
        }

        public async Task<OutsourceStepRequestOutput> GetOutsourceStepRequestOutput(long outsourceStepRequestId)
        {
            var request = await _manufacturingDBContext.OutsourceStepRequest.AsNoTracking()
                .Include(x => x.ProductionStep)
                .ThenInclude(s=>s.Step)
                .Include(x => x.OutsourceStepRequestData)
                .FirstOrDefaultAsync(x => x.OutsourceStepRequestId == outsourceStepRequestId);

            var roles = await _manufacturingDBContext.ProductionStepLinkDataRole.AsNoTracking()
                .Where(x => request.ProductionStep.Select(x => x.ProductionStepId).Contains(x.ProductionStepId))
                .ToListAsync();

            var arrLinkDataId = request.OutsourceStepRequestData.Select(x => x.ProductionStepLinkDataId).ToArray();
            Dictionary<long, decimal> totalOutsourceOrderQuantityMap = await CalcTotalOutsourceOrderQuantity(arrLinkDataId);

            var arrOutput = request.OutsourceStepRequestData.Where(x=> x.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Output)
                .Select(x => {
               
                var role = roles.FirstOrDefault(r => r.ProductionStepLinkDataId == x.ProductionStepLinkDataId && r.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Output);
                var productionStepInfo = request.ProductionStep.FirstOrDefault(s => s.ProductionStepId == role.ProductionStepId);
                return new OutsourceStepRequestDetailOutput
                {
                    ProductionStepLinkDataId = x.ProductionStepLinkDataId,
                    Quantity = x.Quantity,
                    TotalOutsourceOrderQuantity = totalOutsourceOrderQuantityMap.ContainsKey(x.ProductionStepLinkDataId) ? totalOutsourceOrderQuantityMap[x.ProductionStepLinkDataId] : 0,
                    RoleType = (int)EnumProductionStepLinkDataRoleType.Output,
                    ProductionStepTitle =$"{productionStepInfo.Step.StepName} #({productionStepInfo.ProductionStepId})"
                };
            }).ToList();

            return new OutsourceStepRequestOutput
            {
                OutsourceStepRequestCode = request.OutsourceStepRequestCode,
                OutsourceStepRequestFinishDate = request.OutsourceStepRequestFinishDate.GetUnix(),
                ProductionOrderId = request.ProductionOrderId,
                ProductionStepIds = request.ProductionStep.Select(x => x.ProductionStepId).ToArray(),
                OutsourceStepRequestId = request.OutsourceStepRequestId,
                OutsourceStepRequestDate = request.CreatedDatetimeUtc.GetUnix(),
                DetailInputs = arrOutput,
                IsInvalid = request.IsInvalid,
                OutsourceStepRequestStatusId = request.OutsourceStepRequestStatusId,
                Setting = request.Setting.JsonDeserialize<OutsourceStepSetting>()
            };
        }

        private async Task<Dictionary<long, decimal>> CalcTotalOutsourceOrderQuantity(long[] arrLinkDataId)
        {
            return (await _manufacturingDBContext.OutsourceOrderDetail.AsNoTracking()
                .Where(x =>
                    x.OutsourceOrder.OutsourceTypeId == (int)EnumOutsourceType.OutsourceStep
                    && arrLinkDataId.Contains(x.ObjectId)
                    )
                .ToListAsync())
                .GroupBy(x => x.ObjectId)
                .ToDictionary(k => k.Key, v => v.Sum(x => x.Quantity));
        }

        public async Task<bool> UpdateOutsourceStepRequest(long outsourceStepRequestId, OutsourceStepRequestInput requestModel)
        {
            var request = await _manufacturingDBContext.OutsourceStepRequest.FirstOrDefaultAsync(x => x.OutsourceStepRequestId == outsourceStepRequestId);
            if (request == null)
                throw new BadRequestException(OutsourceErrorCode.NotFoundRequest);

            var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                request.OutsourceStepRequestFinishDate = requestModel.OutsourceStepRequestFinishDate.UnixToDateTime(0);
                request.IsInvalid = false;
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
                .Where(x => processOutsourceStep.ProductionSteps.Select(s => s.ProductionStepId).Contains(x.ProductionStepId))
                .ToListAsync();

            var productionStepLinkDatas = await _manufacturingDBContext.ProductionStepLinkData
                .Where(x => processOutsourceStep.ProductionStepLinkDatas.Select(s => s.ProductionStepLinkDataId).Contains(x.ProductionStepLinkDataId))
                .ToListAsync();

            productionSteps.ForEach(x => x.OutsourceStepRequestId = outsourceStepRequestId);

            foreach (var ld in productionStepLinkDatas)
            {
                var source = processOutsourceStep.ProductionStepLinkDatas.FirstOrDefault(x => x.ProductionStepLinkDataId == ld.ProductionStepLinkDataId);
                if(source != null)
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
                .Include(x=>x.ProductionStep)
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

                Dictionary<long, decimal> totalOutsourceOrderQuantityMap = await CalcTotalOutsourceOrderQuantity(detail.Select(x => x.ProductionStepLinkDataId).ToArray());
                detail.ForEach(x =>
                {
                    if (totalOutsourceOrderQuantityMap.ContainsKey(x.ProductionStepLinkDataId))
                        throw new BadRequestException(OutsourceErrorCode.InValidRequestOutsource, $"Đã có đơn hàng gia công cho yêu cầu {request.OutsourceStepRequestCode}");
                });

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
                    OutsourceStepRequestDataQuantityProcessed = quantityProcessedMap.ContainsKey(item.ProductionStepLinkDataId) ? quantityProcessedMap[item.ProductionStepLinkDataId] : 0 
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
                    OutsourceStepRequestFinishDate = item.OutsourceStepRequest.OutsourceStepRequestFinishDate.GetUnix()
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
            var lsOutsourceRequest = await _manufacturingDBContext.OutsourceStepRequest
                .Include(x => x.OutsourceStepRequestData)
                .Where(x => outsourceStepRequestId.Contains(x.OutsourceStepRequestId))
                .ToListAsync();
            foreach (var rq in lsOutsourceRequest)
            {
                var data = rq.OutsourceStepRequestData.Where(x => x.IsImportant == true).ToArray();
                var productionLinkDataIds = data.Select(x => x.ProductionStepLinkDataId);

                var outsourceOrderDetails = await _manufacturingDBContext.OutsourceOrderDetail.AsNoTracking()
                    .Where(x => x.OutsourceOrder.OutsourceTypeId == (int)EnumOutsourceType.OutsourceStep
                        && productionLinkDataIds.Contains(x.ObjectId))
                    .ToListAsync();

                var outsourceOrderIds = outsourceOrderDetails.Select(x => x.OutsourceOrderId).Distinct();

                var totalStatus = (await _manufacturingDBContext.OutsourceTrack.AsNoTracking()
                    .Where(x => outsourceOrderIds.Contains(x.OutsourceOrderId) && !x.ObjectId.HasValue)
                    .ToListAsync())
                    .GroupBy(x => x.OutsourceOrderId)
                    .Select(g => g.OrderByDescending(x => x.OutsourceTrackId).Take(1).FirstOrDefault()?.OutsourceTrackStatusId)
                    .Sum();

                if (totalStatus.GetValueOrDefault() == 0)
                    rq.OutsourceStepRequestStatusId = (int)EnumOutsourceRequestStatusType.Unprocessed;
                else
                {
                    var quantityOrderByRequestDetail = outsourceOrderDetails.GroupBy(x => x.ObjectId)
                                    .ToDictionary(k => k.Key, v => v.Sum(x => x.Quantity));

                    var isCheckOrder = false;
                    foreach (var d in data)
                    {
                        if (!quantityOrderByRequestDetail.ContainsKey(d.ProductionStepLinkDataId)
                            || (d.Quantity - quantityOrderByRequestDetail[d.ProductionStepLinkDataId]) != 0)
                        {
                            isCheckOrder = false;
                            break;
                        }

                        isCheckOrder = true;
                    }
                    if (isCheckOrder && (totalStatus.GetValueOrDefault() == ((int)EnumOutsourceTrackStatus.HandedOver * outsourceOrderIds.Count())))
                        rq.OutsourceStepRequestStatusId = (int)EnumOutsourceRequestStatusType.Processed;
                    else rq.OutsourceStepRequestStatusId = (int)EnumOutsourceRequestStatusType.Processing;
                }
            }

            await _manufacturingDBContext.SaveChangesAsync();

            return true;
        }

        public async Task<IList<OutsourceStepRequestDetailOutput>> GetOutsourceStepRequestDatasByProductionOrderId(long productionOrderId)
        {
            var sqlData = new StringBuilder(@$"SELECT * FROM vOutsourceStepRequestDataExtractInfo v WHERE v.ProductionOrderId = {productionOrderId}");
            var data = (await _manufacturingDBContext.QueryDataTable(sqlData.ToString(), Array.Empty<SqlParameter>())).ConvertData<OutsourceStepRequestDetailOutput>();
            return data;
        }

        private async Task<IList<ProductionStepLinkDataInput>> GetProductionStepLinkDataByListId(long[] lsProductionStepLinkDataId)
        {
            var stepLinkDatas = new List<ProductionStepLinkDataInput>();
            if (lsProductionStepLinkDataId.Length > 0)
            {
                var sql = new StringBuilder("Select * from ProductionStepLinkDataExtractInfo v ");
                var parammeters = new List<SqlParameter>();
                var whereCondition = new StringBuilder();

                whereCondition.Append("v.ProductionStepLinkDataId IN ( ");
                for (int i = 0; i < lsProductionStepLinkDataId.Length; i++)
                {
                    var number = lsProductionStepLinkDataId[i];
                    string pName = $"@ProductionStepLinkDataId{i + 1}";

                    if (i == lsProductionStepLinkDataId.Length - 1)
                        whereCondition.Append($"{pName} )");
                    else
                        whereCondition.Append($"{pName}, ");

                    parammeters.Add(new SqlParameter(pName, number));
                }
                if (whereCondition.Length > 0)
                {
                    sql.Append(" WHERE ");
                    sql.Append(whereCondition);
                }

                stepLinkDatas = (await _manufacturingDBContext.QueryDataTable(sql.ToString(), parammeters.Select(p => p.CloneSqlParam()).ToArray()))
                        .ConvertData<ProductionStepLinkDataInput>();
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

                var entiryRequest = new OutsourceStepRequest
                {
                    OutsourceStepRequestCode = outsourceStepRequestCode,
                    OutsourceStepRequestFinishDate = requestModel.OutsourceStepRequestFinishDate.UnixToDateTime(0),
                    ProductionOrderId = requestModel.ProductionOrderId,
                    IsInvalid = false,
                    OutsourceStepRequestStatusId = (int)EnumOutsourceRequestStatusType.Unprocessed,
                    Setting = requestModel.Setting.JsonSerialize()
                };

                _manufacturingDBContext.OutsourceStepRequest.Add(entiryRequest);
                await _manufacturingDBContext.SaveChangesAsync();

                var lsLinkDataOutput = requestModel.ProductionProcessOutsource.ProductionStepLinkDatas
                    .Where(x => requestModel.ProductionProcessOutsource.ProductionStepLinkDataOutput.Contains(x.ProductionStepLinkDataId));
                // Create outsourceStepRequestData
                foreach (var d in lsLinkDataOutput)
                {
                    if (d.OutsourceQuantity > 0)
                        _manufacturingDBContext.OutsourceStepRequestData.Add(new OutsourceStepRequestData
                        {
                            OutsourceStepRequestId = entiryRequest.OutsourceStepRequestId,
                            ProductionStepLinkDataId = d.ProductionStepLinkDataId,
                            Quantity = d.OutsourceQuantity,
                            ProductionStepLinkDataRoleTypeId = (int) EnumProductionStepLinkDataRoleType.Output,
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
                            OutsourceStepRequestId = entiryRequest.OutsourceStepRequestId,
                            ProductionStepLinkDataId = d.ProductionStepLinkDataId,
                            Quantity = d.ExportOutsourceQuantity,
                            ProductionStepLinkDataRoleTypeId = (int) EnumProductionStepLinkDataRoleType.Input,
                            IsImportant = false,
                            ProductionStepId = d.ProductionStepReceiveId
                        });
                }
                await _manufacturingDBContext.SaveChangesAsync();

                // Update productionStep and linkData
                await SyncInfoForProductionProcess(requestModel.ProductionProcessOutsource, entiryRequest.OutsourceStepRequestId);

                await _customGenCodeHelperService.ConfirmCode(currentConfig.CurrentLastValue);

                await trans.CommitAsync();

                await _activityLogService.CreateLog(EnumObjectType.OutsourceRequest, entiryRequest.OutsourceStepRequestId,
                    $"Thêm mới yêu cầu gia công công đoạn", requestModel.JsonSerialize());

                return new OutsourceStepRequestPrivateKey
                {
                    OutsourceStepRequestCode = entiryRequest.OutsourceStepRequestCode,
                    OutsourceStepRequestId = entiryRequest.OutsourceStepRequestId
                };
            }
            catch (Exception ex)
            {
                await trans.TryRollbackTransactionAsync();
                _logger.LogError(ex, "AddOutsourceStepRequest");
                throw;
            }
        }
    }
}
