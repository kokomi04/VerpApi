using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SqlServer.Management.SqlParser.Metadata;
using VErp.Services.Manafacturing.Model.ProductionStep;
using VErp.Commons.Enums.Manafacturing;
using VErp.Services.Manafacturing.Service.ProductionOrder;
using VErp.Services.Manafacturing.Model.ProductionOrder;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;

namespace VErpApi.Controllers.Manufacturing
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductionScheduleController : ControllerBase
    {
        private readonly IProductionScheduleService _productionScheduleService;

        public ProductionScheduleController(IProductionScheduleService productionScheduleService)
        {
            _productionScheduleService = productionScheduleService;
        }

        [HttpGet]
        [Route("")]
        public async Task<PageData<ProductionScheduleModel>> GetProductionSchedules([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size, [FromQuery] string orderByFieldName, [FromQuery] bool asc, [FromBody] Clause filters = null)
        {
            return await _productionScheduleService.GetProductionSchedule(keyword, page, size, orderByFieldName, asc, filters);
        }

        [HttpGet]
        [Route("plainingOrder")]
        public async Task<IList<ProductionPlaningOrderModel>> GetProductionPlaningOrderDetail()
        {
            return await _productionScheduleService.GetProductionPlaningOrders();
        }

        [HttpGet]
        [Route("plainingOrder/{productionOrderId}")]
        public async Task<IList<ProductionPlaningOrderDetailModel>> GetProductionPlaningOrderDetail([FromRoute]int productionOrderId)
        {
            return await _productionScheduleService.GetProductionPlaningOrderDetail(productionOrderId);
        }

     
    }
}
