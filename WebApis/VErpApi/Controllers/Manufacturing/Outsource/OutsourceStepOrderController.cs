using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
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
    }
}
