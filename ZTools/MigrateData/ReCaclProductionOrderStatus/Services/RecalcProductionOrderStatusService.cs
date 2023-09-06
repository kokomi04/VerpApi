using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject.QueueMessage;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Services.Manafacturing.Service.ProductionProcess;

namespace ReCaclProductionOrderStatus.Services
{
    public interface IRecalcProductionOrderStatusService
    {
        Task Execute();
    }

    public class RecalcProductionOrderStatusService : IRecalcProductionOrderStatusService
    {
        private readonly IMigrateInventoryService migrateInventoryService;
        private readonly IProductionProgressService productionProgressService;
        private readonly ManufacturingDBContext manufacturingDBContext;

        public RecalcProductionOrderStatusService(IProductionProgressService productionProgressService, IMigrateInventoryService migrateInventoryService, ManufacturingDBContext manufacturingDBContext)
        {
            this.productionProgressService = productionProgressService;
            this.migrateInventoryService = migrateInventoryService;
            this.manufacturingDBContext = manufacturingDBContext;
        }

        public async Task Execute()
        {
            var productionOrderCodes = await manufacturingDBContext.ProductionOrder.Select(o => o.ProductionOrderCode).ToListAsync();
            foreach (var code in productionOrderCodes)
            {
                var data = await migrateInventoryService.GetProductionOrderCalcStatusV2Message(new ProductionOrderStatusInventorySumaryMessage()
                {
                    ProductionOrderCode = code,
                    Description = "Auto migrate"
                });

                await productionProgressService.CalcAndUpdateProductionOrderStatusV2(data);
            }
        }
    }
}
