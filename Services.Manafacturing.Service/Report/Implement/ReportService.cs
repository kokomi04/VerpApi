using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Manafacturing.Model.Step;
using Microsoft.Data.SqlClient;
using StepEnity = VErp.Infrastructure.EF.ManufacturingDB.Step;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;
using VErp.Services.Manafacturing.Model.ProductionHandover;
using VErp.Services.Manafacturing.Model.Report;
using VErp.Services.Manafacturing.Model.ProductionOrder;
using VErp.Services.Manafacturing.Model.Outsource.RequestPart;
using VErp.Services.Manafacturing.Model.Outsource.RequestStep;
using VErp.Services.Manafacturing.Model.ProductionStep;

namespace VErp.Services.Manafacturing.Service.Report.Implement
{
    public class ReportService : IReportService
    {
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly ICurrentContextService _currentContextService;
        public ReportService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<ReportService> logger
            , IMapper mapper
            , ICurrentContextService currentContextService)
        {
            _manufacturingDBContext = manufacturingDB;
            _activityLogService = activityLogService;
            _logger = logger;
            _mapper = mapper;
            _currentContextService = currentContextService;
        }

        public async Task<IList<StepModel>> GetSteps(long fromDate, long toDate)
        {
            var fromDateTime = fromDate.UnixToDateTime();
            var toDateTime = toDate.UnixToDateTime();
            var steps = await (from s in _manufacturingDBContext.Step
                               join ps in _manufacturingDBContext.ProductionStep on s.StepId equals ps.StepId
                               //join pso in _manufacturingDBContext.ProductionStepOrder on ps.ProductionStepId equals pso.ProductionStepId
                               //join pod in _manufacturingDBContext.ProductionOrderDetail on pso.ProductionOrderDetailId equals pod.ProductionOrderDetailId
                               join po in _manufacturingDBContext.ProductionOrder on new { ps.ContainerId, ps.ContainerTypeId } equals new { ContainerId = po.ProductionOrderId, ContainerTypeId = (int)EnumContainerType.ProductionOrder }
                               where po.StartDate <= toDateTime && po.EndDate >= fromDateTime
                               select s).Distinct().ProjectTo<StepModel>(_mapper.ConfigurationProvider).ToListAsync();

            return steps;
        }

