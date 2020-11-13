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

        public async Task<ProductionScheduleModel> CreateProductionSchedule(ProductionScheduleModel data)
        {
            throw new NotImplementedException();
        }

        public async Task<PageData<ProductionScheduleModel>> GetProductionSchedule(string keyword, int page, int size, string orderByFieldName, bool asc, Clause filters = null)
        {
            keyword = (keyword ?? "").Trim();
            var parammeters = new List<SqlParameter>();

            var whereCondition = new StringBuilder();
            if (!string.IsNullOrEmpty(keyword))
            {
                whereCondition.Append("(v.ProductionOrderCode LIKE @KeyWord ");
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
                    if (whereCondition.Length > 0) whereCondition.Append(" AND ");
                    whereCondition.Append(filterCondition);
                }
            }

            var sql = new StringBuilder("SELECT * FROM vProductionSchedule v ");
            var totalSql = new StringBuilder("SELECT COUNT(v.ProductionScheduleId) Total FROM vProductionSchedule v ");
            if (whereCondition.Length > 0)
            {
                totalSql.Append("WHERE ");
                totalSql.Append(whereCondition);
                sql.Append("WHERE ");
                sql.Append(whereCondition);
            }
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

        public async Task<ProductionScheduleModel> UpdateProductionSchedule(int productionOrderDetailId, ProductionScheduleModel data)
        {
            throw new NotImplementedException();
        }
    }
}
