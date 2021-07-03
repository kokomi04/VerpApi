using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.ProductionProcessMold;
using VErp.Services.Manafacturing.Service.ProductionProcessMold;

namespace VErpApi.Controllers.Manufacturing
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductionProcessMoldController : VErpBaseController
    {
        private readonly IProductionProcessMoldService _productionProcessMoldService;

        public ProductionProcessMoldController(IProductionProcessMoldService productionProcessMoldService)
        {
            _productionProcessMoldService = productionProcessMoldService;
        }


        [Route("search")]
        [HttpPost]
        [VErpAction(EnumActionType.View)]
        public async Task<PageData<ProductionProcessMoldOutput>> Search([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size, [FromQuery] string orderByFieldName, [FromQuery] bool asc, [FromBody] Clause filters = null)
        {
            return await _productionProcessMoldService.Search(keyword, page, size, orderByFieldName, asc, filters);
        }

        [Route("{productionProcessMoldId}")]
        [HttpGet]
        public async Task<ICollection<ProductionStepMoldModel>> GetProductionProcessMold([FromRoute] long productionProcessMoldId)
        {
            return await _productionProcessMoldService.GetProductionProcessMold(productionProcessMoldId);
        }

        [Route("")]
        [HttpPost]
        public async Task<long> AddProductionProcessMold([FromBody] ProductionProcessMoldInput model)
        {
            return await _productionProcessMoldService.AddProductionProcessMold(model);
        }

        [Route("{productionProcessMoldId}")]
        [HttpPut]
        public async Task<bool> UpdateProductionProcessMold([FromRoute] long productionProcessMoldId, [FromBody] ProductionProcessMoldInput model)
        {
            return await _productionProcessMoldService.UpdateProductionProcessMold(productionProcessMoldId, model);
        }

        [Route("{productionProcessMoldId}")]
        [HttpDelete]
        public async Task<bool> DeleteProductionProcessMold([FromRoute] long productionProcessMoldId)
        {
            return await _productionProcessMoldService.DeleteProductionProcessMold(productionProcessMoldId);
        }

    }
}
