using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Manafacturing.Model.ProductionHandover;
using VErp.Services.Manafacturing.Model.ProductionOrder;
using VErp.Services.Manafacturing.Model.ProductionPlan;
using VErp.Services.Manafacturing.Model.WorkloadPlanModel;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;
using ProductSemiEntity = VErp.Infrastructure.EF.ManufacturingDB.ProductSemi;
using ProductionOrderEntity = VErp.Infrastructure.EF.ManufacturingDB.ProductionOrder;

namespace VErp.Services.Manafacturing.Service.ProductionPlan.Implement
{
    public class ProductionPlanService : IProductionPlanService
    {
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IProductHelperService _productHelperService;
        private readonly IProductCateHelperService _productCateHelperService;
        private readonly IProductBomHelperService _productBomHelperService;
        private readonly IVoucherTypeHelperService _voucherTypeHelperService;
        private readonly ICurrentContextService _currentContext;
        private readonly IOrganizationHelperService _organizationHelperService;
        private readonly IPhysicalFileService _physicalFileService;
        private readonly AppSetting _appSetting;
        public ProductionPlanService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<ProductionPlanService> logger
            , IMapper mapper
            , IProductHelperService productHelperService
            , IProductCateHelperService productCateHelperService
            , IProductBomHelperService productBomHelperService
            , IVoucherTypeHelperService voucherTypeHelperService
            , ICurrentContextService currentContext
            , IOrganizationHelperService organizationHelperService
            , IPhysicalFileService physicalFileService
            , IOptions<AppSetting> appSetting)
        {
            _manufacturingDBContext = manufacturingDB;
            _activityLogService = activityLogService;
            _logger = logger;
            _mapper = mapper;
            _productHelperService = productHelperService;
            _productCateHelperService = productCateHelperService;
            _productBomHelperService = productBomHelperService;
            _voucherTypeHelperService = voucherTypeHelperService;
            _currentContext = currentContext;
            _organizationHelperService = organizationHelperService;
            _physicalFileService = physicalFileService;
            _appSetting = appSetting.Value;
        }

        public async Task<IDictionary<long, List<ProductionWeekPlanModel>>> GetProductionPlan(int? monthPlanId, int? factoryDepartmentId, long startDate, long endDate)
        {

            IQueryable<ProductionOrderDetail> productionOrderDetails;
            if (monthPlanId > 0)
            {
                productionOrderDetails = _manufacturingDBContext.ProductionOrderDetail
                   .Include(pod => pod.ProductionOrder)
                   .Where(pod => pod.ProductionOrder.MonthPlanId == monthPlanId);
            }
            else
            {
                productionOrderDetails = _manufacturingDBContext.ProductionOrderDetail
                   .Include(pod => pod.ProductionOrder)
                   .Where(pod => pod.ProductionOrder.StartDate <= endDate.UnixToDateTime() && pod.ProductionOrder.EndDate >= startDate.UnixToDateTime());

            }

            if (factoryDepartmentId > 0)
            {
                productionOrderDetails = productionOrderDetails.Where(pod => pod.ProductionOrder.FactoryDepartmentId == factoryDepartmentId);
            }

            IQueryable<long> productionOrderDetailIds = productionOrderDetails.Select(pod => pod.ProductionOrderDetailId);

            var productionPlans = await _manufacturingDBContext.ProductionWeekPlan
                .Include(p => p.ProductionWeekPlanDetail)
                .Where(p => productionOrderDetailIds.Contains(p.ProductionOrderDetailId))
                .ToListAsync();

            var result = productionPlans
                .GroupBy(p => p.ProductionOrderDetailId)
                .ToDictionary(g => g.Key, g => g.AsQueryable().ProjectTo<ProductionWeekPlanModel>(_mapper.ConfigurationProvider).ToList());

            return result;
        }

