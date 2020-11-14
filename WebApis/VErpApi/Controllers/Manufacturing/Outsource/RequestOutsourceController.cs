using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.Outsource.RequestPart;
using VErp.Services.Manafacturing.Service.Outsource;

namespace VErpApi.Controllers.Manufacturing.Outsource
{
    [Route("api/manufacturing/requestOutsource")]
    [ApiController]
    public class RequestOutsourceController : ControllerBase
    {
        private readonly IRequestOutsourcePartService _requestPartService;

        public RequestOutsourceController(IRequestOutsourcePartService requestOutsourcePartService)
        {
            _requestPartService = requestOutsourcePartService;
        }

        [HttpGet]
        [Route("parts")]
        public async Task<PageData<RequestOutsourcePartDetailInfo>> GetListRequestPart([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _requestPartService.GetListRequestOutsourcePart(keyword, page, size);
        }
        [HttpGet]
        [Route("parts/{requestOutsourcePartId}")]
        public async Task<RequestOutsourcePartInfo> GetRequestOutsourcePartExtraInfo([FromRoute] int requestOutsourcePartId)
        {
            return await _requestPartService.GetRequestOutsourcePartExtraInfo(requestOutsourcePartId);
        }
        [HttpPost]
        [Route("parts")]
        public async Task<bool> CreateRequestOutsourcePart([FromBody] List<RequestOutsourcePartDetailModel> req)
        {
            return await _requestPartService.CreateRequestOutsourcePart(req);
        }
        [HttpPut]
        [Route("parts/{requestOutsourcePartId}")]
        public async Task<bool> UpdateRequestOutsourcePart([FromRoute] int requestOutsourcePartId, [FromBody] List<RequestOutsourcePartDetailModel> req)
        {
            return await _requestPartService.UpdateRequestOutsourcePart(requestOutsourcePartId, req);
        }

    }
}
