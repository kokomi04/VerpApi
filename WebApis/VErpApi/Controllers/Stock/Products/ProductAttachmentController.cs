using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Dictionary;
using VErp.Services.Stock.Model.Product;
using VErp.Services.Stock.Service.Dictionary;
using VErp.Services.Stock.Service.Products;

namespace VErpApi.Controllers.Stock.Products
{
    [Route("api/productAttachment")]
    public class ProductAttachmentController : VErpBaseController
    {
        private readonly IProductAttachmentService _productAttachmentService;
        public ProductAttachmentController(IProductAttachmentService productAttachmentService)
        {
            _productAttachmentService = productAttachmentService;
        }

        [HttpGet]
        [Route("{productId}")]
        public async Task<IList<ProductAttachmentModel>> GetAttachments([FromRoute] int productId)
        {
            return await _productAttachmentService.GetAttachments(productId);
        }

        [HttpPut]
        [Route("{productId}")]
        public async Task<bool> Update([FromRoute] int productId, [FromBody] IList<ProductAttachmentModel> model)
        {
            return await _productAttachmentService.Update(productId, model);
        }
    }
}
