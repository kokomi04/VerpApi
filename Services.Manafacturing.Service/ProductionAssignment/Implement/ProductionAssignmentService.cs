﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Verp.Resources.Manafacturing.Production.Assignment;
using Verp.Resources.Master.Config.ActionButton;
using VErp.Commons.Constants;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper.Hr;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper.Product;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper.QueueHelper;
using VErp.Infrastructure.ServiceCore.Facade;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Manafacturing.Model.ProductionAssignment;
using VErp.Services.Manafacturing.Model.ProductionOrder;
using VErp.Services.Manafacturing.Service.Facade;
using VErp.Services.Manafacturing.Service.ProductionOrder;
using VErp.Services.Manafacturing.Service.StatusProcess.Implement;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;
using static VErp.Commons.GlobalObject.QueueName.ManufacturingQueueNameConstants;
using static VErp.Services.Manafacturing.Service.Facade.ProductivityWorkloadFacade;
using ProductionAssignmentEntity = VErp.Infrastructure.EF.ManufacturingDB.ProductionAssignment;
using ProductionOrderEntity = VErp.Infrastructure.EF.ManufacturingDB.ProductionOrder;

namespace VErp.Services.Manafacturing.Service.ProductionAssignment.Implement
{
    public class ProductionAssignmentService : StatusProcessService, IProductionAssignmentService
    {
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly ObjectActivityLogFacade _objActivityLogFacade;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IOrganizationHelperService _organizationHelperService;
        private readonly IProductionOrderQueueHelperService _productionOrderQueueHelperService;
        private readonly IProductHelperService _productHelperService;
        private readonly IProductionOrderService _productionOrderService;


