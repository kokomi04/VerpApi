using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenXmlPowerTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface.Manufacturing;
using VErp.Commons.GlobalObject.QueueMessage;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Manafacturing.Model.ProductionOrder;
using VErp.Services.Manafacturing.Service.ProductionAssignment;
using VErp.Services.Manafacturing.Service.ProductionHandover;
using VErp.Services.Manafacturing.Service.ProductionOrder;
using VErp.Services.Manafacturing.Service.StatusProcess.Implement;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;
using ProductionHandoverEntity = VErp.Infrastructure.EF.ManufacturingDB.ProductionHandover;
using ProductionAssignmentEntity = VErp.Infrastructure.EF.ManufacturingDB.ProductionAssignment;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using VErp.Services.Manafacturing.Model.ProductionHandover;
using VErp.Commons.Constants;
using Verp.Cache.Caching;
using VErp.Commons.Constants.Caching;
using Verp.Resources.Manafacturing.Production.Progress;
using VErp.Infrastructure.ServiceCore.Facade;

namespace VErp.Services.Manafacturing.Service.ProductionProcess.Implement
{
    public class ProductionProgressService : StatusProcessService, IProductionProgressService
    {

        //private readonly IProductionHandoverReceiptService _productionHandoverReceiptService;
        //private readonly IMaterialAllocationService _materialAllocationService;
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly ObjectActivityLogFacade _objActivityLogFacade;
        private readonly IActivityLogService _activityLogService;
        //private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly ICachingService _cachingService;

        public ProductionProgressService(ManufacturingDBContext manufacturingDBContext,
            IActivityLogService activityLogService,
             IMapper mapper,
            ILogger<ProductionProgressService> logger,
            ICachingService cachingService) : base(manufacturingDBContext, activityLogService, logger, mapper)
        {
            _manufacturingDBContext = manufacturingDBContext;
            _objActivityLogFacade = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.ProductionOrder);
            _mapper = mapper;
            _cachingService = cachingService;
            //  _logger = logger;
        }

        /*
        public async Task<bool> CalcAndUpdateProductionOrderStatus(ProductionOrderCalcStatusMessage data)
        {
            await _materialAllocationService.UpdateIgnoreAllocation(new[] { data.ProductionOrderCode }, true);

            await _productionHandoverReceiptService.ChangeAssignedProgressStatus(data.ProductionOrderCode, data.Description, data.Inventories);


            var productionOrder = _manufacturingDBContext.ProductionOrder
                .Include(po => po.ProductionOrderDetail)
                .FirstOrDefault(po => po.ProductionOrderCode == data.ProductionOrderCode);

            if (productionOrder == null)
                throw new BadRequestException(GeneralCode.ItemNotFound, "Lệnh sản xuất không tồn tại");

            var departmentHandoverDetails = await GetDepartmentHandoverDetail(productionOrder.ProductionOrderId, null, null, data.Inventories);

            var oldStatus = productionOrder.ProductionOrderStatus;

            if (productionOrder.ProductionOrderStatus == (int)EnumProductionStatus.Finished && productionOrder.IsManualFinish) return true;
            try
            {
                var steps = await _manufacturingDBContext.ProductionStep.Where(s => !s.IsFinish && s.ContainerId == productionOrder.ProductionOrderId && s.ContainerTypeId == (int)EnumContainerType.ProductionOrder).ToListAsync();
                /*
                var inputs = await (from d in _manufacturingDBContext.ProductionStepLinkData
                                    join r in _manufacturingDBContext.ProductionStepLinkDataRole on d.ProductionStepLinkDataId equals r.ProductionStepLinkDataId
                                    join s in _manufacturingDBContext.ProductionStep on r.ProductionStepId equals s.ProductionStepId
                                    where s.ContainerId == productionOrder.ProductionOrderId && s.ContainerTypeId == (int)EnumContainerType.ProductionOrder
                                    && r.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Input
                                    && !s.IsFinish
                                    select new
                                    {
                                        r.ProductionStepId,
                                        d
                                    }).ToListAsync();

                var outputs = await (from d in _manufacturingDBContext.ProductionStepLinkData
                                     join r in _manufacturingDBContext.ProductionStepLinkDataRole on d.ProductionStepLinkDataId equals r.ProductionStepLinkDataId
                                     join s in _manufacturingDBContext.ProductionStep on r.ProductionStepId equals s.ProductionStepId
                                     where s.ContainerId == productionOrder.ProductionOrderId && s.ContainerTypeId == (int)EnumContainerType.ProductionOrder
                                     && r.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Output
                                     && !s.IsFinish
                                     select new
                                     {
                                         r.ProductionStepId,
                                         d
                                     }).ToListAsync();
                */

