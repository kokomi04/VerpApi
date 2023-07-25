using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Manafacturing.Model.ProductionOrder;
using VErp.Services.Manafacturing.Service.ProductionHandover;

namespace VErpApi.Controllers.Manufacturing.Internal
{
    //[Route("api/internal/[controller]")]
    //[ApiController]
    //public class InternalProductionHandoverController : CrossServiceBaseController
    //{
    //    private readonly IProductionHandoverReceiptService _productionHandoverReceiptService;
    //    private readonly IMaterialAllocationService _materialAllocationService;

    //    public InternalProductionHandoverController(IProductionHandoverReceiptService productionHandoverReceiptService, IMaterialAllocationService materialAllocationService)
    //    {
    //        _productionHandoverReceiptService = productionHandoverReceiptService;
    //        _materialAllocationService = materialAllocationService;
    //    }

    //    [HttpPut]
    //    [Route("ignore-allocation")]
    //    public async Task<bool> UpdateIgnoreAllocation([FromBody] string[] productionOrderCodes)
    //    {
    //        return await _materialAllocationService.UpdateIgnoreAllocation(productionOrderCodes);
    //    }
    //}
}