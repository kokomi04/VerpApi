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
        public async Task<PageData<OutsourceStepRequestSearch>> GetListOutsourceStepRequest([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size, [FromQuery] string orderByFieldName, [FromQuery] bool asc, [FromBody] Clause filters = null)
        {
            return await _outsourceStepRequestService.SearchOutsourceStepRequest(keyword, page, size, orderByFieldName, asc, filters);
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
        public async Task<long> CreateRequestOutsourceStep(OutsourceStepRequestModel req)
        {
            return await _outsourceStepRequestService.CreateOutsourceStepRequest(req);
        }

        [HttpPut]
        [Route("{outsourceStepRequestId}")]
        public async Task<bool> UpdateRequestOutsourceStep([FromRoute]long outsourceStepRequestId, OutsourceStepRequestModel req)
        {
            return await _outsourceStepRequestService.UpdateOutsourceStepRequest(outsourceStepRequestId, req);
        }

        [HttpGet]
        [Route("{outsourceStepRequestId}/outsourceStepRequestData")]
        public async Task<IList<OutsourceStepRequestDataInfo>> GetOutsourceStepRequestData([FromRoute]long outsourceStepRequestId)
        {
            return await _outsourceStepRequestService.GetOutsourceStepRequestData(outsourceStepRequestId);
        }

        [HttpGet]
        [Route("")]
        public async Task<IList<OutsourceStepRequestModel>> GetAllOutsourceStepRequest()
        {
            return await _outsourceStepRequestService.GetAllOutsourceStepRequest();
        }

        /// <summary>
        /// Lấy toàn bộ thông tin các công đoạn có YCGC
        /// </summary>
        /// <param name="productionOrderId">Mã ID lệnh sản xuất</param>
        /// <returns></returns>
        [HttpGet]
        [Route("listProductionStepOutsourced")]
        public async Task<IList<ProductionStepInOutsourceStepRequest>> GetProductionStepInOutsourceStepRequest([FromQuery]long productionOrderId)
        {
            return await _outsourceStepRequestService.GetProductionStepHadOutsourceStepRequest(productionOrderId);
        }

        [HttpPut]
        [Route("status")]
        public async Task<bool> UpdateOutsourceStepRequestStatus([FromBody]long[] outsourceStepRequestId)
        {
            return await _outsourceStepRequestService.UpdateOutsourceStepRequestStatus(outsourceStepRequestId);
        }

        [HttpGet]
        [Route("detail/byProductionOrder")]
        public async Task<IList<OutsourceStepRequestDataOutput>> GetOutsourceStepRequestDatasByProductionOrderId([FromQuery] long productionOrderId)
        {
            return await _outsourceStepRequestService.GetOutsourceStepRequestDatasByProductionOrderId(productionOrderId);
        }

    }
}