        /**
         * All output not have input
         *
        //var endSteps = steps.Where(s => s.ParentId != null &&
        //        outputs.Where(p => p.ProductionStepId == s.ProductionStepId)
        //                .All(d => !inputs.Any(o => o.d.ProductionStepLinkDataId == d.d.ProductionStepLinkDataId))
        // ).ToList();

        var assignments = await _manufacturingDBContext.ProductionAssignment.Where(s => s.ProductionOrderId == productionOrder.ProductionOrderId).ToListAsync();

        //var allocation = await _manufacturingDBContext.MaterialAllocation.Where(a => a.ProductionOrderId == productionOrder.ProductionOrderId).ToListAsync();

        var hasAllocation = await _manufacturingDBContext.MaterialAllocation.AnyAsync(a => a.ProductionOrderId == productionOrder.ProductionOrderId);

        var hasHandOver = await _manufacturingDBContext.ProductionHandover.AnyAsync(s => s.ProductionOrderId == productionOrder.ProductionOrderId);

        if (!data.Inventories.Any(iv => iv.ActualQuantity > 0) && !hasHandOver && !hasAllocation)
        {
            if (!steps.Any() || !assignments.Any())
                productionOrder.ProductionOrderStatus = (int)EnumProductionStatus.NotReady;
            else
                productionOrder.ProductionOrderStatus = (int)EnumProductionStatus.Waiting;
        }
        else
        {
            if (departmentHandoverDetails.Any() && departmentHandoverDetails.All(s => s.InputDatas.Where(d => d.FromStepId == null).All(d => d.ReceivedQuantity >= d.RequireQuantity)))//startSteps.All(s => assignments.Any(a => a.ProductionStepId == s.ProductionStepId) && assignments.All(a => a.ProductionStepId == s.StepId && (a.IsManualFinish || a.AssignedProgressStatus == (int)EnumAssignedProgressStatus.Finish))))
                productionOrder.ProductionOrderStatus = (int)EnumProductionStatus.ProcessingFullStarted;
            else
                productionOrder.ProductionOrderStatus = (int)EnumProductionStatus.ProcessingLessStarted;
        }





        var prodDetails = await _manufacturingDBContext.ProductionOrderDetail.Where(d => d.ProductionOrderId == productionOrder.ProductionOrderId).ToListAsync();


        var inputInventories = data.Inventories.Where(d => d.InventoryTypeId == EnumInventoryType.Input);

        bool isFinish = true;

        foreach (var productionOrderDetail in prodDetails)
        {
            var quantity = inputInventories
                .Where(i => i.ProductId == productionOrderDetail.ProductId && i.Status != (int)EnumProductionInventoryRequirementStatus.Rejected)
                .Sum(i => i.ActualQuantity);

            if (quantity < (productionOrderDetail.Quantity + productionOrderDetail.ReserveQuantity))
            {
                isFinish = false;
                break;
            }
        }

        /*

        if (isFinish || endSteps.All(s => assignments.Any(a => a.ProductionStepId == s.ProductionStepId) && assignments.Where(a => a.ProductionStepId == s.ProductionStepId).All(a => a.IsManualFinish || a.AssignedProgressStatus == (int)EnumAssignedProgressStatus.Finish)))
        {
            productionOrder.ProductionOrderStatus = (int)EnumProductionStatus.Completed;

            if (assignments.Any(a => !(a.IsManualFinish || a.AssignedProgressStatus == (int)EnumAssignedProgressStatus.Finish)))
            {
                productionOrder.ProductionOrderStatus = (int)EnumProductionStatus.MissHandOverInfo;
            }

        }
        else
        {

            if (productionOrder.EndDate < DateTime.UtcNow)
            {
                productionOrder.ProductionOrderStatus = (int)EnumProductionStatus.OverDeadline;
            }
            
        }*

        var stepsToProceduction = steps.Where(s => s.ParentId != null).ToList();
        var isCompletedHandoverAssignment = stepsToProceduction.All(s => assignments.Any(a => a.ProductionStepId == s.ProductionStepId) && assignments.Where(a => a.ProductionStepId == s.ProductionStepId).All(a => a.IsManualFinish || a.AssignedProgressStatus == (int)EnumAssignedProgressStatus.Finish));

        if (isFinish)
        {
            if (!isCompletedHandoverAssignment)
            {
                productionOrder.ProductionOrderStatus = (int)EnumProductionStatus.MissHandOverInfo;
            }
            else
            {
                productionOrder.ProductionOrderStatus = (int)EnumProductionStatus.Completed;
            }

        }
        else
        {

            if (productionOrder.EndDate < DateTime.UtcNow)
            {
                productionOrder.ProductionOrderStatus = (int)EnumProductionStatus.OverDeadline;
            }

        }



        if (oldStatus != productionOrder.ProductionOrderStatus)
        {
            _manufacturingDBContext.SaveChanges();
            await _activityLogService.CreateLog(EnumObjectType.ProductionOrder, productionOrder.ProductionOrderId, $"Cập nhật trạng thái lệnh sản xuất, {data.Description}", new { productionOrder, data, isManual = false });
        }


        return true;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "UpdateProductOrderStatus");
        throw;
    }

}
*/
        public async Task<bool> CalcAndUpdateProductionOrderStatusV2(ProductionOrderCalcStatusV2Message data)
        {
            var productionOrder = _manufacturingDBContext.ProductionOrder
                .Include(po => po.ProductionOrderDetail)
                .FirstOrDefault(po => po.ProductionOrderCode == data.ProductionOrderCode);

            if (productionOrder == null)
                throw new BadRequestException(GeneralCode.ItemNotFound, "Lệnh sản xuất không tồn tại");

            await AutoAllowcation(productionOrder.ProductionOrderId, data);

            await UpdateAssignProgress(productionOrder.ProductionOrderId);

            var oldStatus = productionOrder.ProductionOrderStatus;

            if (productionOrder.ProductionOrderStatus == (int)EnumProductionStatus.Finished && productionOrder.IsManualFinish) return true;

            productionOrder.ProductionOrderStatus = (int)await CalcProductionOrderStatus(productionOrder.ProductionOrderId, productionOrder.EndDate, data.InvDetails);


            if (oldStatus != productionOrder.ProductionOrderStatus)
            {
                _manufacturingDBContext.SaveChanges();
                //await _activityLogService.CreateLog(EnumObjectType.ProductionOrder, productionOrder.ProductionOrderId, $"Cập nhật trạng thái lệnh sản xuất, {data.Description}", new { productionOrder, data, isManual = false });

                await _objActivityLogFacade.LogBuilder(() => ProductionProgressActivityLogMessage.UpdateProductionOrderStatus)
                            .MessageResourceFormatDatas(data.Description)
                            .ObjectId(productionOrder.ProductionOrderId)
                            .JsonData(new { productionOrder, data, isManual = false })
                            .CreateLog();
            }

            _cachingService.TryRemove(ProductionOrderCacheKeys.CalcProductionOrderStatusPending(productionOrder.ProductionOrderCode));

            return true;
        }

