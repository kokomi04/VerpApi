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
using VErp.Services.Manafacturing.Model.ProductionOrder;
using VErp.Commons.Enums.Manafacturing;
using Microsoft.Data.SqlClient;

namespace VErp.Services.Manafacturing.Service.ProductionOrder.Implement
{
    public class ProductionScheduleService : IProductionScheduleService
    {
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly ICustomGenCodeHelperService _customGenCodeHelperService;

        public ProductionScheduleService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<ProductionOrderService> logger
            , IMapper mapper
            , ICustomGenCodeHelperService customGenCodeHelperService)
        {
            _manufacturingDBContext = manufacturingDB;
            _activityLogService = activityLogService;
            _logger = logger;
            _mapper = mapper;
            _customGenCodeHelperService = customGenCodeHelperService;
        }

        public async Task<IList<ProductionPlaningOrderDetailModel>> GetProductionPlaningOrderDetail(int productionOrderId)
        {
            var dataSql = @$"
                SELECT v.ProductionOrderDetailId
                    , v.TotalQuantity
                    , v.ProductTitle
                    , v.UnitPrice
                    , v.TotalPrice
                    , v.UnitName
                    , v.PlannedQuantity
                    , v.OrderCode
                    , v.PartnerTitle
                    , v.ProductionStepId
                FROM vProductionOrderDetail v
                WHERE v.ProductionOrderId = @ProductionOrderId
                ";
            var sqlParams = new SqlParameter[]
            {
                new SqlParameter("@ProductionOrderId", productionOrderId)
            };

            var resultData = await _manufacturingDBContext.QueryDataTable(dataSql, sqlParams);
            return resultData.ConvertData<ProductionPlaningOrderDetailModel>();
        }

        public async Task<IList<ProductionPlaningOrderModel>> GetProductionPlaningOrders()
        {
            var dataSql = @$"
                 ;WITH tmp AS (
                    SELECT ProductionOrderId, MAX(ProductionOrderDetailId) ProductionOrderDetailId
                    FROM vProductionPlainingOrder
                    GROUP BY ProductionOrderId    
                )
                SELECT 
                    t.ProductionOrderId
                    , v.ProductionOrderCode
                    , v.VoucherDate
                    , v.FinishDate
                FROM tmp t LEFT JOIN vProductionPlainingOrder v ON t.ProductionOrderDetailId = v.ProductionOrderDetailId
                ORDER BY v.VoucherDate DESC
                ";
            var resultData = await _manufacturingDBContext.QueryDataTable(dataSql, Array.Empty<SqlParameter>());
            return resultData.ConvertData<ProductionPlaningOrderEntity>()
                .AsQueryable()
                .ProjectTo<ProductionPlaningOrderModel>(_mapper.ConfigurationProvider)
                .ToList();
        }

