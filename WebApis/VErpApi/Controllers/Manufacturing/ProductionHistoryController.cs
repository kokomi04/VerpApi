using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Manafacturing.Model.ProductionHandover;
using VErp.Services.Manafacturing.Service.ProductionHandover;

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

        [HttpGet]
        [Route("actualWorkload")]
        public async Task<IDictionary<long, ActualWorkloadModel>> GetActualWorkload([FromQuery] long startDate, [FromQuery] long endDate)
        {
            return await _productionHistoryService.GetActualWorkloadByDate(startDate, endDate);
        }

        [HttpGet]
        [Route("completionActualWorkload")]
        public async Task<IDictionary<long, ActualWorkloadModel>> GetCompletionActualWorkload([FromQuery] long startDate, [FromQuery] long endDate)
        {
            return await _productionHistoryService.GetCompletionActualWorkload(startDate, endDate);
        }

        [HttpDelete]
        [Route("{productionHistoryId}")]
        public async Task<bool> DeleteProductionHistory([FromRoute] long productionHistoryId)
        {
            return await _productionHistoryService.DeleteProductionHistory(productionHistoryId);
        }
    }
}
