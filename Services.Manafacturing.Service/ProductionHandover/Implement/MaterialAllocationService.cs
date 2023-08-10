using AutoMapper;
using AutoMapper.QueryableExtensions;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Verp.Resources.Manafacturing.Production.MaterialAllocation;
using Verp.Resources.Master.Config.ActionButton;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface.Manufacturing;
using VErp.Commons.GlobalObject.InternalDataInterface.Stock;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper.Product;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper.QueueHelper;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Manafacturing.Model.ProductionHandover;
using VErp.Services.Manafacturing.Model.ProductionOrder.Materials;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;
using static VErp.Commons.GlobalObject.QueueName.ManufacturingQueueNameConstants;
using ProductionHandoverEntity = VErp.Infrastructure.EF.ManufacturingDB.ProductionHandover;

namespace VErp.Services.Manafacturing.Service.ProductionHandover.Implement
{
    public class MaterialAllocationService : IMaterialAllocationService
    {
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly ObjectActivityLogFacade _objActivityLogFacade;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IProductHelperService _productHelperService;
        private readonly IProductionOrderQueueHelperService _productionOrderQueueHelperService;

        public MaterialAllocationService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<MaterialAllocationService> logger
            , IMapper mapper
            , IProductHelperService productHelperService, IProductionOrderQueueHelperService productionOrderQueueHelperService)
        {
            _manufacturingDBContext = manufacturingDB;
            _objActivityLogFacade = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.MaterialAllocation);
            _logger = logger;
            _mapper = mapper;
            _productHelperService = productHelperService;
            _productionOrderQueueHelperService = productionOrderQueueHelperService;
        }

        public async Task<IList<MaterialAllocationModel>> GetMaterialAllocations(long productionOrderId)
        {
            var allows = await _manufacturingDBContext.ProductionHandover
                .Where(h => h.ProductionOrderId == productionOrderId && (h.FromDepartmentId == 0 || h.ToDepartmentId == 0) && !h.IsAuto)
                .ToListAsync();
            var lst = new List<MaterialAllocationModel>();
            foreach (var item in allows)
            {
                var allowItem = ProductionHandoverToAllowcationModel(item);
                lst.Add(allowItem);
            }
            return lst;
        }

        private MaterialAllocationModel ProductionHandoverToAllowcationModel(ProductionHandoverEntity item)
        {
            return new MaterialAllocationModel
            {
                MaterialAllocationId = item.ProductionHandoverId,
                ProductionOrderId = item.ProductionOrderId,
                InventoryId = item.InventoryId ?? 0,
                InventoryCode = item.InventoryCode,
                ProductId = (int)item.ObjectId,
                DepartmentId = item.FromDepartmentId > 0 ? item.FromDepartmentId : item.ToDepartmentId,
                ProductionStepId = item.FromProductionStepId > 0 ? item.FromProductionStepId : item.ToProductionStepId,
                AllocationQuantity = item.HandoverQuantity,
                SourceProductId = item.InventoryProductId,
                SourceQuantity = item.InventoryQuantity,
                InventoryDetailId = item.InventoryDetailId,
            };
        }

        private ProductionHandoverEntity AllowcationModelToProductionHandover(MaterialAllocationModel item, bool isInvOut)
        {
            return new ProductionHandoverEntity
            {
                ProductionHandoverId = item.MaterialAllocationId,
                ProductionOrderId = item.ProductionOrderId,
                InventoryId = item.InventoryId,
                InventoryCode = item.InventoryCode,
                ObjectId = item.SourceProductId ?? 0,
                ObjectTypeId = (int)EnumProductionStepLinkDataObjectType.Product,
                FromDepartmentId = isInvOut ? 0 : item.DepartmentId,
                ToDepartmentId = isInvOut ? item.DepartmentId : 0,
                FromProductionStepId = isInvOut ? 0 : item.ProductionStepId,
                ToProductionStepId = isInvOut ? item.ProductionStepId : 0,
                HandoverQuantity = item.SourceQuantity ?? 0,
                InventoryProductId = item.ProductId,
                InventoryQuantity = item.AllocationQuantity,
                InventoryDetailId = item.InventoryDetailId,
            };
        }

        public async Task<AllocationModel> UpdateMaterialAllocation(long productionOrderId, AllocationModel data)
        {
            var productionOrderCode = await _manufacturingDBContext.ProductionOrder.Where(o => productionOrderId == o.ProductionOrderId).Select(o => o.ProductionOrderCode).FirstOrDefaultAsync();

            var invDetailIds = data.MaterialAllocations.Select(a => a.InventoryDetailId).ToList();

            var invs = await _manufacturingDBContext.ProductionOrderInventoryConflict.Where(inv => invDetailIds.Contains(inv.InventoryDetailId)).ToListAsync();

            var sumAllowcations = data.MaterialAllocations.GroupBy(a => a.InventoryDetailId)
                    .Select(g => new { InventoryDetailId = g.Key, TotalAllocationQuantity = g.Sum(d => d.AllocationQuantity) })
                    .ToList();

            foreach (var s in sumAllowcations)
            {
                var invDetail = invs.FirstOrDefault(c => c.InventoryDetailId == s.InventoryDetailId);
                if (invDetail == null)
                {
                    throw GeneralCode.InvalidParams.BadRequest("Không tìm thấy phiếu nhập/xuất kho");
                }
                if (invDetail.InventoryQuantity.SubDecimal(s.TotalAllocationQuantity) < 0)
                {
                    throw GeneralCode.InvalidParams.BadRequest("Số lượng phân bổ vượt quá số lượng phiếu kho " + invDetail.InventoryCode);
                }
            }

            using var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                var currentMaterialAllocations = await _manufacturingDBContext.ProductionHandover
                .Where(h => h.ProductionOrderId == productionOrderId && (h.FromDepartmentId == 0 || h.ToDepartmentId == 0) && !h.IsAuto)
                .ToListAsync();
                _manufacturingDBContext.ProductionHandover.RemoveRange(currentMaterialAllocations);


                foreach (var item in data.MaterialAllocations)
                {
                    var currentMaterialAllocation = currentMaterialAllocations.FirstOrDefault(ma => ma.ProductionHandoverId == item.MaterialAllocationId);
                    var invInfo = invs.FirstOrDefault(inv => inv.InventoryId == item.InventoryId);

                    currentMaterialAllocation = AllowcationModelToProductionHandover(item, invInfo?.InventoryTypeId == (int)EnumInventoryType.Output);
                    _manufacturingDBContext.ProductionHandover.Add(currentMaterialAllocation);
                }


                _manufacturingDBContext.SaveChanges();

                var currentIgnoreAllocations = _manufacturingDBContext.IgnoreAllocation
                   .Where(ia => ia.ProductionOrderId == productionOrderId)
                   .ToList();

                _manufacturingDBContext.IgnoreAllocation.RemoveRange(currentIgnoreAllocations);

                foreach (var item in data.IgnoreAllocations)
                {
                    var entity = _mapper.Map<IgnoreAllocation>(item);
                    entity.ProductionOrderId = productionOrderId;
                    _manufacturingDBContext.IgnoreAllocation.Add(entity);
                }

                _manufacturingDBContext.SaveChanges();
                trans.Commit();
                await _objActivityLogFacade.LogBuilder(() => MaterialAllocationActivityLogMessage.Update)
                   .MessageResourceFormatDatas(productionOrderCode)
                   .ObjectId(productionOrderId)
                   .JsonData(data)
                   .CreateLog();

                data.MaterialAllocations = (await _manufacturingDBContext.ProductionHandover
                    .Where(h => h.ProductionOrderId == productionOrderId && (h.FromDepartmentId == 0 || h.ToDepartmentId == 0) && !h.IsAuto)
                    .ToListAsync()
                    ).Select(h => ProductionHandoverToAllowcationModel(h))
                    .ToList();

                data.IgnoreAllocations = await _manufacturingDBContext.IgnoreAllocation
                    .Where(ma => ma.ProductionOrderId == productionOrderId)
                    .ProjectTo<IgnoreAllocationModel>(_mapper.ConfigurationProvider)
                    .ToListAsync();

                await _productionOrderQueueHelperService.ProductionOrderStatiticChanges(productionOrderCode, $"Cập nhật phân bổ vật tư sản xuất");
                return data;
            }
            catch (Exception ex)
            {
                trans.Rollback();
                _logger.LogError(ex, "UpdateMaterialAllocation");
                throw;
            }
        }

        public async Task<IList<IgnoreAllocationModel>> GetIgnoreAllocations(long productionOrderId)
        {
            var ignoreAllocations = await _manufacturingDBContext.IgnoreAllocation
               .Where(ma => ma.ProductionOrderId == productionOrderId)
               .ProjectTo<IgnoreAllocationModel>(_mapper.ConfigurationProvider)
               .ToListAsync();
            return ignoreAllocations;
        }

        public async Task<ConflictHandoverModel> GetConflictHandovers(long productionOrderId)
        {
            var parammeters = new SqlParameter[]
            {
                new SqlParameter("@ProductionOrderId", productionOrderId)
            };

            var resultData = await _manufacturingDBContext.ExecuteDataProcedure("asp_ProductionHandover_GetInventoryRequirementByProductionOrder", parammeters);

            var inventories = resultData.ConvertData<InternalProductionInventoryRequirementModel>()
                .AsQueryable()
                .ProjectTo<ProductionInventoryRequirementModel>(_mapper.ConfigurationProvider)
                .ToList();

            var materialRequirements = await _manufacturingDBContext.ProductionMaterialsRequirementDetail
                .Include(rd => rd.ProductionMaterialsRequirement)
                .Where(rd => rd.ProductionMaterialsRequirement.ProductionOrderId == productionOrderId)
                .ProjectTo<ProductionMaterialsRequirementDetailListModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

            var handovers = await _manufacturingDBContext.ProductionHandover
                .Where(h => h.ProductionOrderId == productionOrderId)
                .ProjectTo<ProductionHandoverModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

            var productionSteps = (from ps in _manufacturingDBContext.ProductionStep
                                   join p in _manufacturingDBContext.ProductionStep on ps.ParentId equals p.ProductionStepId
                                   join s in _manufacturingDBContext.Step on p.StepId equals s.StepId
                                   where !ps.IsDeleted
                                   && !ps.IsFinish
                                   && ps.IsGroup != true
                                   && ps.ContainerTypeId == (int)EnumProductionProcess.EnumContainerType.ProductionOrder
                                   && ps.ContainerId == productionOrderId
                                   select new ProductionStepSimpleModel
                                   {
                                       ProductionStepId = ps.ProductionStepId,
                                       Title = !string.IsNullOrEmpty(ps.Title) ? ps.Title : s.StepName
                                   })
                                   .ToList();

            var productionStepIds = productionSteps.Select(ps => ps.ProductionStepId).ToList();

            var linkDatas = (from ldr in _manufacturingDBContext.ProductionStepLinkDataRole
                             join ld in _manufacturingDBContext.ProductionStepLinkData on ldr.ProductionStepLinkDataId equals ld.ProductionStepLinkDataId
                             where productionStepIds.Contains(ldr.ProductionStepId)
                             select new
                             {
                                 ldr.ProductionStepLinkDataRoleTypeId,
                                 ld.ProductionStepLinkTypeId,
                                 ld.ProductionStepLinkDataId,
                                 ld.LinkDataObjectId,
                                 ld.LinkDataObjectTypeId,
                                 ldr.ProductionStepId
                             })
                             .ToList();

            var inpLinkDatas = linkDatas.Where(ld => ld.ProductionStepLinkDataRoleTypeId == (int)EnumProductionProcess.EnumProductionStepLinkDataRoleType.Input).ToList();
            var outLinkDatas = linkDatas.Where(ld => ld.ProductionStepLinkDataRoleTypeId == (int)EnumProductionProcess.EnumProductionStepLinkDataRoleType.Output).ToList();

            var inputLinkDatas = inpLinkDatas
                .Where(ild => !outLinkDatas.Any(old => old.ProductionStepLinkDataId == ild.ProductionStepLinkDataId && ild.ProductionStepLinkTypeId == (int)EnumProductionProcess.EnumProductionStepLinkType.Handover))
                .ToList();

            var inputProductionStepIds = inputLinkDatas
                .Select(ild => ild.ProductionStepId)
                .Distinct()
                .ToList();

            var inputProductionSteps = productionSteps.Where(ps => inputProductionStepIds.Contains(ps.ProductionStepId)).ToList();

            var inputDataMap = inputLinkDatas
                .GroupBy(d => d.ProductionStepId)
                .ToDictionary(g => g.Key, g => g
                    .Where(d => d.ProductionStepLinkDataRoleTypeId == (int)EnumProductionProcess.EnumProductionStepLinkDataRoleType.Input)
                    .Select(d => new InOutMaterialModel { ObjectId = d.LinkDataObjectId, ObjectTypeId = d.LinkDataObjectTypeId })
                    .Distinct()
                    .ToList()
                );

            var stepDataMap = linkDatas
                .GroupBy(d => d.ProductionStepId)
                .ToDictionary(g => g.Key, g => new InOutProductionStepModel
                {
                    InputData = g.Where(d => d.ProductionStepLinkDataRoleTypeId == (int)EnumProductionProcess.EnumProductionStepLinkDataRoleType.Input).Select(d => new InOutMaterialModel { ObjectId = d.LinkDataObjectId, ObjectTypeId = d.LinkDataObjectTypeId }).Distinct().ToList(),
                    OutputData = g.Where(d => d.ProductionStepLinkDataRoleTypeId == (int)EnumProductionProcess.EnumProductionStepLinkDataRoleType.Output).Select(d => new InOutMaterialModel { ObjectId = d.LinkDataObjectId, ObjectTypeId = d.LinkDataObjectTypeId }).Distinct().ToList(),
                });

            var lstAssignments = _manufacturingDBContext.ProductionAssignment
                .Where(a => productionStepIds.Contains(a.ProductionStepId))
                .Select(a => new
                {
                    a.ProductionStepId,
                    a.DepartmentId
                })
                .Distinct()
                .ToList();

            var assignments = lstAssignments
                .GroupBy(a => a.ProductionStepId)
                .ToDictionary(g => g.Key, g => g.Select(a => a.DepartmentId).ToList());

            // Map thông tin vật tư đầu vào, bộ phận phân công theo công đoạn
            var inputStepObjectMap = new List<InputStepObjectDepartmentModel>();
            foreach (var inputProductionStepId in inputProductionStepIds)
            {
                var inputStepObjectDepartment = new InputStepObjectDepartmentModel
                {
                    ProductionStepId = inputProductionStepId,
                    ProductIds = inputLinkDatas.Where(d => d.ProductionStepId == inputProductionStepId).Select(d => d.LinkDataObjectId).Distinct().ToList(),
                    DepartmentIds = assignments.ContainsKey(inputProductionStepId) ? assignments[inputProductionStepId] : new List<int>()
                };
                inputStepObjectMap.Add(inputStepObjectDepartment);
            }

            var assignmentDataMap = new Dictionary<int, InOutProductionStepModel>();
            foreach (var item in lstAssignments)
            {
                if (!stepDataMap.ContainsKey(item.ProductionStepId)) continue;
                if (!assignmentDataMap.ContainsKey(item.DepartmentId))
                {
                    assignmentDataMap.Add(item.DepartmentId, stepDataMap[item.ProductionStepId]);
                }
                else
                {
                    assignmentDataMap[item.DepartmentId].InputData.AddRange(stepDataMap[item.ProductionStepId].InputData);
                    assignmentDataMap[item.DepartmentId].OutputData.AddRange(stepDataMap[item.ProductionStepId].OutputData);
                }
            }

            var conflictInventories = inventories
                .Where(inv => !inv.DepartmentId.HasValue
                || (inv.ProductionStepId.HasValue &&
                    (!productionStepIds.Contains(inv.ProductionStepId.Value)
                    || !assignments.ContainsKey(inv.ProductionStepId.Value)
                    || !assignments[inv.ProductionStepId.Value].Contains(inv.DepartmentId.Value)
                    || !stepDataMap.ContainsKey(inv.ProductionStepId.Value)
                    || (inv.InventoryTypeId == EnumInventoryType.Output && !stepDataMap[inv.ProductionStepId.Value].InputData.Any(d => d.ObjectTypeId == (int)EnumProductionProcess.EnumProductionStepLinkDataObjectType.Product && d.ObjectId == inv.ProductId))
                    || (inv.InventoryTypeId == EnumInventoryType.Input && !stepDataMap[inv.ProductionStepId.Value].OutputData.Any(d => d.ObjectTypeId == (int)EnumProductionProcess.EnumProductionStepLinkDataObjectType.Product && d.ObjectId == inv.ProductId))
                    ))
                || (!inv.ProductionStepId.HasValue &&
                    (!assignmentDataMap.ContainsKey(inv.DepartmentId.Value)
                    || (inv.InventoryTypeId == EnumInventoryType.Output && !assignmentDataMap[inv.DepartmentId.Value].InputData.Any(d => d.ObjectTypeId == (int)EnumProductionProcess.EnumProductionStepLinkDataObjectType.Product && d.ObjectId == inv.ProductId))
                    || (inv.InventoryTypeId == EnumInventoryType.Input && !assignmentDataMap[inv.DepartmentId.Value].OutputData.Any(d => d.ObjectTypeId == (int)EnumProductionProcess.EnumProductionStepLinkDataObjectType.Product && d.ObjectId == inv.ProductId))
                    ))
                ).ToList();

            var conflictExportStockInventories = conflictInventories
              .Where(inv => !string.IsNullOrEmpty(inv.InventoryCode) && inv.InventoryTypeId == EnumInventoryType.Output && inv.Status == EnumProductionInventoryRequirementStatus.Accepted)
              .ToList();

            var conflictOtherInventories = conflictInventories
              .Where(inv => string.IsNullOrEmpty(inv.InventoryCode) || inv.InventoryTypeId != EnumInventoryType.Output || inv.Status != EnumProductionInventoryRequirementStatus.Accepted)
              .ToList();

            var conflictMaterialRequirements = materialRequirements
                .Where(mr => !productionStepIds.Contains(mr.ProductionStepId)
                || !assignments.ContainsKey(mr.ProductionStepId)
                || !assignments[mr.ProductionStepId].Contains(mr.DepartmentId)
                || !stepDataMap.ContainsKey(mr.ProductionStepId)
                || !stepDataMap[mr.ProductionStepId].InputData.Any(d => d.ObjectTypeId == (int)EnumProductionProcess.EnumProductionStepLinkDataObjectType.Product && d.ObjectId == mr.ProductId))
                .ToList();

            var conflictHandovers = handovers
                .Where(ho => !productionStepIds.Contains(ho.FromProductionStepId)
                || !productionStepIds.Contains(ho.ToProductionStepId)
                || !assignments.ContainsKey(ho.FromProductionStepId)
                || !assignments.ContainsKey(ho.ToProductionStepId)
                || !assignments[ho.FromProductionStepId].Contains(ho.FromDepartmentId)
                || !assignments[ho.ToProductionStepId].Contains(ho.ToDepartmentId)
                || !stepDataMap.ContainsKey(ho.FromProductionStepId)
                || !stepDataMap.ContainsKey(ho.ToProductionStepId)
                || !stepDataMap[ho.ToProductionStepId].InputData.Any(d => d.ObjectTypeId == (int)ho.ObjectTypeId && d.ObjectId == ho.ObjectId)
                || !stepDataMap[ho.FromProductionStepId].OutputData.Any(d => d.ObjectTypeId == (int)ho.ObjectTypeId && d.ObjectId == ho.ObjectId))
                .ToList();

            var result = new ConflictHandoverModel
            {
                ConflictExportStockInventories = conflictExportStockInventories,
                ConflictOtherInventories = conflictOtherInventories,
                ConflictMaterialRequirements = conflictMaterialRequirements,
                ConflictHandovers = conflictHandovers,
                InputProductionSteps = inputProductionSteps,
                InputStepObjectMap = inputStepObjectMap,
                InputDataMap = inputDataMap,
                Assignments = assignments
            };

            return result;
        }

        private List<int> GetMaterialsConsumptionIds(IEnumerable<ProductMaterialsConsumptionSimpleModel> materialsConsumptions)
        {
            var materialsConsumptionIds = new List<int>();
            foreach (var item in materialsConsumptions)
            {
                materialsConsumptionIds.Add(item.ProductId);
                materialsConsumptionIds.AddRange(GetMaterialsConsumptionIds(item.MaterialsConsumptionInheri));
            }
            return materialsConsumptionIds;
        }

        /*
        public async Task<bool> UpdateIgnoreAllocation(string[] productionOrderCodes, bool ignoreEnqueueUpdateProductionOrderStatus = false)
        {
            var productionOrderIds = _manufacturingDBContext.ProductionOrder
                .Where(po => productionOrderCodes.Contains(po.ProductionOrderCode))
                .Select(po => po.ProductionOrderId)
                .ToList();
            var productIds = _manufacturingDBContext.ProductionOrderDetail
                .Where(pod => productionOrderIds.Contains(pod.ProductionOrderId))
                .Select(pod => pod.ProductId)
                .Distinct()
                .ToArray();

            var materialsConsumptions = await _productHelperService.GetProductMaterialsConsumptions(productIds);

            var materialsConsumptionIds = GetMaterialsConsumptionIds(materialsConsumptions).Distinct().ToList();

            foreach (var productionOrderId in productionOrderIds)
            {
                var parammeters = new SqlParameter[]
                {
                    new SqlParameter("@ProductionOrderId", productionOrderId)
                };

                var resultData = await _manufacturingDBContext.ExecuteDataProcedure("asp_ProductionHandover_GetInventoryRequirementByProductionOrder", parammeters);

                var inventories = resultData.ConvertData<InternalProductionInventoryRequirementModel>()
                    .AsQueryable()
                    .ProjectTo<ProductionInventoryRequirementModel>(_mapper.ConfigurationProvider)
                    .ToList();

                var productionSteps = (from ps in _manufacturingDBContext.ProductionStep
                                       join p in _manufacturingDBContext.ProductionStep on ps.ParentId equals p.ProductionStepId
                                       join s in _manufacturingDBContext.Step on p.StepId equals s.StepId
                                       where !ps.IsDeleted
                                       && !ps.IsFinish
                                       && ps.IsGroup != true
                                       && ps.ContainerTypeId == (int)EnumProductionProcess.EnumContainerType.ProductionOrder
                                       && ps.ContainerId == productionOrderId
                                       select new ProductionStepSimpleModel
                                       {
                                           ProductionStepId = ps.ProductionStepId,
                                           Title = !string.IsNullOrEmpty(ps.Title) ? ps.Title : s.StepName
                                       })
                                       .ToList();

                var productionStepIds = productionSteps.Select(ps => ps.ProductionStepId).ToList();

                var linkDatas = (from ldr in _manufacturingDBContext.ProductionStepLinkDataRole
                                 join ld in _manufacturingDBContext.ProductionStepLinkData on ldr.ProductionStepLinkDataId equals ld.ProductionStepLinkDataId
                                 where productionStepIds.Contains(ldr.ProductionStepId)
                                 select new
                                 {
                                     ldr.ProductionStepLinkDataRoleTypeId,
                                     ld.ProductionStepLinkTypeId,
                                     ld.ProductionStepLinkDataId,
                                     ld.LinkDataObjectId,
                                     ld.LinkDataObjectTypeId,
                                     ldr.ProductionStepId
                                 })
                                 .ToList();

                var inpLinkDatas = linkDatas.Where(ld => ld.ProductionStepLinkDataRoleTypeId == (int)EnumProductionProcess.EnumProductionStepLinkDataRoleType.Input).ToList();
                var outLinkDatas = linkDatas.Where(ld => ld.ProductionStepLinkDataRoleTypeId == (int)EnumProductionProcess.EnumProductionStepLinkDataRoleType.Output).ToList();

                var inputLinkDatas = inpLinkDatas
                    .Where(ild => !outLinkDatas.Any(old => old.ProductionStepLinkDataId == ild.ProductionStepLinkDataId && ild.ProductionStepLinkTypeId == (int)EnumProductionProcess.EnumProductionStepLinkType.Handover))
                    .ToList();

                var inputProductionStepIds = inputLinkDatas
                    .Select(ild => ild.ProductionStepId)
                    .Distinct()
                    .ToList();

                var inputProductionSteps = productionSteps.Where(ps => inputProductionStepIds.Contains(ps.ProductionStepId)).ToList();

                var inputDataMap = inputLinkDatas
                    .GroupBy(d => d.ProductionStepId)
                    .ToDictionary(g => g.Key, g => g.Where(d => d.ProductionStepLinkDataRoleTypeId == (int)EnumProductionProcess.EnumProductionStepLinkDataRoleType.Input).Select(d => new InOutMaterialModel { ObjectId = d.LinkDataObjectId, ObjectTypeId = d.LinkDataObjectTypeId }).Distinct().ToList());

                var stepDataMap = linkDatas
                    .GroupBy(d => d.ProductionStepId)
                    .ToDictionary(g => g.Key, g => new InOutProductionStepModel
                    {
                        InputData = g.Where(d => d.ProductionStepLinkDataRoleTypeId == (int)EnumProductionProcess.EnumProductionStepLinkDataRoleType.Input).Select(d => new InOutMaterialModel { ObjectId = d.LinkDataObjectId, ObjectTypeId = d.LinkDataObjectTypeId }).Distinct().ToList(),
                        OutputData = g.Where(d => d.ProductionStepLinkDataRoleTypeId == (int)EnumProductionProcess.EnumProductionStepLinkDataRoleType.Output).Select(d => new InOutMaterialModel { ObjectId = d.LinkDataObjectId, ObjectTypeId = d.LinkDataObjectTypeId }).Distinct().ToList(),
                    });

                var lstAssignments = _manufacturingDBContext.ProductionAssignment
                    .Where(a => productionStepIds.Contains(a.ProductionStepId))
                    .Select(a => new
                    {
                        a.ProductionStepId,
                        a.DepartmentId
                    })
                    .Distinct()
                    .ToList();

                var assignments = lstAssignments
                    .GroupBy(a => a.ProductionStepId)
                    .ToDictionary(g => g.Key, g => g.Select(a => a.DepartmentId).ToList());


                var assignmentDataMap = new Dictionary<int, InOutProductionStepModel>();
                foreach (var item in lstAssignments)
                {
                    if (!stepDataMap.ContainsKey(item.ProductionStepId)) continue;
                    if (!assignmentDataMap.ContainsKey(item.DepartmentId))
                    {
                        assignmentDataMap.Add(item.DepartmentId, stepDataMap[item.ProductionStepId]);
                    }
                    else
                    {
                        assignmentDataMap[item.DepartmentId].InputData.AddRange(stepDataMap[item.ProductionStepId].InputData);
                        assignmentDataMap[item.DepartmentId].OutputData.AddRange(stepDataMap[item.ProductionStepId].OutputData);
                    }
                }

                var conflictInventories = inventories
                    .Where(inv => !inv.DepartmentId.HasValue
                    || (inv.ProductionStepId.HasValue &&
                        (!productionStepIds.Contains(inv.ProductionStepId.Value)
                        || !assignments.ContainsKey(inv.ProductionStepId.Value)
                        || !assignments[inv.ProductionStepId.Value].Contains(inv.DepartmentId.Value)
                        || !stepDataMap.ContainsKey(inv.ProductionStepId.Value)
                        || (inv.InventoryTypeId == EnumInventoryType.Output && !stepDataMap[inv.ProductionStepId.Value].InputData.Any(d => d.ObjectTypeId == (int)EnumProductionProcess.EnumProductionStepLinkDataObjectType.Product && d.ObjectId == inv.ProductId))
                        || (inv.InventoryTypeId == EnumInventoryType.Input && !stepDataMap[inv.ProductionStepId.Value].OutputData.Any(d => d.ObjectTypeId == (int)EnumProductionProcess.EnumProductionStepLinkDataObjectType.Product && d.ObjectId == inv.ProductId))
                        ))
                    || (!inv.ProductionStepId.HasValue &&
                        (!assignmentDataMap.ContainsKey(inv.DepartmentId.Value)
                        || (inv.InventoryTypeId == EnumInventoryType.Output && !assignmentDataMap[inv.DepartmentId.Value].InputData.Any(d => d.ObjectTypeId == (int)EnumProductionProcess.EnumProductionStepLinkDataObjectType.Product && d.ObjectId == inv.ProductId))
                        || (inv.InventoryTypeId == EnumInventoryType.Input && !assignmentDataMap[inv.DepartmentId.Value].OutputData.Any(d => d.ObjectTypeId == (int)EnumProductionProcess.EnumProductionStepLinkDataObjectType.Product && d.ObjectId == inv.ProductId))
                        ))
                    ).ToList();

                var conflictExportStockInventories = conflictInventories
                  .Where(inv => !string.IsNullOrEmpty(inv.InventoryCode) && inv.InventoryTypeId == EnumInventoryType.Output && inv.Status == EnumProductionInventoryRequirementStatus.Accepted)
                  .ToList();

                var ignoreAllocations = _manufacturingDBContext.IgnoreAllocation
                    .Where(ma => ma.ProductionOrderId == productionOrderId)
                    .ToList();

                var materialAllocations = (await _manufacturingDBContext.ProductionHandover
                    .Where(h => h.ProductionOrderId == productionOrderId && (h.FromDepartmentId == 0 || h.ToDepartmentId == 0) && !h.IsAuto)
                    .ToListAsync()
                    ).Select(h => ProductionHandoverToAllowcationModel(h))
                    .ToList();



                foreach (var item in conflictExportStockInventories)
                {
                    if (ignoreAllocations.Any(ia => ia.InventoryCode == item.InventoryCode) || materialAllocations.Any(ma => ma.InventoryCode == item.InventoryCode)) continue;

                    if (materialsConsumptionIds.Contains(item.ProductId))
                    {
                        var entity = new IgnoreAllocation
                        {
                            ProductId = item.ProductId,
                            InventoryCode = item.InventoryCode,
                            ProductionOrderId = productionOrderId
                        };
                        _manufacturingDBContext.IgnoreAllocation.Add(entity);
                    }
                }
            }
            _manufacturingDBContext.SaveChanges();

            if (!ignoreEnqueueUpdateProductionOrderStatus)
            {
                foreach (var code in productionOrderCodes)
                {
                    await _productionOrderQueueHelperService.ProductionOrderStatiticChanges(code, $"Cập nhật phân bổ vật tư sản xuất");
                }
            }

            return true;
        }*/
    }
}
