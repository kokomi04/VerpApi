using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Service.Activity;
using VErp.Services.Stock.Model.Dictionary;
using VErp.Services.Stock.Model.Product;
using VErp.Services.Stock.Model.Stock;
using VErp.Services.Stock.Service.Dictionary;
using VErp.Services.Stock.Service.FileResources;
using VErp.Services.Stock.Service.Products;
using VErp.Services.Stock.Service.Stock;

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
        [VErpAction(EnumAction.View)]
        [Route("")]
        public async Task<PageData<ProductCateOutput>> Search([FromBody] Clause filters, [FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _productCateService.GetList(keyword, page, size, filters);
        }


        [HttpGet]
        [Route("{productId}")]
        public async Task<ProductCateOutput> GetProduct([FromRoute] int productCateId)
        {
            return await _productCateService.GetInfoProductCate(productCateId);
        }
    }
}