using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.Enums.Manafacturing;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.Outsource.Order;
using VErp.Services.Manafacturing.Service.Outsource;

namespace VErpApi.Controllers.Manufacturing.Outsource
{
    [Route("api/manufacturing/outsourceOrder")]
    [ApiController]
    public class OutsourceOrderController : ControllerBase
    {
        private readonly IOutsourceOrderService _outsourceOrderService;

        public OutsourceOrderController(IOutsourceOrderService outsourceOrderService)
        {
            _outsourceOrderService = outsourceOrderService;
        }

        [HttpGet]
        [Route("parts")]
        public async Task<PageData<OutsoureOrderInfo>> GetListOutsourcePartOrder([FromQuery] string keyWord, [FromQuery] int page, [FromQuery] int size)
        {
            return await _outsourceOrderService.GetListOutsourceOrder((int)EnumProductionProcess.OutsourceOrderRequestContainerType.OutsourcePart, keyWord, page, size);
        }

        [HttpGet]
        [Route("steps")]
        public async Task<PageData<OutsoureOrderInfo>> GetListOutsourceStepOrder([FromQuery] string keyWord, [FromQuery] int page, [FromQuery] int size)
        {
            return await _outsourceOrderService.GetListOutsourceOrder((int)EnumProductionProcess.OutsourceOrderRequestContainerType.OutsourceStep, keyWord, page, size);
        }

        [HttpPost]
        [Route("")]
        public async Task<int> CreateOutsourceOrder([FromBody] OutsoureOrderInfo req)
        {
            return await _outsourceOrderService.CreateOutsourceOrder(req);
        }

        [HttpPut]
        [Route("{outsourceOrderId}")]
        public async Task<bool> UpdateOutsourceOrder([FromRoute] int outsourceOrderId, [FromBody] OutsoureOrderInfo req)
        {
            return await _outsourceOrderService.UpdateOutsourceOrder(outsourceOrderId, req);
        }

        [HttpDelete]
        [Route("{outsourceOrderId}")]
        public async Task<bool> DeleteOutsourceOrder([FromRoute] int outsourceOrderId)
        {
            return await _outsourceOrderService.DeleteOutsourceOrder(outsourceOrderId);       
        }

    }
}