        public bool IsPendingCalcStatus(string producionOrderCode)
        {
            return _cachingService.TryGet<bool>(ProductionOrderCacheKeys.CalcProductionOrderStatusPending(producionOrderCode));
        }

        public async Task<IList<ProductionOrderInventoryConflictModel>> GetConflictInventories(long productionOrderId)
        {
            var lst = await _manufacturingDBContext.ProductionOrderInventoryConflict.Where(p => p.ProductionOrderId == productionOrderId).ToListAsync();
            return _mapper.Map<List<ProductionOrderInventoryConflictModel>>(lst);
        }

        private async Task<EnumProductionStatus> CalcProductionOrderStatus(long productionOrderId, DateTime endDate, IList<InventoryDetailByProductionOrderModel> invDetails)
        {
            var steps = await _manufacturingDBContext.ProductionStep.Where(s => !s.IsFinish && s.ContainerId == productionOrderId && s.ContainerTypeId == (int)EnumContainerType.ProductionOrder).ToListAsync();

            var assignments = await _manufacturingDBContext.ProductionAssignment.Where(s => s.ProductionOrderId == productionOrderId).ToListAsync();

            var hasHandOver = await _manufacturingDBContext.ProductionHandover.AnyAsync(s => s.ProductionOrderId == productionOrderId);

            EnumProductionStatus status;

            if (!hasHandOver)
            {
                if (!steps.Any() || !assignments.Any())
                    status = EnumProductionStatus.NotReady;
                else
                    status = EnumProductionStatus.Waiting;
            }
            else
            {
                if (assignments.All(a => a.AssignedInputStatus == (int)EnumAssignedProgressStatus.Finish))
                    status = EnumProductionStatus.ProcessingFullStarted;
                else
                    status = EnumProductionStatus.ProcessingLessStarted;
            }

            var prodDetails = await _manufacturingDBContext.ProductionOrderDetail.Where(d => d.ProductionOrderId == productionOrderId).ToListAsync();


            bool isFinish = true;

            foreach (var productionOrderDetail in prodDetails)
            {
                var quantity = invDetails
                    .Where(i => i.ProductId == productionOrderDetail.ProductId && i.InventoryTypeId == EnumInventoryType.Input)
                    .Sum(i => i.PrimaryQuantity);

                if (quantity < (productionOrderDetail.Quantity + productionOrderDetail.ReserveQuantity))
                {
                    isFinish = false;
                    break;
                }
            }


            var stepsToProceduction = steps.Where(s => s.ParentId != null).ToList();
            var isCompletedHandoverAssignment = stepsToProceduction.All(s => assignments.Any(a => a.ProductionStepId == s.ProductionStepId) && assignments.Where(a => a.ProductionStepId == s.ProductionStepId).All(a => a.IsManualFinish || a.AssignedProgressStatus == (int)EnumAssignedProgressStatus.Finish));

            if (isFinish)
            {
                if (!isCompletedHandoverAssignment)
                {
                    status = EnumProductionStatus.MissHandOverInfo;
                }
                else
                {
                    status = EnumProductionStatus.Completed;
                }

            }
            else
            {

                if (endDate < DateTime.UtcNow)
                {
                    status = EnumProductionStatus.OverDeadline;
                }

            }

            return status;
        }

