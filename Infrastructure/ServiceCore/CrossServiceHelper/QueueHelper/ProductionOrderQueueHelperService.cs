using System;
using System.Threading.Tasks;
using Verp.Cache.Caching;
using VErp.Commons.Constants.Caching;
using VErp.Commons.GlobalObject.QueueMessage;
using VErp.Commons.GlobalObject.QueueName;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper.General;

namespace VErp.Infrastructure.ServiceCore.CrossServiceHelper.QueueHelper
{


    public interface IProductionOrderQueueHelperService
    {
        Task<bool> ProductionOrderStatiticChanges(string productionOrderCode, string description);
        
        //Task<bool> CalcProductionOrderStatus(ProductionOrderCalcStatusMessage msg);

        Task<bool> CalcProductionOrderStatusV2(ProductionOrderCalcStatusV2Message msg);
    }


    public class ProductionOrderQueueHelperService : IProductionOrderQueueHelperService
    {
        private readonly IQueueProcessHelperService _queueProcessHelperService;
        private readonly ICachingService _cachingService;
        public ProductionOrderQueueHelperService(IQueueProcessHelperService queueProcessHelperService, ICachingService cachingService)
        {
            _queueProcessHelperService = queueProcessHelperService;
            _cachingService = cachingService;
        }

        public async Task<bool> ProductionOrderStatiticChanges(string productionOrderCode, string description)
        {
            var tag = ProductionOrderCacheKeys.CACHE_CALC_PRODUCTION_ORDER_STATUS;
            var key = ProductionOrderCacheKeys.CalcProductionOrderStatusPending(productionOrderCode);

            _cachingService.TryGetSet<int>(tag, key, TimeSpan.FromMinutes(5), (currentCount) =>
            {
                return ++currentCount;
            });

            return await _queueProcessHelperService.EnqueueAsync(ManufacturingQueueNameConstants.PRODUCTION_INVENTORY_STATITICS, new ProductionOrderStatusInventorySumaryMessage()
            {
                Description = description,
                ProductionOrderCode = productionOrderCode,
            });
        }

        //public async Task<bool> CalcProductionOrderStatus(ProductionOrderCalcStatusMessage msg)
        //{
        //    return await _queueProcessHelperService.EnqueueAsync(ManufacturingQueueNameConstants.PRODUCTION_CALC_STATUS, msg);
        //}

        public async Task<bool> CalcProductionOrderStatusV2(ProductionOrderCalcStatusV2Message msg)
        {
            return await _queueProcessHelperService.EnqueueAsync(ManufacturingQueueNameConstants.PRODUCTION_CALC_STATUS_V2, msg);
        }
    }
}
