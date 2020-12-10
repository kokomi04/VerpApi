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
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Commons.Enums.MasterEnum;
using VErp.Infrastructure.ApiCore;

namespace VErpApi.Controllers.Manufacturing
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductionScheduleController : VErpBaseController
    {
        private readonly IProductionScheduleService _productionScheduleService;

        public ProductionScheduleController(IProductionScheduleService productionScheduleService)
        {
            _productionScheduleService = productionScheduleService;
        }

        [HttpGet]
        [Route("{scheduleTurnId}")]
        public async Task<IList<ProductionScheduleModel>> GetProductionSchedules([FromRoute] long scheduleTurnId)
        {
            return await _productionScheduleService.GetProductionSchedules(scheduleTurnId);
        }

        [HttpPost]
        [VErpAction(EnumAction.View)]
        [Route("Search")]
        public async Task<PageData<ProductionScheduleModel>> GetProductionSchedules([FromQuery] string keyword, [FromQuery] long fromDate, [FromQuery] long toDate, [FromQuery] int page, [FromQuery] int size, [FromQuery] string orderByFieldName, [FromQuery] bool asc, [FromBody] Clause filters = null)
        {
            return await _productionScheduleService.GetProductionSchedules(keyword, fromDate, toDate, page, size, orderByFieldName, asc, filters);
        }

        [HttpPost]
        [Route("")]
        public async Task<List<ProductionScheduleInputModel>> CreateProductionSchedule([FromBody] List<ProductionScheduleInputModel> data)
        {
            return await _productionScheduleService.CreateProductionSchedule(data);
        }

        [HttpPut]
        [Route("")]
        public async Task<List<ProductionScheduleInputModel>> UpdateProductionSchedule([FromBody] List<ProductionScheduleInputModel> data)
        {
            return await _productionScheduleService.UpdateProductionSchedule(data);
        }

        [HttpPut]
        [Route("{scheduleTurnId}/status/{status}")]
        public async Task<bool> UpdateProductionScheduleStatus([FromRoute] long scheduleTurnId, [FromRoute] EnumScheduleStatus status)
        {
            return await _productionScheduleService.UpdateProductionScheduleStatus(scheduleTurnId, status);
        }

        [HttpDelete]
        [Route("")]
        public async Task<bool> DeleteProductionSchedule([FromBody] long[] productionScheduleIds)
        {
            return await _productionScheduleService.DeleteProductionSchedule(productionScheduleIds);
        }

        [HttpGet]
        [Route("planningOrder")]
        public async Task<IList<ProductionPlanningOrderModel>> GetProductionPlanningOrderDetail()
        {
            return await _productionScheduleService.GetProductionPlanningOrders();
        }

        [HttpGet]
        [Route("planningOrder/{productionOrderId}")]
        public async Task<IList<ProductionPlanningOrderDetailModel>> GetProductionPlanningOrderDetail([FromRoute]int productionOrderId)
        {
            return await _productionScheduleService.GetProductionPlanningOrderDetail(productionOrderId);
        }


    }
}
