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
        private readonly IValidateProductionOrderService _validateProductionOrderService;

        public ProductionOrderService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<ProductionOrderService> logger
            , IMapper mapper
            , ICustomGenCodeHelperService customGenCodeHelperService
            , IProductHelperService productHelperService
            , IValidateProductionOrderService validateProductionOrderService)
        {
            _manufacturingDBContext = manufacturingDB;
            _activityLogService = activityLogService;
            _logger = logger;
            _mapper = mapper;
            _customGenCodeHelperService = customGenCodeHelperService;
            _productHelperService = productHelperService;
            _validateProductionOrderService = validateProductionOrderService;
        }

        public async Task<PageData<ProductionOrderListModel>> GetProductionOrders(string keyword, int page, int size, string orderByFieldName, bool asc, Clause filters = null)
        {
            keyword = (keyword ?? "").Trim();
            var parammeters = new List<SqlParameter>();

            var whereCondition = new StringBuilder();
            if (!string.IsNullOrEmpty(keyword))
            {
                whereCondition.Append("(v.ProductionOrderCode LIKE @KeyWord ");
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

            if (string.IsNullOrEmpty(orderByFieldName))
            {
                orderByFieldName = "ProductionOrderId";
                asc = true;
            }

            var sql = new StringBuilder(
                @$";WITH tmp AS (
                    SELECT ");

            if(size < 0)
            {
                sql.Append($"ROW_NUMBER() OVER (ORDER BY g.{orderByFieldName} {(asc ? "" : "DESC")}) AS RowNum,");
            }

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

            if (size >= 0)
            {
                sql.Append(@$"ORDER BY g.{orderByFieldName} {(asc ? "" : "DESC")}
                            OFFSET {(page - 1) * size} ROWS
                            FETCH NEXT { size}
                            ROWS ONLY");
            }

            sql.Append(@")
                SELECT v.* FROM tmp t
                LEFT JOIN vProductionOrderDetail v ON t.ProductionOrderId = v.ProductionOrderId ");

            if(size < 0)
            {
                sql.Append(" ORDER BY t.RowNum");
            }

            var resultData = await _manufacturingDBContext.QueryDataTable(sql.ToString(), parammeters.Select(p => p.CloneSqlParam()).ToArray());
            var lst = resultData.ConvertData<ProductionOrderListEntity>().AsQueryable().ProjectTo<ProductionOrderListModel>(_mapper.ConfigurationProvider).ToList();

            return (lst, total, additionResult);
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

                var warnings = await _validateProductionOrderService.GetWarningProductionOrder(productionOrderId, productOrder.ProductionOrderDetail);
                productOrder.MarkInValid = warnings.Count > 0;

                //var detailIds = productOrder.ProductionOrderDetail.Select(od => od.ProductionOrderDetailId).ToList();
                //var countDetailId = _manufacturingDBContext.ProductionStepOrder
                //    .Where(so => detailIds.Contains(so.ProductionOrderDetailId))
                //    .Select(so => so.ProductionOrderDetailId)
                //    .Distinct()
                //    .Count();

                //if (countDetailId == 0)
                //{
                //    productOrder.ProcessStatus = EnumProcessStatus.Waiting;
                //}
                //else if (countDetailId < detailIds.Count)
                //{
                //    productOrder.ProcessStatus = EnumProcessStatus.Incomplete;
                //}
                //else
                //{
                //    productOrder.ProcessStatus = EnumProcessStatus.Complete;
                //}
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

                var productionOrder = _mapper.Map<ProductionOrderEntity>(data);
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
                await _manufacturingDBContext.SaveChangesAsync();

                

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

        public async Task<ProductionOrderInputModel> UpdateProductionOrder(long productionOrderId, ProductionOrderInputModel data)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockProductionOrderKey(productionOrderId));
            using var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
            {
                if (data.StartDate <= 0) throw new BadRequestException(GeneralCode.InvalidParams, "Yêu cầu nhập ngày bắt đầu sản xuất.");
                if (data.EndDate <= 0) throw new BadRequestException(GeneralCode.InvalidParams, "Yêu cầu nhập ngày kết thúc sản xuất.");
                if (data.Date <= 0) throw new BadRequestException(GeneralCode.InvalidParams, "Yêu cầu nhập ngày chứng từ.");

                var productionOrder = _manufacturingDBContext.ProductionOrder
                    .Where(o => o.ProductionOrderId == productionOrderId)
                    .FirstOrDefault();
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

                //// Xóa quy trình sản xuất
                //var delIds = oldDetail.Select(od => od.ProductionOrderDetailId).ToList();
                //var stepOrders = _manufacturingDBContext.ProductionStepOrder.Where(so => delIds.Contains(so.ProductionOrderDetailId)).ToList();
                //// Check gộp quy trình
                //var stepIds = stepOrders.Select(so => so.ProductionStepId).Distinct().ToList();
                //if (_manufacturingDBContext.ProductionStepOrder.Any(so => stepIds.Contains(so.ProductionStepId) && !delIds.Contains(so.ProductionOrderDetailId)))
                //    throw new BadRequestException(GeneralCode.InvalidParams, "Yêu cầu xóa các sản phẩm gộp cùng 1 quy trình cùng nhau");
                //var steps = _manufacturingDBContext.ProductionStep.Where(s => stepIds.Contains(s.ProductionStepId)).ToList();
                //var linkDataRoles = _manufacturingDBContext.ProductionStepLinkDataRole.Where(r => stepIds.Contains(r.ProductionStepId)).ToList();
                //var linkDataIds = linkDataRoles.Select(r => r.ProductionStepLinkDataId).Distinct().ToList();
                //var linkDatas = _manufacturingDBContext.ProductionStepLinkData.Where(d => linkDataIds.Contains(d.ProductionStepLinkDataId)).ToList();

                //// Xóa role
                //_manufacturingDBContext.ProductionStepLinkDataRole.RemoveRange(linkDataRoles);
                //// Xóa quan hệ step-order
                //_manufacturingDBContext.ProductionStepOrder.RemoveRange(stepOrders);

                await _manufacturingDBContext.SaveChangesAsync();
                //// Xóa step
                //foreach (var item in steps)
                //{
                //    item.IsDeleted = true;
                //}
                //// Xóa link data
                //foreach (var item in linkDatas)
                //{
                //    item.IsDeleted = true;
                //}

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

                var delIds = detail.Select(od => od.ProductionOrderDetailId).ToList();
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

        public async Task<bool> UpdateProductionOrderStatus(long productionOrderId, ProductionOrderStatusModel status)
        {
            var productionOrder = _manufacturingDBContext.ProductionOrder
                .Include(po => po.ProductionOrderDetail)
                .FirstOrDefault(po => po.ProductionOrderId == productionOrderId);

            if (productionOrder == null)
                throw new BadRequestException(GeneralCode.ItemNotFound, "Lệnh sản xuất không tồn tại");

            try
            {
                if (status.ProductionOrderStatus == EnumProductionStatus.Finished)
                {
                    // Check nhận đủ số lượng đầu ra
                    var parammeters = new SqlParameter[]
                    {
                        new SqlParameter("@ProductionOrderId", productionOrderId)
                    };
                    var resultData = await _manufacturingDBContext.ExecuteDataProcedure("asp_ProductionHandover_GetInventoryRequirementByProductionOrder", parammeters);

                    var inputInventories = resultData.ConvertData<ProductionInventoryRequirementEntity>();

                    bool isFinish = true;

                    foreach (var productionOrderDetail in productionOrder.ProductionOrderDetail)
                    {
                        var quantity = inputInventories
                            .Where(i => i.ProductId == productionOrderDetail.ProductId && i.Status != (int)EnumProductionInventoryRequirementStatus.Rejected)
                            .Sum(i => i.ActualQuantity.GetValueOrDefault());

                        if (quantity != (productionOrderDetail.Quantity + productionOrderDetail.ReserveQuantity))
                        {
                            isFinish = false;
                            break;
                        }
                    }
                    if (isFinish)
                    {
                        productionOrder.ProductionOrderStatus = (int)status.ProductionOrderStatus;
                        await _activityLogService.CreateLog(EnumObjectType.ProductionOrder, productionOrder.ProductionOrderId, $"Cập nhật trạng thái lệnh sản xuất ", new { productionOrder, status, isManual = false }.JsonSerialize());
                    }
                }
                else
                {

                    if (productionOrder.ProductionOrderStatus < (int)status.ProductionOrderStatus)
                    {
                        productionOrder.ProductionOrderStatus = (int)status.ProductionOrderStatus;
                        await _activityLogService.CreateLog(EnumObjectType.ProductionOrder, productionOrder.ProductionOrderId, $"Cập nhật trạng thái lệnh sản xuất ", new { productionOrder, status, isManual = false }.JsonSerialize());
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

        public async Task<bool> UpdateManualProductionOrderStatus(long productionOrderId, ProductionOrderStatusModel status)
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
    }
}
