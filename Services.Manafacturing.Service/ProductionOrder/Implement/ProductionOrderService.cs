using AutoMapper;
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
using VErp.Services.Manafacturing.Model.ProductionOrder;
using ProductionOrderEntity = VErp.Infrastructure.EF.ManufacturingDB.ProductionOrder;
using VErp.Commons.Enums.Manafacturing;
using Microsoft.Data.SqlClient;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Services.Manafacturing.Model.ProductionHandover;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;

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
        public ProductionOrderService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<ProductionOrderService> logger
            , IMapper mapper
            , ICustomGenCodeHelperService customGenCodeHelperService
            , IProductHelperService productHelperService
            , IOrganizationHelperService organizationHelperService)
        {
            _manufacturingDBContext = manufacturingDB;
            _activityLogService = activityLogService;
            _logger = logger;
            _mapper = mapper;
            _customGenCodeHelperService = customGenCodeHelperService;
            _productHelperService = productHelperService;
            _organizationHelperService = organizationHelperService;
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

            var result = await GetProductionOrders(string.Empty, 1, 0, string.Empty, true, 0, 0, filter);
            return result.List;
        }

        public async Task<PageData<ProductionOrderListModel>> GetProductionOrders(string keyword, int page, int size, string orderByFieldName, bool asc, long fromDate, long toDate, Clause filters = null)
        {
            keyword = (keyword ?? "").Trim();
            var parammeters = new List<SqlParameter>();

            var whereCondition = new StringBuilder();
            if (!string.IsNullOrEmpty(keyword))
            {
                whereCondition.Append("(v.ProductionOrderCode LIKE @KeyWord ");
                whereCondition.Append("OR v.ProductTitle LIKE @Keyword ");
                whereCondition.Append("OR v.PartnerTitle LIKE @Keyword ");
                whereCondition.Append("OR v.ContainerNumber LIKE @Keyword ");
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
                    COUNT(*) Total, SUM(g.AdditionResult) AdditionResult 
                FROM (
                    SELECT v.ProductionOrderId, SUM(v.UnitPrice * (v.Quantity + v.ReserveQuantity)) AdditionResult FROM vProductionOrderDetail v ");

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
	                GROUP BY g.ProductionOrderCode, g.ProductionOrderId, g.Date, g.StartDate, g.EndDate, g.ProductionOrderStatus ");

            var table = await _manufacturingDBContext.QueryDataTable(totalSql.ToString(), parammeters.ToArray());
            var total = 0;
            decimal additionResult = 0;
            if (table != null && table.Rows.Count > 0)
            {
                total = (table.Rows[0]["Total"] as int?).GetValueOrDefault();
                additionResult = (table.Rows[0]["AdditionResult"] as decimal?).GetValueOrDefault();
            }

            if (size > 0)
            {
                sql.Append(@$"ORDER BY g.{orderByFieldName} {(asc ? "" : "DESC")}
                            OFFSET {(page - 1) * size} ROWS
                            FETCH NEXT { size}
                            ROWS ONLY");
            }

            sql.Append(@")
                SELECT v.* FROM tmp t
                LEFT JOIN vProductionOrderDetail v ON t.ProductionOrderId = v.ProductionOrderId ");

            sql.Append(" ORDER BY t.RowNum");

            var resultData = await _manufacturingDBContext.QueryDataTable(sql.ToString(), parammeters.Select(p => p.CloneSqlParam()).ToArray());
            var lst = resultData.ConvertData<ProductionOrderListEntity>().AsQueryable().ProjectTo<ProductionOrderListModel>(_mapper.ConfigurationProvider).ToList();

            return (lst, total, additionResult);
        }

        public async Task<ProductionCapacityModel> GetProductionCapacity(long fromDate, long toDate)
        {
            var fromDateTime = fromDate.UnixToDateTime();
            var toDateTime = toDate.UnixToDateTime();

            var productionCapacities = await (from pod in _manufacturingDBContext.ProductionOrderDetail
                                              join po in _manufacturingDBContext.ProductionOrder on pod.ProductionOrderId equals po.ProductionOrderId
                                              where po.StartDate <= toDateTime && po.EndDate >= fromDateTime
                                              select new
                                              {
                                                  ProductionOrderId = po.ProductionOrderId,
                                                  ProductionOrderCode = po.ProductionOrderCode,
                                                  ProductionOrderDetailId = pod.ProductionOrderDetailId,
                                                  ProductId = pod.ProductId,
                                                  Quantity = pod.Quantity,
                                                  ReserveQuantity = pod.ReserveQuantity,
                                                  StartDate = po.StartDate.GetUnix(),
                                                  EndDate = po.EndDate.GetUnix()
                                              })
                                            .ToListAsync();


            var productionOrderIds = productionCapacities.Select(p => p.ProductionOrderId).Distinct().ToList();


            // Lấy thông tin khối lượng công việc
            var workloadInfo = await (from ps in _manufacturingDBContext.ProductionStep
                                      join g in _manufacturingDBContext.ProductionStep
                                      on new { ps.ProductionStepId, IsDeleted = false, IsGroup = false, IsFinish = false } equals new { ProductionStepId = g.ParentId.Value, IsDeleted = g.IsDeleted, IsGroup = g.IsGroup.Value, IsFinish = g.IsFinish }
                                      join ldr in _manufacturingDBContext.ProductionStepLinkDataRole
                                      on new { g.ProductionStepId, Type = (int)EnumProductionStepLinkDataRoleType.Output } equals new { ldr.ProductionStepId, Type = ldr.ProductionStepLinkDataRoleTypeId }
                                      join ld in _manufacturingDBContext.ProductionStepLinkData on ldr.ProductionStepLinkDataId equals ld.ProductionStepLinkDataId
                                      where !ps.IsDeleted && !ps.IsFinish && ps.IsGroup.Value && productionOrderIds.Contains(ps.ContainerId) && ps.ContainerTypeId == (int)EnumContainerType.ProductionOrder && ps.StepId.HasValue
                                      select new ProductionWordloadInfo
                                      {
                                          ProductionStepId = g.ProductionStepId,
                                          ProductionOrderId = ps.ContainerId,
                                          StepId = ps.StepId.Value,
                                          Quantity = ld.QuantityOrigin - ld.OutsourcePartQuantity.GetValueOrDefault(),
                                          ObjectId = ld.ObjectId,
                                          ObjectTypeId = ld.ObjectTypeId,
                                          WorkloadConvertRate = ld.WorkloadConvertRate
                                      }).ToListAsync();

            var productionStepIds = workloadInfo.Select(w => w.ProductionStepId).Distinct().ToList();

            // Lấy thông tin phân công
            var productionAssignments = _manufacturingDBContext.ProductionAssignment
                .Include(pa => pa.ProductionAssignmentDetail)
                .Where(pa => productionStepIds.Contains(pa.ProductionStepId))
                .ToList();

            // Tính khối lượng công việc theo tỷ lệ thời gian
            foreach (var group in workloadInfo.GroupBy(w => new { w.ProductionStepId, w.ProductionOrderId }))
            {
                var productionCapacity = productionCapacities.First(pc => pc.ProductionOrderId == group.Key.ProductionOrderId);
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
                    item.Quantity = assignmentTime > 0? item.Quantity * calcTime / assignmentTime : 0;
                }
            }

            // Lấy thông tin đầu ra và số giờ công cần
            var stepIds = workloadInfo.Select(w => w.StepId).Distinct().ToList();
            var stepInfo = _manufacturingDBContext.Step
                .Where(s => stepIds.Contains(s.StepId))
                .Select(s => new StepInfo
                {
                    StepId = s.StepId,
                    StepName = s.StepName,
                    Productivity = s.Productivity.GetValueOrDefault()
                })
                .ToList();
            var productivities = stepInfo.ToDictionary(s => s.StepId, s => s.Productivity);

            var productionCapacityDetail = workloadInfo
                .GroupBy(w => new
                {
                    w.ProductionOrderId,
                    w.StepId,
                    w.ObjectId,
                    w.ObjectTypeId
                })
                .Select(g => new
                {
                    g.Key.ProductionOrderId,
                    g.Key.StepId,
                    g.Key.ObjectId,
                    g.Key.ObjectTypeId,
                    Quantity = g.Sum(w => w.Quantity),
                    WorkloadQuantity = g.Sum(w => w.Quantity * w.WorkloadConvertRate)
                })
                .GroupBy(w => w.ProductionOrderId)
                .ToDictionary(g => g.Key, g => g.GroupBy(w => w.StepId)
                .ToDictionary(g => g.Key, g => g.Select(w => new ProductionCapacityDetailModel
                {
                    ObjectId = w.ObjectId,
                    ObjectTypeId = w.ObjectTypeId,
                    Quantity = w.Quantity,
                    WorkloadQuantity = w.WorkloadQuantity,
                    WorkHour = productivities[g.Key] > 0 ? w.WorkloadQuantity / productivities[g.Key] : 0
                }).ToList()));

            // Tính giờ công có
            var stepDetails = _manufacturingDBContext.StepDetail
                .Where(sd => stepIds.Contains(sd.StepId))
                .Select(sd => new
                {
                    sd.StepId,
                    sd.DepartmentId,
                    sd.NumberOfPerson
                })
                .ToList();

            var departmentIds = stepDetails.Select(a => a.DepartmentId).Distinct().ToList();

            // Lấy thông tin phong ban
            var departmentCalendar = (await _organizationHelperService.GetListDepartmentCalendar(fromDate, toDate, departmentIds.ToArray()));

            var departmentHour = new Dictionary<int, decimal>();

            foreach (var departmentId in departmentIds)
            {
                var departmentStepIds = stepDetails.Where(sd => sd.DepartmentId == departmentId).Select(sd => sd.StepId).Distinct().ToList();
                var calendar = departmentCalendar.FirstOrDefault(d => d.DepartmentId == departmentId);
                decimal totalHour = 0;
                for (var workDateUnix = fromDate; workDateUnix < toDate; workDateUnix += 24 * 60 * 60)
                {
                    // Tính số giờ làm việc theo ngày của tổ
                    var workingHourInfo = calendar.DepartmentWorkingHourInfo.Where(wh => wh.StartDate <= workDateUnix).OrderByDescending(wh => wh.StartDate).FirstOrDefault();
                    var overHour = calendar.DepartmentOverHourInfo.FirstOrDefault(oh => oh.StartDate <= workDateUnix && oh.EndDate >= workDateUnix);

                    totalHour += (decimal)((workingHourInfo?.WorkingHourPerDay ?? 0) + (overHour?.OverHour ?? 0));
                }

                var totalWorkHour = productionCapacityDetail.SelectMany(pc => pc.Value).Where(pc => departmentStepIds.Contains(pc.Key)).Sum(pc => pc.Value.Sum(w => w.WorkHour));
                foreach (var departmentStepId in departmentStepIds)
                {
                    if (!departmentHour.ContainsKey(departmentStepId)) departmentHour[departmentStepId] = 0;
                    var stepWorkHour = productionCapacityDetail.Sum(pc => pc.Value.ContainsKey(departmentStepId)? pc.Value[departmentStepId].Sum(w => w.WorkHour) : 0);
                    departmentHour[departmentStepId] += totalWorkHour > 0 ? totalHour * stepWorkHour / totalWorkHour : 0;
                }
            }


            var result = new ProductionCapacityModel
            {
                StepInfo = stepInfo,
                DepartmentHour = departmentHour
            };


            foreach (var productionCapacity in productionCapacities.GroupBy(pc => new { pc.ProductionOrderId, pc.ProductionOrderCode }))
            {
                var productionOrderDetail = productionCapacity
                    .Select(pc => new ProductionOrderDetailCapacityModel
                    {
                        ProductId = pc.ProductId,
                        ProductionOrderDetailId = pc.ProductionOrderDetailId,
                        Quantity = pc.Quantity,
                        ReserveQuantity = pc.ReserveQuantity
                    })
                    .ToList();
                result.ProductionOrder.Add(new ProductionOrderCapacityModel
                {
                    ProductionOrderId = productionCapacity.Key.ProductionOrderId,
                    ProductionOrderCode = productionCapacity.Key.ProductionOrderCode,
                    ProductionCapacityDetail = productionCapacityDetail.ContainsKey(productionCapacity.Key.ProductionOrderId) 
                    ? productionCapacityDetail[productionCapacity.Key.ProductionOrderId]
                    : new Dictionary<int, List<ProductionCapacityDetailModel>>(),
                    ProductionOrderDetail = productionOrderDetail
                });
            }


            return result;
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
                .Where(o => o.ProductionOrderId == productionOrderId)
                .ProjectTo<ProductionOrderOutputModel>(_mapper.ConfigurationProvider)
                .FirstOrDefault();
            if (productOrder != null)
            {
                var sql = $"SELECT * FROM vProductionOrderDetail WHERE ProductionOrderId = @ProductionOrderId";
                var parammeters = new SqlParameter[]
                {
                    new SqlParameter("@ProductionOrderId", productionOrderId)
                };
                var resultData = await _manufacturingDBContext.QueryDataTable(sql, parammeters);

                productOrder.ProductionOrderDetail = resultData.ConvertData<ProductionOrderDetailOutputModel>();

            }

            return productOrder;
        }

        public async Task<ProductionOrderInputModel> CreateProductionOrder(ProductionOrderInputModel data)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockProductionOrderKey(0));
            using var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                if (data.StartDate <= 0) throw new BadRequestException(GeneralCode.InvalidParams, "Yêu cầu nhập ngày bắt đầu sản xuất.");
                if (data.EndDate <= 0) throw new BadRequestException(GeneralCode.InvalidParams, "Yêu cầu nhập ngày kết thúc sản xuất.");
                if (data.Date <= 0) throw new BadRequestException(GeneralCode.InvalidParams, "Yêu cầu nhập ngày chứng từ.");

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

        private async Task<ProductionOrderEntity> SaveProductionOrder(ProductionOrderInputModel data)
        {
            var productionOrder = _mapper.Map<ProductionOrderEntity>(data);
            productionOrder.IsResetProductionProcess = false;
            productionOrder.IsInvalid = true;
            productionOrder.ProductionOrderStatus = (int)EnumProductionStatus.NotReady;
            _manufacturingDBContext.ProductionOrder.Add(productionOrder);
            await _manufacturingDBContext.SaveChangesAsync();

            // Tạo detail
            foreach (var item in data.ProductionOrderDetail)
            {
                item.ProductionOrderDetailId = 0;
                item.ProductionOrderId = productionOrder.ProductionOrderId;
                // Tạo mới
                var entity = _mapper.Map<ProductionOrderDetail>(item);

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

            return productionOrder;
        }

        public async Task<int> CreateMultipleProductionOrder(ProductionOrderInputModel[] data)
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
                    if (item.Date <= 0) throw new BadRequestException(GeneralCode.InvalidParams, "Yêu cầu nhập ngày chứng từ.");

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
                    var productionOrder = await SaveProductionOrder(item);
                    productionOrderId = productionOrder.ProductionOrderId;
                }

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
                if (data.Date <= 0) throw new BadRequestException(GeneralCode.InvalidParams, "Yêu cầu nhập ngày chứng từ.");
                if (data.ProductionOrderDetail.GroupBy(x => new { x.ProductId, x.OrderCode })
                    .Where(x => x.Count() > 1)
                    .Count() > 0)
                    throw new BadRequestException(GeneralCode.InvalidParams, "Xuất hiện mặt hàng trùng lặp trong lệch sản xuất");

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
                        && ld.ObjectTypeId == (int)EnumProductionStepLinkDataObjectType.Product
                        && productIds.Contains(ld.ObjectId)
                        && r.ProductionStepLinkDataRoleTypeId == (int)EnumProductionStepLinkDataRoleType.Output
                        select ld.ObjectId
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

                foreach (var item in data.ProductionOrderDetail)
                {
                    item.ProductionOrderId = productionOrderId;
                    var oldItem = oldDetail.Where(od => od.ProductionOrderDetailId == item.ProductionOrderDetailId).FirstOrDefault();
                    if (oldItem != null)
                    {
                        // Cập nhật
                        invalidPlan = invalidPlan || oldItem.ProductId != item.ProductId || oldItem.Quantity != item.Quantity || oldItem.ReserveQuantity != item.ReserveQuantity;
                        _mapper.Map(item, oldItem);
                        // Gỡ khỏi danh sách cũ
                        oldDetail.Remove(oldItem);
                    }
                    else
                    {
                        invalidPlan = true;
                        item.ProductionOrderDetailId = 0;
                        // Tạo mới
                        var entity = _mapper.Map<ProductionOrderDetail>(item);
                        _manufacturingDBContext.ProductionOrderDetail.Add(entity);
                    }
                }

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

        public async Task<bool> UpdateProductionOrderStatus(long productionOrderId, ProductionOrderStatusDataModel data)
        {
            var productionOrder = _manufacturingDBContext.ProductionOrder
                .Include(po => po.ProductionOrderDetail)
                .FirstOrDefault(po => po.ProductionOrderId == productionOrderId);

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
    }

    class ProductionWordloadInfo
    {
        public long ProductionStepId { get; set; }
        public long ProductionOrderId { get; set; }
        public int StepId { get; set; }
        public decimal Quantity { get; set; }
        public long ObjectId { get; set; }
        public int ObjectTypeId { get; set; }
        public decimal WorkloadConvertRate { get; set; }
    }
}
