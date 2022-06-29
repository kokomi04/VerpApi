using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.MasterEnum.PO;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ApiCore.ModelBinders;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.PurchaseOrder.Model;
using VErp.Services.PurchaseOrder.Model.PurchaseOrder;
using VErp.Services.PurchaseOrder.Model.Request;
using VErp.Services.PurchaseOrder.Service;

namespace VErpApi.Controllers.PurchaseOrder
{
    [Route("api/PurchasingOrder")]
    public class PurchasingOrderController : VErpBaseController
    {

        private readonly IPurchaseOrderService _purchaseOrderService;
        private readonly IPurchaseOrderOutsourceStepService _purchaseOrderOutsourceStepService;
        private readonly IPurchaseOrderOutsourcePartService _purchaseOrderOutsourcePartService;
        private readonly IPurchaseOrderOutsourcePropertyService _purchaseOrderOutsourcePropertyService;
        public PurchasingOrderController(IPurchaseOrderService purchaseOrderService,
                                         IPurchaseOrderOutsourceStepService purchaseOrderOutsourceStepService,
                                         IPurchaseOrderOutsourcePartService purchaseOrderOutsourcePartService,
                                         IPurchaseOrderOutsourcePropertyService purchaseOrderOutsourcePropertyService)
        {
            _purchaseOrderService = purchaseOrderService;
            _purchaseOrderOutsourceStepService = purchaseOrderOutsourceStepService;
            _purchaseOrderOutsourcePartService = purchaseOrderOutsourcePartService;
            _purchaseOrderOutsourcePropertyService = purchaseOrderOutsourcePropertyService;
        }

        /// <summary>
        /// Lấy danh sách đơn đặt hàng
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="purchaseOrderTypes"></param>
        /// <param name="productIds"></param>
        /// <param name="purchaseOrderStatusId"></param>
        /// <param name="poProcessStatusId"></param>
        /// <param name="isChecked"></param>
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
        public async Task<PageData<PurchaseOrderOutputList>> GetList([FromQuery] string keyword, [FromQuery] IList<int> purchaseOrderTypes, [FromQuery] IList<int> productIds, [FromQuery] EnumPurchaseOrderStatus? purchaseOrderStatusId, [FromQuery] EnumPoProcessStatus? poProcessStatusId, [FromQuery] bool? isChecked, [FromQuery] bool? isApproved, [FromQuery] long? fromDate, [FromQuery] long? toDate, [FromQuery] string sortBy, [FromQuery] bool asc, [FromQuery] int page, [FromQuery] int size)
        {
            return await _purchaseOrderService
                .GetList(keyword, purchaseOrderTypes, productIds, purchaseOrderStatusId, poProcessStatusId, isChecked, isApproved, fromDate, toDate, sortBy, asc, page, size)
                .ConfigureAwait(true);
        }


        /// <summary>
        /// Lấy danh sách đơn đặt hàng chi tiết theo sản phẩm
        /// </summary>
        /// <param name="keyword"></param>
        /// <param name="purchaseOrderTypes"></param>
        /// <param name="productIds"></param>
        /// <param name="purchaseOrderStatusId"></param>
        /// <param name="poProcessStatusId"></param>
        /// <param name="isChecked"></param>
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
        public async Task<PageData<PurchaseOrderOutputListByProduct>> GetListByProduct([FromQuery] string keyword, [FromQuery] IList<int> purchaseOrderTypes, [FromQuery] IList<int> productIds, [FromQuery] EnumPurchaseOrderStatus? purchaseOrderStatusId, [FromQuery] EnumPoProcessStatus? poProcessStatusId, [FromQuery] bool? isChecked, [FromQuery] bool? isApproved, [FromQuery] long? fromDate, [FromQuery] long? toDate, [FromQuery] string sortBy, [FromQuery] bool asc, [FromQuery] int page, [FromQuery] int size)
        {
            return await _purchaseOrderService
                .GetListByProduct(keyword, null, purchaseOrderTypes, productIds, purchaseOrderStatusId, poProcessStatusId, isChecked, isApproved, fromDate, toDate, sortBy, asc, page, size)
                .ConfigureAwait(true);
        }

        [HttpPost("GetRowsByCodes")]
        [VErpAction(EnumActionType.View)]
        [GlobalApi]
        public async Task<IList<PurchaseOrderOutputListByProduct>> GetRowsByCodes([FromBody] IList<string> poCodes)
        {
            var data = await _purchaseOrderService.GetListByProduct(string.Empty, poCodes, null, null, null, null, null, null, null, null, string.Empty, false, 1, 0);
            return data.List;
        }

        /// <summary>
        /// Lấy thông tin đặt hàng
        /// </summary>
        /// <param name="purchaseOrderId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("{purchaseOrderId}")]
        public async Task<PurchaseOrderOutput> GetInfo([FromRoute] long purchaseOrderId)
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
        public async Task<long> Create([FromBody] PurchaseOrderInput req)
        {
            switch (req.PurchaseOrderType)
            {
                case EnumPurchasingOrderType.Default:
                    return await _purchaseOrderService.Create(req).ConfigureAwait(true);
                case EnumPurchasingOrderType.OutsourcePart:
                    return await _purchaseOrderOutsourcePartService.CreatePurchaseOrderOutsourcePart(req).ConfigureAwait(true);
                case EnumPurchasingOrderType.OutsourceStep:
                    return await _purchaseOrderOutsourceStepService.CreatePurchaseOrderOutsourceStep(req).ConfigureAwait(true);
                case EnumPurchasingOrderType.OutsourceProperty:
                    return await _purchaseOrderOutsourcePropertyService.CreatePurchaseOrderOutsourceProperty(req).ConfigureAwait(true);
                default:
                    throw new BadRequestException(GeneralCode.InvalidParams, "Không tồn tại loại đơn đặt mua trông hệ thống");
            }
        }

