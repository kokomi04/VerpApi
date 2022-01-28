using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SqlServer.Management.SqlParser.Metadata;
using VErp.Commons.Enums.Manafacturing;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Commons.Enums.MasterEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Manafacturing.Service.ProductionHandover;
using VErp.Services.Manafacturing.Model.ProductionHandover;

namespace VErpApi.Controllers.Manufacturing
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductionHistoryController : VErpBaseController
    {
        private readonly IProductionHistoryService _productionHistoryService;

        public ProductionHistoryController(IProductionHistoryService productionHistoryService)
        {
            _productionHistoryService = productionHistoryService;
        }

        [HttpGet]
        [Route("{productionOrderId}")]
        public async Task<IList<ProductionHistoryModel>> GetProductionHistories([FromRoute] long productionOrderId)
        {
            return await _productionHistoryService.GetProductionHistories(productionOrderId);
        }

        [HttpPost]
        [Route("{productionOrderId}")]
        public async Task<ProductionHistoryModel> CreateProductionHistory([FromRoute] long productionOrderId, [FromBody] ProductionHistoryInputModel data)
        {
            return await _productionHistoryService.CreateProductionHistory(productionOrderId, data);
        }

        [HttpPost]
        [Route("multiple/{productionOrderId}")]
        public async Task<IList<ProductionHistoryModel>> CreateMultipleProductionHistory([FromRoute] long productionOrderId, [FromBody] IList<ProductionHistoryInputModel> data)
        {
            return await _productionHistoryService.CreateMultipleProductionHistory(productionOrderId, data);
        }

        [HttpDelete]
        [Route("{productionHistoryId}")]
        public async Task<bool> DeleteProductionHistory([FromRoute] long productionHistoryId)
        {
            return await _productionHistoryService.DeleteProductionHistory(productionHistoryId);
        }
    }
}