        public async Task<IList<StepProgressModel>> GetProductionProgressReport(long fromDate, long toDate, int[] stepIds)
        {
            var fromDateTime = fromDate.UnixToDateTime();
            var toDateTime = toDate.UnixToDateTime();

            var productionSteps = (from ps in _manufacturingDBContext.ProductionStep
                                   join po in _manufacturingDBContext.ProductionOrder on new { ps.ContainerId, ps.ContainerTypeId } equals new { ContainerId = po.ProductionOrderId, ContainerTypeId = (int)EnumContainerType.ProductionOrder }
                                   where stepIds.Contains(ps.StepId.Value) && po.StartDate <= toDateTime && po.EndDate >= fromDateTime
                                   select new
                                   {
                                       StepId = ps.StepId.Value,
                                       ps.ProductionStepId,
                                       po.ProductionOrderCode,
                                       po.ProductionOrderId,
                                   }).ToList();

            var productionStepIds = productionSteps.Select(ps => ps.ProductionStepId).Distinct().ToList();

            var data = (from r in _manufacturingDBContext.ProductionStepLinkDataRole
                        join d in _manufacturingDBContext.ProductionStepLinkData on r.ProductionStepLinkDataId equals d.ProductionStepLinkDataId
                        where productionStepIds.Contains(r.ProductionStepId)
                        select new
                        {
                            r.ProductionStepId,
                            r.ProductionStepLinkDataRoleTypeId,
                            d.ObjectTypeId,
                            d.ObjectId,
                            d.QuantityOrigin,
                            d.OutsourcePartQuantity,
                            d.OutsourceQuantity,
                            d.ExportOutsourceQuantity
                        })
                        .ToList()
                        .GroupBy(d => d.ProductionStepId)
                        .ToDictionary(g => g.Key, g => g.ToList()); ;

            var productionOrderIds = productionSteps.Select(ps => ps.ProductionOrderId).Distinct().ToList();

            var productMap = _manufacturingDBContext.ProductionOrderDetail
                .Where(pod => productionOrderIds.Contains(pod.ProductionOrderId))
                .Select(pod => new
                {
                    pod.ProductionOrderId,
                    pod.ProductId
                })
                .ToList()
                .GroupBy(pod => pod.ProductionOrderId)
                .ToDictionary(g => g.Key, g => g.Select(d => d.ProductId).ToArray());

            var handovers = _manufacturingDBContext.ProductionHandover
            .Where(h => productionOrderIds.Contains(h.ProductionOrderId))
            .Where(h => h.Status == (int)EnumHandoverStatus.Accepted)
            .ToList();

            var reqInventorys = new Dictionary<long, List<ProductionInventoryRequirementEntity>>();

            foreach (var productionOrderId in productionOrderIds)
            {
                var parammeters = new SqlParameter[]
                {
                    new SqlParameter("@ProductionOrderId", productionOrderId)
                };

                var resultData = await _manufacturingDBContext.ExecuteDataProcedure("asp_ProductionHandover_GetInventoryRequirementByProductionOrder_new", parammeters);

                reqInventorys.Add(productionOrderId, resultData.ConvertData<ProductionInventoryRequirementEntity>()
                    .Where(r => r.Status == (int)EnumProductionInventoryRequirementStatus.Accepted)
                    .ToList());
            }


            var report = new List<StepProgressModel>();
            foreach (var stepProgress in productionSteps.GroupBy(ps => ps.StepId))
            {
                var stepProgressModel = new StepProgressModel
                {
                    StepId = stepProgress.Key
                };

                foreach (var productionStepProgressModel in stepProgress.GroupBy(s => s.ProductionStepId))
                {
                    var productionStepId = productionStepProgressModel.Key;

                    foreach (var stepProductionOrderProgress in productionStepProgressModel.GroupBy(s => s.ProductionOrderId))
                    {
                        var productionOrderId = stepProductionOrderProgress.Key;

                        var stepProductionProgressModel = new StepProductionOrderProgressModel
                        {
                            ProductionOrderCode = stepProductionOrderProgress.First().ProductionOrderCode,
                            ProductIds = productMap.ContainsKey(productionOrderId) ? productMap[productionOrderId] : Array.Empty<int>()
                        };

                        if (data.ContainsKey(productionStepId))
                        {
                            var stepData = data[productionStepId];
                            stepProductionProgressModel.InputData = stepData
                            .Where(d => d.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Input)
                            .GroupBy(d => new { d.ObjectTypeId, d.ObjectId })
                            .Select(g => new StepProgressDataModel
                            {
                                ObjectId = g.Key.ObjectId,
                                ObjectTypeId = (EnumProductionStepLinkDataObjectType)g.Key.ObjectTypeId,
                                TotalQuantity = g.Sum(d => d.QuantityOrigin - d.OutsourcePartQuantity.GetValueOrDefault() - (d.ExportOutsourceQuantity ?? d.OutsourceQuantity.GetValueOrDefault()))
                            }).ToList();

                            stepProductionProgressModel.OutputData = stepData
                                .Where(d => d.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Output)
                                .GroupBy(d => new { d.ObjectTypeId, d.ObjectId })
                                .Select(g => new StepProgressDataModel
                                {
                                    ObjectId = g.Key.ObjectId,
                                    ObjectTypeId = (EnumProductionStepLinkDataObjectType)g.Key.ObjectTypeId,
                                    TotalQuantity = g.Sum(d => d.QuantityOrigin - d.OutsourcePartQuantity.GetValueOrDefault() - d.OutsourceQuantity.GetValueOrDefault())
                                }).ToList();

                            var stepInputHandovers = handovers
                                .Where(h => h.ProductionOrderId == productionOrderId && h.ToProductionStepId == productionStepId)
                                .ToList();

                            var stepOutputHandovers = handovers
                                .Where(h => h.ProductionOrderId == productionOrderId && h.FromProductionStepId == productionStepId)
                                .ToList();

                            var stepInputInventory = reqInventorys[productionOrderId].Where(i => i.InventoryTypeId == (int)EnumInventoryType.Output /*&& i.ProductionStepId == productionStepId*/).ToList();
                            var stepOutputInventory = reqInventorys[productionOrderId].Where(i => i.InventoryTypeId == (int)EnumInventoryType.Input /*&& i.ProductionStepId == productionStepId*/).ToList();

                            foreach (var input in stepProductionProgressModel.InputData)
                            {
                                var receivedQuantity = stepInputHandovers.Where(h => h.ObjectId == input.ObjectId && h.ObjectTypeId == (int)input.ObjectTypeId).Sum(h => h.HandoverQuantity);
                                if (input.ObjectTypeId == EnumProductionStepLinkDataObjectType.Product)
                                {
                                    receivedQuantity += stepInputInventory.Where(i => i.ProductId == (int)input.ObjectId).Sum(i => i.ActualQuantity);
                                }
                                input.ReceivedQuantity = receivedQuantity;
                            }

                            foreach (var output in stepProductionProgressModel.OutputData)
                            {
                                var receivedQuantity = stepOutputHandovers.Where(h => h.ObjectId == output.ObjectId && h.ObjectTypeId == (int)output.ObjectTypeId).Sum(h => h.HandoverQuantity);
                                if (output.ObjectTypeId == EnumProductionStepLinkDataObjectType.Product)
                                {
                                    receivedQuantity += stepOutputInventory.Where(i => i.ProductId == (int)output.ObjectId).Sum(i => i.ActualQuantity);
                                }
                                output.ReceivedQuantity = receivedQuantity;

                                if (output.TotalQuantity > 0 && output.ReceivedQuantity * 100 / output.TotalQuantity > stepProductionProgressModel.ProgressPercent)
                                    stepProductionProgressModel.ProgressPercent = Math.Round(output.ReceivedQuantity * 100 / output.TotalQuantity, 2);
                            }
                        }
                        stepProgressModel.StepProgress.Add(stepProductionProgressModel);
                    }
                }

                report.Add(stepProgressModel);
            }

            return report;
        }


