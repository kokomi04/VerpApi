﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Enums.StockEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.PurchaseOrder.Service.PurchasingRequest;
using VErp.Services.PurchaseOrder.Model.PurchasingRequest;

namespace VErpApi.Controllers.PurchaseOrder.PurchasingRequest
{
    [Route("api/purchasingrequest")]
    public class PurchasingRequestController : VErpBaseController
    {
        private readonly IPurchasingRequestService _purchasingRequestService;

        public PurchasingRequestController(IPurchasingRequestService purchasingRequestService)
        {
            _purchasingRequestService = purchasingRequestService;
        }

        /// <summary>
        /// Lấy danh sách phiếu yêu cầu mua hàng
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="statusList"></param>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetList")]
        public async Task<ApiResponse<PageData<PurchasingRequestOutputModel>>> GetList([FromQuery] string keyword, [FromQuery] List<int> statusList, [FromQuery] long beginTime, [FromQuery] long endTime, [FromQuery] int page, [FromQuery] int size)
        {
            return await _purchasingRequestService.GetList(keyword: keyword, statusList: statusList, beginTime: beginTime, endTime: endTime, page: page, size: size);
        }

        /// <summary>
        /// Lấy thông tin phiếu yêu cầu mua hàng
        /// </summary>
        /// <param name="purchasingRequestId">Id phiếu</param>
        /// <returns>PurchasingRequestOutputModel</returns>
        [HttpGet]
        [Route("{purchasingRequestId}")]
        public async Task<ApiResponse<PurchasingRequestOutputModel>> Get([FromRoute] long purchasingRequestId)
        {
            return await _purchasingRequestService.Get(purchasingRequestId);
        }

        /// <summary>
        /// Thêm mới phiếu yêu cầu mua hàng
        /// </summary>
        /// <param name="req">Model PurchasingRequestInputModel</param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public async Task<ApiResponse<long>> Add([FromBody] PurchasingRequestInputModel req)
        {
            return await _purchasingRequestService.AddPurchasingRequest(UserId, req);
        }

        /// <summary>
        /// Cập nhật phiếu yêu cầu mua hàng
        /// </summary>
        /// <param name="purchasingRequestId">Id phiếu</param>
        /// <param name="req">Model PurchasingRequestInputModel</param>
        /// <returns></returns>
        [HttpPut]
        [Route("{purchasingRequestId}")]
        public async Task<ApiResponse> Update([FromRoute] long purchasingRequestId, [FromBody] PurchasingRequestInputModel req)
        {
            return await _purchasingRequestService.UpdatePurchasingRequest(purchasingRequestId, UserId, req);
        }

        /// <summary>
        /// Gửi duyệt phiếu yêu cầu mua hàng
        /// </summary>
        /// <param name="purchasingRequestId">Id phiếu yêu cầu mua hàng</param>        
        /// <returns></returns>
        [HttpPut]
        [Route("{purchasingRequestId}/SendApprove")]
        [VErpAction(EnumAction.Censor)]
        public async Task<ApiResponse> SentToApprove([FromRoute] long purchasingRequestId)
        {
            return await _purchasingRequestService.SendToApprove(purchasingRequestId, UserId);
        }

        /// <summary>
        /// Duyệt phiếu yêu cầu mua hàng
        /// </summary>
        /// <param name="purchasingRequestId">Id phiếu yêu cầu mua hàng</param>        
        /// <returns></returns>
        [HttpPut]
        [Route("{purchasingRequestId}/Approve")]
        [VErpAction(EnumAction.Censor)]
        public async Task<ApiResponse> Approve([FromRoute] long purchasingRequestId)
        {
            return await _purchasingRequestService.ApprovePurchasingRequest(purchasingRequestId, UserId);
        }

        /// <summary>
        ///  Từ chối phiếu yêu cầu mua hàng
        /// </summary>
        /// <param name="purchasingRequestId"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{purchasingRequestId}/Reject")]
        [VErpAction(EnumAction.Censor)]
        public async Task<ApiResponse> Reject([FromRoute] long purchasingRequestId)
        {
            return await _purchasingRequestService.RejectPurchasingRequest(purchasingRequestId, UserId);
        }

        /// <summary>
        /// Xóa phiếu yêu cầu mua hàng
        /// </summary>
        /// <param name="purchasingRequestId"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{purchasingRequestId}")]
        public async Task<ApiResponse> Delete([FromRoute] long purchasingRequestId)
        {
            var currentUserId = UserId;
            return await _purchasingRequestService.DeletePurchasingRequest(purchasingRequestId, currentUserId);
        }
    }
}
