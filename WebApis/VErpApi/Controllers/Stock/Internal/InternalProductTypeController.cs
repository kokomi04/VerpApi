using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Dictionary;
using VErp.Services.Stock.Service.Dictionary;

namespace VErpApi.Controllers.Stock.Internal
{
    [Route("api/internal/[controller]")]
    [ApiController]
    public class InternalProductTypeController : CrossServiceBaseController
    {
        private readonly IProductTypeService _productTypeService;
        public InternalProductTypeController(IProductTypeService productTypeService)
        {
            _productTypeService = productTypeService;
        }

        [HttpPost]
        [VErpAction(EnumActionType.View)]
        [Route("")]
        public async Task<PageData<ProductTypeOutput>> Search([FromBody] Clause filters, [FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _productTypeService.GetList(keyword, page, size, filters);
        }
    }
}