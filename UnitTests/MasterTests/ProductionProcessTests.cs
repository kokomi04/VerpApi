using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library;
using VErp.Infrastructure.EF.StockDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Service.ProductionOrder;
using VErp.Services.Manafacturing.Service.ProductionProcess;
using VErp.Services.Master.Service.Dictionay;
using VErp.Services.Stock.Model.Inventory;
using VErp.Services.Stock.Model.Product;
using VErp.Services.Stock.Model.Stock;
using VErp.Services.Stock.Service.Dictionary;
using VErp.Services.Stock.Service.Products;
using VErp.Services.Stock.Service.Stock;
using Xunit;
using static VErp.Commons.GlobalObject.InternalDataInterface.ProductModel;

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
        public async Task TestFoundProductionStepLinkDataOutsource()
        {
            var productionOrderId = 10022;
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
                var data = await productOrderMaterialsService.GetProductionOrderMaterials(productionOrderId);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
