using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Manafacturing.Model.Outsource.RequestStep;
using VErp.Services.Manafacturing.Service.Outsource;

namespace VErpApi.Controllers.Manufacturing.Outsource
{
    [Route("api/manufacturing/outsourceStepRequest")]
    [ApiController]
    public class OutsourceStepRequestController : VErpBaseController
    {
        private readonly IOutsourceStepRequestService _outsourceStepRequestService;

        public OutsourceStepRequestController(IOutsourceStepRequestService outsourceStepRequestService)
        {
            _outsourceStepRequestService = outsourceStepRequestService;
        }

        [HttpGet]
        [Route("{outsourceStepRequestId}")]
        public async Task<OutsourceStepRequestInfo> GetRequestOutsourceStep([FromRoute] long outsourceStepRequestId)
        {
            return await _outsourceStepRequestService.GetOutsourceStepRequest(outsourceStepRequestId);
        }

        [HttpDelete]
        [Route("{outsourceStepRequestId}")]
        public async Task<bool> DeleteRequestOutsourceStep([FromRoute] long outsourceStepRequestId)
        {
            return await _outsourceStepRequestService.DeleteOutsourceStepRequest(outsourceStepRequestId);
        }

        [HttpPost]
        [Route("")]
        public async Task<long> CreateRequestOutsourceStep(OutsourceStepRequestInfo req)
        {
            return await _outsourceStepRequestService.CreateOutsourceStepRequest(req);
        }

        [HttpPut]
        [Route("{outsourceStepRequestId}")]
        public async Task<bool> UpdateRequestOutsourceStep([FromRoute]long outsourceStepRequestId, OutsourceStepRequestInfo req)
        {
            return await _outsourceStepRequestService.UpdateOutsourceStepRequest(outsourceStepRequestId, req);
        }
    }
}
