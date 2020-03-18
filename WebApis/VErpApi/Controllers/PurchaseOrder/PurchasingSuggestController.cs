﻿using System.Collections.Generic;
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
        private readonly IPurchasingSuggestService _purchasingSuggestService;

        public PurchasingSuggestController(IPurchasingSuggestService purchasingSuggestService)
        {
            _purchasingSuggestService = purchasingSuggestService;
        }

        /// <summary>
        /// Lấy danh sách phiếu đề nghị mua hàng
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="purchasingSuggestStatusId"></param>
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
        public async Task<ApiResponse<PageData<PurchasingSuggestOutputList>>> GetList([FromQuery] string keyword, [FromQuery] EnumPurchasingSuggestStatus? purchasingSuggestStatusId, [FromQuery] EnumPoProcessStatus? poProcessStatusId, [FromQuery] bool? isApproved, [FromQuery] long? fromDate, [FromQuery] long? toDate, [FromQuery]string sortBy, [FromQuery] bool asc, [FromQuery] int page, [FromQuery] int size)
        {
            return await _purchasingSuggestService.GetList(keyword, purchasingSuggestStatusId, poProcessStatusId, isApproved, fromDate, toDate, sortBy, asc, page, size);
        }

        /// <summary>
        /// Lấy thông tin phiếu đề nghị mua hàng
        /// </summary>
        /// <param name="purchasingSuggestId">Id phiếu</param>
        /// <returns>PurchasingSuggestOutputModel</returns>
        [HttpGet]
        [Route("{PurchasingSuggestId}")]
        public async Task<ApiResponse<PurchasingSuggestOutput>> GetInfo([FromRoute] long purchasingSuggestId)
        {
            return await _purchasingSuggestService.GetInfo(purchasingSuggestId);
        }

        /// <summary>
        /// Thêm mới phiếu đề nghị mua hàng
        /// </summary>
        /// <param name="req">Model PurchasingSuggestInputModel</param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public async Task<ApiResponse<long>> Add([FromBody] PurchasingSuggestInput req)
        {
            return await _purchasingSuggestService.Create(req);
        }

        /// <summary>
        /// Cập nhật phiếu đề nghịmua hàng
        /// </summary>
        /// <param name="purchasingSuggestId">Id phiếu</param>
        /// <param name="req">Model PurchasingSuggestInputModel</param>
        /// <returns></returns>
        [HttpPut]
        [Route("{PurchasingSuggestId}")]
        public async Task<ApiResponse> Update([FromRoute] long purchasingSuggestId, [FromBody] PurchasingSuggestInput req)
        {
            return await _purchasingSuggestService.Update(purchasingSuggestId, req);
        }

        /// <summary>
        /// Gửi duyệt phiếu đề nghị mua hàng
        /// </summary>
        /// <param name="purchasingSuggestId">Id phiếu yêu cầu mua hàng</param>        
        /// <returns></returns>
        [HttpPut]
        [Route("{PurchasingSuggestId}/SendCensor")]
        public async Task<ApiResponse> SentToApprove([FromRoute] long purchasingSuggestId)
        {
            return await _purchasingSuggestService.SendToCensor(purchasingSuggestId);
        }

        /// <summary>
        /// Duyệt phiếu đề nghị mua hàng
        /// </summary>
        /// <param name="purchasingSuggestId">Id phiếu yêu cầu mua hàng</param>        
        /// <returns></returns>
        [HttpPut]
        [Route("{PurchasingSuggestId}/Approve")]
        [VErpAction(EnumAction.Censor)]
        public async Task<ApiResponse> Approve([FromRoute] long purchasingSuggestId)
        {
            return await _purchasingSuggestService.Approve(purchasingSuggestId);
        }

        /// <summary>
        ///  Từ chối phiếu đề nghị mua hàng
        /// </summary>
        /// <param name="purchasingSuggestId"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{PurchasingSuggestId}/Reject")]
        [VErpAction(EnumAction.Censor)]
        public async Task<ApiResponse> Reject([FromRoute] long purchasingSuggestId)
        {
            return await _purchasingSuggestService.Reject(purchasingSuggestId);
        }

        /// <summary>
        /// Xóa phiếu đề nghị mua hàng
        /// </summary>
        /// <param name="PurchasingSuggestId"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{PurchasingSuggestId}")]
        public async Task<ApiResponse> Delete([FromRoute] long purchasingSuggestId)
        {
            return await _purchasingSuggestService.Delete(purchasingSuggestId);
        }

        /// <summary>
        /// Cập nhật trạng thái mua hàng cho phiếu đề nghị mua hàng
        /// </summary>
        /// <param name="purchasingSuggestId"></param>
        /// <param name="poProcessStatusId"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{PurchasingSuggestId}/UpdatePoProcessStatus")]
        public async Task<ApiResponse> UpdatePoProcessStatus([FromRoute] long purchasingSuggestId, [FromBody] EnumPoProcessStatus poProcessStatusId)
        {
            return await _purchasingSuggestService.UpdatePoProcessStatus(purchasingSuggestId, poProcessStatusId);
        }
    }
}
