using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.MasterEnum.PO;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Enums.StockEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.ApiCore.ModelBinders;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.Activity;
using VErp.Services.PurchaseOrder.Model;
using VErp.Services.PurchaseOrder.Model.Request;
using VErp.Services.PurchaseOrder.Service;

namespace VErpApi.Controllers.PurchaseOrder
{
    [Route("api/PurchaseOrder/Request")]
    public class PurchasingRequestController : VErpBaseController
    {
        private readonly IPurchasingRequestService _purchasingRequestService;

        public PurchasingRequestController(IPurchasingRequestService purchasingRequestService)
        {
            _purchasingRequestService = purchasingRequestService;
        }

        /// <summary>
        /// Lấy danh sách phiếu yêu cầu vật tư
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="purchasingRequestStatusId"></param>
        /// <param name="poProcessStatusId"></param>
        /// <param name="isApproved"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="sortBy"></param>
        /// <param name="asc"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetList")]
        public async Task<PageData<PurchasingRequestOutputList>> GetList([FromQuery] string keyword, [FromQuery] IList<int> productIds, [FromQuery] EnumPurchasingRequestStatus? purchasingRequestStatusId, [FromQuery] EnumPoProcessStatus? poProcessStatusId, [FromQuery] bool? isApproved, [FromQuery] long? fromDate, [FromQuery] long? toDate, [FromQuery] string sortBy, [FromQuery] bool asc, [FromQuery] int page, [FromQuery] int size)
        {
            return await _purchasingRequestService.GetList(keyword, productIds, purchasingRequestStatusId, poProcessStatusId, isApproved, fromDate, toDate, sortBy, asc, page, size).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("GetListByProduct")]
        public async Task<PageData<PurchasingRequestOutputListByProduct>> GetListByProduct([FromQuery] string keyword, [FromQuery] IList<int> productIds, [FromQuery] EnumPurchasingRequestStatus? purchasingRequestStatusId, [FromQuery] EnumPoProcessStatus? poProcessStatusId, [FromQuery] bool? isApproved, [FromQuery] long? fromDate, [FromQuery] long? toDate, [FromQuery] string sortBy, [FromQuery] bool asc, [FromQuery] int page, [FromQuery] int size)
        {
            return await _purchasingRequestService.GetListByProduct(keyword, productIds, purchasingRequestStatusId, poProcessStatusId, isApproved, fromDate, toDate, sortBy, asc, page, size).ConfigureAwait(true);
        }

        /// <summary>
        /// Lấy thông tin phiếu yêu cầu vật tư
        /// </summary>
        /// <param name="purchasingRequestId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{purchasingRequestId}")]
        public async Task<PurchasingRequestOutput> GetInfo([FromRoute] long purchasingRequestId)
        {
            return await _purchasingRequestService.GetInfo(purchasingRequestId).ConfigureAwait(true);
        }

        /// <summary>
        /// Thêm mới phiếu yêu cầu vật tư
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public async Task<long> Add([FromBody] PurchasingRequestInput req)
        {
            return await _purchasingRequestService.Create(EnumPurchasingRequestType.Normal, req).ConfigureAwait(true);
        }


       

        [HttpGet]
        [Route("OrderDetail/{orderDetailId}")]
        public async Task<PurchasingRequestOutput> GetFromOrderMaterial([FromRoute] long orderDetailId)
        {
            return await _purchasingRequestService.GetByOrderDetailId(orderDetailId).ConfigureAwait(true);
        }

        [HttpPost]
        [Route("OrderDetail/{orderDetailId}")]
        public async Task<long> CreateFromOrderMaterial([FromRoute] long orderDetailId, [FromBody] PurchasingRequestInput req)
        {
            if (req == null) throw new BadRequestException(GeneralCode.InvalidParams);
            req.OrderDetailId = orderDetailId;
            return await _purchasingRequestService.Create(EnumPurchasingRequestType.OrderMaterial, req).ConfigureAwait(true);
        }

        [HttpPut]
        [Route("OrderDetail/{orderDetailId}/{purchasingRequestId}")]
        public async Task<bool> UpdateFromOrderMaterial([FromRoute] long orderDetailId, [FromRoute] long purchasingRequestId, [FromBody] PurchasingRequestInput req)
        {
            if (req == null) throw new BadRequestException(GeneralCode.InvalidParams);
            req.OrderDetailId = orderDetailId;
            return await _purchasingRequestService.Update(EnumPurchasingRequestType.OrderMaterial, purchasingRequestId, req).ConfigureAwait(true);
        }

