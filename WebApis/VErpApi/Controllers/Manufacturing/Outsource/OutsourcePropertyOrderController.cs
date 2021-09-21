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
using VErp.Commons.GlobalObject;

namespace VErpApi.Controllers.Manufacturing.Outsource
{
    [Route("api/manufacturing/outsourcePropertyOrder")]
    [ApiController]
    public class OutsourcePropertyOrderController : VErpBaseController
    {
        private readonly IOutsourcePropertyService _outsourcePropertyOrderService;

        public OutsourcePropertyOrderController(IOutsourcePropertyService outsourcePropertyOrderService)
        {
            _outsourcePropertyOrderService = outsourcePropertyOrderService;
        }

        [HttpPost]
        [Route("")]
        public async Task<long> Create([FromBody] OutsourcePropertyOrderInput req)
        {
            return await _outsourcePropertyOrderService.Create(req);
        }

        [HttpGet]
        [Route("{outsourceOrderId}")]
        public async Task<OutsourcePropertyOrderInput> GetInfo([FromRoute] long outsourceOrderId)
        {
            return await _outsourcePropertyOrderService.Info(outsourceOrderId);
        }

        [HttpGet]
        [Route("PropertyCalc/{propertyCalcId}")]
        public async Task<OutsourcePropertyOrderInput> GetInfoByPropertyCalcId([FromRoute] long propertyCalcId)
        {
            return await _outsourcePropertyOrderService.GetInfoByPropertyCalcId(propertyCalcId);
        }


        [HttpPost]
        [Route("search")]
        [VErpAction(EnumActionType.View)]
        public async Task<PageData<OutsourcePropertyOrderList>> Search(
            [FromQuery] string keyword,
            [FromQuery] int page,
            [FromQuery] int size,
            [FromQuery] string orderByFieldName,
            [FromQuery] bool asc,
            [FromQuery] long fromDate,
            [FromQuery] long toDate,
            [FromBody] Clause filters)
        {
            return await _outsourcePropertyOrderService.GetList(keyword, page, size, orderByFieldName, asc, fromDate, toDate, filters);
        }

        [HttpPut]
        [Route("{outsourceOrderId}")]
        public async Task<bool> Update([FromRoute]long outsourceOrderId, [FromBody] OutsourcePropertyOrderInput req) {
            return await _outsourcePropertyOrderService.Update(outsourceOrderId, req);
        }

        [HttpDelete]
        [Route("{outsourceOrderId}")]
        public async Task<bool> DeleteOutsouceStepOrder([FromRoute] long outsourceOrderId) {
            return await _outsourcePropertyOrderService.Delete(outsourceOrderId);
        }
    }
}
