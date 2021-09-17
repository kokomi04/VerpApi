using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Manafacturing.Model.Outsource.RequestStep;
using VErp.Services.Manafacturing.Model.ProductionStep;
using VErp.Services.Manafacturing.Service.Outsource;
using VErp.Commons.GlobalObject;

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

        [HttpPost]
        [Route("search")]
        public async Task<PageData<OutsourceStepRequestSearch>> GetListOutsourceStepRequest(
            [FromQuery] string keyword,
            [FromQuery] int page,
            [FromQuery] int size,
            [FromQuery] string orderByFieldName,
            [FromQuery] bool asc,
            [FromQuery] long fromDate,
            [FromQuery] long toDate,
            [FromBody] Clause filters = null)
        {
            return await _outsourceStepRequestService.SearchOutsourceStepRequest(keyword, page, size, orderByFieldName, asc, fromDate, toDate, filters);
        }

        [HttpGet]
        [Route("{outsourceStepRequestId}")]
        public async Task<OutsourceStepRequestOutput> GetRequestOutsourceStep([FromRoute] long outsourceStepRequestId)
        {
            return await _outsourceStepRequestService.GetOutsourceStepRequestOutput(outsourceStepRequestId);
        }

        [HttpDelete]
        [Route("{outsourceStepRequestId}")]
        public async Task<bool> DeleteRequestOutsourceStep([FromRoute] long outsourceStepRequestId)
        {
            return await _outsourceStepRequestService.DeleteOutsourceStepRequest(outsourceStepRequestId);
        }

        [HttpPost]
        [Route("")]
        public async Task<OutsourceStepRequestPrivateKey> CreateRequestOutsourceStep(OutsourceStepRequestInput req)
        {
            return await _outsourceStepRequestService.AddOutsourceStepRequest(req);
        }

        [HttpPut]
        [Route("{outsourceStepRequestId}")]
        public async Task<bool> UpdateRequestOutsourceStep([FromRoute]long outsourceStepRequestId, OutsourceStepRequestInput req)
        {
            return await _outsourceStepRequestService.UpdateOutsourceStepRequest(outsourceStepRequestId, req);
        }

        [HttpGet]
        [Route("{outsourceStepRequestId}/outsourceStepRequestData")]
        public async Task<IList<OutsourceStepRequestDataExtraInfo>> GetOutsourceStepRequestData([FromRoute]long outsourceStepRequestId)
        {
            return await _outsourceStepRequestService.GetOutsourceStepRequestData(outsourceStepRequestId);
        }

        [HttpGet]
        [Route("")]
        public async Task<IList<OutsourceStepRequestModel>> GetAllOutsourceStepRequest()
        {
            return await _outsourceStepRequestService.GetAllOutsourceStepRequest();
        }

        [HttpPut]
        [Route("status")]
        public async Task<bool> UpdateOutsourceStepRequestStatus([FromBody]long[] outsourceStepRequestId)
        {
            return await _outsourceStepRequestService.UpdateOutsourceStepRequestStatus(outsourceStepRequestId);
        }

        [HttpGet]
        [Route("detail/byProductionOrder")]
        public async Task<IList<OutsourceStepRequestDetailOutput>> GetOutsourceStepRequestDatasByProductionOrderId([FromQuery] long productionOrderId)
        {
            return await _outsourceStepRequestService.GetOutsourceStepRequestDatasByProductionOrderId(productionOrderId);
        }

        [HttpGet]
        [Route("{outsourceStepRequestId}/materialsConsumption")]
        public async Task<IList<OutsourceStepRequestMaterialsConsumption>> GetOutsourceStepMaterialsConsumption([FromRoute] long outsourceStepRequestId)
        {
            return await _outsourceStepRequestService.GetOutsourceStepMaterialsConsumption(outsourceStepRequestId);
        }
    }
}
