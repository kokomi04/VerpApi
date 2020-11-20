using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.Enums.Manafacturing;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.Outsource.Order;
using VErp.Services.Manafacturing.Service.Outsource;

namespace VErpApi.Controllers.Manufacturing.Outsource
{
    [Route("api/manufacturing/outsourceOrder")]
    [ApiController]
    public class OutsourceOrderController : VErpBaseController
    {
        private readonly IOutsourceOrderService _outsourceOrderService;

        public OutsourceOrderController(IOutsourceOrderService outsourceOrderService)
        {
            _outsourceOrderService = outsourceOrderService;
        }

        [HttpPost]
        [Route("part")]
        public async Task<long> CreateOutsourceOrderPart([FromBody] OutsourceOrderInfo req)
        {
            return await _outsourceOrderService.CreateOutsourceOrderPart(req);
        }

        [HttpGet]
        [Route("part")]
        public async Task<PageData<OutsourceOrderPartDetailOutput>> GetListOutsourceOrderPart([FromQuery]string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _outsourceOrderService.GetListOutsourceOrderPart(keyword, page, size);
        }

    }
}
