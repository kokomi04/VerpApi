using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.Product;
using VErp.Services.Stock.Service.Products;

namespace VErpApi.Controllers.Stock.Products
{
    [Route("api/products")]
    public class ProductMaterialsConsumptionController : VErpBaseController
    {
        private readonly IProductMaterialsConsumptionService _productMaterialsConsumptionService;
        private readonly IProductMaterialsConsumptionGroupService _productMaterialsConsumptionGroupService;

        public ProductMaterialsConsumptionController(IProductMaterialsConsumptionService productMaterialsConsumptionService
            , IProductMaterialsConsumptionGroupService productMaterialsConsumptionGroupService)
        {
            _productMaterialsConsumptionService = productMaterialsConsumptionService;
            _productMaterialsConsumptionGroupService = productMaterialsConsumptionGroupService;
        }

        [HttpPut]
        [Route("{productId}/materialsConsumption")]
        public async Task<bool> UpdateProductMaterialsConsumptionService([FromRoute] int productId, [FromBody] ICollection<ProductMaterialsConsumptionInput> model)
        {
            return await _productMaterialsConsumptionService.UpdateProductMaterialsConsumptionService(productId, model);
        }


        [HttpPut]
        [Route("{productId}/materialsConsumption/{productMaterialsConsumptionId}")]
        public async Task<bool> UpdateProductMaterialsConsumptionService([FromRoute] int productId, [FromRoute] int productMaterialsConsumptionId, [FromBody] ProductMaterialsConsumptionInput model)
        {
            return await _productMaterialsConsumptionService.UpdateProductMaterialsConsumptionService(productId, productMaterialsConsumptionId, model);
        }

        [HttpGet]
        [Route("{productId}/materialsConsumption")]
        public async Task<IEnumerable<ProductMaterialsConsumptionOutput>> GetProductMaterialsConsumptionService([FromRoute] int productId)
        {
            return await _productMaterialsConsumptionService.GetProductMaterialsConsumptionService(productId);
        }

        [HttpGet]
        [Route("materialsConsumption/fieldDataForMapping")]
        public CategoryNameModel GetCustomerFieldDataForMapping()
        {
            return _productMaterialsConsumptionService.GetCustomerFieldDataForMapping();
        }

        [HttpPost]
        [Route("{productId}/materialsConsumption/importFromMapping")]
        public async Task<bool> ImportMaterialsConsumptionFromMapping([FromRoute] int productId, [FromForm] string mapping, [FromForm] IFormFile file, [FromQuery] int materialsConsumptionGroupId)
        {
            if (file == null)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }

            return await _productMaterialsConsumptionService.ImportMaterialsConsumptionFromMapping(productId, JsonConvert.DeserializeObject<ImportExcelMapping>(mapping), file.OpenReadStream(), materialsConsumptionGroupId).ConfigureAwait(true);
        }

        [HttpPost]
        [VErpAction(EnumActionType.View)]
        [Route("materialsConsumptionGroups/search")]
        public async Task<PageData<ProductMaterialsConsumptionGroupModel>> SearchProductMaterialsConsumptionGroup([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _productMaterialsConsumptionGroupService.SearchProductMaterialsConsumptionGroup(keyword, page, size);
        }

        [HttpPost]
        [Route("materialsConsumptionGroups")]
        public async Task<int> AddProductMaterialsConsumptionGroup([FromBody] ProductMaterialsConsumptionGroupModel model)
        {
            return await _productMaterialsConsumptionGroupService.AddProductMaterialsConsumptionGroup(model);
        }

        [HttpPut]
        [Route("materialsConsumptionGroups/{groupId}")]
        public async Task<bool> UpdateProductMaterialsConsumptionGroup([FromRoute] int groupId, [FromBody] ProductMaterialsConsumptionGroupModel model)
        {
            return await _productMaterialsConsumptionGroupService.UpdateProductMaterialsConsumptionGroup(groupId, model);
        }

        [HttpGet]
        [Route("materialsConsumptionGroups/{groupId}")]
        public async Task<ProductMaterialsConsumptionGroupModel> GetProductMaterialsConsumptionGroup([FromRoute] int groupId)
        {
            return await _productMaterialsConsumptionGroupService.GetProductMaterialsConsumptionGroup(groupId);
        }

        [HttpDelete]
        [Route("materialsConsumptionGroups/{groupId}")]
        public async Task<bool> UpdateProductMaterialsConsumptionGroup([FromRoute]  int groupId)
        {
            return await _productMaterialsConsumptionGroupService.DeleteProductMaterialsConsumptionGroup(groupId);
        }
    }
}
