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

namespace VErp.Services.Manafacturing.Service.ProductionProcess.Implement
{
    public class ProductionProgressService : StatusProcessService, IProductionProgressService
    {

        private readonly IProductionHandoverReceiptService _productionHandoverReceiptService;
        private readonly IMaterialAllocationService _materialAllocationService;
        private readonly IProductionOrderService _productionOrderService;
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly IActivityLogService _activityLogService;
        private readonly IProductionAssignmentService _productionAssignmentService;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public ProductionProgressService(IProductionHandoverReceiptService productionHandoverReceiptService,
            IMaterialAllocationService materialAllocationService,
            IProductionOrderService productionOrderService,
            ManufacturingDBContext manufacturingDBContext,
            IActivityLogService activityLogService,
             IMapper mapper,
            ILogger<ProductionProgressService> logger,
            IProductionAssignmentService productionAssignmentService) : base(manufacturingDBContext, activityLogService, logger, mapper)
        {
            _productionHandoverReceiptService = productionHandoverReceiptService;
            _materialAllocationService = materialAllocationService;
            _productionOrderService = productionOrderService;
            _manufacturingDBContext = manufacturingDBContext;
            _activityLogService = activityLogService;
            _mapper = mapper;
            _logger = logger;
            _productionAssignmentService = productionAssignmentService;
        }


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


                /**
                 * All output not have input
                 */
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
                    
                }*/

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


