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
using VErp.Commons.Enums.Manafacturing;

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

        public async Task<ProductionOrderMaterialsModel> GetProductionOrderMaterialsCalc(long productionOrderId)
        {
            var productionOrder = await _manufacturingDBContext.ProductionOrder.AsNoTracking().Include(x => x.ProductionOrderDetail).FirstOrDefaultAsync(o => o.ProductionOrderId == productionOrderId);
            if (productionOrder == null)
                throw new BadRequestException(ProductOrderErrorCode.ProductOrderNotfound);

            if (productionOrder.IsResetProductionProcess)
                await ResetProductionOrderMaterials(productionOrderId);

            var productMap = productionOrder.ProductionOrderDetail.ToDictionary(k => k.ProductId, v => v.Quantity);

            var materialsMain = await GetProductionOrderMaterialsMainCalc(productionOrderId);
            var materialsConsump = await GetProductionOrderMaterialsConsumptionCalc(productionOrderId, productMap);

            var materials = new List<ProductionOrderMaterialsCalc>();
            materials.AddRange(materialsMain);
            materials.AddRange(materialsConsump);

           
            return new ProductionOrderMaterialsModel
            {
                IsReset = productionOrder.IsResetProductionProcess,
                materials = materials.OrderBy(x => x.ProductId).ThenBy(x => x.DepartmentId.HasValue).ToList(),
            };
        }

        private async Task<IList<ProductionOrderMaterialsCalc>> GetProductionOrderMaterialsMainCalc(long productionOrderId)
        {
            var roles = await _manufacturingDBContext.ProductionStepLinkDataRole.AsNoTracking()
                            .Include(x => x.ProductionStep).ThenInclude(s => s.Step)
                            .Include(x => x.ProductionStepLinkData)
                            .Where(x => x.ProductionStep.ContainerId == productionOrderId && x.ProductionStep.ContainerTypeId == (int)EnumContainerType.ProductionOrder)
                            .ToListAsync();
            if (roles.Count == 0)
                throw new BadRequestException(ProductOrderErrorCode.ProductOrderNotfound, "Quy trình sản xuất chưa được thiết lập");

            var roleInputData = roles.GroupBy(x => x.ProductionStepLinkDataId)
                .Where(x => x.Count() == 1)
                .Select(x => x.First())
                .Where(x => x.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Input
                        && (x.ProductionStepLinkData.ProductionStepLinkDataTypeId == (int)EnumProductionStepLinkDataType.None
                            || x.ProductionStepLinkData.ProductionStepLinkDataTypeId == (int)EnumProductionStepLinkDataType.Others))
                .ToList();

            var productionSteps = await _manufacturingDBContext.ProductionStep.AsNoTracking()
                .Where(x => x.IsGroup == true && x.ContainerTypeId == (int)EnumContainerType.ProductionOrder && x.ContainerId == productionOrderId)
                .Include(x=>x.Step)
                .ToListAsync();

            var productionAssignments = _manufacturingDBContext.ProductionAssignment.AsNoTracking()
                                            .Where(x => roleInputData.Select(x => x.ProductionStepId).Contains(x.ProductionStepId))
                                            .Include(x => x.ProductionStepLinkData)
                                            .Select(x => new
                                            {
                                                x.ProductionStepId,
                                                x.DepartmentId,
                                                RateQuantity = Math.Round(x.AssignmentQuantity / (x.ProductionStepLinkData.Quantity - x.ProductionStepLinkData.OutsourceQuantity.GetValueOrDefault()), 5)
                                            }).ToArray();

            var materialsAssigned = (from r in roleInputData
                                     join a in productionAssignments
                                         on r.ProductionStepId equals a.ProductionStepId
                                     join s in productionSteps
                                        on r.ProductionStep.ParentId equals s.ProductionStepId
                                     where r.ProductionStepLinkData.ObjectTypeId == (int)EnumProductionStepLinkDataObjectType.Product
                                     select new ProductionOrderMaterialsCalc
                                     {
                                         AssignmentQuantity = a.RateQuantity * (r.ProductionStepLinkData.Quantity - r.ProductionStepLinkData.OutsourceQuantity.GetValueOrDefault()),
                                         DepartmentId = a.DepartmentId,
                                         ProductId = r.ProductionStepLinkData.ObjectId,
                                         ProductionStepId = r.ProductionStepId,
                                         ProductionStepTitle = string.Concat(s.Step?.StepName, $@" ({r.ProductionStep.Title})"),
                                         ProductionStepLinkDataId = r.ProductionStepLinkDataId,
                                         Quantity = a.RateQuantity * (r.ProductionStepLinkData.Quantity - r.ProductionStepLinkData.OutsourceQuantity.GetValueOrDefault()),
                                         RateQuantity = a.RateQuantity,
                                         InventoryRequirementStatusId = EnumProductionOrderMaterials.EnumInventoryRequirementStatus.NotCreateYet,
                                         ConversionRate = 1,
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
                                      join s in productionSteps
                                       on r.ProductionStep.ParentId equals s.ProductionStepId
                                      join a in calcuTotalAssignmentQuantity
                                           on r.ProductionStepLinkDataId equals a.ProductionStepLinkDataId into assignMap
                                      from m in assignMap.DefaultIfEmpty()
                                      let AssignmentQuantity = (r.ProductionStepLinkData.Quantity - r.ProductionStepLinkData.OutsourceQuantity.GetValueOrDefault()) - m?.TotalAssignmentQuantity
                                      where AssignmentQuantity is null || AssignmentQuantity > 0 && r.ProductionStepLinkData.ObjectTypeId == (int)EnumProductionStepLinkDataObjectType.Product
                                      select new ProductionOrderMaterialsCalc
                                      {
                                          AssignmentQuantity = !AssignmentQuantity.HasValue ? r.ProductionStepLinkData.Quantity - r.ProductionStepLinkData.OutsourceQuantity.GetValueOrDefault() : AssignmentQuantity,
                                          ProductId = r.ProductionStepLinkData.ObjectId,
                                          ProductionStepId = r.ProductionStepId,
                                          ProductionStepTitle = string.Concat(s.Step?.StepName, $@" ({r.ProductionStep.Title})"),
                                          ProductionStepLinkDataId = r.ProductionStepLinkDataId,
                                          Quantity = AssignmentQuantity.HasValue ? AssignmentQuantity.GetValueOrDefault() : (r.ProductionStepLinkData.Quantity - r.ProductionStepLinkData.OutsourceQuantity.GetValueOrDefault()),
                                          RateQuantity = !AssignmentQuantity.HasValue ? 1 : (AssignmentQuantity.GetValueOrDefault() / (r.ProductionStepLinkData.Quantity - r.ProductionStepLinkData.OutsourceQuantity.GetValueOrDefault())),
                                          InventoryRequirementStatusId = EnumProductionOrderMaterials.EnumInventoryRequirementStatus.NotCreateYet,
                                          ConversionRate = 1,
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
                item.PrivateKeyTable = m.ProductionOrderMaterialsId;
                item.InventoryRequirementStatusId = m.InventoryRequirementStatusId;

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
                        PrivateKeyTable = r.ProductionOrderMaterialsId,
                        InventoryRequirementStatusId = r.InventoryRequirementStatusId,
                        DepartmentId = r.DepartmentId,
                        ConversionRate = r.ConversionRate,
                    });
                materialsReplacement.AddRange(child);
            }

            materialsAssigned.AddRange(materialsReplacement);
            return materialsAssigned;
        }

        public async Task<IList<ProductionOrderMaterialsCalc>> GetProductionOrderMaterialsConsumptionCalc(long productionOrderId, Dictionary<int, decimal?> productMap)
        {
            var materials = (await _productHelperService.GetProductMaterialsConsumptions(productMap.Keys.ToArray()))
                .GroupBy(x => new { x.ProductMaterialsConsumptionGroupId, x.MaterialsConsumptionId, x.DepartmentId })
                .Select(x =>
                {
                    var f = x.First();
                    var quantity = x.Select(x => (x.Quantity + x.TotalQuantityInheritance) * productMap[x.ProductId]).Sum();

                    return new ProductionOrderMaterialsCalc
                    {
                        DepartmentId = f.DepartmentId,
                        ProductId = f.MaterialsConsumptionId,
                        Quantity = quantity.GetValueOrDefault(),
                        InventoryRequirementStatusId = EnumProductionOrderMaterials.EnumInventoryRequirementStatus.NotCreateYet,
                        ConversionRate = 1,
                        AssignmentQuantity = quantity,
                        ProductMaterialsConsumptionGroupId = f.ProductMaterialsConsumptionGroupId,
                        RateQuantity = 1
                    };
                }).ToList();

            var materialsDb = await GetProductionOrderMaterialsConsump(productionOrderId);
            var materialsReplacement = new List<ProductionOrderMaterialsCalc>();
            foreach (var item in materials)
            {
                var m = materialsDb.FirstOrDefault(x => x.DepartmentId.GetValueOrDefault() == item.DepartmentId.GetValueOrDefault()
                            && x.ProductId == item.ProductId && x.ProductMaterialsConsumptionGroupId == item.ProductMaterialsConsumptionGroupId
                            && x.IsReplacement == false);
                if (m == null) continue;

                item.PrivateKeyTable = m.ProductionOrderMaterialsConsumptionId;
                item.InventoryRequirementStatusId = m.InventoryRequirementStatusId;
                item.AssignmentQuantity = m.Quantity;

                var child = materialsDb.Where(x => x.ParentId.HasValue && x.ParentId == m.ProductionOrderMaterialsConsumptionId && x.IsReplacement == true)
                    .Select(r => new ProductionOrderMaterialsCalc
                    {
                        AssignmentQuantity = r.Quantity,
                        ProductId = r.ProductId,
                        Quantity = decimal.Zero,
                        IsReplacement = r.IsReplacement,
                        ParentId = r.ParentId,
                        InventoryRequirementStatusId = r.InventoryRequirementStatusId,
                        DepartmentId = r.DepartmentId,
                        ConversionRate = r.ConversionRate,
                        ProductMaterialsConsumptionGroupId = r.ProductMaterialsConsumptionGroupId,
                        RateQuantity = 1,
                        PrivateKeyTable = r.ProductionOrderMaterialsConsumptionId,
                    });
                materialsReplacement.AddRange(child);
            }

            materials.AddRange(materialsReplacement);
            return materials;
        }

        public async Task<bool> UpdateProductionOrderMaterials(long productionOrderId, IList<ProductionOrderMaterialsInput> materials)
        {
            var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                var productionOrder = await _manufacturingDBContext.ProductionOrder.FirstOrDefaultAsync(o => o.ProductionOrderId == productionOrderId);
                if (productionOrder == null)
                    throw new BadRequestException(ProductOrderErrorCode.ProductOrderNotfound);

                await UpdateProductionOrderMaterialsMain(productionOrderId, materials.Where(x=>x.ProductMaterialsConsumptionGroupId == 0).ToList());
                await UpdateProductionOrderMaterialsConsump(productionOrderId, materials.Where(x => x.ProductMaterialsConsumptionGroupId > 0).ToList());

                productionOrder.IsResetProductionProcess = false;

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

        private async Task UpdateProductionOrderMaterialsConsump(long productionOrderId, IList<ProductionOrderMaterialsInput> materials)
        {
            var materialsDb = await _manufacturingDBContext.ProductionOrderMaterialsConsumption
                                .Where(x => x.ProductionOrderId == productionOrderId)
                                .ToListAsync();

            var newMaterials = materials.AsQueryable().Where(x => x.ProductionOrderMaterialsId == 0)
            .Select(x=> new ProductionOrderMaterialsConsumption
            {
                ProductMaterialsConsumptionGroupId = x.ProductMaterialsConsumptionGroupId,
                Quantity = x.Quantity,
                UnitId = x.UnitId,
                ProductionOrderId = x.ProductionOrderId,
                ParentId = x.ParentId,
                ProductId = x.ProductId,
                ConversionRate = x.ConversionRate,
                DepartmentId = x.DepartmentId,
                IsReplacement = x.IsReplacement,
                InventoryRequirementStatusId = (int) x.InventoryRequirementStatusId,
                ProductionOrderMaterialsConsumptionId = x.ProductionOrderMaterialsId
                
            })
            .ToArray();

            var newMaterialsReplacement = materials.AsQueryable().SelectMany(x => x.materialsReplacement).Where(x => x.ProductionOrderMaterialsId == 0)
            .Select(x => new ProductionOrderMaterialsConsumption
            {
                ProductMaterialsConsumptionGroupId = x.ProductMaterialsConsumptionGroupId,
                Quantity = x.Quantity,
                UnitId = x.UnitId,
                ProductionOrderId = x.ProductionOrderId,
                ParentId = x.ParentId,
                ProductId = x.ProductId,
                ConversionRate = x.ConversionRate,
                DepartmentId = x.DepartmentId,
                IsReplacement = x.IsReplacement,
                InventoryRequirementStatusId = (int)x.InventoryRequirementStatusId,
                ProductionOrderMaterialsConsumptionId = x.ProductionOrderMaterialsId
            })
            .ToArray();

            foreach (var m in materialsDb)
            {
                var s = materials.FirstOrDefault(x => x.ProductionOrderMaterialsId == m.ProductionOrderMaterialsConsumptionId);
                if (s == null)
                {
                    s = materials.SelectMany(x => x.materialsReplacement).FirstOrDefault(x => x.ProductionOrderMaterialsId == m.ProductionOrderMaterialsConsumptionId);
                }
                if (s == null)
                    m.IsDeleted = true;
                else
                {
                    m.ProductMaterialsConsumptionGroupId = s.ProductMaterialsConsumptionGroupId;
                    m.Quantity = s.Quantity;
                    m.UnitId = s.UnitId;
                    m.ProductionOrderId = s.ProductionOrderId;
                    m.ParentId = s.ParentId;
                    m.ProductId = s.ProductId;
                    m.ConversionRate = s.ConversionRate;
                    m.DepartmentId = s.DepartmentId;
                    m.IsReplacement = s.IsReplacement;
                    m.InventoryRequirementStatusId = (int)s.InventoryRequirementStatusId;
                }
            }


            await _manufacturingDBContext.ProductionOrderMaterialsConsumption.AddRangeAsync(newMaterials);
            await _manufacturingDBContext.SaveChangesAsync();
            materialsDb.AddRange(newMaterials);
            foreach (var item in newMaterialsReplacement)
            {
                var parent = materialsDb.Where(x => x.IsReplacement == false && x.IsDeleted == false)
                    .FirstOrDefault(x => x.DepartmentId == item.DepartmentId && x.ProductMaterialsConsumptionGroupId == item.ProductMaterialsConsumptionGroupId);
                if (parent == null)
                    throw new BadRequestException(ProductOrderErrorCode.NotFoundMaterials, "Vật liệu thay thế không được gắn với vật liệu được thay thế");

                item.ParentId = parent.ProductionOrderMaterialsConsumptionId;
            }
            await _manufacturingDBContext.ProductionOrderMaterialsConsumption.AddRangeAsync(newMaterialsReplacement);
            await _manufacturingDBContext.SaveChangesAsync();
        }

        private async Task UpdateProductionOrderMaterialsMain(long productionOrderId, IList<ProductionOrderMaterialsInput> materials)
        {
            var materialsDb = await _manufacturingDBContext.ProductionOrderMaterials
                                .Where(x => x.ProductionOrderId == productionOrderId)
                                .ToListAsync();

            var newMaterials = materials.AsQueryable().Where(x => x.ProductionOrderMaterialsId == 0)
            .ProjectTo<ProductionOrderMaterials>(_mapper.ConfigurationProvider)
            .ToArray();

            var newMaterialsReplacement = materials.AsQueryable().SelectMany(x => x.materialsReplacement).Where(x => x.ProductionOrderMaterialsId == 0)
            .ProjectTo<ProductionOrderMaterials>(_mapper.ConfigurationProvider)
            .ToArray();

            foreach (var m in materialsDb)
            {
                var s = materials.FirstOrDefault(x => x.ProductionOrderMaterialsId == m.ProductionOrderMaterialsId);
                if (s == null)
                {
                    s = materials.SelectMany(x => x.materialsReplacement).FirstOrDefault(x => x.ProductionOrderMaterialsId == m.ProductionOrderMaterialsId);
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
                var parent = materialsDb.Where(x => x.IsReplacement == false && x.IsDeleted == false)
                    .FirstOrDefault(x => x.DepartmentId == item.DepartmentId && x.ProductionStepLinkDataId == item.ProductionStepLinkDataId);
                if (parent == null)
                    throw new BadRequestException(ProductOrderErrorCode.NotFoundMaterials, "Vật liệu thay thế không được gắn với vật liệu được thay thế");

                item.ParentId = parent.ProductionOrderMaterialsId;
            }
            await _manufacturingDBContext.ProductionOrderMaterials.AddRangeAsync(newMaterialsReplacement);
            await _manufacturingDBContext.SaveChangesAsync();
        }

        public async Task<IList<ProductionOrderMaterialsOutput>> GetProductionOrderMaterials(long productionOrderId)
        {
            var materialsDb = await _manufacturingDBContext.ProductionOrderMaterials.AsNoTracking()
               .Where(x => x.ProductionOrderId == productionOrderId)
               .ProjectTo<ProductionOrderMaterialsOutput>(_mapper.ConfigurationProvider)
               .ToListAsync();

            return materialsDb;
        }

        public async Task<IList<ProductionOrderMaterialsConsumptionModel>> GetProductionOrderMaterialsConsump(long productionOrderId)
        {
            var materialsDb = await _manufacturingDBContext.ProductionOrderMaterialsConsumption.AsNoTracking()
               .Where(x => x.ProductionOrderId == productionOrderId)
               .ProjectTo<ProductionOrderMaterialsConsumptionModel>(_mapper.ConfigurationProvider)
               .ToListAsync();

            return materialsDb;
        }

        private async Task<bool> ResetProductionOrderMaterials(long productionOrderId)
        {
            var materialsDb = await _manufacturingDBContext.ProductionOrderMaterials
               .Where(x => x.ProductionOrderId == productionOrderId)
               .ToListAsync();
            materialsDb.ForEach(x => x.IsDeleted = true);

            var materialsConsumpDb = await _manufacturingDBContext.ProductionOrderMaterialsConsumption
               .Where(x => x.ProductionOrderId == productionOrderId)
               .ToListAsync();
            materialsConsumpDb.ForEach(x => x.IsDeleted = true);

            await _manufacturingDBContext.SaveChangesAsync();
            return true;
        }
    }
}
