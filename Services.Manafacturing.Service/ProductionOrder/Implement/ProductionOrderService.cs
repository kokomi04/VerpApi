using AutoMapper;
using AutoMapper.QueryableExtensions;
using DocumentFormat.OpenXml.EMMA;
using Grpc.Core;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NPOI.SS.Formula.Functions;
using OpenXmlPowerTools;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verp.Cache.RedisCache;
using Verp.Resources.Manafacturing.Production;
using Verp.Resources.Master.Config.ActionButton;
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
using VErp.Infrastructure.ServiceCore.CrossServiceHelper.QueueHelper;
using VErp.Infrastructure.ServiceCore.Facade;
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
        private readonly ObjectActivityLogFacade _objActivityLogFacade;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly ICustomGenCodeHelperService _customGenCodeHelperService;
        private readonly IProductHelperService _productHelperService;
        private readonly IOrganizationHelperService _organizationHelperService;
        private readonly IDraftDataHelperService _draftDataHelperService;
        private readonly ICurrentContextService _currentContextService;
        private readonly IProductionOrderQueueHelperService _productionOrderQueueHelperService;

        public ProductionOrderService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<ProductionOrderService> logger
            , IMapper mapper
            , ICustomGenCodeHelperService customGenCodeHelperService
            , IProductHelperService productHelperService
            , IOrganizationHelperService organizationHelperService
            , IDraftDataHelperService draftDataHelperService
            , ICurrentContextService currentContextService
            , IProductionOrderQueueHelperService productionOrderQueueHelperService)
        {
            _manufacturingDBContext = manufacturingDB;
            _objActivityLogFacade = activityLogService.CreateObjectTypeActivityLog(EnumObjectType.ProductionOrder);
            _logger = logger;
            _mapper = mapper;
            _customGenCodeHelperService = customGenCodeHelperService;
            _productHelperService = productHelperService;
            _organizationHelperService = organizationHelperService;
            _draftDataHelperService = draftDataHelperService;
            _currentContextService = currentContextService;
            _productionOrderQueueHelperService = productionOrderQueueHelperService;
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

        public async Task<IList<ProductionOrderOutputModel>> GetProductionOrdersByCodes(IList<string> productionOrderCodes)
        {
            if (productionOrderCodes.Count == 0) return Array.Empty<ProductionOrderOutputModel>();

            var productOrders = _manufacturingDBContext.ProductionOrder
                .Include(x => x.ProductionOrderAttachment)
                .Where(o => productionOrderCodes.Contains(o.ProductionOrderCode));
            var sql = $"SELECT * FROM vProductionOrderDetail WHERE ProductionOrderCode IN (SELECT [NValue] FROM @ProductionOrderCodes)";
            var parammeters = new SqlParameter[]
            {
                    productionOrderCodes.ToSqlParameter("@ProductionOrderCodes")
            };
            var resultData = await _manufacturingDBContext.QueryDataTableRaw(sql, parammeters);

            var details = resultData.ConvertData<ProductionOrderDetailOutputModel>();


            var lst = new List<ProductionOrderOutputModel>();
            foreach (var productOrder in productOrders)
            {


                var model = _mapper.Map<ProductionOrderOutputModel>(productOrder);


                model.ProductionOrderDetail = details.Where(d => d.ProductionOrderId == productOrder.ProductionOrderId).ToList();
                lst.Add(model);

            }

            return lst;
        }

        public async Task<IList<ProductionOrderOutputModel>> GetProductionOrdersByIds(IList<long> productionOrderIds)
        {
            if (productionOrderIds.Count == 0) return Array.Empty<ProductionOrderOutputModel>();

            var productOrders = _manufacturingDBContext.ProductionOrder
                .Include(x => x.ProductionOrderAttachment)
                .Where(o => productionOrderIds.Contains(o.ProductionOrderId));
            var sql = $"SELECT * FROM vProductionOrderDetail WHERE ProductionOrderId IN (SELECT [Value] FROM @ProductionOrderIds)";
            var parammeters = new SqlParameter[]
            {
                    productionOrderIds.ToSqlParameter("@ProductionOrderIds")
            };
            var resultData = await _manufacturingDBContext.QueryDataTableRaw(sql, parammeters);

            var details = resultData.ConvertData<ProductionOrderDetailOutputModel>();


            var lst = new List<ProductionOrderOutputModel>();
            foreach (var productOrder in productOrders)
            {


                var model = _mapper.Map<ProductionOrderOutputModel>(productOrder);


                model.ProductionOrderDetail = details.Where(d => d.ProductionOrderId == productOrder.ProductionOrderId).ToList();
                lst.Add(model);

            }

            return lst;
        }

        public async Task<PageData<ProductionOrderListModel>> GetProductionOrders(int? monthPlanId, int? factoryDepartmentId, string keyword, int page, int size, string orderByFieldName, bool asc, long fromDate, long toDate, bool? hasNewProductionProcessVersion = null, Clause filters = null)
        {
            keyword = (keyword ?? "").Trim();
            var parammeters = new List<SqlParameter>();

            var whereCondition = new StringBuilder();

            if (monthPlanId > 0)
            {
                if (whereCondition.Length > 0)
                    whereCondition.Append(" AND ");
                whereCondition.Append("v.MonthPlanId = @MonthPlanId ");

                parammeters.Add(new SqlParameter("@MonthPlanId", monthPlanId));
            }

            if (factoryDepartmentId > 0)
            {
                if (whereCondition.Length > 0)
                    whereCondition.Append(" AND ");
                whereCondition.Append("v.FactoryDepartmentId = @FactoryDepartmentId ");

                parammeters.Add(new SqlParameter("@FactoryDepartmentId", factoryDepartmentId));
            }

            if (!string.IsNullOrEmpty(keyword))
            {
                if (whereCondition.Length > 0)
                    whereCondition.Append(" AND ");

                whereCondition.Append("(v.ProductionOrderCode LIKE @KeyWord ");
                whereCondition.Append("OR v.ProductCode LIKE @Keyword ");
                whereCondition.Append("OR v.ProductName LIKE @Keyword ");
                whereCondition.Append("OR v.CustomerPO LIKE @Keyword ");
                whereCondition.Append("OR v.OrderCode LIKE @Keyword ");
                whereCondition.Append("OR v.Description LIKE @Keyword ) ");
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
                suffix = filters.FilterClauseProcess("vProductionOrderDetail", "v", filterCondition, parammeters, suffix);
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


            Type t = typeof(ProductionOrderListModel);

            var sortingPropertyInfo = t.GetProperties().FirstOrDefault(prop => prop.Name?.ToLower() == orderByFieldName.ToLower());

            var orderByExp = $"g.{orderByFieldName}";

            if (sortingPropertyInfo?.PropertyType == typeof(bool))
            {
                orderByExp = $"CONVERT(int,g.{orderByFieldName})";
            }

            orderByExp = asc ? $"MIN({orderByExp})" : $"MAX({orderByExp})";


            var sql = new StringBuilder(
                @$";WITH tmp AS (
                    SELECT ");

            sql.Append($"ROW_NUMBER() OVER (ORDER BY {orderByExp} {(asc ? "" : "DESC")}) AS RowNum,");


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
	                GROUP BY g.ProductionOrderId ");

            var table = await _manufacturingDBContext.QueryDataTableRaw(totalSql.ToString(), parammeters.ToArray());
            var total = 0;
            // decimal additionResult = 0;
            if (table != null && table.Rows.Count > 0)
            {
                total = (table.Rows[0]["Total"] as int?).GetValueOrDefault();
                // additionResult = (table.Rows[0]["AdditionResult"] as decimal?).GetValueOrDefault();
            }

            if (size > 0)
            {
                sql.Append(@$"ORDER BY {orderByExp} {(asc ? "" : "DESC")}
                            OFFSET {(page - 1) * size} ROWS
                            FETCH NEXT {size}
                            ROWS ONLY");
            }

            sql.Append(@")
                SELECT v.* FROM tmp t
                LEFT JOIN vProductionOrderDetail v ON t.ProductionOrderId = v.ProductionOrderId ");

            sql.Append(" ORDER BY t.RowNum");

            var resultData = await _manufacturingDBContext.QueryDataTableRaw(sql.ToString(), parammeters.Select(p => p.CloneSqlParam()).ToArray());
            var lst = resultData.ConvertData<ProductionOrderListEntity>().AsQueryable().ProjectTo<ProductionOrderListModel>(_mapper.ConfigurationProvider).ToList();

            return (lst, total);
        }

        public async Task<PageData<ProductOrderModelExtra>> GetProductionOrderList(string keyword, int page, int size, string orderByFieldName, bool asc, long fromDate, long toDate, Clause filters = null)
        {
            keyword = (keyword ?? "").Trim();
            var parammeters = new List<SqlParameter>();

            var whereCondition = new StringBuilder();
            if (!string.IsNullOrEmpty(keyword))
            {
                whereCondition.Append("( v.ProductionOrderCode LIKE @KeyWord ");
                whereCondition.Append("OR v.ProductCode LIKE @Keyword ");
                whereCondition.Append("OR v.ProductName LIKE @Keyword ");
                whereCondition.Append("OR v.CustomerPO LIKE @Keyword ");
                whereCondition.Append("OR v.Description LIKE @Keyword ");
                whereCondition.Append("OR v.OrderCode LIKE @Keyword ) ");
                parammeters.Add(new SqlParameter("@Keyword", $"%{keyword}%"));
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
                suffix = filters.FilterClauseProcess("vProductionOrderDetail", "v", filterCondition, parammeters, suffix);
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

            Type t = typeof(ProductionOrderListModel);

            var sortingPropertyInfo = t.GetProperties().FirstOrDefault(prop => prop.Name?.ToLower() == orderByFieldName.ToLower());

            var orderByExp = $"g.{orderByFieldName}";

            if (sortingPropertyInfo?.PropertyType == typeof(bool))
            {
                orderByExp = $"CONVERT(int,g.{orderByFieldName})";
            }

            orderByExp = asc ? $"MIN({orderByExp})" : $"MAX({orderByExp})";

            var sql = new StringBuilder(
                @$";WITH tmp AS (
                    SELECT ");


            sql.Append($"ROW_NUMBER() OVER (ORDER BY {orderByExp} {(asc ? "" : "DESC")}) AS RowNum,");


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
	                GROUP BY g.ProductionOrderId ");

            var table = await _manufacturingDBContext.QueryDataTableRaw(totalSql.ToString(), parammeters.ToArray());
            var total = 0;
            // decimal additionResult = 0;
            if (table != null && table.Rows.Count > 0)
            {
                total = (table.Rows[0]["Total"] as int?).GetValueOrDefault();
                // additionResult = (table.Rows[0]["AdditionResult"] as decimal?).GetValueOrDefault();
            }

            if (size > 0)
            {
                sql.Append(@$"ORDER BY {orderByExp} {(asc ? "" : "DESC")}
                            OFFSET {(page - 1) * size} ROWS
                            FETCH NEXT {size}
                            ROWS ONLY");
            }

            sql.Append(@")
                SELECT * FROM tmp t
                OUTER APPLY(SELECT TOP 1 * 
                FROM vProductionOrderDetail v  
                WHERE t.ProductionOrderId = v.ProductionOrderId
                ORDER BY CAST(HasNewProductionProcessVersion AS int) DESC) u");


            var resultData = await _manufacturingDBContext.QueryDataTableRaw(sql.ToString(), parammeters.Select(p => p.CloneSqlParam()).ToArray());
            var lst = resultData.ConvertData<ProductOrderModelExtra>();

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

        public async Task<IList<ProductionOrderStepWorkloadModel>> ListWorkLoadsByMultipleProductionOrders(IList<long> productionOrderIds)
        {
            var productionOrderInfos = await _manufacturingDBContext.ProductionOrder.Include(po => po.ProductionOrderDetail)
                  .Where(po => productionOrderIds.Contains(po.ProductionOrderId))
                  .ToListAsync();


            var workLoads = await GetProductionWorkLoads(productionOrderInfos, null);

            var result = new List<ProductionOrderStepWorkloadModel>();
            foreach (var (productionOrderId, stepWorkloads) in workLoads)
            {
                var pStepWorkload = new ProductionOrderStepWorkloadModel()
                {
                    ProductionOrderId = productionOrderId,
                    StepWorkLoads = stepWorkloads.Select(step => new ProductionStepOutputObjectWorkloadModel()
                    {
                        StepId = step.Key,
                        Outputs = step.Value
                    })
                    .ToList()
                };
                result.Add(pStepWorkload);
            }
            return result;
        }


        public async Task<ProductionCapacityModel> GetProductionCapacity(int? monthPlanId, long fromDate, long toDate, int? assignDepartmentId)
        {

            IList<ProductionOrderEntity> productionOrders;

            if (monthPlanId > 0)
            {
                var monthPlanInfo = await _manufacturingDBContext.MonthPlan.FirstOrDefaultAsync(m => m.MonthPlanId == monthPlanId);
                if (monthPlanInfo == null) throw GeneralCode.ItemNotFound.BadRequest();

                fromDate = monthPlanInfo.StartDate.GetUnix();
                toDate = monthPlanInfo.EndDate.GetUnix();

                productionOrders = await _manufacturingDBContext.ProductionOrder.Include(po => po.ProductionOrderDetail)
                    .Where(po => po.MonthPlanId == monthPlanId)
                    .ToListAsync();
            }
            else
            {
                var fromDateTime = fromDate.UnixToDateTime();
                var toDateTime = toDate.UnixToDateTime();

                productionOrders = await _manufacturingDBContext.ProductionOrder.Include(po => po.ProductionOrderDetail)
                        .Where(po => po.StartDate <= toDateTime && po.EndDate >= fromDateTime)
                        .ToListAsync();
            }

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
            var stepHourTotal = new Dictionary<int, decimal>();

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

            var stepHoursDetail = new Dictionary<int, IList<StepDepartmentHour>>();

            var departmentHourTotal = new Dictionary<int, decimal>();

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

                if (!departmentHourTotal.ContainsKey(departmentId))
                {
                    departmentHourTotal.Add(departmentId, totalHour);
                }


                var totalWorkHour = productionCapacityDetail.SelectMany(pc => pc.Value).Where(pc => departmentStepIds.Contains(pc.Key)).Sum(pc => pc.Value.Sum(w => w.WorkHour));
                // Duyệt danh sách công đoạn tổ đảm nhiệm => tính ra số giờ làm việc của tổ cho từng công đoạn theo tỷ lệ KLCV
                foreach (var departmentStepId in departmentStepIds)
                {
                    if (!stepHourTotal.ContainsKey(departmentStepId)) stepHourTotal[departmentStepId] = 0;
                    var stepWorkHour = productionCapacityDetail.Sum(pc => pc.Value.ContainsKey(departmentStepId) ? pc.Value[departmentStepId].Sum(w => w.WorkHour) : 0);
                    var h = totalWorkHour > 0 ? totalHour * stepWorkHour / totalWorkHour : 0;
                    stepHourTotal[departmentStepId] += h;

                    if (!stepHoursDetail.ContainsKey(departmentStepId))
                    {
                        stepHoursDetail.Add(departmentStepId, new List<StepDepartmentHour>());
                    }
                    var detail = stepHoursDetail[departmentStepId].FirstOrDefault(d => d.DepartmentId == departmentId);
                    if (detail == null)
                    {
                        detail = new StepDepartmentHour() { DepartmentId = departmentId, AssignedHours = 0, HourTotal = 0 };
                        stepHoursDetail[departmentStepId].Add(detail);
                    }
                    detail.AssignedHours += stepWorkHour;
                    detail.HourTotal += h;
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
                StepHourTotal = stepHourTotal,
                AssignedStepHours = assignedHours,
                StepHoursDetail = stepHoursDetail,
                DepartmentHourTotal = departmentHourTotal,
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

        public async Task<ProductionCapacityModel> GetProductionCapacityByAssignmentDate(long fromDate, long toDate, int? factoryDepartmentId)
        {

            IList<ProductionOrderEntity> productionOrders;


            var fromDateTime = fromDate.UnixToDateTime();
            var toDateTime = toDate.UnixToDateTime();

            var assigns = await (
                from a in _manufacturingDBContext.ProductionAssignment
                join ps in _manufacturingDBContext.ProductionStep on a.ProductionStepId equals ps.ProductionStepId
                join parent in _manufacturingDBContext.ProductionStep on ps.ParentId equals parent.ProductionStepId
                where a.EndDate >= fromDateTime && a.StartDate <= toDateTime
                select new
                {
                    a.ProductionOrderId,
                    a.DepartmentId,
                    a.ProductionStepId,
                    ParentProductionStepId = parent.ProductionStepId,
                    parent.StepId
                }
                )
                .ToListAsync();

            var productionOrderIds = assigns.Select(a => a.ProductionOrderId)
                .Distinct()
                .ToList();

            productionOrders = await _manufacturingDBContext.ProductionOrder.Include(po => po.ProductionOrderDetail)
                    .Where(po => (!factoryDepartmentId.HasValue || po.FactoryDepartmentId == factoryDepartmentId) && productionOrderIds.Contains(po.ProductionOrderId))
                    .ToListAsync();

            // Lấy thông tin đầu ra và số giờ công cần
            var productionCapacityDetail = await GetProductionWorkLoads(productionOrders, null);


            // var stepIds = assigns.Select(c => c.StepId).Distinct().ToList();
            var lstStepIds = new HashSet<int>();

            var departmentIds = assigns.Select(a => a.DepartmentId).Distinct().ToList();

            // Lấy thông tin phong ban
            var departmentCalendar = (await _organizationHelperService.GetListDepartmentCalendar(fromDate, toDate, departmentIds.ToArray()));
            var departments = (await _organizationHelperService.GetDepartmentSimples(departmentIds.ToArray()));
            var stepHourTotal = new Dictionary<int, decimal>();

            var assignedHours = new Dictionary<int, decimal>();
            foreach (var (productionOrderId, stepCapacity) in productionCapacityDetail)
            {
                foreach (var (stepId, capacities) in stepCapacity)
                {
                    if (!assignedHours.ContainsKey(stepId))
                    {
                        assignedHours.Add(stepId, 0);
                    }

                    assignedHours[stepId] += capacities.Sum(s => s.Details.Sum(d => d.AssignInfos.Sum(a => a.ByDates.Where(date => date.WorkDate >= fromDate && date.WorkDate <= toDate).Sum(date => date.WorkHourPerDay ?? 0))));
                    if (assignedHours[stepId] > 0 && !lstStepIds.Contains(stepId))
                    {
                        lstStepIds.Add(stepId);
                    }
                }
            }

            var departmentAssigns = new Dictionary<int, IList<DepartmentStepAssignHours>>();
            var stepHoursDetail = new Dictionary<int, IList<StepDepartmentHour>>();

            var deparmentHourTotal = new Dictionary<int, decimal>();

            foreach (var (productionOrderId, productionCapacities) in productionCapacityDetail)
            {

                foreach (var (stepId, pStepCapac) in productionCapacities)
                {
                    foreach (var c in pStepCapac)
                    {
                        foreach (var assign in c.Details)
                        {
                            foreach (var departmentAssign in assign.AssignInfos)
                            {

                                var dates = departmentAssign.ByDates.Where(d => d.WorkDate >= fromDate && d.WorkDate <= toDate).ToList();
                                if (dates.Count > 0)
                                {
                                    if (!departmentAssigns.ContainsKey(departmentAssign.DepartmentId))
                                    {
                                        departmentAssigns.Add(departmentAssign.DepartmentId, new List<DepartmentStepAssignHours>());
                                    }
                                    var departmentAssignHour = departmentAssigns[departmentAssign.DepartmentId];
                                    var depStepHour = departmentAssignHour.FirstOrDefault(s => s.StepId == stepId);
                                    var total = dates.Sum(d => d.WorkHourPerDay ?? 0);
                                    if (depStepHour == null)
                                    {
                                        depStepHour = new DepartmentStepAssignHours()
                                        {
                                            StepId = stepId,
                                            Hours = total
                                        };
                                        departmentAssignHour.Add(depStepHour);
                                    }
                                    else
                                    {
                                        depStepHour.Hours += total;
                                    }
                                }


                            }
                        }
                    }
                }
            }


            foreach (var departmentId in departmentIds)
            {

                var calendar = departmentCalendar.FirstOrDefault(c => c.DepartmentId == departmentId);
                var department = departments.FirstOrDefault(d => d.DepartmentId == departmentId);
                decimal totalHour = 0;

                var offDays = calendar.DepartmentDayOffCalendar.Select(o => o.Day.UnixToDateTime(_currentContextService.TimeZoneOffset).Date).ToList();

                var departmentStepsWorkHours = new Dictionary<int, decimal>();

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

                if (!deparmentHourTotal.ContainsKey(departmentId))
                {
                    deparmentHourTotal.Add(departmentId, totalHour);
                }

                if (departmentAssigns.ContainsKey(departmentId))
                {
                    var departmentStepIds = departmentAssigns[departmentId].Select(d => d.StepId).Distinct().ToList();

                    var totalAssignHours = departmentAssigns[departmentId].Sum(s => s.Hours);

                    // Duyệt danh sách công đoạn tổ đảm nhiệm => tính ra số giờ làm việc của tổ cho từng công đoạn theo tỷ lệ KLCV
                    foreach (var departmentStepId in departmentStepIds)
                    {
                        if (!stepHourTotal.ContainsKey(departmentStepId)) stepHourTotal[departmentStepId] = 0;
                        var stepWorkHour = departmentAssigns[departmentId].Where(d => d.StepId == departmentStepId).Sum(s => s.Hours);

                        var h = totalAssignHours > 0 ? totalHour * stepWorkHour / totalAssignHours : 0;

                        stepHourTotal[departmentStepId] += h;

                        if (!stepHoursDetail.ContainsKey(departmentStepId))
                        {
                            stepHoursDetail.Add(departmentStepId, new List<StepDepartmentHour>());
                        }
                        var detail = stepHoursDetail[departmentStepId].FirstOrDefault(d => d.DepartmentId == departmentId);
                        if (detail == null)
                        {
                            detail = new StepDepartmentHour() { DepartmentId = departmentId, AssignedHours = 0, HourTotal = 0 };
                            stepHoursDetail[departmentStepId].Add(detail);
                        }
                        detail.AssignedHours += stepWorkHour;
                        detail.HourTotal += h;
                    }
                }
            }


            var stepInfo = _manufacturingDBContext.Step
             .Where(s => lstStepIds.Contains(s.StepId))
             .Select(s => new StepInfo
             {
                 StepId = s.StepId,
                 StepName = s.StepName
             })
             .ToList();

            var result = new ProductionCapacityModel
            {
                StepInfo = stepInfo,
                StepHourTotal = stepHourTotal,
                AssignedStepHours = assignedHours,
                StepHoursDetail = stepHoursDetail,
                DepartmentHourTotal = deparmentHourTotal,
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

        public async Task<CapacityStepByProduction> GetProductionWorkLoads(IList<ProductionOrderEntity> productionOrders, long? assignDepartmentId1)
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
                                           Quantity = ld.Quantity,// + (ld.ExportOutsourceQuantity ?? 0),
                                           OutsourceQuantity = (ld.OutsourcePartQuantity ?? 0) + (ld.OutsourceQuantity ?? 0),
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

            Dictionary<string, decimal?> ProductivitiesCaches = new Dictionary<string, decimal?>();
            Func<EnumProductionStepLinkDataObjectType, long, int, decimal?> getProductivity = (EnumProductionStepLinkDataObjectType objectTypeId, long objectId, int stepId) =>
            {
                var key = $"{objectTypeId}|{objectId}|{stepId}";
                if (ProductivitiesCaches.ContainsKey(key)) return ProductivitiesCaches[key];

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

                ProductivitiesCaches.Add(key, productivityByStep);
                return productivityByStep;
            };

            var assignmentsByStep = productionAssignments1.GroupBy(a => a.ProductionStepId)
                .ToDictionary(a => a.Key, a => a.ToList());

            var workloadInfosByLinkedData = workloadInfos.GroupBy(w => w.ProductionStepLinkDataId)
                .ToDictionary(w => w.Key, w => w.FirstOrDefault());

            //TODO Optimize
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

                            assignmentsByStep.TryGetValue(d.ProductionStepId, out var assignmentStep);
                            if (assignmentStep == null)
                            {
                                assignmentStep = new List<Infrastructure.EF.ManufacturingDB.ProductionAssignment>();
                            }

                            var departmentIds = assignmentStep.Select(a => a.DepartmentId).Distinct().ToList();


                            var assignInfos = new List<CapacityAssignInfo>();
                            foreach (var depId in departmentIds)
                            {
                                decimal assignQuantity = 0;
                                bool isSelectionAssign = false;

                                var assignStep = assignmentStep.FirstOrDefault(w => w.DepartmentId == depId);

                                var byDates = new List<ProductionAssignmentDetailModel>();

                                //var assignWorkInfo = workloadInfos.FirstOrDefault(w => w.ProductionStepLinkDataId == assignStep.ProductionStepLinkDataId);
                                workloadInfosByLinkedData.TryGetValue(assignStep.ProductionStepLinkDataId, out var assignWorkInfo);
                                var byDateAssign = _mapper.Map<List<ProductionAssignmentDetailModel>>(assignStep.ProductionAssignmentDetail);
                                if (assignWorkInfo != null)
                                {

                                    var rateQuantiy = assignWorkInfo.Quantity > 0 ? d.Quantity / assignWorkInfo.Quantity : 0;

                                    assignQuantity = assignStep.AssignmentQuantity * rateQuantiy;

                                    byDates = byDateAssign.Select(a =>
                                    {
                                        var byDate = new ProductionAssignmentDetailModel()
                                        {
                                            WorkDate = a.WorkDate,
                                            QuantityPerDay = a.QuantityPerDay * rateQuantiy
                                        };

                                        //var workloads = workloadInfos.Where(s => s.ProductionStepId == d.ProductionStepId).ToList();
                                        //var workloadInfo = workloads.FirstOrDefault(w => w.ProductionStepLinkDataId == d.ProductionStepLinkDataId);


                                        //decimal? totalWorkload = 0;
                                        //decimal? totalHours = 0;
                                        //foreach (var w in workloads)
                                        //{
                                        //    var assignQuantity = workloadInfo.Quantity > 0 ? a.QuantityPerDay * w.Quantity / workloadInfo.Quantity : 0;
                                        //    var workload = assignQuantity * w.WorkloadConvertRate;
                                        //    var hour = productivityByStep > 0 ? workload / productivityByStep : 0;
                                        //    totalWorkload += workload;
                                        //    totalHours += hour;

                                        //}
                                        //=> Only one

                                        //var workloadInfo = workloadInfos.FirstOrDefault(w => w.ProductionStepLinkDataId == d.ProductionStepLinkDataId);
                                        workloadInfosByLinkedData.TryGetValue(d.ProductionStepLinkDataId, out var workloadInfo);

                                        decimal? totalWorkload = 0;
                                        decimal? totalHours = 0;

                                        var assignQuantity = a.QuantityPerDay * rateQuantiy;
                                        var workload = assignQuantity * workloadInfo.WorkloadConvertRate;
                                        var hour = productivityByStep > 0 ? workload / productivityByStep : 0;
                                        totalWorkload += workload;
                                        totalHours += hour;


                                        //byDate.SetWorkHourPerDay(totalHours);
                                        //byDate.SetWorkloadPerDay(totalWorkload);

                                        byDate.WorkloadPerDay = byDate.WorkloadPerDay ?? totalWorkload;
                                        byDate.WorkHourPerDay = byDate.WorkHourPerDay ?? totalHours;

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
                                    StartDate = assignStep?.StartDate?.GetUnix(),
                                    EndDate = assignStep?.EndDate?.GetUnix(),
                                    IsManualSetStartDate = assignStep.IsManualSetStartDate,
                                    IsManualSetEndDate = assignStep.IsManualSetEndDate,
                                    RateInPercent = assignStep.RateInPercent,
                                    IsSelectionAssign = isSelectionAssign,
                                    ByDates = byDates
                                });
                            }


                            //var currentDepartmentAssign = assignInfos.FirstOrDefault(a => a.DepartmentId == assignDepartmentId);

                            var workloadQuantity = d.Quantity * d.WorkloadConvertRate.Value;
                            return new ProductionStepWorkloadAssignModel
                            {
                                StepId = d.StepId,
                                ProductionStepId = d.ProductionStepId,
                                ProductionStepTitle = d.ProductionStepTitle,
                                ProductionStepLinkDataId = d.ProductionStepLinkDataId,
                                Quantity = d.Quantity,
                                //IsSelectionAssign = currentDepartmentAssign?.IsSelectionAssign ?? false,
                                WorkloadConvertRate = d.WorkloadConvertRate.Value,
                                WorkloadQuantity = workloadQuantity,

                                Productivity = productivityByStep ?? 0,

                                WorkHour = productivityByStep > 0 ? workloadQuantity / productivityByStep.Value : 0,
                                OutsourceQuantity = d.OutsourceQuantity,
                                //AssignQuantity = currentDepartmentAssign?.AssignQuantity,
                                //AssignWorkloadQuantity = currentDepartmentAssign?.AssignWorkloadQuantity,
                                //StartDate = currentDepartmentAssign?.StartDate,
                                //EndDate = currentDepartmentAssign?.EndDate,
                                //IsManualSetDate = currentDepartmentAssign?.IsManualSetDate ?? false,
                                //RateInPercent = currentDepartmentAssign?.RateInPercent ?? 100,
                                //ByDates = currentDepartmentAssign?.ByDates,
                                //AssignWorkHour = currentDepartmentAssign?.AssignWorkHour,
                                AssignInfos = assignInfos
                            };
                        }).ToList()
                    };
                })
                .GroupBy(w => w.ProductionOrderId)

                .ToCustomDictionary(new CapacityStepByProduction(),
                    w => w.Key,
                    w => w.GroupBy(c => c.StepId).ToCustomDictionary(new CapacityByStep(), c => c.Key, c => c.ToIList())
                );

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
                var resultData = await _manufacturingDBContext.QueryDataTableRaw(sql, parammeters);

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
                IGenerateCodeContext generateCodeCtx = null;
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

                if (data.ProductionOrderDetail.Any(x => x.Quantity <= 0))
                    throw new BadRequestException(GeneralCode.InvalidParams, "Số lượng vào lệnh không được để trống");

                if (string.IsNullOrEmpty(data.ProductionOrderCode))
                {
                    var (code, ctx) = await GenerateProductOrderCode(null, data);
                    data.ProductionOrderCode = code;
                    generateCodeCtx = ctx;
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
                if (generateCodeCtx != null)
                {
                    await generateCodeCtx.ConfirmCode();
                }

                await _objActivityLogFacade.LogBuilder(() => ProductionOrderActivityLogMessage.Create)
                   .MessageResourceFormatDatas(productionOrder.ProductionOrderCode)
                   .ObjectId(productionOrder.ProductionOrderId)
                   .JsonData(data)
                   .CreateLog();

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
            productionOrder.ProductionOrderAssignmentStatusId = (int)EnumProductionOrderAssignmentStatus.NoAssignment;

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
                var ctxs = new List<IGenerateCodeContext>();

                var baseValueChains = new Dictionary<string, int>();
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
                    if (item.ProductionOrderDetail.Any(x => x.Quantity <= 0))
                        throw new BadRequestException(GeneralCode.InvalidParams, "Số lượng vào lệnh không được để trống");

                    //string currentCode = currentConfig.CurrentLastValue.LastCode;
                    var (code, ctx) = await GenerateProductOrderCode(null, item, baseValueChains);
                    item.ProductionOrderCode = code;
                    ctxs.Add(ctx);

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
                foreach (var item in ctxs)
                {
                    await item.ConfirmCode();
                }

                await _objActivityLogFacade.LogBuilder(() => ProductionOrderActivityLogMessage.CreateMulti)
                   .MessageResourceFormatDatas(data.Length)
                   .ObjectId(productionOrderId)
                   .JsonData(data)
                   .CreateLog();

                return data.Length;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "CreateProductOrders");
                throw;
            }
        }
        private async Task<(string, IGenerateCodeContext)> GenerateProductOrderCode(long? productOrderId, ProductionOrderInputModel model, Dictionary<string, int> baseValueChains = null)
        {
            model.ProductionOrderCode = (model.ProductionOrderCode ?? "").Trim();

            var ctx = _customGenCodeHelperService.CreateGenerateCodeContext(baseValueChains);

            var code = await ctx
                .SetConfig(EnumObjectType.ProductionOrder)
                .SetConfigData(productOrderId ?? 0, model.Date, model.ProductionOrderDetail.FirstOrDefault().OrderCode)
                .TryValidateAndGenerateCode(_manufacturingDBContext.ProductionOrder, model.ProductionOrderCode, (s, code) => s.ProductionOrderId != productOrderId && s.ProductionOrderCode == code); ;
            return (code, ctx);
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
                if (data.ProductionOrderDetail.Any(x => x.Quantity <= 0))
                    throw new BadRequestException(GeneralCode.InvalidParams, "Số lượng vào lệnh không được để trống");
                var productionOrder = _manufacturingDBContext.ProductionOrder
                    .Where(o => o.ProductionOrderId == productionOrderId)
                    .FirstOrDefault();

                bool invalidPlan = productionOrder.StartDate.GetUnix() != data.StartDate || productionOrder.EndDate.GetUnix() != data.EndDate;

                if (productionOrder == null) throw new BadRequestException(ProductOrderErrorCode.ProductOrderNotfound);

                if (data.UpdatedDatetimeUtc != productionOrder.UpdatedDatetimeUtc.GetUnix())
                {
                    throw GeneralCode.DataIsOld.BadRequest();
                }

                data.ProductionOrderAssignmentStatusId = (EnumProductionOrderAssignmentStatus?)productionOrder.ProductionOrderAssignmentStatusId;

                var oldIsManualFinish = productionOrder.IsManualFinish;

                var isSetManualFinish = false;
                if ((int)data.ProductionOrderStatus != productionOrder.ProductionOrderStatus && data.ProductionOrderStatus == EnumProductionStatus.Finished)
                {
                    isSetManualFinish = true;
                }

                _mapper.Map(data, productionOrder);

                if (isSetManualFinish)
                {
                    productionOrder.IsManualFinish = true;
                }
                else
                {
                    if (data.ProductionOrderStatus != EnumProductionStatus.Finished)
                    {
                        productionOrder.IsManualFinish = false;
                    }
                    else
                    {
                        productionOrder.IsManualFinish = oldIsManualFinish;
                    }

                }



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

                if (isSetManualFinish)//set all assign to finish
                {
                    var productionAssignments = _manufacturingDBContext.ProductionAssignment
                     .Where(a => a.ProductionOrderId == productionOrder.ProductionOrderId)
                     .ToList();
                    foreach (var productionAssignment in productionAssignments)
                    {
                        productionAssignment.AssignedProgressStatus = (int)EnumAssignedProgressStatus.Finish;
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

                if (_manufacturingDBContext.HasChanges())
                    productionOrder.UpdatedDatetimeUtc = DateTime.UtcNow;

                await _manufacturingDBContext.SaveChangesAsync();

                trans.Commit();

                await _objActivityLogFacade.LogBuilder(() => ProductionOrderActivityLogMessage.Update)
                   .MessageResourceFormatDatas(productionOrder.ProductionOrderCode)
                   .ObjectId(productionOrder.ProductionOrderId)
                   .JsonData(data)
                   .CreateLog();
       
                await _productionOrderQueueHelperService.ProductionOrderStatiticChanges(productionOrder?.ProductionOrderCode, $"Cập nhật thông tin lệnh");

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


                var histories = await _manufacturingDBContext.ProductionHistory.Where(a => a.ProductionOrderId == productionOrderId).ToListAsync();
                foreach (var h in histories)
                {
                    h.IsDeleted = true;
                }



                var handovers = await _manufacturingDBContext.ProductionHandover.Where(a => a.ProductionOrderId == productionOrderId).ToListAsync();
                foreach (var h in handovers)
                {
                    h.IsDeleted = true;
                }


                await _manufacturingDBContext.SaveChangesAsync();

                var receivedIds = histories.Select(h => h.ProductionHandoverReceiptId).Distinct()
                    .Union(handovers.Select(h => h.ProductionHandoverReceiptId).Distinct())
                    .Distinct()
                    .ToList();

                var receiveInfos = await _manufacturingDBContext.ProductionHandoverReceipt
                    .Include(r => r.ProductionHandover)
                    .Include(r => r.ProductionHistory)
                    .Where(r => receivedIds.Contains(r.ProductionHandoverReceiptId))
                    .ToListAsync();
                foreach (var r in receiveInfos)
                {
                    if (!r.ProductionHandover.Any() && !r.ProductionHistory.Any())
                    {
                        r.IsDeleted = true;
                    }
                }

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

                await _objActivityLogFacade.LogBuilder(() => ProductionOrderActivityLogMessage.Delete)
                   .MessageResourceFormatDatas(productionOrder.ProductionOrderCode)
                   .ObjectId(productionOrder.ProductionOrderId)
                   .JsonData(productionOrder)
                   .CreateLog();
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
                var resultData = await _manufacturingDBContext.QueryDataTableRaw(sql, parammeters);

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

                        await _objActivityLogFacade.LogBuilder(() => ProductionOrderActivityLogMessage.Update)
                                .MessageResourceFormatDatas(productionOrder.ProductionOrderCode)
                                .ObjectId(productionOrder.ProductionOrderId)
                                .JsonData(new { productionOrder, data, isManual = false })
                                .CreateLog();
                    }
                }
                else
                {

                    if (productionOrder.ProductionOrderStatus < (int)data.ProductionOrderStatus)
                    {
                        productionOrder.ProductionOrderStatus = (int)data.ProductionOrderStatus;

                        await _objActivityLogFacade.LogBuilder(() => ProductionOrderActivityLogMessage.Update)
                                .MessageResourceFormatDatas(productionOrder.ProductionOrderCode)
                                .ObjectId(productionOrder.ProductionOrderId)
                                .JsonData(new { productionOrder, data, isManual = false })
                                .CreateLog();
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

                    await _objActivityLogFacade.LogBuilder(() => ProductionOrderActivityLogMessage.Update)
                                .MessageResourceFormatDatas(productionOrder.ProductionOrderCode)
                                .ObjectId(productionOrder.ProductionOrderId)
                                .JsonData(new { productionOrder, status, isManual = true })
                                .CreateLog();
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

        public async Task<bool> EditDate(UpdateDatetimeModel data)
        {
            long[] productionOrderDetailIds = data.ProductionOrderDetailIds;
            long startDate = data.StartDate;
            long planEndDate = data.PlanEndDate;
            long endDate = data.EndDate;



            if (startDate <= 0) throw new BadRequestException(GeneralCode.InvalidParams, "Yêu cầu nhập ngày bắt đầu sản xuất.");
            if (endDate <= 0) throw new BadRequestException(GeneralCode.InvalidParams, "Yêu cầu nhập ngày kết thúc sản xuất.");
            if (planEndDate <= 0) throw new BadRequestException(GeneralCode.InvalidParams, "Yêu cầu nhập ngày kết thúc hàng trắng.");
            if (startDate > planEndDate) throw new BadRequestException(GeneralCode.InvalidParams, "Ngày bắt đầu không được lớn hơn ngày kết thúc hàng trắng. Vui lòng chọn lại kế hoạch sản xuất!");
            if (planEndDate > endDate) throw new BadRequestException(GeneralCode.InvalidParams, "Ngày kết thúc hàng trắng không được lớn hơn ngày kết thúc. Vui lòng chọn lại kế hoạch sản xuất!");

            var productionOrderDetails = await _manufacturingDBContext.ProductionOrderDetail
                .Where(pod => productionOrderDetailIds.Contains(pod.ProductionOrderDetailId))
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
                    productionOrder.MonthPlanId = data.MonthPlanId;
                    productionOrder.FromWeekPlanId = data.FromWeekPlanId;
                    productionOrder.ToWeekPlanId = data.ToWeekPlanId;
                }
                _manufacturingDBContext.SaveChanges();

                foreach (var productionOrder in productionOrders)
                {

                    await _objActivityLogFacade.LogBuilder(() => ProductionOrderActivityLogMessage.UpdateDate)
                                .MessageResourceFormatDatas(productionOrder.ProductionOrderCode)
                                .ObjectId(productionOrder.ProductionOrderId)
                                .JsonData(productionOrder)
                                .CreateLog();
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateProductOrderDateTime");
                throw;
            }
        }
        public async Task<bool> UpdateMultipleProductionOrders(List<ProductionOrderPropertyUpdate> updateDatas, List<long> productionOrderIds)
        {
            if (productionOrderIds.Count > 0)
            {
                var sql = $"SELECT * FROM ProductionOrder p JOIN @PIds v ON p.ProductionOrderId = v.[Value]";
                var resultData = await _manufacturingDBContext.QueryDataTableRaw(sql, new[] { productionOrderIds.ToSqlParameter("@PIds") });
                if (resultData.Rows.Count > 0)
                {
                    if (updateDatas.Any(x => x.FieldName == "ProductionOrderCode"))
                    {
                        throw new BadRequestException($@"Không thể sửa đồng loạt giá trị cột Mã LSX");
                    }
                    var sqlParams = new List<SqlParameter>();
                    sqlParams.Add(productionOrderIds.ToSqlParameter("@productionOrderIds"));
                    foreach (ProductionOrderPropertyUpdate column in updateDatas)
                    {
                        sqlParams.Add(new SqlParameter("@" + column.FieldName, (resultData.Rows[0][column.FieldName].GetType().GetDataType()).GetSqlValue(column.NewValue)));
                    }
                    var sqlupdate = $"UPDATE [ProductionOrder] SET {string.Join(",", updateDatas.Select(c => $"[{c.FieldName}] = @{c.FieldName}"))} WHERE ProductionOrderId IN (SELECT [Value] FROM @productionOrderIds)";
                    await _manufacturingDBContext.Database.ExecuteSqlRawAsync($"{sqlupdate}", sqlParams);
                }
                else
                    throw new BadRequestException(GeneralCode.ItemNotFound);
            }
            else
                throw new BadRequestException(GeneralCode.ItemNotFound);

            return true;
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
                entity.IsWeekPlanSplitByWeekOfYear = model.IsWeekPlanSplitByWeekOfYear;
            };

            await _manufacturingDBContext.SaveChangesAsync();
            return true;
        }
        #endregion
        private class DepartmentStepAssignHours
        {
            public int StepId { get; set; }
            public decimal Hours { get; set; }
        }
    }





}
