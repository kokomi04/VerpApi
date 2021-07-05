using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.Outsource.RequestPart;
using VErp.Services.Manafacturing.Service.Outsource;

namespace VErpApi.Controllers.Manufacturing.Outsource
{
    [Route("api/manufacturing/outsourcePartRequest")]
    [ApiController]
    public class OutsourcePartRequestController : VErpBaseController
    {
        private readonly IOutsourcePartRequestService _outsourcePartRequestService;

        public OutsourcePartRequestController(IOutsourcePartRequestService outsourcePartRequestService)
        {
            _outsourcePartRequestService = outsourcePartRequestService;
        }

        [HttpPost]
        [Route("search")]
        public async Task<PageData<OutsourcePartRequestDetailInfo>> GetListRequestPart([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size,[FromBody] Clause filters = null)
        {
            return await _outsourcePartRequestService.GetListOutsourcePartRequest(keyword, page, size, filters);
        }

        [HttpGet]
        [Route("{outsourcePartRequestId}")]
        public async Task<OutsourcePartRequestInfo> GetRequestOutsourcePartExtraInfo([FromRoute] long outsourcePartRequestId)
        {
            return await _outsourcePartRequestService.GetOutsourcePartRequestExtraInfo(outsourcePartRequestId);
        }

        [HttpPost]
        [Route("")]
        public async Task<long> CreateRequestOutsourcePart([FromBody] OutsourcePartRequestInfo req)
        {
            return await _outsourcePartRequestService.CreateOutsourcePartRequest(req);
        }

        [HttpPut]
        [Route("{outsourcePartRequestId}")]
        public async Task<bool> UpdateRequestOutsourcePart([FromRoute] long outsourcePartRequestId, [FromBody] OutsourcePartRequestInfo req)
        {
            return await _outsourcePartRequestService.UpdateOutsourcePartRequest(outsourcePartRequestId, req);
        }

        [HttpDelete]
        [Route("{outsourcePartRequestId}")]
        public async Task<bool> DeletedRequestOutsourcePart([FromRoute] long outsourcePartRequestId) {
            return await _outsourcePartRequestService.DeletedOutsourcePartRequest(outsourcePartRequestId);
        }

        [HttpGet]
        [Route("detail/byProductionOrder")]
        public async Task<IList<OutsourcePartRequestDetailInfo>> GetOutsourcePartRequestDetailByProductionOrderId([FromQuery] long productionOrderId)
        {
            return await _outsourcePartRequestService.GetOutsourcePartRequestDetailByProductionOrderId(productionOrderId);
        }

        [HttpPut]
        [Route("status")]
        public async Task<bool> UpdateOutsourcePartRequestStatus([FromBody] long[] outsourcePartRequestId)
        {
            return await _outsourcePartRequestService.UpdateOutsourcePartRequestStatus(outsourcePartRequestId);
        }

        [HttpGet]
        [Route("byProductionOrder")]
        public async Task<IList<OutsourcePartRequestOutput>> GetOutsourcePartRequestByProductionOrderId([FromQuery] long productionOrderId)
        {
            return await _outsourcePartRequestService.GetOutsourcePartRequestByProductionOrderId(productionOrderId);
        }
    }
}