        public ProductionAssignmentService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<ProductionAssignmentService> logger
            , IMapper mapper
            , IOrganizationHelperService organizationHelperService, IProductionOrderQueueHelperService productionOrderQueueHelperService, IProductHelperService productHelperService, IProductionOrderService productionOrderService) : base(manufacturingDB, activityLogService, logger, mapper)
        {
            _manufacturingDBContext = manufacturingDB;
            _objActivityLogFacade = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.ProductionAssignment);
            _logger = logger;
            _mapper = mapper;
            _organizationHelperService = organizationHelperService;
            _productionOrderQueueHelperService = productionOrderQueueHelperService;
            _productHelperService = productHelperService;
            _productionOrderService = productionOrderService;
        }

        public async Task<bool> DismissUpdateWarning(long productionOrderId)
        {
            try
            {
                // Cập nhật lại trạng thái thay đổi số lượng LSX
                var productionOrder = _manufacturingDBContext.ProductionOrder.FirstOrDefault(po => po.ProductionOrderId == productionOrderId);
                if (productionOrder == null) throw new BadRequestException(GeneralCode.InvalidParams, "Lệnh sản xuất không tồn tại");
                if (productionOrder.IsUpdateProcessForAssignment == true)
                {
                    productionOrder.IsUpdateProcessForAssignment = false;
                }
                await _manufacturingDBContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DismissUpdateWarning");
                throw;
            }
        }

        public async Task<IList<ProductionAssignmentModel>> GetProductionAssignments(long productionOrderId)
        {
            return await GetByProductionOrders(new[] { productionOrderId });
        }

        public async Task<IList<ProductionAssignmentModel>> GetByProductionOrders(IList<long> productionOrderIds)
        {

            var assignmentQuery = _manufacturingDBContext.ProductionAssignment
              .Where(a => productionOrderIds.Contains(a.ProductionOrderId));

            return await GetProductionAssignment(assignmentQuery);
        }


        public async Task<IList<ProductionAssignmentModel>> GetByDateRange(long fromDate, long toDate)
        {
            var assignmentQuery = _manufacturingDBContext.ProductionAssignment
                .Include(a => a.ProductionAssignmentDetail)
              .Where(a => a.ProductionAssignmentDetail.Any(d => d.WorkDate >= fromDate.UnixToDateTime() && d.WorkDate <= toDate.UnixToDateTime()));

            return await GetProductionAssignment(assignmentQuery);
        }

        public async Task<ProductionAssignmentModel> GetProductionAssignment(long productionOrderId, long productionStepId, int departmentId)
        {

            var assignmentQuery = _manufacturingDBContext.ProductionAssignment
               .Where(a => a.ProductionOrderId == productionOrderId
               && a.ProductionStepId == productionStepId
               && a.DepartmentId == departmentId);

            return (await GetProductionAssignment(assignmentQuery)).FirstOrDefault();
        }


        private async Task<IList<ProductionAssignmentModel>> GetProductionAssignment(IQueryable<ProductionAssignmentEntity> productionAssignments)
        {
            productionAssignments = from a in productionAssignments
                                        //join s in _manufacturingDBContext.ProductionStep on a.ProductionOrderId equals s.ContainerId
                                    join o in _manufacturingDBContext.ProductionOrder on a.ProductionOrderId equals o.ProductionOrderId
                                    //where s.ContainerTypeId==(int)EnumContainerType.ProductionOrder
                                    select a;

            var assignments = await productionAssignments
                .Include(a => a.ProductionAssignmentDetail)
                .ProjectTo<ProductionAssignmentModel>(_mapper.ConfigurationProvider)
                .ToListAsync();


            var poIds = assignments.Select(a => a.ProductionOrderId).Distinct().ToList();
            var productionOrders = await _manufacturingDBContext.ProductionOrder.Include(po => po.ProductionOrderDetail).Where(p => poIds.Contains(p.ProductionOrderId)).ToListAsync();
            var workLoads = await _productionOrderService.GetProductionWorkLoads(productionOrders, null);

            var workloadInfos = workLoads.SelectMany(production =>
                                        production.Value.SelectMany(step =>
                                                                        step.Value.SelectMany(group =>
                                                                                                    group.Details.Select(v => (ProductionStepWorkloadModel)v)
                                                                                              )
                                                                    )
                                    )
                .ToList();

            var workloadsByStep = workloadInfos.GroupBy(w => w.ProductionStepId)
                .ToDictionary(w => w.Key, w => w.ToList());

            foreach (var a in assignments)
            {

                workloadsByStep.TryGetValue(a.ProductionStepId ?? 0, out var workloads);
                var workloadInfo = workloads?.FirstOrDefault(w => w.ProductionStepLinkDataId == a.ProductionStepLinkDataId);


                if (workloadInfo != null)
                {

                    decimal totalWorkload = 0;
                    decimal? totalHours = 0;
                    foreach (var w in workloads)
                    {

                        var assignQuantity = workloadInfo.Quantity > 0 ? a.AssignmentQuantity * w.Quantity / workloadInfo.Quantity : 0;
                        var workload = assignQuantity * w.WorkloadConvertRate;
                        var hour = w.Productivity > 0 ? workload / w.Productivity : 0;
                        totalWorkload += workload;
                        totalHours += hour;

                    }

                    a.AssignmentWorkload = a.AssignmentWorkload ?? totalWorkload;
                    a.AssignmentHours = a.AssignmentHours ?? totalHours;
                    //a.SetAssignmentWorkload(totalWorkload);
                    //a.SetAssignmentWorkHour(totalHours);
                }

                foreach (var d in a.ProductionAssignmentDetail)
                {

                    if (workloadInfo != null)
                    {
                        decimal? totalWorkload = 0;
                        decimal? totalHours = 0;
                        foreach (var w in workloads)
                        {

                            var assignQuantity = workloadInfo.Quantity > 0 ? d.QuantityPerDay * w.Quantity / workloadInfo.Quantity : 0;
                            var workload = assignQuantity * w.WorkloadConvertRate;
                            var hour = w.Productivity > 0 ? workload / w.Productivity : 0;
                            totalWorkload += workload;
                            totalHours += hour;

                        }

                        d.WorkloadPerDay = d.WorkloadPerDay ?? totalWorkload;
                        d.WorkHourPerDay = d.WorkHourPerDay ?? totalHours;
                        //d.SetWorkloadPerDay(totalWorkload);
                        //d.SetWorkHourPerDay(totalHours);

                    }
                }
            }

            return assignments;
        }

        public async Task<bool> UpdateProductionAssignment(long productionOrderId, GeneralAssignmentModel data)
        {
            if (data.ProductionStepAssignment == null)
            {
                data.ProductionStepAssignment = new GeneralProductionStepAssignmentModel[0];
            }
            data.ProductionStepAssignment = data.ProductionStepAssignment.Where(a => a.ProductionAssignments?.Length > 0).ToArray();

            var productionOrder = _manufacturingDBContext.ProductionOrder.FirstOrDefault(po => po.ProductionOrderId == productionOrderId);
            if (productionOrder == null) throw new BadRequestException(GeneralCode.InvalidParams, "Lệnh sản xuất không tồn tại");

            // Cập nhật trạng thái sau khi thay đổi phân công do update quy trình cho LSX
            if (productionOrder.IsUpdateProcessForAssignment == true)
            {
                productionOrder.IsUpdateProcessForAssignment = false;
            }

            // Validate
            var steps = _manufacturingDBContext.ProductionStep
                .Include(s => s.ProductionStepLinkDataRole)
                .ThenInclude(r => r.ProductionStepLinkData)
                .Where(s => s.ContainerTypeId == (int)EnumContainerType.ProductionOrder && s.ContainerId == productionOrderId)
                .ToList();

            var productionOderDetails = (
                from po in _manufacturingDBContext.ProductionOrder
                join pod in _manufacturingDBContext.ProductionOrderDetail on po.ProductionOrderId equals pod.ProductionOrderId
                where po.ProductionOrderId == productionOrderId
                select new
                {
                    pod.ProductionOrderDetailId,
                    pod.ProductId,
                    ProductionOrderQuantity = pod.Quantity + pod.ReserveQuantity,
                    po.StartDate,
                    po.EndDate
                }).ToList();

            if (productionOderDetails.Count == 0) throw new BadRequestException(GeneralCode.InvalidParams, "Lệnh sản xuất không tồn tại");

            //var productionOrderDetailIds = productionOderDetails.Select(s => s.ProductionOrderDetailId).ToList();

            var oldProductionAssignments = _manufacturingDBContext.ProductionAssignment
                   .Include(a => a.ProductionAssignmentDetail)
                   .ThenInclude(d => d.ProductionAssignmentDetailLinkData)
                   .Where(s => s.ProductionOrderId == productionOrderId)
                   .ToList();

          
            // Validate tổ đã thực hiện sản xuất
            //var parammeters = new SqlParameter[]
            //{
            //    new SqlParameter("@ProductionOrderId", productionOrderId)
            //};
            //var resultData = await _manufacturingDBContext.ExecuteDataProcedure("asp_ProductionHandover_GetInventoryRequirementByProductionOrder", parammeters);

            //var inputInventorys = resultData.ConvertData<ProductionInventoryRequirementEntity>()
            //    .Where(r => r.Status != (int)EnumProductionInventoryRequirementStatus.Rejected)
            //    .ToList();

            //var handovers = _manufacturingDBContext.ProductionHandover
            //    .Where(h => h.ProductionOrderId == productionOrderId)
            //    .ToList();

            var mapData = new Dictionary<long,
                (
                List<(ProductionAssignmentEntity Entity, ProductionAssignmentModel Model)> UpdateProductionStepAssignments,
                List<ProductionAssignmentModel> CreateProductionStepAssignments)>();

            // Danh sách khai báo chi phí
            //var productionScheduleTurnShifts = _manufacturingDBContext.ProductionScheduleTurnShift
            //    .Where(s => s.ProductionOrderId == productionOrderId)
            //    .ToList();
            // Danh sách khai báo vật tư tiêu hao
            //var scheduleTurnShifts = _manufacturingDBContext.ProductionScheduleTurnShift
            //    .Where(s => s.ProductionOrderId == productionOrderId)
            //    .ToList();

            // Danh sách phân công của các công đoạn bị xóa
            var assignedProductionStepIds = data.ProductionStepAssignment
                .Select(a => a.ProductionStepId)
                .ToList();

            var deletedProductionStepAssignments = oldProductionAssignments
                    .Where(s => !assignedProductionStepIds.Contains(s.ProductionStepId))
                    .ToList();

            // Lấy thông tin outsource
            //var outSource = step.ProductionStepLinkDataRole
            //   .FirstOrDefault(r => r.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Output && r.ProductionStepLinkData.OutsourceQuantity.HasValue);
            //.FirstOrDefault();

            var duplicatePStep = data.ProductionStepAssignment
                .GroupBy(a => a.ProductionStepId)
                .FirstOrDefault(a => a.Count() > 1);

            if (duplicatePStep != null)
            {
                var step = steps.FirstOrDefault(s => s.ProductionStepId == duplicatePStep.Key);
                throw new BadRequestException(GeneralCode.InvalidParams, "Tồn tại nhiều hơn 1 phân công công đoạn " + step?.Title);
            }

            foreach (var assignmentStep in data.ProductionStepAssignment)
            {
                if (assignmentStep.ProductionAssignments.Any(a => a.ProductionOrderId != productionOrderId || a.ProductionStepId != assignmentStep.ProductionStepId))
                    throw new BadRequestException(GeneralCode.InvalidParams, "Thông tin kế công đoạn sản xuất giữa các tổ không khớp");


                var step = steps.FirstOrDefault(s => s.ProductionStepId == assignmentStep.ProductionStepId);
                if (step == null) throw new BadRequestException(GeneralCode.InvalidParams, "Công đoạn sản xuất không tồn tại");

                var isDuplicateLink = assignmentStep.ProductionAssignments.GroupBy(a => a.ProductionStepLinkDataId).Count() > 1;
                if (isDuplicateLink)
                {
                    throw new BadRequestException(GeneralCode.InvalidParams, "Cần đồng nhất phân công 1 đầu ra, công đoạn " + step?.Title);
                }

                var linkData = step.ProductionStepLinkDataRole.FirstOrDefault(r => r.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Output
                && r.ProductionStepLinkDataId == assignmentStep.ProductionAssignments.First().ProductionStepLinkDataId
                )?.ProductionStepLinkData;
                if (linkData == null)
                {
                    throw new BadRequestException(GeneralCode.InvalidParams, "Đầu ra không tồn tại, công đoạn " + step?.Title);
                }

                var totalAssignQuantity = assignmentStep.ProductionAssignments.Sum(a => a.AssignmentQuantity);
                if (totalAssignQuantity.SubProductionDecimal(linkData.Quantity) > 0)
                {
                    throw new BadRequestException(GeneralCode.InvalidParams, $"Số lượng phân công lớn hơn số lượng trong kế hoạch sản xuất, {step.Title}");
                }


                var oldProductionStepAssignments = oldProductionAssignments
                   .Where(s => s.ProductionStepId == assignmentStep.ProductionStepId)
                   .ToList();


                var updateAssignments = new List<(ProductionAssignmentEntity Entity, ProductionAssignmentModel Model)>();
                var newAssignments = new List<ProductionAssignmentModel>();

                foreach (var d in assignmentStep.ProductionAssignments)
                {
                    var sumByDays = (d.ProductionAssignmentDetail?.Sum(d => d.QuantityPerDay) ?? 0);
                    if (d.AssignmentQuantity.SubDecimal(sumByDays) != 0)
                    {
                        throw new BadRequestException(GeneralCode.InvalidParams, $"Tổng số lượng phân công từng ngày phải bằng số lượng phân công, {step.Title}");
                    }


                    if (d.AssignmentQuantity <= 0)
                        throw new BadRequestException(GeneralCode.InvalidParams, $"Số lượng phân công phải lớn hơn 0, {step.Title}");
                    d.IsUseMinAssignHours = d.ProductionAssignmentDetail?.Any(d => d.IsUseMinAssignHours == true) == true;

                    var entity = oldProductionStepAssignments.FirstOrDefault(a => a.DepartmentId == d.DepartmentId);
                    if (entity == null)
                    {
                        newAssignments.Add(d);
                    }
                    else
                    {
                        if (d.IsChange(entity))
                        {
                            updateAssignments.Add((entity, d));
                        }
                        oldProductionStepAssignments.Remove(entity);
                    }
                }

                mapData.Add(assignmentStep.ProductionStepId, (updateAssignments, newAssignments));

                deletedProductionStepAssignments.AddRange(oldProductionStepAssignments);
            }




            using (var trans = await _manufacturingDBContext.Database.BeginTransactionAsync())
                try
                {
                    // Thêm thông tin thời gian biểu làm việc
                    //var startDate = productionOderDetails[0].StartDate;
                    //var endDate = productionOderDetails[0].EndDate;

                    //// Xử lý thông tin làm việc của tổ theo từng ngày
                    //var departmentIds = data.DepartmentTimeTable.Select(d => d.DepartmentId).ToList();

                    //var oldTimeTable = _manufacturingDBContext.DepartmentTimeTable
                    //    .Where(t => departmentIds.Contains(t.DepartmentId) && t.WorkDate >= startDate && t.WorkDate <= endDate).ToList();

                    //_manufacturingDBContext.DepartmentTimeTable.RemoveRange(oldTimeTable);

                    //foreach (var item in data.DepartmentTimeTable)
                    //{
                    //    var entity = _mapper.Map<DepartmentTimeTable>(item);
                    //    _manufacturingDBContext.DepartmentTimeTable.Add(entity);
                    //}

                    ClearAssignmentDetails(oldProductionAssignments);
                    _manufacturingDBContext.SaveChanges();

                    var productionStepWorkInfos = _manufacturingDBContext.ProductionStepWorkInfo.Where(w => assignedProductionStepIds.Contains(w.ProductionStepId)).ToList();
                    // Xóa phân công có công đoạn bị xóa khỏi quy trình

                    await DeleteAssignmentRef(productionOrderId, deletedProductionStepAssignments);

                    _manufacturingDBContext.SaveChanges();

                    foreach (var productionStepAssignments in data.ProductionStepAssignment)
                    {
                        // Thêm thông tin công việc
                        var productionStepWorkInfo = productionStepWorkInfos.FirstOrDefault(w => w.ProductionStepId == productionStepAssignments.ProductionStepId);
                        if (productionStepWorkInfo == null)
                        {
                            productionStepWorkInfo = _mapper.Map<ProductionStepWorkInfo>(productionStepAssignments.ProductionStepWorkInfo);
                            productionStepWorkInfo.ProductionStepId = productionStepAssignments.ProductionStepId;
                            _manufacturingDBContext.ProductionStepWorkInfo.Add(productionStepWorkInfo);
                        }
                        else
                        {
                            _mapper.Map(productionStepAssignments.ProductionStepWorkInfo, productionStepWorkInfo);
                        }


                        // Thêm mới phân công
                        if (mapData[productionStepAssignments.ProductionStepId].CreateProductionStepAssignments.Count > 0)
                        {
                            var newEntities = mapData[productionStepAssignments.ProductionStepId].CreateProductionStepAssignments.AsQueryable()
                               .ProjectTo<ProductionAssignmentEntity>(_mapper.ConfigurationProvider)
                               .ToList();
                            foreach (var newEntitie in newEntities)
                            {
                                newEntitie.AssignedProgressStatus = (int)EnumAssignedProgressStatus.Waiting;
                            }
                            _manufacturingDBContext.ProductionAssignment.AddRange(newEntities);
                        }

                        // Cập nhật phân công
                        foreach (var tuple in mapData[productionStepAssignments.ProductionStepId].UpdateProductionStepAssignments)
                        {
                            tuple.Entity.ProductionAssignmentDetail.Clear();
                            _mapper.Map(tuple.Model, tuple.Entity);
                        }
                    }

                    // Update reset process status
                    productionOrder.IsResetProductionProcess = true;


                    _manufacturingDBContext.SaveChanges();


                    await SetProductionAssignmentInfo(productionOrder, steps);

                    // Cập nhật trạng thái cho lệnh và phân công
                    //await UpdateFullAssignedProgressStatus(productionOrderId);

                    await trans.CommitAsync();

                    var productionOrderInfo = await _manufacturingDBContext.ProductionOrder.FirstOrDefaultAsync(p => p.ProductionOrderId == productionOrderId);
                    await _objActivityLogFacade.LogBuilder(() => ProductionAssignmentActivityLogMessage.Update)
                   .MessageResourceFormatDatas(productionOrderInfo?.ProductionOrderCode)
                   .ObjectId(productionOrderId)
                   .JsonData(data)
                   .CreateLog();

                    await _productionOrderQueueHelperService.ProductionOrderStatiticChanges(productionOrderInfo?.ProductionOrderCode, $"Cập nhật phân công");

                    return true;
                }
                catch (Exception ex)
                {
                    await trans.RollbackAsync();
                    _logger.LogError(ex, "UpdateProductAssignment");
                    throw;
                }
        }

        private async Task SetProductionAssignmentInfos(IList<long> productionOrderIds)
        {
            var infos = await _manufacturingDBContext.ProductionOrder.Where(o => productionOrderIds.Contains(o.ProductionOrderId)).ToListAsync();
            var steps = await _manufacturingDBContext.ProductionStep
            .Where(s => s.ContainerTypeId == (int)EnumContainerType.ProductionOrder && productionOrderIds.Contains(s.ContainerId))
            .ToListAsync();

            foreach (var po in infos)
            {
                await SetProductionAssignmentInfo(po, steps.Where(s => s.ContainerId == po.ProductionOrderId).ToList());
            }

        }


        private async Task SetProductionAssignmentInfo(ProductionOrderEntity productionOrder, IList<ProductionStep> productionSteps)
        {

            var productionAssignments = await _manufacturingDBContext.ProductionAssignment
                  .Include(a => a.ProductionAssignmentDetail)
                  .Include(a => a.ProductionStepLinkData)
                  .Where(s => s.ProductionOrderId == productionOrder.ProductionOrderId)
                  .ToListAsync();

            var productionOrderAssignmentStatusId = productionSteps.Count == 0 ? EnumProductionOrderAssignmentStatus.NoAssignment : EnumProductionOrderAssignmentStatus.Completed;

            foreach (var s in productionSteps.Where(s => s.IsGroup != true && !s.IsFinish))
            {
                s.ProductionStepAssignmentStatusId = (int)EnumProductionOrderAssignmentStatus.NoAssignment;
                var linkData = productionAssignments.FirstOrDefault(d => d.ProductionStepId == s.ProductionStepId)?.ProductionStepLinkData;
                if (linkData != null)
                {
                    var total = linkData.Quantity + (linkData.ExportOutsourceQuantity ?? 0);

                    var stepAssignments = productionAssignments.Where(a => a.ProductionStepLinkDataId == linkData.ProductionStepLinkDataId);

                    var assignmentQuantity = stepAssignments.Sum(a => a.AssignmentQuantity);

                    var isInvalidAssignment = stepAssignments.Any(s => !s.StartDate.HasValue || !s.EndDate.HasValue || !s.ProductionAssignmentDetail.Any());

                    if (assignmentQuantity > 0)
                    {
                        s.ProductionStepAssignmentStatusId = (int)EnumProductionOrderAssignmentStatus.AssignProcessing;
                    }

                    if (assignmentQuantity >= total && !isInvalidAssignment)
                    {
                        s.ProductionStepAssignmentStatusId = (int)EnumProductionOrderAssignmentStatus.Completed;
                    }
                    else
                    {
                        productionOrderAssignmentStatusId = EnumProductionOrderAssignmentStatus.AssignProcessing;
                    }

                }
                else
                {
                    productionOrderAssignmentStatusId = EnumProductionOrderAssignmentStatus.AssignProcessing;
                }
            }

            productionOrder.ProductionOrderAssignmentStatusId = (int)productionOrderAssignmentStatusId;

            _manufacturingDBContext.SaveChanges();
        }

        public async Task DeleteAssignmentRef(long productionOrderId, IList<ProductionAssignmentEntity> deletedProductionStepAssignments)
        {

            var handovers = await _manufacturingDBContext.ProductionHandover
                .Where(h => h.ProductionOrderId == productionOrderId)
                .ToListAsync();

            var histories = await _manufacturingDBContext.ProductionHistory
               .Where(h => h.ProductionOrderId == productionOrderId)
               .ToListAsync();

            var humanResources = await _manufacturingDBContext.ProductionHumanResource
                .Where(hr => hr.ProductionOrderId == productionOrderId)
                .ToListAsync();

            foreach (var oldProductionAssignment in deletedProductionStepAssignments)
            {
                // Xóa bàn giao liên quan tới phân công bị xóa
                var deleteHandovers = handovers
                    .Where(h => (h.FromProductionStepId == oldProductionAssignment.ProductionStepId || h.ToProductionStepId == oldProductionAssignment.ProductionStepId)
                    && (h.FromDepartmentId == oldProductionAssignment.DepartmentId || h.ToDepartmentId == oldProductionAssignment.DepartmentId))
                    .ToList();

                foreach (var h in deleteHandovers)
                {
                    h.IsDeleted = true;
                }

                // Xóa lịch sử sx liên quan tới phân công bị xóa
                var deleteHistories = histories
                    .Where(h => h.ProductionStepId == oldProductionAssignment.ProductionStepId
                    && h.DepartmentId == oldProductionAssignment.DepartmentId)
                    .ToList();

                foreach (var h in deleteHistories)
                {
                    h.IsDeleted = true;
                }

                // Xóa khai báo nhân công
                var deleteHumanResources = humanResources
                    .Where(hr => hr.ProductionStepId == oldProductionAssignment.ProductionStepId && hr.DepartmentId == oldProductionAssignment.DepartmentId)
                    .ToList();

                // _manufacturingDBContext.ProductionHumanResource.RemoveRange(deleteHumanResources);
                foreach (var r in deleteHumanResources)
                {
                    r.IsDeleted = true;
                }


            }

            ClearAssignmentDetails(deletedProductionStepAssignments);

            _manufacturingDBContext.ProductionAssignment.RemoveRange(deletedProductionStepAssignments);
        }


        public void ClearAssignmentDetails(IList<ProductionAssignmentEntity> clearDetailProductionStepAssignments)
        {

            foreach (var oldProductionAssignment in clearDetailProductionStepAssignments)
            {

                foreach (var d in oldProductionAssignment.ProductionAssignmentDetail)
                {
                    d.ProductionAssignmentDetailLinkData.Clear();
                }

                // Xóa chi tiết phân công
                oldProductionAssignment.ProductionAssignmentDetail.Clear();

            }
        }


        /*
        public async Task<bool> UpdateProductionAssignment(long productionOrderId, long productionStepId, ProductionAssignmentModel[] data, ProductionStepWorkInfoInputModel info)
        {
            var productionOrder = _manufacturingDBContext.ProductionOrder.FirstOrDefault(po => po.ProductionOrderId == productionOrderId);
            if (productionOrder == null) throw new BadRequestException(GeneralCode.InvalidParams, "Lệnh sản xuất không tồn tại");

            // Validate
            var step = _manufacturingDBContext.ProductionStep
                .Include(s => s.ProductionStepLinkDataRole)
                .ThenInclude(r => r.ProductionStepLinkData)
                .Where(s => s.ProductionStepId == productionStepId)
                .FirstOrDefault();

            if (data.Any(a => a.ProductionOrderId != productionOrderId || a.ProductionStepId != productionStepId))
                throw new BadRequestException(GeneralCode.InvalidParams, "Thông tin kế hoạch hoặc công đoạn sản xuất giữa các tổ không khớp");

            if (step == null) throw new BadRequestException(GeneralCode.InvalidParams, "Công đoạn sản xuất không tồn tại");

            //if (data.Any(a => a.Productivity <= 0)) throw new BadRequestException(GeneralCode.InvalidParams, "Năng suất không hợp lệ");

            var productionOderDetails = (
                from po in _manufacturingDBContext.ProductionOrder
                join pod in _manufacturingDBContext.ProductionOrderDetail on po.ProductionOrderId equals pod.ProductionOrderId
                where po.ProductionOrderId == productionOrderId
                select new
                {
                    pod.ProductionOrderDetailId,
                    pod.ProductId,
                    ProductionOrderQuantity = pod.Quantity + pod.ReserveQuantity,
                    po.StartDate,
                    po.EndDate
                }).ToList();

            if (productionOderDetails.Count == 0) throw new BadRequestException(GeneralCode.InvalidParams, "Lệnh sản xuất không tồn tại");

            var productionOrderDetailIds = productionOderDetails.Select(s => s.ProductionOrderDetailId).ToList();

            //if (!_manufacturingDBContext.ProductionStepOrder
            //    .Any(so => productionOrderDetailIds.Contains(so.ProductionOrderDetailId) && so.ProductionStepId == productionStepId))
            //{
            //    throw new BadRequestException(GeneralCode.InvalidParams, "Công đoạn sản xuất không tồn tại trong quy trình sản xuất");
            //}

            var linkDatas = step.ProductionStepLinkDataRole
                .Where(r => r.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Output)
                .ToDictionary(r => r.ProductionStepLinkDataId,
                r =>
                {
                    return Math.Round(r.ProductionStepLinkData.Quantity, 8);
                });

            if (data.Any(d => d.AssignmentQuantity <= 0))
                throw new BadRequestException(GeneralCode.InvalidParams, "Số lượng phân công phải lớn hơn 0");

            // Lấy thông tin outsource
            var outSource = step.ProductionStepLinkDataRole
                .Where(r => r.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Output && r.ProductionStepLinkData.OutsourceQuantity.HasValue)
                .FirstOrDefault();

            foreach (var linkData in linkDatas)
            {
                decimal totalAssignmentQuantity = 0;

                if (outSource != null)
                {
                    totalAssignmentQuantity += linkData.Value * outSource.ProductionStepLinkData.OutsourceQuantity.GetValueOrDefault()
                        / outSource.ProductionStepLinkData.Quantity;
                }

                foreach (var assignment in data)
                {
                    var sourceData = linkDatas[assignment.ProductionStepLinkDataId];
                    totalAssignmentQuantity += assignment.AssignmentQuantity * linkData.Value / sourceData;
                }

                if (totalAssignmentQuantity.SubProductionDecimal(linkData.Value) > 0)
                    throw new BadRequestException(GeneralCode.InvalidParams, "Số lượng phân công lớn hơn số lượng trong kế hoạch sản xuất");
            }

            var oldProductionAssignments = _manufacturingDBContext.ProductionAssignment
                .Include(a => a.ProductionAssignmentDetail)
                .Where(s => s.ProductionOrderId == productionOrderId && s.ProductionStepId == productionStepId)
                .ToList();

            var updateAssignments = new List<(ProductionAssignmentEntity Entity, ProductionAssignmentModel Model)>();
            var newAssignments = new List<ProductionAssignmentModel>();
            foreach (var item in data)
            {
                var entity = oldProductionAssignments.FirstOrDefault(a => a.DepartmentId == item.DepartmentId);
                if (entity == null)
                {
                    newAssignments.Add(item);
                }
                else
                {
                    if (item.IsChange(entity))
                    {
                        updateAssignments.Add((entity, item));
                    }
                    oldProductionAssignments.Remove(entity);
                }
            }

            // Validate khai báo chi phí
            var deleteAssignDepartmentIds = oldProductionAssignments.Select(a => a.DepartmentId).ToList();
            //if (_manufacturingDBContext.ProductionScheduleTurnShift
            //    .Any(s => s.ProductionOrderId == productionOrderId && s.ProductionStepId == productionStepId && deleteAssignDepartmentIds.Contains(s.DepartmentId)))
            //{
            //    throw new BadRequestException(GeneralCode.InvalidParams, "Không thể xóa phân công cho tổ đã khai báo chi phí");
            //}

            // Validate vật tư tiêu hao
            if (_manufacturingDBContext.ProductionConsumMaterial
                .Any(m => m.ProductionOrderId == productionOrderId && m.ProductionStepId == productionStepId && deleteAssignDepartmentIds.Contains(m.DepartmentId)))
            {
                throw new BadRequestException(GeneralCode.InvalidParams, "Không thể xóa phân công cho tổ đã khai báo vật tư tiêu hao");
            }

            // Validate tổ đã thực hiện sản xuất
            //var parammeters = new SqlParameter[]
            //{
            //    new SqlParameter("@ProductionOrderId", productionOrderId)
            //};
            //var resultData = await _manufacturingDBContext.ExecuteDataProcedure("asp_ProductionHandover_GetInventoryRequirementByProductionOrder", parammeters);

            //var inputInventorys = resultData.ConvertData<ProductionInventoryRequirementEntity>()
            //    .Where(r => r.Status != (int)EnumProductionInventoryRequirementStatus.Rejected)
            //    .ToList();

            var handovers = _manufacturingDBContext.ProductionHandover
                .Where(h => h.ProductionOrderId == productionOrderId && (h.FromProductionStepId == productionStepId || h.ToProductionStepId == productionStepId))
                .ToList();

            // Validate xóa tổ đã tham gia sản xuất
            //var productIds = step.ProductionStepLinkDataRole
            //        .Where(r => r.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Output && r.ProductionStepLinkData.ObjectTypeId == (int)EnumProductionStepLinkDataObjectType.Product)
            //        .Select(r => r.ProductionStepLinkData.ObjectId)
            //        .ToList();
            //if (inputInventorys.Any(r => productIds.Contains(r.ProductId) && r.DepartmentId.HasValue && deleteAssignDepartmentIds.Contains(r.DepartmentId.Value))
            //    || handovers.Any(h => deleteAssignDepartmentIds.Contains(h.FromDepartmentId) || deleteAssignDepartmentIds.Contains(h.ToDepartmentId)))
            //{
            //    throw new BadRequestException(GeneralCode.InvalidParams, "Không thể xóa phân công cho tổ đã tham gia sản xuất");
            //}

            // Validate sửa tổ đã tham gia sản xuất
            //foreach (var tuple in updateAssignments)
            //{


            //}

            try
            {
                // Thêm thông tin thời gian biểu làm việc
                var startDate = productionOderDetails[0].StartDate;
                var endDate = productionOderDetails[0].EndDate;

                //var startDateUnix = startDate.GetUnix();
                //var endDateUnix = endDate.GetUnix();

                var departmentIds = data.Select(a => a.DepartmentId).ToList();

                //var oldTimeTable = _manufacturingDBContext.DepartmentTimeTable.Where(t => departmentIds.Contains(t.DepartmentId) && t.WorkDate >= startDate && t.WorkDate <= endDate).ToList();
                //_manufacturingDBContext.DepartmentTimeTable.RemoveRange(oldTimeTable);

                //foreach (var item in timeTable)
                //{
                //    var entity = _mapper.Map<DepartmentTimeTable>(item);
                //    _manufacturingDBContext.DepartmentTimeTable.Add(entity);
                //}

                // Thêm thông tin công việc
                var productionStepWorkInfo = _manufacturingDBContext.ProductionStepWorkInfo
                    .FirstOrDefault(w => w.ProductionStepId == productionStepId);
                if (productionStepWorkInfo == null)
                {
                    productionStepWorkInfo = _mapper.Map<ProductionStepWorkInfo>(info);
                    productionStepWorkInfo.ProductionStepId = productionStepId;
                    _manufacturingDBContext.ProductionStepWorkInfo.Add(productionStepWorkInfo);
                }
                else
                {
                    _mapper.Map(info, productionStepWorkInfo);
                }

                // Xóa phân công
                if (oldProductionAssignments.Count > 0)
                {
                    foreach (var oldProductionAssignment in oldProductionAssignments)
                    {
                        // Xóa bàn giao liên quan tới phân công bị xóa
                        var deleteHandovers = handovers
                            .Where(h => (h.FromProductionStepId == oldProductionAssignment.ProductionStepId || h.ToProductionStepId == oldProductionAssignment.ProductionStepId)
                            && (h.FromDepartmentId == oldProductionAssignment.DepartmentId || h.ToDepartmentId == oldProductionAssignment.DepartmentId))
                            .ToList();

                        _manufacturingDBContext.ProductionHandover.RemoveRange(deleteHandovers);
                        oldProductionAssignment.ProductionAssignmentDetail.Clear();
                    }
                    _manufacturingDBContext.SaveChanges();
                    _manufacturingDBContext.ProductionAssignment.RemoveRange(oldProductionAssignments);
                }
                // Thêm mới phân công
                var newEntities = newAssignments.AsQueryable()
                    .ProjectTo<ProductionAssignmentEntity>(_mapper.ConfigurationProvider)
                    .ToList();
                foreach (var newEntitie in newEntities)
                {
                    newEntitie.AssignedProgressStatus = (int)EnumAssignedProgressStatus.Waiting;
                }
                _manufacturingDBContext.ProductionAssignment.AddRange(newEntities);
                // Cập nhật phân công
                foreach (var tuple in updateAssignments)
                {
                    tuple.Entity.ProductionAssignmentDetail.Clear();
                    _mapper.Map(tuple.Model, tuple.Entity);
                }

                // Update reset process status
                productionOrder.IsResetProductionProcess = true;

                // Cập nhật trạng thái cho lệnh và phân công
                await UpdateFullAssignedProgressStatus(productionOrderId);

                _manufacturingDBContext.SaveChanges();

                await _activityLogService.CreateLog(EnumObjectType.ProductionAssignment, productionStepId, $"Cập nhật phân công sản xuất cho lệnh sản xuất {productionOrderId}", data);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateProductAssignment");
                throw;
            }
        }*/


        public async Task<IList<DepartmentAssignFreeDate>> DepartmentsFreeDates(DepartmentAssignFreeDateInput req)
        {

            var parammeters = new SqlParameter[]
            {
                req.DepartmentIds.ToSqlParameter("@DepartmentIds"),
                req.ExceptProductionOrderIds.ToSqlParameter("@ExceptProductionOrderIds"),
            };
            var resultData = await _manufacturingDBContext.QueryListProc<DepartmentAssignFreeDate>("asp_ProductionAssignment_DepartmentFreeDate", parammeters);
            return resultData;
        }

        public async Task<bool> UpdateDepartmentAssignmentDate(int departmentId, IList<DepartmentAssignUpdateDateModel> data)
        {
            using (var trans = await _manufacturingDBContext.Database.BeginTransactionAsync())
            {

                var productionStepIds = data.Select(d => d.ProductionStepId).ToList();
                var assignsByDepartment = await _manufacturingDBContext.ProductionAssignment
                     .Include(a => a.ProductionAssignmentDetail)
                     .ThenInclude(d => d.ProductionAssignmentDetailLinkData)
                     .Where(a => a.DepartmentId == departmentId && productionStepIds.Contains(a.ProductionStepId))
                     .ToListAsync();

                var productionOrderIds = assignsByDepartment.Select(a => a.ProductionOrderId).Distinct().ToList();

                var removingDetails = new List<ProductionAssignmentDetail>();

                var removingDetailDatas = new List<ProductionAssignmentDetailLinkData>();

                var addingDetails = new List<ProductionAssignmentDetail>();

                foreach (var assign in assignsByDepartment)
                {
                    var updateInfo = data.FirstOrDefault(d => d.ProductionStepId == assign.ProductionStepId);
                    if (updateInfo != null)
                    {


                        assign.StartDate = updateInfo.StartDate.UnixToDateTime();
                        assign.EndDate = updateInfo.EndDate.UnixToDateTime();
                        assign.IsManualSetStartDate = updateInfo.IsManualSetStartDate;
                        assign.IsManualSetEndDate = updateInfo.IsManualSetEndDate;
                        assign.RateInPercent = updateInfo.RateInPercent;
                        assign.IsUseMinAssignHours = updateInfo?.Details?.Any(d => d.IsUseMinAssignHours == true) == true;

                        foreach (var d in assign.ProductionAssignmentDetail)
                        {
                            removingDetailDatas.AddRange(d.ProductionAssignmentDetailLinkData);
                        }
                        removingDetails.AddRange(assign.ProductionAssignmentDetail);
                        var details = _mapper.Map<List<ProductionAssignmentDetail>>(updateInfo.Details);

                        foreach (var d in details)
                        {
                            d.ProductionOrderId = assign.ProductionOrderId;
                            d.ProductionStepId = assign.ProductionStepId;
                            d.DepartmentId = assign.DepartmentId;
                        }


                        var sumByDays = details?.Sum(d => d.QuantityPerDay) ?? 0;
                        if (assign.AssignmentQuantity.SubDecimal(sumByDays) != 0)
                        {
                            throw new BadRequestException(GeneralCode.InvalidParams, "Tổng số lượng phân công từng ngày phải bằng số lượng phân công!");
                        }

                        addingDetails.AddRange(details);
                    }
                }
                _manufacturingDBContext.ProductionAssignmentDetailLinkData.RemoveRange(removingDetailDatas);
                await _manufacturingDBContext.SaveChangesAsync();

                _manufacturingDBContext.ProductionAssignmentDetail.RemoveRange(removingDetails);
                await _manufacturingDBContext.SaveChangesAsync();

                await _manufacturingDBContext.ProductionAssignmentDetail.AddRangeAsync(addingDetails);
                await _manufacturingDBContext.SaveChangesAsync();

                //await UpdateProductionOrderAssignmentStatus(productionOrderIds);
                await SetProductionAssignmentInfos(productionOrderIds);

                await trans.CommitAsync();
                return true;
            }
        }

        public async Task<PageData<DepartmentProductionAssignmentModel>> DepartmentProductionAssignment(int departmentId, string keyword, long? productionOrderId, int page, int size, string orderByFieldName, bool asc, long? fromDate, long? toDate)
        {
            var fDate = fromDate.UnixToDateTime();
            var tDate = toDate.UnixToDateTime();
            keyword = string.IsNullOrEmpty(keyword) ? string.Empty : keyword.Trim();
            var assignmentQuery = (
                from a in _manufacturingDBContext.ProductionAssignment
                join s in _manufacturingDBContext.ProductionStep.Where(s => s.ContainerTypeId == (int)EnumContainerType.ProductionOrder) on a.ProductionStepId equals s.ProductionStepId
                join o in _manufacturingDBContext.ProductionOrder on a.ProductionOrderId equals o.ProductionOrderId
                join od in _manufacturingDBContext.ProductionOrderDetail on o.ProductionOrderId equals od.ProductionOrderId
                where a.DepartmentId == departmentId && (fDate != null && tDate != null ? o.Date >= fDate && o.Date <= tDate : true) && o.ProductionOrderCode.Contains(keyword)
                select new
                {
                    o.ProductionOrderId,
                    o.ProductionOrderCode,
                    od.OrderDetailId,
                    od.ProductId,
                    o.StartDate,
                    o.EndDate,
                    TotalQuantity = od.Quantity + od.ReserveQuantity,
                    o.ProductionOrderStatus
                })
                .Distinct();
            if (productionOrderId.HasValue)
            {
                assignmentQuery = assignmentQuery.Where(a => a.ProductionOrderId == productionOrderId);
            }

            var total = await assignmentQuery.CountAsync();
            if (string.IsNullOrWhiteSpace(orderByFieldName))
            {
                orderByFieldName = nameof(DepartmentProductionAssignmentModel.StartDate);
            }

            var pagedData = size > 0 || total > 10000 ? await assignmentQuery.SortByFieldName(orderByFieldName, asc).Skip((page - 1) * size).Take(size).ToListAsync() : await assignmentQuery.ToListAsync();

            return (pagedData.Select(d => new DepartmentProductionAssignmentModel()
            {
                ProductionOrderId = d.ProductionOrderId,
                ProductionOrderCode = d.ProductionOrderCode,
                OrderDetailId = d.OrderDetailId,
                ProductId = d.ProductId,
                StartDate = d.StartDate.GetUnix(),
                EndDate = d.EndDate.GetUnix(),
                ProductQuantity = d.TotalQuantity,
                ProductionOrderStatus = (EnumProductionStatus)d.ProductionOrderStatus
            }).ToList(), total);
        }

        public async Task<CapacityOutputModel> GetGeneralCapacityDepartments(long productionOrderId)
        {
            var productionTime = await (
                from o in _manufacturingDBContext.ProductionOrder
                where o.ProductionOrderId == productionOrderId
                select new
                {
                    o.StartDate,
                    o.EndDate
                }).FirstOrDefaultAsync();

            if (productionTime == null)
                throw new BadRequestException(GeneralCode.InvalidParams, "Kế hoạch sản xuất không tồn tại");

            var productionSteps = _manufacturingDBContext.ProductionStep
                .Where(ps => ps.ContainerTypeId == (int)EnumContainerType.ProductionOrder && ps.ContainerId == productionOrderId && ps.StepId.HasValue).ToList();

            if (productionSteps.Count == 0)
                throw new BadRequestException(GeneralCode.InvalidParams, "Công đoạn sản xuất không tồn tại");

            var departmentIdMap = (from sd in _manufacturingDBContext.StepDetail
                                   join ps in _manufacturingDBContext.ProductionStep on sd.StepId equals ps.StepId
                                   where ps.ContainerTypeId == (int)EnumContainerType.ProductionOrder && ps.ContainerId == productionOrderId
                                   select new
                                   {
                                       ps.ProductionStepId,
                                       sd.DepartmentId
                                   })
                                   .ToList()
                                   .GroupBy(sd => sd.ProductionStepId)
                                   .ToDictionary(sd => sd.Key, sd => sd.Select(sd => sd.DepartmentId)
                                   .ToList());

            var productionStepIds = productionSteps.Select(ps => ps.ProductionStepId).ToList();
            var departmentIds = departmentIdMap.SelectMany(sd => sd.Value).ToList();

            var includeAssignments = _manufacturingDBContext.ProductionAssignment
                .Where(a => productionStepIds.Contains(a.ProductionStepId)
                    && a.ProductionOrderId == productionOrderId
                    && !departmentIds.Contains(a.DepartmentId)
                )
                .Select(a => new
                {
                    a.ProductionStepId,
                    a.DepartmentId
                })
                .ToList()
                .GroupBy(sd => sd.ProductionStepId)
                .ToDictionary(sd => sd.Key, sd => sd.Select(sd => sd.DepartmentId).ToList());

            departmentIds.AddRange(includeAssignments.SelectMany(sd => sd.Value).ToList());
            foreach (var includeAssignment in includeAssignments)
            {
                departmentIdMap[includeAssignment.Key].AddRange(includeAssignment.Value);
            }

            if (departmentIdMap.Any(sd => sd.Value.Count == 0))
                throw new BadRequestException(GeneralCode.InvalidParams, "Tồn tại công đoạn chưa thiết lập tổ sản xuất");

            var capacityDepartments = departmentIds.Distinct().ToDictionary(d => d, d => new List<CapacityModel>());

            var assignOthers = (
                from a in _manufacturingDBContext.ProductionAssignment
                where departmentIds.Contains(a.DepartmentId)
                    && a.ProductionOrderId != productionOrderId
                    && a.StartDate <= productionTime.EndDate
                    && a.EndDate >= productionTime.StartDate
                join ps in _manufacturingDBContext.ProductionStep on a.ProductionStepId equals ps.ProductionStepId
                join p in _manufacturingDBContext.ProductionStep on ps.ParentId equals p.ProductionStepId
                join s in _manufacturingDBContext.Step on p.StepId equals s.StepId
                join sd in _manufacturingDBContext.StepDetail on new { s.StepId, a.DepartmentId } equals new { sd.StepId, sd.DepartmentId }
                join po in _manufacturingDBContext.ProductionOrder on ps.ContainerId equals po.ProductionOrderId
                join d in _manufacturingDBContext.ProductionStepLinkData on a.ProductionStepLinkDataId equals d.ProductionStepLinkDataId
                join ldr in _manufacturingDBContext.ProductionStepLinkDataRole on new
                {
                    ps.ProductionStepId,
                    ProductionStepLinkDataRoleTypeId = (int)EnumProductionStepLinkDataRoleType.Output
                }
                equals new
                {
                    ldr.ProductionStepId,
                    ldr.ProductionStepLinkDataRoleTypeId
                }
                join ld in _manufacturingDBContext.ProductionStepLinkData on ldr.ProductionStepLinkDataId equals ld.ProductionStepLinkDataId
                join ildr in _manufacturingDBContext.ProductionStepLinkDataRole on new
                {
                    ldr.ProductionStepLinkDataId,
                    ProductionStepLinkDataRoleTypeId = (int)EnumProductionStepLinkDataRoleType.Input
                }
                equals new
                {
                    ildr.ProductionStepLinkDataId,
                    ildr.ProductionStepLinkDataRoleTypeId
                } into ildrs
                from ildr in ildrs.DefaultIfEmpty()
                join psw in _manufacturingDBContext.ProductionStepWorkInfo on ps.ProductionStepId equals psw.ProductionStepId into psws
                from psw in psws.DefaultIfEmpty()
                orderby a.CreatedDatetimeUtc ascending
                select new
                {
                    a.DepartmentId,
                    a.ProductionStepId,
                    a.AssignmentQuantity,
                    a.StartDate,
                    a.EndDate,
                    a.CreatedDatetimeUtc,

                    d.LinkDataObjectId,
                    LinkDataObjectTypeId = (EnumProductionStepLinkDataObjectType)d.LinkDataObjectTypeId,
                    s.StepId,
                    s.StepName,

                    po.ProductionOrderCode,
                    po.ProductionOrderId,
                    ld.WorkloadConvertRate,
                    OutputQuantity = ld.Quantity,
                    IsImport = ildr == null,
                    psw.MinHour,
                    psw.MaxHour
                })
               .ToList();


            var productIds = assignOthers
               .Where(w => w.LinkDataObjectTypeId == EnumProductionStepLinkDataObjectType.Product)
               .Select(w => (int)w.LinkDataObjectId)
               .Distinct()
               .ToList();

            var semiIds = assignOthers
                .Where(w => w.LinkDataObjectTypeId == EnumProductionStepLinkDataObjectType.ProductSemi)
                .Select(w => w.LinkDataObjectId)
                .Distinct()
                .ToList();

            var workloadFacade = new ProductivityWorkloadFacade(_manufacturingDBContext, _productHelperService);
            var (productTargets, semiTargets) = await workloadFacade.GetProductivities(productIds, semiIds);


            var otherAssignments = assignOthers.Select(a =>
            {

                decimal? productivityByStep = null;

                ProductTargetProductivityByStep target = null;
                if (a.LinkDataObjectTypeId == EnumProductionStepLinkDataObjectType.ProductSemi)
                {
                    semiTargets.TryGetValue(a.LinkDataObjectId, out target);
                }
                else
                {
                    productTargets.TryGetValue((int)a.LinkDataObjectId, out target);
                }

                ProductStepTargetProductivityDetail targetByStep = null;
                target?.TryGetValue(a.StepId, out targetByStep);

                if (targetByStep != null)
                {
                    productivityByStep = targetByStep.TargetProductivity;
                    if (targetByStep.ProductivityTimeTypeId == EnumProductivityTimeType.Day)
                    {
                        productivityByStep /= (decimal)OrganizationConstants.WORKING_HOUR_PER_DAY;
                    }
                }

                var rate = a.WorkloadConvertRate ?? (targetByStep?.Rate ?? 1);
                return new
                {
                    a.DepartmentId,
                    a.ProductionStepId,
                    a.AssignmentQuantity,
                    a.StartDate,
                    a.EndDate,
                    a.CreatedDatetimeUtc,

                    a.LinkDataObjectId,
                    a.LinkDataObjectTypeId,
                    a.StepName,
                    ProductivityPerPerson = productivityByStep,
                    a.ProductionOrderCode,
                    a.ProductionOrderId,

                    WorkloadConvertRate = rate,
                    Workload = a.OutputQuantity * rate,

                    a.OutputQuantity,
                    a.IsImport,
                    a.MinHour,
                    a.MaxHour
                };
            }).GroupBy(a => new
            {
                a.DepartmentId,
                a.ProductionStepId
            })
                .Select(g => new AssignmentCapacityInfo
                {
                    DepartmentId = g.Key.DepartmentId,
                    ProductionStepId = g.Key.ProductionStepId,
                    AssignmentQuantity = g.Max(a => a.AssignmentQuantity),
                    StartDate = g.Max(a => a.StartDate),
                    EndDate = g.Max(a => a.EndDate),
                    CreatedDatetimeUtc = g.Max(a => a.CreatedDatetimeUtc),
                    Workload = g.Sum(a => a.Workload),
                    ObjectId = g.Max(a => a.LinkDataObjectId),
                    ObjectTypeId = g.Max(a => a.LinkDataObjectTypeId),
                    StepName = g.Max(a => a.StepName),
                    ProductivityPerPerson = g.Max(a => a.ProductivityPerPerson ?? 0),
                    ProductionOrderCode = g.Max(a => a.ProductionOrderCode),
                    ProductionOrderId = g.Max(a => a.ProductionOrderId),
                    OutputQuantity = g.Sum(a => a.OutputQuantity),
                    ImportStockQuantity = g.Sum(a => a.IsImport ? a.OutputQuantity : 0),
                    MinHour = g.Max(a => a.MinHour),
                    MaxHour = g.Max(a => a.MaxHour),
                })
                .ToList();

            var otherAssignmentDetails = (
                from a in _manufacturingDBContext.ProductionAssignment
                where departmentIds.Contains(a.DepartmentId)
                    && a.ProductionOrderId != productionOrderId
                    && a.StartDate <= productionTime.EndDate
                    && a.EndDate >= productionTime.StartDate
                join ad in _manufacturingDBContext.ProductionAssignmentDetail on new { a.ProductionOrderId, a.ProductionStepId, a.DepartmentId } equals new { ad.ProductionOrderId, ad.ProductionStepId, ad.DepartmentId }
                select new
                {
                    a.DepartmentId,
                    a.ProductionStepId,
                    ad.QuantityPerDay,
                    ad.WorkDate
                })
                .ToList();

            foreach (var otherAssignment in otherAssignments)
            {
                otherAssignment.ProductionAssignmentDetail = otherAssignmentDetails
                    .Where(d => d.DepartmentId == otherAssignment.DepartmentId && d.ProductionStepId == otherAssignment.ProductionStepId)
                    .Select(d => new AssignmentCapacityDetail
                    {
                        QuantityPerDay = d.QuantityPerDay,
                        WorkDate = d.WorkDate
                    })
                    .ToList();
            }

            //var otherProductionStepIds = otherAssignments.Select(a => a.ProductionStepId).Distinct().ToList();
            //var otherProductionOrderIds = otherAssignments.Select(a => a.ProductionOrderId).Distinct().ToList();

            //var handovers = _manufacturingDBContext.ProductionHandover
            //    .Where(h => otherProductionStepIds.Contains(h.FromProductionStepId) && departmentIds.Contains(h.FromDepartmentId) && h.Status == (int)EnumHandoverStatus.Accepted)
            //    .ToList();

            //var inventoryRequirements = new Dictionary<long, List<ProductionInventoryRequirementModel>>();

            //foreach (var otherProductionOrderId in otherProductionOrderIds)
            //{
            //    var parammeters = new SqlParameter[]
            //    {
            //        new SqlParameter("@ProductionOrderId", otherProductionOrderId)
            //    };
            //    var resultData = await _manufacturingDBContext.ExecuteDataProcedure("asp_ProductionHandover_GetInventoryRequirementByProductionOrder", parammeters);

            //    inventoryRequirements.Add(otherProductionOrderId, resultData.ConvertData<ProductionInventoryRequirementEntity>()
            //        .Where(ir => ir.DepartmentId.HasValue && departmentIds.Contains(ir.DepartmentId.Value) && ir.Status == (int)EnumProductionInventoryRequirementStatus.Accepted)
            //        .AsQueryable()
            //        .ProjectTo<ProductionInventoryRequirementModel>(_mapper.ConfigurationProvider)
            //        .ToList());
            //}

            departmentIds = otherAssignments.Select(a => a.DepartmentId).Distinct().ToList();
            var startDateUnix = productionTime.StartDate.GetUnix();
            var endDateUnix = productionTime.EndDate.GetUnix();
            if (otherAssignments.Count > 0)
            {
                var minDate = otherAssignments.Min(a => a.StartDate).GetUnix();
                var maxDate = otherAssignments.Max(a => a.EndDate).GetUnix();
                startDateUnix = startDateUnix < minDate ? startDateUnix : (minDate ?? startDateUnix);
                endDateUnix = endDateUnix > maxDate ? endDateUnix : (maxDate ?? endDateUnix);
            }

            // Lấy thông tin phong ban
            var departments = (await _organizationHelperService.GetDepartmentSimples(departmentIds.ToArray()));
            var departmentCalendar = (await _organizationHelperService.GetListDepartmentCalendar(startDateUnix, endDateUnix, departmentIds.ToArray()));

            foreach (var group in otherAssignments.GroupBy(a => new { a.ProductionOrderId, a.ObjectId, a.ObjectTypeId, a.DepartmentId }))
            {
                //var totalInventoryQuantity = group.Key.ObjectTypeId == (int)EnumProductionStepLinkDataObjectType.Product
                //    ? 0
                //    : inventoryRequirements[group.Key.ProductionOrderId]
                //    .Where(ir => !ir.ProductionStepId.HasValue && ir.DepartmentId == group.Key.DepartmentId && ir.ProductId == group.Key.ObjectId)
                //    .Sum(ir => ir.ActualQuantity);

                var calendar = departmentCalendar.FirstOrDefault(d => d.DepartmentId == group.Key.DepartmentId);
                var department = departments.FirstOrDefault(d => d.DepartmentId == group.Key.DepartmentId);

                foreach (var otherAssignment in group)
                {
                    var productionStepName = $"{otherAssignment.StepName}";
                    //var completedQuantity = handovers
                    //.Where(h => h.FromProductionStepId == otherAssignment.ProductionStepId
                    //&& h.FromDepartmentId == otherAssignment.DepartmentId
                    //&& h.ObjectId == otherAssignment.ObjectId
                    //&& h.ObjectTypeId == otherAssignment.ObjectTypeId)
                    //.Sum(h => h.HandoverQuantity);

                    //if (otherAssignment.ImportStockQuantity > 0)
                    //{
                    //    var allocatedQuantity = inventoryRequirements[group.Key.ProductionOrderId]
                    //        .Where(ir => ir.ProductionStepId.HasValue
                    //        && ir.ProductionStepId.Value == otherAssignment.ProductionStepId
                    //        && ir.DepartmentId == group.Key.DepartmentId
                    //        && ir.ProductId == group.Key.ObjectId)
                    //        .Sum(ir => ir.ActualQuantity);

                    //    var departmentImportStockQuantity = otherAssignment.AssignmentQuantity * otherAssignment.ImportStockQuantity / otherAssignment.TotalQuantity;
                    //    var unallocatedQuantity = totalInventoryQuantity > departmentImportStockQuantity - allocatedQuantity ? departmentImportStockQuantity - allocatedQuantity : totalInventoryQuantity;
                    //    completedQuantity += (allocatedQuantity + unallocatedQuantity);
                    //    totalInventoryQuantity -= unallocatedQuantity;
                    //}

                    var capacityDepartment = new CapacityModel
                    {
                        ProductionOrderCode = otherAssignment.ProductionOrderCode,
                        StepName = productionStepName,
                        Workload = otherAssignment.Workload,
                        AssingmentQuantity = otherAssignment.AssignmentQuantity,
                        //LinkDataQuantity = otherAssignment.OutputQuantity,
                        OutputQuantity = otherAssignment.OutputQuantity,
                        StartDate = otherAssignment.StartDate.GetUnix(),
                        EndDate = otherAssignment.EndDate.GetUnix(),
                        CreatedDatetimeUtc = otherAssignment.CreatedDatetimeUtc.GetUnix(),
                        ObjectId = otherAssignment.ObjectId,
                        ObjectTypeId = otherAssignment.ObjectTypeId,
                        //CompletedQuantity = completedQuantity
                    };

                    // Nếu có tồn tại năng suất
                    if (otherAssignment.ProductivityPerPerson > 0 && department != null && department.NumberOfPerson > 0)
                    {
                        foreach (var productionAssignmentDetail in otherAssignment.ProductionAssignmentDetail)
                        {
                            var capacityPerDay = otherAssignment.OutputQuantity > 0 ? (otherAssignment.Workload
                                * productionAssignmentDetail.QuantityPerDay.Value)
                                / (otherAssignment.OutputQuantity
                                * otherAssignment.ProductivityPerPerson) : 0;
                            capacityDepartment.CapacityDetail.Add(new CapacityDetailModel
                            {
                                WorkDate = productionAssignmentDetail.WorkDate.GetUnix(),
                                StepName = productionStepName,
                                ProductionOrderCode = otherAssignment.ProductionOrderCode,
                                CapacityPerDay = capacityPerDay,
                                Productivity = otherAssignment.ProductivityPerPerson
                            });
                        }
                    }
                    else
                    {
                        foreach (var productionAssignmentDetail in otherAssignment.ProductionAssignmentDetail)
                        {
                            var workDateUnix = productionAssignmentDetail.WorkDate.GetUnix();
                            // Tính số giờ làm việc theo ngày của tổ
                            // Tìm tăng ca
                            var overHour = calendar.DepartmentOverHourInfo.FirstOrDefault(oh => oh.StartDate <= productionAssignmentDetail.WorkDate.GetUnix() && oh.EndDate >= productionAssignmentDetail.WorkDate.GetUnix());
                            var increase = calendar.DepartmentIncreaseInfo.FirstOrDefault(i => i.StartDate <= productionAssignmentDetail.WorkDate.GetUnix() && i.EndDate >= productionAssignmentDetail.WorkDate.GetUnix());
                            var workingHourInfo = calendar.DepartmentWorkingHourInfo.Where(wh => wh.StartDate <= productionAssignmentDetail.WorkDate.GetUnix()).OrderByDescending(wh => wh.StartDate).FirstOrDefault();


                            var workingHoursPerDay = (decimal)((workingHourInfo?.WorkingHourPerDay ?? 0 * (department?.NumberOfPerson ?? 0 + increase?.NumberOfPerson ?? 0)) + (overHour?.OverHour ?? 0 * overHour?.NumberOfPerson ?? 0));

                            var totalHour = capacityDepartments[otherAssignment.DepartmentId]
                                .SelectMany(c => c.CapacityDetail)
                                .Where(c => c.WorkDate == workDateUnix)
                                .Sum(c => c.CapacityPerDay);

                            var capacityPerDay = workingHoursPerDay < totalHour ? 0 : workingHoursPerDay - totalHour;
                            if (otherAssignment.MinHour.HasValue && capacityPerDay < otherAssignment.MinHour) capacityPerDay = 0;
                            if (otherAssignment.MaxHour.HasValue && capacityPerDay > otherAssignment.MaxHour) capacityPerDay = otherAssignment.MaxHour;

                            capacityDepartment.CapacityDetail.Add(new CapacityDetailModel
                            {
                                WorkDate = workDateUnix,
                                StepName = productionStepName,
                                ProductionOrderCode = otherAssignment.ProductionOrderCode,
                                CapacityPerDay = capacityPerDay
                            });
                        }
                    }
                    capacityDepartments[otherAssignment.DepartmentId].Add(capacityDepartment);
                }
            }

            return new CapacityOutputModel
            {
                CapacityData = capacityDepartments
            };
        }



        public async Task<IList<ProductionStepWorkInfoOutputModel>> GetListProductionStepWorkInfo(long productionOrderId)
        {
            return await (from w in _manufacturingDBContext.ProductionStepWorkInfo
                          join ps in _manufacturingDBContext.ProductionStep on w.ProductionStepId equals ps.ProductionStepId
                          where ps.ContainerTypeId == (int)EnumContainerType.ProductionOrder && ps.ContainerId == productionOrderId
                          select w)
                          .ProjectTo<ProductionStepWorkInfoOutputModel>(_mapper.ConfigurationProvider)
                          .ToListAsync();
        }

        //public async Task<IList<DepartmentTimeTableModel>> GetDepartmentTimeTable(int[] departmentIds, long startDate, long endDate)
        //{
        //    DateTime startDateTime = startDate.UnixToDateTime().GetValueOrDefault();
        //    DateTime endDateTime = endDate.UnixToDateTime().GetValueOrDefault();

        //    return await _manufacturingDBContext.DepartmentTimeTable
        //        .Where(t => departmentIds.Contains(t.DepartmentId) && t.WorkDate >= startDateTime && t.WorkDate <= endDateTime)
        //        .ProjectTo<DepartmentTimeTableModel>(_mapper.ConfigurationProvider)
        //        .ToListAsync();
        //}

        public async Task<bool> ChangeAssignedProgressStatus(long productionOrderId, long productionStepId, int departmentId, EnumAssignedProgressStatus status)
        {
            var assignment = _manufacturingDBContext.ProductionAssignment
                .FirstOrDefault(a => a.ProductionOrderId == productionOrderId && a.ProductionStepId == productionStepId && a.DepartmentId == departmentId);
            if (assignment == null) throw new BadRequestException(GeneralCode.InvalidParams, "Công việc không tồn tại");
            try
            {

                if (status != EnumAssignedProgressStatus.Finish)
                {
                    assignment.IsManualFinish = false;
                }
                else
                {
                    if (assignment.AssignedProgressStatus != (int)status)
                    {
                        assignment.IsManualFinish = true;
                    }

                }

                assignment.AssignedProgressStatus = (int)status;

                if (assignment.AssignedProgressStatus == (int)EnumAssignedProgressStatus.Finish)
                {
                    assignment.AssignedInputStatus = (int)EnumAssignedProgressStatus.Finish;
                }

                _manufacturingDBContext.SaveChanges();


                var productionOrderInfo = await _manufacturingDBContext.ProductionOrder.FirstOrDefaultAsync(p => p.ProductionOrderId == productionOrderId);
                var step = await _manufacturingDBContext.ProductionStep.FirstOrDefaultAsync(s => s.ProductionStepId == assignment.ProductionStepId);

                await _productionOrderQueueHelperService.ProductionOrderStatiticChanges(productionOrderInfo?.ProductionOrderCode, $"Cập nhật trạng thái công đoạn {step?.Title}");
                await _objActivityLogFacade.LogBuilder(() => ProductionAssignmentActivityLogMessage.UpdateStatus)
                   .MessageResourceFormatDatas(productionOrderInfo?.ProductionOrderCode)
                   .ObjectId(productionOrderId)
                   .JsonData(assignment)
                   .CreateLog();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateProductAssignment");
                throw;
            }
        }


        private class AssignmentCapacityDetail
        {
            public DateTime WorkDate { get; set; }
            public decimal? QuantityPerDay { get; set; }
            //public decimal? HourPerDay { get; set; }
        }
        private class AssignmentCapacityInfo
        {
            public int DepartmentId { get; set; }
            public long ProductionStepId { get; set; }
            public decimal AssignmentQuantity { get; set; }
            public decimal? Workload { get; set; }
            public EnumProductionStepLinkDataObjectType ObjectTypeId { get; set; }
            public long ObjectId { get; set; }
            public string StepName { get; set; }
            public decimal ProductivityPerPerson { get; set; }
            public string ProductionOrderCode { get; set; }
            public long ProductionOrderId { get; set; }
            public decimal OutputQuantity { get; set; }
            public decimal ImportStockQuantity { get; set; }
            public decimal? MinHour { get; set; }
            public decimal? MaxHour { get; set; }

            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public DateTime CreatedDatetimeUtc { get; set; }

            public List<AssignmentCapacityDetail> ProductionAssignmentDetail { get; set; }
        }

    }
}
