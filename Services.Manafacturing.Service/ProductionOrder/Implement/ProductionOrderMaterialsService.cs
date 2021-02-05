using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Service;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;
using VErp.Services.Manafacturing.Model.ProductionStep;
using AutoMapper.QueryableExtensions;
using VErp.Services.Manafacturing.Model.ProductionOrder;

namespace VErp.Services.Manafacturing.Service.ProductionOrder.Implement
{
    public class ProductionOrderMaterialsService : IProductionOrderMaterialsService
    {
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly ICustomGenCodeHelperService _customGenCodeHelperService;
        private readonly IProductHelperService _productHelperService;

        public ProductionOrderMaterialsService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<ProductionOrderMaterialsService> logger
            , IMapper mapper
            , ICustomGenCodeHelperService customGenCodeHelperService
            , IProductHelperService productHelperService)
        {
            _manufacturingDBContext = manufacturingDB;
            _activityLogService = activityLogService;
            _logger = logger;
            _mapper = mapper;
            _customGenCodeHelperService = customGenCodeHelperService;
            _productHelperService = productHelperService;
        }

        public async Task<IList<ProductionOrderMaterialsModel>> GetProductionOrderMaterials(long productionOrderId)
        {
            var roles = await _manufacturingDBContext.ProductionStepLinkDataRole.AsNoTracking()
                .Include(x => x.ProductionStep).ThenInclude(s => s.Step)
                .Include(x => x.ProductionStepLinkData)
                .Where(x => x.ProductionStep.ContainerId == productionOrderId && x.ProductionStep.ContainerTypeId == (int)EnumContainerType.ProductionOrder)
                .ToListAsync();

            var roleInputData = roles.GroupBy(x => x.ProductionStepLinkDataId)
                .Where(x => x.Count() == 1)
                .Select(x => x.First())
                .Where(x => x.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Input)
                .ToList();

            var materialsAssigned = (from r in roleInputData
                                     join a in _manufacturingDBContext.ProductionAssignment
                                         on r.ProductionStepLinkDataId equals a.ProductionStepLinkDataId
                                     select new ProductionOrderMaterialsModel
                                     {
                                         AssignmentQuantity = a.AssignmentQuantity,
                                         DepartmentId = a.DepartmentId,
                                         ProductId = r.ProductionStepLinkData.ObjectId,
                                         ProductionStepId = r.ProductionStepId,
                                         ProductionStepTitle = r.ProductionStep.Step?.StepName,
                                         ProductionStepLinkDataId = r.ProductionStepLinkDataId,
                                         Quantity = r.ProductionStepLinkData.Quantity - r.ProductionStepLinkData.OutsourceQuantity.GetValueOrDefault(),
                                         RateQuantity = a.AssignmentQuantity /(r.ProductionStepLinkData.Quantity - r.ProductionStepLinkData.OutsourceQuantity.GetValueOrDefault())
                                     }).ToList();
            var calcuTotalAssignmentQuantity = from m in materialsAssigned
                                               group m by new
                                               {
                                                   m.ProductionStepId,
                                                   m.ProductionStepLinkDataId
                                               } into g
                                               select new
                                               {
                                                   g.Key.ProductionStepId,
                                                   g.Key.ProductionStepLinkDataId,
                                                   TotalAssignmentQuantity = g.Sum(x => x.AssignmentQuantity)
                                               };
            var materialsUnAssigned = from r in roleInputData
                                      join a in calcuTotalAssignmentQuantity
                                           on r.ProductionStepLinkDataId equals a.ProductionStepLinkDataId into assignMap
                                      from m in assignMap.DefaultIfEmpty()
                                      let AssignmentQuantity = (r.ProductionStepLinkData.Quantity - r.ProductionStepLinkData.OutsourceQuantity.GetValueOrDefault()) - m?.TotalAssignmentQuantity
                                      where AssignmentQuantity is null || AssignmentQuantity > 0
                                      select new ProductionOrderMaterialsModel
                                      {
                                          AssignmentQuantity = !AssignmentQuantity.HasValue ? r.ProductionStepLinkData.Quantity - r.ProductionStepLinkData.OutsourceQuantity.GetValueOrDefault() : AssignmentQuantity,
                                          ProductId = r.ProductionStepLinkData.ObjectId,
                                          ProductionStepId = r.ProductionStepId,
                                          ProductionStepTitle = r.ProductionStep.Step?.StepName,
                                          ProductionStepLinkDataId = r.ProductionStepLinkDataId,
                                          Quantity = r.ProductionStepLinkData.Quantity - r.ProductionStepLinkData.OutsourceQuantity.GetValueOrDefault(),
                                          RateQuantity = !  AssignmentQuantity.HasValue ? 1 : (AssignmentQuantity.GetValueOrDefault() / (r.ProductionStepLinkData.Quantity - r.ProductionStepLinkData.OutsourceQuantity.GetValueOrDefault()))
                                      };

            materialsAssigned.AddRange(materialsUnAssigned);

            return materialsAssigned.OrderBy(x => x.ProductionStepLinkDataId).ThenBy(x => x.DepartmentId.HasValue).ToList();
        }
    }
}