        private async Task UpdateAssignProgress(long productionOrderId)
        {
            var productionSteps = await _manufacturingDBContext.ProductionStep.Where(p => p.ContainerId == productionOrderId && p.ContainerTypeId == (int)EnumContainerType.ProductionOrder).ToListAsync();

            var assignments = await _manufacturingDBContext.ProductionAssignment.Where(p => p.ProductionOrderId == productionOrderId).ToListAsync();

            var assignRequirements = await GetAssignRequirements(productionOrderId);

            var handovers = await (from h in _manufacturingDBContext.ProductionHandover
                                   join r in _manufacturingDBContext.ProductionHandoverReceipt on h.ProductionHandoverReceiptId equals r.ProductionHandoverReceiptId into rs
                                   from r in rs.DefaultIfEmpty()
                                   where (h.ProductionOrderId == productionOrderId &&
                                   (h.Status == (int)EnumHandoverStatus.Accepted || r != null && r.HandoverStatusId == (int)EnumHandoverStatus.Accepted)
                                   )
                                   select h
                                  ).ToListAsync();


            foreach (var assign in assignments)
            {
                var requireOuts = assignRequirements
                    .Where(r => r.ProductionStepId == assign.ProductionStepId
                        && r.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Output
                    ).ToList();
              
                assign.AssignedProgressStatus = (int)CaclAssignOutputStatus(assign, requireOuts, handovers);
            }

            foreach (var assign in assignments)
            {
                var requireIns = assignRequirements
                    .Where(r => r.ProductionStepId == assign.ProductionStepId
                        && r.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Input
                    ).ToList();

                assign.AssignedInputStatus = (int)CalcAssignInputStatus(productionSteps, assignments, assign, requireIns, handovers);
            }
            await _manufacturingDBContext.SaveChangesAsync();
        }

        private EnumAssignedProgressStatus CaclAssignOutputStatus(ProductionAssignmentEntity currentAssignment, List<StepDepartmentAssignment> requireOuts, IList<ProductionHandoverEntity> handovers)
        {
            if (currentAssignment.IsManualFinish && currentAssignment.AssignedProgressStatus == (int)EnumAssignedProgressStatus.Finish) return EnumAssignedProgressStatus.Finish;

            var status = EnumAssignedProgressStatus.Waiting;
            var hasAnyOutputHandover = handovers.Any(h => h.FromProductionStepId == currentAssignment.ProductionStepId && h.FromDepartmentId == currentAssignment.DepartmentId);
            if (hasAnyOutputHandover)
            {
                status = EnumAssignedProgressStatus.HandingOver;
            }

            var isAllRequireCompleted = true;
            foreach (var requireOut in requireOuts)
            {
                var totalHandover = handovers.Where(h => h.FromDepartmentId == requireOut.DepartmentId
                                        && h.FromProductionStepId == requireOut.ProductionStepId
                                        //&& h.ToProductionStepId == requireOut.RefProductionStepId
                                        && h.ObjectTypeId == (int)requireOut.LinkDataObjectTypeId
                                        && h.ObjectId == (int)requireOut.LinkDataObjectId
                                        )
                                        .Sum(h => h.HandoverQuantity);
                if (totalHandover < requireOut.RequiredQuantity)
                {
                    isAllRequireCompleted = false;
                    break;
                }
            }

            if (isAllRequireCompleted)
            {
                status = EnumAssignedProgressStatus.Finish;
            }

            return status;
        }


