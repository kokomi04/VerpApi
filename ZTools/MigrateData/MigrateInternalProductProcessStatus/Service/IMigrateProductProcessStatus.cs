using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using VErp.Commons.Enums.Manafacturing;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.ManufacturingDB;
using VErp.Infrastructure.EF.StockDB;
using VErp.Services.Manafacturing.Service.ProductionProcess;
using static VErp.Commons.Enums.Manafacturing.EnumProductionProcess;

namespace MigrateProductProcessStatus.Services
{
    public interface IMigrateProductProcessStatus
    {
        Task Execute();
    }

    public class MigrateProductProcessStatus : IMigrateProductProcessStatus
    {
        private readonly StockDBContext _stockDBContext;
        private readonly ManufacturingDBContext _manufacturingDBContext;
        private readonly IValidateProductionProcessService _validateProductionProcess;
        private readonly IProductionProcessService _productionProcessService;

        public MigrateProductProcessStatus(StockDBContext stockDBContext, ManufacturingDBContext manufacturingDBContext,
            IValidateProductionProcessService validateProductionProcess,
            IProductionProcessService productionProcessService)
        {
            _stockDBContext = stockDBContext;
            _manufacturingDBContext = manufacturingDBContext;
            _validateProductionProcess = validateProductionProcess;
            _productionProcessService = productionProcessService;
        }

        public async Task Execute()
        {
            var products = await _stockDBContext
                .Product
                .Where(p => p.ProductionProcessStatusId != (int)EnumProductionProcessStatus.NotCreatedYet)
                .ToListAsync();
            var productIds = products.Select(p => (long)p.ProductId).ToList();
            var productProcess = await _productionProcessService.GetProductionProcessByContainerIds(EnumContainerType.Product, productIds);

            var total = productIds.Count;
            var dem = 0;
            Console.Write($"Calc 0 / {total}");

            foreach (var containerId in productIds)
            {
                var mess = (await _validateProductionProcess.ValidateProductionProcess(EnumContainerType.Product, containerId, productProcess.FirstOrDefault(p => p.ContainerId == containerId)));
                if (mess.Count() > 0)
                {
                    var product = products.FirstOrDefault(p => p.ProductId == containerId);
                    if (product != null && product.ProductionProcessStatusId != (int)EnumProductionProcessStatus.CreateButNotYet)
                    {
                        product.ProductionProcessStatusId = (int)EnumProductionProcessStatus.CreateButNotYet;
                    }
                }
                else
                {
                    var product = products.FirstOrDefault(p => p.ProductId == containerId);
                    if (product != null && product.ProductionProcessStatusId != (int)EnumProductionProcessStatus.Created)
                    {
                        product.ProductionProcessStatusId = (int)EnumProductionProcessStatus.Created;
                    }
                }

                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write($"Calc {dem++} / {total}  ");

            }
            await _stockDBContext.SaveChangesAsync();
        }
    }
}
