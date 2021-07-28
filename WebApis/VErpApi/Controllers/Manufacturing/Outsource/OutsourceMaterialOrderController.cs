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
    [Route("api/manufacturing/outsourceMaterialOrder")]
    [ApiController]
    public class OutsourceMaterialOrderController : VErpBaseController
    {
        private readonly IOutsourceMaterialService _outsourceMaterialOrderService;

        public OutsourceMaterialOrderController(IOutsourceMaterialService outsourceMaterialOrderService)
        {
            _outsourceMaterialOrderService = outsourceMaterialOrderService;
        }

        [HttpPost]
        [Route("")]
        public async Task<long> Create([FromBody] OutsourceStepOrderInput req)
        {
            return await _outsourceMaterialOrderService.Create(req);
        }

        [HttpGet]
        [Route("{outsourceOrderId}")]
        public async Task<OutsourceStepOrderOutput> GetInfo([FromRoute] long outsourceOrderId)
        {
            return await _outsourceMaterialOrderService.Info(outsourceOrderId);
        }

        [HttpPost]
        [Route("search")]
        [VErpAction(EnumActionType.View)]
        public async Task<PageData<OutsourceMaterialOrderList>> Search(
            [FromQuery] string keyword,
            [FromQuery] int page,
            [FromQuery] int size,
            [FromQuery] string orderByFieldName,
            [FromQuery] bool asc,
            [FromQuery] long fromDate,
            [FromQuery] long toDate,
            [FromBody] Clause filters)
        {
            return await _outsourceMaterialOrderService.GetList(keyword, page, size, orderByFieldName, asc, fromDate, toDate, filters);
        }

        [HttpPut]
        [Route("{outsourceOrderId}")]
        public async Task<bool> Update([FromRoute]long outsourceOrderId, [FromBody] OutsourceStepOrderOutput req) {
            return await _outsourceMaterialOrderService.Update(outsourceOrderId, req);
        }

        [HttpDelete]
        [Route("{outsourceOrderId}")]
        public async Task<bool> DeleteOutsouceStepOrder([FromRoute] long outsourceOrderId) {
            return await _outsourceMaterialOrderService.Delete(outsourceOrderId);
        }
    }
}
