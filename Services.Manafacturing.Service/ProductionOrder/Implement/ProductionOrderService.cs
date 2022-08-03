using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verp.Cache.RedisCache;
using VErp.Commons.Constants;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Manafacturing.Model.ProductionAssignment;
using VErp.Services.Manafacturing.Model.ProductionOrder;
using VErp.Services.Manafacturing.Service.Facade;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;
using static VErp.Services.Manafacturing.Service.Facade.ProductivityWorkloadFacade;
using ProductionOrderEntity = VErp.Infrastructure.EF.ManufacturingDB.ProductionOrder;

namespace VErp.Services.Manafacturing.Service.ProductionOrder.Implement
{
    public class ProductionOrderService : IProductionOrderService
    {
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly ICustomGenCodeHelperService _customGenCodeHelperService;
        private readonly IProductHelperService _productHelperService;
        private readonly IOrganizationHelperService _organizationHelperService;
        private readonly IDraftDataHelperService _draftDataHelperService;
        private readonly ICurrentContextService _currentContextService;

        public ProductionOrderService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<ProductionOrderService> logger
            , IMapper mapper
            , ICustomGenCodeHelperService customGenCodeHelperService
            , IProductHelperService productHelperService
            , IOrganizationHelperService organizationHelperService
            , IDraftDataHelperService draftDataHelperService
            , ICurrentContextService currentContextService
            )
        {
            _manufacturingDBContext = manufacturingDB;
            _activityLogService = activityLogService;
            _logger = logger;
            _mapper = mapper;
            _customGenCodeHelperService = customGenCodeHelperService;
            _productHelperService = productHelperService;
            _organizationHelperService = organizationHelperService;
            _draftDataHelperService = draftDataHelperService;
            _currentContextService = currentContextService;
        }

        public async Task<bool> UpdateProductionProcessVersion(long productionOrderId, int productId)
        {
            var details = _manufacturingDBContext.ProductionOrderDetail
                .Where(o => o.ProductionOrderId == productionOrderId && o.ProductId == productId)
                .ToList();

            if (details.Count == 0)
                throw new BadRequestException(GeneralCode.InvalidParams, "Lệnh SX không tồn tại");

            var version = await _productHelperService.GetProductionProcessVersion(productId);

            details.ForEach(x => x.ProductionProcessVersion = version);

            await _manufacturingDBContext.SaveChangesAsync();

            return true;
        }

        public async Task<IList<ProductionOrderListModel>> GetProductionOrdersByCodes(IList<string> productionOrderCodes)
        {
            if (productionOrderCodes.Count == 0) return Array.Empty<ProductionOrderListModel>();
            var filter = new SingleClause()
            {
                DataType = EnumDataType.Text,
                FieldName = nameof(ProductionOrderListModel.ProductionOrderCode),
                Operator = EnumOperator.InList,
                Value = productionOrderCodes
            };

            var result = await GetProductionOrders(string.Empty, 1, 0, string.Empty, true, 0, 0, null, filter);
            return result.List;
        }

        public async Task<IList<ProductionOrderListModel>> GetProductionOrdersByIds(IList<long> productionOrderIds)
        {
            if (productionOrderIds.Count == 0) return Array.Empty<ProductionOrderListModel>();
            var filter = new SingleClause()
            {
                DataType = EnumDataType.BigInt,
                FieldName = nameof(ProductionOrderListModel.ProductionOrderId),
                Operator = EnumOperator.InList,
                Value = productionOrderIds
            };

            var result = await GetProductionOrders(string.Empty, 1, 0, string.Empty, true, 0, 0, null, filter);
            return result.List;
        }

