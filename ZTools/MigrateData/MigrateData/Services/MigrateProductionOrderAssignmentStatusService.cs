using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Infrastructure.EF.AccountancyDB;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.EF.StockDB;
using VErp.Services.Accountancy.Service.Input;
using VErp.Infrastructure.EF.EFExtensions;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.Library;

namespace MigrateData.Services
{


    internal interface IMigrateProductionOrderAssignmentStatusService
    {
        Task Execute();
    }

    internal class MigrateProductionOrderAssignmentStatusService : IMigrateProductionOrderAssignmentStatusService
    {
        private readonly ManufacturingDBContext _manufacturingDBContext;

        public MigrateProductionOrderAssignmentStatusService(ManufacturingDBContext manufacturingDBContext)
        {
            _manufacturingDBContext = manufacturingDBContext;
        }

        public async Task Execute()
        {
            var productionOrderIds = await _manufacturingDBContext.ProductionAssignment
                .IgnoreQueryFilters()
                .Select(p => p.ProductionOrderId)
                .ToListAsync();

            var steps = await _manufacturingDBContext.ProductionStep
             .IgnoreQueryFilters()
             .Where(s => !s.IsDeleted)
             .Include(s => s.ProductionStepLinkDataRole.Where(r => r.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Output))
             .ThenInclude(r => r.ProductionStepLinkData)
             .Where(s => s.ContainerTypeId == (int)EnumContainerType.ProductionOrder && productionOrderIds.Contains(s.ContainerId))
             .ToListAsync();

            var assigns = await _manufacturingDBContext.ProductionAssignment.IgnoreQueryFilters().Include(a => a.ProductionAssignmentDetail).Where(a => productionOrderIds.Contains(a.ProductionOrderId)).ToListAsync();

            var infos = await _manufacturingDBContext.ProductionOrder.IgnoreQueryFilters().Where(o => productionOrderIds.Contains(o.ProductionOrderId)).ToListAsync();

            foreach (var productionOrderId in productionOrderIds)
            {
                var productionOrderAssignmentStatus = assigns.Count > 0 ? EnumProductionOrderAssignmentStatus.AssignProcessing : EnumProductionOrderAssignmentStatus.NoAssignment;

                var allCompleted = true;

                var productionSteps = steps.Where(s => s.ContainerId == productionOrderId).ToList();
                foreach (var productionStep in productionSteps)
                {
                    var outputs = productionStep.ProductionStepLinkDataRole;
                    var isAssignmentCompleted = false;
                    foreach (var o in outputs)
                    {
                        var stepAssigns = assigns.Where(a => a.ProductionStepLinkDataId == o.ProductionStepLinkDataId);

                        var assignQuantity = stepAssigns.Sum(a => a.AssignmentQuantity);

                        var isCompletedAssignDate = stepAssigns.All(a => a.StartDate.HasValue && a.EndDate.HasValue && a.ProductionAssignmentDetail.Count > 0);

                        if (assignQuantity.SubProductionDecimal(o.ProductionStepLinkData.Quantity) == 0 && isCompletedAssignDate)
                        {
                            isAssignmentCompleted = true;
                        }
                    }
                    if (outputs.Count > 0 && !isAssignmentCompleted)
                    {
                        allCompleted = false;
                    }
                }

                if (allCompleted && productionSteps.Count > 0)
                {
                    productionOrderAssignmentStatus = EnumProductionOrderAssignmentStatus.Completed;
                }
                var info = infos.FirstOrDefault(o => o.ProductionOrderId == productionOrderId);

                if (info != null)
                {
                    info.ProductionOrderAssignmentStatusId = (int)productionOrderAssignmentStatus;
                }
            }

            await _manufacturingDBContext.SaveChangesAsync();
        }
    }
}
