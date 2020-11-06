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
            var query = _manufacturingDBContext.ProductionOrder.AsQueryable();
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(o => o.ProductionOrderCode.Contains(keyword) || o.Description.Contains(keyword));
            }
            query = query.InternalFilter(filters);
            var total = await query.CountAsync();
            var lst = await (size > 0 ? query.Skip((page - 1) * size).Take(size) : query)
                .ProjectTo<ProductionOrderListModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return (lst, total);
        }

        public async Task<ProductionOrderModel> GetProductionOrder(int productionOrderId)
        {
            return _manufacturingDBContext.ProductionOrder
                .Include(o => o.ProductionOrderDetail)
                .Where(o => o.ProductionOrderId == productionOrderId)
                .ProjectTo<ProductionOrderModel>(_mapper.ConfigurationProvider)
                .FirstOrDefault();
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
                var productOrder = _mapper.Map<ProductionOrderEntity>(data);
                _manufacturingDBContext.ProductionOrder.Add(productOrder);
                await _manufacturingDBContext.SaveChangesAsync();
                trans.Commit();
                data.ProductionOrderId = productOrder.ProductionOrderId;
                await _activityLogService.CreateLog(EnumObjectType.ProductionOrder, productOrder.ProductionOrderId, $"Thêm mới dữ liệu lệnh sản xuất {productOrder.ProductionOrderId}", data.JsonSerialize());
                return data;
            }
            catch (Exception ex)
            {
                trans.TryRollbackTransaction();
                _logger.LogError(ex, "CreateProductOrder");
                throw;
            }
        }

        public Task<bool> DeleteProductionOrder(int productionOrderId)
        {
            throw new NotImplementedException();
        }

        public Task<ProductionOrderModel> UpdateProductionOrder(int productionOrderId, ProductionOrderModel data)
        {
            throw new NotImplementedException();
        }
    }
}