        [HttpDelete]
        [Route("OrderDetail/{orderDetailId}/{purchasingRequestId}")]
        public async Task<bool> DeleteFromOrderMaterial([FromRoute] long orderDetailId, [FromRoute] long purchasingRequestId)
        {
            return await _purchasingRequestService.Delete(orderDetailId, purchasingRequestId).ConfigureAwait(true);
        }


        [HttpPost]
        [Route("parseDetailsFromExcelMapping")]
        public IAsyncEnumerable<PurchasingRequestInputDetail> parseDetailsFromExcelMapping([FromFormString] SingleInvoicePurchasingRequestExcelMappingModel mapping, [FromForm] IFormFile file)
        {
            if (file == null)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }
            return _purchasingRequestService.ParseInvoiceDetails(mapping, file.OpenReadStream());
        }

        /// <summary>
        /// Cập nhật phiếu yêu cầu vật tư
        /// </summary>
        /// <param name="purchasingRequestId"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{purchasingRequestId}")]
        public async Task<bool> Update([FromRoute] long purchasingRequestId, [FromBody] PurchasingRequestInput req)
        {
            return await _purchasingRequestService.Update(EnumPurchasingRequestType.Normal, purchasingRequestId, req).ConfigureAwait(true);
        }

        /// <summary>
        /// Gửi duyệt phiếu yêu cầu vật tư
        /// </summary>
        /// <param name="purchasingRequestId">Id phiếu yêu cầu mua hàng</param>        
        /// <returns></returns>
        [HttpPut]
        [Route("{purchasingRequestId}/SendCensor")]
        public async Task<bool> SentToApprove([FromRoute] long purchasingRequestId)
        {
            return await _purchasingRequestService.SendToCensor(purchasingRequestId).ConfigureAwait(true);
        }

        /// <summary>
        /// Duyệt phiếu yêu cầu vật tư
        /// </summary>
        /// <param name="purchasingRequestId">Id phiếu yêu cầu mua hàng</param>        
        /// <returns></returns>
        [HttpPut]
        [Route("{purchasingRequestId}/Approve")]
        [VErpAction(EnumActionType.Censor)]
        public async Task<bool> Approve([FromRoute] long purchasingRequestId)
        {
            return await _purchasingRequestService.Approve(purchasingRequestId).ConfigureAwait(true);
        }

        /// <summary>
        ///  Từ chối phiếu yêu cầu vật tư
        /// </summary>
        /// <param name="purchasingRequestId"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{purchasingRequestId}/Reject")]
        [VErpAction(EnumActionType.Censor)]
        public async Task<bool> Reject([FromRoute] long purchasingRequestId)
        {
            return await _purchasingRequestService.Reject(purchasingRequestId).ConfigureAwait(true);
        }

        /// <summary>
        /// Xóa phiếu yêu cầu vật tư
        /// </summary>
        /// <param name="purchasingRequestId"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{purchasingRequestId}")]
        public async Task<bool> Delete([FromRoute] long purchasingRequestId)
        {
            return await _purchasingRequestService.Delete(null, purchasingRequestId).ConfigureAwait(true);
        }

        /// <summary>
        /// Cập nhật trạng thái mua hàng cho đơn yêu cầu vật tư
        /// </summary>
        /// <param name="purchasingRequestId"></param>
        /// <param name="poProcessStatusModel"></param>
        /// <returns></returns>

        [HttpPut]
        [Route("{purchasingRequestId}/UpdatePoProcessStatus")]
        public async Task<bool> UpdatePoProcessStatus([FromRoute] long purchasingRequestId, [FromBody] UpdatePoProcessStatusModel poProcessStatusModel)
        {
            if (poProcessStatusModel == null) throw new BadRequestException(GeneralCode.InvalidParams);
            return await _purchasingRequestService.UpdatePoProcessStatus(purchasingRequestId, poProcessStatusModel.PoProcessStatusId).ConfigureAwait(true);
        }

        /// <summary>
        /// Lấy thông tin phiếu yêu cầu vật tư theo LSX
        /// </summary>
        /// <param name="productionOrderId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public async Task<PurchasingRequestOutput> GetPurchasingRequestByProductionOrderId([FromQuery] long productionOrderId)
        {
            return await _purchasingRequestService.GetPurchasingRequestByProductionOrderId(productionOrderId);
        }
    }
}