        public async Task<ProductionOrderStepModel> GetProductionOrderStepProgress(long fromDate, long toDate)
        {
            var fromDateTime = fromDate.UnixToDateTime();
            var toDateTime = toDate.UnixToDateTime();
            var report = new ProductionOrderStepModel();

            var productionOrderParammeters = new List<SqlParameter>
            {
                new SqlParameter("@FromDate", fromDateTime),
                new SqlParameter("@ToDate", toDateTime)
            };
            string sql = "SELECT * FROM vProductionOrderDetail v WHERE v.StartDate <= @ToDate AND v.EndDate >= @FromDate ORDER BY v.ProductionOrderId";
            var productionOrderData = await _manufacturingDBContext.QueryDataTable(sql, productionOrderParammeters.ToArray());
            var productionOrderDetails = productionOrderData
                .ConvertData<ProductionOrderListEntity>()
                .AsQueryable()
                .ProjectTo<ProductionOrderInfoModel>(_mapper.ConfigurationProvider)
                .ToList();

            report.ProductionOrderDetails = productionOrderDetails;

            var productionOrderIds = productionOrderDetails.Select(pod => pod.ProductionOrderId).Distinct().ToList();

            var productionSteps = (from ps in _manufacturingDBContext.ProductionStep
                                   join s in _manufacturingDBContext.Step on ps.StepId equals s.StepId
                                   where productionOrderIds.Contains(ps.ContainerId) && ps.ContainerTypeId == (int)EnumContainerType.ProductionOrder
                                   select new
                                   {
                                       StepId = ps.StepId.Value,
                                       s.StepName,
                                       ps.ProductionStepId,
                                       ProductionOrderId = ps.ContainerId
                                   }).ToList();

            if (productionSteps.Count == 0) return report;

            var productionStepIds = productionSteps.Select(ps => ps.ProductionStepId).Distinct().ToList();

            var data = (from r in _manufacturingDBContext.ProductionStepLinkDataRole
                        join d in _manufacturingDBContext.ProductionStepLinkData on r.ProductionStepLinkDataId equals d.ProductionStepLinkDataId
                        where productionStepIds.Contains(r.ProductionStepId)
                        select new
                        {
                            r.ProductionStepId,
                            r.ProductionStepLinkDataRoleTypeId,
                            d.ObjectTypeId,
                            d.ObjectId,
                            d.QuantityOrigin,
                            d.OutsourcePartQuantity,
                            d.OutsourceQuantity,
                            d.ExportOutsourceQuantity
                        })
                        .ToList()
                        .GroupBy(d => d.ProductionStepId)
                        .ToDictionary(g => g.Key, g => g.ToList());

            var handovers = _manufacturingDBContext.ProductionHandover
                .Where(h => productionOrderIds.Contains(h.ProductionOrderId))
                .Where(h => h.Status == (int)EnumHandoverStatus.Accepted)
                .ToList();

            var reqInventorys = new Dictionary<long, List<ProductionInventoryRequirementEntity>>();

            foreach (var productionOrderId in productionOrderIds)
            {
                var parammeters = new SqlParameter[]
                {
                    new SqlParameter("@ProductionOrderId", productionOrderId)
                };

                var resultData = await _manufacturingDBContext.ExecuteDataProcedure("asp_ProductionHandover_GetInventoryRequirementByProductionOrder_new", parammeters);

                reqInventorys.Add(productionOrderId, resultData.ConvertData<ProductionInventoryRequirementEntity>()
                    .Where(r => r.Status == (int)EnumProductionInventoryRequirementStatus.Accepted)
                    .ToList());
            }

            foreach (var stepProgress in productionSteps.GroupBy(ps => ps.StepId))
            {
                report.Steps.Add(new StepInfoModel
                {
                    StepId = stepProgress.Key,
                    StepName = stepProgress.First().StepName
                });

                var productionOrderStepItem = productionOrderIds.ToDictionary(id => id, id => new List<ProductionOrderStepProgressModel>());
                report.ProductionOrderStepProgress.Add(stepProgress.Key, productionOrderStepItem);

                foreach (var productionStepProgressModel in stepProgress.GroupBy(s => s.ProductionStepId))
                {
                    var productionStepId = productionStepProgressModel.Key;

                    foreach (var stepProductionOrderProgress in productionStepProgressModel.GroupBy(s => s.ProductionOrderId))
                    {
                        var productionOrderId = stepProductionOrderProgress.Key;
                        if (data.ContainsKey(productionStepId))
                        {
                            var stepData = data[productionStepId];
                            var inputData = stepData
                            .Where(d => d.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Input)
                            .GroupBy(d => new { d.ObjectTypeId, d.ObjectId })
                            .Select(g => new StepProgressDataModel
                            {
                                ObjectId = g.Key.ObjectId,
                                ObjectTypeId = (EnumProductionStepLinkDataObjectType)g.Key.ObjectTypeId,
                                TotalQuantity = g.Sum(d => d.QuantityOrigin - d.OutsourcePartQuantity.GetValueOrDefault() - (d.ExportOutsourceQuantity ?? d.OutsourceQuantity.GetValueOrDefault()))
                            }).ToList();

                            var outputData = stepData
                                .Where(d => d.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Output)
                                .GroupBy(d => new { d.ObjectTypeId, d.ObjectId })
                                .Select(g => new StepProgressDataModel
                                {
                                    ObjectId = g.Key.ObjectId,
                                    ObjectTypeId = (EnumProductionStepLinkDataObjectType)g.Key.ObjectTypeId,
                                    TotalQuantity = g.Sum(d => d.QuantityOrigin - d.OutsourcePartQuantity.GetValueOrDefault() - d.OutsourceQuantity.GetValueOrDefault())
                                }).ToList();

                            var stepInputHandovers = handovers
                                .Where(h => h.ProductionOrderId == productionOrderId && h.ToProductionStepId == productionStepId)
                                .ToList();

                            var stepOutputHandovers = handovers
                                .Where(h => h.ProductionOrderId == productionOrderId && h.FromProductionStepId == productionStepId)
                                .ToList();

                            var stepInputInventory = reqInventorys[productionOrderId].Where(i => i.InventoryTypeId == (int)EnumInventoryType.Output /*&& i.ProductionStepId == productionStepId*/).ToList();
                            var stepOutputInventory = reqInventorys[productionOrderId].Where(i => i.InventoryTypeId == (int)EnumInventoryType.Input/* && i.ProductionStepId == productionStepId*/).ToList();

                            foreach (var input in inputData)
                            {
                                var receivedQuantity = stepInputHandovers.Where(h => h.ObjectId == input.ObjectId && h.ObjectTypeId == (int)input.ObjectTypeId).Sum(h => h.HandoverQuantity);
                                if (input.ObjectTypeId == EnumProductionStepLinkDataObjectType.Product)
                                {
                                    receivedQuantity += stepInputInventory.Where(i => i.ProductId == (int)input.ObjectId).Sum(i => i.ActualQuantity);
                                }
                                input.ReceivedQuantity = receivedQuantity;
                            }

                            decimal progressPercent = 0;

                            foreach (var output in outputData)
                            {
                                var receivedQuantity = stepOutputHandovers.Where(h => h.ObjectId == output.ObjectId && h.ObjectTypeId == (int)output.ObjectTypeId).Sum(h => h.HandoverQuantity);
                                if (output.ObjectTypeId == EnumProductionStepLinkDataObjectType.Product)
                                {
                                    receivedQuantity += stepOutputInventory.Where(i => i.ProductId == (int)output.ObjectId).Sum(i => i.ActualQuantity);
                                }
                                output.ReceivedQuantity = receivedQuantity;

                                if (output.TotalQuantity > 0 && output.ReceivedQuantity * 100 / output.TotalQuantity > progressPercent)
                                    progressPercent = Math.Round(output.ReceivedQuantity * 100 / output.TotalQuantity, 2);
                            }

                            foreach (var item in stepProductionOrderProgress)
                            {
                                report.ProductionOrderStepProgress[stepProgress.Key][item.ProductionOrderId].Add(new ProductionOrderStepProgressModel
                                {
                                    InputData = inputData,
                                    OutputData = outputData,
                                    ProgressPercent = progressPercent
                                });
                            }
                        }
                    }
                }
            }

            return report;
        }


