using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Stock.Model.Product.Calc;
using VErp.Services.Stock.Service.Products;

namespace VErpApi.Controllers.Stock.Products
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductPurityCalcController : VErpBaseController
    {
        private readonly IProductPurityCalcService _productPurityCalcService;
        public ProductPurityCalcController(IProductPurityCalcService productPurityCalcService)
        {
            _productPurityCalcService = productPurityCalcService;
        }

        [HttpGet]
        [Route("")]
        public Task<IList<ProductPurityCalcModel>> Get()
        {
            return _productPurityCalcService.GetList();
        }

        [HttpGet]
        [Route("{id}")]
        public Task<ProductPurityCalcModel> GetInfo([FromRoute] int id)
        {
            return _productPurityCalcService.GetInfo(id);
        }

        [HttpPut]
        [Route("{id}")]
        public Task<bool> Update([FromRoute] int id, [FromBody] ProductPurityCalcModel model)
        {
            return _productPurityCalcService.Update(id, model);
        }


        [HttpDelete]
        [Route("{id}")]
        public Task<bool> Delete([FromRoute] int id)
        {
            return _productPurityCalcService.Delete(id);
        }

        [HttpPost]
        [Route("{id}")]
        public Task<int> Create([FromBody] ProductPurityCalcModel model)
        {
            return _productPurityCalcService.Create(model);
        }

    }
}