        public async Task<PageData<ProductionScheduleModel>> GetProductionSchedule(string keyword, long fromDate, long toDate, int page, int size, string orderByFieldName, bool asc, Clause filters = null)
        {
            keyword = (keyword ?? "").Trim();
            var parammeters = new List<SqlParameter>();

            var whereCondition = new StringBuilder(" (v.StartDate <= @FromDate AND v.EndDate >= @ToDate) ");

            var fromDateTime = fromDate.UnixToDateTime();
            var toDateTime = toDate.UnixToDateTime();
            if(!fromDateTime.HasValue || !toDateTime.HasValue)
                throw new BadRequestException(GeneralCode.InvalidParams, "Vui lòng chọn ngày bắt đầu, ngày kết thúc");

            parammeters.Add(new SqlParameter("@FromDate", fromDateTime.Value));
            parammeters.Add(new SqlParameter("@ToDate", toDateTime.Value));

            if (!string.IsNullOrEmpty(keyword))
            {
                whereCondition.Append("AND (v.ProductionOrderCode LIKE @KeyWord ");
                whereCondition.Append("|| v.ProductTitle LIKE @Keyword ");
                whereCondition.Append("|| v.OrderCode LIKE @Keyword ");
                whereCondition.Append("|| v.PartnerTitle LIKE @Keyword ");
                whereCondition.Append("|| v.UnitName LIKE @Keyword ) ");
                parammeters.Add(new SqlParameter("@Keyword", $"%{keyword}%"));
            }

            if (filters != null)
            {
                var suffix = 0;
                var filterCondition = new StringBuilder();
                filters.FilterClauseProcess("vProductionSchedule", "v", ref filterCondition, ref parammeters, ref suffix);
                if (filterCondition.Length > 2)
                {
                    whereCondition.Append(" AND ");
                    whereCondition.Append(filterCondition);
                }
            }

            var sql = new StringBuilder("SELECT * FROM vProductionSchedule v ");
            var totalSql = new StringBuilder("SELECT COUNT(v.ProductionScheduleId) Total FROM vProductionSchedule v ");

            totalSql.Append("WHERE ");
            totalSql.Append(whereCondition);
            sql.Append("WHERE ");
            sql.Append(whereCondition);

            orderByFieldName = string.IsNullOrEmpty(orderByFieldName) ? "ProductionOrderDetailId" : orderByFieldName;
            sql.Append($" ORDER BY v.[{orderByFieldName}] {(asc ? "" : "DESC")}");

            var table = await _manufacturingDBContext.QueryDataTable(totalSql.ToString(), parammeters.ToArray());

            var total = 0;
            if (table != null && table.Rows.Count > 0)
            {
                total = (table.Rows[0]["Total"] as int?).GetValueOrDefault();
            }

            if (size >= 0)
            {
                sql.Append(@$" OFFSET {(page - 1) * size} ROWS
                FETCH NEXT { size}
                ROWS ONLY");
            }

            var resultData = await _manufacturingDBContext.QueryDataTable(sql.ToString(), parammeters.Select(p => p.CloneSqlParam()).ToArray());
            var lst = resultData.ConvertData<ProductionScheduleEntity>().AsQueryable().ProjectTo<ProductionScheduleModel>(_mapper.ConfigurationProvider).ToList();

            return (lst, total);
        }
        public async Task<List<ProductionScheduleInputModel>> CreateProductionSchedule(List<ProductionScheduleInputModel> data)
        {
            // Get plaining order detail
            var plainingOrderSql = @$"
                SELECT v.ProductionOrderDetailId
                    , v.TotalQuantity
                    , v.ProductTitle
                    , v.UnitPrice
                    , v.TotalPrice
                    , v.UnitName
                    , v.PlannedQuantity
                    , v.OrderCode
                    , v.PartnerTitle
                    , v.ProductionStepId
                    , v.ScheduleTurnId
                FROM vProductionOrderDetail v
                WHERE v.ProductionOrderId IN ({string.Join(",", data.Select(od => od.ProductionOrderDetailId).Distinct().ToList())})
                ";

            var plainingOrders = (await _manufacturingDBContext.QueryDataTable(plainingOrderSql, Array.Empty<SqlParameter>()))
                .ConvertData<ProductionPlaningOrderDetailModel>();

            // Validate
            if (plainingOrders.Count == 0 || plainingOrders.Count != data.Count)
                throw new BadRequestException(GeneralCode.InvalidParams, "Sản phẩm không có trong danh sách cần lên kế hoạch sản xuất");

            if (data.Count > 1)
            {
                // Validate nếu sản phẩm chung quy trình
                if (plainingOrders.Select(po => po.ProductionStepId).Distinct().Count() > 1)
                    throw new BadRequestException(GeneralCode.InvalidParams, "Danh sách sản phẩm không cùng trong một quy trình sản xuất");

                // Validate thời gian
                if (data.Select(s => s.StartDate).Distinct().Count() > 1 || data.Select(s => s.EndDate).Distinct().Count() > 1)
                    throw new BadRequestException(GeneralCode.InvalidParams, "Danh sách sản phẩm phải trùng ngày bắt đầu, ngày kết thúc");

                // Validate số lượng
                var quantityMap = plainingOrders.Join(data, po => po.ProductionOrderDetailId, s => s.ProductionOrderDetailId, (po, s) => new
                {
                    po.TotalQuantity,
                    s.ProductionScheduleQuantity
                }).ToList();
                for (int indx = 1; indx < quantityMap.Count; indx++)
                {
                    if (quantityMap[0].ProductionScheduleQuantity * quantityMap[indx].TotalQuantity
                        != quantityMap[indx].ProductionScheduleQuantity * quantityMap[0].TotalQuantity)
                        throw new BadRequestException(GeneralCode.InvalidParams, "Số lượng các sản phẩm có tỷ lệ không hợp lệ");
                }
            }

            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockProductionOrderKey(0));
            try
            {
                var currentTurnId = _manufacturingDBContext.ProductionSchedule.Max(s => s.ScheduleTurnId);
                var dataMap = new List<(ProductionScheduleInputModel Input, ProductionSchedule Entity)>();
                foreach (var item in data)
                {
                    item.ScheduleTurnId = currentTurnId + 1;
                    var productionSchedule = _mapper.Map<ProductionSchedule>(item);
                    _manufacturingDBContext.ProductionSchedule.Add(productionSchedule);
                    dataMap.Add((item, productionSchedule));
                    await _activityLogService.CreateLog(EnumObjectType.ProductionSchedule, productionSchedule.ProductionOrderDetailId, $"Thêm mới lịch sản xuất cho LSX", data.JsonSerialize());
                }

                _manufacturingDBContext.SaveChanges();
                foreach (var item in dataMap)
                {
                    item.Input.ProductionScheduleId = item.Entity.ProductionScheduleId;
                }
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateProductSchedule");
                throw;
            }
        }