        private EnumAssignedProgressStatus CalcAssignInputStatus(IList<ProductionStep> productionSteps, IList<ProductionAssignmentEntity> assignments, ProductionAssignmentEntity currentAssignment, List<StepDepartmentAssignment> requireIns, IList<ProductionHandoverEntity> handovers)
        {
            if (currentAssignment.IsManualFinish && currentAssignment.AssignedProgressStatus == (int)EnumAssignedProgressStatus.Finish) return EnumAssignedProgressStatus.Finish;

            var status = EnumAssignedProgressStatus.Waiting;
            var hasAnyInputHandover = handovers.Any(h => h.ToProductionStepId == currentAssignment.ProductionStepId && h.ToDepartmentId == currentAssignment.DepartmentId);
            if (hasAnyInputHandover)
            {
                status = EnumAssignedProgressStatus.HandingOver;
            }

            var isAllRequireCompleted = true;
            var fromStepIds = new HashSet<long>();
            foreach (var requireIn in requireIns)
            {
                var totalHandover = handovers.Where(h => h.ToDepartmentId == requireIn.DepartmentId
                                        && h.ToProductionStepId == requireIn.ProductionStepId
                                        //&& h.FromProductionStepId == requireIn.RefProductionStepId
                                        && h.ObjectTypeId == (int)requireIn.LinkDataObjectTypeId
                                        && h.ObjectId == (int)requireIn.LinkDataObjectId
                                        )
                                        .Sum(h => h.HandoverQuantity);

                if (!fromStepIds.Contains(requireIn.RefProductionStepId))
                {
                    fromStepIds.Add(requireIn.RefProductionStepId);
                }

                if (totalHandover < requireIn.RequiredQuantity)
                {
                    isAllRequireCompleted = false;
                    break;
                }
            }

            if (isAllRequireCompleted)
            {
                return EnumAssignedProgressStatus.Finish;
            }

            var beforeStepsAreNotFromStock = fromStepIds.All(sId => sId > 0);
            var beforeStepsAreCompletedAssignment = fromStepIds.All(sId => productionSteps.FirstOrDefault(s => s.ProductionStepId == sId)?.ProductionStepAssignmentStatusId == (int)EnumProductionOrderAssignmentStatus.Completed);
            var beforeStepsAreDone = assignments
                .Where(s => fromStepIds.Contains(s.ProductionStepId))
                .All(a => a.AssignedProgressStatus == (int)EnumAssignedProgressStatus.Finish);

            if (beforeStepsAreNotFromStock && beforeStepsAreCompletedAssignment && beforeStepsAreDone)
            {
                return EnumAssignedProgressStatus.Finish;
            }

            return status;
        }


