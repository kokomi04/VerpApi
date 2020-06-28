using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.MasterEnum.PO;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Enums.StockEnum;
using VErp.Commons.GlobalObject;
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
        public async Task<PageData<PurchaseOrderOutputList>> GetList([FromQuery] string keyword, [FromQuery] EnumPurchaseOrderStatus? purchaseOrderStatusId, [FromQuery] EnumPoProcessStatus? poProcessStatusId, [FromQuery] bool? isChecked, [FromQuery] bool? isApproved, [FromQuery] long? fromDate, [FromQuery] long? toDate, [FromQuery] string sortBy, [FromQuery] bool asc, [FromQuery] int page, [FromQuery] int size)
        {
            return await _purchaseOrderService
                .GetList(keyword, purchaseOrderStatusId, poProcessStatusId, isChecked, isApproved, fromDate, toDate, sortBy, asc, page, size)
                .ConfigureAwait(true);
        }


        /// <summary>
        /// Lấy danh sách đơn đặt hàng chi tiết theo sản phẩm
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="productIds"></param>
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
        [Route("GetListByProduct")]
        public async Task<PageData<PurchaseOrderOutputListByProduct>> GetListByProduct([FromQuery] string keyword, [FromQuery] IList<int> productIds, [FromQuery] EnumPurchaseOrderStatus? purchaseOrderStatusId, [FromQuery] EnumPoProcessStatus? poProcessStatusId, [FromQuery] bool? isChecked, [FromQuery] bool? isApproved, [FromQuery] long? fromDate, [FromQuery] long? toDate, [FromQuery] string sortBy, [FromQuery] bool asc, [FromQuery] int page, [FromQuery] int size)
        {
            return await _purchaseOrderService
                .GetListByProduct(keyword, productIds, purchaseOrderStatusId, poProcessStatusId, isChecked, isApproved, fromDate, toDate, sortBy, asc, page, size)
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
        public async Task<bool> Update([FromRoute] long purchaseOrderId, [FromBody] PurchaseOrderInput req)
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
        public async Task<bool> SentToCensor([FromRoute] long purchaseOrderId)
        {
            return await _purchaseOrderService
                .SentToCensor(purchaseOrderId)
                .ConfigureAwait(true);
        }

        /// <summary>
        /// Kiểm soát kiểm tra PO
        /// </summary>
        /// <param name="purchaseOrderId">Id phiếu yêu cầu mua hàng</param>        
        /// <returns></returns>
        [HttpPut]
        [Route("{purchaseOrderId}/Check")]
        [VErpAction(EnumAction.Check)]
        public async Task<bool> Checked([FromRoute] long purchaseOrderId)
        {
            return await _purchaseOrderService
                 .Checked(purchaseOrderId)
                 .ConfigureAwait(true);
        }

        /// <summary>
        ///  Kiểm soát từ chối PO
        /// </summary>
        /// <param name="purchaseOrderId"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{purchaseOrderId}/RejectCheck")]
        [VErpAction(EnumAction.Check)]
        public async Task<bool> RejectCheck([FromRoute] long purchaseOrderId)
        {
            return await _purchaseOrderService
                .RejectCheck(purchaseOrderId)
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
        public async Task<bool> Approve([FromRoute] long purchaseOrderId)
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
        public async Task<bool> Reject([FromRoute] long purchaseOrderId)
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
        public async Task<bool> Delete([FromRoute] long purchaseOrderId)
        {
            return await _purchaseOrderService
                .Delete(purchaseOrderId)
                .ConfigureAwait(true);
        }

        /// <summary>
        /// Cập nhật trạng thái mua hàng cho phiếu đặt hàng
        /// </summary>
        /// <param name="purchaseOrderId"></param>
        /// <param name="poProcessStatusModel"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{purchaseOrderId}/UpdatePoProcessStatus")]
        public async Task<bool> UpdatePoProcessStatus([FromRoute] long purchaseOrderId, [FromBody] UpdatePoProcessStatusModel poProcessStatusModel)
        {
            if (poProcessStatusModel == null) throw new BadRequestException(GeneralCode.InvalidParams);

            return await _purchaseOrderService
                .UpdatePoProcessStatus(purchaseOrderId, poProcessStatusModel.PoProcessStatusId)
                .ConfigureAwait(true);
        }


        /// <summary>
        /// Lấy danh sách PO đã tạo từ suggest
        /// </summary>
        /// <param name="purchasingSuggestIds"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetPurchaseOrderBySuggest")]
        public async Task<IDictionary<long, IList<PurchaseOrderOutputBasic>>> GetPurchaseOrderBySuggest([FromQuery] IList<long> purchasingSuggestIds)
        {
            return await _purchaseOrderService.GetPurchaseOrderBySuggest(purchasingSuggestIds).ConfigureAwait(true);
        }

        /// <summary>
        /// Lấy danh sách PO đã tạo từ assignment
        /// </summary>
        /// <param name="poAssignmentIds"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetPurchaseOrderByAssignment")]
        public async Task<IDictionary<long, IList<PurchaseOrderOutputBasic>>> GetPurchaseOrderByAssignment([FromQuery] IList<long> poAssignmentIds)
        {
            return await _purchaseOrderService.GetPurchaseOrderByAssignment(poAssignmentIds).ConfigureAwait(true);
        }
    }
}