        public async Task<IDictionary<long, List<ProductionWeekPlanModel>>> UpdateProductionPlan(IDictionary<long, List<ProductionWeekPlanModel>> data)
        {

            using var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                var productionOrderDetailIds = data.Select(d => d.Key).ToList();
                var productionOrderDetails = _manufacturingDBContext.ProductionOrderDetail
                    .Where(pod => productionOrderDetailIds.Contains(pod.ProductionOrderDetailId))
                    .ToList();
                if (productionOrderDetails.Count != productionOrderDetailIds.Count)
                    throw new BadRequestException(GeneralCode.InvalidParams, "Chi tiết lệnh sản xuất không tồn tại");

                var allProductionWeekPlans = _manufacturingDBContext.ProductionWeekPlan
                        .Where(p => productionOrderDetailIds.Contains(p.ProductionOrderDetailId))
                        .ToList();
                var allProductionWeekPlanIds = allProductionWeekPlans.Select(p => p.ProductionWeekPlanId).ToList();
                var allProductionWeekPlanDetails = _manufacturingDBContext.ProductionWeekPlanDetail
                     .Where(pd => allProductionWeekPlanIds.Contains(pd.ProductionWeekPlanId))
                     .ToList();

                foreach (var item in data)
                {
                    var productionOrderDetailId = item.Key;
                    var productionOrderDetail = productionOrderDetails.First(pod => pod.ProductionOrderDetailId == productionOrderDetailId);
                    var currentProductionWeekPlans = allProductionWeekPlans
                        .Where(p => p.ProductionOrderDetailId == productionOrderDetailId)
                        .ToList();

                    foreach (var productionWeekPlanModel in item.Value)
                    {
                        var currentProductionWeekPlan = currentProductionWeekPlans
                            .FirstOrDefault(cp => cp.StartDate == productionWeekPlanModel.StartDate.UnixToDateTime());
                        if (currentProductionWeekPlan == null)
                        {
                            // Tạo mới
                            currentProductionWeekPlan = _mapper.Map<ProductionWeekPlan>(productionWeekPlanModel);
                            currentProductionWeekPlan.ProductionOrderDetailId = productionOrderDetailId;
                            _manufacturingDBContext.ProductionWeekPlan.Add(currentProductionWeekPlan);
                            _manufacturingDBContext.SaveChanges();
                        }
                        else if (currentProductionWeekPlan.ProductQuantity != productionWeekPlanModel.ProductQuantity)
                        {
                            // Update
                            currentProductionWeekPlan.ProductQuantity = productionWeekPlanModel.ProductQuantity;
                        }
                        // Cập nhât detail
                        if (currentProductionWeekPlan != null)
                        {
                            // Xóa dữ liệu cũ
                            var currentProductionWeekPlanDetails = allProductionWeekPlanDetails.Where(pd => pd.ProductionWeekPlanId == currentProductionWeekPlan.ProductionWeekPlanId).ToList();
                            _manufacturingDBContext.ProductionWeekPlanDetail.RemoveRange(currentProductionWeekPlanDetails);
                        }
                        foreach (var detail in productionWeekPlanModel.ProductionWeekPlanDetail)
                        {
                            var productionWeekPlanDetail = _mapper.Map<ProductionWeekPlanDetail>(detail);
                            productionWeekPlanDetail.ProductionWeekPlanId = currentProductionWeekPlan.ProductionWeekPlanId;
                            _manufacturingDBContext.ProductionWeekPlanDetail.AddRange(productionWeekPlanDetail);
                        }

                    }


                    // Xóa kế hoạch tuần 
                    var deleteProductionWeekPlans = currentProductionWeekPlans.Where(cp => !item.Value.Any(p => p.StartDate.UnixToDateTime() == cp.StartDate)).ToList();
                    var deleteProductionWeekPlanIds = deleteProductionWeekPlans.Select(p => p.ProductionWeekPlanId).ToList();
                    var deleteProductionWeekPlanDetails = allProductionWeekPlanDetails.Where(pd => deleteProductionWeekPlanIds.Contains(pd.ProductionWeekPlanId)).ToList();

                    _manufacturingDBContext.ProductionWeekPlanDetail.RemoveRange(deleteProductionWeekPlanDetails);
                    _manufacturingDBContext.ProductionWeekPlan.RemoveRange(deleteProductionWeekPlans);

                    _manufacturingDBContext.SaveChanges();
                }

                // Map valid
                var productionOrderIds = productionOrderDetails.Select(pod => pod.ProductionOrderId).Distinct().ToList();
                var productionOrders = _manufacturingDBContext.ProductionOrder.Where(po => productionOrderIds.Contains(po.ProductionOrderId)).ToList();
                foreach (var productionOrder in productionOrders)
                {
                    productionOrder.InvalidPlan = false;
                }
                _manufacturingDBContext.SaveChanges();
                trans.Commit();

                foreach (var item in data)
                {
                    var productionOrderDetailId = item.Key;
                    var productionOrderDetail = productionOrderDetails.First(pod => pod.ProductionOrderDetailId == productionOrderDetailId);
                    var productionOrder = productionOrders.Find(po => po.ProductionOrderId == productionOrderDetail.ProductionOrderId);
                    await _activityLogService.CreateLog(EnumObjectType.ProductionPlan, productionOrderDetail.ProductionOrderId, $"Cập nhật dữ liệu kế hoạch tuần cho lệnh {productionOrder.ProductionOrderCode}", data.JsonSerialize());
                }

                var productionPlans = await _manufacturingDBContext.ProductionWeekPlan
                    .Include(p => p.ProductionWeekPlanDetail)
                    .Where(p => productionOrderDetailIds.Contains(p.ProductionOrderDetailId))
                    .ToListAsync();

                var result = productionPlans
                    .GroupBy(p => p.ProductionOrderDetailId)
                    .ToDictionary(g => g.Key, g => g.AsQueryable().ProjectTo<ProductionWeekPlanModel>(_mapper.ConfigurationProvider).ToList());

                return data;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "UpdateProductPlan");
                throw;
            }
        }

        public async Task<bool> DeleteProductionPlan(long productionOrderId)
        {
            using var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                var productionOrder = _manufacturingDBContext.ProductionOrder
                    .Include(po => po.ProductionOrderDetail)
                    .Where(po => po.ProductionOrderId == productionOrderId).FirstOrDefault();
                if (productionOrder == null) throw new BadRequestException(GeneralCode.InvalidParams, "Lệnh sản xuất không tồn tại");

                var productionOrderDetailIds = productionOrder.ProductionOrderDetail.Select(pod => pod.ProductionOrderDetailId).ToList();

                var currentProductionWeekPlans = _manufacturingDBContext.ProductionWeekPlan
                    .Where(p => productionOrderDetailIds.Contains(p.ProductionOrderDetailId))
                    .ToList();

                var currentProductionWeekPlanIds = currentProductionWeekPlans.Select(p => p.ProductionWeekPlanId).ToList();
                var currentProductionWeekPlanDetails = _manufacturingDBContext.ProductionWeekPlanDetail
                    .Where(pd => currentProductionWeekPlanIds.Contains(pd.ProductionWeekPlanId))
                    .ToList();

                _manufacturingDBContext.ProductionWeekPlanDetail.RemoveRange(currentProductionWeekPlanDetails);
                _manufacturingDBContext.ProductionWeekPlan.RemoveRange(currentProductionWeekPlans);

                await _activityLogService.CreateLog(EnumObjectType.ProductionPlan, productionOrderId, $"Xóa dữ liệu kế hoạch tuần cho lệnh {productionOrder.ProductionOrderCode}", currentProductionWeekPlans.JsonSerialize());
                return true;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "DeleteProductWeekPlan");
                throw;
            }
        }

        public async Task<(Stream stream, string fileName, string contentType)> ProductionPlanExport(int? monthPlanId, int? factoryDepartmentId, long startDate, long endDate, ProductionPlanExportModel data, IList<string> mappingFunctionKeys = null)
        {
            var productionPlanExport = new ProductionPlanExportFacade();
            productionPlanExport.SetProductionPlanService(this);
            productionPlanExport.SetProductHelperService(_productHelperService);
            productionPlanExport.SetProductCateHelperService(_productCateHelperService);
            productionPlanExport.SetProductBomHelperService(_productBomHelperService);
            productionPlanExport.SetCurrentContextService(_currentContext);
            productionPlanExport.SetVoucherTypeHelperService(_voucherTypeHelperService);
            productionPlanExport.SetOrganizationHelperService(_organizationHelperService);
            productionPlanExport.SetPhysicalFileService(_physicalFileService);
            productionPlanExport.SetAppSetting(_appSetting);
            productionPlanExport.SetManufacturingDBContext(_manufacturingDBContext);
            return await productionPlanExport.Export(monthPlanId, factoryDepartmentId, startDate, endDate, data, mappingFunctionKeys);

        }


        public async Task<(Stream stream, string fileName, string contentType)> ProductionWorkloadPlanExport(int monthPlanId, int? factoryDepartmentId, long startDate, long endDate, string monthPlanName, IList<string> mappingFunctionKeys = null)
        {
            var groupSteps = _manufacturingDBContext.StepGroup.ToList();
            var steps = _manufacturingDBContext.Step.ToList();
            var extraInfos = await _manufacturingDBContext.ProductionPlanExtraInfo
                .Where(d => d.MonthPlanId == monthPlanId)
                .ProjectTo<ProductionPlanExtraInfoModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

            var productionPlanExport = new ProductionPlanExportFacade();
            productionPlanExport.SetProductionPlanService(this);
            productionPlanExport.SetProductHelperService(_productHelperService);
            productionPlanExport.SetProductCateHelperService(_productCateHelperService);
            productionPlanExport.SetProductBomHelperService(_productBomHelperService);
            productionPlanExport.SetCurrentContextService(_currentContext);
            productionPlanExport.SetStepInfo(groupSteps, steps);



            return await productionPlanExport.WorkloadExport(monthPlanId, factoryDepartmentId, startDate, endDate, monthPlanName, extraInfos, mappingFunctionKeys);
        }

        public List<ProductSemiEntity> GetProductSemis(List<long> productSemiIds)
        {
            return _manufacturingDBContext.ProductSemi.Where(ps => productSemiIds.Contains(ps.ProductSemiId)).ToList();
        }


        public async Task<IList<ProductionOrderListModel>> GetProductionPlans(int? monthPlanId, int? factoryDepartmentId, long startDate, long endDate)
        {

            var parammeters = new List<SqlParameter>();

            var sql = new StringBuilder(
                @$";WITH tmp AS (
                    SELECT ");


            sql.Append($"ROW_NUMBER() OVER (ORDER BY g.Date) AS RowNum,");


            sql.Append(@" g.ProductionOrderId
                        FROM(
                            SELECT * FROM vProductionOrderDetail v");

            var condition = new StringBuilder();
            if (monthPlanId > 0)
            {
                condition.Append("v.MonthPlanId = @MonthPlanId");
                parammeters.Add(new SqlParameter("@MonthPlanId", monthPlanId));
            }
            else
            {
                if (startDate > 0 && endDate > 0)
                {
                    condition.Append("v.StartDate <= @ToDate AND v.PlanEndDate >= @FromDate");
                    parammeters.Add(new SqlParameter("@FromDate", startDate.UnixToDateTime()));
                    parammeters.Add(new SqlParameter("@ToDate", endDate.UnixToDateTime()));
                }
            }

            if (factoryDepartmentId > 0)
            {
                if (condition.Length > 0)
                    condition.Append(" AND ");
                condition.Append("v.FactoryDepartmentId = @FactoryDepartmentId");
                parammeters.Add(new SqlParameter("@FactoryDepartmentId", factoryDepartmentId));
            }

            if (condition.Length > 0)
            {
                sql.Append(" WHERE ");
                sql.Append(condition);
            }


            sql.Append(
                   @") g
	                GROUP BY g.ProductionOrderCode, g.ProductionOrderId, g.Date, g.StartDate, g.EndDate, g.ProductionOrderStatus ");


            sql.Append(@")
                SELECT v.* FROM tmp t
                LEFT JOIN vProductionOrderDetail v ON t.ProductionOrderId = v.ProductionOrderId ");

            sql.Append(" ORDER BY t.RowNum");

            var resultData = await _manufacturingDBContext.QueryDataTableRaw(sql.ToString(), parammeters.Select(p => p.CloneSqlParam()).ToArray());
            var lst = resultData.ConvertData<ProductionOrderListEntity>().AsQueryable().ProjectTo<ProductionOrderListModel>(_mapper.ConfigurationProvider).ToList();

            return lst;
        }

        public async Task<IDictionary<long, WorkloadPlanModel>> GetWorkloadPlanByDate(int? monthPlanId, int? factoryDepartmentId, long startDate, long endDate)
        {

            IQueryable<ProductionOrderEntity> productionOrders;
            if (monthPlanId > 0)
            {
                productionOrders = _manufacturingDBContext.ProductionOrder
                  .Where(po => po.MonthPlanId == monthPlanId);

            }
            else
            {
                var startDateTime = startDate.UnixToDateTime();
                var endDateTime = endDate.UnixToDateTime();

                productionOrders = _manufacturingDBContext.ProductionOrder
                   .Where(po => po.PlanEndDate >= startDateTime && po.StartDate <= endDateTime);

            }

            if (factoryDepartmentId > 0)
            {
                productionOrders = productionOrders.Where(o => o.FactoryDepartmentId == factoryDepartmentId);
            }

            IList<long> productionOrderIds = productionOrders.Select(po => po.ProductionOrderId)
                   .ToList();
            return await GetWorkloadPlan(productionOrderIds);
        }


        public async Task<IDictionary<long, WorkloadPlanModel>> GetWorkloadPlan(IList<long> productionOrderIds)
        {
            var result = new Dictionary<long, WorkloadPlanModel>();
            productionOrderIds = productionOrderIds.Distinct().ToList();

            var productionSteps = await _manufacturingDBContext.ProductionStep
               .Where(ps => productionOrderIds.Contains(ps.ContainerId)
               && ps.ContainerTypeId == (int)EnumContainerType.ProductionOrder
               && ps.StepId.HasValue
               && ps.IsGroup.HasValue
               && !ps.IsFinish)
               .ToListAsync();

            if (productionSteps.Count == 0) return result;
            var parentIds = productionSteps.Select(ps => ps.ProductionStepId).ToList();

            var groups = _manufacturingDBContext.ProductionStep
                .Where(ps => ps.ParentId.HasValue && parentIds.Contains(ps.ParentId.Value) && !ps.IsFinish)
                .ToList();

            var outsourceStepRequestIds = groups.Where(g => g.OutsourceStepRequestId.HasValue).Select(g => g.OutsourceStepRequestId).ToList();
            var outsourceStepRequests = _manufacturingDBContext.OutsourceStepRequest.Where(o => outsourceStepRequestIds.Contains(o.OutsourceStepRequestId)).ToList();
            var groupIds = groups.Select(g => g.ProductionStepId).ToList();

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

            foreach (var productionOrderId in productionOrderIds)
            {
                var workloadPlanModel = new WorkloadPlanModel();

                var productionGroup = groups.Where(g => g.ContainerId == productionOrderId).ToList();

                foreach (var inOutGroup in productionGroup)
                {
                    var parentStep = productionSteps.First(ps => ps.ProductionStepId == inOutGroup.ParentId);

                    if (!workloadPlanModel.WorkloadOutput.ContainsKey(parentStep.StepId.Value))
                    {
                        workloadPlanModel.WorkloadOutput.Add(parentStep.StepId.Value, new List<WorkloadOutputModel>());
                    }

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

                    var outputDatas = new List<StepInOutData>();

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
                                .Where(d => d.ObjectId == outputLinkData.LinkDataObjectId
                                && (int)d.ObjectTypeId == outputLinkData.LinkDataObjectTypeId
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
                                    ObjectId = outputLinkData.LinkDataObjectId,
                                    ObjectTypeId = outputLinkData.LinkDataObjectTypeId,
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
                            .Where(d => d.ObjectId == outputLinkData.LinkDataObjectId
                            && d.ObjectTypeId == outputLinkData.LinkDataObjectTypeId
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
                                ObjectId = outputLinkData.LinkDataObjectId,
                                ObjectTypeId = outputLinkData.LinkDataObjectTypeId,
                                RequireQuantity = outputLinkData.QuantityOrigin - outputLinkData.OutsourcePartQuantity.GetValueOrDefault(),
                                TotalQuantity = outputLinkData.QuantityOrigin - outputLinkData.OutsourcePartQuantity.GetValueOrDefault(),
                                ReceivedQuantity = 0,
                                ToStepTitle = toStepId.HasValue ? $"{toStep.StepName}(#{toStep.ProductionStepId})" : "Kho",
                                ToStepId = toStepId,
                            });
                        }
                    }

                    foreach (var outputData in outputDatas)
                    {
                        var workloadOutput = workloadPlanModel.WorkloadOutput[parentStep.StepId.Value]
                            .FirstOrDefault(wo => wo.ObjectId == outputData.ObjectId && wo.ObjectTypeId == outputData.ObjectTypeId);
                        if (workloadOutput == null)
                        {
                            workloadOutput = new WorkloadOutputModel
                            {
                                ObjectId = outputData.ObjectId,
                                ObjectTypeId = outputData.ObjectTypeId,
                                Quantity = outputData.TotalQuantity,
                            };
                            workloadPlanModel.WorkloadOutput[parentStep.StepId.Value].Add(workloadOutput);
                        }
                        else
                        {
                            workloadOutput.Quantity += outputData.TotalQuantity;
                        }
                    }
                }


                result.Add(productionOrderId, workloadPlanModel);
            }

            return result;
        }

        public async Task<IDictionary<long, List<ImportProductModel>>> GetMonthlyImportStock(int monthPlanId)
        {
            var monthPlan = _manufacturingDBContext.MonthPlan.FirstOrDefault(p => p.MonthPlanId == monthPlanId);
            if (monthPlan == null) throw new BadRequestException(GeneralCode.InvalidParams, "Không tồn tại kế hoạch tháng");

            var lastestDate = DateTime.UtcNow.Date;

            if (lastestDate > monthPlan.EndDate) lastestDate = monthPlan.EndDate;
            if (lastestDate < monthPlan.StartDate) lastestDate = monthPlan.StartDate;

            var currentWeekPlan = _manufacturingDBContext.WeekPlan.Where(w => w.StartDate <= lastestDate).OrderBy(w => w.StartDate).LastOrDefault();
            if (currentWeekPlan == null) throw new BadRequestException(GeneralCode.InvalidParams, "Không tồn tại kế hoạch tuần");


            var productionOrderDetails = _manufacturingDBContext.ProductionOrderDetail
                .Include(pd => pd.ProductionOrder)
                .Where(pd => pd.ProductionOrder.MonthPlanId == monthPlanId)
                .ToList();

            var productOderIds = productionOrderDetails.Select(pd => pd.ProductionOrderId).Distinct().ToList();

            var groupSteps = (await (from po in _manufacturingDBContext.ProductionOrder
                                     join ps in _manufacturingDBContext.ProductionStep on new { po.ProductionOrderId, ContainerTypeId = (int)EnumContainerType.ProductionOrder } equals new { ProductionOrderId = ps.ContainerId, ps.ContainerTypeId }
                                     join s in _manufacturingDBContext.Step on ps.StepId equals s.StepId
                                     join sg in _manufacturingDBContext.StepGroup on s.StepGroupId equals sg.StepGroupId
                                     where productOderIds.Contains(po.ProductionOrderId)
                                     select new
                                     {
                                         sg.StepGroupId,
                                         po.ProductionOrderId
                                     }).ToListAsync())
                             .GroupBy(sg => sg.StepGroupId)
                             .ToDictionary(g => g.Key, g => g.Select(sg => sg.ProductionOrderId).Distinct().ToList());


            var finishStepIds = _manufacturingDBContext.ProductionStep
                 .Where(ps => ps.ContainerTypeId == (int)EnumContainerType.ProductionOrder
                 && productOderIds.Contains(ps.ContainerId)
                 && ps.IsFinish
                 && !ps.IsGroup.Value)
                 .Select(ps => ps.ProductionStepId)
                 .Distinct()
                 .ToList();

            var linkDataIds = _manufacturingDBContext.ProductionStepLinkDataRole
                .Where(lr => lr.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Input && finishStepIds.Contains(lr.ProductionStepId))
                .Select(lr => lr.ProductionStepLinkDataId)
                .Distinct()
                .ToList();

            var lastestStepIds = _manufacturingDBContext.ProductionStepLinkDataRole
                .Where(lr => lr.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Output && linkDataIds.Contains(lr.ProductionStepLinkDataId))
                .Select(lr => lr.ProductionStepId)
                .Distinct()
                .ToList();

            var importStockObjects = _manufacturingDBContext.ProductionHistory
                .Where(ph => lastestStepIds.Contains(ph.ProductionStepId) && ph.ObjectTypeId == (int)EnumProductionStepLinkDataObjectType.Product)
                .ToList();

            var result = new Dictionary<long, List<ImportProductModel>>();

            foreach (var groupStep in groupSteps)
            {
                result.Add(groupStep.Key, new List<ImportProductModel>());

                foreach (var productionOrderId in groupStep.Value)
                {

                    var productDatas = productionOrderDetails
                        .Where(pod => pod.ProductionOrderId == productionOrderId)
                        .GroupBy(pod => new { pod.ProductId, pod.OrderCode, pod.PartnerId })
                        .Select(g => new
                        {
                            g.Key.ProductId,
                            g.Key.OrderCode,
                            g.Key.PartnerId,
                            Quantity = g.Sum(pod => pod.Quantity),
                        })
                        .ToList();

                    foreach (var productData in productDatas)
                    {
                        var totalPlanQuantity = productDatas.Where(p => p.ProductId == productData.ProductId).Sum(p => p.Quantity);

                        var totalImportQuantity = importStockObjects
                            .Where(imp => imp.ObjectId == productData.ProductId && imp.ProductionOrderId == productionOrderId)
                            .Sum(imp => imp.ProductionQuantity);

                        var totalLastestDateImportQuantity = importStockObjects
                            .Where(imp => imp.ObjectId == productData.ProductId && imp.ProductionOrderId == productionOrderId && imp.Date == lastestDate)
                            .Sum(imp => imp.ProductionQuantity);

                        var totalLastestWeekImportQuantity = importStockObjects
                           .Where(imp => imp.ObjectId == productData.ProductId && imp.ProductionOrderId == productionOrderId && imp.Date >= currentWeekPlan.StartDate && imp.Date <= currentWeekPlan.EndDate)
                           .Sum(imp => imp.ProductionQuantity);
                      
                        var monthlyImportStock = new ImportProductModel
                        {
                            ProductId = productData.ProductId,
                            PlanQuantity = productData.Quantity,
                            PartnerId = productData.PartnerId,
                            ImportQuantity = totalPlanQuantity > 0 ? totalImportQuantity * productData.Quantity / totalPlanQuantity : 0,
                            LastestDateImportQuantity = totalPlanQuantity > 0 ? totalLastestDateImportQuantity * productData.Quantity / totalPlanQuantity : 0,
                            LastestWeekImportQuantity = totalPlanQuantity > 0 ? totalLastestWeekImportQuantity * productData.Quantity / totalPlanQuantity : 0,
                            OrderCode = productData.OrderCode
                        };
                        result[groupStep.Key].Add(monthlyImportStock);
                    }
                }
            }

            return result;
        }
    }
}
