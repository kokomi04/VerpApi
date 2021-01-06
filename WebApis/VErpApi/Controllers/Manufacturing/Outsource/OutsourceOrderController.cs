using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.Enums.Manafacturing;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.Outsource.Order;
using VErp.Services.Manafacturing.Service.Outsource;

namespace VErpApi.Controllers.Manufacturing.Outsource
{
    [Route("api/manufacturing/outsourceOrder")]
    [ApiController]
    public class OutsourceOrderController : VErpBaseController
    {
        private readonly IOutsourcePartOrderService _outsourceOrderService;

        public OutsourceOrderController(IOutsourcePartOrderService outsourceOrderService)
        {
            _outsourceOrderService = outsourceOrderService;
        }

        [HttpPost]
        [Route("part")]
        public async Task<long> CreateOutsourceOrderPart([FromBody] OutsourceOrderInfo req)
        {
            return await _outsourceOrderService.CreateOutsourceOrderPart(req);
        }

        [HttpPost]
        [Route("part/search")]
        public async Task<PageData<OutsourcePartOrderDetailInfo>> GetListOutsourceOrderPart([FromQuery]string keyword, [FromQuery] int page, [FromQuery] int size, [FromBody] Clause filters)
        {
            return await _outsourceOrderService.GetListOutsourceOrderPart(keyword, page, size, filters);
        }

        [HttpDelete]
        [Route("part/{outsourceOrderId}")]
        public async Task<bool> DeleteOutsourceOrderPart([FromRoute] long outsourceOrderId)
        {
            return await _outsourceOrderService.DeleteOutsourceOrderPart(outsourceOrderId);
        }

        [HttpGet]
        [Route("part/{outsourceOrderId}")]
        public async Task<OutsourceOrderInfo> GetOutsourceOrderPart([FromRoute] long outsourceOrderId)
        {
            return await _outsourceOrderService.GetOutsourceOrderPart(outsourceOrderId);
        }

        [HttpPut]
        [Route("part/{outsourceOrderId}")]
        public async Task<bool> UpdateOutsourceOrderPart([FromRoute] long outsourceOrderId, [FromBody] OutsourceOrderInfo req)
        {
            return await _outsourceOrderService.UpdateOutsourceOrderPart(outsourceOrderId, req);
        }

    }
}
