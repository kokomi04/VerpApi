using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NPOI.POIFS.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Manafacturing.Model.ProductionOrder.Materials;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;

namespace VErp.Services.Manafacturing.Service.ProductionOrder.Implement
{
    public class ProductionOrderMaterialSetService : IProductionOrderMaterialSetService
    {
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly ICustomGenCodeHelperService _customGenCodeHelperService;
        private readonly IProductHelperService _productHelperService;

        public ProductionOrderMaterialSetService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<ProductionOrderMaterialSetService> logger
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

        public async Task<ProductionOrderMaterialInfo> GetProductionOrderMaterialsCalc(long productionOrderId)
        {
            var productionOrder = await _manufacturingDBContext.ProductionOrder.AsNoTracking().Include(x => x.ProductionOrderDetail).FirstOrDefaultAsync(o => o.ProductionOrderId == productionOrderId);
            if (productionOrder == null)
                throw new BadRequestException(ProductOrderErrorCode.ProductOrderNotfound);

            if (productionOrder.IsResetProductionProcess)
                await ResetProductionOrderMaterials(productionOrderId);

            var productQuantity = productionOrder.ProductionOrderDetail.GroupBy(x => x.ProductId).ToDictionary(k => k.Key, v => v.Sum(x => x.Quantity));

            var standardMainMaterials = await GetMainMaterials(productionOrderId);

            var standards = new List<ProductionOrderMaterialGroupStandardModel>()
            {

                new ProductionOrderMaterialGroupStandardModel()
                {
                    ProductMaterialsConsumptionGroupId =0,
                    Materials = standardMainMaterials
                }

            };

            var steps = await _manufacturingDBContext.Step.AsNoTracking().ToListAsync();

            var consumStandardMaterials = (await _productHelperService.GetProductMaterialsConsumptions(productQuantity.Keys.ToArray()))
                .GroupBy(x => x.ProductMaterialsConsumptionGroupId)
                .Select(x => new ProductionOrderMaterialGroupStandardModel()
                {
                    ProductMaterialsConsumptionGroupId = x.Key,
                    Materials = x.GroupBy(g => new { g.MaterialsConsumptionId, g.DepartmentId })
                    .Select(g =>
                    {
                        var f = g.First();
                        var quantity = x.Select(x => (x.Quantity + x.TotalQuantityInheritance) * productQuantity[x.ProductId]).Sum();

                        return new ProductionOrderMaterialStandard
                        {
                            DepartmentId = f.DepartmentId,
                            ProductId = f.MaterialsConsumptionId,
                            StepId = f.StepId,
                            StepName = steps.FirstOrDefault(s => s.StepId == f.StepId)?.StepName,
                            ProductionStepId = null,
                            ProductionStepTitle = null,
                            ProductionStepLinkDataId = null,
                            Quantity = quantity,
                            RateQuantity = 1
                        };
                    }).ToList()
                });

            standards.AddRange(consumStandardMaterials);

            var materialSets = await _manufacturingDBContext.ProductionOrderMaterialSet.Include(s => s.ProductionOrderMaterialSetConsumptionGroup).Include(s => s.ProductionOrderMaterials).ToListAsync();
            var calcs = materialSets.Select(s => GetMaterialSetModel(s, standards)).ToList();

            return new ProductionOrderMaterialInfo
            {
                IsReset = productionOrder.IsResetProductionProcess,
                Standards = standards,
                Calcs = calcs
            };
        }

        public async Task<bool> UpdateAll(long productionOrderId, IList<ProductionOrderMaterialSetModel> model)
        {
            using var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();

            var productionOrder = await _manufacturingDBContext.ProductionOrder.FirstOrDefaultAsync(o => o.ProductionOrderId == productionOrderId);
            if (productionOrder == null)
                throw new BadRequestException(ProductOrderErrorCode.ProductOrderNotfound);

            foreach (var calc in model)
            {
                if (calc.ProductionOrderMaterialSetId > 0)
                {
                    await UpdateMaterialSet(productionOrderId, calc.ProductionOrderMaterialSetId, calc);
                }
                else
                {
                    await CreateMaterialSet(productionOrderId, calc);
                }
            }
        
            productionOrder.IsResetProductionProcess = false;

            await _manufacturingDBContext.SaveChangesAsync();

            await trans.CommitAsync();
            return true;
        }

