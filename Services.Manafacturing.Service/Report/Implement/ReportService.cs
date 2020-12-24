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

namespace VErp.Services.Manafacturing.Service.Report.Implement
{
    public class ReportService : IReportService
    {
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public ReportService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<ReportService> logger
            , IMapper mapper)
        {
            _manufacturingDBContext = manufacturingDB;
            _activityLogService = activityLogService;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<IList<StepModel>> GetSteps(long fromDate, long toDate)
        {
            var fromDateTime = fromDate.UnixToDateTime();
            var toDateTime = toDate.UnixToDateTime();
            var steps = await (from s in _manufacturingDBContext.Step
                               join ps in _manufacturingDBContext.ProductionStep on s.StepId equals ps.StepId
                               join po in _manufacturingDBContext.ProductionOrder on new { ps.ContainerTypeId, ps.ContainerId } equals new { ContainerTypeId = (int)EnumContainerType.ProductionOrder, ContainerId = po.ProductionOrderId }
                               join pod in _manufacturingDBContext.ProductionOrderDetail on po.ProductionOrderId equals pod.ProductionOrderId
                               join sh in _manufacturingDBContext.ProductionSchedule on pod.ProductionOrderDetailId equals sh.ProductionOrderDetailId
                               where sh.StartDate <= toDateTime && sh.EndDate >= fromDateTime
                               select s).Distinct().ProjectTo<StepModel>(_mapper.ConfigurationProvider).ToListAsync();

            return steps;
        }

        public async Task<IList<StepProgressModel>> GetProductionProgressReport(long fromDate, long toDate, int[] stepIds)
        {
            var fromDateTime = fromDate.UnixToDateTime();
            var toDateTime = toDate.UnixToDateTime();

            var productionSteps = (from ps in _manufacturingDBContext.ProductionStep
                                   join po in _manufacturingDBContext.ProductionOrder on new { ps.ContainerTypeId, ps.ContainerId } equals new { ContainerTypeId = (int)EnumContainerType.ProductionOrder, ContainerId = po.ProductionOrderId }
                                   join pod in _manufacturingDBContext.ProductionOrderDetail on po.ProductionOrderId equals pod.ProductionOrderId
                                   join sh in _manufacturingDBContext.ProductionSchedule on pod.ProductionOrderDetailId equals sh.ProductionOrderDetailId
                                   where stepIds.Contains(ps.StepId.Value) && sh.StartDate <= toDateTime && sh.EndDate >= fromDateTime
                                   select new
                                   {
                                       StepId = ps.StepId.Value,
                                       ps.ProductionStepId,
                                       po.ProductionOrderCode,
                                       pod.ProductId,
                                       OrderQuantity = pod.Quantity.GetValueOrDefault() + pod.ReserveQuantity.GetValueOrDefault(),
                                       sh.ProductionScheduleQuantity,
                                       sh.ProductionOrderDetailId,
                                       sh.ScheduleTurnId,
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
                            d.Quantity
                        })
                        .ToList()
                        .GroupBy(d => d.ProductionStepId)
                        .ToDictionary(g => g.Key, g => g.ToList()); ;

            var scheduleTurnIds = productionSteps.Select(ps => ps.ScheduleTurnId).Distinct().ToList();
            var handovers = _manufacturingDBContext.ProductionHandover
            .Where(h => scheduleTurnIds.Contains(h.ScheduleTurnId))
            .Where(h => h.Status == (int)EnumHandoverStatus.Accepted)
            .ToList();

            var reqInventorys = new Dictionary<long, List<ProductionInventoryRequirementEntity>>();

            foreach (var scheduleTurnId in scheduleTurnIds)
            {
                var parammeters = new SqlParameter[]
                {
                    new SqlParameter("@ScheduleTurnId", scheduleTurnId)
                };

                var resultData = await _manufacturingDBContext.ExecuteDataProcedure("asp_ProductionHandover_GetInventoryRequirementByScheduleTurn", parammeters);

                reqInventorys.Add(scheduleTurnId, resultData.ConvertData<ProductionInventoryRequirementEntity>()
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
                    var stepData = data[productionStepId];

                    foreach (var stepScheduleProgress in productionStepProgressModel.GroupBy(s => s.ScheduleTurnId))
                    {
                        var scheduleTurnId = stepScheduleProgress.Key;
                        var productionOrderDetailId = stepScheduleProgress.First().ProductionOrderDetailId;
                        var productionScheduleQuantity = stepScheduleProgress.First().ProductionScheduleQuantity;
                        var orderQuantity = stepScheduleProgress.First().OrderQuantity;
                        var productId = stepScheduleProgress.First().ProductId;

                        var previousSchedule = productionStepProgressModel
                            .Where(s => s.ProductionOrderDetailId == productionOrderDetailId
                            && s.ScheduleTurnId < scheduleTurnId)
                            .GroupBy(s => s.ScheduleTurnId)
                            .Select(g => g.First().ProductionScheduleQuantity)
                            .ToList();
                        var isFinish = previousSchedule.Sum(p => p) + productionScheduleQuantity < orderQuantity;

                        var stepScheduleProgressModel = new StepScheduleProgressModel
                        {
                            ProductionOrderCode = stepScheduleProgress.First().ProductionOrderCode,
                            ProductIds = stepScheduleProgress.Select(s => s.ProductId).ToArray(),
                        };

                        stepScheduleProgressModel.InputData = stepData
                            .Where(d => d.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Input)
                            .GroupBy(d => new { d.ObjectTypeId, d.ObjectId })
                            .Select(g => new StepScheduleProgressDataModel
                            {
                                ObjectId = g.Key.ObjectId,
                                ObjectTypeId = (EnumProductionStepLinkDataObjectType)g.Key.ObjectTypeId,
                                TotalQuantity = isFinish
                                ? Math.Round(g.Sum(d => d.Quantity) * productionScheduleQuantity / orderQuantity, 5)
                                : orderQuantity - previousSchedule.Sum(p => Math.Round(g.Sum(d => d.Quantity) * p / orderQuantity, 5))
                            }).ToList();

                        stepScheduleProgressModel.OutputData = stepData
                            .Where(d => d.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Output)
                            .GroupBy(d => new { d.ObjectTypeId, d.ObjectId })
                            .Select(g => new StepScheduleProgressDataModel
                            {
                                ObjectId = g.Key.ObjectId,
                                ObjectTypeId = (EnumProductionStepLinkDataObjectType)g.Key.ObjectTypeId,
                                TotalQuantity = isFinish
                                ? Math.Round(g.Sum(d => d.Quantity) * productionScheduleQuantity / orderQuantity, 5)
                                : orderQuantity - previousSchedule.Sum(p => Math.Round(g.Sum(d => d.Quantity) * p / orderQuantity, 5))
                            }).ToList();

                        var stepInputHandovers = handovers
                            .Where(h => h.ScheduleTurnId == scheduleTurnId && h.ToProductionStepId == productionStepId)
                            .ToList();

                        var stepOutputHandovers = handovers
                            .Where(h => h.ScheduleTurnId == scheduleTurnId && h.FromProductionStepId == productionStepId)
                            .ToList();

                        var stepInputInventory = reqInventorys[scheduleTurnId].Where(i => i.InventoryTypeId == (int)EnumInventoryType.Output && i.ProductionStepId == productionStepId).ToList();
                        var stepOutputInventory = reqInventorys[scheduleTurnId].Where(i => i.InventoryTypeId == (int)EnumInventoryType.Input && i.ProductionStepId == productionStepId).ToList();



                        foreach (var input in stepScheduleProgressModel.InputData)
                        {
                            var receivedQuantity = stepInputHandovers.Where(h => h.ObjectId == input.ObjectId && h.ObjectTypeId == (int)input.ObjectTypeId).Sum(h => h.HandoverQuantity);
                            if (input.ObjectTypeId == EnumProductionStepLinkDataObjectType.Product)
                            {
                                receivedQuantity += stepInputInventory.Where(i => i.ProductId == (int)input.ObjectId).Sum(i => i.ActualQuantity).GetValueOrDefault();
                            }
                            input.ReceivedQuantity = receivedQuantity;
                        }

                        foreach (var output in stepScheduleProgressModel.OutputData)
                        {
                            var receivedQuantity = stepOutputHandovers.Where(h => h.ObjectId == output.ObjectId && h.ObjectTypeId == (int)output.ObjectTypeId).Sum(h => h.HandoverQuantity);
                            if (output.ObjectTypeId == EnumProductionStepLinkDataObjectType.Product)
                            {
                                receivedQuantity += stepOutputInventory.Where(i => i.ProductId == (int)output.ObjectId).Sum(i => i.ActualQuantity).GetValueOrDefault();
                            }
                            output.ReceivedQuantity = receivedQuantity;

                            if (output.ReceivedQuantity * 100 / output.TotalQuantity > stepScheduleProgressModel.ProgressPercent)
                                stepScheduleProgressModel.ProgressPercent = Math.Round(output.ReceivedQuantity * 100 / output.TotalQuantity, 2);
                        }

                        stepProgressModel.StepScheduleProgress.Add(stepScheduleProgressModel);
                    }
                }

                report.Add(stepProgressModel);
            }

            return report;
        }
        public async Task<IList<ProductionScheduleReportModel>> GetProductionScheduleReport(long fromDate, long toDate)
        {
            var parammeters = new List<SqlParameter>();

            var fromDateTime = fromDate.UnixToDateTime();
            var toDateTime = toDate.UnixToDateTime();
            if (!fromDateTime.HasValue || !toDateTime.HasValue)
                throw new BadRequestException(GeneralCode.InvalidParams, "Vui lòng chọn ngày bắt đầu, ngày kết thúc");

            parammeters.Add(new SqlParameter("@FromDate", fromDateTime.Value));
            parammeters.Add(new SqlParameter("@ToDate", toDateTime.Value));

            var sql = new StringBuilder("SELECT * FROM vProductionSchedule v WHERE v.StartDate <= @ToDate AND v.EndDate >= @FromDate");

            var resultData = await _manufacturingDBContext.QueryDataTable(sql.ToString(), parammeters.Select(p => p.CloneSqlParam()).ToArray());
            var lst = resultData
                .ConvertData<ProductionScheduleEntity>()
                .AsQueryable()
                .ProjectTo<ProductionScheduleReportModel>(_mapper.ConfigurationProvider)
                .ToList();

            var scheduleTurnIds = lst.Select(s => s.ScheduleTurnId.Value).Distinct().ToList();

            var handovers = _manufacturingDBContext.ProductionHandover
                .Where(h => scheduleTurnIds.Contains(h.ScheduleTurnId))
                .Where(h => h.Status == (int)EnumHandoverStatus.Accepted)
                .ToList();

            var reqInventorys = new Dictionary<long, List<ProductionInventoryRequirementEntity>>();

            foreach (var scheduleTurnId in scheduleTurnIds)
            {
                var invParammeters = new SqlParameter[]
                {
                    new SqlParameter("@ScheduleTurnId", scheduleTurnId)
                };

                var invData = await _manufacturingDBContext.ExecuteDataProcedure("asp_ProductionHandover_GetInventoryRequirementByScheduleTurn", invParammeters);

                reqInventorys.Add(scheduleTurnId, invData.ConvertData<ProductionInventoryRequirementEntity>()
                    .Where(r => r.InventoryTypeId == (int)EnumInventoryType.Input && r.Status == (int)EnumProductionInventoryRequirementStatus.Accepted)
                    .ToList());
            }

            var productionSteps = (from ps in _manufacturingDBContext.ProductionStep
                                   join s in _manufacturingDBContext.Step on ps.StepId equals s.StepId
                                   join po in _manufacturingDBContext.ProductionOrder on new { ps.ContainerTypeId, ps.ContainerId } equals new { ContainerTypeId = (int)EnumContainerType.ProductionOrder, ContainerId = po.ProductionOrderId }
                                   join pod in _manufacturingDBContext.ProductionOrderDetail on po.ProductionOrderId equals pod.ProductionOrderId
                                   join sh in _manufacturingDBContext.ProductionSchedule on pod.ProductionOrderDetailId equals sh.ProductionOrderDetailId
                                   where scheduleTurnIds.Contains(sh.ScheduleTurnId)
                                   select new
                                   {
                                       s.StepName,
                                       ps.ProductionStepId,
                                       sh.ScheduleTurnId,
                                   })
                                   .ToList();

            var productionStepIds = productionSteps.Select(ps => ps.ProductionStepId).Distinct().ToList();
            var scheduleProductionSteps = productionSteps.GroupBy(d => d.ScheduleTurnId).ToDictionary(g => g.Key, g => g.ToList());

            var outputData = (from r in _manufacturingDBContext.ProductionStepLinkDataRole
                              join d in _manufacturingDBContext.ProductionStepLinkData on r.ProductionStepLinkDataId equals d.ProductionStepLinkDataId
                              where productionStepIds.Contains(r.ProductionStepId) && r.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Output
                              select new
                              {
                                  r.ProductionStepId,
                                  d.ObjectTypeId,
                                  d.ObjectId,
                                  d.Quantity
                              })
                              .ToList()
                              .GroupBy(d => d.ProductionStepId)
                              .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var schedule in lst)
            {
                var scheduleTurnId = schedule.ScheduleTurnId.Value;
                schedule.CompletedQuantity = reqInventorys[scheduleTurnId]
                    .Where(i => i.ProductId == schedule.ProductId)
                    .Sum(i => i.ActualQuantity)
                    .GetValueOrDefault();

                var stepTitle = new StringBuilder();

                var previousSchedule = lst
                           .Where(s => s.ProductionOrderDetailId == schedule.ProductionOrderDetailId && s.ScheduleTurnId < scheduleTurnId)
                           .GroupBy(s => s.ScheduleTurnId)
                           .Select(g => g.First().ProductionScheduleQuantity)
                           .ToList();
                var isFinish = previousSchedule.Sum(p => p) + schedule.ProductionScheduleQuantity < schedule.TotalQuantity;

                foreach (var productionStep in scheduleProductionSteps[scheduleTurnId])
                {
                    var outputStepData = outputData[productionStep.ProductionStepId];

                    var output = outputStepData
                            .GroupBy(d => new { d.ObjectTypeId, d.ObjectId })
                            .Select(g => new
                            {
                                g.Key.ObjectId,
                                g.Key.ObjectTypeId,
                                TotalQuantity = isFinish
                                ? Math.Round(g.Sum(d => d.Quantity) * schedule.ProductionScheduleQuantity / schedule.TotalQuantity, 5)
                                : schedule.TotalQuantity - previousSchedule.Sum(p => Math.Round(g.Sum(d => d.Quantity) * p / schedule.TotalQuantity, 5))
                            }).ToList();

                    var stepOutputHandovers = handovers
                        .Where(h => h.ScheduleTurnId == scheduleTurnId && h.FromProductionStepId == productionStep.ProductionStepId)
                        .ToList();

                    var stepOutputInventory = reqInventorys[scheduleTurnId]
                        .Where(i => i.ProductionStepId == productionStep.ProductionStepId)
                        .ToList();

                    if (output.Any(o =>
                    {
                        var receivedQuantity = stepOutputHandovers.Where(h => h.ObjectId == o.ObjectId && h.ObjectTypeId == (int)o.ObjectTypeId).Sum(h => h.HandoverQuantity);
                        if (o.ObjectTypeId == (int)EnumProductionStepLinkDataObjectType.Product)
                        {
                            receivedQuantity += stepOutputInventory.Where(i => i.ProductId == (int)o.ObjectId).Sum(i => i.ActualQuantity).GetValueOrDefault();
                        }
                        return receivedQuantity < o.TotalQuantity;
                    }))
                    {
                        stepTitle.Append(productionStep.StepName);
                        stepTitle.Append("-");
                    }
                }
                schedule.UnfinishedStepTitle = stepTitle.ToString().Trim('-');
            }
            return lst;
        }


        public async Task<IList<ProcessingScheduleListModel>> GetProcessingScheduleList()
        {
            return null;
        }

    }
}
