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
using VErp.Services.Manafacturing.Model.Outsource.Order;
using VErp.Services.Manafacturing.Model.Outsource.Track;
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
        public async Task<long> CreateOutsourceStepOrderPart([FromBody] OutsourceStepOrderInput req)
        {
            return await _outsourceStepOrderService.CreateOutsourceStepOrder(req);
        }

        [HttpGet]
        [Route("{outsourceStepOrderId}")]
        public async Task<OutsourceStepOrderOutput> GetOutsourceStepOrderPart([FromRoute] long outsourceStepOrderId)
        {
            return await _outsourceStepOrderService.GetOutsourceStepOrder(outsourceStepOrderId);
        }

        [HttpPost]
        [Route("search")]
        [VErpAction(EnumActionType.View)]
        public async Task<PageData<OutsourceStepOrderSeach>> SearchOutsourceStepOrder([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size, [FromQuery] string orderByFieldName, [FromQuery] bool asc, [FromBody] Clause filters)
        {
            return await _outsourceStepOrderService.SearchOutsourceStepOrder(keyword, page, size, orderByFieldName, asc, filters);
        }

        [HttpPut]
        [Route("{outsourceStepOrderId}")]
        public async Task<bool> UpdateOutsourceStepOrder([FromRoute]long outsourceStepOrderId,[FromBody] OutsourceStepOrderOutput req) {
            return await _outsourceStepOrderService.UpdateOutsourceStepOrder(outsourceStepOrderId, req);
        }

        [HttpDelete]
        [Route("{outsourceStepOrderId}")]
        public async Task<bool> DeleteOutsouceStepOrder([FromRoute] long outsourceStepOrderId) {
            return await _outsourceStepOrderService.DeleteOutsouceStepOrder(outsourceStepOrderId);
        }
    }
}
