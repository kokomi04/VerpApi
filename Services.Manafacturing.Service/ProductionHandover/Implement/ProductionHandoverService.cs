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
using VErp.Services.Manafacturing.Model.ProductionAssignment;
using VErp.Commons.Enums.Manafacturing;
using Microsoft.Data.SqlClient;
using ProductionHandoverEntity = VErp.Infrastructure.EF.ManufacturingDB.ProductionHandover;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;
using VErp.Services.Manafacturing.Model.ProductionHandover;

namespace VErp.Services.Manafacturing.Service.ProductionHandover.Implement
{
    public class ProductionHandoverService : IProductionHandoverService
    {
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

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
                if (!_manufacturingDBContext.ProductionAssignment.Any(a => a.ProductionStepId == data.FromProductionStepId && a.DepartmentId == data.FromDepartmentId && a.ProductionOrderId == productionOrderId))
                    throw new BadRequestException(GeneralCode.InvalidParams, "Không tồn tại phân công công việc cho tổ bàn giao");
                if (!_manufacturingDBContext.ProductionAssignment.Any(a => a.ProductionStepId == data.ToProductionStepId && a.DepartmentId == data.ToDepartmentId && a.ProductionOrderId == productionOrderId))
                    throw new BadRequestException(GeneralCode.InvalidParams, "Không tồn tại phân công công việc cho tổ được bàn giao");
                var productionHandover = _mapper.Map<ProductionHandoverEntity>(data);
                productionHandover.Status = (int)status;
                productionHandover.ProductionOrderId = productionOrderId;
                _manufacturingDBContext.ProductionHandover.Add(productionHandover);
                _manufacturingDBContext.SaveChanges();
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

        public async Task<PageData<DepartmentHandoverModel>> GetDepartmentHandovers(long departmentId, string keyword, int page, int size, Clause filters = null)
        {
            keyword = (keyword ?? "").Trim();
            var parammeters = new List<SqlParameter>();

            var whereCondition = new StringBuilder("v.DepartmentId = @DepartmentId");
            parammeters.Add(new SqlParameter("@DepartmentId", departmentId));
            if (!string.IsNullOrEmpty(keyword))
            {
                whereCondition.Append("(v.OrderCode LIKE @KeyWord ");
                whereCondition.Append("OR v.ProductionOrderCode LIKE @Keyword ");
                whereCondition.Append("OR v.ProductTitle LIKE @Keyword ");
                whereCondition.Append("OR v.StepName LIKE @Keyword ");
                whereCondition.Append("OR v.Material LIKE @Keyword ");
                whereCondition.Append("OR v.InOutType LIKE @Keyword ");
                whereCondition.Append("OR v.ReciprocalStep LIKE @Keyword ) ");

                parammeters.Add(new SqlParameter("@Keyword", $"%{keyword}%"));
            }

            if (filters != null)
            {
                var suffix = 0;
                var filterCondition = new StringBuilder();
                filters.FilterClauseProcess("vProductionDepartmentHandover", "v", ref filterCondition, ref parammeters, ref suffix);
                if (filterCondition.Length > 2)
                {
                    if (whereCondition.Length > 0) whereCondition.Append(" AND ");
                    whereCondition.Append(filterCondition);
                }
            }

            var sql = new StringBuilder(
                @";WITH tmp AS (
                    SELECT g.ProductionOrderId, g.ProductionStepId
                    FROM(
                        SELECT * FROM vProductionDepartmentHandover v");

            var totalSql = new StringBuilder(
                @"SELECT 
                    COUNT(*) Total 
                FROM (
                    SELECT v.ProductionOrderId, v.ProductionStepId FROM vProductionDepartmentHandover v ");
            if (whereCondition.Length > 0)
            {
                totalSql.Append(" WHERE ");
                totalSql.Append(whereCondition);
               
                sql.Append(" WHERE ");
                sql.Append(whereCondition);
            }

            totalSql.Append(" GROUP BY v.ProductionOrderId, v.ProductionStepId ) g");
            sql.Append(
                    @") g
	                GROUP BY g.ProductionOrderId, g.ProductionStepId
                    ORDER BY g.ProductionOrderId, g.ProductionStepId");

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
            sql.Append(@")
                SELECT v.* FROM tmp t
                INNER JOIN vProductionDepartmentHandover v ON t.ProductionOrderId = v.ProductionOrderId AND t.ProductionStepId = v.ProductionStepId");

            var resultData = await _manufacturingDBContext.QueryDataTable(sql.ToString(), parammeters.Select(p => p.CloneSqlParam()).ToArray());
            var lst = resultData.ConvertData<DepartmentHandoverEntity>().AsQueryable().ProjectTo<DepartmentHandoverModel>(_mapper.ConfigurationProvider).ToList();

            return (lst, total);
        }
    }
}