        public async Task<IList<ProductionReportModel>> GetProductionOrderReport(long fromDate, long toDate)
        {
            var fromDateTime = fromDate.UnixToDateTime();
            var toDateTime = toDate.UnixToDateTime();
            if (!fromDateTime.HasValue || !toDateTime.HasValue)
                throw new BadRequestException(GeneralCode.InvalidParams, "Vui lòng chọn ngày bắt đầu, ngày kết thúc");
            var parammeters = new List<SqlParameter>
            {
                new SqlParameter("@FromDate", fromDateTime),
                new SqlParameter("@ToDate", toDateTime)
            };
            string sql = "SELECT * FROM vProductionOrderDetail v WHERE v.StartDate <= @ToDate AND v.EndDate >= @FromDate ORDER BY v.ProductionOrderId";
            var resultData = await _manufacturingDBContext.QueryDataTable(sql, parammeters.ToArray());
            var productionOrderDetails = resultData
                .ConvertData<ProductionOrderListEntity>()
                .AsQueryable()
                .ProjectTo<ProductionReportModel>(_mapper.ConfigurationProvider)
                .ToList();

            var productionOrderIds = productionOrderDetails.Select(pod => pod.ProductionOrderId).Distinct().ToList();

            var handovers = _manufacturingDBContext.ProductionHandover
                .Where(h => productionOrderIds.Contains(h.ProductionOrderId))
                .Where(h => h.Status == (int)EnumHandoverStatus.Accepted)
                .ToList();

            var reqInventorys = new Dictionary<long, List<ProductionInventoryRequirementEntity>>();

            foreach (var productionOrderId in productionOrderIds)
            {
                var invParammeters = new SqlParameter[]
                {
                new SqlParameter("@ProductionOrderId", productionOrderId)
                };

                var invData = await _manufacturingDBContext.ExecuteDataProcedure("asp_ProductionHandover_GetInventoryRequirementByProductionOrder_new", invParammeters);

                reqInventorys.Add(productionOrderId, invData.ConvertData<ProductionInventoryRequirementEntity>()
                    .Where(r => r.InventoryTypeId == (int)EnumInventoryType.Input && r.Status == (int)EnumProductionInventoryRequirementStatus.Accepted)
                    .ToList());
            }

            var productionSteps = (from ps in _manufacturingDBContext.ProductionStep
                                   join s in _manufacturingDBContext.Step on ps.StepId equals s.StepId
                                   join po in _manufacturingDBContext.ProductionOrder on new { ps.ContainerId, ps.ContainerTypeId } equals new { ContainerId = po.ProductionOrderId, ContainerTypeId = (int)EnumContainerType.ProductionOrder }
                                   where productionOrderIds.Contains(po.ProductionOrderId)
                                   select new
                                   {
                                       s.StepName,
                                       ps.ProductionStepId,
                                       po.ProductionOrderId,
                                   })
                                   .ToList();

            var productionStepIds = productionSteps.Select(ps => ps.ProductionStepId).Distinct().ToList();
            var productionOrderSteps = productionSteps.GroupBy(d => d.ProductionOrderId).ToDictionary(g => g.Key, g => g.ToList());

            var outputData = (from r in _manufacturingDBContext.ProductionStepLinkDataRole
                              join d in _manufacturingDBContext.ProductionStepLinkData on r.ProductionStepLinkDataId equals d.ProductionStepLinkDataId
                              where productionStepIds.Contains(r.ProductionStepId) && r.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Output
                              select new
                              {
                                  r.ProductionStepId,
                                  d.ObjectTypeId,
                                  d.ObjectId,
                                  d.QuantityOrigin,
                                  d.OutsourcePartQuantity,
                                  d.OutsourceQuantity
                              })
                              .ToList()
                              .GroupBy(d => d.ProductionStepId)
                              .ToDictionary(g => g.Key, g => g.ToList());


            Dictionary<long, string> unfinishedStepMap = new Dictionary<long, string>();

            foreach (var productionOrderId in productionOrderIds)
            {
                var stepTitle = new StringBuilder();

                foreach (var productionStep in productionOrderSteps[productionOrderId])
                {
                    if (!outputData.ContainsKey(productionStep.ProductionStepId)) continue;
                    if (stepTitle.StringContains(productionStep.StepName)) continue;
                    var outputStepData = outputData[productionStep.ProductionStepId];

                    var output = outputStepData
                            .GroupBy(d => new { d.ObjectTypeId, d.ObjectId })
                            .Select(g => new
                            {
                                g.Key.ObjectId,
                                g.Key.ObjectTypeId,
                                TotalQuantity = g.Sum(d => d.QuantityOrigin - d.OutsourcePartQuantity.GetValueOrDefault() - d.OutsourceQuantity.GetValueOrDefault())
                            })
                            .ToList();

                    var stepOutputHandovers = handovers
                        .Where(h => h.ProductionOrderId == productionStep.ProductionStepId && h.FromProductionStepId == productionStep.ProductionStepId)
                        .ToList();

                    var stepOutputInventory = reqInventorys[productionOrderId]
                        //.Where(i => i.ProductionStepId == productionStep.ProductionStepId)
                        .ToList();

                    if (output.Any(o =>
                    {
                        var receivedQuantity = stepOutputHandovers.Where(h => h.ObjectId == o.ObjectId && h.ObjectTypeId == (int)o.ObjectTypeId).Sum(h => h.HandoverQuantity);
                        if (o.ObjectTypeId == (int)EnumProductionStepLinkDataObjectType.Product)
                        {
                            receivedQuantity += stepOutputInventory.Where(i => i.ProductId == (int)o.ObjectId).Sum(i => i.ActualQuantity);
                        }
                        return receivedQuantity < o.TotalQuantity;
                    }))
                    {
                        stepTitle.Append(productionStep.StepName);
                        stepTitle.Append(" - ");
                    }
                }

                unfinishedStepMap.Add(productionOrderId, stepTitle.ToString().Trim().Trim('-').Trim());
            }

            foreach (var productionOrderDetail in productionOrderDetails)
            {
                productionOrderDetail.UnfinishedStepTitle = unfinishedStepMap[productionOrderDetail.ProductionOrderId];

                var productImportQuantity = reqInventorys[productionOrderDetail.ProductionOrderId]
                      .Where(i => i.ProductId == productionOrderDetail.ProductId)
                      .Sum(i => i.ActualQuantity);

                productionOrderDetail.CompletedQuantity = productImportQuantity;
            }

            return productionOrderDetails;
        }


