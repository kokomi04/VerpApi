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

        [HttpPost]
        [Route("")]
        public async Task<long> CreateOutsourceOrder([FromBody] OutsourceOrderInfo req)
        {
            return await _outsourceOrderService.CreateOutsourceOrder(req);
        }
        

    }
}
