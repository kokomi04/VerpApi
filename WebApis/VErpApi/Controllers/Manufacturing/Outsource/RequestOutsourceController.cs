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
    [Route("api/requestOutsource")]
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
        public async Task<PageData<RequestOutsourcePartModel>> GetListRequestPart([FromQuery] string keyWord, [FromQuery] int page, [FromQuery] int size)
        {
            return await _requestPartService.GetListRequest(keyWord, page, size);
        }
        [HttpGet]
        [Route("parts/{requestPartId}")]
        public async Task<RequestOutsourcePartModel> GetRequestPartById([FromRoute] int requestPartId)
        {
            return await _requestPartService.GetRequestById(requestPartId);
        }
        [HttpPost]
        [Route("parts")]
        public async Task<int> CreateRequestPart([FromBody] RequestOutsourcePartModel req)
        {
            return await _requestPartService.CreateRequest(req);
        }
        [HttpPut]
        [Route("parts/{requestPartId}")]
        public async Task<bool> UpdateRequestPart([FromRoute] int requestPartId, [FromBody] RequestOutsourcePartModel req)
        {
            return await _requestPartService.UpdateRequest(requestPartId, req);
        }
        [HttpGet]
        [Route("parts/{requestPartId}")]
        public async Task<bool> DeleteRequestPart([FromRoute] int requestPartId)
        {
            return await _requestPartService.DeleteRequest(requestPartId);
        }

    }
}