        public async Task<IList<ProcessingOrderListModel>> GetProcessingOrderList()
        {
            var sql = @$"SELECT 
                    v.ProductionOrderId,
                    v.ProductionOrderCode,
                    v.ProductTitle,
                    v.StartDate,
                    v.EndDate  
                FROM vProductionOrder v 
                WHERE v.ProductionOrderStatus = {(int)EnumProductionStatus.Processing} OR v.ProductionOrderStatus = {(int)EnumProductionStatus.OverDeadline}";

            var resultData = await _manufacturingDBContext.QueryDataTable(sql, Array.Empty<SqlParameter>());
            var lst = resultData
                .ConvertData<ProcessingOrderListEntity>()
                .AsQueryable()
                .ProjectTo<ProcessingOrderListModel>(_mapper.ConfigurationProvider)
                .ToList();

            var productionOrderIds = lst.Select(s => s.ProductionOrderId).Distinct().ToList();

            var steps = (from ps in _manufacturingDBContext.ProductionStep 
                         join s in _manufacturingDBContext.Step on ps.StepId equals s.StepId
                         where productionOrderIds.Contains(ps.ContainerId) && ps.ContainerTypeId == (int) EnumContainerType.ProductionOrder
                         select new
                         {
                             s.StepId,
                             s.StepName,
                             ProductionOrderId = ps.ContainerId
                         })
                         .ToList()
                         .GroupBy(s => s.ProductionOrderId)
                         .ToDictionary(g => g.Key, g => g.Select(s => new StepListModel
                         {
                             StepId = s.StepId,
                             StepName = s.StepName
                         }).Distinct().ToList());
            foreach (var item in lst)
            {
                if (steps.ContainsKey(item.ProductionOrderId))
                    item.Steps = steps[item.ProductionOrderId];
            }

            return lst;
        }

