using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.MasterEnum.PO;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Enums.StockEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.Activity;
using VErp.Services.PurchaseOrder.Model;
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
        public async Task<ServiceResult<PageData<PurchasingRequestOutputList>>> GetList([FromQuery] string keyword, [FromQuery] IList<int> productIds, [FromQuery] EnumPurchasingRequestStatus? purchasingRequestStatusId, [FromQuery] EnumPoProcessStatus? poProcessStatusId, [FromQuery] bool? isApproved, [FromQuery] long? fromDate, [FromQuery] long? toDate, [FromQuery]string sortBy, [FromQuery] bool asc, [FromQuery] int page, [FromQuery] int size)
        {
            return await _purchasingRequestService.GetList(keyword, productIds, purchasingRequestStatusId, poProcessStatusId, isApproved, fromDate, toDate, sortBy, asc, page, size).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("GetListByProduct")]
        public async Task<ServiceResult<PageData<PurchasingRequestOutputListByProduct>>> GetListByProduct([FromQuery] string keyword, [FromQuery] IList<int> productIds, [FromQuery] EnumPurchasingRequestStatus? purchasingRequestStatusId, [FromQuery] EnumPoProcessStatus? poProcessStatusId, [FromQuery] bool? isApproved, [FromQuery] long? fromDate, [FromQuery] long? toDate, [FromQuery]string sortBy, [FromQuery] bool asc, [FromQuery] int page, [FromQuery] int size)
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
        public async Task<ServiceResult<PurchasingRequestOutput>> GetInfo([FromRoute] long purchasingRequestId)
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
        public async Task<ServiceResult<long>> Add([FromBody] PurchasingRequestInput req)
        {
            return await _purchasingRequestService.Create(req).ConfigureAwait(true);
        }

        /// <summary>
        /// Cập nhật phiếu yêu cầu vật tư
        /// </summary>
        /// <param name="purchasingRequestId"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{purchasingRequestId}")]
        public async Task<ServiceResult> Update([FromRoute] long purchasingRequestId, [FromBody] PurchasingRequestInput req)
        {
            return await _purchasingRequestService.Update(purchasingRequestId, req).ConfigureAwait(true);
        }

        /// <summary>
        /// Gửi duyệt phiếu yêu cầu vật tư
        /// </summary>
        /// <param name="purchasingRequestId">Id phiếu yêu cầu mua hàng</param>        
        /// <returns></returns>
        [HttpPut]
        [Route("{purchasingRequestId}/SendCensor")]
        public async Task<ServiceResult> SentToApprove([FromRoute] long purchasingRequestId)
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
        [VErpAction(EnumAction.Censor)]
        public async Task<ServiceResult> Approve([FromRoute] long purchasingRequestId)
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
        [VErpAction(EnumAction.Censor)]
        public async Task<ServiceResult> Reject([FromRoute] long purchasingRequestId)
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
        public async Task<ServiceResult> Delete([FromRoute] long purchasingRequestId)
        {
            return await _purchasingRequestService.Delete(purchasingRequestId).ConfigureAwait(true);
        }

        /// <summary>
        /// Cập nhật trạng thái mua hàng cho đơn yêu cầu vật tư
        /// </summary>
        /// <param name="purchasingRequestId"></param>
        /// <param name="poProcessStatusModel"></param>
        /// <returns></returns>

        [HttpPut]
        [Route("{purchasingRequestId}/UpdatePoProcessStatus")]
        public async Task<ServiceResult> UpdatePoProcessStatus([FromRoute] long purchasingRequestId, [FromBody] UpdatePoProcessStatusModel poProcessStatusModel)
        {
            if (poProcessStatusModel == null) return GeneralCode.InvalidParams;
            return await _purchasingRequestService.UpdatePoProcessStatus(purchasingRequestId, poProcessStatusModel.PoProcessStatusId).ConfigureAwait(true);
        }
    }
}