        public async Task<bool> CalcAndUpdateProductionOrderStatusV2(ProductionOrderCalcStatusV2Message data)
        {
           

            var productionOrder = _manufacturingDBContext.ProductionOrder
                .Include(po => po.ProductionOrderDetail)
                .FirstOrDefault(po => po.ProductionOrderCode == data.ProductionOrderCode);

            if (productionOrder == null)
                throw new BadRequestException(GeneralCode.ItemNotFound, "Lệnh sản xuất không tồn tại");

            await AutoAllowcation(productionOrder.ProductionOrderId, data);

            //await _materialAllocationService.UpdateIgnoreAllocation(new[] { data.ProductionOrderCode }, true);

            //await _productionHandoverReceiptService.ChangeAssignedProgressStatus(data.ProductionOrderCode, data.Description, data.Inventories);


            var departmentHandoverDetails = await GetDepartmentHandoverDetail(productionOrder.ProductionOrderId, null, null, data.Inventories);

            var oldStatus = productionOrder.ProductionOrderStatus;

            if (productionOrder.ProductionOrderStatus == (int)EnumProductionStatus.Finished && productionOrder.IsManualFinish) return true;
            try
            {
                var steps = await _manufacturingDBContext.ProductionStep.Where(s => !s.IsFinish && s.ContainerId == productionOrder.ProductionOrderId && s.ContainerTypeId == (int)EnumContainerType.ProductionOrder).ToListAsync();

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


                /**
                 * All output not have input
                 */
                //var endSteps = steps.Where(s => s.ParentId != null &&
                //        outputs.Where(p => p.ProductionStepId == s.ProductionStepId)
                //                .All(d => !inputs.Any(o => o.d.ProductionStepLinkDataId == d.d.ProductionStepLinkDataId))
                // ).ToList();

                var assignments = await _manufacturingDBContext.ProductionAssignment.Where(s => s.ProductionOrderId == productionOrder.ProductionOrderId).ToListAsync();

                //var allocation = await _manufacturingDBContext.MaterialAllocation.Where(a => a.ProductionOrderId == productionOrder.ProductionOrderId).ToListAsync();

                var hasAllocation = await _manufacturingDBContext.MaterialAllocation.AnyAsync(a => a.ProductionOrderId == productionOrder.ProductionOrderId);

                var hasHandOver = await _manufacturingDBContext.ProductionHandover.AnyAsync(s => s.ProductionOrderId == productionOrder.ProductionOrderId);

                if (!data.Inventories.Any(iv => iv.InventoryQuantity > 0) && !hasHandOver && !hasAllocation)
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
                        .Where(i => i.ProductId == productionOrderDetail.ProductId)
                        .Sum(i => i.InventoryQuantity);

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
                    
                }*/

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
                var invDetails = data.Inventories.Where(inv => inv.InventoryDetailId == a.InventoryDetailId).ToList();
                if (!invDetails.Any()) return true;
                if (invDetails.Any(d => d.ProductId != a.ObjectId)) return true;
                return false;
            });
            foreach (var invalidHandover in invalidManualAllocations)
            {
                manualAllowcations.Remove(invalidHandover);
            }
            _manufacturingDBContext.ProductionHandover.RemoveRange(invalidManualAllocations);

            await _manufacturingDBContext.SaveChangesAsync();
            return manualAllowcations;

        }

        private async Task<List<StepDepartmentAssignment>> GetProductRequirements(long productionOrderId)
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
                var stepAssignments = assignments.Where(a => a.ProductionStepId == pStep.ProductionStepId).ToList();

                foreach (var a in stepAssignments)
                {
                    var assignLinkData = pStep.ProductionStepLinkDataRole
                           .Where(r => r.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Output)
                           .Select(r => r.ProductionStepLinkData)
                           .FirstOrDefault(d => d.ProductionStepLinkDataId == a.ProductionStepLinkDataId);
                    if (assignLinkData == null) continue;


                    var createAssignmentLinkQuantity = (EnumProductionStepLinkDataRoleType ProductionStepLinkDataRoleTypeId, ProductionStepLinkData linkData) =>
                    {
                        var requireQuantity = a.AssignmentQuantity * linkData.QuantityOrigin / assignLinkData.QuantityOrigin;
                        return new StepDepartmentAssignment
                        {
                            ProductionStepLinkDataRoleTypeId = ProductionStepLinkDataRoleTypeId,
                            DataLinkId = linkData.ProductionStepLinkDataId,
                            ProductionStepId = pStep.ProductionStepId,
                            ProductId = (int)linkData.LinkDataObjectId,
                            DepartmentId = a.DepartmentId,
                            RequiredQuantity = requireQuantity,
                            HandoverQuantity = 0,
                            RemainQuantity = requireQuantity
                        };
                    };

                    var fromStocks = pStep.ProductionStepLinkDataRole
                      .Where(r => r.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Input)
                      .Select(r => r.ProductionStepLinkData)
                      .Where(d => d.LinkDataObjectTypeId == (int)EnumProductionStepLinkDataObjectType.Product && !linkDataOutputs.Any(linkDataId => linkDataId == d.ProductionStepLinkDataId))
                      .ToList();

                    foreach (var requireExport in fromStocks)
                    {
                        inputAssignments.Add(createAssignmentLinkQuantity(EnumProductionStepLinkDataRoleType.Input, requireExport));
                    }

                    var toStocks = pStep.ProductionStepLinkDataRole
                      .Where(r => r.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Output)
                      .Select(r => r.ProductionStepLinkData)
                      .Where(d => d.LinkDataObjectTypeId == (int)EnumProductionStepLinkDataObjectType.Product && !linkDataInputs.Any(linkDataId => linkDataId == d.ProductionStepLinkDataId))
                      .ToList();

                    foreach (var requireImport in toStocks)
                    {
                        outputAssignments.Add(createAssignmentLinkQuantity(EnumProductionStepLinkDataRoleType.Output, requireImport));
                    }
                }
            }

            return inputAssignments.Union(outputAssignments).ToList();
        }

        private IList<ProductionHandoverEntity> AssignInvToProductAssignment(long productionOrderId, List<StepDepartmentAssignment> productAssignments, InventoryByProductionOrderModel inv, decimal quantity)
        {
            var remaining = quantity;
            var newHandovers = new List<ProductionHandoverEntity>();

            for (var i = 0; i < productAssignments.Count; i++)
            {
                var productAssignment = productAssignments[i];

                var departmentId = productAssignment.DepartmentId;
                var productionStepId = productAssignment.ProductionStepId;
                var dataLinkId = productAssignment.DataLinkId;

                if (remaining == 0) continue;

                var assignQuantity = 0M;
                if (i == productAssignments.Count - 1)
                {
                    assignQuantity = remaining;
                }
                else
                {
                    if (productAssignment.RemainQuantity >= remaining)
                    {
                        assignQuantity = remaining;
                    }
                    else
                    {
                        assignQuantity = productAssignment.RemainQuantity;
                    }
                }
                remaining -= assignQuantity;

                var handoverItem = new ProductionHandoverEntity()
                {
                    HandoverDatetime = inv.InventoryDate,
                    ProductionOrderId = productionOrderId,
                    ObjectId = inv.ProductId,
                    ObjectTypeId = (int)EnumProductionStepLinkDataObjectType.Product,
                    FromDepartmentId = inv.InventoryTypeId == EnumInventoryType.Output ? 0 : departmentId,
                    FromProductionStepId = inv.InventoryTypeId == EnumInventoryType.Output ? 0 : productionStepId,
                    ToDepartmentId = inv.InventoryTypeId == EnumInventoryType.Output ? departmentId : 0,
                    ToProductionStepId = inv.InventoryTypeId == EnumInventoryType.Output ? productionStepId : 0,
                    HandoverQuantity = assignQuantity,
                    Status = (int)EnumHandoverStatus.Accepted,
                    InventoryRequirementDetailId = inv.InventoryRequirementDetailId,
                    InventoryDetailId = inv.InventoryDetailId,
                    InventoryProductId = inv.ProductId,
                    ProductionStepLinkDataId = dataLinkId,
                    IsAuto = true
                };

                newHandovers.Add(handoverItem);
            }
            return newHandovers;
        }

        private async Task AutoAllowcation(long productionOrderId, ProductionOrderCalcStatusV2Message data)
        {

            var manualAllowcations = await ResetInventoryHandovers(productionOrderId, data);

            var productAssignments = await GetProductRequirements(productionOrderId);

            var newConflicts = new List<ProductionOrderInventoryConflict>();

            var newHandovers = new List<ProductionHandoverEntity>();

            foreach (var inv in data.Inventories)
            {
                var remaining = inv.InventoryQuantity;

                var handoverManual = manualAllowcations.Where(ma => ma.InventoryDetailId == inv.InventoryDetailId
                //&& ma.ProductionStepLinkDataId == inputQuantity.ProductionStepLinkDataId
                ).ToList();


                foreach (var manual in handoverManual)
                {
                    remaining -= manual.HandoverQuantity;
                    var assignment = productAssignments
                        .Where(a =>
                        (
                        a.DepartmentId == manual.ToDepartmentId && a.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Input
                        || a.DepartmentId == manual.FromDepartmentId && a.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Output
                        )
                        && a.ProductId == manual.ObjectId
                        && (a.ProductionStepId == manual.ToProductionStepId && a.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Input
                        || a.ProductionStepId == manual.FromProductionStepId && a.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Output
                        ))
                        .OrderByDescending(a => a.DataLinkId == manual.ProductionStepLinkDataId)
                        .FirstOrDefault();

                    if (assignment == null) continue;

                    assignment.HandoverQuantity += manual.HandoverQuantity;
                    assignment.RemainQuantity = assignment.RequiredQuantity - assignment.HandoverQuantity;
                }

                var avaiableAssignmentsForProducts = productAssignments.Where(a => a.ProductId == inv.ProductId &&
                (a.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Input && inv.InventoryTypeId == EnumInventoryType.Output
                || a.ProductionStepLinkDataRoleTypeId == EnumProductionStepLinkDataRoleType.Output && inv.InventoryTypeId == EnumInventoryType.Input
                )
                ).ToList();


                var matchAllAssignments = avaiableAssignmentsForProducts.Where(a => a.ProductionStepId == inv.ProductionStepId && a.DepartmentId == inv.DepartmentId).ToList();

                IList<ProductionHandoverEntity> handovers = new List<ProductionHandoverEntity>();
                if (matchAllAssignments.Count > 0)
                {
                    handovers = AssignInvToProductAssignment(productionOrderId, matchAllAssignments, inv, remaining);
                }
                else
                {
                    var matchDepartments = avaiableAssignmentsForProducts.Where(a => a.DepartmentId == inv.DepartmentId).ToList();
                    if (matchDepartments.Count > 0)
                    {
                        handovers = AssignInvToProductAssignment(productionOrderId, matchAllAssignments, inv, remaining);
                    }

                }

                if (handovers.Count > 0)
                {
                    newHandovers.AddRange(handovers);

                }
                else
                {
                    var newConflict = new ProductionOrderInventoryConflict()
                    {
                        ProductionOrderId = productionOrderId,
                        InventoryDetailId = inv.InventoryDetailId,
                        ProductId = inv.ProductId,
                        InventoryTypeId = (int)inv.InventoryTypeId,
                        InventoryId = inv.InventoryId,
                        InventoryDate = inv.InventoryDate,
                        InventoryCode = inv.InventoryCode,
                        InventoryQuantity = inv.InventoryQuantity,
                        InventoryRequirementDetailId = inv.InventoryRequirementDetailId,
                        InventoryRequirementId = inv.InventoryRequirementId,
                        RequireQuantity = inv.RequireQuantity,
                        InventoryRequirementCode = inv.InventoryRequirementCode,
                        Content = inv.Content
                    };

                    newConflicts.Add(newConflict);
                }

            }

            await _manufacturingDBContext.ProductionHandover.AddRangeAsync(newHandovers);

            await _manufacturingDBContext.ProductionOrderInventoryConflict.AddRangeAsync(newConflicts);

            await _manufacturingDBContext.SaveChangesAsync();

        }

        private sealed class StepDepartmentAssignment
        {
            public long DataLinkId { get; set; }
            public EnumProductionStepLinkDataRoleType ProductionStepLinkDataRoleTypeId { get; set; }
            public required long ProductionStepId { get; set; }
            public required int ProductId { get; set; }
            public required int DepartmentId { get; set; }
            public required decimal RequiredQuantity { get; set; }
            public required decimal HandoverQuantity { get; set; }
            public required decimal RemainQuantity { get; set; }
        }
    }
}