        public async Task<IList<StepReportModel>> GetProcessingStepReport(long productionOrderId, int[] stepIds)
        {
            var productionOrderDetails = _manufacturingDBContext.ProductionOrderDetail
                .Where(pod => pod.ProductionOrderId == productionOrderId)
                .ToList();

            if (productionOrderDetails.Count == 0) throw new BadRequestException(GeneralCode.InvalidParams, "Chi tiết LSX không tồn tại");


            var allProductionSteps = (from ps in _manufacturingDBContext.ProductionStep
                                      join s in _manufacturingDBContext.Step on ps.StepId equals s.StepId
                                      where ps.ContainerId == productionOrderId && ps.ContainerTypeId == (int)EnumContainerType.ProductionOrder
                                      select new
                                      {
                                          s.StepId,
                                          s.StepName,
                                          ps.ProductionStepId,
                                      })
                                      .ToList();

            // Sắp xếp step theo thứ tự thực hiện
            var allProductionStepIds = allProductionSteps.Select(ps => ps.ProductionStepId).ToList();
            var allDataRole = (from r in _manufacturingDBContext.ProductionStepLinkDataRole
                               join d in _manufacturingDBContext.ProductionStepLinkData on r.ProductionStepLinkDataId equals d.ProductionStepLinkDataId
                               where allProductionStepIds.Contains(r.ProductionStepId)
                               select new
                               {
                                   r.ProductionStepId,
                                   d.ObjectTypeId,
                                   d.ObjectId,
                                   d.QuantityOrigin,
                                   d.OutsourcePartQuantity,
                                   d.ProductionStepLinkDataId,
                                   r.ProductionStepLinkDataRoleTypeId
                               })
                              .ToList();

            var productionStepIds = new List<long>();
            var sortDataRole = allDataRole.ToList();
            while (allProductionStepIds.Count > 0)
            {
                // Lấy ra những công đoạn chỉ nhận từ kho
                var firstStepIds = allProductionStepIds.Where(s =>
                {
                    // Danh sách đầu vào
                    var inputLinkIds = sortDataRole
                    .Where(r => r.ProductionStepId == s && r.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Input)
                    .Select(r => r.ProductionStepLinkDataId)
                    .ToList();
                    // Nếu tất cả đầu vào là từ kho
                    return !sortDataRole.Any(r => inputLinkIds.Contains(r.ProductionStepLinkDataId)
                    && r.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Output
                    && r.ProductionStepId != s);
                }).ToList();

                if (firstStepIds.Count <= 0) throw new BadRequestException(GeneralCode.InternalError, "Quy trình tồn tại vòng lặp");

                productionStepIds.AddRange(firstStepIds);
                allProductionStepIds.RemoveAll(s => firstStepIds.Contains(s));
                sortDataRole.RemoveAll(r => firstStepIds.Contains(r.ProductionStepId));
            }

            var outputData = allDataRole
                .Where(r => r.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Output)
                .Select(r => new
                {
                    r.ProductionStepId,
                    r.ObjectTypeId,
                    r.ObjectId,
                    r.QuantityOrigin,
                    r.OutsourcePartQuantity,
                    r.ProductionStepLinkDataId
                })
                .ToList()
                .GroupBy(d => d.ProductionStepId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var handovers = _manufacturingDBContext.ProductionHandover
                .Where(h => h.ProductionOrderId == productionOrderId)
                .Where(h => h.Status == (int)EnumHandoverStatus.Accepted)
                .ToList();

            var invParammeters = new SqlParameter[]
            {
                new SqlParameter("@ProductionOrderId", productionOrderId)
            };
            var invData = await _manufacturingDBContext.ExecuteDataProcedure("asp_ProductionHandover_GetInventoryRequirementByProductionOrder_new", invParammeters);
            var reqInventorys = invData.ConvertData<ProductionInventoryRequirementEntity>()
                    .Where(r => r.InventoryTypeId == (int)EnumInventoryType.Input && r.Status == (int)EnumProductionInventoryRequirementStatus.Accepted)
                    .ToList();

            var assignments = _manufacturingDBContext.ProductionAssignment
                .Where(a => a.ProductionOrderId == productionOrderId)
                .ToList();

            var result = new List<StepReportModel>();


            foreach (var productionStepId in productionStepIds)
            {
                var productionStep = allProductionSteps.First(s => s.ProductionStepId == productionStepId);
                if (!stepIds.Contains(productionStep.StepId)) continue;

                var stepReport = new StepReportModel
                {
                    StepId = productionStep.StepId,
                    StepName = productionStep.StepName,
                    StepProgressPercent = 0
                };

                var outputStepData = outputData[productionStepId];

                var outputs = outputStepData
                        .GroupBy(d => new { d.ObjectTypeId, d.ObjectId })
                        .Select(g => new
                        {
                            g.Key.ObjectId,
                            g.Key.ObjectTypeId,
                            TotalQuantity = g.Sum(d => d.QuantityOrigin - d.OutsourcePartQuantity.GetValueOrDefault())
                        }).ToList();

                var stepOutputHandovers = handovers
                    .Where(h => h.FromProductionStepId == productionStepId)
                    .ToList();

                var stepOutputInventory = reqInventorys
                    //.Where(i => i.ProductionStepId == productionStepId)
                    .ToList();

                foreach (var output in outputs)
                {
                    var receivedQuantity = stepOutputHandovers.Where(h => h.ObjectId == output.ObjectId && h.ObjectTypeId == (int)output.ObjectTypeId).Sum(h => h.HandoverQuantity);
                    if (output.ObjectTypeId == (int)EnumProductionStepLinkDataObjectType.Product)
                    {
                        receivedQuantity += stepOutputInventory.Where(i => i.ProductId == (int)output.ObjectId).Sum(i => i.ActualQuantity);
                    }
                    var stepProgressPercent = Math.Round(receivedQuantity * 100 / output.TotalQuantity, 2);
                    if (stepProgressPercent > stepReport.StepProgressPercent) stepReport.StepProgressPercent = stepProgressPercent;
                }

                var stepAssignments = assignments.Where(a => a.ProductionStepId == productionStepId).ToList();
                foreach (var stepAssignment in stepAssignments)
                {
                    var departmentProgress = new DepartmentProgress
                    {
                        DepartmentId = stepAssignment.DepartmentId,
                        DepartmentProgressPercent = 0
                    };

                    var assignmentLinkData = outputStepData.FirstOrDefault(d => d.ProductionStepLinkDataId == stepAssignment.ProductionStepLinkDataId);
                    var totalQuantityAssign = assignmentLinkData.QuantityOrigin - assignmentLinkData.OutsourcePartQuantity.GetValueOrDefault();

                    foreach (var output in outputs)
                    {
                        var receivedQuantity = stepOutputHandovers.Where(h => h.FromDepartmentId == stepAssignment.DepartmentId && h.ObjectId == output.ObjectId && h.ObjectTypeId == (int)output.ObjectTypeId).Sum(h => h.HandoverQuantity);
                        if (output.ObjectTypeId == (int)EnumProductionStepLinkDataObjectType.Product)
                        {
                            receivedQuantity += stepOutputInventory.Where(i => i.DepartmentId == stepAssignment.DepartmentId && i.ProductId == (int)output.ObjectId).Sum(i => i.ActualQuantity);
                        }
                        var departmentProgressPercent = Math.Round((receivedQuantity * 100 * totalQuantityAssign) / (stepAssignment.AssignmentQuantity * output.TotalQuantity), 2);
                        if (departmentProgressPercent > departmentProgress.DepartmentProgressPercent) departmentProgress.DepartmentProgressPercent = departmentProgressPercent;
                    }

                    stepReport.DepartmentProgress.Add(departmentProgress);
                }
                result.Add(stepReport);
            }

            return result;
        }

        public async Task<IList<OutsourcePartRequestReportModel>> GetOursourcePartRequestReport(long fromDate, long toDate, long? productionOrderId)
        {
            var fromDateTime = fromDate.UnixToDateTime();
            var toDateTime = toDate.UnixToDateTime();
            var parammeters = new List<SqlParameter>();

            if (!fromDateTime.HasValue || !toDateTime.HasValue)
                throw new BadRequestException(GeneralCode.InvalidParams, "Vui lòng chọn ngày bắt đầu, ngày kết thúc");

            parammeters.Add(new SqlParameter("@FromDate", fromDateTime.Value));
            parammeters.Add(new SqlParameter("@ToDate", toDateTime.Value));

            var sql = new StringBuilder("SELECT * FROM vOutsourcePartRequestExtractInfo v " +
                "WHERE v.OutsourcePartRequestFinishDate <= @ToDate AND v.OutsourcePartRequestFinishDate >= @FromDate");

            if (productionOrderId.GetValueOrDefault() > 0)
            {
                parammeters.Add(new SqlParameter("@ProductionOrderId", productionOrderId));
                sql.Append(" AND v.ProductionOrderId = @ProductionOrderId");
            }

            var queryData = await _manufacturingDBContext.QueryDataTable(sql.ToString(), parammeters.Select(p => p.CloneSqlParam()).ToArray());
            var reportData = queryData
                .ConvertData<OutsourcePartRequestDetailExtractInfo>()
                .AsQueryable()
                .ProjectTo<OutsourcePartRequestReportModel>(_mapper.ConfigurationProvider)
                .ToList();

            var quantityCompleteMaps = (await _manufacturingDBContext.OutsourceTrack.AsNoTracking()
                        .Include(x => x.OutsourceOrder)
                        .Where(x => x.OutsourceOrder.OutsourceTypeId == (int)EnumOutsourceType.OutsourcePart && x.ObjectId.GetValueOrDefault() > 0)
                        .ToListAsync())
                        .GroupBy(x => x.ObjectId)
                        .ToDictionary
                        (
                            k => k.Key,
                            v => v.Sum(x => x.Quantity)
                        );
            foreach (var r in reportData)
            {
                if (quantityCompleteMaps.ContainsKey(r.OutsourcePartRequestDetailId))
                    r.QuantityComplete = quantityCompleteMaps[r.OutsourcePartRequestDetailId].GetValueOrDefault();
            }

            return reportData;
        }

        public async Task<PageData<OutsourceStepRequestReportModel>> GetOursourceStepRequestReport(int page, int size, string orderByFieldName, bool asc, Clause filters)
        {
            var reportData = new List<OutsourceStepRequestReportModel>();

            var query = await _manufacturingDBContext.OutsourceStepRequest
                .Include(x => x.ProductionStep)
                .ThenInclude(x => x.Step)
                .Include(x => x.OutsourceStepRequestData)
                .Include(x => x.ProductionOrder)
                .AsNoTracking()
                .ToListAsync();

            foreach (var rq in query)
            {
                var productionStep = string.Join(", ", (_mapper.Map<IList<ProductionStepModel>>(rq.ProductionStep)).Select(x => x.Title));

                var data = rq.OutsourceStepRequestData.Where(x => x.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Output && x.IsImportant == true)
                    .Select(d => new OutsourceStepRequestReportModel
                    {
                        OutsourceStepRequestId = rq.OutsourceStepRequestId,
                        OutsourcePartRequestFinishDate = rq.OutsourceStepRequestFinishDate.GetUnix(),
                        OutsourceStepRequestCode = rq.OutsourceStepRequestCode,
                        ProductionOrderCode = rq.ProductionOrder.ProductionOrderCode,
                        ProductionOrderId = rq.ProductionOrderId,
                        ProductionStepArrayString = productionStep,
                        ProductionStepLinkDataId = d.ProductionStepLinkDataId,
                        Quantity = d.Quantity
                    });
                reportData.AddRange(data);
            }

            if (reportData.Count > 0)
            {
                var lsProductionStepLinkDataId = reportData.Select(x => x.ProductionStepLinkDataId).ToArray();
                var sql = new StringBuilder("SELECT * FROM ProductionStepLinkDataExtractInfo v ");
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

                var lsLinkDataInfos = (await _manufacturingDBContext.QueryDataTable(sql.ToString(), parammeters.Select(p => p.CloneSqlParam()).ToArray()))
                        .ConvertData<ProductionStepLinkDataInput>().ToDictionary(k => k.ProductionStepLinkDataId, v => v);

                var quantityCompleteMaps = (await _manufacturingDBContext.OutsourceTrack.AsNoTracking()
                        .Include(x => x.OutsourceOrder)
                        .Where(x => x.OutsourceOrder.OutsourceTypeId == (int)EnumOutsourceType.OutsourceStep && x.ObjectId.GetValueOrDefault() > 0)
                        .ToListAsync())
                        .GroupBy(x => x.ObjectId)
                        .ToDictionary
                        (
                            k => k.Key,
                            v => v.Sum(x => x.Quantity)
                        );

                foreach (var r in reportData)
                {
                    if (!lsLinkDataInfos.ContainsKey(r.ProductionStepLinkDataId))
                        continue;

                    var info = lsLinkDataInfos[r.ProductionStepLinkDataId];
                    r.ProductionStepLinkDataTitle = info.ObjectTitle;
                    r.UnitId = info.UnitId;

                    if (quantityCompleteMaps.ContainsKey(r.ProductionStepLinkDataId))
                        r.QuantityComplete = quantityCompleteMaps[r.ProductionStepLinkDataId].GetValueOrDefault();
                }
            }

            var queryFilter = reportData.AsQueryable();
            if (filters != null)
                queryFilter = queryFilter.AsQueryable().InternalFilter(filters);

            if (!string.IsNullOrWhiteSpace(orderByFieldName))
                queryFilter = queryFilter.InternalOrderBy(orderByFieldName, asc);

            var lst = (size > 0 ? queryFilter.Skip((page - 1) * size).Take(size) : queryFilter).ToList();

            var total = queryFilter.Count();

            return (lst, total);
        }
    }
}
