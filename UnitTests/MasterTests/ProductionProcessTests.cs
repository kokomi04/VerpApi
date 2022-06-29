using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using VErp.Services.Manafacturing.Service.ProductionOrder;
using VErp.Services.Manafacturing.Service.ProductionProcess;
using Xunit;

namespace MasterTests
{
    public class ProductionProcessTests : BaseDevelopmentUnitStartup
    {
        private readonly IProductionProcessService productionProcessService;
        private readonly IProductionOrderMaterialsService productOrderMaterialsService;

        public ProductionProcessTests()
            : base(subsidiaryId: 2, userId: 2)
        {
            productionProcessService = webHost.Services.GetService<IProductionProcessService>();
            productOrderMaterialsService = webHost.Services.GetService<IProductionOrderMaterialsService>();
        }

        [Fact]
        public void TestFoundProductionStepLinkDataOutsource()
        {
            //var productionOrderId = 10022;
            try
            {
                //await productionProcessService.FoundProductionStepLinkDataOutsource(productionOrderId);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [Fact]
        public async Task TestGetProductionOrderMaterials()
        {
            var productionOrderId = 10045;
            try
            {
                var data = await productOrderMaterialsService.GetProductionOrderMaterialsCalc(productionOrderId);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
