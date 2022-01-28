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
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Manafacturing.Model.ProductionAssignment;
using VErp.Commons.Enums.Manafacturing;
using Microsoft.Data.SqlClient;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;
using VErp.Services.Manafacturing.Model.ProductionHandover;
using ProductionAssignmentEntity = VErp.Infrastructure.EF.ManufacturingDB.ProductionAssignment;
using VErp.Services.Manafacturing.Model.ProductionOrder.Materials;
using Newtonsoft.Json;

namespace VErp.Services.Manafacturing.Service.StatusProcess.Implement
{
    public abstract class StatusProcessService : IStatusProcessService
    {
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        public StatusProcessService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger logger
            , IMapper mapper)
        {
            _manufacturingDBContext = manufacturingDB;
            _activityLogService = activityLogService;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<IList<ProductionInventoryRequirementModel>> GetProductionInventoryRequirements(long productionOrderId)
        {
            var parammeters = new SqlParameter[]
            {
                new SqlParameter("@ProductionOrderId", productionOrderId)
            };
            var resultData = await _manufacturingDBContext.ExecuteDataProcedure("asp_ProductionHandover_GetInventoryRequirementByProductionOrder", parammeters);

            return resultData.ConvertData<ProductionInventoryRequirementEntity>()
                .AsQueryable()
                .ProjectTo<ProductionInventoryRequirementModel>(_mapper.ConfigurationProvider)
                .ToList();
        }

        public async Task<IList<DepartmentHandoverDetailModel>> GetDepartmentHandoverDetail(long productionOrderId, long? productionStepId = null, int? departmentId = null, IList<ProductionInventoryRequirementEntity> inventories = null)
        {
            var result = new List<DepartmentHandoverDetailModel>();
            var productionSteps = _manufacturingDBContext.ProductionStep
                .Where(ps => ps.ContainerId == productionOrderId
                && (!productionStepId.HasValue || ps.ProductionStepId == productionStepId)
                && ps.ContainerTypeId == (int)EnumContainerType.ProductionOrder
                && ps.IsGroup.HasValue
                && ps.IsGroup.Value
                && !ps.IsFinish)
                .ToList();

            if (productionSteps.Count == 0) return result;
            var parentIds = productionSteps.Select(ps => ps.ProductionStepId).ToList();

            var groups = _manufacturingDBContext.ProductionStep
                .Where(ps => ps.ParentId.HasValue && parentIds.Contains(ps.ParentId.Value) && !ps.IsFinish)
                .ToList();

            var outsourceStepRequestIds = groups.Where(g => g.OutsourceStepRequestId.HasValue).Select(g => g.OutsourceStepRequestId).ToList();
            var outsourceStepRequests = _manufacturingDBContext.OutsourceStepRequest.Where(o => outsourceStepRequestIds.Contains(o.OutsourceStepRequestId)).ToList();
            var groupIds = groups.Select(g => g.ProductionStepId).ToList();

            var allAssignments = _manufacturingDBContext.ProductionAssignment
                .Where(a => a.ProductionOrderId == productionOrderId)
                .ProjectTo<ProductionAssignmentModel>(_mapper.ConfigurationProvider)
                .ToList();


            var assignments = allAssignments
                .Where(a => a.ProductionStepId.HasValue && groupIds.Contains(a.ProductionStepId.Value) && (!departmentId.HasValue || a.DepartmentId == departmentId))
                .ToList();

            var allInputLinkDatas = (from ld in _manufacturingDBContext.ProductionStepLinkData
                                     join ldr in _manufacturingDBContext.ProductionStepLinkDataRole on ld.ProductionStepLinkDataId equals ldr.ProductionStepLinkDataId
                                     where groupIds.Contains(ldr.ProductionStepId) && ldr.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Input
                                     select new
                                     {
                                         ldr.ProductionStepId,
                                         ProductionStepLinkData = ld
                                     }).ToList();

            var allOutputLinkDatas = (from ld in _manufacturingDBContext.ProductionStepLinkData
                                      join ldr in _manufacturingDBContext.ProductionStepLinkDataRole on ld.ProductionStepLinkDataId equals ldr.ProductionStepLinkDataId
                                      where groupIds.Contains(ldr.ProductionStepId) && ldr.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Output
                                      select new
                                      {
                                          ldr.ProductionStepId,
                                          ProductionStepLinkData = ld
                                      }).ToList();

            var departmentIds = assignments.Select(a => a.DepartmentId).Distinct().ToList();

            // Lấy thông tin đầu vào/ra tất cả phân công công đoạn trong lệnh
            var allProductionAssignments = (
                from a in _manufacturingDBContext.ProductionAssignment
                join ps in _manufacturingDBContext.ProductionStep on a.ProductionStepId equals ps.ProductionStepId
                join d in _manufacturingDBContext.ProductionStepLinkData on a.ProductionStepLinkDataId equals d.ProductionStepLinkDataId
                join ldr in _manufacturingDBContext.ProductionStepLinkDataRole on ps.ProductionStepId equals ldr.ProductionStepId
                join ld in _manufacturingDBContext.ProductionStepLinkData on ldr.ProductionStepLinkDataId equals ld.ProductionStepLinkDataId
                join ildr in _manufacturingDBContext.ProductionStepLinkDataRole on new
                {
                    ldr.ProductionStepLinkDataId,
                    IsInput = ldr.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Input
                } equals new
                {
                    ildr.ProductionStepLinkDataId,
                    IsInput = !(ildr.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Input)
                } into ildrs
                from ildr in ildrs.DefaultIfEmpty()
                where departmentIds.Contains(a.DepartmentId)
                    && ps.ContainerTypeId == (int)EnumContainerType.ProductionOrder
                    && ps.ContainerId == productionOrderId
                    && ld.ObjectTypeId == (int)EnumProductionStepLinkDataObjectType.Product
                orderby a.CreatedDatetimeUtc ascending
                select new
                {
                    a.DepartmentId,
                    a.ProductionStepId,
                    a.AssignmentQuantity,
                    ldr.ProductionStepLinkDataRoleTypeId,
                    TotalQuantity = d.QuantityOrigin - d.OutsourcePartQuantity.GetValueOrDefault(),
                    ld.ObjectId,
                    OutputQuantity = ld.Quantity - d.OutsourcePartQuantity.GetValueOrDefault(),
                    IsHandover = ildr == null
                })
                .GroupBy(a => new
                {
                    a.DepartmentId,
                    a.ProductionStepId,
                    a.ProductionStepLinkDataRoleTypeId,
                    a.ObjectId
                })
                .Select(g => new
                {
                    g.Key.DepartmentId,
                    g.Key.ProductionStepId,
                    g.Key.ObjectId,
                    g.Key.ProductionStepLinkDataRoleTypeId,
                    AssignmentQuantity = g.Max(a => a.AssignmentQuantity),
                    TotalQuantity = g.Max(a => a.TotalQuantity),
                    OutputQuantity = g.Sum(a => a.OutputQuantity),
                    HandoverStockQuantity = g.Sum(a => a.IsHandover ? a.OutputQuantity : 0)
                })
                .Where(a => a.HandoverStockQuantity > 0)
                .Select(a => new AssigmentLinkData
                {
                    DepartmentId = a.DepartmentId,
                    ProductionStepId = a.ProductionStepId,
                    ObjectId = a.ObjectId,
                    ProductionStepLinkDataRoleTypeId = a.ProductionStepLinkDataRoleTypeId,
                    HandoverStockQuantity = a.HandoverStockQuantity * a.AssignmentQuantity / a.TotalQuantity
                })
                .ToList();

            // Lấy thông tin xuất nhập kho
            if (inventories == null)
            {
                var parammeters = new SqlParameter[]
                {
                        new SqlParameter("@ProductionOrderId", productionOrderId)
                };
                var resultData = await _manufacturingDBContext.ExecuteDataProcedure("asp_ProductionHandover_GetInventoryRequirementByProductionOrder", parammeters);

                inventories = resultData.ConvertData<ProductionInventoryRequirementEntity>();
            }

            var inventoryRequirements = inventories
                .AsQueryable()
                .ProjectTo<ProductionInventoryRequirementModel>(_mapper.ConfigurationProvider)
                .ToList();

            var productionStepIds = groups.Select(g => g.ProductionStepId).ToList();
            // Lấy thông tin vật tư yêu cầu thêm
            var allMaterialRequirements = _manufacturingDBContext.ProductionMaterialsRequirementDetail
                 .Include(mrd => mrd.ProductionMaterialsRequirement)
                 .Where(mrd => mrd.ProductionMaterialsRequirement.ProductionOrderId == productionOrderId
                 && productionStepIds.Contains(mrd.ProductionStepId)
                 && departmentIds.Contains(mrd.DepartmentId)
                 && mrd.ProductionMaterialsRequirement.CensorStatus != (int)EnumProductionMaterialsRequirementStatus.Accepted)
                 .ProjectTo<ProductionMaterialsRequirementDetailListModel>(_mapper.ConfigurationProvider)
                 .ToList();

            // Lấy thông tin phân bổ thủ công
            var allMaterialAllocations = _manufacturingDBContext.MaterialAllocation
                .Where(ma => ma.ProductionOrderId == productionOrderId && productionStepIds.Contains(ma.ProductionStepId) && departmentIds.Contains(ma.DepartmentId))
                .ToList();

            // Lấy thông tin bàn giao
            var allHandovers = _manufacturingDBContext.ProductionHandover
                    .Where(h => h.ProductionOrderId == productionOrderId
                    && ((departmentIds.Contains(h.FromDepartmentId) && productionStepIds.Contains(h.FromProductionStepId))
                    || (departmentIds.Contains(h.ToDepartmentId) && productionStepIds.Contains(h.ToProductionStepId))))
                    .ProjectTo<ProductionHandoverModel>(_mapper.ConfigurationProvider)
                    .ToList();

            var allLinkDataIds = (allInputLinkDatas.Select(ld => ld.ProductionStepLinkData.ProductionStepLinkDataId)
                .Concat(allOutputLinkDatas.Select(ld => ld.ProductionStepLinkData.ProductionStepLinkDataId)).ToList());

            var allStepLinkData = (from ldr in _manufacturingDBContext.ProductionStepLinkDataRole
                                   join gps in _manufacturingDBContext.ProductionStep on ldr.ProductionStepId equals gps.ProductionStepId
                                   join o in _manufacturingDBContext.OutsourceStepRequest on gps.OutsourceStepRequestId equals o.OutsourceStepRequestId into gos
                                   from o in gos.DefaultIfEmpty()
                                   join ps in _manufacturingDBContext.ProductionStep on gps.ParentId equals ps.ProductionStepId
                                   join s in _manufacturingDBContext.Step on ps.StepId equals s.StepId
                                   where !ps.IsFinish && allLinkDataIds.Contains(ldr.ProductionStepLinkDataId)
                                   select new StepLinkDataInfo
                                   {
                                       ProductionStepLinkDataId = ldr.ProductionStepLinkDataId,
                                       ProductionStepId = gps.ProductionStepId,
                                       StepName = s.StepName,
                                       OutsourceStepRequestId = o.OutsourceStepRequestId,
                                       OutsourceStepRequestCode = o.OutsourceStepRequestCode
                                   })
                                   .ToList();


            foreach (var inOutGroup in groups)
            {
                var cloneAllProductionAssignments = JsonConvert.DeserializeObject<List<AssigmentLinkData>>(JsonConvert.SerializeObject(allProductionAssignments));

                var inOutStepGroupAssignments = assignments
                        .Where(a => a.ProductionStepId.HasValue && a.ProductionStepId.Value == inOutGroup.ProductionStepId)
                        .ToList();

                var outsourceStepRequest = outsourceStepRequests.FirstOrDefault(o => o.OutsourceStepRequestId == inOutGroup.OutsourceStepRequestId);

                var inputLinkDatas = allInputLinkDatas.Where(ld => ld.ProductionStepId == inOutGroup.ProductionStepId).Select(ld => ld.ProductionStepLinkData).ToList();
                var outputLinkDatas = allOutputLinkDatas.Where(ld => ld.ProductionStepId == inOutGroup.ProductionStepId).Select(ld => ld.ProductionStepLinkData).ToList();
                var linkDataIds = (inputLinkDatas.Select(ldr => ldr.ProductionStepLinkDataId).Concat(outputLinkDatas.Select(ldr => ldr.ProductionStepLinkDataId)).ToList());

                // Danh sách liên kết đầu vào / ra với công đoạn hiện tại
                var stepMap = allStepLinkData
                    .Where(sld => sld.ProductionStepId != inOutGroup.ProductionStepId && linkDataIds.Contains(sld.ProductionStepLinkDataId))
                    .GroupBy(ldr => ldr.ProductionStepLinkDataId)
                    .ToDictionary(g => g.Key, g => g.First());

                // Lấy thông tin phân công các công đoạn liền kề
                var stepIds = stepMap.Select(m => m.Value.ProductionStepId).ToList();
                var adjacentAssignments = allAssignments
                        .Where(a => stepIds.Contains(a.ProductionStepId.Value))
                        .ToList();

                var inputDatas = new List<StepInOutData>();
                var outputDatas = new List<StepInOutData>();

                // Lấy thông tin đầu vào
                foreach (var inputLinkData in inputLinkDatas)
                {
                    // Nếu có nguồn vào => vật tư được bàn giao từ công đoạn trước
                    // Nếu không có nguồn vào => vật tư được xuất từ kho
                    var fromStep = stepMap.ContainsKey(inputLinkData.ProductionStepLinkDataId) ? stepMap[inputLinkData.ProductionStepLinkDataId] : null;
                    long? fromStepId = fromStep?.ProductionStepId ?? null;

                    // Nếu công đoạn có nhận đầu vào từ gia công và không cùng gia công với công đoạn trước
                    if (inputLinkData.OutsourceQuantity > 0 && fromStep != null && fromStep.OutsourceStepRequestId != (outsourceStepRequest?.OutsourceStepRequestId ?? 0))
                    {
                        var ousourceInput = inputDatas
                            .Where(d => d.ObjectId == inputLinkData.ObjectId
                            && d.ObjectTypeId == inputLinkData.ObjectTypeId
                            && d.FromStepId == fromStepId
                            && d.OutsourceStepRequestId == fromStep.OutsourceStepRequestId)
                            .FirstOrDefault();

                        if (ousourceInput != null)
                        {
                            ousourceInput.RequireQuantity += inputLinkData.QuantityOrigin - inputLinkData.OutsourcePartQuantity.GetValueOrDefault();
                            ousourceInput.TotalQuantity += inputLinkData.QuantityOrigin - inputLinkData.OutsourcePartQuantity.GetValueOrDefault();
                        }
                        else
                        {
                            inputDatas.Add(new StepInOutData
                            {
                                ObjectId = inputLinkData.ObjectId,
                                ObjectTypeId = inputLinkData.ObjectTypeId,
                                RequireQuantity = inputLinkData.QuantityOrigin - inputLinkData.OutsourcePartQuantity.GetValueOrDefault(),
                                TotalQuantity = inputLinkData.QuantityOrigin - inputLinkData.OutsourcePartQuantity.GetValueOrDefault(),
                                ReceivedQuantity = 0,
                                FromStepTitle = $"{fromStep.StepName} - {fromStep.OutsourceStepRequestCode}",
                                FromStepId = fromStepId,
                                OutsourceStepRequestId = fromStep.OutsourceStepRequestId
                            });
                        }
                    }

                    var item = inputDatas
                        .Where(d => d.ObjectId == inputLinkData.ObjectId
                        && d.ObjectTypeId == inputLinkData.ObjectTypeId
                        && d.FromStepId == fromStepId
                        && !d.OutsourceStepRequestId.HasValue)
                        .FirstOrDefault();

                    if (item != null)
                    {
                        item.RequireQuantity += inputLinkData.QuantityOrigin - inputLinkData.OutsourcePartQuantity.GetValueOrDefault() - inputLinkData.ExportOutsourceQuantity.GetValueOrDefault();
                        item.TotalQuantity += inputLinkData.QuantityOrigin - inputLinkData.OutsourcePartQuantity.GetValueOrDefault();
                    }
                    else
                    {
                        inputDatas.Add(new StepInOutData
                        {
                            ObjectId = inputLinkData.ObjectId,
                            ObjectTypeId = inputLinkData.ObjectTypeId,
                            RequireQuantity = inputLinkData.QuantityOrigin - inputLinkData.OutsourcePartQuantity.GetValueOrDefault(),
                            TotalQuantity = inputLinkData.QuantityOrigin - inputLinkData.OutsourcePartQuantity.GetValueOrDefault(),
                            ReceivedQuantity = 0,
                            FromStepTitle = fromStepId.HasValue ? $"{fromStep.StepName}" : "Kho",
                            FromStepId = fromStepId
                        });
                    }
                }

                // Lấy thông tin đầu ra
                foreach (var outputLinkData in outputLinkDatas)
                {
                    // Nếu có nguồn ra => vật tư bàn giao tới công đoạn sau
                    // Nếu không có nguồn ra => vật tư được nhập vào kho
                    var toStep = stepMap.ContainsKey(outputLinkData.ProductionStepLinkDataId) ? stepMap[outputLinkData.ProductionStepLinkDataId] : null;
                    long? toStepId = toStep?.ProductionStepId ?? null;

                    // Nếu công đoạn có đầu ra từ gia công và không cùng gia công với công đoạn sau
                    if (outputLinkData.ExportOutsourceQuantity > 0 && toStep != null && toStep.OutsourceStepRequestId > 0 && toStep.OutsourceStepRequestId != (outsourceStepRequest?.OutsourceStepRequestId ?? 0))
                    {
                        var ousourceOutput = outputDatas
                            .Where(d => d.ObjectId == outputLinkData.ObjectId
                            && d.ObjectTypeId == outputLinkData.ObjectTypeId
                            && d.ToStepId == toStepId
                            && d.OutsourceStepRequestId == toStep.OutsourceStepRequestId)
                            .FirstOrDefault();

                        if (ousourceOutput != null)
                        {
                            ousourceOutput.RequireQuantity += outputLinkData.QuantityOrigin - outputLinkData.OutsourcePartQuantity.GetValueOrDefault();
                            ousourceOutput.TotalQuantity += outputLinkData.QuantityOrigin - outputLinkData.OutsourcePartQuantity.GetValueOrDefault();
                        }
                        else
                        {
                            outputDatas.Add(new StepInOutData
                            {
                                ObjectId = outputLinkData.ObjectId,
                                ObjectTypeId = outputLinkData.ObjectTypeId,
                                RequireQuantity = outputLinkData.QuantityOrigin - outputLinkData.OutsourcePartQuantity.GetValueOrDefault(),
                                TotalQuantity = outputLinkData.QuantityOrigin - outputLinkData.OutsourcePartQuantity.GetValueOrDefault(),
                                ReceivedQuantity = 0,
                                ToStepTitle = $"{toStep.StepName}(#{toStep.ProductionStepId}) - {toStep.OutsourceStepRequestCode}",
                                ToStepId = toStepId,
                                OutsourceStepRequestId = toStep.OutsourceStepRequestId
                            });
                        }
                    }

                    var item = outputDatas
                        .Where(d => d.ObjectId == outputLinkData.ObjectId
                        && d.ObjectTypeId == outputLinkData.ObjectTypeId
                        && d.ToStepId == toStepId
                        && !d.OutsourceStepRequestId.HasValue)
                        .FirstOrDefault();

                    if (item != null)
                    {
                        item.RequireQuantity += outputLinkData.QuantityOrigin - outputLinkData.OutsourcePartQuantity.GetValueOrDefault();
                        item.TotalQuantity += outputLinkData.QuantityOrigin - outputLinkData.OutsourcePartQuantity.GetValueOrDefault();
                    }
                    else
                    {
                        outputDatas.Add(new StepInOutData
                        {
                            ObjectId = outputLinkData.ObjectId,
                            ObjectTypeId = outputLinkData.ObjectTypeId,
                            RequireQuantity = outputLinkData.QuantityOrigin - outputLinkData.OutsourcePartQuantity.GetValueOrDefault(),
                            TotalQuantity = outputLinkData.QuantityOrigin - outputLinkData.OutsourcePartQuantity.GetValueOrDefault(),
                            ReceivedQuantity = 0,
                            ToStepTitle = toStepId.HasValue ? $"{toStep.StepName}(#{toStep.ProductionStepId})" : "Kho",
                            ToStepId = toStepId,
                        });
                    }
                }

                // Tính toán theo phân công
                foreach (var productionAssignment in inOutStepGroupAssignments)
                {
                    // Lấy thông tin bàn giao
                    var handovers = allHandovers
                        .Where(h => ((h.FromDepartmentId == productionAssignment.DepartmentId && h.FromProductionStepId == inOutGroup.ProductionStepId)
                        || (h.ToDepartmentId == productionAssignment.DepartmentId && h.ToProductionStepId == inOutGroup.ProductionStepId)))
                        .ToList();

                    // Lấy thông tin yêu cầu thêm
                    var materialRequirements = allMaterialRequirements
                        .Where(mrd => mrd.ProductionStepId == inOutGroup.ProductionStepId
                        && mrd.DepartmentId == productionAssignment.DepartmentId)
                        .ToList();

                    var outputLink = outputLinkDatas.Where(ld => ld.ProductionStepLinkDataId == productionAssignment.ProductionStepLinkDataId).FirstOrDefault();
                    var quantity = outputLink != null ? outputLink.QuantityOrigin - outputLink.OutsourcePartQuantity.GetValueOrDefault() : 0;
                    if (quantity == 0) throw new BadRequestException(GeneralCode.InvalidParams, "Dữ liệu đầu ra dùng để phân công không còn tồn tại trong quy trình");

                    var detail = new DepartmentHandoverDetailModel
                    {
                        ProductionStepId = inOutGroup.ProductionStepId,
                        DepartmentId = productionAssignment.DepartmentId,
                        AdjacentAssignments = adjacentAssignments,
                        InputDatas = JsonConvert.DeserializeObject<List<StepInOutData>>(JsonConvert.SerializeObject(inputDatas)),
                        OutputDatas = JsonConvert.DeserializeObject<List<StepInOutData>>(JsonConvert.SerializeObject(outputDatas)),

                    };

                    // Tính toán khối lượng đầu vào theo phân công công việc
                    foreach (var inputData in detail.InputDatas)
                    {
                        inputData.RequireQuantity = inputData.RequireQuantity * productionAssignment.AssignmentQuantity / quantity;
                        inputData.TotalQuantity = inputData.TotalQuantity * productionAssignment.AssignmentQuantity / quantity;
                    }

                    // Tính toán khối lượng đầu vào đã thực hiện
                    foreach (var inputLinkData in inputLinkDatas)
                    {
                        // Nếu có nguồn vào => vật tư được bàn giao từ công đoạn trước
                        // Nếu không có nguồn vào => vật tư được xuất từ kho
                        var fromStep = stepMap.ContainsKey(inputLinkData.ProductionStepLinkDataId) ? stepMap[inputLinkData.ProductionStepLinkDataId] : null;
                        long? fromStepId = fromStep?.ProductionStepId ?? null;

                        // Nếu công đoạn có nhận đầu vào từ gia công và không cùng gia công với công đoạn trước
                        if (inputLinkData.OutsourceQuantity > 0 && fromStep != null && fromStep.OutsourceStepRequestId != (outsourceStepRequest?.OutsourceStepRequestId ?? 0))
                        {
                            var ousourceInput = detail.InputDatas
                                .Where(d => d.ObjectId == inputLinkData.ObjectId
                                && d.ObjectTypeId == inputLinkData.ObjectTypeId
                                && d.FromStepId == fromStepId
                                && d.OutsourceStepRequestId == fromStep.OutsourceStepRequestId)
                                .FirstOrDefault();

                            ousourceInput.InventoryRequirementHistories = inventoryRequirements
                                .Where(i => i.DepartmentId == departmentId
                                && i.ProductionStepId == inOutGroup.ProductionStepId
                                && i.InventoryTypeId == EnumInventoryType.Output
                                && i.ProductId == inputLinkData.ObjectId
                                && i.OutsourceStepRequestId.HasValue
                                && i.OutsourceStepRequestId.Value == fromStep.OutsourceStepRequestId)
                                .ToList();

                            ousourceInput.MaterialsRequirementHistories = materialRequirements
                                .Where(mr => mr.ProductId == inputLinkData.ObjectId
                                && mr.OutsourceStepRequestId.HasValue
                                && mr.OutsourceStepRequestId.Value == fromStep.OutsourceStepRequestId)
                                .ToList();

                            ousourceInput.ReceivedQuantity = ousourceInput.InventoryRequirementHistories
                                .Where(h => h.Status == EnumProductionInventoryRequirementStatus.Accepted).Sum(h => h.ActualQuantity);
                        }

                        var item = detail.InputDatas
                            .Where(d => d.ObjectId == inputLinkData.ObjectId
                            && d.ObjectTypeId == inputLinkData.ObjectTypeId
                            && d.FromStepId == fromStepId
                            && !d.OutsourceStepRequestId.HasValue)
                            .FirstOrDefault();

                        if (fromStepId.HasValue)
                        {
                            item.HandoverHistories = handovers.Where(h => h.ToDepartmentId == productionAssignment.DepartmentId
                                && h.ToProductionStepId == inOutGroup.ProductionStepId
                                && h.ObjectId == inputLinkData.ObjectId
                                && h.ObjectTypeId == (EnumProductionStepLinkDataObjectType)inputLinkData.ObjectTypeId
                                && h.FromProductionStepId == fromStepId.Value)
                                .ToList();
                            item.ReceivedQuantity = item.HandoverHistories.Where(h => h.Status == EnumHandoverStatus.Accepted).Sum(h => h.HandoverQuantity);
                        }
                        else
                        {
                            // Phiếu xuất kho đã phân bổ
                            item.InventoryRequirementHistories = inventoryRequirements.Where(h => h.DepartmentId == productionAssignment.DepartmentId
                                   && h.ProductionStepId == inOutGroup.ProductionStepId
                                   && h.InventoryTypeId == EnumInventoryType.Output
                                   && h.ProductId == inputLinkData.ObjectId
                                   && !h.OutsourceStepRequestId.HasValue)
                                   .ToList();

                            item.MaterialsRequirementHistories = materialRequirements
                                .Where(mr => mr.ProductId == inputLinkData.ObjectId && !mr.OutsourceStepRequestId.HasValue)
                                .ToList();

                            item.ReceivedQuantity = item.HandoverHistories.Where(h => h.Status == EnumHandoverStatus.Accepted).Sum(h => h.HandoverQuantity)
                                + item.InventoryRequirementHistories.Where(h => h.Status == EnumProductionInventoryRequirementStatus.Accepted).Sum(h => h.ActualQuantity);


                            // Xử lý phiếu xuất kho phân bổ thủ công
                            var materialAllocations = allMaterialAllocations
                                .Where(ma => ma.SourceProductId.HasValue
                                && ma.SourceProductId.Value == inputLinkData.ObjectId
                                && ma.ProductionStepId == inOutGroup.ProductionStepId
                                && ma.DepartmentId == productionAssignment.DepartmentId)
                                .ToList();

                            foreach (var materialAllocation in materialAllocations)
                            {
                                var inv = inventoryRequirements
                                    .FirstOrDefault(inv => inv.InventoryCode == materialAllocation.InventoryCode && inv.ProductId == materialAllocation.ProductId);
                                if (inv == null) continue;
                                var history = JsonConvert.DeserializeObject<ProductionInventoryRequirementModel>(JsonConvert.SerializeObject(inv));

                                if (!materialAllocation.SourceProductId.HasValue && inv.ProductId == inputLinkData.ObjectId)
                                {
                                    history.ActualQuantity = materialAllocation.AllocationQuantity;
                                }
                                else
                                {
                                    history.ActualQuantity = materialAllocation.SourceQuantity.GetValueOrDefault();
                                    history.ProductId = materialAllocation.SourceProductId.GetValueOrDefault();
                                }

                                item.InventoryRequirementHistories.Add(history);
                                item.ReceivedQuantity += history.ActualQuantity;
                            }

                            // Xử lý các phiếu xuất kho chưa phân bổ công đoạn
                            var unallocatedInventories = inventoryRequirements.Where(h => h.DepartmentId == productionAssignment.DepartmentId
                                 && (!h.ProductionStepId.HasValue || h.ProductionStepId == 0)
                                 && h.InventoryTypeId == EnumInventoryType.Output
                                 && h.ProductId == inputLinkData.ObjectId
                                 && !h.OutsourceStepRequestId.HasValue
                                 && h.ActualQuantity > 0)
                                 .ToList();

                            foreach (var inventory in unallocatedInventories)
                            {
                                var totalInventoryQuantity = inventory.ActualQuantity;
                                bool isLastest = false;
                                foreach (var assignment in cloneAllProductionAssignments)
                                {
                                    if (totalInventoryQuantity <= 0) break;

                                    if (assignment.ObjectId != inputLinkData.ObjectId
                                         || assignment.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Output) continue;

                                    var allocatedQuantity = inventoryRequirements
                                        .Where(ir => ir.ProductionStepId.HasValue
                                            && ir.ProductionStepId.Value == assignment.ProductionStepId
                                            && ir.DepartmentId == productionAssignment.DepartmentId
                                            && ir.InventoryTypeId == EnumInventoryType.Output
                                            && ir.ProductId == inputLinkData.ObjectId)
                                        .Sum(ir => ir.ActualQuantity);

                                    if (assignment.HandoverStockQuantity <= allocatedQuantity) continue;

                                    var unallocatedQuantity = (totalInventoryQuantity > assignment.HandoverStockQuantity - allocatedQuantity) ? assignment.HandoverStockQuantity - allocatedQuantity : totalInventoryQuantity;
                                    if (assignment.ProductionStepId == inOutGroup.ProductionStepId)
                                    {
                                        item.ReceivedQuantity += unallocatedQuantity;
                                        var history = JsonConvert.DeserializeObject<ProductionInventoryRequirementModel>(JsonConvert.SerializeObject(inventory));
                                        history.ActualQuantity = unallocatedQuantity;
                                        item.InventoryRequirementHistories.Add(history);
                                        isLastest = true;
                                    }
                                    else
                                    {
                                        isLastest = false;
                                    }

                                    assignment.HandoverStockQuantity -= unallocatedQuantity;
                                    totalInventoryQuantity -= unallocatedQuantity;
                                }
                                if (totalInventoryQuantity > 0 && isLastest) item.ReceivedQuantity += totalInventoryQuantity;
                            }

                        }
                    }

                    // Tính toán khối lượng đầu ra theo phân công công việc
                    foreach (var outputData in detail.OutputDatas)
                    {
                        outputData.RequireQuantity = outputData.RequireQuantity * productionAssignment.AssignmentQuantity / quantity;
                        outputData.TotalQuantity = outputData.TotalQuantity * productionAssignment.AssignmentQuantity / quantity;
                    }

                    // Tính toán khối lượng đầu ra đã thực hiện
                    foreach (var outputLinkData in outputLinkDatas)
                    {
                        // Nếu có nguồn ra => vật tư bàn giao tới công đoạn sau
                        // Nếu không có nguồn ra => vật tư được nhập vào kho
                        var toStep = stepMap.ContainsKey(outputLinkData.ProductionStepLinkDataId) ? stepMap[outputLinkData.ProductionStepLinkDataId] : null;
                        long? toStepId = toStep?.ProductionStepId ?? null;

                        // Nếu công đoạn có đầu ra từ gia công và không cùng gia công với công đoạn sau
                        if (outputLinkData.ExportOutsourceQuantity > 0 && toStep != null && toStep.OutsourceStepRequestId > 0 && toStep.OutsourceStepRequestId != (outsourceStepRequest?.OutsourceStepRequestId ?? 0))
                        {
                            var ousourceOutput = detail.OutputDatas
                                .Where(d => d.ObjectId == outputLinkData.ObjectId
                                && d.ObjectTypeId == outputLinkData.ObjectTypeId
                                && d.ToStepId == toStepId
                                && d.OutsourceStepRequestId == toStep.OutsourceStepRequestId)
                                .FirstOrDefault();


                            ousourceOutput.InventoryRequirementHistories = inventoryRequirements
                                    .Where(i => i.DepartmentId == productionAssignment.DepartmentId
                                    && i.ProductionStepId == inOutGroup.ProductionStepId
                                    && i.InventoryTypeId == EnumInventoryType.Input
                                    && i.ProductId == outputLinkData.ObjectId
                                    && i.OutsourceStepRequestId.HasValue
                                    && i.OutsourceStepRequestId.Value == toStep.OutsourceStepRequestId)
                                    .ToList();

                            ousourceOutput.ReceivedQuantity = ousourceOutput.InventoryRequirementHistories.Where(h => h.Status == EnumProductionInventoryRequirementStatus.Accepted).Sum(h => h.ActualQuantity);
                        }

                        var item = detail.OutputDatas
                            .Where(d => d.ObjectId == outputLinkData.ObjectId
                            && d.ObjectTypeId == outputLinkData.ObjectTypeId
                            && d.ToStepId == toStepId
                            && !d.OutsourceStepRequestId.HasValue)
                            .FirstOrDefault();

                        if (toStepId.HasValue)
                        {
                            item.HandoverHistories = handovers.Where(h => h.FromDepartmentId == productionAssignment.DepartmentId
                                    && h.FromProductionStepId == inOutGroup.ProductionStepId
                                    && h.ObjectId == outputLinkData.ObjectId
                                    && h.ObjectTypeId == (EnumProductionStepLinkDataObjectType)outputLinkData.ObjectTypeId
                                    && h.ToProductionStepId == toStepId.Value)
                                    .ToList();

                            item.ReceivedQuantity = item.HandoverHistories.Where(h => h.Status == EnumHandoverStatus.Accepted).Sum(h => h.HandoverQuantity);
                        }
                        else
                        {
                            item.InventoryRequirementHistories = inventoryRequirements
                                    .Where(h => h.DepartmentId == productionAssignment.DepartmentId
                                    && h.ProductionStepId == inOutGroup.ProductionStepId
                                    && h.InventoryTypeId == EnumInventoryType.Input
                                    && h.ProductId == outputLinkData.ObjectId
                                    && !h.OutsourceStepRequestId.HasValue)
                                    .ToList();

                            item.ReceivedQuantity = item.HandoverHistories.Where(h => h.Status == EnumHandoverStatus.Accepted).Sum(h => h.HandoverQuantity)
                                + item.InventoryRequirementHistories.Where(h => h.Status == EnumProductionInventoryRequirementStatus.Accepted).Sum(h => h.ActualQuantity);

                            // Xử lý các phiếu nhập kho chưa phân bổ công đoạn
                            var unallocatedInventories = inventoryRequirements.Where(h => h.DepartmentId == productionAssignment.DepartmentId
                                 && (!h.ProductionStepId.HasValue || h.ProductionStepId == 0)
                                 && h.InventoryTypeId == EnumInventoryType.Input
                                 && h.ProductId == outputLinkData.ObjectId
                                 && !h.OutsourceStepRequestId.HasValue
                                 && h.ActualQuantity > 0)
                                 .ToList();

                            foreach (var inventory in unallocatedInventories)
                            {
                                var totalInventoryQuantity = inventory.ActualQuantity;
                                bool isLastest = false;
                                foreach (var assignment in cloneAllProductionAssignments)
                                {
                                    if (totalInventoryQuantity <= 0) break;

                                    var allocatedQuantity = inventoryRequirements
                                        .Where(ir => ir.ProductionStepId.HasValue
                                            && ir.ProductionStepId.Value == assignment.ProductionStepId
                                            && ir.DepartmentId == productionAssignment.DepartmentId
                                            && ir.InventoryTypeId == EnumInventoryType.Input
                                            && ir.ProductId == outputLinkData.ObjectId)
                                        .Sum(ir => ir.ActualQuantity);

                                    if (assignment.HandoverStockQuantity <= allocatedQuantity) continue;

                                    if (assignment.ObjectId != outputLinkData.ObjectId
                                         || assignment.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Input) continue;


                                    var unallocatedQuantity = totalInventoryQuantity > assignment.HandoverStockQuantity - allocatedQuantity ? assignment.HandoverStockQuantity - allocatedQuantity : totalInventoryQuantity;
                                    if (assignment.ProductionStepId == inOutGroup.ProductionStepId)
                                    {
                                        item.ReceivedQuantity += unallocatedQuantity;
                                        item.InventoryRequirementHistories.Add(inventory);
                                        isLastest = true;
                                    }
                                    else
                                    {
                                        isLastest = false;
                                    }

                                    assignment.HandoverStockQuantity -= unallocatedQuantity;
                                    totalInventoryQuantity -= unallocatedQuantity;
                                }
                                if (totalInventoryQuantity > 0 && isLastest) item.ReceivedQuantity += totalInventoryQuantity;
                            }
                        }
                    }
                    result.Add(detail);
                }
            }

            return result;
        }

        public async Task<bool> ChangeAssignedProgressStatus(string productionOrderCode, string inventoryCode, IList<ProductionInventoryRequirementEntity> inventories = null)
        {
            var productionOrder = _manufacturingDBContext.ProductionOrder
                .FirstOrDefault(po => po.ProductionOrderCode == productionOrderCode);

            if (productionOrder == null)
                throw new BadRequestException(GeneralCode.ItemNotFound, "Lệnh sản xuất không tồn tại");

            var productionAssignments = _manufacturingDBContext.ProductionAssignment
                 .Where(a => a.ProductionOrderId == productionOrder.ProductionOrderId)
                 .ToList();

            var productionStepIds = productionAssignments.Select(pa => pa.ProductionStepId).Distinct().ToList();

            var productionSteps = _manufacturingDBContext.ProductionStep.Where(ps => productionStepIds.Contains(ps.ProductionStepId)).ToList();

            var bOk = true;

            var departmentHandoverDetails = await GetDepartmentHandoverDetail(productionOrder.ProductionOrderId, null, null, inventories);
            if (departmentHandoverDetails.Count == 0) return bOk;

            var updateAssignments = new List<ProductionAssignmentEntity>();
            try
            {
                foreach (var productionAssignment in productionAssignments)
                {
                    if (productionAssignment?.AssignedProgressStatus == (int)EnumAssignedProgressStatus.Finish && productionAssignment.IsManualFinish) continue;

                    var productionStep = productionSteps.FirstOrDefault(ps => ps.ProductionStepId == productionAssignment.ProductionStepId);

                    if (productionStep == null) continue;

                    var departmentHandoverDetail = departmentHandoverDetails.FirstOrDefault(dh => dh.DepartmentId == productionAssignment.DepartmentId && dh.ProductionStepId == productionAssignment.ProductionStepId);

                    if (departmentHandoverDetail == null) continue;
                    var inoutDatas = departmentHandoverDetail.InputDatas.Union(departmentHandoverDetail.OutputDatas);
                    var status = inoutDatas.All(d => d.ReceivedQuantity >= d.RequireQuantity) ? EnumAssignedProgressStatus.Finish : EnumAssignedProgressStatus.HandingOver;

                    if (productionAssignment.AssignedProgressStatus == (int)status) continue;

                    updateAssignments.Add(productionAssignment);
                    productionAssignment.AssignedProgressStatus = (int)status;
                    productionAssignment.IsManualFinish = false;
                }
                _manufacturingDBContext.SaveChanges();

                await _activityLogService.CreateLog(EnumObjectType.ProductionAssignment, productionOrder.ProductionOrderId, $"Cập nhật trạng thái phân công sản xuất cho lệnh sản xuất {productionOrderCode} sau khi duyệt phiếu {inventoryCode}", updateAssignments.JsonSerialize());

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateProductAssignment");
                throw;
            }
            return bOk;
        }

        public async Task<bool> ChangeAssignedProgressStatus(long productionOrderId, long productionStepId, int departmentId, IList<ProductionInventoryRequirementEntity> inventories = null, IList<DepartmentHandoverDetailModel> departmentHandoverDetails = null)
        {
            var productionAssignment = _manufacturingDBContext.ProductionAssignment
                   .Where(a => a.ProductionOrderId == productionOrderId
                   && a.ProductionStepId == productionStepId
                   && a.DepartmentId == departmentId)
                   .FirstOrDefault();

            if (productionAssignment?.AssignedProgressStatus == (int)EnumAssignedProgressStatus.Finish && productionAssignment.IsManualFinish) return true;

            var productionStep = _manufacturingDBContext.ProductionStep.FirstOrDefault(ps => ps.ProductionStepId == productionStepId);

            if (productionStep == null)
            {
                throw new BadRequestException(GeneralCode.ItemNotFound, "Quy trình sản xuất đã thay đổi sau khi tạo phiếu. Vui lòng tạo lại yêu cầu xuất/nhập kho từ bàn giao/thống kê theo quy trình");
            }

            if (departmentHandoverDetails == null)
            {
                departmentHandoverDetails = (await GetDepartmentHandoverDetail(productionOrderId, productionStep.ParentId.Value, departmentId, inventories));
            }
            if (departmentHandoverDetails.Count == 0) return true;
            var departmentHandoverDetail = departmentHandoverDetails.FirstOrDefault(dh => dh.DepartmentId == departmentId && dh.ProductionStepId == productionStepId);
            if (departmentHandoverDetail == null) return true;
            var inoutDatas = departmentHandoverDetail.InputDatas.Union(departmentHandoverDetail.OutputDatas);
            var status = inoutDatas.All(d => d.ReceivedQuantity >= d.RequireQuantity) ? EnumAssignedProgressStatus.Finish : EnumAssignedProgressStatus.HandingOver;

            if (productionAssignment.AssignedProgressStatus == (int)status) return true;

            try
            {
                productionAssignment.AssignedProgressStatus = (int)status;
                productionAssignment.IsManualFinish = false;
                _manufacturingDBContext.SaveChanges();
                await _activityLogService.CreateLog(EnumObjectType.ProductionAssignment, productionOrderId, $"Cập nhật trạng thái phân công sản xuất cho lệnh sản xuất {productionOrderId}", productionAssignment.JsonSerialize());
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateProductAssignment");
                throw;
            }
        }

        public async Task<bool> UpdateFullAssignedProgressStatus(long productionOrderId)
        {
            var departmentHandoverDetails = await GetDepartmentHandoverDetail(productionOrderId);
            // Danh sách công việc
            var assignments = await _manufacturingDBContext.ProductionAssignment.Where(pa => pa.ProductionOrderId == productionOrderId).ToListAsync();

            // Đổi trạng thái SX lệnh
            // Check nhận đủ số lượng đầu ra

            bool isFinish = true;
            var productionOrder = await _manufacturingDBContext.ProductionOrder.FirstAsync(po => po.ProductionOrderId == productionOrderId);
            var productionOrderDetails = await _manufacturingDBContext.ProductionOrderDetail.Where(pod => pod.ProductionOrderId == productionOrderId).ToListAsync();
            foreach (var productionOrderDetail in productionOrderDetails)
            {
                decimal quantity = 0;
                foreach (var departmentHandoverDetail in departmentHandoverDetails)
                {
                    quantity += departmentHandoverDetail.OutputDatas
                        .Where(o => o.ObjectId == productionOrderDetail.ProductId && o.ObjectTypeId == (int)EnumProductionStepLinkDataObjectType.Product)
                        .Sum(o => o.ReceivedQuantity);
                }


                if (quantity < (productionOrderDetail.Quantity + productionOrderDetail.ReserveQuantity))
                {
                    isFinish = false;
                    break;
                }
            }

            if (isFinish)
            {
                productionOrder.ProductionOrderStatus = (int)EnumProductionStatus.Finished;
                await _activityLogService.CreateLog(EnumObjectType.ProductionOrder, productionOrder.ProductionOrderId, $"Cập nhật trạng thái lệnh sản xuất khởi tạo", new { productionOrder, isManual = false }.JsonSerialize());
            }

            // Đổi trạng thái phân công
            foreach (var assignment in assignments)
            {
                await ChangeAssignedProgressStatus(productionOrderId, assignment.ProductionStepId, assignment.DepartmentId, null, departmentHandoverDetails);
            }
            return true;
        }

        private class StepLinkDataInfo
        {
            public long ProductionStepLinkDataId { get; set; }
            public long ProductionStepId { get; set; }
            public string StepName { get; set; }
            public long? OutsourceStepRequestId { get; set; }
            public string OutsourceStepRequestCode { get; set; }
        }

        private class AssigmentLinkData
        {
            public int DepartmentId { get; set; }
            public long ProductionStepId { get; set; }
            public long ObjectId { get; set; }
            public int ProductionStepLinkDataRoleTypeId { get; set; }
            public decimal HandoverStockQuantity { get; set; }
        }
    }
}
