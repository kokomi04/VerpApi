using System;
using System.Threading.Tasks;
using Xunit;

namespace MasterTests
{
    public class OutsourceOrderTests : BaseDevelopmentUnitStartup
    {
        //private readonly IOutsourcePartOrderService outsourcePartOrderService;

        public OutsourceOrderTests()
            : base(subsidiaryId: 2, userId: 2)
        {
            // outsourcePartOrderService = webHost.Services.GetService<IOutsourcePartOrderService>();
        }

        [Fact]
        public async Task TestGetMaterialsOutsourcePartOrder()
        {
            //var outsourceOrderId = 10055;
            try
            {
                // var rs = await outsourcePartOrderService.GetMaterials(outsourceOrderId);
                await Task.CompletedTask;
            }
            catch (Exception)
            {

                throw;
            }


        }

    }
}
