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
    public class InternalProductCateController : CrossServiceBaseController
    {
        private readonly IProductCateService _productCateService;
        public InternalProductCateController(IProductCateService productCateService)
        {
            _productCateService = productCateService;
        }

        [HttpPost]
        [VErpAction(EnumActionType.View)]
        [Route("")]
        public async Task<PageData<ProductCateOutput>> Search([FromBody] Clause filters, [FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size, [FromQuery] string orderBy, [FromQuery] bool asc)
        {
            return await _productCateService.GetList(keyword, page, size, orderBy, asc, filters);
        }


        [HttpGet]
        [Route("{productId}")]
        public async Task<ProductCateOutput> GetProduct([FromRoute] int productCateId)
        {
            return await _productCateService.GetInfoProductCate(productCateId);
        }
    }
}