        private ProductionOrderMaterialSetModel GetMaterialSetModel(ProductionOrderMaterialSet set, IList<ProductionOrderMaterialGroupStandardModel> standards)
        {
            var groupIds = set.ProductionOrderMaterialSetConsumptionGroup.Select(g => g.ProductMaterialsConsumptionGroupId).ToList();
            var lst = standards.Where(s => groupIds.Contains(s.ProductMaterialsConsumptionGroupId))
                .SelectMany(s => s.Materials)
                .GroupBy(m => new { m.ProductId, m.DepartmentId })
                .Select(g =>
                {
                    return new ProductionOrderMaterialAssign()
                    {
                        ProductId = g.Key.ProductId,
                        Quantity = g.Sum(m => m.Quantity),
                        DepartmentId = g.Key.DepartmentId,
                        ProductionStepId = g.Select(s => s.ProductionStepId).Distinct().Count() == 1 ? g.First().ProductionStepId : null,
                        ProductionStepTitle = g.Select(s => s.ProductionStepTitle).Distinct().Count() == 1 ? g.First().ProductionStepTitle : null,
                        StepId = g.Select(s => s.StepId).Distinct().Count() == 1 ? g.First().StepId : null,
                        StepName = g.Select(s => s.StepName).Distinct().Count() == 1 ? g.First().StepName : null,
                        ProductionStepLinkDataId = g.Select(s => s.ProductionStepLinkDataId).Distinct().Count() == 1 ? g.First().ProductionStepLinkDataId : null,
                        RateQuantity = g.Select(s => s.RateQuantity).Distinct().Count() == 1 ? g.First().RateQuantity : g.Average(f => f.RateQuantity),
                    };
                })
                .ToList();

            var replacements = new List<ProductionOrderMaterialAssign>();
            foreach (var item in lst)
            {
                var m = set.ProductionOrderMaterials.FirstOrDefault(x => x.DepartmentId.GetValueOrDefault() == item.DepartmentId.GetValueOrDefault()
                            && x.ProductionStepLinkDataId == item.ProductionStepLinkDataId
                            && x.IsReplacement == false);
                if (m == null) continue;

                item.AssignmentQuantity = m.Quantity;
                item.ProductionOrderMaterialsId = m.ProductionOrderMaterialsId;

                var child = set.ProductionOrderMaterials.Where(x => x.ParentId.HasValue && x.ParentId == m.ProductionOrderMaterialsId && x.IsReplacement == true)
                    .Select(r => new ProductionOrderMaterialAssign
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
                        ProductionOrderMaterialsId = r.ProductionOrderMaterialsId,
                        //InventoryRequirementStatusId = r.InventoryRequirementStatusId,
                        DepartmentId = r.DepartmentId,
                        ConversionRate = r.ConversionRate,
                    });
                replacements.AddRange(child);
            }
            lst.AddRange(replacements);
            return new ProductionOrderMaterialSetModel()
            {
                ProductionOrderMaterialSetId = set.ProductionOrderMaterialSetId,
                //EnumInventoryRequirementStatus InventoryRequirementStatusId { get; set; }
                Title = set.Title,
                ProductMaterialsConsumptionGroupIds = groupIds,
                //ProductionOrderMaterialSetTypeId = set.ProductionOrderMaterialSetTypeId,
                CreatedByUserId = set.CreatedByUserId,
                UpdatedByUserId = set.UpdatedByUserId,
                Materials = lst.OrderBy(m => m.ProductId).ThenBy(m => m.DepartmentId).ToList()
            };
        }

        private async Task<List<ProductionOrderMaterialStandard>> GetMainMaterials(long productionOrderId)
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
                .Include(x => x.Step)
                .ToListAsync();

            var productionAssignments = (await _manufacturingDBContext.ProductionAssignment.AsNoTracking()
                                            .Where(x => roleInputData.Select(x => x.ProductionStepId).Contains(x.ProductionStepId))
                                            .Include(x => x.ProductionStepLinkData)
                                            .Select(x => new
                                            {
                                                x.ProductionStepId,
                                                x.DepartmentId,
                                                x.AssignmentQuantity,
                                                x.ProductionStepLinkData.Quantity
                                            }).
                                            ToListAsync())
                                            .Select(x => new
                                            {
                                                x.ProductionStepId,
                                                x.DepartmentId,
                                                RateQuantity = x.Quantity > 0 ? Math.Round(x.AssignmentQuantity / x.Quantity, 5) : 0
                                            })
                                            .ToArray();

