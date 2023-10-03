﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ApiCore.ModelBinders;
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
            return await _productMaterialsConsumptionService.UpdateProductMaterialsConsumption(productId, model);
        }


        [HttpPut]
        [Route("{productId}/materialsConsumption/{productMaterialsConsumptionId}")]
        public async Task<bool> UpdateProductMaterialsConsumptionService([FromRoute] int productId, [FromRoute] int productMaterialsConsumptionId, [FromBody] ProductMaterialsConsumptionInput model)
        {
            return await _productMaterialsConsumptionService.UpdateProductMaterialsConsumption(productId, productMaterialsConsumptionId, model);
        }

        [HttpPost]
        [Route("{productId}/materialsConsumption")]
        public async Task<long> AddProductMaterialsConsumptionService([FromRoute] int productId, [FromBody] ProductMaterialsConsumptionInput model)
        {
            return await _productMaterialsConsumptionService.AddProductMaterialsConsumption(productId, model);
        }

        [HttpPost]
        [Route("materialsConsumptionByProductIds")]
        [VErpAction(EnumActionType.View)]
        public async Task<IDictionary<int, IEnumerable<ProductMaterialsConsumptionOutput>>> GetProductMaterialsConsumptionByProductIds([FromBody] IList<int> productIds)
        {
            return await _productMaterialsConsumptionService.GetProductMaterialsConsumptionByProductIds(productIds);
        }


        [HttpGet]
        [Route("{productId}/materialsConsumption")]
        public async Task<IEnumerable<ProductMaterialsConsumptionOutput>> GetProductMaterialsConsumptionService([FromRoute] int productId)
        {
            return await _productMaterialsConsumptionService.GetProductMaterialsConsumption(productId);
        }

        [HttpGet]
        [Route("materialsConsumption/fieldDataForMapping")]
        public CategoryNameModel GetCustomerFieldDataForMapping()
        {
            return _productMaterialsConsumptionService.GetCustomerFieldDataForMapping();
        }

        [HttpPost]
        [Route("{productId}/materialsConsumption/importFromMapping")]
        public async Task<bool> ImportMaterialsConsumptionFromMapping([FromRoute] int productId, [FromFormString] ImportExcelMapping mapping, IFormFile file)
        {
            if (file == null || mapping == null)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }
            mapping.FileName = file.FileName;
            return await _productMaterialsConsumptionService.ImportMaterialsConsumptionFromMapping(productId, mapping, file.OpenReadStream()).ConfigureAwait(true);
        }

        [HttpPost]
        [Route("{productId}/materialsConsumption/previewFromMapping")]
        public async Task<IList<MaterialsConsumptionByProduct>> ImportMaterialsConsumptionFromMappingAsPreviewData([FromRoute] int productId, [FromFormString] ImportExcelMapping mapping, IFormFile file)
        {
            if (file == null || mapping == null)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }
            mapping.FileName = file.FileName;

            return await _productMaterialsConsumptionService.ImportMaterialsConsumptionFromMappingAsPreviewData(productId, mapping, file.OpenReadStream()).ConfigureAwait(true);
        }

        [HttpPost]
        [Route("materialsConsumption/previewFromMapping")]
        public async Task<IList<MaterialsConsumptionByProduct>> ImportMaterialsConsumptionFromMappingAsPreviewData([FromFormString] ImportExcelMapping mapping, IFormFile file)
        {
            if (file == null || mapping == null)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }
            mapping.FileName = file.FileName;

            return await _productMaterialsConsumptionService.ImportMaterialsConsumptionFromMappingAsPreviewData(null, mapping, file.OpenReadStream()).ConfigureAwait(true);
        }

        [HttpPost]
        [Route("materialsConsumption/importFromMapping")]
        public async Task<bool> ImportMaterialsConsumptionFromMapping([FromFormString] ImportExcelMapping mapping, IFormFile file)
        {
            if (file == null || mapping == null)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }
            mapping.FileName = file.FileName;

            return await _productMaterialsConsumptionService.ImportMaterialsConsumptionFromMapping(null, mapping, file.OpenReadStream()).ConfigureAwait(true);
        }

        [HttpPost]
        [VErpAction(EnumActionType.View)]
        [Route("{productId}/materialsConsumption/exports")]
        public async Task<IActionResult> ExportProductMaterialsConsumption([FromRoute] int productId)
        {
            var (stream, fileName, contentType) = await _productMaterialsConsumptionService.ExportProductMaterialsConsumption(productId);

            return new FileStreamResult(stream, !string.IsNullOrWhiteSpace(contentType) ? contentType : "application/octet-stream") { FileDownloadName = fileName };
        }
        [HttpPost]
        [VErpAction(EnumActionType.View)]
        [Route("materialsConsumption/exports")]
        public async Task<IActionResult> ExportProductMaterialsConsumptions([FromBody] List<int> productIds, [FromQuery] bool isExportAllTopBOM)
        {
            var (stream, fileName, contentType) = await _productMaterialsConsumptionService.ExportProductMaterialsConsumptions(productIds, isExportAllTopBOM);

            return new FileStreamResult(stream, !string.IsNullOrWhiteSpace(contentType) ? contentType : "application/octet-stream") { FileDownloadName = fileName };
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
        public async Task<bool> UpdateProductMaterialsConsumptionGroup([FromRoute] int groupId)
        {
            return await _productMaterialsConsumptionGroupService.DeleteProductMaterialsConsumptionGroup(groupId);
        }
    }
}
