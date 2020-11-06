using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.ErrorCodes;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Manafacturing.Model.ProductionOrder;

namespace VErp.Services.Manafacturing.Service.ProductionOrder.Implement
{
    public class ProductionOrderService : IProductionOrderService
    {
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly IActivityLogService _activityLogService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public ProductionOrderService(ManufacturingDBContext manufacturingDB
            , IActivityLogService activityLogService
            , ILogger<ProductionOrderService> logger
            , IMapper mapper)
        {
            _manufacturingDBContext = manufacturingDB;
            _activityLogService = activityLogService;
            _logger = logger;
            _mapper = mapper;
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

        public async Task<int> CreateProductionOrder(ProductionOrderModel data)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteProductionOrder(int productionOrderId)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateProductionOrder(int productionOrderId, ProductionOrderModel data)
        {
            throw new NotImplementedException();
        }
    }
}