        public async Task<PageData<ProductionOrderListModel>> GetProductionOrders(string keyword, int page, int size, string orderByFieldName, bool asc, long fromDate, long toDate, bool? hasNewProductionProcessVersion = null, Clause filters = null)
        {
            keyword = (keyword ?? "").Trim();
            var parammeters = new List<SqlParameter>();

            var whereCondition = new StringBuilder();
            if (!string.IsNullOrEmpty(keyword))
            {
                whereCondition.Append("(v.ProductionOrderCode LIKE @KeyWord ");
                whereCondition.Append("OR v.ProductCode LIKE @Keyword ");
                whereCondition.Append("OR v.ProductName LIKE @Keyword ");
                whereCondition.Append("OR v.CustomerPO LIKE @Keyword ");
                whereCondition.Append("OR v.OrderCode LIKE @Keyword ) ");
                parammeters.Add(new SqlParameter("@Keyword", $"%{keyword}%"));
            }

            if (hasNewProductionProcessVersion.HasValue)
            {
                if (whereCondition.Length > 0)
                    whereCondition.Append(" AND ");
                whereCondition.Append(" v.HasNewProductionProcessVersion = @HasNewProductionProcessVersion ");
                parammeters.Add(new SqlParameter("@HasNewProductionProcessVersion", hasNewProductionProcessVersion.Value));
            }

            if (fromDate > 0 && toDate > 0)
            {
                if (whereCondition.Length > 0)
                    whereCondition.Append(" AND ");
                whereCondition.Append(" (v.Date >= @FromDate AND v.Date <= @ToDate ) ");
                parammeters.Add(new SqlParameter("@FromDate", fromDate.UnixToDateTime()));
                parammeters.Add(new SqlParameter("@ToDate", toDate.UnixToDateTime()));
            }

            if (filters != null)
            {
                var suffix = 0;
                var filterCondition = new StringBuilder();
                filters.FilterClauseProcess("vProductionOrderDetail", "v", ref filterCondition, ref parammeters, ref suffix);
                if (filterCondition.Length > 2)
                {
                    if (whereCondition.Length > 0) whereCondition.Append(" AND ");
                    whereCondition.Append(filterCondition);
                }
            }

            if (string.IsNullOrEmpty(orderByFieldName))
            {
                orderByFieldName = "ProductionOrderId";
                asc = true;
            }

            var sql = new StringBuilder(
                @$";WITH tmp AS (
                    SELECT ");


            sql.Append($"ROW_NUMBER() OVER (ORDER BY g.{orderByFieldName} {(asc ? "" : "DESC")}) AS RowNum,");


            sql.Append(@" g.ProductionOrderId
                        FROM(
                            SELECT * FROM vProductionOrderDetail v");

            var totalSql = new StringBuilder(
                @"SELECT 
                    COUNT(*) Total
                FROM (
                    SELECT v.ProductionOrderId FROM vProductionOrderDetail v ");

            if (whereCondition.Length > 0)
            {
                totalSql.Append(" WHERE ");
                totalSql.Append(whereCondition);

                sql.Append(" WHERE ");
                sql.Append(whereCondition);
            }
            totalSql.Append(" GROUP BY v.ProductionOrderId ) g");
            sql.Append(
                   @") g
	                GROUP BY g.ProductionOrderCode, g.ProductionOrderId, g.Date, g.StartDate, g.EndDate, g.PlanEndDate, g.ProductionOrderStatus ");

            var table = await _manufacturingDBContext.QueryDataTable(totalSql.ToString(), parammeters.ToArray());
            var total = 0;
            // decimal additionResult = 0;
            if (table != null && table.Rows.Count > 0)
            {
                total = (table.Rows[0]["Total"] as int?).GetValueOrDefault();
                // additionResult = (table.Rows[0]["AdditionResult"] as decimal?).GetValueOrDefault();
            }

            if (size > 0)
            {
                sql.Append(@$"ORDER BY g.{orderByFieldName} {(asc ? "" : "DESC")}
                            OFFSET {(page - 1) * size} ROWS
                            FETCH NEXT {size}
                            ROWS ONLY");
            }

            sql.Append(@")
                SELECT v.* FROM tmp t
                LEFT JOIN vProductionOrderDetail v ON t.ProductionOrderId = v.ProductionOrderId ");

            sql.Append(" ORDER BY t.RowNum");

            var resultData = await _manufacturingDBContext.QueryDataTable(sql.ToString(), parammeters.Select(p => p.CloneSqlParam()).ToArray());
            var lst = resultData.ConvertData<ProductionOrderListEntity>().AsQueryable().ProjectTo<ProductionOrderListModel>(_mapper.ConfigurationProvider).ToList();

            return (lst, total);
        }

        public async Task<PageData<ProductOrderModel>> GetProductionOrdersNotDetail(string keyword, int page, int size, string orderByFieldName, bool asc, long fromDate, long toDate, Clause filters = null)
        {
            keyword = (keyword ?? "").Trim();

            var query = _manufacturingDBContext.ProductionOrder.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(keyword))
                query = query.Where(x => x.ProductionOrderCode.Contains(keyword));

            if (fromDate > 0)
            {
                var time = fromDate.UnixToDateTime();
                query = query.Where(q => q.Date >= time);
            }

            if (toDate > 0)
            {
                var time = toDate.UnixToDateTime();
                query = query.Where(q => q.Date <= time);
            }
            query = query.InternalFilter(filters).InternalOrderBy(orderByFieldName, asc);

            var total = await query.CountAsync();

            var lst = (await (size > 0 ? query.Skip((page - 1) * size).Take(size) : query).ToListAsync())
                .Select(x => new ProductOrderModel
                {
                    ProductionOrderId = x.ProductionOrderId,
                    ProductionOrderCode = x.ProductionOrderCode,
                    StartDate = x.StartDate.GetUnix(),
                    EndDate = x.EndDate.GetUnix(),
                    Date = x.Date.GetUnix(),
                    Description = x.Description,
                    IsDraft = x.IsDraft,
                    IsInvalid = x.IsInvalid,
                    ProductionOrderStatus = (EnumProductionStatus)x.ProductionOrderStatus,
                    IsUpdateQuantity = x.IsUpdateQuantity,
                    IsUpdateProcessForAssignment = x.IsUpdateProcessForAssignment,
                    
                }).ToList();

            return (lst, total);
        }

        public async Task<IList<ProductionStepWorkloadModel>> ListWorkLoads(long productionOrderId)
        {
            var productionOrderInfo = await _manufacturingDBContext.ProductionOrder.Include(po => po.ProductionOrderDetail)
                  .FirstOrDefaultAsync(po => po.ProductionOrderId == productionOrderId);

            if (productionOrderInfo == null) throw GeneralCode.ItemNotFound.BadRequest();

            var workLoads = await GetProductionWorkLoads(new[] { productionOrderInfo }, null);

            //var result = new List<ProductionStepWorkloadModel>();

            return workLoads.SelectMany(production =>
                                        production.Value.SelectMany(step =>
                                                                        step.Value.SelectMany(group =>
                                                                                                    group.Details.Select(v => (ProductionStepWorkloadModel)v)
                                                                                              )
                                                                    )
                                    )
                .ToList();
        }


        public async Task<ProductionCapacityModel> GetProductionCapacity(long fromDate, long toDate, int? assignDepartmentId)
        {

            var fromDateTime = fromDate.UnixToDateTime();
            var toDateTime = toDate.UnixToDateTime();

            var productionOrders = await _manufacturingDBContext.ProductionOrder.Include(po => po.ProductionOrderDetail)
                    .Where(po => po.StartDate <= toDateTime && po.EndDate >= fromDateTime)
                    .ToListAsync();


            // Lấy thông tin đầu ra và số giờ công cần
            var productionCapacityDetail = await GetProductionWorkLoads(productionOrders, assignDepartmentId);


            // Lấy thông tin phân công
            //var productionAssignments = _manufacturingDBContext.ProductionAssignment
            //    .Include(pa => pa.ProductionAssignmentDetail)
            //    .Where(pa => productionStepIds.Contains(pa.ProductionStepId))
            //    .ToList();

            /*
            // Tính khối lượng công việc theo tỷ lệ thời gian
            foreach (var group in workloadInfos.GroupBy(w => new { w.ProductionStepId, w.ProductionOrderId }))
            {
                var productionCapacity = productionOrderDetails.First(pc => pc.ProductionOrderId == group.Key.ProductionOrderId);
                var startDate = productionCapacity.StartDate;
                var endDate = productionCapacity.EndDate;
                var productionStepAssigments = productionAssignments.Where(pa => pa.ProductionStepId == group.Key.ProductionStepId).ToList();
                if (productionStepAssigments.Count > 0)
                {
                    startDate = productionStepAssigments.Min(pa => pa.StartDate).GetUnix();
                    endDate = productionStepAssigments.Max(pa => pa.EndDate).GetUnix();
                }

                var assignmentTime = endDate - startDate;
                var calcTime = (endDate > toDate ? toDate : endDate) - (startDate < fromDate ? fromDate : startDate);
                foreach (var item in group)
                {
                    item.Quantity = assignmentTime > 0 ? item.Quantity * calcTime / assignmentTime : 0;
                }
            }*/

            var stepIds = productionCapacityDetail.Values.SelectMany(c => c.Keys).Distinct().ToList();


            // Tính giờ công có
            var stepDetails = _manufacturingDBContext.StepDetail
                .Where(sd => stepIds.Contains(sd.StepId))
                .Select(sd => new
                {
                    sd.StepId,
                    sd.DepartmentId
                })
                .ToList();

            var departmentIds = stepDetails.Select(a => a.DepartmentId).Distinct().ToList();

            // Lấy thông tin phong ban
            var departmentCalendar = (await _organizationHelperService.GetListDepartmentCalendar(fromDate, toDate, departmentIds.ToArray()));
            var departments = (await _organizationHelperService.GetDepartmentSimples(departmentIds.ToArray()));
            var departmentHour = new Dictionary<int, decimal>();

            var assignedHours = new Dictionary<int, decimal>();
            foreach (var (productionOrderId, stepCapacity) in productionCapacityDetail)
            {
                foreach (var (stepId, capacities) in stepCapacity)
                {
                    if (!assignedHours.ContainsKey(stepId))
                    {
                        assignedHours.Add(stepId, 0);
                    }

                    assignedHours[stepId] += capacities.Sum(s => s.Details.Sum(d => d.AssignInfos.Sum(a => a.AssignWorkHour)));
                }
            }

            foreach (var departmentId in departmentIds)
            {
                // Danh sách công đoạn tổ đảm nhiệm
                var departmentStepIds = stepDetails.Where(sd => sd.DepartmentId == departmentId)
                    .Select(sd => sd.StepId)
                    .Distinct()
                    .ToList();

                var calendar = departmentCalendar.FirstOrDefault(c => c.DepartmentId == departmentId);
                var department = departments.FirstOrDefault(d => d.DepartmentId == departmentId);
                decimal totalHour = 0;

                var offDays = calendar.DepartmentDayOffCalendar.Select(o => o.Day.UnixToDateTime(_currentContextService.TimeZoneOffset).Date).ToList();

                for (var workDateUnix = fromDate; workDateUnix <= toDate; workDateUnix += 24 * 60 * 60)
                {
                    var date = workDateUnix.UnixToDateTime(_currentContextService.TimeZoneOffset).Date;

                    var dayOfWeek = date.DayOfWeek;
                    // Tính số giờ làm việc theo ngày của tổ
                    var workingHourInfo = calendar.DepartmentWorkingHourInfo.Where(wh => wh.StartDate <= workDateUnix).OrderByDescending(wh => wh.StartDate).FirstOrDefault();
                    var overHour = calendar.DepartmentOverHourInfo.FirstOrDefault(oh => oh.StartDate <= workDateUnix && oh.EndDate >= workDateUnix);
                    var increase = calendar.DepartmentIncreaseInfo.FirstOrDefault(i => i.StartDate <= workDateUnix && i.EndDate >= workDateUnix);

                    var workingHourPerDay = workingHourInfo?.WorkingHourPerDay ?? 0;
                    var numberOfPerson = department?.NumberOfPerson ?? 0;
                    var increasePerson = increase?.NumberOfPerson ?? 0;

                    var overHourPerday = overHour?.OverHour ?? 0;
                    var overPerson = overHour?.NumberOfPerson ?? 0;

                    var totalWorkingHour = workingHourPerDay * (numberOfPerson + increasePerson);

                    if (offDays.Contains(date))
                    {
                        totalWorkingHour = 0;
                    }

                    var totalOverHour = overHourPerday * overPerson;

                    totalHour += (decimal)(totalWorkingHour + totalOverHour);
                }

                var totalWorkHour = productionCapacityDetail.SelectMany(pc => pc.Value).Where(pc => departmentStepIds.Contains(pc.Key)).Sum(pc => pc.Value.Sum(w => w.WorkHour));
                // Duyệt danh sách công đoạn tổ đảm nhiệm => tính ra số giờ làm việc của tổ cho từng công đoạn theo tỷ lệ KLCV
                foreach (var departmentStepId in departmentStepIds)
                {
                    if (!departmentHour.ContainsKey(departmentStepId)) departmentHour[departmentStepId] = 0;
                    var stepWorkHour = productionCapacityDetail.Sum(pc => pc.Value.ContainsKey(departmentStepId) ? pc.Value[departmentStepId].Sum(w => w.WorkHour) : 0);
                    departmentHour[departmentStepId] += totalWorkHour > 0 ? totalHour * stepWorkHour / totalWorkHour : 0;
                }
            }


            var stepInfo = _manufacturingDBContext.Step
             .Where(s => stepIds.Contains(s.StepId))
             .Select(s => new StepInfo
             {
                 StepId = s.StepId,
                 StepName = s.StepName
             })
             .ToList();

            var result = new ProductionCapacityModel
            {
                StepInfo = stepInfo,
                DepartmentHour = departmentHour,
                AssignedStepHours = assignedHours
            };

            foreach (var productionOrder in productionOrders)
            {
                var productionOrderDetail = productionOrder
                    .ProductionOrderDetail
                    .Select(pc => new ProductionOrderDetailQuantityModel
                    {
                        OrderCode = pc.OrderCode,
                        ProductId = pc.ProductId,
                        ProductionOrderDetailId = pc.ProductionOrderDetailId,
                        Quantity = pc.Quantity,
                        ReserveQuantity = pc.ReserveQuantity
                    })
                    .ToList();


                if (!productionCapacityDetail.TryGetValue(productionOrder.ProductionOrderId, out var prodCap))
                {
                    prodCap = new CapacityByStep();
                }

                result.ProductionOrder.Add(new ProductionOrderCapacityModel
                {
                    ProductionOrderId = productionOrder.ProductionOrderId,
                    ProductionOrderCode = productionOrder.ProductionOrderCode,
                    StartDate = productionOrder.StartDate.GetUnix(),
                    EndDate = productionOrder.EndDate.GetUnix(),
                    ProductionCapacityDetail = prodCap,
                    ProductionOrderDetail = productionOrderDetail
                });
            }

            return result;
        }

        public async Task<CapacityStepByProduction> GetProductionWorkLoads(IList<ProductionOrderEntity> productionOrders, long? assignDepartmentId)
        {

            var productionOrderIds = productionOrders.Select(p => p.ProductionOrderId).Distinct().ToList();

            // Lấy thông tin khối lượng công việc
            var workloadInfos = await (from s in _manufacturingDBContext.ProductionStep
                                       join p in _manufacturingDBContext.ProductionStep on s.ParentId equals p.ProductionStepId
                                       join ldr in _manufacturingDBContext.ProductionStepLinkDataRole
                                       on new { s.ProductionStepId, Type = (int)EnumProductionStepLinkDataRoleType.Output } equals new { ldr.ProductionStepId, Type = ldr.ProductionStepLinkDataRoleTypeId }
                                       join ld in _manufacturingDBContext.ProductionStepLinkData on ldr.ProductionStepLinkDataId equals ld.ProductionStepLinkDataId
                                       where !s.IsFinish && p.IsGroup.Value && productionOrderIds.Contains(s.ContainerId) && s.ContainerTypeId == (int)EnumContainerType.ProductionOrder && p.StepId.HasValue
                                       select new ProductionWorkloadInfo
                                       {
                                           ProductionStepId = s.ProductionStepId,
                                           ProductionStepTitle = s.Title,
                                           ProductionOrderId = s.ContainerId,
                                           StepId = p.StepId.Value,
                                           ProductionStepLinkDataId = ld.ProductionStepLinkDataId,
                                           Quantity = ld.Quantity,
                                           ObjectId = ld.LinkDataObjectId,
                                           ObjectTypeId = (EnumProductionStepLinkDataObjectType)ld.LinkDataObjectTypeId,
                                           WorkloadConvertRate = ld.WorkloadConvertRate
                                       }).ToListAsync();

            var productIds = workloadInfos
                .Where(w => w.ObjectTypeId == EnumProductionStepLinkDataObjectType.Product)
                .Select(w => (int)w.ObjectId)
                .ToList();

            productIds.AddRange(productionOrders.SelectMany(d => d.ProductionOrderDetail.Select(d => d.ProductId)));

            var semiIds = workloadInfos
                .Where(w => w.ObjectTypeId == EnumProductionStepLinkDataObjectType.ProductSemi)
                .Select(w => w.ObjectId)
                .ToList();

            var workloadFacade = new ProductivityWorkloadFacade(_manufacturingDBContext, _productHelperService);
            var (productTargets, semiTargets) = await workloadFacade.GetProductivities(productIds, semiIds);


            foreach (var workload in workloadInfos)
            {

                ProductTargetProductivityByStep target = null;
                if (workload.ObjectTypeId == EnumProductionStepLinkDataObjectType.ProductSemi)
                {
                    semiTargets.TryGetValue(workload.ObjectId, out target);
                }
                else
                {
                    productTargets.TryGetValue((int)workload.ObjectId, out target);
                }

                ProductStepTargetProductivityDetail targetByStep = null;
                target?.TryGetValue(workload.StepId, out targetByStep);

                if (!workload.WorkloadConvertRate.HasValue || workload.WorkloadConvertRate <= 0)
                {
                    workload.WorkloadConvertRate = targetByStep?.Rate ?? 1;
                }

            }


            var productionStepIds = workloadInfos.Select(w => w.ProductionStepId).Distinct().ToList();

            // Lấy thông tin phân công
            var productionAssignments1 = _manufacturingDBContext.ProductionAssignment
                .Include(pa => pa.ProductionAssignmentDetail)
                .Where(pa => productionStepIds.Contains(pa.ProductionStepId))
                .ToList();


            // Lấy thông tin đầu ra và số giờ công cần
            // var productionCapacityDetail = new CapacityStepByProduction();

            Func<EnumProductionStepLinkDataObjectType, long, int, decimal?> getProductivity = (EnumProductionStepLinkDataObjectType objectTypeId, long objectId, int stepId) =>
            {
                decimal? productivityByStep = null;

                ProductTargetProductivityByStep target = null;
                if (objectTypeId == EnumProductionStepLinkDataObjectType.ProductSemi)
                {
                    semiTargets.TryGetValue(objectId, out target);
                }
                else
                {
                    productTargets.TryGetValue((int)objectId, out target);
                }

                ProductStepTargetProductivityDetail targetByStep = null;
                target?.TryGetValue(stepId, out targetByStep);

                if (targetByStep != null)
                {

                    productivityByStep = targetByStep.TargetProductivity;
                    if (targetByStep.ProductivityTimeTypeId == EnumProductivityTimeType.Day)
                    {
                        productivityByStep /= (decimal)OrganizationConstants.WORKING_HOUR_PER_DAY;
                    }

                }

                return productivityByStep;
            };

            return workloadInfos
                .GroupBy(w => new
                {
                    w.ProductionOrderId,
                    w.StepId,
                    w.ObjectId,
                    w.ObjectTypeId
                })
                .Select(g =>
                {

                    decimal? productivityByStep = getProductivity(g.Key.ObjectTypeId, g.Key.ObjectId, g.Key.StepId);

                    var totalWorkloadQuantity = g.Sum(w => w.Quantity * w.WorkloadConvertRate.Value);
                    return new ProductionCapacityDetailModel
                    {
                        ProductionOrderId = g.Key.ProductionOrderId,
                        StepId = g.Key.StepId,
                        ObjectId = g.Key.ObjectId,
                        ObjectTypeId = g.Key.ObjectTypeId,
                        Quantity = g.Sum(w => w.Quantity),
                        WorkloadQuantity = totalWorkloadQuantity,
                        TargetProductivity = productivityByStep ?? 0,
                        WorkHour = productivityByStep > 0 ? totalWorkloadQuantity / productivityByStep.Value : 0,
                        Details = g.Select(d =>
                        {

                            //var assign = productionAssignments.FirstOrDefault(a => a.ProductionStepLinkDataId == d.ProductionStepLinkDataId);
                            //if (assign != null)
                            //{
                            //    assignQuantity = assign.AssignmentQuantity;
                            //}

                            var departmentIds = productionAssignments1.Where(a => a.ProductionStepId == d.ProductionStepId).Select(a => a.DepartmentId).ToList();


                            var assignInfos = new List<CapacityAssignInfo>();
                            foreach (var depId in departmentIds)
                            {
                                decimal assignQuantity = 0;
                                bool isSelectionAssign = false;

                                var assignStep = productionAssignments1.FirstOrDefault(w => w.ProductionStepId == d.ProductionStepId && w.DepartmentId == depId);

                                var byDates = new List<ProductionAssignmentDetailModel>();

                                var workInfo = workloadInfos.FirstOrDefault(w => w.ProductionStepLinkDataId == assignStep.ProductionStepLinkDataId);
                                var byDateAssign = _mapper.Map<List<ProductionAssignmentDetailModel>>(assignStep.ProductionAssignmentDetail);
                                if (workInfo != null)
                                {
                                    var rateQuantiy = workInfo.Quantity > 0 ? assignStep.AssignmentQuantity / workInfo.Quantity : 0;

                                    assignQuantity = d.Quantity * rateQuantiy;

                                    byDates = byDateAssign.Select(a =>
                                    {
                                        var byDate = new ProductionAssignmentDetailModel()
                                        {
                                            WorkDate = a.WorkDate,
                                            QuantityPerDay = a.QuantityPerDay * rateQuantiy
                                        };

                                        var workloads = workloadInfos.Where(s => s.ProductionStepId == d.ProductionStepId).ToList();
                                        var workloadInfo = workloads.FirstOrDefault(w => w.ProductionStepLinkDataId == d.ProductionStepLinkDataId);


                                        decimal? totalWorkload = 0;
                                        decimal? totalHours = 0;
                                        foreach (var w in workloads)
                                        {
                                            var assignQuantity = workloadInfo.Quantity > 0 ? a.QuantityPerDay * w.Quantity / workloadInfo.Quantity : 0;
                                            var workload = assignQuantity * w.WorkloadConvertRate;
                                            var hour = productivityByStep > 0 ? workload / productivityByStep : 0;
                                            totalWorkload += workload;
                                            totalHours += hour;

                                        }

                                        byDate.SetWorkHourPerDay(totalHours);
                                        byDate.SetWorkloadPerDay(totalWorkload);

                                        return byDate;


                                    }).ToList();
                                }

                                isSelectionAssign = d.ProductionStepLinkDataId == assignStep.ProductionStepLinkDataId;

                                var wokloadQuantiy = assignQuantity * d.WorkloadConvertRate.Value;
                                assignInfos.Add(new CapacityAssignInfo()
                                {
                                    DepartmentId = depId,
                                    AssignQuantity = assignQuantity,
                                    AssignWorkloadQuantity = wokloadQuantiy,
                                    AssignWorkHour = productivityByStep > 0 ? wokloadQuantiy / productivityByStep.Value : 0,
                                    StartDate = assignStep?.StartDate,
                                    EndDate = assignStep?.EndDate,
                                    IsManualSetDate = assignStep.IsManualSetDate,
                                    RateInPercent = assignStep.RateInPercent,
                                    IsSelectionAssign = isSelectionAssign,
                                    ByDates = byDates
                                });
                            }


                            var currentDepartmentAssign = assignInfos.FirstOrDefault(a => a.DepartmentId == assignDepartmentId);

                            var workloadQuantity = d.Quantity * d.WorkloadConvertRate.Value;
                            return new ProductionStepWorkloadAssignModel
                            {
                                ProductionStepId = d.ProductionStepId,
                                ProductionStepTitle = d.ProductionStepTitle,
                                ProductionStepLinkDataId = d.ProductionStepLinkDataId,
                                Quantity = d.Quantity,
                                IsSelectionAssign = currentDepartmentAssign?.IsSelectionAssign ?? false,
                                WorkloadConvertRate = d.WorkloadConvertRate.Value,
                                WorkloadQuantity = workloadQuantity,

                                Productivity = productivityByStep ?? 0,

                                WorkHour = productivityByStep > 0 ? workloadQuantity / productivityByStep.Value : 0,

                                AssignQuantity = currentDepartmentAssign?.AssignQuantity,
                                AssignWorkloadQuantity = currentDepartmentAssign?.AssignWorkloadQuantity,
                                StartDate = currentDepartmentAssign?.StartDate?.GetUnix(),
                                EndDate = currentDepartmentAssign?.EndDate?.GetUnix(),
                                IsManualSetDate = currentDepartmentAssign?.IsManualSetDate ?? false,
                                RateInPercent = currentDepartmentAssign?.RateInPercent ?? 100,
                                ByDates = currentDepartmentAssign?.ByDates,
                                AssignWorkHour = currentDepartmentAssign?.AssignWorkHour,
                                AssignInfos = assignInfos
                            };
                        }).ToList()
                    };
                })
                .GroupBy(w => w.ProductionOrderId)

                .ToCustomDictionary(new CapacityStepByProduction(), w => w.Key, w => w.GroupBy(c => c.StepId).ToCustomDictionary(new CapacityByStep(), c => c.Key, c => c.ToIList()));

        }


        public async Task<IList<ProductionOrderExtraInfo>> GetProductionOrderExtraInfo(long orderId)
        {
            var parammeters = new SqlParameter[]
            {
                new SqlParameter("@OrderId", orderId)
            };
            var resultData = await _manufacturingDBContext.ExecuteDataProcedure("asp_ProductionOrder_GetExtraInfoByOrderId", parammeters);

            return resultData.ConvertData<ProductionOrderExtraInfo>();
        }

        public async Task<ProductionOrderOutputModel> GetProductionOrder(long productionOrderId)
        {
            var productOrder = _manufacturingDBContext.ProductionOrder
                .Include(x => x.ProductionOrderAttachment)
                .FirstOrDefault(o => o.ProductionOrderId == productionOrderId);

            ProductionOrderOutputModel model = null;

            if (productOrder != null)
            {

                model = _mapper.Map<ProductionOrderOutputModel>(productOrder);

                var sql = $"SELECT * FROM vProductionOrderDetail WHERE ProductionOrderId = @ProductionOrderId";
                var parammeters = new SqlParameter[]
                {
                    new SqlParameter("@ProductionOrderId", productionOrderId)
                };
                var resultData = await _manufacturingDBContext.QueryDataTable(sql, parammeters);

                model.ProductionOrderDetail = resultData.ConvertData<ProductionOrderDetailOutputModel>();

            }
            else
            {
                throw new BadRequestException(GeneralCode.InvalidParams, "Lệnh SX không tồn tại");
            }

            return model;
        }

        public async Task<IList<ProductionOrderDetailByOrder>> GetProductionHistoryByOrder(IList<string> orderCodes, IList<int> productIds)
        {
            var procDetails = _manufacturingDBContext.ProductionOrderDetail.AsQueryable();
            if (productIds?.Count > 0)
            {
                procDetails = procDetails.Where(d => productIds.Contains(d.ProductId));
            }

            return await (
                 from o in _manufacturingDBContext.ProductionOrder
                 join d in procDetails on o.ProductionOrderId equals d.ProductionOrderId
                 where orderCodes.Contains(d.OrderCode)
                 select new ProductionOrderDetailByOrder
                 {
                     ProductionOrderId = o.ProductionOrderId,
                     ProductionOrderCode = o.ProductionOrderCode,
                     Date = o.Date,
                     Description = o.Description,
                     ProductionOrderDetailId = d.ProductionOrderDetailId,
                     ProductId = d.ProductId,
                     OrderCode = d.OrderCode,
                     Quantity = d.Quantity,
                     ReserveQuantity = d.ReserveQuantity,
                     Note = d.Note
                 }).ToListAsync();
        }

        public async Task<IList<OrderProductInfo>> GetOrderProductInfo(IList<long> productionOderIds)
        {
            var result = await _manufacturingDBContext.ProductionOrderDetail
                .Where(pod => productionOderIds.Contains(pod.ProductionOrderId))
                .Select(pod => new OrderProductInfo
                {
                    ProductionOrderId = pod.ProductionOrderId,
                    ProductionOrderDetailId = pod.ProductionOrderDetailId,
                    OrderDetailId = pod.OrderDetailId,
                    ProductId = pod.ProductId
                })
                .ToListAsync();
            return result;
        }

        public async Task<ProductionOrderInputModel> CreateProductionOrder(ProductionOrderInputModel data)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockProductionOrderKey(0));
            using var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                var config = await GetProductionOrderConfiguration();

                if (config != null && !config.IsEnablePlanEndDate)
                {
                    data.PlanEndDate = data.EndDate;
                }

                if (data.StartDate <= 0) throw new BadRequestException(GeneralCode.InvalidParams, "Yêu cầu nhập ngày bắt đầu sản xuất.");
                if (data.EndDate <= 0) throw new BadRequestException(GeneralCode.InvalidParams, "Yêu cầu nhập ngày kết thúc sản xuất.");
                if (data.PlanEndDate <= 0) throw new BadRequestException(GeneralCode.InvalidParams, "Yêu cầu nhập ngày kết thúc hàng trắng.");
                if (data.StartDate > data.PlanEndDate) throw new BadRequestException(GeneralCode.InvalidParams, "Ngày bắt đầu không được lớn hơn ngày kết thúc hàng trắng. Vui lòng chọn lại kế hoạch sản xuất!");
                if (data.PlanEndDate > data.EndDate) throw new BadRequestException(GeneralCode.InvalidParams, "Ngày kết thúc hàng trắng không được lớn hơn ngày kết thúc. Vui lòng chọn lại kế hoạch sản xuất!");
                if (data.Date <= 0) throw new BadRequestException(GeneralCode.InvalidParams, "Yêu cầu nhập ngày chứng từ.");

                if (data.ProductionOrderDetail.Count == 0)
                    throw new BadRequestException(GeneralCode.InvalidParams, "Cần có thông tin mặt hàng cần sản xuất");

                if (data.ProductionOrderDetail.GroupBy(x => new { x.ProductId, x.OrderCode })
                    .Where(x => x.Count() > 1)
                    .Count() > 0)
                    throw new BadRequestException(GeneralCode.InvalidParams, "Xuất hiện mặt hàng trùng lặp trong lệch sản xuất");

                CustomGenCodeOutputModel currentConfig = null;
                if (string.IsNullOrEmpty(data.ProductionOrderCode))
                {
                    currentConfig = await _customGenCodeHelperService.CurrentConfig(EnumObjectType.ProductionOrder, EnumObjectType.ProductionOrder, 0, null, data.ProductionOrderCode, data.StartDate);
                    if (currentConfig == null)
                    {
                        throw new BadRequestException(GeneralCode.ItemNotFound, "Chưa thiết định cấu hình sinh mã");
                    }
                    bool isFirst = true;
                    do
                    {
                        if (!isFirst) await _customGenCodeHelperService.ConfirmCode(currentConfig?.CurrentLastValue);

                        var generated = await _customGenCodeHelperService.GenerateCode(currentConfig.CustomGenCodeId, currentConfig.CurrentLastValue.LastValue, null, data.ProductionOrderCode, data.StartDate);
                        if (generated == null)
                        {
                            throw new BadRequestException(GeneralCode.InternalError, "Không thể sinh mã ");
                        }
                        data.ProductionOrderCode = generated.CustomCode;
                        isFirst = false;
                    } while (_manufacturingDBContext.ProductionOrder.Any(o => o.ProductionOrderCode == data.ProductionOrderCode));
                }
                else
                {
                    // Validate unique
                    if (_manufacturingDBContext.ProductionOrder.Any(o => o.ProductionOrderCode == data.ProductionOrderCode))
                        throw new BadRequestException(ProductOrderErrorCode.ProductOrderCodeAlreadyExisted);
                }
                var productionOrder = await SaveProductionOrder(data);
                trans.Commit();
                data.ProductionOrderId = productionOrder.ProductionOrderId;

                await _customGenCodeHelperService.ConfirmCode(currentConfig?.CurrentLastValue);

                await _activityLogService.CreateLog(EnumObjectType.ProductionOrder, productionOrder.ProductionOrderId, $"Thêm mới dữ liệu lệnh sản xuất {productionOrder.ProductionOrderCode}", data.JsonSerialize());

                return data;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "CreateProductOrder");
                throw;
            }
        }

        private async Task<ProductionOrderEntity> SaveProductionOrder(ProductionOrderInputModel data, int? monthPlanId = null)
        {
            var productionOrder = _mapper.Map<ProductionOrderEntity>(data);
            productionOrder.IsResetProductionProcess = false;
            productionOrder.IsInvalid = true;
            productionOrder.ProductionOrderStatus = (int)EnumProductionStatus.NotReady;
            _manufacturingDBContext.ProductionOrder.Add(productionOrder);
            await _manufacturingDBContext.SaveChangesAsync();

            var extraPlans = new List<(ProductionOrderDetail Entity, int SortOrder)>();

            // Tạo detail
            foreach (var item in data.ProductionOrderDetail)
            {
                item.ProductionOrderDetailId = 0;
                item.ProductionOrderId = productionOrder.ProductionOrderId;

                // Tạo mới
                var entity = _mapper.Map<ProductionOrderDetail>(item);
                entity.ProductionProcessVersion = await _productHelperService.GetProductionProcessVersion(entity.ProductId);

                if (monthPlanId.HasValue && item.SortOrder.HasValue) extraPlans.Add((entity, item.SortOrder.Value));
                _manufacturingDBContext.ProductionOrderDetail.Add(entity);
            }

            // Tạo đính kèm
            foreach (var attach in data.ProductionOrderAttachment)
            {
                attach.ProductionOrderId = productionOrder.ProductionOrderId;

                var entityAttach = _mapper.Map<ProductionOrderAttachment>(attach);

                _manufacturingDBContext.ProductionOrderAttachment.Add(entityAttach);
            }

            await _manufacturingDBContext.SaveChangesAsync();
            if (monthPlanId.HasValue)
            {
                foreach (var extraPlan in extraPlans)
                {
                    var entityInfo = new ProductionPlanExtraInfo
                    {
                        MonthPlanId = monthPlanId.Value,
                        ProductionOrderDetailId = extraPlan.Entity.ProductionOrderDetailId,
                        Note = extraPlan.Entity.Note,
                        SortOrder = extraPlan.SortOrder
                    };
                    _manufacturingDBContext.ProductionPlanExtraInfo.Add(entityInfo);
                }

                await _manufacturingDBContext.SaveChangesAsync();

            }

            return productionOrder;
        }

        public async Task<int> CreateMultipleProductionOrder(int monthPlanId, ProductionOrderInputModel[] data)
        {
            if (data.Length == 0) return 0;
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockProductionOrderKey(0));
            using var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                var startDate = data.Min(d => d.StartDate);
                CustomGenCodeOutputModel currentConfig = await _customGenCodeHelperService.CurrentConfig(EnumObjectType.ProductionOrder, EnumObjectType.ProductionOrder, 0, null, null, startDate);
                if (currentConfig == null)
                {
                    throw new BadRequestException(GeneralCode.ItemNotFound, "Chưa thiết định cấu hình sinh mã");
                }
                int currentValue = currentConfig.CurrentLastValue.LastValue;
                string currentCode = currentConfig.CurrentLastValue.LastCode;
                foreach (var item in data)
                {
                    if (item.StartDate <= 0) throw new BadRequestException(GeneralCode.InvalidParams, "Yêu cầu nhập ngày bắt đầu sản xuất.");
                    if (item.EndDate <= 0) throw new BadRequestException(GeneralCode.InvalidParams, "Yêu cầu nhập ngày kết thúc sản xuất.");
                    if (item.PlanEndDate <= 0) throw new BadRequestException(GeneralCode.InvalidParams, "Yêu cầu nhập ngày kết thúc hàng trắng.");
                    if (item.Date <= 0) throw new BadRequestException(GeneralCode.InvalidParams, "Yêu cầu nhập ngày chứng từ.");
                    if (item.StartDate > item.PlanEndDate) throw new BadRequestException(GeneralCode.InvalidParams, "Ngày bắt đầu không được lớn hơn ngày kết thúc hàng trắng. Vui lòng chọn lại kế hoạch sản xuất!");
                    if (item.PlanEndDate > item.EndDate) throw new BadRequestException(GeneralCode.InvalidParams, "Ngày kết thúc hàng trắng không được lớn hơn ngày kết thúc. Vui lòng chọn lại kế hoạch sản xuất!");
                    if (item.ProductionOrderDetail.GroupBy(x => new { x.ProductId, x.OrderCode })
                        .Where(x => x.Count() > 1)
                        .Count() > 0)
                        throw new BadRequestException(GeneralCode.InvalidParams, "Xuất hiện mặt hàng trùng lặp trong lệch sản xuất");



                    //string currentCode = currentConfig.CurrentLastValue.LastCode;
                    do
                    {
                        var generated = await _customGenCodeHelperService.GenerateCode(currentConfig.CustomGenCodeId, currentValue, null, currentCode, item.StartDate);
                        if (generated == null)
                        {
                            throw new BadRequestException(GeneralCode.InternalError, "Không thể sinh mã ");
                        }
                        item.ProductionOrderCode = generated.CustomCode;
                        currentValue = generated.LastValue;
                        currentCode = generated.CustomCode;
                    } while (_manufacturingDBContext.ProductionOrder.Any(o => o.ProductionOrderCode == item.ProductionOrderCode));
                }
                long productionOrderId = 0;
                foreach (var item in data)
                {
                    var productionOrder = await SaveProductionOrder(item, monthPlanId);
                    productionOrderId = productionOrder.ProductionOrderId;
                }

                // Xóa dữ liệu nháp
                await _draftDataHelperService.DeleteDraftData((int)EnumObjectType.DraftData, monthPlanId);

                trans.Commit();
                currentConfig.CurrentLastValue.LastValue = currentValue;
                currentConfig.CurrentLastValue.LastCode = currentCode;

                await _customGenCodeHelperService.ConfirmCode(currentConfig?.CurrentLastValue);

                await _activityLogService.CreateLog(EnumObjectType.ProductionOrder, productionOrderId, $"Thêm {data.Length} lệnh sản xuất từ kế hoạch", data.JsonSerialize());

                return data.Length;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "CreateProductOrders");
                throw;
            }
        }

