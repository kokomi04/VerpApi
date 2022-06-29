using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Stock.Model.Product;
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
