using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.QueueMessage;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper.Hr;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper.Inv;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper.Product;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper.QueueHelper;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper.System;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Services.Stock.Service.FileResources;
using VErp.Services.Stock.Service.Products;
using VErp.Services.Stock.Service.Stock;
using VErp.Services.Stock.Service.Stock.Implement;

namespace ReCaclProductionOrderStatus.Services
{
    public interface IMigrateInventoryService
    {
        Task<ProductionOrderCalcStatusV2Message> GetProductionOrderCalcStatusV2Message(ProductionOrderStatusInventorySumaryMessage msg);
    }

    public class MigrateInventoryService : InventoryServiceAbstract, IMigrateInventoryService
    {
        public MigrateInventoryService(StockDBContext stockContext
            , ILogger<InventoryService> logger
            , ICurrentContextService currentContextService
            , ICustomGenCodeHelperService customGenCodeHelperService
            , IProductionOrderQueueHelperService productionOrderQueueHelperService
            ) : base(stockContext, logger, customGenCodeHelperService, currentContextService, productionOrderQueueHelperService)
        {

        }
    }
}