        private async Task AutoAllowcation(long productionOrderId, ProductionOrderCalcStatusV2Message data)
        {

            var manualAllowcations = await ResetInventoryHandovers(productionOrderId, data);

            var productAssignmentsWithStocks = (await GetAssignRequirements(productionOrderId)).Where(ra => ra.RefProductionStepId == 0 || ra.IsOutsourceStep).ToList();

            var newConflicts = new List<ProductionOrderInventoryConflict>();

            var newHandovers = new List<ProductionHandoverEntity>();

            var ignores = await _manufacturingDBContext.IgnoreAllocation.Where(a => a.ProductionOrderId == productionOrderId).Select(a => a.InventoryDetailId).ToListAsync();

            var requiredDetailRemaining = new Dictionary<long, decimal>();
            foreach (var requireDetail in data.InvRequireDetails)
            {
                requiredDetailRemaining.Add(requireDetail.InventoryRequirementDetailId, requireDetail.PrimaryQuantity);
            }

            foreach (var inv in data.InvDetails)
            {
                var remaining = inv.PrimaryQuantity;

                var handoverManual = manualAllowcations.Where(ma => ma.InventoryDetailId == inv.InventoryDetailId
                //&& ma.ProductionStepLinkDataId == inputQuantity.ProductionStepLinkDataId
                ).ToList();


                foreach (var manual in handoverManual)
                {
                    remaining -= manual.HandoverQuantity;
                    var assignment = productAssignmentsWithStocks
                        .Where(a =>
                        (
                        a.DepartmentId == manual.ToDepartmentId && a.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Input
                        || a.DepartmentId == manual.FromDepartmentId && a.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Output
                        )
                        && a.LinkDataObjectId == manual.ObjectId
                        && (a.ProductionStepId == manual.ToProductionStepId && a.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Input
                        || a.ProductionStepId == manual.FromProductionStepId && a.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Output
                        ))
                        .OrderByDescending(a => a.DataLinkId == manual.ProductionStepLinkDataId)
                        .FirstOrDefault();

                    if (assignment == null) continue;

                    assignment.HandoverQuantity += manual.HandoverQuantity;
                    assignment.RemainQuantity = assignment.RequiredQuantity - assignment.HandoverQuantity;
                }


                var avaiableAssignmentsForProducts = productAssignmentsWithStocks.Where(a => a.LinkDataObjectId == inv.ProductId &&
                   (a.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Input && inv.InventoryTypeId == EnumInventoryType.Output
                   || a.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Output && inv.InventoryTypeId == EnumInventoryType.Input
                   )
               ).ToList();

                var invRequireDetails = data.InvRequireDetails.Where(d => d.ProductId == inv.ProductId &&
                    (
                    d.InventoryRequirementDetailId == inv.InventoryRequirementDetailId
                    || d.DepartmentId == inv.DepartmentId && requiredDetailRemaining[d.InventoryRequirementDetailId] > 0)
                    )
                    .OrderByDescending(d => d.InventoryRequirementDetailId == inv.InventoryRequirementDetailId)
                    .ToList();

                foreach (var requireDetail in invRequireDetails)
                {
                    var requireQuantity = requiredDetailRemaining[requireDetail.InventoryRequirementDetailId];

                    decimal assignQuantity = Math.Min(remaining, requireQuantity);

                    if (assignQuantity > 0)
                    {
                        var matchByRequirements = avaiableAssignmentsForProducts.Where(a => a.DepartmentId == requireDetail.DepartmentId && a.ProductionStepId == requireDetail.ProductionStepId).ToList();

                        IList<ProductionHandoverEntity> requirementHandovers;

                        var isAssignRemaingForLast = false;
                        decimal assignRemaning = 0;

                        if (requireDetail.InventoryRequirementDetailId == inv.InventoryRequirementDetailId)
                        {
                            assignQuantity = remaining;

                            isAssignRemaingForLast = true;

                            (requirementHandovers, remaining) = AssignInvToProductAssignment(productionOrderId, matchByRequirements, inv, requireDetail.InventoryRequirementDetailId, assignQuantity, isAssignRemaingForLast);

                        }
                        else
                        {
                            if (remaining - assignQuantity <= Numbers.MINIMUM_ACCEPT_DECIMAL_NUMBER)
                            {
                                assignQuantity = remaining;
                                isAssignRemaingForLast = true;
                            }

                            (requirementHandovers, assignRemaning) = AssignInvToProductAssignment(productionOrderId, matchByRequirements, inv, requireDetail.InventoryRequirementDetailId, assignQuantity, isAssignRemaingForLast);

                            remaining -= (assignQuantity - assignRemaning);
                        }




                        requireQuantity -= assignQuantity;
                        requiredDetailRemaining[requireDetail.InventoryRequirementDetailId] = requireQuantity;


                        newHandovers.AddRange(requirementHandovers);
                    }

                }

                if (remaining > 0)
                {

                    var matchAllAssignments = avaiableAssignmentsForProducts.Where(a => a.DepartmentId == inv.DepartmentId).ToList();


                    if (matchAllAssignments.Count > 0)
                    {
                        (var byDepartmentHandovers, remaining) = AssignInvToProductAssignment(productionOrderId, matchAllAssignments, inv, null, remaining, true);
                        newHandovers.AddRange(byDepartmentHandovers);
                    }
                }

                if (remaining > 0 || handoverManual.Count > 0)
                {
                    var sum = handoverManual.Sum(x => x.InventoryQuantity ?? 0);
                    var status = EnumConflictAllowcationStatus.None;
                    if (sum > 0)
                        status = EnumConflictAllowcationStatus.Processing;

                    var invRequiredDetal = data.InvRequireDetails.FirstOrDefault(d => d.InventoryRequirementDetailId == inv.InventoryRequirementDetailId);
                    if (sum >= invRequiredDetal?.PrimaryQuantity)
                        status = EnumConflictAllowcationStatus.Completed;
                    if (ignores.Contains(inv.InventoryDetailId))
                        status = EnumConflictAllowcationStatus.Completed;

                    var newConflict = new ProductionOrderInventoryConflict()
                    {
                        ProductionOrderId = productionOrderId,
                        InventoryDetailId = inv.InventoryDetailId,
                        ProductId = inv.ProductId,
                        InventoryTypeId = (int)inv.InventoryTypeId,
                        InventoryId = inv.InventoryId,
                        InventoryDate = inv.Date,
                        InventoryCode = inv.InventoryCode,
                        InventoryQuantity = inv.PrimaryQuantity,
                        InventoryRequirementDetailId = inv.InventoryRequirementDetailId,
                        InventoryRequirementId = null,
                        RequireQuantity = invRequiredDetal?.PrimaryQuantity,
                        InventoryRequirementCode = invRequiredDetal?.InventoryRequirementCode,
                        Content = inv.Description,
                        HandoverInventoryQuantitySum = sum,
                        ConflictAllowcationStatusId = (int)status
                    };

                    newConflicts.Add(newConflict);
                }

            }

            await _manufacturingDBContext.ProductionHandover.AddRangeAsync(newHandovers);

            await _manufacturingDBContext.ProductionOrderInventoryConflict.AddRangeAsync(newConflicts);

            await _manufacturingDBContext.SaveChangesAsync();

        }

        private async Task<IList<ProductionHandoverEntity>> ResetInventoryHandovers(long productionOrderId, ProductionOrderCalcStatusV2Message data)
        {
            var oldAutoAllowcations = await _manufacturingDBContext.ProductionHandover
                .Where(h => h.ProductionOrderId == productionOrderId
                    && h.IsAuto &&
                    (h.FromDepartmentId == 0 || h.ToDepartmentId == 0)
                ).ToListAsync();
            _manufacturingDBContext.ProductionHandover.RemoveRange(oldAutoAllowcations);

            var oldConflicts = await _manufacturingDBContext.ProductionOrderInventoryConflict
                .Where(c => c.ProductionOrderId == productionOrderId)
                .ToListAsync();
            _manufacturingDBContext.ProductionOrderInventoryConflict.RemoveRange(oldConflicts);

            var manualAllowcations = await _manufacturingDBContext.ProductionHandover
                .Where(h => h.ProductionOrderId == productionOrderId
                    && !h.IsAuto
                    && (h.FromDepartmentId == 0 || h.ToDepartmentId == 0)
                //&& 
                ).ToListAsync();

            var invalidManualAllocations = manualAllowcations.Where(a =>
            {
                var invDetails = data.InvDetails.Where(inv => inv.InventoryDetailId == a.InventoryDetailId).ToList();
                if (!invDetails.Any()) return true;
                if (invDetails.Any(d => d.ProductId != a.InventoryProductId)) return true;
                return false;
            }).ToList();
            foreach (var invalidHandover in invalidManualAllocations)
            {
                manualAllowcations.Remove(invalidHandover);
            }
            _manufacturingDBContext.ProductionHandover.RemoveRange(invalidManualAllocations);

            await _manufacturingDBContext.SaveChangesAsync();
            return manualAllowcations;

        }

