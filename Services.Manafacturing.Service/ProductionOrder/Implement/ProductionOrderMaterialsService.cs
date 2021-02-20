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
using VErp.Services.Manafacturing.Model.ProductionOrder.Materials;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;

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

        public async Task<IList<ProductionOrderMaterialsCalc>> GetProductionOrderMaterialsCalc(long productionOrderId)
        {
            var roles = await _manufacturingDBContext.ProductionStepLinkDataRole.AsNoTracking()
                .Include(x => x.ProductionStep).ThenInclude(s => s.Step)
                .Include(x => x.ProductionStepLinkData)
                .Where(x => x.ProductionStep.ContainerId == productionOrderId && x.ProductionStep.ContainerTypeId == (int)EnumContainerType.ProductionOrder)
                .ToListAsync();

            var roleInputData = roles.GroupBy(x => x.ProductionStepLinkDataId)
                .Where(x => x.Count() == 1)
                .Select(x => x.First())
                .Where(x => x.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Input
                        && (x.ProductionStepLinkData.ProductionStepLinkDataTypeId == (int)EnumProductionStepLinkDataType.None
                            || x.ProductionStepLinkData.ProductionStepLinkDataTypeId == (int)EnumProductionStepLinkDataType.Others))
                .ToList();

            var materialsAssigned = (from r in roleInputData
                                     join a in _manufacturingDBContext.ProductionAssignment
                                         on r.ProductionStepLinkDataId equals a.ProductionStepLinkDataId
                                     select new ProductionOrderMaterialsCalc
                                     {
                                         AssignmentQuantity = a.AssignmentQuantity,
                                         DepartmentId = a.DepartmentId,
                                         ProductId = r.ProductionStepLinkData.ObjectId,
                                         ProductionStepId = r.ProductionStepId,
                                         ProductionStepTitle = r.ProductionStep.Step?.StepName,
                                         ProductionStepLinkDataId = r.ProductionStepLinkDataId,
                                         Quantity = r.ProductionStepLinkData.Quantity - r.ProductionStepLinkData.OutsourceQuantity.GetValueOrDefault(),
                                         RateQuantity = a.AssignmentQuantity / (r.ProductionStepLinkData.Quantity - r.ProductionStepLinkData.OutsourceQuantity.GetValueOrDefault())
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
                                      select new ProductionOrderMaterialsCalc
                                      {
                                          AssignmentQuantity = !AssignmentQuantity.HasValue ? r.ProductionStepLinkData.Quantity - r.ProductionStepLinkData.OutsourceQuantity.GetValueOrDefault() : AssignmentQuantity,
                                          ProductId = r.ProductionStepLinkData.ObjectId,
                                          ProductionStepId = r.ProductionStepId,
                                          ProductionStepTitle = r.ProductionStep.Step?.StepName,
                                          ProductionStepLinkDataId = r.ProductionStepLinkDataId,
                                          Quantity = r.ProductionStepLinkData.Quantity - r.ProductionStepLinkData.OutsourceQuantity.GetValueOrDefault(),
                                          RateQuantity = !AssignmentQuantity.HasValue ? 1 : (AssignmentQuantity.GetValueOrDefault() / (r.ProductionStepLinkData.Quantity - r.ProductionStepLinkData.OutsourceQuantity.GetValueOrDefault()))
                                      };

            materialsAssigned.AddRange(materialsUnAssigned);

            var materialsDb = await GetProductionOrderMaterials(productionOrderId);

            var materialsReplacement = new List<ProductionOrderMaterialsCalc>();
            foreach (var item in materialsAssigned)
            {
                var m = materialsDb.FirstOrDefault(x => x.DepartmentId.GetValueOrDefault() == item.DepartmentId.GetValueOrDefault()
                            && x.ProductionStepLinkDataId == item.ProductionStepLinkDataId
                            && x.IsReplacement == false);
                if (m == null) continue;

                item.AssignmentQuantity = m.Quantity;
                item.ProductionOrderMaterialsId = m.ProductionOrderMaterialsId;

                var child = materialsDb.Where(x => x.ParentId.HasValue && x.ParentId == m.ProductionOrderMaterialsId && x.IsReplacement == true)
                    .Select(r => new ProductionOrderMaterialsCalc
                    {
                        AssignmentQuantity = r.Quantity,
                        ProductId = r.ProductId,
                        ProductionStepId = item.ProductionStepId,
                        ProductionStepTitle = item.ProductionStepTitle,
                        ProductionStepLinkDataId = r.ProductionStepLinkDataId,
                        Quantity = decimal.Zero,
                        RateQuantity = 1,
                        IsReplacement = r.IsReplacement,
                        ParentId = r.ParentId,
                        ProductionOrderMaterialsId = r.ProductionOrderMaterialsId
                    });
                materialsReplacement.AddRange(child);
            }

            materialsAssigned.AddRange(materialsReplacement);

            return materialsAssigned.OrderBy(x => x.ProductionStepLinkDataId).ThenBy(x => x.DepartmentId.HasValue).ToList();
        }

        public async Task<bool> UpdateProductionOrderMaterials(long productionOrderId, IList<ProductionOrderMaterialsInput> materials)
        {
            var materialsDb = await _manufacturingDBContext.ProductionOrderMaterials
                .Where(x => x.ProductionOrderId == productionOrderId)
                .ToListAsync();
            var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                var newMaterials = materials.AsQueryable().Where(x => x.ProductionOrderMaterialsId == 0)
                .ProjectTo<ProductionOrderMaterials>(_mapper.ConfigurationProvider)
                .ToArray();

                var newMaterialsReplacement = materials.AsQueryable().SelectMany(x=>x.materialsReplacement).Where(x => x.ProductionOrderMaterialsId == 0 && x.Quantity > 0)
                .ProjectTo<ProductionOrderMaterials>(_mapper.ConfigurationProvider)
                .ToArray();

                foreach (var m in materialsDb)
                {
                    var s = materials.FirstOrDefault(x => x.ProductionOrderMaterialsId == m.ProductionOrderMaterialsId);
                    if (s == null)
                    {
                        s = materials.SelectMany(x => x.materialsReplacement).Where(x => x.Quantity > 0).FirstOrDefault(x => x.ProductionOrderMaterialsId == m.ProductionOrderMaterialsId);
                    }
                    if (s == null)
                        m.IsDeleted = true;
                    else
                        _mapper.Map(s, m);
                }


                await _manufacturingDBContext.ProductionOrderMaterials.AddRangeAsync(newMaterials);
                await _manufacturingDBContext.SaveChangesAsync();
                materialsDb.AddRange(newMaterials);
                foreach (var item in newMaterialsReplacement)
                {
                    var parent = materialsDb.Where(x => x.IsReplacement == false && x.IsDeleted == false).First(x => x.DepartmentId == item.DepartmentId && x.ProductionStepLinkDataId == item.ProductionStepLinkDataId);
                    if(parent == null)
                        throw new BadRequestException(ProductOrderErrorCode.NotFoundMaterials, "Vật liệu thay thế không được gắn với vật liệu được thay thế");

                    item.ParentId = parent.ProductionOrderMaterialsId;
                }
                await _manufacturingDBContext.ProductionOrderMaterials.AddRangeAsync(newMaterialsReplacement);

                await _manufacturingDBContext.SaveChangesAsync();

                await trans.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await trans.TryRollbackTransactionAsync();
                _logger.LogError(ex, "UpdateProductionOrderMaterials");
                throw;
            }

        }

        public async Task<IList<ProductionOrderMaterialsOutput>> GetProductionOrderMaterials(long productionOrderId)
        {
            var materialsDb = await _manufacturingDBContext.ProductionOrderMaterials.AsNoTracking()
               .Where(x => x.ProductionOrderId == productionOrderId)
               .ProjectTo<ProductionOrderMaterialsOutput>(_mapper.ConfigurationProvider)
               .ToListAsync();

            return materialsDb;
        }
    }
}
