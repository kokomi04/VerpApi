﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verp.Cache.RedisCache;
using VErp.Commons.Enums.ErrorCodes;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Manafacturing.Model.ProductionAssignment;
using VErp.Commons.Enums.Manafacturing;
using Microsoft.Data.SqlClient;
using ProductionHandoverEntity = VErp.Infrastructure.EF.ManufacturingDB.ProductionHandover;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;
using VErp.Services.Manafacturing.Model.ProductionHandover;
using VErp.Services.Manafacturing.Model.ProductionOrder.Materials;

namespace VErp.Services.Manafacturing.Service.ProductionHandover.Implement
{
    public class ProductionHandoverService : IProductionHandoverService
    {
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private const int STOCK_DEPARTMENT_ID = -1;
        public ProductionHandoverService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<ProductionHandoverService> logger
            , IMapper mapper
            , ICustomGenCodeHelperService customGenCodeHelperService)
        {
            _manufacturingDBContext = manufacturingDB;
            _activityLogService = activityLogService;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<ProductionHandoverModel> ConfirmProductionHandover(long productionOrderId, long productionHandoverId, EnumHandoverStatus status)
        {
            var productionHandover = _manufacturingDBContext.ProductionHandover.FirstOrDefault(ho => ho.ProductionOrderId == productionOrderId && ho.ProductionHandoverId == productionHandoverId);
            if (productionHandover == null) throw new BadRequestException(GeneralCode.InvalidParams, "Bàn giao công việc không tồn tại");
            if (productionHandover.Status != (int)EnumHandoverStatus.Waiting) throw new BadRequestException(GeneralCode.InvalidParams, "Chỉ được phép xác nhận các bàn giao đang chờ xác nhận");
            try
            {
                productionHandover.Status = (int)status;
                _manufacturingDBContext.SaveChanges();

                if (productionHandover.Status == (int)EnumHandoverStatus.Accepted)
                {
                    await ChangeAssignedProgressStatus(productionOrderId, productionHandover.FromProductionStepId, productionHandover.FromDepartmentId);
                    await ChangeAssignedProgressStatus(productionOrderId, productionHandover.ToProductionStepId, productionHandover.ToDepartmentId);
                }
                await _activityLogService.CreateLog(EnumObjectType.ProductionHandover, productionHandover.ProductionHandoverId, $"Xác nhận bàn giao công việc", productionHandover.JsonSerialize());
                return _mapper.Map<ProductionHandoverModel>(productionHandover);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateProductHandover");
                throw;
            }
        }

        public async Task<ProductionHandoverModel> CreateProductionHandover(long productionOrderId, ProductionHandoverInputModel data)
        {
            return await CreateProductionHandover(productionOrderId, data, EnumHandoverStatus.Waiting);
        }

        private async Task<ProductionHandoverModel> CreateProductionHandover(long productionOrderId, ProductionHandoverInputModel data, EnumHandoverStatus status)
        {
            try
            {
                if (data.FromDepartmentId == STOCK_DEPARTMENT_ID && data.ToDepartmentId == STOCK_DEPARTMENT_ID)
                {
                    if (!_manufacturingDBContext.OutsourceStepRequestData.Any(o => o.ProductionStepId == data.FromProductionStepId))
                        throw new BadRequestException(GeneralCode.InvalidParams, "Công đoạn giao không có gia công công đoạn");
                    if (!_manufacturingDBContext.OutsourceStepRequestData.Any(o => o.ProductionStepId == data.ToProductionStepId))
                        throw new BadRequestException(GeneralCode.InvalidParams, "Công đoạn nhận không có gia công công đoạn");
                }
                else
                {
                    if (!_manufacturingDBContext.ProductionAssignment.Any(a => a.ProductionStepId == data.FromProductionStepId && a.DepartmentId == data.FromDepartmentId && a.ProductionOrderId == productionOrderId))
                        throw new BadRequestException(GeneralCode.InvalidParams, "Công đoạn giao không tồn tại phân công công việc cho tổ bàn giao");
                    if (!_manufacturingDBContext.ProductionAssignment.Any(a => a.ProductionStepId == data.ToProductionStepId && a.DepartmentId == data.ToDepartmentId && a.ProductionOrderId == productionOrderId))
                        throw new BadRequestException(GeneralCode.InvalidParams, "Công đoạn nhận không tồn tại phân công công việc cho tổ nhận");
                }

                var productionHandover = _mapper.Map<ProductionHandoverEntity>(data);
                productionHandover.Status = (int)status;
                productionHandover.ProductionOrderId = productionOrderId;
                _manufacturingDBContext.ProductionHandover.Add(productionHandover);
                _manufacturingDBContext.SaveChanges();
                if (productionHandover.Status == (int)EnumHandoverStatus.Accepted && data.FromDepartmentId != STOCK_DEPARTMENT_ID && data.ToDepartmentId != STOCK_DEPARTMENT_ID)
                {
                    await ChangeAssignedProgressStatus(productionOrderId, productionHandover.FromProductionStepId, productionHandover.FromDepartmentId);
                    await ChangeAssignedProgressStatus(productionOrderId, productionHandover.ToProductionStepId, productionHandover.ToDepartmentId);
                }
                await _activityLogService.CreateLog(EnumObjectType.ProductionHandover, productionHandover.ProductionHandoverId, $"Tạo bàn giao công việc / yêu cầu xuất kho", data.JsonSerialize());
                return _mapper.Map<ProductionHandoverModel>(productionHandover);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateProductHandover");
                throw;
            }
        }

        public async Task<bool> DeleteProductionHandover(long productionHandoverId)
        {
            try
            {
                var productionHandover = _manufacturingDBContext.ProductionHandover
                    .Where(h => h.ProductionHandoverId == productionHandoverId)
                    .FirstOrDefault();

                if (productionHandover == null)
                    throw new BadRequestException(GeneralCode.InvalidParams, "Không tồn tại bàn giao công việc");
                productionHandover.IsDeleted = true;
                _manufacturingDBContext.SaveChanges();
                if (productionHandover.Status == (int)EnumHandoverStatus.Accepted)
                {
                    await ChangeAssignedProgressStatus(productionHandover.ProductionOrderId, productionHandover.ToProductionStepId, productionHandover.ToDepartmentId);
                    await ChangeAssignedProgressStatus(productionHandover.ProductionOrderId, productionHandover.FromProductionStepId, productionHandover.FromDepartmentId);
                }
                await _activityLogService.CreateLog(EnumObjectType.ProductionHandover, productionHandoverId, $"Xoá bàn giao công việc", productionHandover.JsonSerialize());
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateProductHandover");
                throw;
            }
        }

        public async Task<ProductionHandoverModel> CreateStatictic(long productionOrderId, ProductionHandoverInputModel data)
        {
            return await CreateProductionHandover(productionOrderId, data, EnumHandoverStatus.Accepted);
        }

        public async Task<IList<ProductionHandoverModel>> GetProductionHandovers(long productionOrderId)
        {
            return await _manufacturingDBContext.ProductionHandover
                .Where(h => h.ProductionOrderId == productionOrderId)
                .ProjectTo<ProductionHandoverModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

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

        public async Task<PageData<DepartmentHandoverModel>> GetDepartmentHandovers(long departmentId, string keyword, int page, int size, long fromDate, long toDate, int? stepId, int? productId)
        {
            keyword = (keyword ?? "").Trim();
            var parammeters = new List<SqlParameter>()
            {
                new SqlParameter("@Keyword", keyword),
                new SqlParameter("@DepartmentId", departmentId),
                new SqlParameter("@Size", size),
                new SqlParameter("@Page", page),
                new SqlParameter("@FromDate", fromDate.UnixToDateTime()),
                new SqlParameter("@ToDate", toDate.UnixToDateTime()),
                new SqlParameter("@StepId", stepId.GetValueOrDefault()),
                new SqlParameter("@ProductId", productId.GetValueOrDefault())
            };

            var dataSet = await _manufacturingDBContext.ExecuteMultipleDataProcedure("asp_ProductionDepartmentHandover", parammeters.ToArray());

            var total = 0;
            IList<DepartmentHandoverModel> lst = null;
            Dictionary<string, object> additionResult = new Dictionary<string, object>();
            if (dataSet != null && dataSet.Tables.Count > 0)
            {
                IList<NonCamelCaseDictionary> data = dataSet.Tables[0].ConvertData();
                total = (data[0]["Total"] as int?).GetValueOrDefault();
                foreach (var item in dataSet.Tables[0].ConvertFirstRowData())
                {
                    additionResult.Add(item.Key, item.Value.value);
                }
                IList<DepartmentHandoverEntity> resultData = dataSet.Tables[1].ConvertData<DepartmentHandoverEntity>();
                lst = resultData.AsQueryable().ProjectTo<DepartmentHandoverModel>(_mapper.ConfigurationProvider).ToList();
            }

            return (lst, total, additionResult);
        }


        public async Task<Dictionary<long, DepartmentHandoverDetailModel>> GetDepartmentHandoverDetail(long productionOrderId, long productionStepId, long departmentId, IList<ProductionInventoryRequirementEntity> inventories = null)
        {

            var productionStep = _manufacturingDBContext.ProductionStep
                .FirstOrDefault(ps => ps.ContainerId == productionOrderId
                && ps.ProductionStepId == productionStepId
                && ps.ContainerTypeId == (int)EnumContainerType.ProductionOrder
                && ps.IsGroup.HasValue
                && ps.IsGroup.Value
                && !ps.IsFinish);

            if (productionStep == null) throw new BadRequestException(GeneralCode.InvalidParams, "Không tồn tại công đoạn");

            var groups = _manufacturingDBContext.ProductionStep
                .Where(ps => ps.ParentId == productionStepId && !ps.IsFinish)
                .ToList();

            var result = new Dictionary<long, DepartmentHandoverDetailModel>();
            var outsourceStepRequestIds = groups.Where(g => g.OutsourceStepRequestId.HasValue).Select(g => g.OutsourceStepRequestId).ToList();
            var outsourceStepRequests = _manufacturingDBContext.OutsourceStepRequest.Where(o => outsourceStepRequestIds.Contains(o.OutsourceStepRequestId)).ToList();
            var groupIds = groups.Select(g => g.ProductionStepId).ToList();

            var productionAssignments = _manufacturingDBContext.ProductionAssignment
                .Where(a => a.ProductionOrderId == productionOrderId
                && groupIds.Contains(a.ProductionStepId)
                && a.DepartmentId == departmentId)
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
                where a.DepartmentId == departmentId
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
                    a.ProductionStepId
                })
                .Select(g => new
                {
                    g.Key.DepartmentId,
                    g.Key.ProductionStepId,
                    AssignmentQuantity = g.Max(a => a.AssignmentQuantity),
                    ProductionStepLinkDataRoleTypeId = g.Max(a => a.ProductionStepLinkDataRoleTypeId),
                    TotalQuantity = g.Max(a => a.TotalQuantity),
                    ObjectId = g.Max(a => a.ObjectId),
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

            foreach (var inOutGroup in groups)
            {
                var outsourceStepRequest = outsourceStepRequests.FirstOrDefault(o => o.OutsourceStepRequestId == productionStep.OutsourceStepRequestId);
                var productionAssignment = productionAssignments
                   .Where(a => a.ProductionStepId == inOutGroup.ProductionStepId)
                   .FirstOrDefault();

                if (productionAssignment == null) throw new BadRequestException(GeneralCode.InvalidParams, "Tổ không được phân công trong công đoạn");

                var inputLinkDatas = allInputLinkDatas.Where(ld => ld.ProductionStepId == inOutGroup.ProductionStepId).Select(ld => ld.ProductionStepLinkData).ToList();

                var outputLinkDatas = allOutputLinkDatas.Where(ld => ld.ProductionStepId == inOutGroup.ProductionStepId).Select(ld => ld.ProductionStepLinkData).ToList();

                var linkDataIds = (inputLinkDatas.Select(ldr => ldr.ProductionStepLinkDataId).Concat(outputLinkDatas.Select(ldr => ldr.ProductionStepLinkDataId)).ToList());


                var stepMap = (from ldr in _manufacturingDBContext.ProductionStepLinkDataRole
                               join gps in _manufacturingDBContext.ProductionStep on ldr.ProductionStepId equals gps.ProductionStepId
                               join o in _manufacturingDBContext.OutsourceStepRequest on gps.OutsourceStepRequestId equals o.OutsourceStepRequestId into gos
                               from o in gos.DefaultIfEmpty()
                               join ps in _manufacturingDBContext.ProductionStep on gps.ParentId equals ps.ProductionStepId
                               join s in _manufacturingDBContext.Step on ps.StepId equals s.StepId
                               where !ps.IsFinish && ldr.ProductionStepId != inOutGroup.ProductionStepId && linkDataIds.Contains(ldr.ProductionStepLinkDataId)
                               select new StepLinkDateInfo
                               {
                                   ProductionStepLinkDataId = ldr.ProductionStepLinkDataId,
                                   ProductionStepId = gps.ProductionStepId,
                                   StepName = s.StepName,
                                   OutsourceStepRequestId = o.OutsourceStepRequestId,
                                   OutsourceStepRequestCode = o.OutsourceStepRequestCode
                               })
                              .ToList()
                              .GroupBy(ldr => ldr.ProductionStepLinkDataId)
                              .ToDictionary(g => g.Key, g => g.First());

                var stepIds = stepMap.Select(m => m.Value.ProductionStepId).ToList();

                var handovers = _manufacturingDBContext.ProductionHandover
                    .Where(h => h.ProductionOrderId == productionOrderId
                    && ((h.FromDepartmentId == departmentId && h.FromProductionStepId == inOutGroup.ProductionStepId) || (h.ToDepartmentId == departmentId && h.ToProductionStepId == inOutGroup.ProductionStepId)))
                    .ProjectTo<ProductionHandoverModel>(_mapper.ConfigurationProvider)
                    .ToList();


                // Lấy thông tin yêu cầu thêm
                var materialRequirements = _manufacturingDBContext.ProductionMaterialsRequirementDetail
                    .Include(mrd => mrd.ProductionMaterialsRequirement)
                    .Where(mrd => mrd.ProductionMaterialsRequirement.ProductionOrderId == productionOrderId
                    && mrd.ProductionStepId == inOutGroup.ProductionStepId
                    && mrd.DepartmentId == departmentId
                    && mrd.ProductionMaterialsRequirement.CensorStatus != (int)EnumProductionMaterialsRequirementStatus.Accepted)
                    .ProjectTo<ProductionMaterialsRequirementDetailListModel>(_mapper.ConfigurationProvider)
                    .ToList();

                var outputLink = outputLinkDatas.Where(ld => ld.ProductionStepLinkDataId == productionAssignment.ProductionStepLinkDataId).FirstOrDefault();
                var quantity = outputLink != null ? outputLink.QuantityOrigin - outputLink.OutsourcePartQuantity.GetValueOrDefault() : 0;
                if (quantity == 0) throw new BadRequestException(GeneralCode.InvalidParams, "Dữ liệu đầu ra dùng để phân công không còn tồn tại trong quy trình");

                var detail = new DepartmentHandoverDetailModel
                {
                    Assignments = _manufacturingDBContext.ProductionAssignment
                        .Where(a => stepIds.Contains(a.ProductionStepId) && a.ProductionOrderId == productionOrderId)
                        .ProjectTo<ProductionAssignmentModel>(_mapper.ConfigurationProvider)
                        .ToList()
                };
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
                            .Where(d => d.ObjectId == inputLinkData.ObjectId && d.ObjectTypeId == inputLinkData.ObjectTypeId && d.FromStepId == fromStepId && d.OutsourceStepRequestId == fromStep.OutsourceStepRequestId)
                            .FirstOrDefault();

                        if (ousourceInput != null)
                        {
                            ousourceInput.RequireQuantity += inputLinkData.QuantityOrigin - inputLinkData.OutsourcePartQuantity.GetValueOrDefault();
                            ousourceInput.TotalQuantity += inputLinkData.QuantityOrigin - inputLinkData.OutsourcePartQuantity.GetValueOrDefault();
                        }
                        else
                        {
                            var inventoryRequirementHistories = new List<ProductionInventoryRequirementModel>();
                            var materialsRequirementHistories = new List<ProductionMaterialsRequirementDetailListModel>();

                            inventoryRequirementHistories = inventoryRequirements
                                .Where(i => i.DepartmentId == departmentId
                                && i.ProductionStepId == inOutGroup.ProductionStepId
                                && i.InventoryTypeId == EnumInventoryType.Output
                                && i.ProductId == inputLinkData.ObjectId
                                && i.OutsourceStepRequestId.HasValue
                                && i.OutsourceStepRequestId.Value == fromStep.OutsourceStepRequestId)
                                .ToList();

                            materialsRequirementHistories = materialRequirements
                                .Where(mr => mr.ProductId == inputLinkData.ObjectId
                                && mr.OutsourceStepRequestId.HasValue
                                && mr.OutsourceStepRequestId.Value == fromStep.OutsourceStepRequestId)
                                .ToList();

                            var receivedQuantity = inventoryRequirementHistories.Where(h => h.Status == EnumProductionInventoryRequirementStatus.Accepted).Sum(h => h.ActualQuantity);

                            detail.InputDatas.Add(new StepInOutData
                            {
                                ObjectId = inputLinkData.ObjectId,
                                ObjectTypeId = inputLinkData.ObjectTypeId,
                                RequireQuantity = inputLinkData.QuantityOrigin - inputLinkData.OutsourcePartQuantity.GetValueOrDefault(),
                                TotalQuantity = inputLinkData.QuantityOrigin - inputLinkData.OutsourcePartQuantity.GetValueOrDefault(),
                                ReceivedQuantity = receivedQuantity,
                                FromStepTitle = $"{fromStep.StepName} - {fromStep.OutsourceStepRequestCode}",
                                FromStepId = fromStepId,
                                HandoverHistories = new List<ProductionHandoverModel>(),
                                InventoryRequirementHistories = inventoryRequirementHistories,
                                MaterialsRequirementHistories = materialsRequirementHistories,
                                OutsourceStepRequestId = fromStep.OutsourceStepRequestId
                            });
                        }
                    }

                    var item = detail.InputDatas
                        .Where(d => d.ObjectId == inputLinkData.ObjectId && d.ObjectTypeId == inputLinkData.ObjectTypeId && d.FromStepId == fromStepId && !d.OutsourceStepRequestId.HasValue)
                        .FirstOrDefault();

                    if (item != null)
                    {
                        item.RequireQuantity += inputLinkData.QuantityOrigin - inputLinkData.OutsourcePartQuantity.GetValueOrDefault() - inputLinkData.ExportOutsourceQuantity.GetValueOrDefault();
                        item.TotalQuantity += inputLinkData.QuantityOrigin - inputLinkData.OutsourcePartQuantity.GetValueOrDefault();
                    }
                    else
                    {
                        var handoverHistories = new List<ProductionHandoverModel>();
                        var inventoryRequirementHistories = new List<ProductionInventoryRequirementModel>();
                        var materialsRequirementHistories = new List<ProductionMaterialsRequirementDetailListModel>();
                        decimal receivedQuantity = 0;
                        if (fromStepId.HasValue)
                        {
                            handoverHistories = handovers.Where(h => h.ToDepartmentId == departmentId
                                && h.ToProductionStepId == inOutGroup.ProductionStepId
                                && h.ObjectId == inputLinkData.ObjectId
                                && h.ObjectTypeId == (EnumProductionStepLinkDataObjectType)inputLinkData.ObjectTypeId
                                && h.FromProductionStepId == fromStepId.Value)
                                .ToList();
                            receivedQuantity = handoverHistories.Where(h => h.Status == EnumHandoverStatus.Accepted).Sum(h => h.HandoverQuantity);
                        }
                        else
                        {
                            // Phiếu xuất kho đã phân bổ
                            inventoryRequirementHistories = inventoryRequirements.Where(h => h.DepartmentId == departmentId
                                   && h.ProductionStepId == inOutGroup.ProductionStepId
                                   && h.InventoryTypeId == EnumInventoryType.Output
                                   && h.ProductId == inputLinkData.ObjectId
                                   && !h.OutsourceStepRequestId.HasValue)
                                   .ToList();

                            materialsRequirementHistories = materialRequirements
                                .Where(mr => mr.ProductId == inputLinkData.ObjectId && !mr.OutsourceStepRequestId.HasValue)
                                .ToList();

                            receivedQuantity = handoverHistories.Where(h => h.Status == EnumHandoverStatus.Accepted).Sum(h => h.HandoverQuantity)
                                + inventoryRequirementHistories.Where(h => h.Status == EnumProductionInventoryRequirementStatus.Accepted).Sum(h => h.ActualQuantity);

                            // Xử lý các phiếu xuất kho chưa phân bổ công đoạn
                            var unallocatedInventories = inventoryRequirements.Where(h => h.DepartmentId == departmentId
                                 && h.ProductionStepId == null
                                 && h.InventoryTypeId == EnumInventoryType.Output
                                 && h.ProductId == inputLinkData.ObjectId
                                 && !h.OutsourceStepRequestId.HasValue
                                 && h.ActualQuantity > 0)
                                 .ToList();
                            foreach (var inventory in unallocatedInventories)
                            {
                                var totalInventoryQuantity = inventory.ActualQuantity;
                                bool isLastest = false;
                                foreach (var assignment in allProductionAssignments)
                                {
                                    if (totalInventoryQuantity <= 0) break;

                                    var allocatedQuantity = inventoryRequirements
                                        .Where(ir => ir.ProductionStepId.HasValue
                                            && ir.ProductionStepId.Value == assignment.ProductionStepId
                                            && ir.DepartmentId == departmentId
                                            && ir.InventoryTypeId == EnumInventoryType.Output
                                            && ir.ProductId == inputLinkData.ObjectId)
                                        .Sum(ir => ir.ActualQuantity);

                                    if (assignment.HandoverStockQuantity <= allocatedQuantity) break;
                                    if (assignment.ObjectId != inputLinkData.ObjectId
                                         || assignment.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Output) continue;


                                    var unallocatedQuantity = totalInventoryQuantity > assignment.HandoverStockQuantity - allocatedQuantity ? assignment.HandoverStockQuantity - allocatedQuantity : totalInventoryQuantity;
                                    if (assignment.ProductionStepId == inOutGroup.ProductionStepId)
                                    {
                                        receivedQuantity += unallocatedQuantity;
                                        inventoryRequirementHistories.Add(inventory);
                                        isLastest = true;
                                    }
                                    else
                                    {
                                        isLastest = false;
                                    }

                                    assignment.HandoverStockQuantity -= unallocatedQuantity;
                                    totalInventoryQuantity -= unallocatedQuantity;
                                }
                                if (totalInventoryQuantity > 0 && isLastest) receivedQuantity += totalInventoryQuantity;
                            }
                        }

                        detail.InputDatas.Add(new StepInOutData
                        {
                            ObjectId = inputLinkData.ObjectId,
                            ObjectTypeId = inputLinkData.ObjectTypeId,
                            RequireQuantity = inputLinkData.QuantityOrigin - inputLinkData.OutsourcePartQuantity.GetValueOrDefault(),
                            TotalQuantity = inputLinkData.QuantityOrigin - inputLinkData.OutsourcePartQuantity.GetValueOrDefault(),
                            ReceivedQuantity = receivedQuantity,
                            FromStepTitle = fromStepId.HasValue ? $"{fromStep.StepName}" : "Kho",
                            FromStepId = fromStepId,
                            HandoverHistories = handoverHistories,
                            InventoryRequirementHistories = inventoryRequirementHistories,
                            MaterialsRequirementHistories = materialsRequirementHistories
                        });
                    }
                }


                // Tính toán khối lượng đầu vào theo phân công công việc
                foreach (var inputData in detail.InputDatas)
                {
                    inputData.RequireQuantity = inputData.RequireQuantity * productionAssignment.AssignmentQuantity / quantity;
                    inputData.TotalQuantity = inputData.TotalQuantity * productionAssignment.AssignmentQuantity / quantity;
                }

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
                            .Where(d => d.ObjectId == outputLinkData.ObjectId && d.ObjectTypeId == outputLinkData.ObjectTypeId && d.ToStepId == toStepId && d.OutsourceStepRequestId == toStep.OutsourceStepRequestId)
                            .FirstOrDefault();

                        if (ousourceOutput != null)
                        {
                            ousourceOutput.RequireQuantity += outputLinkData.QuantityOrigin - outputLinkData.OutsourcePartQuantity.GetValueOrDefault();
                            ousourceOutput.TotalQuantity += outputLinkData.QuantityOrigin - outputLinkData.OutsourcePartQuantity.GetValueOrDefault();
                        }
                        else
                        {
                            var inventoryRequirementHistories = new List<ProductionInventoryRequirementModel>();

                            inventoryRequirementHistories = inventoryRequirements
                                .Where(i => i.DepartmentId == departmentId
                                && i.ProductionStepId == inOutGroup.ProductionStepId
                                && i.InventoryTypeId == EnumInventoryType.Input
                                && i.ProductId == outputLinkData.ObjectId
                                && i.OutsourceStepRequestId.HasValue
                                && i.OutsourceStepRequestId.Value == toStep.OutsourceStepRequestId)
                                .ToList();

                            var receivedQuantity = inventoryRequirementHistories.Where(h => h.Status == EnumProductionInventoryRequirementStatus.Accepted).Sum(h => h.ActualQuantity);

                            detail.OutputDatas.Add(new StepInOutData
                            {
                                ObjectId = outputLinkData.ObjectId,
                                ObjectTypeId = outputLinkData.ObjectTypeId,
                                RequireQuantity = outputLinkData.QuantityOrigin - outputLinkData.OutsourcePartQuantity.GetValueOrDefault(),
                                TotalQuantity = outputLinkData.QuantityOrigin - outputLinkData.OutsourcePartQuantity.GetValueOrDefault(),
                                ReceivedQuantity = receivedQuantity,
                                ToStepTitle = $"{toStep.StepName}(#{toStep.ProductionStepId}) - {toStep.OutsourceStepRequestCode}",
                                ToStepId = toStepId,
                                HandoverHistories = new List<ProductionHandoverModel>(),
                                InventoryRequirementHistories = inventoryRequirementHistories,
                                MaterialsRequirementHistories = new List<ProductionMaterialsRequirementDetailListModel>(),
                                OutsourceStepRequestId = toStep.OutsourceStepRequestId
                            });
                        }
                    }

                    var item = detail.OutputDatas
                        .Where(d => d.ObjectId == outputLinkData.ObjectId && d.ObjectTypeId == outputLinkData.ObjectTypeId && d.ToStepId == toStepId && !d.OutsourceStepRequestId.HasValue)
                        .FirstOrDefault();

                    if (item != null)
                    {
                        item.RequireQuantity += outputLinkData.QuantityOrigin - outputLinkData.OutsourcePartQuantity.GetValueOrDefault();
                        item.TotalQuantity += outputLinkData.QuantityOrigin - outputLinkData.OutsourcePartQuantity.GetValueOrDefault();
                    }
                    else
                    {
                        var handoverHistories = new List<ProductionHandoverModel>();
                        var inventoryRequirementHistories = new List<ProductionInventoryRequirementModel>();
                        var materialsRequirementHistories = new List<ProductionMaterialsRequirementDetailListModel>();
                        decimal receivedQuantity = 0;
                        if (toStepId.HasValue)
                        {
                            handoverHistories = handovers.Where(h => h.FromDepartmentId == departmentId
                                && h.FromProductionStepId == inOutGroup.ProductionStepId
                                && h.ObjectId == outputLinkData.ObjectId
                                && h.ObjectTypeId == (EnumProductionStepLinkDataObjectType)outputLinkData.ObjectTypeId
                                && h.ToProductionStepId == toStepId.Value)
                                .ToList();

                            receivedQuantity = handoverHistories.Where(h => h.Status == EnumHandoverStatus.Accepted).Sum(h => h.HandoverQuantity);
                        }
                        else
                        {
                            inventoryRequirementHistories = inventoryRequirements
                                .Where(h => h.DepartmentId == departmentId
                                && h.ProductionStepId == inOutGroup.ProductionStepId
                                && h.InventoryTypeId == EnumInventoryType.Input
                                && h.ProductId == outputLinkData.ObjectId
                                && !h.OutsourceStepRequestId.HasValue)
                                .ToList();


                            // Xử lý các phiếu xuất kho chưa phân bổ công đoạn
                            inventoryRequirementHistories.AddRange(inventoryRequirements.Where(h => h.DepartmentId == departmentId
                                && h.ProductionStepId == null
                                && h.InventoryTypeId == EnumInventoryType.Input
                                && h.ProductId == outputLinkData.ObjectId
                                && !h.OutsourceStepRequestId.HasValue)
                                .ToList());

                            receivedQuantity = handoverHistories.Where(h => h.Status == EnumHandoverStatus.Accepted).Sum(h => h.HandoverQuantity)
                            + inventoryRequirementHistories.Where(h => h.Status == EnumProductionInventoryRequirementStatus.Accepted).Sum(h => h.ActualQuantity);

                            // Xử lý các phiếu xuất kho chưa phân bổ công đoạn
                            var unallocatedInventories = inventoryRequirements.Where(h => h.DepartmentId == departmentId
                                 && h.ProductionStepId == null
                                 && h.InventoryTypeId == EnumInventoryType.Input
                                 && h.ProductId == outputLinkData.ObjectId
                                 && !h.OutsourceStepRequestId.HasValue
                                 && h.ActualQuantity > 0)
                                 .ToList();
                            foreach (var inventory in unallocatedInventories)
                            {
                                var totalInventoryQuantity = inventory.ActualQuantity;
                                bool isLastest = false;
                                foreach (var assignment in allProductionAssignments)
                                {
                                    if (totalInventoryQuantity <= 0) break;

                                    var allocatedQuantity = inventoryRequirements
                                        .Where(ir => ir.ProductionStepId.HasValue
                                            && ir.ProductionStepId.Value == assignment.ProductionStepId
                                            && ir.DepartmentId == departmentId
                                            && ir.InventoryTypeId == EnumInventoryType.Input
                                            && ir.ProductId == outputLinkData.ObjectId)
                                        .Sum(ir => ir.ActualQuantity);

                                    if (assignment.HandoverStockQuantity <= allocatedQuantity) break;
                                    if (assignment.ObjectId != outputLinkData.ObjectId
                                         || assignment.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Input) continue;


                                    var unallocatedQuantity = totalInventoryQuantity > assignment.HandoverStockQuantity - allocatedQuantity ? assignment.HandoverStockQuantity - allocatedQuantity : totalInventoryQuantity;
                                    if (assignment.ProductionStepId == inOutGroup.ProductionStepId)
                                    {
                                        receivedQuantity += unallocatedQuantity;
                                        inventoryRequirementHistories.Add(inventory);
                                        isLastest = true;
                                    }
                                    else
                                    {
                                        isLastest = false;
                                    }

                                    assignment.HandoverStockQuantity -= unallocatedQuantity;
                                    totalInventoryQuantity -= unallocatedQuantity;
                                }
                                if (totalInventoryQuantity > 0 && isLastest) receivedQuantity += totalInventoryQuantity;
                            }
                        }



                        detail.OutputDatas.Add(new StepInOutData
                        {
                            ObjectId = outputLinkData.ObjectId,
                            ObjectTypeId = outputLinkData.ObjectTypeId,
                            RequireQuantity = outputLinkData.QuantityOrigin - outputLinkData.OutsourcePartQuantity.GetValueOrDefault(),
                            TotalQuantity = outputLinkData.QuantityOrigin - outputLinkData.OutsourcePartQuantity.GetValueOrDefault(),
                            ReceivedQuantity = receivedQuantity,
                            ToStepTitle = toStepId.HasValue ? $"{toStep.StepName}(#{toStep.ProductionStepId})" : "Kho",
                            ToStepId = toStepId,
                            HandoverHistories = handoverHistories,
                            InventoryRequirementHistories = inventoryRequirementHistories,
                            MaterialsRequirementHistories = materialsRequirementHistories
                        });
                    }
                }

