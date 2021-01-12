using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Dictionary;
using VErp.Services.Stock.Model.Product;
using VErp.Services.Stock.Service.Dictionary;
using VErp.Services.Stock.Service.Products;

namespace VErpApi.Controllers.Stock.Products
{
    [Route("api/productBom")]
    public class ProductBomController : VErpBaseController
    {
        private readonly IProductBomService _productBomService;
        public ProductBomController(IProductBomService productBomService)
        {
            _productBomService = productBomService;
        }

        [HttpGet]
        [Route("{productId}")]
        public async Task<IList<ProductBomOutput>> GetBOM([FromRoute] int productId)
        {
            return await _productBomService.GetBom(productId);
        }

        [HttpPut]
        [Route("{productId}")]
        public async Task<bool> Update([FromRoute] int productId, [FromBody] ProductBomModel model)
        {
            if (model == null) throw new BadRequestException(GeneralCode.InvalidParams);
            return await _productBomService.Update(productId, model.ProductBoms, model.ProductMaterials);
        }

        [HttpPost]
        [VErpAction(EnumActionType.View)]
        [Route("exports")]
        public async Task<IActionResult> Export([FromBody] IList<int> productIds)
        {
            if (productIds == null) throw new BadRequestException(GeneralCode.InvalidParams);
            var (stream, fileName, contentType) = await _productBomService.ExportBom(productIds);

            return new FileStreamResult(stream, !string.IsNullOrWhiteSpace(contentType) ? contentType : "application/octet-stream") { FileDownloadName = fileName };
        }

        [HttpGet]
        [Route("fieldDataForMapping")]
        public CategoryNameModel GetCustomerFieldDataForMapping()
        {
            return _productBomService.GetCustomerFieldDataForMapping();
        }

        [HttpPost]
        [Route("importFromMapping")]
        public async Task<bool> ImportFromMapping([FromForm] string mapping, [FromForm] IFormFile file)
        {
            if (file == null)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }

            return await _productBomService.ImportBomFromMapping(JsonConvert.DeserializeObject<ImportExcelMapping>(mapping), file.OpenReadStream()).ConfigureAwait(true);
        }
    }
}
