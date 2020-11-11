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
        public async Task<PageData<RequestOutsourcePartInfo>> GetListRequestPart([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _requestPartService.GetListRequestOutsourcePart(keyword, page, size);
        }
        [HttpGet]
        [Route("parts/{productionOrderDetailId}")]
        public async Task<IList<RequestOutsourcePartInfo>> GetRequestOutsourcePartExtraInfo([FromRoute] int productionOrderDetailId)
        {
            return await _requestPartService.GetRequestOutsourcePartExtraInfo(productionOrderDetailId);
        }
        [HttpPost]
        [Route("parts")]
        public async Task<bool> CreateRequestOutsourcePart([FromBody] List<RequestOutsourcePartModel> req)
        {
            return await _requestPartService.CreateRequestOutsourcePart(req);
        }
        [HttpPut]
        [Route("parts/{productionOrderDetailId}")]
        public async Task<bool> UpdateRequestOutsourcePart([FromRoute] int productionOrderDetailId, [FromBody] List<RequestOutsourcePartModel> req)
        {
            return await _requestPartService.UpdateRequestOutsourcePart(productionOrderDetailId, req);
        }

    }
}
