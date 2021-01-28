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
using VErp.Services.Manafacturing.Service.Outsource;
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
    public class OutsourceOrderTests : BaseDevelopmentUnitStartup
    {
        private readonly IOutsourcePartOrderService outsourcePartOrderService;

        public OutsourceOrderTests()
            : base(subsidiaryId: 2, userId: 2)
        {
            outsourcePartOrderService = webHost.Services.GetService<IOutsourcePartOrderService>();
        }

        [Fact]
        public async Task TestGetMaterialsOutsourcePartOrder()
        {
            var outsourceOrderId = 10048;
            try
            {
                var rs = await outsourcePartOrderService.GetMaterials(outsourceOrderId);
            }
            catch (Exception)
            {

                throw;
            }


        }
        
    }
}
