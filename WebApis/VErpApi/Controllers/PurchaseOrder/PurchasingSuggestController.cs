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
    [Route("api/PurchaseOrder/Suggest")]
    public class PurchasingSuggestController : VErpBaseController
    {
        private readonly IPurchasingRequestService _purchasingRequestService;

        public PurchasingSuggestController(IPurchasingRequestService purchasingRequestService)
        {
            _purchasingRequestService = purchasingRequestService;
        }

        /// <summary>
        /// Lấy danh sách phiếu đề nghị mua hàng
        /// </summary>     
        /// <returns></returns>
        [HttpGet]
        [Route("GetList")]
        public async Task<ApiResponse<PageData<PurchasingRequestOutputList>>> GetList([FromQuery] string keyword, [FromQuery] EnumPurchasingRequestStatus? purchasingRequestStatusId, [FromQuery] EnumPoProcessStatus? poProcessStatusId, [FromQuery] bool? isApproved, [FromQuery] long? fromDate, [FromQuery] long? toDate, [FromQuery]string sortBy, [FromQuery] bool asc, [FromQuery] int page, [FromQuery] int size)
        {
            return await _purchasingRequestService.GetList(keyword, purchasingRequestStatusId, poProcessStatusId, isApproved, fromDate, toDate, sortBy, asc, page, size);
        }

        /// <summary>
        /// Lấy thông tin phiếu đề nghị mua hàng
        /// </summary>
        /// <param name="purchasingRequestId">Id phiếu</param>
        /// <returns>PurchasingRequestOutputModel</returns>
        [HttpGet]
        [Route("{purchasingRequestId}")]
        public async Task<ApiResponse<PurchasingRequestOutput>> GetInfo([FromRoute] long purchasingRequestId)
        {
            return await _purchasingRequestService.GetInfo(purchasingRequestId);
        }

        /// <summary>
        /// Thêm mới phiếu đề nghị mua hàng
        /// </summary>
        /// <param name="req">Model PurchasingRequestInputModel</param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public async Task<ApiResponse<long>> Add([FromBody] PurchasingRequestInput req)
        {
            return await _purchasingRequestService.Create(req);
        }

        /// <summary>
        /// Cập nhật phiếu đề nghịmua hàng
        /// </summary>
        /// <param name="purchasingRequestId">Id phiếu</param>
        /// <param name="req">Model PurchasingRequestInputModel</param>
        /// <returns></returns>
        [HttpPut]
        [Route("{purchasingRequestId}")]
        public async Task<ApiResponse> Update([FromRoute] long purchasingRequestId, [FromBody] PurchasingRequestInput req)
        {
            return await _purchasingRequestService.Update(purchasingRequestId, req);
        }

        /// <summary>
        /// Gửi duyệt phiếu đề nghị mua hàng
        /// </summary>
        /// <param name="purchasingRequestId">Id phiếu yêu cầu mua hàng</param>        
        /// <returns></returns>
        [HttpPut]
        [Route("{purchasingRequestId}/SendCensor")]
        public async Task<ApiResponse> SentToApprove([FromRoute] long purchasingRequestId)
        {
            return await _purchasingRequestService.SendToCensor(purchasingRequestId);
        }

        /// <summary>
        /// Duyệt phiếu đề nghị mua hàng
        /// </summary>
        /// <param name="purchasingRequestId">Id phiếu yêu cầu mua hàng</param>        
        /// <returns></returns>
        [HttpPut]
        [Route("{purchasingRequestId}/Approve")]
        [VErpAction(EnumAction.Censor)]
        public async Task<ApiResponse> Approve([FromRoute] long purchasingRequestId)
        {
            return await _purchasingRequestService.Approve(purchasingRequestId);
        }

        /// <summary>
        ///  Từ chối phiếu đề nghị mua hàng
        /// </summary>
        /// <param name="purchasingRequestId"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{purchasingRequestId}/Reject")]
        [VErpAction(EnumAction.Censor)]
        public async Task<ApiResponse> Reject([FromRoute] long purchasingRequestId)
        {
            return await _purchasingRequestService.Reject(purchasingRequestId);
        }

        /// <summary>
        /// Xóa phiếu đề nghị mua hàng
        /// </summary>
        /// <param name="purchasingRequestId"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{purchasingRequestId}")]
        public async Task<ApiResponse> Delete([FromRoute] long purchasingRequestId)
        {
            return await _purchasingRequestService.Delete(purchasingRequestId);
        }

        /// <summary>
        /// Cập nhật trạng thái mua hàng cho phiếu đề nghị mua hàng
        /// </summary>
        /// <param name="purchasingRequestId"></param>
        /// <param name="poProcessStatusId"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{purchasingRequestId}/UpdatePoProcessStatus")]
        public async Task<ApiResponse> UpdatePoProcessStatus([FromRoute] long purchasingRequestId, [FromBody] EnumPoProcessStatus poProcessStatusId)
        {
            return await _purchasingRequestService.UpdatePoProcessStatus(purchasingRequestId, poProcessStatusId);
        }
    }
}