            var materialsAssigned = (from r in roleInputData
                                     join a in productionAssignments
                                         on r.ProductionStepId equals a.ProductionStepId
                                     join s in productionSteps
                                        on r.ProductionStep.ParentId equals s.ProductionStepId
                                     where r.ProductionStepLinkData.LinkDataObjectTypeId == (int)EnumProductionStepLinkDataObjectType.Product
                                     select new ProductionOrderMaterialStandard
                                     {
                                         DepartmentId = a.DepartmentId,
                                         ProductId = r.ProductionStepLinkData.LinkDataObjectId,
                                         StepId = s.Step?.StepId,
                                         StepName = s.Step?.StepName,
                                         ProductionStepId = r.ProductionStepId,
                                         ProductionStepTitle = string.Concat(s.Step?.StepName, $@" ({r.ProductionStep.Title})"),
                                         ProductionStepLinkDataId = r.ProductionStepLinkDataId,
                                         Quantity = a.RateQuantity * r.ProductionStepLinkData.Quantity,
                                         RateQuantity = a.RateQuantity
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
                                                   TotalAssignmentQuantity = g.Sum(x => x.Quantity)
                                               };
            var materialsUnAssigned = from r in roleInputData
                                      join s in productionSteps
                                       on r.ProductionStep.ParentId equals s.ProductionStepId
                                      join a in calcuTotalAssignmentQuantity
                                           on r.ProductionStepLinkDataId equals a.ProductionStepLinkDataId into assignMap
                                      from m in assignMap.DefaultIfEmpty()
                                      let AssignmentQuantity = r.ProductionStepLinkData.Quantity - m?.TotalAssignmentQuantity
                                      where AssignmentQuantity is null || AssignmentQuantity > 0 && r.ProductionStepLinkData.LinkDataObjectTypeId == (int)EnumProductionStepLinkDataObjectType.Product
                                      select new ProductionOrderMaterialStandard
                                      {
                                          DepartmentId = null,
                                          ProductId = r.ProductionStepLinkData.LinkDataObjectId,
                                          StepId = s.Step?.StepId,
                                          StepName = s.Step?.StepName,
                                          ProductionStepId = r.ProductionStepId,
                                          ProductionStepTitle = string.Concat(s.Step?.StepName, $@" ({r.ProductionStep.Title})"),
                                          ProductionStepLinkDataId = r.ProductionStepLinkDataId,
                                          Quantity = AssignmentQuantity.HasValue ? AssignmentQuantity.GetValueOrDefault() : r.ProductionStepLinkData.Quantity,
                                          RateQuantity = !AssignmentQuantity.HasValue ? 1 : (AssignmentQuantity.GetValueOrDefault() / r.ProductionStepLinkData.Quantity)
                                      };

            materialsAssigned.AddRange(materialsUnAssigned);

            return materialsAssigned;
        }

        private async Task<long> CreateMaterialSet(long productionOrderId, ProductionOrderMaterialSetModel model)
        {
            ValidateSetModel(model);

            var info = new ProductionOrderMaterialSet()
            {
                Title = model.Title,
                ProductionOrderId = productionOrderId,
                IsMultipleConsumptionGroupId = model.ProductMaterialsConsumptionGroupIds?.Count > 1, // model.ProductionOrderMaterialSetTypeId,                
            };
            await _manufacturingDBContext.ProductionOrderMaterialSet.AddAsync(info);
            await _manufacturingDBContext.SaveChangesAsync();

            await UpdateMaterialSetData(info, model);


            return info.ProductionOrderMaterialSetId;
        }


        private async Task<bool> UpdateMaterialSet(long productionOrderId, long productionOrderMaterialSetId, ProductionOrderMaterialSetModel model)
        {


            ValidateSetModel(model);

            var info = await _manufacturingDBContext.ProductionOrderMaterialSet.FirstOrDefaultAsync(s => s.ProductionOrderId == productionOrderId && s.ProductionOrderMaterialSetId == productionOrderMaterialSetId);
            if (info == null)
                throw new BadRequestException("Không tìm thấy bảng tính");
            info.Title = model.Title;

            await UpdateMaterialSetData(info, model);

            return true;
        }

