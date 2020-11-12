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

        public ProductionOrderService(ManufacturingDBContext manufacturingDB
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
        public async Task<PageData<ProductionOrderListModel>> GetProductionOrders(string keyword, int page, int size, Clause filters = null)
        {
            keyword = (keyword ?? "").Trim();
            var query = _manufacturingDBContext.ProductionOrderDetail
                .Include(od => od.ProductionOrder)
                .ProjectTo<ProductionOrderListModel>(_mapper.ConfigurationProvider);
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(o => o.ProductionOrderCode.Contains(keyword) || o.Description.Contains(keyword));
            }
            query = query.InternalFilter(filters);
            var total = await query.CountAsync();
            var lst = await (size > 0 ? query.Skip((page - 1) * size).Take(size) : query)
                .ToListAsync();
            var lstGroup = lst.GroupBy(o => o.ProductionOrderId).ToList();
            foreach (var group in lstGroup)
            {
                var parammeters = new SqlParameter[]
                {
                    new SqlParameter("@ProductionOrderId", group.Key)
                };
                var resultData = await _manufacturingDBContext.ExecuteDataProcedure("asp_ProductionOrder_GetExtraInfoByProductionOrderId", parammeters);

                var lstEtraInfo = resultData.ConvertData<ProductionOrderExtraInfo>();

                foreach (var item in group)
                {
                    var extraInfo = lstEtraInfo.First(i => i.ProductionOrderDetailId == item.ProductionOrderDetailId);
                    item.ExtraInfo = extraInfo;
                }
            }

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

        public async Task<ProductionOrderModel> GetProductionOrder(int productionOrderId)
        {
            var productOrder = _manufacturingDBContext.ProductionOrder
                .Include(o => o.ProductionOrderDetail)
                .Where(o => o.ProductionOrderId == productionOrderId)
                .ProjectTo<ProductionOrderModel>(_mapper.ConfigurationProvider)
                .FirstOrDefault();
            if (productOrder != null)
            {
                var parammeters = new SqlParameter[]
                {
                    new SqlParameter("@ProductionOrderId", productionOrderId)
                };
                var resultData = await _manufacturingDBContext.ExecuteDataProcedure("asp_ProductionOrder_GetExtraInfoByProductionOrderId", parammeters);

                var lstEtraInfo = resultData.ConvertData<ProductionOrderExtraInfo>();

                foreach (var item in productOrder.ProductionOrderDetail)
                {
                    var extraInfo = lstEtraInfo.First(i => i.ProductionOrderDetailId == item.ProductionOrderDetailId);
                    item.ExtraInfo = extraInfo;
                }

                productOrder.HasProcess = _manufacturingDBContext.ProductionStep
                .Any(s => s.ContainerTypeId == (int)EnumProductionProcess.ContainerType.LSX && s.ContainerId == productionOrderId);
            }

            return productOrder;
        }

        public async Task<ProductionOrderModel> CreateProductionOrder(ProductionOrderModel data)
        {
            using var @lock = await DistributedLockFactory.GetLockAsync(DistributedLockFactory.GetLockProductionOrderKey(0));
            using var trans = await _manufacturingDBContext.Database.BeginTransactionAsync();
            try
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
                    _manufacturingDBContext.ProductionOrderDetail.Add(entity);
                }
                await _manufacturingDBContext.SaveChangesAsync();
                trans.Commit();
                data.ProductionOrderId = productionOrder.ProductionOrderId;
                await _customGenCodeHelperService.ConfirmCode(EnumObjectType.InputType, 0);
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
        public async Task<ProductionOrderModel> UpdateProductionOrder(int productionOrderId, ProductionOrderModel data)
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
                foreach (var item in oldDetail)
                {
                    item.IsDeleted = true;
                }

                await _manufacturingDBContext.SaveChangesAsync();
                trans.Commit();

                await _activityLogService.CreateLog(EnumObjectType.ProductionOrder, productionOrder.ProductionOrderId, $"Cập nhật dữ liệu lệnh sản xuất {productionOrder.ProductionOrderCode}", data.JsonSerialize());
                data = _manufacturingDBContext.ProductionOrder
                    .Include(o => o.ProductionOrderDetail)
                    .Where(o => o.ProductionOrderId == productionOrderId)
                    .ProjectTo<ProductionOrderModel>(_mapper.ConfigurationProvider)
                    .FirstOrDefault();
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
