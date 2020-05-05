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
        public async Task<ServiceResult<PageData<PurchasingSuggestOutputList>>> GetList([FromQuery] string keyword, [FromQuery] EnumPurchasingSuggestStatus? purchasingSuggestStatusId, [FromQuery] EnumPoProcessStatus? poProcessStatusId, [FromQuery] bool? isApproved, [FromQuery] long? fromDate, [FromQuery] long? toDate, [FromQuery]string sortBy, [FromQuery] bool asc, [FromQuery] int page, [FromQuery] int size)
        {
            return await _purchasingSuggestService.GetList(keyword, purchasingSuggestStatusId, poProcessStatusId, isApproved, fromDate, toDate, sortBy, asc, page, size).ConfigureAwait(true);
        }

        /// <summary>
        /// Lấy danh sách phiếu đề nghị mua hàng chi tiết theo sản phẩm
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="productIds"></param>
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
        [Route("GetListByProduct")]
        public async Task<ServiceResult<PageData<PurchasingSuggestOutputListByProduct>>> GetListByProduct([FromQuery] string keyword, [FromQuery] IList<int> productIds, [FromQuery] EnumPurchasingSuggestStatus? purchasingSuggestStatusId, [FromQuery] EnumPoProcessStatus? poProcessStatusId, [FromQuery] bool? isApproved, [FromQuery] long? fromDate, [FromQuery] long? toDate, [FromQuery]string sortBy, [FromQuery] bool asc, [FromQuery] int page, [FromQuery] int size)
        {
            return await _purchasingSuggestService.GetListByProduct(keyword, productIds, purchasingSuggestStatusId, poProcessStatusId, isApproved, fromDate, toDate, sortBy, asc, page, size).ConfigureAwait(true);
        }

        /// <summary>
        /// Lấy thông tin phiếu đề nghị mua hàng
        /// </summary>
        /// <param name="purchasingSuggestId">Id phiếu</param>
        /// <returns>PurchasingSuggestOutputModel</returns>
        [HttpGet]
        [Route("{purchasingSuggestId}")]
        public async Task<ServiceResult<PurchasingSuggestOutput>> GetInfo([FromRoute] long purchasingSuggestId)
        {
            return await _purchasingSuggestService.GetInfo(purchasingSuggestId).ConfigureAwait(true);
        }

        /// <summary>
        /// Thêm mới phiếu đề nghị mua hàng
        /// </summary>
        /// <param name="req">Model PurchasingSuggestInputModel</param>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public async Task<ServiceResult<long>> Add([FromBody] PurchasingSuggestInput req)
        {
            return await _purchasingSuggestService.Create(req).ConfigureAwait(true);
        }

        /// <summary>
        /// Cập nhật phiếu đề nghịmua hàng
        /// </summary>
        /// <param name="purchasingSuggestId">Id phiếu</param>
        /// <param name="req">Model PurchasingSuggestInputModel</param>
        /// <returns></returns>
        [HttpPut]
        [Route("{purchasingSuggestId}")]
        public async Task<ServiceResult> Update([FromRoute] long purchasingSuggestId, [FromBody] PurchasingSuggestInput req)
        {
            return await _purchasingSuggestService.Update(purchasingSuggestId, req).ConfigureAwait(true);
        }

        /// <summary>
        /// Gửi duyệt phiếu đề nghị mua hàng
        /// </summary>
        /// <param name="purchasingSuggestId">Id phiếu yêu cầu mua hàng</param>        
        /// <returns></returns>
        [HttpPut]
        [Route("{purchasingSuggestId}/SendCensor")]
        public async Task<ServiceResult> SentToApprove([FromRoute] long purchasingSuggestId)
        {
            return await _purchasingSuggestService.SendToCensor(purchasingSuggestId).ConfigureAwait(true);
        }

        /// <summary>
        /// Duyệt phiếu đề nghị mua hàng
        /// </summary>
        /// <param name="purchasingSuggestId">Id phiếu yêu cầu mua hàng</param>        
        /// <returns></returns>
        [HttpPut]
        [Route("{purchasingSuggestId}/Approve")]
        [VErpAction(EnumAction.Censor)]
        public async Task<ServiceResult> Approve([FromRoute] long purchasingSuggestId)
        {
            return await _purchasingSuggestService.Approve(purchasingSuggestId).ConfigureAwait(true);
        }

        /// <summary>
        ///  Từ chối phiếu đề nghị mua hàng
        /// </summary>
        /// <param name="purchasingSuggestId"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{purchasingSuggestId}/Reject")]
        [VErpAction(EnumAction.Censor)]
        public async Task<ServiceResult> Reject([FromRoute] long purchasingSuggestId)
        {
            return await _purchasingSuggestService.Reject(purchasingSuggestId).ConfigureAwait(true);
        }

        /// <summary>
        /// Xóa phiếu đề nghị mua hàng
        /// </summary>
        /// <param name="purchasingSuggestId"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{purchasingSuggestId}")]
        public async Task<ServiceResult> Delete([FromRoute] long purchasingSuggestId)
        {
            return await _purchasingSuggestService.Delete(purchasingSuggestId).ConfigureAwait(true);
        }

        /// <summary>
        /// Cập nhật trạng thái mua hàng cho phiếu đề nghị mua hàng
        /// </summary>
        /// <param name="purchasingSuggestId"></param>
        /// <param name="poProcessStatusModel"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{purchasingSuggestId}/UpdatePoProcessStatus")]
        public async Task<ServiceResult> UpdatePoProcessStatus([FromRoute] long purchasingSuggestId, [FromBody] UpdatePoProcessStatusModel poProcessStatusModel)
        {
            if (poProcessStatusModel == null) return GeneralCode.InvalidParams;
            return await _purchasingSuggestService.UpdatePoProcessStatus(purchasingSuggestId, poProcessStatusModel.PoProcessStatusId).ConfigureAwait(true);
        }


        /// <summary>
        /// Lấy tất cả danh sách phân công mua hàng
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="assigneeUserId"></param>
        /// <param name="poAssignmentStatusId"></param>
        /// <param name="purchasingSuggestId"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="sortBy"></param>
        /// <param name="asc"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("AllUsers/Assignments")]
        public async Task<ServiceResult<PageData<PoAssignmentOutputList>>> AllUsersAssignments([FromQuery] string keyword, [FromQuery] int? assigneeUserId, [FromQuery] EnumPoAssignmentStatus? poAssignmentStatusId, [FromQuery]  long? purchasingSuggestId, [FromQuery]  long? fromDate, [FromQuery]  long? toDate, [FromQuery]  string sortBy, [FromQuery]  bool asc, [FromQuery]  int page, [FromQuery]  int size)
        {
            return await _purchasingSuggestService
                .PoAssignmentListByUser(keyword, poAssignmentStatusId, assigneeUserId, purchasingSuggestId, fromDate, toDate, sortBy, asc, page, size)
                .ConfigureAwait(true);
        }

        /// <summary>
        /// Lấy danh sách phân công mua hàng theo sản phẩm
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="assigneeUserId"></param>
        /// <param name="productIds"></param>
        /// <param name="poAssignmentStatusId"></param>
        /// <param name="purchasingSuggestId"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="sortBy"></param>
        /// <param name="asc"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("AllUsers/AssignmentsByProduct")]
        public async Task<ServiceResult<PageData<PoAssignmentOutputListByProduct>>> AllUsersAssignmentsByProduct([FromQuery] string keyword, [FromQuery] int? assigneeUserId, [FromQuery] IList<int> productIds, [FromQuery] EnumPoAssignmentStatus? poAssignmentStatusId, [FromQuery]  long? purchasingSuggestId, [FromQuery]  long? fromDate, [FromQuery]  long? toDate, [FromQuery]  string sortBy, [FromQuery]  bool asc, [FromQuery]  int page, [FromQuery]  int size)
        {
            return await _purchasingSuggestService
                .PoAssignmentListByProduct(keyword, productIds, poAssignmentStatusId, assigneeUserId, purchasingSuggestId, fromDate, toDate, sortBy, asc, page, size)
                .ConfigureAwait(true);
        }

        /// <summary>
        /// Lấy thông tin phân công mua hàng theo user đang đăng nhập
        /// </summary>
        /// <param name="poAssignmentId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("AllUsers/Assignments/{poAssignmentId}")]
        public async Task<ServiceResult<PoAssignmentOutput>> AllUsersAssignmentInfo([FromRoute] long poAssignmentId)
        {
            return await _purchasingSuggestService
                .PoAssignmentInfo(poAssignmentId, null)
                .ConfigureAwait(true);
        }


        /// <summary>
        /// Lấy danh sách phân công mua hàng được giao của user đang login
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="poAssignmentStatusId"></param>
        /// <param name="purchasingSuggestId"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="sortBy"></param>
        /// <param name="asc"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("CurrentUser/Assignments")]
        public async Task<ServiceResult<PageData<PoAssignmentOutputList>>> AssignmentsByCurrentUser([FromQuery] string keyword, [FromQuery]  EnumPoAssignmentStatus? poAssignmentStatusId, [FromQuery]  long? purchasingSuggestId, [FromQuery]  long? fromDate, [FromQuery]  long? toDate, [FromQuery]  string sortBy, [FromQuery]  bool asc, [FromQuery]  int page, [FromQuery]  int size)
        {
            return await _purchasingSuggestService
                .PoAssignmentListByUser(keyword, poAssignmentStatusId, UserId, purchasingSuggestId, fromDate, toDate, sortBy, asc, page, size)
                .ConfigureAwait(true);
        }

        /// <summary>
        /// Lấy danh sách phân công mua hàng được giao của user đang login chi tiết theo sản phẩm
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="productIds"></param>
        /// <param name="poAssignmentStatusId"></param>
        /// <param name="purchasingSuggestId"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <param name="sortBy"></param>
        /// <param name="asc"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("CurrentUser/AssignmentsByProduct")]
        public async Task<ServiceResult<PageData<PoAssignmentOutputListByProduct>>> AssignmentsByCurrentUserByProduct([FromQuery] string keyword, [FromQuery] IList<int> productIds, [FromQuery] EnumPoAssignmentStatus? poAssignmentStatusId, [FromQuery]  long? purchasingSuggestId, [FromQuery]  long? fromDate, [FromQuery]  long? toDate, [FromQuery]  string sortBy, [FromQuery]  bool asc, [FromQuery]  int page, [FromQuery]  int size)
        {
            return await _purchasingSuggestService
                .PoAssignmentListByProduct(keyword, productIds, poAssignmentStatusId, UserId, purchasingSuggestId, fromDate, toDate, sortBy, asc, page, size)
                .ConfigureAwait(true);
        }

        /// <summary>
        /// Lấy thông tin phân công mua hàng theo user đang đăng nhập
        /// </summary>
        /// <param name="poAssignmentId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("CurrentUser/Assignments/{poAssignmentId}")]
        public async Task<ServiceResult<PoAssignmentOutput>> CurrentUserAssignmentInfo([FromRoute] long poAssignmentId)
        {
            return await _purchasingSuggestService
                .PoAssignmentInfo(poAssignmentId, UserId)
                .ConfigureAwait(true);
        }

        /// <summary>
        /// Xác nhận phân công
        /// </summary>
        /// <param name="poAssignmentId"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("CurrentUser/Assignments/{poAssignmentId}/Confirm")]
        public async Task<ServiceResult> PoAssignmentsUserConfirm([FromRoute] long poAssignmentId)
        {
            return await _purchasingSuggestService.PoAssignmentUserConfirm(poAssignmentId).ConfigureAwait(true);
        }

        /// <summary>
        /// Lấy danh sách phân công mua hàng
        /// </summary>
        /// <param name="purchasingSuggestId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{purchasingSuggestId}/Assignments")]
        public async Task<ServiceResult<IList<PoAssignmentOutput>>> SuggestAssignments([FromRoute] long purchasingSuggestId)
        {
            return await _purchasingSuggestService.PoAssignmentListBySuggest(purchasingSuggestId).ConfigureAwait(true);
        }

        /// <summary>
        /// Thêm mới phân công mua hàng
        /// </summary>
        /// <param name="purchasingSuggestId"></param>
        /// <param name="poAssignment"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{purchasingSuggestId}/Assignments")]
        public async Task<ServiceResult<long>> CreatePoAssignments([FromRoute] long purchasingSuggestId, [FromBody] PoAssignmentInput poAssignment)
        {
            return await _purchasingSuggestService.PoAssignmentCreate(purchasingSuggestId, poAssignment).ConfigureAwait(true);
        }

        /// <summary>
        /// Cập nhật phân công mua hàng
        /// </summary>
        /// <param name="purchasingSuggestId"></param>
        /// <param name="poAssignmentId"></param>
        /// <param name="poAssignment"></param>
        /// <returns></returns>
        [HttpPut]
        [Route("{purchasingSuggestId}/Assignments/{poAssignmentId}")]
        public async Task<ServiceResult> UpdatePoAssignments([FromRoute] long purchasingSuggestId, [FromRoute] long poAssignmentId, [FromBody] PoAssignmentInput poAssignment)
        {
            return await _purchasingSuggestService.PoAssignmentUpdate(purchasingSuggestId, poAssignmentId, poAssignment).ConfigureAwait(true);
        }

        /// <summary>
        /// Gửi đến người được phân công
        /// </summary>
        /// <param name="purchasingSuggestId"></param>
        /// <param name="poAssignmentId">Id phân công</param>
        /// <returns></returns>
        [HttpPut]
        [Route("{purchasingSuggestId}/Assignments/{poAssignmentId}/SendToUser")]
        public async Task<ServiceResult> PoAssignmentsSendToUser([FromRoute] long purchasingSuggestId, [FromRoute] long poAssignmentId)
        {
            return await _purchasingSuggestService.PoAssignmentSendToUser(purchasingSuggestId, poAssignmentId).ConfigureAwait(true);
        }


        /// <summary>
        /// Xóa phân công mua hàng
        /// </summary>
        /// <param name="purchasingSuggestId"></param>
        /// <param name="poAssignmentId"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("{purchasingSuggestId}/Assignments/{poAssignmentId}")]
        public async Task<ServiceResult> DeletePoAssignments([FromRoute] long purchasingSuggestId, [FromRoute] long poAssignmentId)
        {
            return await _purchasingSuggestService.PoAssignmentDelete(purchasingSuggestId, poAssignmentId).ConfigureAwait(true);
        }
    }
}
