using System;
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
    [Route("api/PurchasingOrder")]
    public class PurchasingOrderController : VErpBaseController
    {

        private readonly IPurchaseOrderService _purchaseOrderService;
        public PurchasingOrderController(IPurchaseOrderService purchaseOrderService)
        {
            _purchaseOrderService = purchaseOrderService;

        }

        /// <summary>
        /// Lấy danh sách đơn đặt hàng
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="purchaseOrderStatusId"></param>
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
        public async Task<ServiceResult<PageData<PurchaseOrderOutputList>>> GetList([FromQuery] string keyword, [FromQuery] EnumPurchaseOrderStatus? purchaseOrderStatusId, [FromQuery] EnumPoProcessStatus? poProcessStatusId, [FromQuery] bool? isApproved, [FromQuery] long? fromDate, [FromQuery] long? toDate, [FromQuery]string sortBy, [FromQuery] bool asc, [FromQuery] int page, [FromQuery] int size)
        {
            return await _purchaseOrderService
                .GetList(keyword, purchaseOrderStatusId, poProcessStatusId, isApproved, fromDate, toDate, sortBy, asc, page, size)
                .ConfigureAwait(true);
        }

        /// <summary>
        /// Lấy thông tin đặt hàng
        /// </summary>
        /// <param name="purchaseOrderId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{purchaseOrderId}")]
        public async Task<ServiceResult<PurchaseOrderOutput>> GetInfo([FromRoute] long purchaseOrderId)
        {
            return await _purchaseOrderService
                .GetInfo(purchaseOrderId)
                .ConfigureAwait(true);
        }

        /// <summary>
        /// Thêm mới danh sách đặt hàng
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public async Task<ServiceResult<long>> Create([FromBody] PurchaseOrderInput req)
        {
            return await _purchaseOrderService
                .Create(req)
                .ConfigureAwait(true);
        }

        /// <summary>
        /// Cập nhật đặt hàng PO
        /// </summary>
        /// <param name="purchaseOrderId"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{purchaseOrderId}")]
        public async Task<ServiceResult> Update([FromRoute] long purchaseOrderId, [FromBody] PurchaseOrderInput req)
        {
            return await _purchaseOrderService
                .Update(purchaseOrderId, req)
                .ConfigureAwait(true);
        }

        /// <summary>
        /// Gửi duyệt đơn đặt hàng PO
        /// </summary>
        /// <param name="purchaseOrderId"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{purchaseOrderId}/SendCensor")]
        public async Task<ServiceResult> SentToCensor([FromRoute] long purchaseOrderId)
        {
            return await _purchaseOrderService
                .SentToCensor(purchaseOrderId)
                .ConfigureAwait(true);
        }

        /// <summary>
        /// Duyệt phiếu đơn đặt hàng PO
        /// </summary>
        /// <param name="purchaseOrderId">Id phiếu yêu cầu mua hàng</param>        
        /// <returns></returns>
        [HttpPut]
        [Route("{purchaseOrderId}/Approve")]
        [VErpAction(EnumAction.Censor)]
        public async Task<ServiceResult> Approve([FromRoute] long purchaseOrderId)
        {
            return await _purchaseOrderService
                 .Approve(purchaseOrderId)
                 .ConfigureAwait(true);
        }

        /// <summary>
        ///  Từ chối phiếu đề nghị mua hàng
        /// </summary>
        /// <param name="purchaseOrderId"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{purchaseOrderId}/Reject")]
        [VErpAction(EnumAction.Censor)]
        public async Task<ServiceResult> Reject([FromRoute] long purchaseOrderId)
        {
            return await _purchaseOrderService
                .Reject(purchaseOrderId)
                .ConfigureAwait(true);
        }

        /// <summary>
        /// Xóa phiếu đặt hàng PO
        /// </summary>
        /// <param name="purchaseOrderId"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{purchaseOrderId}")]
        public async Task<ServiceResult> Delete([FromRoute] long purchaseOrderId)
        {
            return await _purchaseOrderService
                .Delete(purchaseOrderId)
                .ConfigureAwait(true);
        }

        /// <summary>
        /// Cập nhật trạng thái mua hàng cho phiếu đặt hàng
        /// </summary>
        /// <param name="purchaseOrderId"></param>
        /// <param name="poProcessStatusId"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{purchaseOrderId}/UpdatePoProcessStatus")]
        public async Task<ServiceResult> UpdatePoProcessStatus([FromRoute] long purchaseOrderId, [FromBody] EnumPoProcessStatus poProcessStatusId)
        {
            return await _purchaseOrderService
                .UpdatePoProcessStatus(purchaseOrderId, poProcessStatusId)
                .ConfigureAwait(true);
        }
    }
}
