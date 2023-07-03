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
using VErp.Services.Stock.Model.Product;
using VErp.Services.Stock.Model.Product.Bom;
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

        [HttpPost]
        [Route("ByProductIds")]
        [VErpAction(EnumActionType.View)]
        [GlobalApi]
        public async Task<IDictionary<int, IList<ProductBomOutput>>> ByProductIds([FromBody] IList<int> productIds)
        {
            return await _productBomService.GetBoms(productIds);
        }


        [HttpGet]
        [Route("{productId}")]
        [GlobalApi]
        public async Task<IList<ProductBomOutput>> GetBOM([FromRoute] int productId)
        {
            return await _productBomService.GetBom(productId);
        }

        [HttpPost]
        [Route("products")]
        public async Task<IList<ProductElementModel>> GetElements([FromBody] int[] productIds)
        {
            return await _productBomService.GetProductElements(productIds);
        }

        [HttpPut]
        [Route("{productId}")]
        public async Task<bool> Update([FromRoute] int productId, [FromBody] ProductBomModel model)
        {
            if (model == null) throw new BadRequestException(GeneralCode.InvalidParams);
            var updateModel = new ProductBomUpdateInfoModel()
            {
                BomInfo = new ProductBomUpdateInfo(model.ProductBoms),
                MaterialsInfo = new ProductBomMaterialUpdateInfo(model.ProductMaterials, model.IsCleanOldMaterial),
                PropertiesInfo = new ProductBomPropertyUpdateInfo(model.ProductProperties, model.IsCleanOldProperties),
                IgnoreStepInfo = new ProductBomIgnoreStepUpdateInfo(model.ProductIgnoreSteps, model.IsCleanOldIgnoreStep)
            };
            return await _productBomService.Update(productId, updateModel);
        }

        [HttpPost]
        [VErpAction(EnumActionType.View)]
        [Route("exports")]
        public async Task<IActionResult> Export([FromBody] IList<int> productIds, [FromQuery] bool isTopBOM)
        {
            if (productIds == null) throw new BadRequestException(GeneralCode.InvalidParams);
            var (stream, fileName, contentType) = await _productBomService.ExportBom(productIds, isTopBOM);

            return new FileStreamResult(stream, !string.IsNullOrWhiteSpace(contentType) ? contentType : "application/octet-stream") { FileDownloadName = fileName };
        }

        [HttpGet]
        [Route("fieldDataForMapping")]
        public async Task<CategoryNameModel> GetBomFieldDataForMapping()
        {
            return await _productBomService.GetBomFieldDataForMapping();
        }

        [HttpPost]
        [Route("importFromMapping")]
        public async Task<bool> ImportFromMapping([FromFormString] ImportExcelMapping mapping, IFormFile file)
        {
            if (file == null || mapping == null)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }

            mapping.FileName = file.FileName;
            return await _productBomService.ImportBomFromMapping(mapping, file.OpenReadStream()).ConfigureAwait(true);
        }

        [HttpPost]
        [Route("previewFromMapping")]
        public async Task<IList<ProductBomByProduct>> PreviewFromMapping([FromFormString] ImportExcelMapping mapping, IFormFile file)
        {
            if (file == null)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }

            return await _productBomService.PreviewBomFromMapping(mapping, file.OpenReadStream()).ConfigureAwait(true);
        }
    }
}
