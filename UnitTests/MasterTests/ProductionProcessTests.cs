using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using VErp.Services.Manafacturing.Service.ProductionAssignment;
using VErp.Services.Manafacturing.Service.ProductionHandover;
using VErp.Services.Manafacturing.Service.ProductionOrder;
using VErp.Services.Manafacturing.Service.ProductionProcess;
using VErp.Services.Manafacturing.Service.StatusProcess;
using Xunit;

namespace MasterTests
{
    public class ProductionProcessTests : BaseDevelopmentUnitStartup
    {
        private readonly IProductionProcessService productionProcessService;
        private readonly IProductionOrderMaterialsService productOrderMaterialsService;
        private readonly IProductionAssignmentService productionAssignmentService;
        private readonly IProductionHandoverReceiptService productionHandoverReceiptService;

        public ProductionProcessTests()
            : base(subsidiaryId: 2, userId: 2)
        {
            productionProcessService = webHost.Services.GetService<IProductionProcessService>();
            productOrderMaterialsService = webHost.Services.GetService<IProductionOrderMaterialsService>();
            productionAssignmentService = webHost.Services.GetService<IProductionAssignmentService>();
            productionHandoverReceiptService = webHost.Services.GetService<IProductionHandoverReceiptService>();
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

        [Fact]
        public async Task TestAssignStatus()
        {
            var productionOrderId = 20708L;
            try
            {
                //await productionAssignmentService.UpdateProductionOrderAssignmentStatus(new[] { productionOrderId });
            }
            catch (Exception)
            {

                throw;
            }
        }

        [Fact]
        public async Task UpdateAllAssignStatus()
        {
          
            try
            {
                var productionOrderIds = await _manufacturingDBContext.ProductionAssignment.Select(a => a.ProductionOrderId).Distinct().ToListAsync();
               // await productionAssignmentService.UpdateProductionOrderAssignmentStatus(productionOrderIds);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