        private async Task<List<StepDepartmentAssignment>> GetAssignRequirements(long productionOrderId)
        {
            var assignments = await _manufacturingDBContext.ProductionAssignment.Include(a => a.ProductionAssignmentDetail)
                .Where(a => a.ProductionOrderId == productionOrderId)
                .OrderByDescending(a => a.AssignmentQuantity)
                .ThenBy(a => a.DepartmentId)
                .ToListAsync();

            var productionSteps = await _manufacturingDBContext.ProductionStep
                .Include(s => s.Step)
                .Include(s => s.ProductionStepLinkDataRole)
                .ThenInclude(r => r.ProductionStepLinkData)
                .Where(s => s.ContainerId == productionOrderId && s.ContainerTypeId == (int)EnumContainerType.ProductionOrder && !s.IsFinish && s.IsGroup != true)
                .ToListAsync();

            productionSteps = productionSteps.OrderBy(s => s.Step.SortOrder).ThenBy(s => s.SortOrder).ToList();

            var linkDataInputs = productionSteps.SelectMany(s =>
                s.ProductionStepLinkDataRole
                .Where(r => r.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Input)
                .Select(r => r.ProductionStepLinkData.ProductionStepLinkDataId)
            ).ToList();

            var linkDataOutputs = productionSteps.SelectMany(s =>
                s.ProductionStepLinkDataRole
                .Where(r => r.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Output)
                .Select(r => r.ProductionStepLinkData.ProductionStepLinkDataId)
            ).ToList();

            var inputAssignments = new List<StepDepartmentAssignment>();
            var outputAssignments = new List<StepDepartmentAssignment>();

            foreach (var pStep in productionSteps)
            {

                var toCurrentStep = pStep.ProductionStepLinkDataRole
                  .Where(r => r.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Input)
                  .Select(r => r.ProductionStepLinkData)
                  // .Where(d => d.LinkDataObjectTypeId == (int)EnumProductionStepLinkDataObjectType.Product && !linkDataOutputs.Any(linkDataId => linkDataId == d.ProductionStepLinkDataId))
                  .ToList();

                var fromCurrentStep = pStep.ProductionStepLinkDataRole
                   .Where(r => r.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Output)
                   .Select(r => r.ProductionStepLinkData)
                   //.Where(d => d.LinkDataObjectTypeId == (int)EnumProductionStepLinkDataObjectType.Product && !linkDataInputs.Any(linkDataId => linkDataId == d.ProductionStepLinkDataId))
                   .ToList();


                var stepAssignments = assignments.Where(a => a.ProductionStepId == pStep.ProductionStepId).ToList();

                foreach (var a in stepAssignments)
                {
                    var assignLinkData = pStep.ProductionStepLinkDataRole
                           .Where(r => r.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Output)
                           .Select(r => r.ProductionStepLinkData)
                           .FirstOrDefault(d => d.ProductionStepLinkDataId == a.ProductionStepLinkDataId);
                    if (assignLinkData == null) continue;


                    var createAssignmentLinkQuantity = (EnumProductionStepLinkDataRoleType productionStepLinkDataRoleTypeId, ProductionStepLinkData linkData, long refProductionStepId) =>
                    {
                        var requireQuantity = a.AssignmentQuantity * linkData.QuantityOrigin / assignLinkData.QuantityOrigin;
                        return new StepDepartmentAssignment
                        {
                            RefProductionStepId = refProductionStepId,
                            IsOutsourceStep = assignLinkData.ExportOutsourceQuantity > 0 || pStep.OutsourceStepRequestId > 0,
                            ProductionStepLinkDataRoleTypeId = productionStepLinkDataRoleTypeId,
                            DataLinkId = linkData.ProductionStepLinkDataId,
                            AssignDataLinkId = a.ProductionStepLinkDataId,
                            ProductionStepId = pStep.ProductionStepId,
                            LinkDataObjectTypeId = (EnumProductionStepLinkDataObjectType)linkData.LinkDataObjectTypeId,
                            LinkDataObjectId = linkData.LinkDataObjectId,
                            DepartmentId = a.DepartmentId,
                            RequiredQuantity = requireQuantity,
                            HandoverQuantity = 0,
                            RemainQuantity = requireQuantity
                        };
                    };


                    foreach (var requireExport in toCurrentStep)
                    {
                        var refProductionStepId = linkDataOutputs.FirstOrDefault(linkDataId => linkDataId == requireExport.ProductionStepLinkDataId);

                        if (requireExport.LinkDataObjectTypeId != (int)EnumProductionStepLinkDataObjectType.Product)
                        {
                            refProductionStepId = 0;//stock
                        }


                        inputAssignments.Add(createAssignmentLinkQuantity(EnumProductionStepLinkDataRoleType.Input, requireExport, refProductionStepId));
                    }


                    foreach (var requireImport in fromCurrentStep)
                    {

                        var refProductionStepId = linkDataInputs.FirstOrDefault(linkDataId => linkDataId == requireImport.ProductionStepLinkDataId);

                        if (requireImport.LinkDataObjectTypeId != (int)EnumProductionStepLinkDataObjectType.Product)
                        {
                            refProductionStepId = 0;//stock
                        }

                        outputAssignments.Add(createAssignmentLinkQuantity(EnumProductionStepLinkDataRoleType.Output, requireImport, refProductionStepId));

                    }
                }
            }

            return inputAssignments.Union(outputAssignments).ToList();
        }

