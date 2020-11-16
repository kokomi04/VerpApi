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

        public ProductionOrderService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<ProductionOrderService> logger
            , IMapper mapper
            , ICustomGenCodeHelperService customGenCodeHelperService
            , IProductHelperService productHelperService)
        {
            _manufacturingDBContext = manufacturingDB;
            _activityLogService = activityLogService;
            _logger = logger;
            _mapper = mapper;
            _customGenCodeHelperService = customGenCodeHelperService;
            _productHelperService = productHelperService;
        }
        public async Task<PageData<ProductionOrderListModel>> GetProductionOrders(string keyword, int page, int size, string orderByFieldName, bool asc, Clause filters = null)
        {
            keyword = (keyword ?? "").Trim();
            var parammeters = new List<SqlParameter>();

            var whereCondition = new StringBuilder();
            if (!string.IsNullOrEmpty(keyword))
            {
                whereCondition.Append("(v.ProductionOrderCode LIKE @KeyWord ");
                whereCondition.Append("OR v.Description LIKE @Keyword ");
                whereCondition.Append("OR v.ProductTitle LIKE @Keyword ");
                whereCondition.Append("OR v.PartnerTitle LIKE @Keyword ");
                whereCondition.Append("OR v.OrderCode LIKE @Keyword ) ");
                parammeters.Add(new SqlParameter("@Keyword", $"%{keyword}%"));
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

            var sql = new StringBuilder("SELECT * FROM vProductionOrderDetail v ");
            var totalSql = new StringBuilder("SELECT COUNT(v.ProductionOrderDetailId) Total FROM vProductionOrderDetail v ");
            if (whereCondition.Length > 0)
            {
                totalSql.Append("WHERE ");
                totalSql.Append(whereCondition);
                sql.Append("WHERE ");
                sql.Append(whereCondition);
            }
            orderByFieldName = string.IsNullOrEmpty(orderByFieldName) ? "ProductionOrderId" : orderByFieldName;
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
            var lst = resultData.ConvertData<ProductionOrderListEntity>().AsQueryable().ProjectTo<ProductionOrderListModel>(_mapper.ConfigurationProvider).ToList();

            return (lst, total);
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

        public async Task<ProductionOrderOutputModel> GetProductionOrder(int productionOrderId)
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


                var detailIds = productOrder.ProductionOrderDetail.Select(od => od.ProductionOrderDetailId).ToList();
                var countDetailId = _manufacturingDBContext.ProductionStepOrder
                    .Where(so => detailIds.Contains(so.ProductionOrderDetailId))
                    .Select(so => so.ProductionOrderDetailId)
                    .Distinct()
                    .Count();

                if(countDetailId == 0)
                {
                    productOrder.ProcessStatus = EnumProcessStatus.Waiting;
                }
                else if (countDetailId < detailIds.Count)
                {
                    productOrder.ProcessStatus = EnumProcessStatus.Incomplete;
                }
                else
                {
                    productOrder.ProcessStatus = EnumProcessStatus.Complete;
                }
            }

            return productOrder;
        }

        public async Task<ProductionOrderInputModel> CreateProductionOrder(ProductionOrderInputModel data)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockProductionOrderKey(0));
            using var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                if (string.IsNullOrEmpty(data.ProductionOrderCode))
                {
                    CustomGenCodeOutputModelOut currentConfig = await _customGenCodeHelperService.CurrentConfig(EnumObjectType.ProductionOrder, 0);
                    if (currentConfig == null)
                    {
                        throw new BadRequestException(GeneralCode.ItemNotFound, "Chưa thiết định cấu hình sinh mã");
                    }
                    var generated = await _customGenCodeHelperService.GenerateCode(currentConfig.CustomGenCodeId, currentConfig.LastValue);
                    if (generated == null)
                    {
                        throw new BadRequestException(GeneralCode.InternalError, "Không thể sinh mã ");
                    }
                    data.ProductionOrderCode = generated.CustomCode;
                }
                else
                {
                    // Validate unique
                    if (_manufacturingDBContext.ProductionOrder.Any(o => o.ProductionOrderCode == data.ProductionOrderCode))
                        throw new BadRequestException(ProductOrderErrorCode.ProductOrderCodeAlreadyExisted);
                }

                var productionOrder = _mapper.Map<ProductionOrderEntity>(data);

                _manufacturingDBContext.ProductionOrder.Add(productionOrder);
                await _manufacturingDBContext.SaveChangesAsync();

                // Tạo detail
                foreach (var item in data.ProductionOrderDetail)
                {
                    item.ProductionOrderDetailId = 0;
                    item.ProductionOrderId = productionOrder.ProductionOrderId;
                    // Tạo mới
                    var entity = _mapper.Map<ProductionOrderDetail>(item);
                    entity.Status = (int)EnumProductionStatus.Waiting;

                    _manufacturingDBContext.ProductionOrderDetail.Add(entity);
                }
                await _manufacturingDBContext.SaveChangesAsync();
                trans.Commit();
                data.ProductionOrderId = productionOrder.ProductionOrderId;
                if (string.IsNullOrEmpty(data.ProductionOrderCode))
                {
                    await _customGenCodeHelperService.ConfirmCode(EnumObjectType.InputType, 0);
                }
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

        public async Task<ProductionOrderInputModel> UpdateProductionOrder(int productionOrderId, ProductionOrderInputModel data)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockProductionOrderKey(productionOrderId));
            using var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                var productionOrder = _manufacturingDBContext.ProductionOrder
                    .Where(o => o.ProductionOrderId == productionOrderId)
                    .FirstOrDefault();
                if (productionOrder == null) throw new BadRequestException(ProductOrderErrorCode.ProductOrderNotfound);
                _mapper.Map(data, productionOrder);

                var oldDetail = _manufacturingDBContext.ProductionOrderDetail.Where(od => od.ProductionOrderId == productionOrderId).ToList();

                foreach (var item in data.ProductionOrderDetail)
                {
                    item.ProductionOrderId = productionOrderId;
                    var oldItem = oldDetail.Where(od => od.ProductionOrderDetailId == item.ProductionOrderDetailId).FirstOrDefault();
                    if (oldItem != null)
                    {
                        // Cập nhật
                        _mapper.Map(item, oldItem);
                        // Gỡ khỏi danh sách cũ
                        oldDetail.Remove(oldItem);
                    }
                    else
                    {
                        item.ProductionOrderDetailId = 0;
                        // Tạo mới
                        var entity = _mapper.Map<ProductionOrderDetail>(item);
                        _manufacturingDBContext.ProductionOrderDetail.Add(entity);
                    }
                }
                // Xóa
                // Validate lịch sản xuất
                var delIds = oldDetail.Select(od => od.ProductionOrderDetailId).ToList();
                var hasScheduleDetailIds = _manufacturingDBContext.ProductionSchedule
                    .Where(sc => delIds.Contains(sc.ProductionOrderDetailId))
                    .Select(sc => sc.ProductionOrderDetailId)
                    .Distinct()
                    .ToList();
                if (hasScheduleDetailIds.Count > 0)
                {
                    var productIds = oldDetail
                        .Where(od => hasScheduleDetailIds.Contains(od.ProductionOrderDetailId))
                        .Select(od => od.ProductId)
                        .ToList();
                    var products = (await _productHelperService.GetListProducts(productIds)).Select(p => p.ProductCode).ToList();
                    throw new BadRequestException(GeneralCode.InvalidParams, $"Tồn tại lịch sản xuất các mặt hàng: {string.Join(",", products)}");
                }

                // Xóa quy trình sản xuất
                var stepOrders = _manufacturingDBContext.ProductionStepOrder.Where(so => delIds.Contains(so.ProductionOrderDetailId)).ToList();
                // Check gộp quy trình
                var stepIds = stepOrders.Select(so => so.ProductionStepId).Distinct().ToList();
                if (_manufacturingDBContext.ProductionStepOrder.Any(so => stepIds.Contains(so.ProductionStepId) && !delIds.Contains(so.ProductionOrderDetailId)))
                    throw new BadRequestException(GeneralCode.InvalidParams, "Yếu cầu xóa các sản phẩm gộp cùng 1 quy trình cùng nhau");
                var steps = _manufacturingDBContext.ProductionStep.Where(s => stepIds.Contains(s.ProductionStepId)).ToList();
                var linkDataRoles = _manufacturingDBContext.ProductionStepLinkDataRole.Where(r => stepIds.Contains(r.ProductionStepId)).ToList();
                var linkDataIds = linkDataRoles.Select(r => r.ProductionStepLinkDataId).Distinct().ToList();
                var linkDatas = _manufacturingDBContext.ProductionStepLinkData.Where(d => linkDataIds.Contains(d.ProductionStepLinkDataId)).ToList();

                // Xóa role
                _manufacturingDBContext.ProductionStepLinkDataRole.RemoveRange(linkDataRoles);
                // Xóa quan hệ step-order
                _manufacturingDBContext.ProductionStepOrder.RemoveRange(stepOrders);
                await _manufacturingDBContext.SaveChangesAsync();
                // Xóa step
                foreach(var item in steps)
                {
                    item.IsDeleted = true;
                }
                // Xóa link data
                foreach (var item in linkDatas)
                {
                    item.IsDeleted = true;
                }

                // Xóa chi tiết
                foreach (var item in oldDetail)
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

        public async Task<bool> DeleteProductionOrder(int productionOrderId)
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

                // Validate lịch sản xuất
                var delIds = detail.Select(od => od.ProductionOrderDetailId).ToList();
                var hasScheduleDetailIds = _manufacturingDBContext.ProductionSchedule
                    .Where(sc => delIds.Contains(sc.ProductionOrderDetailId))
                    .Select(sc => sc.ProductionOrderDetailId)
                    .Distinct()
                    .ToList();
                if (hasScheduleDetailIds.Count > 0)
                {
                    var productIds = detail
                        .Where(od => hasScheduleDetailIds.Contains(od.ProductionOrderDetailId))
                        .Select(od => od.ProductId)
                        .ToList();
                    var products = (await _productHelperService.GetListProducts(productIds)).Select(p => p.ProductCode).ToList();
                    throw new BadRequestException(GeneralCode.InvalidParams, $"Tồn tại lịch sản xuất các mặt hàng: {string.Join(",", products)}");
                }
                // Xóa quy trình sản xuất
                var stepOrders = _manufacturingDBContext.ProductionStepOrder.Where(so => delIds.Contains(so.ProductionOrderDetailId)).ToList();
                var stepIds = stepOrders.Select(so => so.ProductionStepId).Distinct().ToList();
                var steps = _manufacturingDBContext.ProductionStep.Where(s => stepIds.Contains(s.ProductionStepId)).ToList();
                var linkDataRoles = _manufacturingDBContext.ProductionStepLinkDataRole.Where(r => stepIds.Contains(r.ProductionStepId)).ToList();
                var linkDataIds = linkDataRoles.Select(r => r.ProductionStepLinkDataId).Distinct().ToList();
                var linkDatas = _manufacturingDBContext.ProductionStepLinkData.Where(d => linkDataIds.Contains(d.ProductionStepLinkDataId)).ToList();

                // Xóa role
                _manufacturingDBContext.ProductionStepLinkDataRole.RemoveRange(linkDataRoles);
                // Xóa quan hệ step-order
                _manufacturingDBContext.ProductionStepOrder.RemoveRange(stepOrders);
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
    }
}
