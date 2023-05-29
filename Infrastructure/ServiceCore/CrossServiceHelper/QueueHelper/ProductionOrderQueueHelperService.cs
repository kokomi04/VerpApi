using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject.QueueMessage;
using VErp.Commons.GlobalObject.QueueName;
using VErp.Commons.Library;
using VErp.Infrastructure.AppSettings.Model;
using VErp.Infrastructure.ServiceCore.Service;

namespace VErp.Infrastructure.ServiceCore.CrossServiceHelper.QueueHelper
{


    public interface IProductionOrderQueueHelperService
    {
        Task<bool> ProductionOrderStatiticChanges(string productionOrderCode, string description);
        Task<bool> CalcProductionOrderStatus(ProductionOrderCalcStatusMessage msg);
    }


    public class ProductionOrderQueueHelperService : IProductionOrderQueueHelperService
    {
        private readonly IQueueProcessHelperService _queueProcessHelperService;
        public ProductionOrderQueueHelperService(IQueueProcessHelperService queueProcessHelperService)
        {
            _queueProcessHelperService = queueProcessHelperService;
        }

        public async Task<bool> ProductionOrderStatiticChanges(string productionOrderCode, string description)
        {

            return await _queueProcessHelperService.EnqueueAsync(ManufacturingQueueNameConstants.PRODUCTION_INVENTORY_STATITICS, new ProductionOrderStatusInventorySumaryMessage()
            {
                Description = description,
                ProductionOrderCode = productionOrderCode,
            });
        }

        public async Task<bool> CalcProductionOrderStatus(ProductionOrderCalcStatusMessage msg)
        {
            return await _queueProcessHelperService.EnqueueAsync(ManufacturingQueueNameConstants.PRODUCTION_CALC_STATUS, msg);
        }
    }
}