        public async Task<List<ProductionScheduleInputModel>> UpdateProductionSchedule(List<ProductionScheduleInputModel> data)
        {
            var scheduleIds = data.Select(s => s.ProductionScheduleId).ToList();
            var productionOrderIds = data.Select(s => s.ProductionOrderDetailId).ToList();
            var productionSchedules = _manufacturingDBContext.ProductionSchedule.Where(s => scheduleIds.Contains(s.ProductionScheduleId)).ToList();
            if (productionSchedules.Count != data.Count)
                throw new BadRequestException(GeneralCode.ItemNotFound, "Lịch sản xuất không tồn tại");
            if (productionSchedules.Any(s => s.ProductionScheduleStatus == (int)EnumProductionStatus.Finished))
                throw new BadRequestException(GeneralCode.InvalidParams, "Không được thay đổi lịch sản xuất đã hoàn thành");
            if (productionSchedules.Select(s => s.ScheduleTurnId).Distinct().Count() > 1)
                throw new BadRequestException(GeneralCode.InvalidParams, "Danh sách sản phẩm không cùng trong một quy trình sản xuất");

            if (data.Count > 1)
            {
                // Validate thời gian
                if (data.Select(s => s.StartDate).Distinct().Count() > 1 || data.Select(s => s.EndDate).Distinct().Count() > 1)
                    throw new BadRequestException(GeneralCode.InvalidParams, "Danh sách sản phẩm phải trùng ngày bắt đầu, ngày kết thúc");

                var productionOrderQuantity = _manufacturingDBContext.ProductionOrderDetail
                .Where(od => productionOrderIds.Contains(od.ProductionOrderDetailId))
                .Select(od => new
                {
                    od.ProductionOrderDetailId,
                    TotalQuantity = od.Quantity + od.ReserveQuantity
                })
                .ToDictionary(od => od.ProductionOrderDetailId, od => od.TotalQuantity);

                for (int indx = 1; indx < data.Count; indx++)
                {
                    if (data[0].ProductionScheduleQuantity * productionOrderQuantity[data[indx].ProductionOrderDetailId]
                        != data[indx].ProductionScheduleQuantity * productionOrderQuantity[data[0].ProductionOrderDetailId])
                        throw new BadRequestException(GeneralCode.InvalidParams, "Số lượng các sản phẩm có tỷ lệ không hợp lệ");
                }
            }

            try
            {
                foreach (var item in data)
                {
                    var entity = productionSchedules.FirstOrDefault(s => s.ProductionScheduleId == item.ProductionScheduleId);
                    entity.ProductionScheduleQuantity = item.ProductionScheduleQuantity;
                    entity.StartDate = item.StartDate.UnixToDateTime().Value;
                    entity.EndDate = item.EndDate.UnixToDateTime().Value;
                    await _activityLogService.CreateLog(EnumObjectType.ProductionSchedule, entity.ProductionScheduleId, $"Cập nhật lịch sản xuất", data.JsonSerialize());
                }
                _manufacturingDBContext.SaveChanges();
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateProductSchedule");
                throw;
            }
        }

        public async Task<bool> DeleteProductionSchedule(int[] productionScheduleIds)
        {
            var productionSchedules = _manufacturingDBContext.ProductionSchedule.Where(s => productionScheduleIds.Contains(s.ProductionScheduleId)).ToList();
            if (productionSchedules.Count != productionScheduleIds.Length)
                throw new BadRequestException(GeneralCode.ItemNotFound, "Lịch sản xuất không tồn tại");
            if (productionSchedules.Any(s => s.ProductionScheduleStatus != (int)EnumProductionStatus.Waiting))
                throw new BadRequestException(GeneralCode.InvalidParams, "Chỉ được xóa lịch sản xuất đã chưa thực hiện");
            if (productionSchedules.Select(s => s.ScheduleTurnId).Distinct().Count() > 1)
                throw new BadRequestException(GeneralCode.InvalidParams, "Danh sách sản phẩm không cùng trong một quy trình sản xuất");

            try
            {
                foreach (var item in productionSchedules)
                {
                    item.IsDeleted = true;
                    await _activityLogService.CreateLog(EnumObjectType.ProductionSchedule, item.ProductionScheduleId, $"Xóa lịch sản xuất ", item.JsonSerialize());
                }

                _manufacturingDBContext.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteProductSchedule");
                throw;
            }
        }
    }
}