        public async Task<ProductionOrderInputModel> UpdateProductionOrder(long productionOrderId, ProductionOrderInputModel data)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockProductionOrderKey(productionOrderId));
            using var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                if (data.StartDate <= 0) throw new BadRequestException(GeneralCode.InvalidParams, "Yêu cầu nhập ngày bắt đầu sản xuất.");
                if (data.EndDate <= 0) throw new BadRequestException(GeneralCode.InvalidParams, "Yêu cầu nhập ngày kết thúc sản xuất.");
                if (data.PlanEndDate <= 0) throw new BadRequestException(GeneralCode.InvalidParams, "Yêu cầu nhập ngày kết thúc hàng trắng.");
                if (data.StartDate > data.PlanEndDate) throw new BadRequestException(GeneralCode.InvalidParams, "Ngày bắt đầu không được lớn hơn ngày kết thúc hàng trắng. Vui lòng chọn lại kế hoạch sản xuất!");
                if (data.PlanEndDate > data.EndDate) throw new BadRequestException(GeneralCode.InvalidParams, "Ngày kết thúc hàng trắng không được lớn hơn ngày kết thúc. Vui lòng chọn lại kế hoạch sản xuất!");
                if (data.Date <= 0) throw new BadRequestException(GeneralCode.InvalidParams, "Yêu cầu nhập ngày chứng từ.");
                if (data.ProductionOrderDetail.GroupBy(x => new { x.ProductId, x.OrderCode })
                    .Where(x => x.Count() > 1)
                    .Count() > 0)
                    throw new BadRequestException(GeneralCode.InvalidParams, "Xuất hiện mặt hàng trùng lặp trong lệch sản xuất");

                if (data.ProductionOrderDetail.Count == 0)
                    throw new BadRequestException(GeneralCode.InvalidParams, "Cần có thông tin mặt hàng cần sản xuất");

                var productionOrder = _manufacturingDBContext.ProductionOrder
                    .Where(o => o.ProductionOrderId == productionOrderId)
                    .FirstOrDefault();

                bool invalidPlan = productionOrder.StartDate.GetUnix() != data.StartDate || productionOrder.EndDate.GetUnix() != data.EndDate;

                if (productionOrder == null) throw new BadRequestException(ProductOrderErrorCode.ProductOrderNotfound);
                _mapper.Map(data, productionOrder);

                // Kiểm tra quy trình sản xuất có đầy đủ đầu ra trong lệnh sản xuất mới chưa => nếu chưa đặt lại trạng thái sản xuất về đang thiết lập
                var productIds = data.ProductionOrderDetail.Select(od => (long)od.ProductId).ToList();
                // Lấy ra thông tin đầu ra nhập kho trong quy trình
                var processProductIds = (
                        from ld in _manufacturingDBContext.ProductionStepLinkData
                        join r in _manufacturingDBContext.ProductionStepLinkDataRole on ld.ProductionStepLinkDataId equals r.ProductionStepLinkDataId
                        join ps in _manufacturingDBContext.ProductionStep on r.ProductionStepId equals ps.ProductionStepId
                        where ps.ContainerId == productionOrderId
                        && ps.ContainerTypeId == (int)EnumContainerType.ProductionOrder
                        && ld.LinkDataObjectTypeId == (int)EnumProductionStepLinkDataObjectType.Product
                        && productIds.Contains(ld.LinkDataObjectId)
                        && r.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Output
                        select ld.LinkDataObjectId
                    )
                    .Distinct()
                    .ToList();
                var includeProductIds = productIds.Where(p => !processProductIds.Any(d => d == p)).ToList();
                if (includeProductIds.Count > 0)
                {
                    productionOrder.ProductionOrderStatus = (int)EnumProductionStatus.NotReady;
                }

                var oldDetail = _manufacturingDBContext.ProductionOrderDetail.Where(od => od.ProductionOrderId == productionOrderId).ToList();
                var oldDetailIds = oldDetail.Select(d => d.ProductionOrderDetailId).ToList();
                // Biến kiểm tra thay đổi số lượng
                bool? isUpdateQuantity = productionOrder.IsUpdateQuantity;

                foreach (var item in data.ProductionOrderDetail)
                {
                    item.ProductionOrderId = productionOrderId;
                    var oldItem = oldDetail.Where(od => od.ProductionOrderDetailId == item.ProductionOrderDetailId).FirstOrDefault();
                    if (oldItem != null)
                    {
                        if (oldItem.ProductId != item.ProductId || oldItem.Quantity != item.Quantity || oldItem.ReserveQuantity != item.ReserveQuantity)
                        {
                            isUpdateQuantity = true;
                        }
                        // Cập nhật
                        invalidPlan = invalidPlan || oldItem.ProductId != item.ProductId || oldItem.Quantity != item.Quantity || oldItem.ReserveQuantity != item.ReserveQuantity;
                        _mapper.Map(item, oldItem);
                        // Gỡ khỏi danh sách cũ
                        oldDetail.Remove(oldItem);
                    }
                    else
                    {
                        isUpdateQuantity = true;
                        invalidPlan = true;
                        item.ProductionOrderDetailId = 0;
                        // Tạo mới
                        var entity = _mapper.Map<ProductionOrderDetail>(item);
                        _manufacturingDBContext.ProductionOrderDetail.Add(entity);
                    }
                }

                productionOrder.IsUpdateQuantity = isUpdateQuantity;

                var oldAttach = _manufacturingDBContext.ProductionOrderAttachment.Where(att => att.ProductionOrderId == productionOrderId).ToList();

                foreach (var item in data.ProductionOrderAttachment)
                {
                    item.ProductionOrderId = productionOrderId;
                    var oldItem = oldAttach.Where(od => od.ProductionOrderAttachmentId == item.ProductionOrderAttachmentId).FirstOrDefault();

                    if (oldItem != null)
                    {
                        // Cập nhật
                        _mapper.Map(item, oldItem);
                        // Gỡ khỏi danh sách cũ
                        oldAttach.Remove(oldItem);
                    }
                    else
                    {
                        item.ProductionOrderAttachmentId = 0;
                        // Tạo mới
                        var entity = _mapper.Map<ProductionOrderAttachment>(item);
                        _manufacturingDBContext.ProductionOrderAttachment.Add(entity);
                    }
                }

                await _manufacturingDBContext.SaveChangesAsync();

                invalidPlan = invalidPlan || oldDetail.Count > 0;
                if (invalidPlan && _manufacturingDBContext.ProductionWeekPlan.Any(wp => oldDetailIds.Contains(wp.ProductionOrderDetailId)))
                {
                    productionOrder.InvalidPlan = true;
                }

                // Xóa chi tiết
                foreach (var item in oldDetail)
                {
                    if (_manufacturingDBContext.OutsourcePartRequest.Any(x => x.ProductionOrderDetailId == item.ProductionOrderDetailId))
                        throw new BadRequestException(GeneralCode.InvalidParams, "Tồn tại đơn gia công chi tiết của mặt hàng bị xóa bỏ. Phải xóa yêu cầu gia công liên quan trước.");

                    item.IsDeleted = true;
                }

                // Xóa đính kèm
                foreach (var item in oldAttach)
                {
                    item.IsDeleted = true;
                }

                await _manufacturingDBContext.SaveChangesAsync();

                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.ProductionOrder, productionOrder.ProductionOrderId, $"Cập nhật dữ liệu lệnh sản xuất {productionOrder.ProductionOrderCode}", data.JsonSerialize());
                return data;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "UpdateProductOrder");
                throw;
            }
        }

        public async Task<bool> DeleteProductionOrder(long productionOrderId)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockProductionOrderKey(productionOrderId));
            using var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                var productionOrder = _manufacturingDBContext.ProductionOrder
                    .Where(o => o.ProductionOrderId == productionOrderId)
                    .FirstOrDefault();
                if (productionOrder == null) throw new BadRequestException(ProductOrderErrorCode.ProductOrderNotfound);
                productionOrder.IsDeleted = true;

                var detail = _manufacturingDBContext.ProductionOrderDetail.Where(od => od.ProductionOrderId == productionOrderId).ToList();

                var attach = _manufacturingDBContext.ProductionOrderAttachment.Where(od => od.ProductionOrderId == productionOrderId).ToList();

                var delIds = detail.Select(od => od.ProductionOrderDetailId).ToList();

                // Xóa quy trình sản xuất
                var steps = _manufacturingDBContext.ProductionStep.Where(s => s.ContainerId == productionOrderId && s.ContainerTypeId == (int)EnumContainerType.ProductionOrder).ToList();
                var stepIds = steps.Select(ps => ps.ProductionStepId).ToList();
                var linkDataRoles = _manufacturingDBContext.ProductionStepLinkDataRole.Where(r => stepIds.Contains(r.ProductionStepId)).ToList();
                var linkDataIds = linkDataRoles.Select(r => r.ProductionStepLinkDataId).Distinct().ToList();
                var linkDatas = _manufacturingDBContext.ProductionStepLinkData.Where(d => linkDataIds.Contains(d.ProductionStepLinkDataId)).ToList();

                // Xóa role
                _manufacturingDBContext.ProductionStepLinkDataRole.RemoveRange(linkDataRoles);
                await _manufacturingDBContext.SaveChangesAsync();
                // Xóa step
                foreach (var item in steps)
                {
                    item.IsDeleted = true;
                }
                // Xóa link data
                foreach (var item in linkDatas)
                {
                    item.IsDeleted = true;
                }

                // Xóa chi tiết
                foreach (var item in detail)
                {
                    item.IsDeleted = true;
                }

                // Xóa chi tiết
                foreach (var item in attach)
                {
                    item.IsDeleted = true;
                }

                await _manufacturingDBContext.SaveChangesAsync();
                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.ProductionOrder, productionOrder.ProductionOrderId, $"Xóa lệnh sản xuất {productionOrder.ProductionOrderCode}", productionOrder.JsonSerialize());
                return true;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "DeleteProductOrder");
                throw;
            }
        }

        public async Task<ProductionOrderDetailOutputModel> GetProductionOrderDetail(long? productionOrderDetailId)
        {

            if (productionOrderDetailId.HasValue)
            {
                var sql = $"SELECT * FROM vProductionOrderDetail WHERE ProductionOrderDetailId = @ProductionOrderDetailId";
                var parammeters = new SqlParameter[]
                {
                    new SqlParameter("@ProductionOrderDetailId", productionOrderDetailId.Value)
                };
                var resultData = await _manufacturingDBContext.QueryDataTable(sql, parammeters);

                var rs = resultData.ConvertData<ProductionOrderDetailOutputModel>().FirstOrDefault();
                return rs;
            }

            return null;
        }

        public async Task<IList<ProductOrderModel>> GetProductionOrders()
        {
            var rs = await _manufacturingDBContext.ProductionOrder.AsNoTracking()
                .Select(x => new ProductOrderModel
                {
                    Description = x.Description,
                    EndDate = x.EndDate.GetUnix(),
                    IsDraft = x.IsDraft,
                    StartDate = x.StartDate.GetUnix(),
                    ProductionOrderCode = x.ProductionOrderCode,
                    ProductionOrderId = x.ProductionOrderId
                })
                .ToListAsync();
            return rs;
        }



        public async Task<bool> UpdateProductionOrderStatus(ProductionOrderStatusDataModel data)
        {
            var productionOrder = _manufacturingDBContext.ProductionOrder
                .Include(po => po.ProductionOrderDetail)
                .FirstOrDefault(po => po.ProductionOrderCode == data.ProductionOrderCode);

            if (productionOrder == null)
                throw new BadRequestException(GeneralCode.ItemNotFound, "Lệnh sản xuất không tồn tại");

            try
            {
                if (data.ProductionOrderStatus == EnumProductionStatus.Finished)
                {
                    // Check nhận đủ số lượng đầu ra

                    var inputInventories = data.Inventories;

                    bool isFinish = true;

                    foreach (var productionOrderDetail in productionOrder.ProductionOrderDetail)
                    {
                        var quantity = inputInventories
                            .Where(i => i.ProductId == productionOrderDetail.ProductId && i.Status != (int)EnumProductionInventoryRequirementStatus.Rejected)
                            .Sum(i => i.ActualQuantity);

                        if (quantity < (productionOrderDetail.Quantity + productionOrderDetail.ReserveQuantity))
                        {
                            isFinish = false;
                            break;
                        }
                    }
                    if (isFinish)
                    {
                        productionOrder.ProductionOrderStatus = (int)data.ProductionOrderStatus;
                        await _activityLogService.CreateLog(EnumObjectType.ProductionOrder, productionOrder.ProductionOrderId, $"Cập nhật trạng thái lệnh sản xuất ", new { productionOrder, data, isManual = false }.JsonSerialize());
                    }
                }
                else
                {

                    if (productionOrder.ProductionOrderStatus < (int)data.ProductionOrderStatus)
                    {
                        productionOrder.ProductionOrderStatus = (int)data.ProductionOrderStatus;
                        await _activityLogService.CreateLog(EnumObjectType.ProductionOrder, productionOrder.ProductionOrderId, $"Cập nhật trạng thái lệnh sản xuất ", new { productionOrder, data, isManual = false }.JsonSerialize());
                    }
                }
                _manufacturingDBContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateProductOrderStatus");
                throw;
            }
        }

        public async Task<bool> UpdateManualProductionOrderStatus(long productionOrderId, ProductionOrderStatusDataModel status)
        {
            var productionOrder = _manufacturingDBContext.ProductionOrder.FirstOrDefault(po => po.ProductionOrderId == productionOrderId);
            if (productionOrder == null)
                throw new BadRequestException(GeneralCode.ItemNotFound, "Lệnh sản xuất không tồn tại");

            if (productionOrder.ProductionOrderStatus > (int)status.ProductionOrderStatus)
                throw new BadRequestException(GeneralCode.ItemNotFound, "Không được phép cập nhật ngược trạng thái");

            try
            {
                if (productionOrder.ProductionOrderStatus != (int)status.ProductionOrderStatus)
                {
                    productionOrder.ProductionOrderStatus = (int)status.ProductionOrderStatus;
                    await _activityLogService.CreateLog(EnumObjectType.ProductionOrder, productionOrder.ProductionOrderId, $"Cập nhật trạng thái lệch sản xuất ", new { productionOrder, status, isManual = true }.JsonSerialize());
                }
                _manufacturingDBContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateProductOrderStatus");
                throw;
            }
        }

        public async Task<bool> EditNote(long productionOrderDetailId, string note)
        {
            var productionOrderDetail = await _manufacturingDBContext.ProductionOrderDetail.FirstOrDefaultAsync(pod => pod.ProductionOrderDetailId == productionOrderDetailId);
            if (productionOrderDetail == null)
                throw new BadRequestException(GeneralCode.ItemNotFound, "Lệnh sản xuất không tồn tại");

            try
            {
                productionOrderDetail.Note = note;
                _manufacturingDBContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateProductOrderNote");
                throw;
            }
        }

        public async Task<bool> EditDate(long[] productionOrderDetailId, long startDate, long planEndDate, long endDate)
        {
            if (startDate <= 0) throw new BadRequestException(GeneralCode.InvalidParams, "Yêu cầu nhập ngày bắt đầu sản xuất.");
            if (endDate <= 0) throw new BadRequestException(GeneralCode.InvalidParams, "Yêu cầu nhập ngày kết thúc sản xuất.");
            if (planEndDate <= 0) throw new BadRequestException(GeneralCode.InvalidParams, "Yêu cầu nhập ngày kết thúc hàng trắng.");
            if (startDate > planEndDate) throw new BadRequestException(GeneralCode.InvalidParams, "Ngày bắt đầu không được lớn hơn ngày kết thúc hàng trắng. Vui lòng chọn lại kế hoạch sản xuất!");
            if (planEndDate > endDate) throw new BadRequestException(GeneralCode.InvalidParams, "Ngày kết thúc hàng trắng không được lớn hơn ngày kết thúc. Vui lòng chọn lại kế hoạch sản xuất!");

            var productionOrderDetails = await _manufacturingDBContext.ProductionOrderDetail
                .Where(pod => productionOrderDetailId.Contains(pod.ProductionOrderDetailId))
                .ToListAsync();

            var productionOrderIds = productionOrderDetails.Select(pod => pod.ProductionOrderId).Distinct().ToList();

            var productionOrders = _manufacturingDBContext.ProductionOrder.Where(po => productionOrderIds.Contains(po.ProductionOrderId)).ToList();

            if (productionOrders.Count == 0)
                throw new BadRequestException(GeneralCode.ItemNotFound, "Lệnh sản xuất không tồn tại");

            var validStatuses = new[] {
                EnumProductionStatus.NotReady,
                EnumProductionStatus.Waiting
            };

            // Validate trạng thái
            if (productionOrders.Any(po => !validStatuses.Contains((EnumProductionStatus)po.ProductionOrderStatus)))
                throw new BadRequestException(GeneralCode.ItemNotFound, "Lệnh sản xuất đã đi vào sản xuất");

            try
            {
                foreach (var productionOrder in productionOrders)
                {
                    productionOrder.StartDate = startDate.UnixToDateTime().Value;
                    productionOrder.EndDate = endDate.UnixToDateTime().Value;
                    productionOrder.PlanEndDate = planEndDate.UnixToDateTime().Value;
                }
                _manufacturingDBContext.SaveChanges();

                foreach (var productionOrder in productionOrders)
                {
                    await _activityLogService.CreateLog(EnumObjectType.ProductionOrder, productionOrder.ProductionOrderId, $"Cập nhật thời gian lệch sản xuất từ kế hoạch", productionOrder.JsonSerialize());
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateProductOrderDateTime");
                throw;
            }
        }

        #region Production Order Configuration
        public async Task<ProductionOrderConfigurationModel> GetProductionOrderConfiguration()
        {
            var entity = await _manufacturingDBContext.ProductionOrderConfiguration.FirstOrDefaultAsync();
            if (entity == null) return new ProductionOrderConfigurationModel();

            return _mapper.Map<ProductionOrderConfigurationModel>(entity);
        }

        public async Task<bool> UpdateProductionOrderConfiguration(ProductionOrderConfigurationModel model)
        {
            var entity = await _manufacturingDBContext.ProductionOrderConfiguration.FirstOrDefaultAsync();
            if (entity == null)
            {
                await _manufacturingDBContext.ProductionOrderConfiguration.AddAsync(_mapper.Map<ProductionOrderConfiguration>(model));
            }
            else
            {
                entity.IsEnablePlanEndDate = model.IsEnablePlanEndDate;
                entity.NumberOfDayPed = model.NumberOfDayPed;
            };

            await _manufacturingDBContext.SaveChangesAsync();
            return true;
        }
        #endregion
    }



}