        [HttpGet]
        [Route("fieldDataForMapping")]
        public CategoryNameModel GetFieldDataForMapping()
        {
            return _purchaseOrderService.GetFieldDataForMapping();
        }

        [HttpPost]
        [Route("parseDetailsFromExcelMapping")]
        public IAsyncEnumerable<PurchaseOrderInputDetail> ImportFromMapping([FromFormString] ImportExcelMappingExtra<SingleInvoiceStaticContent> data, IFormFile file)
        {
            if (file == null || data == null)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }
            data.Mapping.FileName = file.FileName;
            return _purchaseOrderService.ParseInvoiceDetails(data.Mapping, data.Extra, file.OpenReadStream());
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
            switch (req.PurchaseOrderType)
            {
                case EnumPurchasingOrderType.Default:
                    return await _purchaseOrderService.Update(purchaseOrderId, req).ConfigureAwait(true);
                case EnumPurchasingOrderType.OutsourcePart:
                    return await _purchaseOrderOutsourcePartService.UpdatePurchaseOrderOutsourcePart(purchaseOrderId, req).ConfigureAwait(true);
                case EnumPurchasingOrderType.OutsourceStep:
                    return await _purchaseOrderOutsourceStepService.UpdatePurchaseOrderOutsourceStep(purchaseOrderId, req).ConfigureAwait(true);
                case EnumPurchasingOrderType.OutsourceProperty:
                    return await _purchaseOrderOutsourcePropertyService.UpdatePurchaseOrderOutsourceProperty(purchaseOrderId, req).ConfigureAwait(true);
                default:
                    throw new BadRequestException(GeneralCode.InvalidParams, "Không tồn tại loại đơn đặt mua trông hệ thống");
            }
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
        [VErpAction(EnumActionType.Check)]
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
        [VErpAction(EnumActionType.Check)]
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
        [VErpAction(EnumActionType.Censor)]
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
        [VErpAction(EnumActionType.Censor)]
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
            var po = await _purchaseOrderService.GetInfo(purchaseOrderId);
            switch ((EnumPurchasingOrderType)po.PurchaseOrderType)
            {
                case EnumPurchasingOrderType.Default:
                    return await _purchaseOrderService.Delete(purchaseOrderId).ConfigureAwait(true);
                case EnumPurchasingOrderType.OutsourcePart:
                    return await _purchaseOrderOutsourcePartService.DeletePurchaseOrderOutsourcePart(purchaseOrderId).ConfigureAwait(true);
                case EnumPurchasingOrderType.OutsourceStep:
                    return await _purchaseOrderOutsourceStepService.DeletePurchaseOrderOutsourceStep(purchaseOrderId).ConfigureAwait(true);
                case EnumPurchasingOrderType.OutsourceProperty:
                    return await _purchaseOrderOutsourcePropertyService.DeletePurchaseOrderOutsourceProperty(purchaseOrderId).ConfigureAwait(true);
                default:
                    throw new BadRequestException(GeneralCode.InvalidParams, "Không tồn tại loại đơn đặt mua trông hệ thống");
            }
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

        /// <summary>
        /// Gửi email thông báo kiểm tra/duyệt đơn mua hàng
        /// </summary>
        /// <param name="purchaseOrderId"></param>
        /// <param name="mailCode"></param>
        /// <param name="mailTo"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("{purchaseOrderId}/notify/sendMail")]
        public async Task<bool> SendMailNotifyCheckAndCensor([FromRoute] long purchaseOrderId, [FromQuery] string mailCode, [FromBody] string[] mailTo)
        {
            return await _purchaseOrderService.SendMailNotifyCheckAndCensor(purchaseOrderId, mailCode, mailTo).ConfigureAwait(true);
        }

        /// <summary>
        /// Tổng hợp những chi tiết của PO gia công đã/chưa phân bổ
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("AggregatePurchaseOrderOutsourcePart")]
        public async Task<IList<PurchaseOrderOutsourcePartAllocate>> GetAllPurchaseOrderOutsourcePart()
        {
            return await _purchaseOrderService.GetAllPurchaseOrderOutsourcePart();
        }

        /// <summary>
        /// Get thông tin bổ sung cho PO gia công chi tiết (mã YCGC, mã LSX)
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("{purchaseOrderId}/EnrichDataForPurchaseOrderOutsourcePart")]
        public async Task<IList<EnrichDataPurchaseOrderAllocate>> EnrichDataForPurchaseOrderOutsourcePart([FromRoute] long purchaseOrderId)
        {
            return await _purchaseOrderService.EnrichDataForPurchaseOrderAllocate(purchaseOrderId);
        }
    }
}
