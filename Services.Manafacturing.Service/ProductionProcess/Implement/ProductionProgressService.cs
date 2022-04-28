using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Manafacturing.Model.ProductionOrder;
using VErp.Services.Manafacturing.Service.ProductionAssignment;
using VErp.Services.Manafacturing.Service.ProductionAssignment.Implement;
using VErp.Services.Manafacturing.Service.ProductionHandover;
using VErp.Services.Manafacturing.Service.ProductionOrder;
using VErp.Services.Manafacturing.Service.StatusProcess;
using VErp.Services.Manafacturing.Service.StatusProcess.Implement;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;

namespace VErp.Services.Manafacturing.Service.ProductionProcess.Implement
{
    public class ProductionProgressService : StatusProcessService, IProductionProgressService
    {

        private readonly IProductionHandoverService _productionHandoverService;
        private readonly IMaterialAllocationService _materialAllocationService;
        private readonly IProductionOrderService _productionOrderService;
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly IActivityLogService _activityLogService;
        private readonly IProductionAssignmentService _productionAssignmentService;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public ProductionProgressService(IProductionHandoverService productionHandoverService,
            IMaterialAllocationService materialAllocationService,
            IProductionOrderService productionOrderService,
            ManufacturingDBContext manufacturingDBContext,
            IActivityLogService activityLogService,
             IMapper mapper,
            ILogger<ProductionProgressService> logger,
            IProductionAssignmentService productionAssignmentService) : base(manufacturingDBContext, activityLogService, logger, mapper)
        {
            _productionHandoverService = productionHandoverService;
            _materialAllocationService = materialAllocationService;
            _productionOrderService = productionOrderService;
            _manufacturingDBContext = manufacturingDBContext;
            _activityLogService = activityLogService;
            _mapper = mapper;
            _logger = logger;
            _productionAssignmentService = productionAssignmentService;
        }


        public async Task<bool> CalcAbdUpdateProductionOrderStatus(ProductionOrderStatusDataModel data)
        {
            await _materialAllocationService.UpdateIgnoreAllocation(new[] { data.ProductionOrderCode });

            await _productionHandoverService.ChangeAssignedProgressStatus(data.ProductionOrderCode, data.InventoryCode, data.Inventories);


            var productionOrder = _manufacturingDBContext.ProductionOrder
                .Include(po => po.ProductionOrderDetail)
                .FirstOrDefault(po => po.ProductionOrderCode == data.ProductionOrderCode);

            if (productionOrder == null)
                throw new BadRequestException(GeneralCode.ItemNotFound, "Lệnh sản xuất không tồn tại");

            var departmentHandoverDetails = await GetDepartmentHandoverDetail(productionOrder.ProductionOrderId, null, null, data.Inventories);

            var oldStatus = productionOrder.ProductionOrderStatus;

            if (productionOrder.ProductionOrderStatus == (int)EnumProductionStatus.Finished) return true;
            try
            {
                var steps = await _manufacturingDBContext.ProductionStep.Where(s => s.ContainerId == productionOrder.ProductionOrderId && s.ContainerTypeId == (int)EnumContainerType.ProductionOrder).ToListAsync();

                var inputs = await (from d in _manufacturingDBContext.ProductionStepLinkData
                                    join r in _manufacturingDBContext.ProductionStepLinkDataRole on d.ProductionStepLinkDataId equals r.ProductionStepLinkDataId
                                    join s in _manufacturingDBContext.ProductionStep on r.ProductionStepId equals s.ProductionStepId
                                    where s.ContainerId == productionOrder.ProductionOrderId && s.ContainerTypeId == (int)EnumContainerType.ProductionOrder
                                    && r.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Input
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
                                     select new
                                     {
                                         r.ProductionStepId,
                                         d
                                     }).ToListAsync();


                /**
                 * All output not have input
                 */
                var endSteps = steps.Where(s => s.ParentId != null &&
                        outputs.Where(p => p.ProductionStepId == s.ProductionStepId)
                                .All(d => !inputs.Any(o => o.d.ProductionStepLinkDataId == d.d.ProductionStepLinkDataId))
                 ).ToList();

                var assignments = await _manufacturingDBContext.ProductionAssignment.Where(s => s.ProductionOrderId == productionOrder.ProductionOrderId).ToListAsync();

                //var allocation = await _manufacturingDBContext.MaterialAllocation.Where(a => a.ProductionOrderId == productionOrder.ProductionOrderId).ToListAsync();

                var hasAllocation = await _manufacturingDBContext.MaterialAllocation.AnyAsync(a => a.ProductionOrderId == productionOrder.ProductionOrderId);

                var hasHandOver = await _manufacturingDBContext.ProductionHandover.AnyAsync(s => s.ProductionOrderId == productionOrder.ProductionOrderId);

                if (!data.Inventories.Any() && !hasHandOver && !hasAllocation)
                {
                    productionOrder.ProductionOrderStatus = (int)EnumProductionStatus.Waiting;
                }
                else
                {
                    productionOrder.ProductionOrderStatus = (int)EnumProductionStatus.ProcessingLessStarted;
                }



                if (departmentHandoverDetails.All(s => s.InputDatas.Where(d => d.FromStepId == null).All(d => d.ReceivedQuantity >= d.RequireQuantity)))//startSteps.All(s => assignments.Any(a => a.ProductionStepId == s.ProductionStepId) && assignments.All(a => a.ProductionStepId == s.StepId && (a.IsManualFinish || a.AssignedProgressStatus == (int)EnumAssignedProgressStatus.Finish))))
                {
                    productionOrder.ProductionOrderStatus = (int)EnumProductionStatus.ProcessingFullStarted;
                }

                if (endSteps.All(s => assignments.Any(a => a.ProductionStepId == s.ProductionStepId) && assignments.All(a => a.ProductionStepId == s.StepId && (a.IsManualFinish || a.AssignedProgressStatus == (int)EnumAssignedProgressStatus.Finish))))
                {
                    productionOrder.ProductionOrderStatus = (int)EnumProductionStatus.Completed;
                }
                else
                {

                    if (productionOrder.EndDate < DateTime.UtcNow)
                    {
                        productionOrder.ProductionOrderStatus = (int)EnumProductionStatus.OverDeadline;
                    }

                    var prodDetails = await _manufacturingDBContext.ProductionOrderDetail.Where(d => d.ProductionOrderId == productionOrder.ProductionOrderId).ToListAsync();


                    var inputInventories = data.Inventories;

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
                    if (isFinish)
                    {
                        productionOrder.ProductionOrderStatus = (int)EnumProductionStatus.MissHandOverInfo;
                    }
                }

                if (oldStatus != productionOrder.ProductionOrderStatus)
                {
                    _manufacturingDBContext.SaveChanges();
                    await _activityLogService.CreateLog(EnumObjectType.ProductionOrder, productionOrder.ProductionOrderId, $"Cập nhật trạng thái lệnh sản xuất ", new { productionOrder, data, isManual = false }.JsonSerialize());
                }


                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateProductOrderStatus");
                throw;
            }

        }
    }
}
