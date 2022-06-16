using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.MasterEnum.PO;
using VErp.Commons.Enums.PO;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ApiCore.ModelBinders;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.PurchaseOrder.Model;
using VErp.Services.PurchaseOrder.Model.PoProviderPricing;
using VErp.Services.PurchaseOrder.Service.Po;

namespace VErpApi.Controllers.PurchaseOrder
{
    [Route("api/PurchasingOrder/PoProviderPricing")]
    public class PoProviderPricingController : VErpBaseController
    {

        private readonly IPoProviderPricingService _poProviderPricingService;
        public PoProviderPricingController(IPoProviderPricingService poProviderPricingService)
        {
            _poProviderPricingService = poProviderPricingService;

        }

        [HttpGet]
        [Route("GetList")]
        public async Task<PageData<PoProviderPricingOutputList>> GetList([FromQuery] string keyword, [FromQuery] int? customerId, [FromQuery] IList<int> productIds, [FromQuery] EnumPoProviderPricingStatus? poProviderPricingStatusId, [FromQuery] EnumPoProcessStatus? poProcessStatusId, [FromQuery] bool? isChecked, [FromQuery] bool? isApproved, [FromQuery] long? fromDate, [FromQuery] long? toDate, [FromQuery] string sortBy, [FromQuery] bool asc, [FromQuery] int page, [FromQuery] int size)
        {
            return await _poProviderPricingService
                .GetList(keyword, customerId, productIds, poProviderPricingStatusId, poProcessStatusId, isChecked, isApproved, fromDate, toDate, sortBy, asc, page, size)
                .ConfigureAwait(true);
        }


        [HttpGet]
        [Route("GetListByProduct")]
        public async Task<PageData<PoProviderPricingOutputListByProduct>> GetListByProduct([FromQuery] string keyword, [FromQuery] int? customerId, [FromQuery] IList<int> productIds, [FromQuery] EnumPoProviderPricingStatus? poProviderPricingStatusId, [FromQuery] EnumPoProcessStatus? poProcessStatusId, [FromQuery] bool? isChecked, [FromQuery] bool? isApproved, [FromQuery] long? fromDate, [FromQuery] long? toDate, [FromQuery] string sortBy, [FromQuery] bool asc, [FromQuery] int page, [FromQuery] int size)
        {
            return await _poProviderPricingService
                .GetListByProduct(keyword, customerId, null, productIds, poProviderPricingStatusId, poProcessStatusId, isChecked, isApproved, fromDate, toDate, sortBy, asc, page, size)
                .ConfigureAwait(true);
        }

        [HttpPost("GetRowsByCodes")]
        [VErpAction(EnumActionType.View)]
        [GlobalApi]
        public async Task<IList<PoProviderPricingOutputListByProduct>> GetRowsByCodes([FromBody] IList<string> codes)
        {
            var data = await _poProviderPricingService.GetListByProduct(string.Empty, null, codes, null, null, null, null, null, null, null, string.Empty, false, 1, 0);
            return data.List;
        }

        [HttpGet]
        [Route("{poProviderPricingId}")]
        public async Task<PoProviderPricingModel> GetInfo([FromRoute] long poProviderPricingId)
        {
            return await _poProviderPricingService
                .GetInfo(poProviderPricingId)
                .ConfigureAwait(true);
        }

        [HttpPost]
        [Route("")]
        public async Task<long> Create([FromBody] PoProviderPricingModel req)
        {
            return await _poProviderPricingService
                .Create(req)
                .ConfigureAwait(true);
        }




        [HttpPut]
        [Route("{poProviderPricingId}")]
        public async Task<bool> Update([FromRoute] long poProviderPricingId, [FromBody] PoProviderPricingModel req)
        {
            return await _poProviderPricingService
                .Update(poProviderPricingId, req)
                .ConfigureAwait(true);
        }


        [HttpPut]
        [Route("{poProviderPricingId}/SendCensor")]
        public async Task<bool> SentToCensor([FromRoute] long poProviderPricingId)
        {
            return await _poProviderPricingService
                .SentToCensor(poProviderPricingId)
                .ConfigureAwait(true);
        }


        [HttpPut]
        [Route("{poProviderPricingId}/Check")]
        [VErpAction(EnumActionType.Check)]
        public async Task<bool> Checked([FromRoute] long poProviderPricingId)
        {
            return await _poProviderPricingService
                 .Checked(poProviderPricingId)
                 .ConfigureAwait(true);
        }


        [HttpPut]
        [Route("{poProviderPricingId}/RejectCheck")]
        [VErpAction(EnumActionType.Check)]
        public async Task<bool> RejectCheck([FromRoute] long poProviderPricingId)
        {
            return await _poProviderPricingService
                .RejectCheck(poProviderPricingId)
                .ConfigureAwait(true);
        }

        [HttpPut]
        [Route("{poProviderPricingId}/Approve")]
        [VErpAction(EnumActionType.Censor)]
        public async Task<bool> Approve([FromRoute] long poProviderPricingId)
        {
            return await _poProviderPricingService
                 .Approve(poProviderPricingId)
                 .ConfigureAwait(true);
        }


        [HttpPut]
        [Route("{poProviderPricingId}/Reject")]
        [VErpAction(EnumActionType.Censor)]
        public async Task<bool> Reject([FromRoute] long poProviderPricingId)
        {
            return await _poProviderPricingService
                .Reject(poProviderPricingId)
                .ConfigureAwait(true);
        }


        [HttpDelete]
        [Route("{poProviderPricingId}")]
        public async Task<bool> Delete([FromRoute] long poProviderPricingId)
        {
            return await _poProviderPricingService
                .Delete(poProviderPricingId)
                .ConfigureAwait(true);
        }

        [HttpPut]
        [Route("{poProviderPricingId}/UpdatePoProcessStatus")]
        public async Task<bool> UpdatePoProcessStatus([FromRoute] long poProviderPricingId, [FromBody] UpdatePoProcessStatusModel poProcessStatusModel)
        {
            if (poProcessStatusModel == null) throw new BadRequestException(GeneralCode.InvalidParams);

            return await _poProviderPricingService
                .UpdatePoProcessStatus(poProviderPricingId, poProcessStatusModel.PoProcessStatusId)
                .ConfigureAwait(true);
        }


        [HttpGet]
        [Route("fieldDataForMapping")]
        public CategoryNameModel GetFieldDataForMapping()
        {
            return _poProviderPricingService.GetFieldDataForMapping();
        }

        [HttpPost]
        [Route("parseDetailsFromExcelMapping")]
        public IAsyncEnumerable<PoProviderPricingOutputDetail> ImportFromMapping([FromFormString] ImportExcelMapping mapping, IFormFile file)
        {
            if (file == null || mapping == null)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }
            mapping.FileName = file.FileName;
            return _poProviderPricingService.ParseInvoiceDetails(mapping, file.OpenReadStream());
        }
    }
}
