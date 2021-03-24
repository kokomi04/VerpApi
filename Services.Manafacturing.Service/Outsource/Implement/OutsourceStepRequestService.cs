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

            var arrOutput = request.OutsourceStepRequestData.Select(x => {
                var role = roles.FirstOrDefault(r => r.ProductionStepLinkDataId == x.ProductionStepLinkDataId && r.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Output);
                var productionStepInfo = request.ProductionStep.FirstOrDefault(s => s.ProductionStepId == role.ProductionStepId);
                return new OutsourceStepRequestDetailOutput
                {
                    ProductionStepLinkDataId = x.ProductionStepLinkDataId,
                    Quantity = x.Quantity.GetValueOrDefault(),
                    TotalOutsourceOrderQuantity = totalOutsourceOrderQuantityMap.ContainsKey(x.ProductionStepLinkDataId) ? totalOutsourceOrderQuantityMap[x.ProductionStepLinkDataId] : 0,
                    RoleType = (int)EnumProductionStepLinkDataRoleType.Output,
                    ProductionStepTitle =$"{productionStepInfo.Step.StepName} #({productionStepInfo.ProductionStepId})"
                };
            }).ToList();



            //var arrLinkDataInput = roles.GroupBy(r => r.ProductionStepLinkDataId)
            //    .Where(g => g.Count() == 1)
            //    .Select(g => g.First())
            //    .Where(l => l.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Input)
            //    .Select(x => x.ProductionStepLinkData);

            //var ldOutPut = roles.FirstOrDefault(x => x.ProductionStepLinkDataId == arrOutput.FirstOrDefault().ProductionStepLinkDataId).ProductionStepLinkData;
            //decimal rateQuantity = Math.Round(ldOutPut.OutsourceQuantity.GetValueOrDefault() / ldOutPut.QuantityOrigin, 5);

            //var arrInput = arrLinkDataInput.Select(x => new OutsourceStepRequestDetailOutput
            //{
            //    ProductionStepLinkDataId = x.ProductionStepLinkDataId,
            //    Quantity = Math.Round(x.QuantityOrigin * rateQuantity, 5),
            //    TotalOutsourceOrderQuantity = Math.Round(Math.Round(arrOutput[0].TotalOutsourceOrderQuantity / arrOutput[0].Quantity, 5) * x.QuantityOrigin, 5),
            //    RoleType = (int)EnumProductionStepLinkDataRoleType.Input
            //});

            //arrOutput.AddRange(arrInput);
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

        public async Task<IList<OutsourceStepRequestDataInfo>> GetOutsourceStepRequestData(long outsourceStepRequestId)
        {
            var outsourceStepRequest = await _manufacturingDBContext.OutsourceStepRequest.AsNoTracking()
                .Include(x => x.OutsourceStepRequestData)
                .ProjectTo<OutsourceStepRequestInfo>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(x => x.OutsourceStepRequestId == outsourceStepRequestId);
            if (outsourceStepRequest == null)
                throw new BadRequestException(OutsourceErrorCode.NotFoundRequest);

            var roles = await _manufacturingDBContext.ProductionStep.AsNoTracking()
                .Include(s => s.ProductionStepLinkDataRole)
                .Where(x => x.ContainerId == outsourceStepRequest.ProductionOrderId && x.ContainerTypeId == (int)EnumContainerType.ProductionOrder)
                .SelectMany(x => x.ProductionStepLinkDataRole, (s, d) => new ProductionStepLinkDataRoleModel
                {
                    ProductionStepId = s.ProductionStepId,
                    ProductionStepLinkDataId = d.ProductionStepLinkDataId,
                    ProductionStepLinkDataRoleTypeId = (EnumProductionStepLinkDataRoleType)d.ProductionStepLinkDataRoleTypeId,
                }).ToListAsync();

            var lsProductionStepId = FoundProductionStepInOutsourceStepRequest(outsourceStepRequest.OutsourceStepRequestData, roles);

            var productionSteps = await _manufacturingDBContext.ProductionStep.AsNoTracking()
                .Include(s => s.Step)
                .Include(s => s.ProductionStepLinkDataRole)
                .ThenInclude(r => r.ProductionStepLinkData)
                .Where(s => lsProductionStepId.Contains(s.ProductionStepId))
                .ProjectTo<ProductionStepInfo>(_mapper.ConfigurationProvider)
                .ToListAsync();

            var lst = new List<OutsourceStepRequestDataInfo>();
            foreach (var productionStep in productionSteps)
            {
                var outsourceStepRequestDatas = productionStep.ProductionStepLinkDatas
                    .Where(x => outsourceStepRequest.OutsourceStepRequestData
                                .Select(y => y.ProductionStepLinkDataId).Contains(x.ProductionStepLinkDataId))
                    .Select(x => new OutsourceStepRequestDataInfo
                    {
                        OutsourceStepRequestCode = outsourceStepRequest.OutsourceStepRequestCode,
                        OutsourceStepRequestId = outsourceStepRequest.OutsourceStepRequestId,
                        ProductionStepId = productionStep.ProductionStepId,
                        ProductionStepTitle = productionStep.Title,
                        ProductionStepLinkDataId = x.ProductionStepLinkDataId,
                        ProductionStepLinkDataQuantity = x.Quantity,
                        ProductionStepLinkDataRoleTypeId = x.ProductionStepLinkDataRoleTypeId,
                        OutsourceStepRequestDataQuantity = outsourceStepRequest.OutsourceStepRequestData.FirstOrDefault(s => s.ProductionStepLinkDataId == x.ProductionStepLinkDataId).OutsourceStepRequestDataQuantity,
                        ProductionStepLinkDataTitle = string.Empty,
                        OutsourceStepRequestFinishDate = outsourceStepRequest.OutsourceStepRequestFinishDate,
                        ProductionOrderCode = outsourceStepRequest.ProductionOrderCode
                    })
                    .ToList();
                if (outsourceStepRequestDatas.Count == 0)
                    lst.Add(new OutsourceStepRequestDataInfo
                    {
                        OutsourceStepRequestCode = outsourceStepRequest.OutsourceStepRequestCode,
                        OutsourceStepRequestId = outsourceStepRequest.OutsourceStepRequestId,
                        ProductionStepId = productionStep.ProductionStepId,
                        ProductionStepTitle = productionStep.Title,
                        ProductionStepLinkDataRoleTypeId = EnumProductionStepLinkDataRoleType.Input,
                        OutsourceStepRequestFinishDate = outsourceStepRequest.OutsourceStepRequestFinishDate
                    });
                else
                    lst.AddRange(outsourceStepRequestDatas);
            }

            var groupbySumQuantityProcessed = await (from order in _manufacturingDBContext.OutsourceOrder.Where(x => x.OutsourceTypeId == (int)EnumContainerType.ProductionOrder)
                                                     join detail in _manufacturingDBContext.OutsourceOrderDetail on order.OutsourceOrderId equals detail.OutsourceOrderId
                                                     group new { order, detail } by detail.ObjectId into g
                                                     select new
                                                     {
                                                         ProductionStepLinkDataId = g.Key,
                                                         OutsourceStepRequestQuantityProcessed = g.Sum(x => x.detail.Quantity)
                                                     }).ToListAsync();

            var output = lst.FirstOrDefault(x => x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Output);
            var outputProcessed = groupbySumQuantityProcessed.FirstOrDefault(x => x.ProductionStepLinkDataId == output.ProductionStepLinkDataId);

            var percent = output.OutsourceStepRequestDataQuantity / output.ProductionStepLinkDataQuantity;
            var percentProcessed = outputProcessed == null ? 0 : outputProcessed.OutsourceStepRequestQuantityProcessed / output.OutsourceStepRequestDataQuantity;
            lst.ForEach(x =>
            {
                x.OutsourceStepRequestDataQuantity = percent * x.ProductionStepLinkDataQuantity;
                x.OutsourceStepRequestDataQuantityProcessed = percentProcessed * x.OutsourceStepRequestDataQuantity;
            });

            return lst;
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

        public async Task<IList<ProductionStepInOutsourceStepRequest>> GetProductionStepHadOutsourceStepRequest(long productionOrderId)
        {
            var outsourceStepRequest = await _manufacturingDBContext.OutsourceStepRequest.AsNoTracking()
                .Include(x => x.OutsourceStepRequestData)
                .Where(x => x.ProductionOrderId == productionOrderId)
                .ProjectTo<OutsourceStepRequestModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

            var groups = outsourceStepRequest.GroupBy(x => x.ProductionOrderId);
            var data = new List<ProductionStepInOutsourceStepRequest>();

            var roles = await _manufacturingDBContext.ProductionStep.AsNoTracking()
               .Include(s => s.ProductionStepLinkDataRole)
               .Where(x => x.ContainerId == productionOrderId && x.ContainerTypeId == (int)EnumContainerType.ProductionOrder)
               .SelectMany(x => x.ProductionStepLinkDataRole, (s, d) => new ProductionStepLinkDataRoleInput
               {
                   ProductionStepId = s.ProductionStepId,
                   ProductionStepLinkDataId = d.ProductionStepLinkDataId,
                   ProductionStepLinkDataRoleTypeId = (EnumProductionStepLinkDataRoleType)d.ProductionStepLinkDataRoleTypeId,
                   ProductionStepCode = s.ProductionStepCode,
               }).ToListAsync();

            foreach (var request in outsourceStepRequest)
            {
                var lst = FoundProductionStepInOutsourceStepRequest(request.OutsourceStepRequestData, roles.Cast<ProductionStepLinkDataRoleModel>().ToList())
                    .Select(productionStepId => new ProductionStepInOutsourceStepRequest
                    {
                        ProductionProcessId = request.ProductionProcessId,
                        ProductionStepId = productionStepId,
                        OutsourceStepRequestCode = request.OutsourceStepRequestCode,
                        OutsourceStepRequestId = request.OutsourceStepRequestId,
                        ProductionStepCode = roles.FirstOrDefault(x => x.ProductionStepId == productionStepId)?.ProductionStepCode
                    });
                data.AddRange(lst);
            }

            return data;
        }

        public async Task<IList<ProductionStepInfo>> GeneralOutsourceStepOfProductionOrder(long productionOrderId)
        {
            var outsourceStepRequest = await _manufacturingDBContext.OutsourceStepRequest.AsNoTracking()
                .Include(x => x.OutsourceStepRequestData)
                .Where(x => x.ProductionOrderId == productionOrderId)
                .ProjectTo<OutsourceStepRequestModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

            var groups = outsourceStepRequest.GroupBy(x => x.ProductionOrderId);
            var data = new List<ProductionStepInOutsourceStepRequest>();

            var roles = await _manufacturingDBContext.ProductionStep.AsNoTracking()
               .Include(s => s.ProductionStepLinkDataRole)
               .Where(x => x.ContainerId == productionOrderId && x.ContainerTypeId == (int)EnumContainerType.ProductionOrder)
               .SelectMany(x => x.ProductionStepLinkDataRole, (s, d) => new ProductionStepLinkDataRoleModel
               {
                   ProductionStepId = s.ProductionStepId,
                   ProductionStepLinkDataId = d.ProductionStepLinkDataId,
                   ProductionStepLinkDataRoleTypeId = (EnumProductionStepLinkDataRoleType)d.ProductionStepLinkDataRoleTypeId,
               }).ToListAsync();

            var results = new List<ProductionStepInfo>();
            foreach (var request in outsourceStepRequest)
            {
                var lst = FoundProductionStepInOutsourceStepRequest(request.OutsourceStepRequestData, roles);
                var detail = request.OutsourceStepRequestData
                    .FirstOrDefault(x => x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Output);
                var linkDataOrigin = _manufacturingDBContext.ProductionStepLinkData.FirstOrDefault(x => x.ProductionStepLinkDataId == detail.ProductionStepLinkDataId);

                var percent = detail.OutsourceStepRequestDataQuantity.Value / linkDataOrigin.Quantity;

                var productionSteps = await _manufacturingDBContext.ProductionStep.AsNoTracking()
                .Include(s => s.Step)
                .Include(s => s.ProductionStepLinkDataRole)
                .ThenInclude(r => r.ProductionStepLinkData)
                .Where(s => lst.Contains(s.ProductionStepId))
                .ProjectTo<ProductionStepInfo>(_mapper.ConfigurationProvider)
                .ToListAsync();

                foreach (var productionStep in productionSteps)
                {
                    productionStep.ProductionStepLinkDatas = productionStep.ProductionStepLinkDatas
                                                                .Where(x =>
                                                                            x.ProductionStepLinkDataRoleTypeId
                                                                            == EnumProductionStepLinkDataRoleType.Output
                                                                 )
                                                                .ToList();

                    productionStep.ProductionStepLinkDatas.ForEach(x =>
                        {
                            x.Quantity *= percent;
                        });
                }

                results.AddRange(productionSteps);
            }
            return results;
        }

        public async Task<bool> UpdateOutsourceStepRequestStatus(long[] outsourceStepRequestId)
        {
            var lsOutsourceRequest = await _manufacturingDBContext.OutsourceStepRequest
                .Include(x => x.OutsourceStepRequestData)
                .Where(x => outsourceStepRequestId.Contains(x.OutsourceStepRequestId))
                .ToListAsync();
            foreach (var rq in lsOutsourceRequest)
            {
                var productionLinkDataIds = rq.OutsourceStepRequestData.Select(x => x.ProductionStepLinkDataId);

                var outsourceOrderDetails = await _manufacturingDBContext.OutsourceOrderDetail.AsNoTracking()
                    .Where(x => x.OutsourceOrder.OutsourceTypeId == (int)EnumOutsourceType.OutsourceStep
                        && productionLinkDataIds.Contains(x.ObjectId))
                    .ToListAsync();

                var outsourceOrderIds = outsourceOrderDetails.Select(x => x.OutsourceOrderId);

                var totalStatus = (await _manufacturingDBContext.OutsourceTrack.AsNoTracking()
                    .Where(x => outsourceOrderIds.Contains(x.OutsourceOrderId)
                        && (!x.ObjectId.HasValue || productionLinkDataIds.Contains(x.ObjectId.GetValueOrDefault())))
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
                    foreach (var d in rq.OutsourceStepRequestData)
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

        #region private
        private async Task<IList<ProductionStepLinkDataInput>> GetProductionStepLinkDataByListId(List<long> lsProductionStepLinkDataId)
        {
            var stepLinkDatas = new List<ProductionStepLinkDataInput>();
            if (lsProductionStepLinkDataId.Count > 0)
            {
                var sql = new StringBuilder("Select * from ProductionStepLinkDataExtractInfo v ");
                var parammeters = new List<SqlParameter>();
                var whereCondition = new StringBuilder();

                whereCondition.Append("v.ProductionStepLinkDataId IN ( ");
                for (int i = 0; i < lsProductionStepLinkDataId.Count; i++)
                {
                    var number = lsProductionStepLinkDataId[i];
                    string pName = $"@ProductionStepLinkDataId{i + 1}";

                    if (i == lsProductionStepLinkDataId.Count - 1)
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

        private IList<long> FoundProductionStepInOutsourceStepRequest(IList<OutsourceStepRequestDataModel> outsourceStepRequestDatas, List<ProductionStepLinkDataRoleModel> roles)
        {
            var outputData = outsourceStepRequestDatas
                .Where(x => x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Output)
                .Select(x => x.ProductionStepLinkDataId)
                .ToList();

            var inputData = outsourceStepRequestDatas
                .Where(x => x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Input)
                .Select(x => x.ProductionStepLinkDataId)
                .ToList();

            var productionStepStartId = roles.Where(x => inputData.Contains(x.ProductionStepLinkDataId)
                   && x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Input)
                .Select(x => x.ProductionStepId)
                .Distinct()
                .ToList();
            var productionStepEndId = roles.Where(x => outputData.Contains(x.ProductionStepLinkDataId)
                     && x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Output)
                .Select(x => x.ProductionStepId)
                .Distinct()
                .ToList();

            var lsProductionStepId = new List<long>();
            foreach (var id in productionStepEndId)
                FindTraceProductionStep(inputData, roles, productionStepStartId, lsProductionStepId, id);

            return lsProductionStepId
                    .Union(productionStepEndId)
                    .Union(productionStepStartId)
                    .Distinct()
                    .ToList();
        }

        private void FindTraceProductionStep(List<long> inputLinkData, List<ProductionStepLinkDataRoleModel> roles, List<long> productionStepStartId, List<long> result, long productionStepId)
        {
            var roleInput = roles.Where(x => x.ProductionStepId == productionStepId
                    && !inputLinkData.Contains(x.ProductionStepLinkDataId)
                    && x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Input)
                .ToList();
            foreach (var input in roleInput)
            {
                var roleOutput = roles.Where(x => x.ProductionStepLinkDataId == input.ProductionStepLinkDataId
                        && x.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Output)
                    .FirstOrDefault();

                if (roleOutput == null) continue;

                result.Add(roleOutput.ProductionStepId);
                FindTraceProductionStep(inputLinkData, roles, productionStepStartId, result, roleOutput.ProductionStepId);
            }
        }

        private async Task<bool> UpdateProductionStepLinkDataRelative(long outsourceStepRequestId, IList<OutsourceStepRequestData> outsourceStepRequestDatas, IList<long> lsProductionStep, decimal newPercent, decimal oldPercent = 0)
        {
            var productionSteps = await _manufacturingDBContext.ProductionStep.AsNoTracking()
                                        .Where(x => lsProductionStep.Contains(x.ProductionStepId))
                                        .ProjectTo<ProductionStepInfo>(_mapper.ConfigurationProvider)
                                        .ToListAsync();

            var selectLinkDataIds = productionSteps.SelectMany(x => x.ProductionStepLinkDatas.Where(x => x.ProductionStepLinkDataTypeId == EnumProductionStepLinkDataType.None)).Select(x => x.ProductionStepLinkDataId).Distinct().ToList();
            var productionStepLinkDataEntity = await _manufacturingDBContext.ProductionStepLinkData
                                                        .Where(x => selectLinkDataIds.Contains(x.ProductionStepLinkDataId))
                                                        .ToListAsync();
            var productionStepLinkDataInfo = await GetProductionStepLinkDataByListId(productionStepLinkDataEntity.Select(x => x.ProductionStepLinkDataId).ToList());

            foreach (var linkData in productionStepLinkDataEntity)
            {
                var info = productionStepLinkDataInfo.FirstOrDefault(x => x.ProductionStepLinkDataId == linkData.ProductionStepLinkDataId);

                if (!linkData.OutsourceQuantity.HasValue)
                    linkData.OutsourceQuantity = decimal.Zero;
                if (!linkData.ExportOutsourceQuantity.HasValue)
                    linkData.ExportOutsourceQuantity = decimal.Zero;
                var oldValue = linkData.Quantity * oldPercent;
                var newValue = linkData.Quantity * newPercent;


                var requestData = outsourceStepRequestDatas.FirstOrDefault(x => x.ProductionStepLinkDataId == linkData.ProductionStepLinkDataId);

                if (requestData != null && requestData.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Input)
                {
                    linkData.ExportOutsourceQuantity += (newValue - oldValue);
                    if (linkData.ExportOutsourceQuantity > linkData.Quantity)
                        throw new BadRequestException(OutsourceErrorCode.InValidRequestOutsource, $"Số lượng gia công của chi tiết \"{info.ObjectTitle}\" vượt qua cho phép");
                }
                else
                {
                    linkData.OutsourceQuantity += (newValue - oldValue);
                    if (linkData.OutsourceQuantity > linkData.Quantity && requestData != null)
                        throw new BadRequestException(OutsourceErrorCode.InValidRequestOutsource, $"Số lượng gia công của chi tiết \"{info.ObjectTitle}\" vượt qua cho phép");
                }
            }

            await _manufacturingDBContext.SaveChangesAsync();

            //if (oldPercent == decimal.Zero)
            //    await CreateLinkDataAndRoleOutsourceStep(outsourceStepRequestDatas, productionStepLinkDataEntity);
            //else if (newPercent == decimal.Zero)
            //    await DeleteLinkDataAndRoleOutsourceStep(outsourceStepRequestId);
            //else
            //    await UpdateLinkDataAndRoleOutsourceStep(outsourceStepRequestId, productionStepLinkDataEntity);
            return true;
        }

        private async Task CreateLinkDataAndRoleOutsourceStep(List<OutsourceStepRequestData> outsourceStepRequestDatas, List<ProductionStepLinkData> productionStepLinkDataEntity)
        {
            var roles = await _manufacturingDBContext.ProductionStepLinkDataRole.AsNoTracking()
                                        .Where(x => outsourceStepRequestDatas.Select(y => y.ProductionStepLinkDataId).Contains(x.ProductionStepLinkDataId))
                                        .ToListAsync();

            var newStepLinkDataEntity = new List<ProductionStepLinkData>();
            var newRoleEntity = new List<ProductionStepLinkDataRoleInput>();

            foreach (var outsourceData in outsourceStepRequestDatas)
            {
                var roleType = (int)EnumProductionStepLinkDataRoleType.Input;
                if (outsourceData.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Input)
                    roleType = (int)EnumProductionStepLinkDataRoleType.Output;

                var linkData = productionStepLinkDataEntity.FirstOrDefault(x => x.ProductionStepLinkDataId == outsourceData.ProductionStepLinkDataId);
                var role = roles.FirstOrDefault(x => x.ProductionStepLinkDataRoleTypeId == roleType && x.ProductionStepLinkDataId == linkData.ProductionStepLinkDataId);
                if (role == null) continue;

                var nLinkData = new ProductionStepLinkData
                {
                    ProductionStepLinkDataCode = $"{linkData.ProductionStepLinkDataCode}/{outsourceData.OutsourceStepRequestId}",
                    Quantity = linkData.OutsourceQuantity.Value,
                    ObjectId = linkData.ObjectId,
                    ObjectTypeId = linkData.ObjectTypeId,
                    OutsourceRequestDetailId = outsourceData.OutsourceStepRequestId,
                    ProductionStepLinkDataTypeId = (int)EnumProductionStepLinkDataType.StepLinkDataOutsourceStep,
                };
                var nRole = new ProductionStepLinkDataRoleInput
                {
                    ProductionStepId = role.ProductionStepId,
                    ProductionStepLinkDataRoleTypeId = (EnumProductionStepLinkDataRoleType)role.ProductionStepLinkDataRoleTypeId,
                    ProductionStepLinkDataCode = nLinkData.ProductionStepLinkDataCode
                };

                newStepLinkDataEntity.Add(nLinkData);
                newRoleEntity.Add(nRole);
            }

            await _manufacturingDBContext.ProductionStepLinkData.AddRangeAsync(newStepLinkDataEntity);
            await _manufacturingDBContext.SaveChangesAsync();
            foreach (var r in newRoleEntity)
            {
                var d = newStepLinkDataEntity.FirstOrDefault(x => x.ProductionStepLinkDataCode == r.ProductionStepLinkDataCode);
                r.ProductionStepLinkDataId = d.ProductionStepLinkDataId;
            }
            await _manufacturingDBContext.ProductionStepLinkDataRole.AddRangeAsync(_mapper.Map<IList<ProductionStepLinkDataRole>>(newRoleEntity));
            await _manufacturingDBContext.SaveChangesAsync();
        }

        private async Task UpdateLinkDataAndRoleOutsourceStep(long outsourceStepRequestId, List<ProductionStepLinkData> productionStepLinkDataEntity)
        {
            var linkDataOutsource = await _manufacturingDBContext.ProductionStepLinkData
                                            .Where(x => x.ProductionStepLinkDataTypeId == (int)EnumProductionStepLinkDataType.StepLinkDataOutsourceStep
                                                      && x.OutsourceRequestDetailId == outsourceStepRequestId)
                                            .ToListAsync();
            foreach (var ld in linkDataOutsource)
            {
                var t = productionStepLinkDataEntity.FirstOrDefault(x => x.ProductionStepLinkDataCode == ld.ProductionStepLinkDataCode.Substring(0, ld.ProductionStepLinkDataCode.IndexOf("/")));
                ld.Quantity = t.OutsourceQuantity.Value;
            }

            await _manufacturingDBContext.SaveChangesAsync();
        }

        private async Task DeleteLinkDataAndRoleOutsourceStep(long outsourceStepRequestId)
        {
            var linkDataOutsource = await _manufacturingDBContext.ProductionStepLinkData
                                            .Where(x => x.ProductionStepLinkDataTypeId == (int)EnumProductionStepLinkDataType.StepLinkDataOutsourceStep
                                                      && x.OutsourceRequestDetailId == outsourceStepRequestId)
                                            .ToListAsync();
            var roles = await _manufacturingDBContext.ProductionStepLinkDataRole
                                .Where(x => linkDataOutsource.Select(y => y.ProductionStepLinkDataId).Contains(x.ProductionStepLinkDataId))
                                .ToListAsync();

            linkDataOutsource.ForEach(x => x.IsDeleted = true);
            _manufacturingDBContext.ProductionStepLinkDataRole.RemoveRange(roles);

            await _manufacturingDBContext.SaveChangesAsync();
        }
        #endregion

        //refactor
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
                            ProductionStepLinkDataRoleTypeId = 2,
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
