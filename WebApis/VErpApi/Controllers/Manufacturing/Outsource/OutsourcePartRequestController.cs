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

        public OutsourcePartRequestController(IOutsourcePartRequestService requestOutsourcePartService)
        {
            _outsourcePartRequestService = requestOutsourcePartService;
        }

        [HttpPost]
        [Route("search")]
        public async Task<PageData<RequestOutsourcePartDetailInfo>> GetListRequestPart([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size,[FromBody] Clause filters = null)
        {
            return await _outsourcePartRequestService.GetListOutsourcePartRequest(keyword, page, size, filters);
        }

        [HttpGet]
        [Route("{outsourcePartRequestId}")]
        public async Task<RequestOutsourcePartInfo> GetRequestOutsourcePartExtraInfo([FromRoute] int outsourcePartRequestId)
        {
            return await _outsourcePartRequestService.GetOutsourcePartRequestExtraInfo(outsourcePartRequestId);
        }

        [HttpPost]
        [Route("")]
        public async Task<long> CreateRequestOutsourcePart([FromBody] RequestOutsourcePartInfo req)
        {
            return await _outsourcePartRequestService.CreateOutsourcePartRequest(req);
        }

        [HttpPut]
        [Route("{outsourcePartRequestId}")]
        public async Task<bool> UpdateRequestOutsourcePart([FromRoute] int outsourcePartRequestId, [FromBody] RequestOutsourcePartInfo req)
        {
            return await _outsourcePartRequestService.UpdateOutsourcePartRequest(outsourcePartRequestId, req);
        }

        [HttpDelete]
        [Route("{outsourcePartRequestId}")]
        public async Task<bool> DeletedRequestOutsourcePart([FromRoute] int outsourcePartRequestId) {
            return await _outsourcePartRequestService.DeletedOutsourcePartRequest(outsourcePartRequestId);
        }
    }
}