        private void ValidateSetModel(ProductionOrderMaterialSetModel model)
        {
            if (model.ProductMaterialsConsumptionGroupIds == null || model.ProductMaterialsConsumptionGroupIds.Count == 0)
            {
                throw new BadRequestException("Vui lòng chọn nhóm vật tư tiêu hao");
            }
        }

        private async Task<bool> UpdateMaterialSetData(ProductionOrderMaterialSet info, ProductionOrderMaterialSetModel model)
        {

            _manufacturingDBContext.ProductionOrderMaterialSetConsumptionGroup.RemoveRange(_manufacturingDBContext.ProductionOrderMaterialSetConsumptionGroup.Where(g => g.ProductionOrderMaterialSetId == info.ProductionOrderMaterialSetId));

            var groups = model.ProductMaterialsConsumptionGroupIds.Select(groupId => new ProductionOrderMaterialSetConsumptionGroup()
            {
                ProductionOrderMaterialSetId = info.ProductionOrderMaterialSetId,
                ProductMaterialsConsumptionGroupId = groupId
            });

            await _manufacturingDBContext.ProductionOrderMaterialSetConsumptionGroup.AddRangeAsync(groups);

            _manufacturingDBContext.ProductionOrderMaterials.RemoveRange(_manufacturingDBContext.ProductionOrderMaterials.Where(g => g.ProductionOrderMaterialSetId == info.ProductionOrderMaterialSetId));


            var allMaterials = model.Materials
                .Select(m => new ProductionOrderMaterials
                {
                    ProductionOrderMaterialsId = 0,
                    ProductionOrderId = info.ProductionOrderId,
                    ProductionStepLinkDataId = m.ProductionStepLinkDataId,
                    ProductId = m.ProductId,
                    ConversionRate = m.ConversionRate,
                    Quantity = m.Quantity,
                    UnitId = 0,
                    StepId = m.StepId,
                    DepartmentId = m.DepartmentId,
                    //InventoryRequirementStatusId =,
                    ParentId = m.ParentId,
                    IsReplacement = m.IsReplacement,
                    ProductionOrderMaterialSetId = info.ProductionOrderMaterialSetId

                }).ToList();

            var originMaterials = allMaterials.Where(m => !m.IsReplacement);
            await _manufacturingDBContext.ProductionOrderMaterials.AddRangeAsync(originMaterials);
            await _manufacturingDBContext.SaveChangesAsync();

            foreach (var item in allMaterials.Where(m => m.IsReplacement).ToList())
            {
                var parent = originMaterials.Where(x => !x.IsReplacement)
                    .FirstOrDefault(x => x.DepartmentId == item.DepartmentId && x.ProductionStepLinkDataId == item.ProductionStepLinkDataId);
                if (parent == null)
                    throw new BadRequestException(ProductOrderErrorCode.NotFoundMaterials, "Vật liệu thay thế không được gắn với vật liệu được thay thế");

                item.ParentId = parent.ProductionOrderMaterialsId;
            }
            await _manufacturingDBContext.ProductionOrderMaterials.AddRangeAsync(allMaterials.Where(m => m.IsReplacement).ToList());
            await _manufacturingDBContext.SaveChangesAsync();

            return true;
        }


        private async Task<bool> ResetProductionOrderMaterials(long productionOrderId)
        {
            using (var trans = await _manufacturingDBContext.Database.BeginTransactionAsync())
            {
                var sets = await _manufacturingDBContext.ProductionOrderMaterialSet.Include(s => s.ProductionOrderMaterialSetConsumptionGroup).ToListAsync();
                var groups = sets.SelectMany(s => s.ProductionOrderMaterialSetConsumptionGroup).ToList();

                _manufacturingDBContext.ProductionOrderMaterialSetConsumptionGroup.RemoveRange(groups);

                sets.ForEach(s => s.IsDeleted = true);

                var materialsDb = await _manufacturingDBContext.ProductionOrderMaterials
                   .Where(x => x.ProductionOrderId == productionOrderId)
                   .ToListAsync();

                materialsDb.ForEach(x => x.IsDeleted = true);

                await _manufacturingDBContext.SaveChangesAsync();

                await trans.CommitAsync();
            }
            return true;
        }
    }
}
