using System.Collections.Generic;
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
using VErp.Services.Master.Model.Activity;
using VErp.Services.PurchaseOrder.Service.PurchasingSuggest;
using VErp.Services.PurchaseOrder.Model.PurchasingSuggest;

namespace VErpApi.Controllers.PurchaseOrder
{
    [Route("api/purchasingsuggest")]
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
        /// <param name="statusList"></param>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetList")]
        public async Task<ApiResponse<PageData<PurchasingSuggestOutputModel>>> GetList([FromQuery] string keyword, [FromQuery] List<EnumPurchasingSuggestStatus> statusList, [FromQuery] long beginTime, [FromQuery] long endTime, [FromQuery] int page, [FromQuery] int size)
        {
            return await _purchasingSuggestService.GetList(keyword: keyword, statusList: statusList, beginTime: beginTime, endTime: endTime, page: page, size: size);
        }

        /// <summary>
        /// Lấy thông tin phiếu đề nghị mua hàng
        /// </summary>
        /// <param name="purchasingSuggestId">Id phiếu</param>
        /// <returns>PurchasingSuggestOutputModel</returns>
        [HttpGet]
        [Route("{purchasingSuggestId}")]
        public async Task<ApiResponse<PurchasingSuggestOutputModel>> Get([FromRoute] long purchasingSuggestId)
        {
            return await _purchasingSuggestService.Get(purchasingSuggestId);
        }

        /// <summary>
        /// Thêm mới phiếu đề nghị mua hàng
        /// </summary>
        /// <param name="req">Model PurchasingSuggestInputModel</param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public async Task<ApiResponse<long>> Add([FromBody] PurchasingSuggestInputModel req)
        {
            return await _purchasingSuggestService.AddPurchasingSuggest(UserId, req);
        }

        /// <summary>
        /// Cập nhật phiếu đề nghị mua hàng
        /// </summary>
        /// <param name="purchasingSuggestId">Id phiếu</param>
        /// <param name="req">Model PurchasingSuggestInputModel</param>
        /// <returns></returns>
        [HttpPut]
        [Route("{purchasingSuggestId}")]
        public async Task<ApiResponse> Update([FromRoute] long purchasingSuggestId, [FromBody] PurchasingSuggestInputModel req)
        {
            return await _purchasingSuggestService.UpdatePurchasingSuggest(purchasingSuggestId, UserId, req);
        }

        /// <summary>
        /// Gửi duyệt phiếu đề nghị mua hàng
        /// </summary>
        /// <param name="purchasingSuggestId">Id phiếu đề nghị mua hàng</param>        
        /// <returns></returns>
        [HttpPut]
        [Route("{purchasingSuggestId}/SendApprove")]
        [VErpAction(EnumAction.Censor)]
        public async Task<ApiResponse> SentToApprove([FromRoute] long purchasingSuggestId)
        {
            return await _purchasingSuggestService.SendToApprove(purchasingSuggestId, UserId);
        }

        /// <summary>
        /// Duyệt phiếu đề nghị mua hàng
        /// </summary>
        /// <param name="purchasingSuggestId">Id phiếu đề nghị mua hàng</param>        
        /// <returns></returns>
        [HttpPut]
        [Route("{purchasingSuggestId}/Approve")]
        [VErpAction(EnumAction.Censor)]
        public async Task<ApiResponse> Approve([FromRoute] long purchasingSuggestId, EnumPurchasingSuggestStatus status)
        {
            return await _purchasingSuggestService.ApprovePurchasingSuggest(purchasingSuggestId, status,UserId);
        }

        /// <summary>
        ///  Từ chối phiếu đề nghị mua hàng
        /// </summary>
        /// <param name="purchasingSuggestId"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{purchasingSuggestId}/Reject")]
        [VErpAction(EnumAction.Censor)]
        public async Task<ApiResponse> Reject([FromRoute] long purchasingSuggestId)
        {
            return await _purchasingSuggestService.RejectPurchasingSuggest(purchasingSuggestId, UserId);
        }

        /// <summary>
        /// Xóa phiếu đề nghị mua hàng
        /// </summary>
        /// <param name="purchasingSuggestId"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{purchasingSuggestId}")]
        public async Task<ApiResponse> Delete([FromRoute] long purchasingSuggestId)
        {
            var currentUserId = UserId;
            return await _purchasingSuggestService.DeletePurchasingSuggest(purchasingSuggestId, currentUserId);
        }

        /// <summary>
        /// Thêm ghi chú vào phiếu đề nghị mua VTHH
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="actionTypeId"></param>
        /// <param name="note"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddNote")]
        public async Task<ApiResponse> AddNote(long objectId, int actionTypeId = 0, string note = "")
        {
            var currentUserId = UserId;
            return await _purchasingSuggestService.AddNote(objectId, currentUserId, actionTypeId, note);
        }

        /// <summary>
        /// Lấy danh sách ghi chú của phiếu đề nghị mua VTHH
        /// </summary>
        /// <param name="objectId"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetNoteList")]
        public async Task<ApiResponse<PageData<UserActivityLogOuputModel>>> GetNoteList(long objectId, int page = 1, int size = 20)
        {
            return await _purchasingSuggestService.GetNoteList(objectId, page, size);
        }
    }
}