                // Tính toán khối lượng đầu ra theo phân công công việc
                foreach (var outputData in detail.OutputDatas)
                {
                    outputData.RequireQuantity = outputData.RequireQuantity * productionAssignment.AssignmentQuantity / quantity;
                    outputData.TotalQuantity = outputData.TotalQuantity * productionAssignment.AssignmentQuantity / quantity;
                }

                result.Add(inOutGroup.ProductionStepId, detail);
            }

            return result;
        }

        public async Task<bool> ChangeAssignedProgressStatus(string productionOrderCode, int departmentId, IList<ProductionInventoryRequirementEntity> inventories = null)
        {
            var productionOrder = _manufacturingDBContext.ProductionOrder
                .FirstOrDefault(po => po.ProductionOrderCode == productionOrderCode);

            if (productionOrder == null)
                throw new BadRequestException(GeneralCode.ItemNotFound, "Lệnh sản xuất không tồn tại");

            var productionStepIds = _manufacturingDBContext.ProductionAssignment
                 .Where(a => a.ProductionOrderId == productionOrder.ProductionOrderId && a.DepartmentId == departmentId)
                 .Select(a => a.ProductionStepId)
                 .ToList();
            var bOk = true;
            foreach (var productionStepId in productionStepIds)
            {
                bOk = bOk && (await ChangeAssignedProgressStatus(productionOrder.ProductionOrderId, productionStepId, departmentId, inventories));
            }

            return bOk;

        }

        private async Task<bool> ChangeAssignedProgressStatus(long productionOrderId, long productionStepId, int departmentId, IList<ProductionInventoryRequirementEntity> inventories = null)
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

            var departmentHandoverDetail = (await GetDepartmentHandoverDetail(productionOrderId, productionStep.ParentId.Value, departmentId, inventories))[productionStepId];
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

        private class StepLinkDateInfo
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
