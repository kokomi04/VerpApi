using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.MasterEnum.PO;
using VErp.Commons.Enums.PO;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Enums.StockEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.ApiCore.ModelBinders;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.Activity;
using VErp.Services.PurchaseOrder.Model;
using VErp.Services.PurchaseOrder.Model.PoProviderPricing;
using VErp.Services.PurchaseOrder.Model.PurchaseOrder;
using VErp.Services.PurchaseOrder.Model.Request;
using VErp.Services.PurchaseOrder.Service;
using VErp.Services.PurchaseOrder.Service.Po;

namespace VErpApi.Controllers.PurchaseOrder
{
    [Route("api/PurchasingOrder/PoProvidePricing")]
    public class PoProviderPricingOrderController : VErpBaseController
    {

        private readonly IPoProviderPricingService _poProviderPricingService;
        public PoProviderPricingOrderController(IPoProviderPricingService poProviderPricingService)
        {
            _poProviderPricingService = poProviderPricingService;

        }

        [HttpGet]
        [Route("GetList")]
        public async Task<PageData<PoProviderPricingOutputList>> GetList([FromQuery] string keyword, [FromQuery] IList<int> productIds, [FromQuery] EnumPoProviderPricingStatus? purchaseOrderStatusId, [FromQuery] EnumPoProcessStatus? poProcessStatusId, [FromQuery] bool? isChecked, [FromQuery] bool? isApproved, [FromQuery] long? fromDate, [FromQuery] long? toDate, [FromQuery] string sortBy, [FromQuery] bool asc, [FromQuery] int page, [FromQuery] int size)
        {
            return await _poProviderPricingService
                .GetList(keyword, productIds, purchaseOrderStatusId, poProcessStatusId, isChecked, isApproved, fromDate, toDate, sortBy, asc, page, size)
                .ConfigureAwait(true);
        }


        [HttpGet]
        [Route("GetListByProduct")]
        public async Task<PageData<PoProviderPricingOutputListByProduct>> GetListByProduct([FromQuery] string keyword, [FromQuery] IList<int> productIds, [FromQuery] EnumPoProviderPricingStatus? purchaseOrderStatusId, [FromQuery] EnumPoProcessStatus? poProcessStatusId, [FromQuery] bool? isChecked, [FromQuery] bool? isApproved, [FromQuery] long? fromDate, [FromQuery] long? toDate, [FromQuery] string sortBy, [FromQuery] bool asc, [FromQuery] int page, [FromQuery] int size)
        {
            return await _poProviderPricingService
                .GetListByProduct(keyword, null, productIds, purchaseOrderStatusId, poProcessStatusId, isChecked, isApproved, fromDate, toDate, sortBy, asc, page, size)
                .ConfigureAwait(true);
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

    }
}