        private (IList<ProductionHandoverEntity> newHandovers, decimal remaining) AssignInvToProductAssignment(long productionOrderId, List<StepDepartmentAssignment> productAssignments, InventoryDetailByProductionOrderModel inv, long? inventoryRequirementDetailId, decimal quantity, bool isAssignRemaingForLast)
        {
            var assignRemaning = quantity;
            var newHandovers = new List<ProductionHandoverEntity>();

            for (var i = 0; i < productAssignments.Count; i++)
            {
                var productAssignment = productAssignments[i];

                var departmentId = productAssignment.DepartmentId;
                var productionStepId = productAssignment.ProductionStepId;
                var dataLinkId = productAssignment.DataLinkId;

                if (assignRemaning == 0) continue;

                decimal assignQuantity;
                if (isAssignRemaingForLast && i == productAssignments.Count - 1)
                {
                    assignQuantity = assignRemaning;
                }
                else
                {
                    if (productAssignment.RemainQuantity >= assignRemaning || inventoryRequirementDetailId > 0)//assign all for inv requirement
                    {
                        assignQuantity = assignRemaning;
                    }
                    else
                    {
                        assignQuantity = productAssignment.RemainQuantity;
                    }
                }

                productAssignment.RemainQuantity -= assignQuantity;

                assignRemaning -= assignQuantity;

                var handoverItem = new ProductionHandoverEntity()
                {
                    HandoverDatetime = inv.Date,
                    ProductionOrderId = productionOrderId,
                    ObjectId = inv.ProductId,
                    ObjectTypeId = (int)EnumProductionStepLinkDataObjectType.Product,
                    FromDepartmentId = inv.InventoryTypeId == EnumInventoryType.Output ? 0 : departmentId,
                    FromProductionStepId = inv.InventoryTypeId == EnumInventoryType.Output ? 0 : productionStepId,
                    ToDepartmentId = inv.InventoryTypeId == EnumInventoryType.Output ? departmentId : 0,
                    ToProductionStepId = inv.InventoryTypeId == EnumInventoryType.Output ? productionStepId : 0,
                    HandoverQuantity = assignQuantity,
                    Status = (int)EnumHandoverStatus.Accepted,
                    InventoryRequirementDetailId = inventoryRequirementDetailId,
                    InventoryDetailId = inv.InventoryDetailId,
                    InventoryProductId = inv.ProductId,
                    InventoryId = inv.InventoryId,
                    InventoryCode = inv.InventoryCode,
                    InventoryQuantity = assignQuantity,
                    Note = "auto",
                    ProductionStepLinkDataId = dataLinkId,
                    IsAuto = true
                };

                newHandovers.Add(handoverItem);
            }
            return (newHandovers, assignRemaning);
        }

        private sealed class StepDepartmentAssignment
        {
            public required long RefProductionStepId { get; set; }
            public required bool IsOutsourceStep { get; set; }
            public required long DataLinkId { get; set; }
            public required long AssignDataLinkId { get; set; }
            public EnumProductionStepLinkDataRoleType ProductionStepLinkDataRoleTypeId { get; set; }
            public required long ProductionStepId { get; set; }
            public required EnumProductionStepLinkDataObjectType LinkDataObjectTypeId { get; set; }
            public required long LinkDataObjectId { get; set; }
            public required int DepartmentId { get; set; }
            public required decimal RequiredQuantity { get; set; }
            public required decimal HandoverQuantity { get; set; }
            public required decimal RemainQuantity { get; set; }
        }

    }
}
