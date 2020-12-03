using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.Outsource.Order;
using VErp.Services.Manafacturing.Service.Outsource;

namespace VErpApi.Controllers.Manufacturing.Outsource
{
    [Route("api/manufacturing/outsourceStepOrder")]
    [ApiController]
    public class OutsourceStepOrderController : VErpBaseController
    {
        private readonly IOutsourceStepOrderService _outsourceStepOrderService;

        public OutsourceStepOrderController(IOutsourceStepOrderService outsourceStepOrderService)
        {
            _outsourceStepOrderService = outsourceStepOrderService;
        }

        [HttpPost]
        [Route("")]
        public async Task<long> CreateOutsourceStepOrderPart([FromBody] OutsourceStepOrderModel req)
        {
            return await _outsourceStepOrderService.CreateOutsourceStepOrderPart(req);
        }

        [HttpGet]
        [Route("{outsourceStepOrderId}")]
        public async Task<OutsourceStepOrderModel> GetOutsourceStepOrderPart([FromRoute] long outsourceStepOrderId)
        {
            return await _outsourceStepOrderService.GetOutsourceStepOrder(outsourceStepOrderId);
        }

        [HttpPost]
        [Route("search")]
        public async Task<PageData<OutsourceStepOrderSeach>> SearchOutsourceStepOrder(string keyword, int page, int size)
        {
            return await _outsourceStepOrderService.SearchOutsourceStepOrder(keyword, page, size);
        }

        [HttpPut]
        [Route("{outsourceStepOrderId}")]
        public async Task<bool> UpdateOutsourceStepOrder([FromRoute]long outsourceStepOrderId,[FromBody] OutsourceStepOrderModel req) {
            return await _outsourceStepOrderService.UpdateOutsourceStepOrder(outsourceStepOrderId, req);
        }

        [HttpDelete]
        [Route("{outsourceStepOrderId}")]
        public async Task<bool> DeleteOutsouceStepOrder([FromRoute] long outsourceStepOrderId) {
            return await _outsourceStepOrderService.DeleteOutsouceStepOrder(outsourceStepOrderId);
        }
    }
}
