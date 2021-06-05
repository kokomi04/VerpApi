using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.PurchaseOrder.Model.Voucher;
using VErp.Services.PurchaseOrder.Service.Voucher;
using System.IO;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ApiCore.ModelBinders;

namespace VErpApi.Controllers.PurchaseOrder.Data
{
    [ObjectDataApi(EnumObjectType.VoucherType, "voucherTypeId")]
    [Route("api/PurchasingOrder/data/VoucherBills")]

    public class VoucherController : VErpBaseController
    {
        private readonly IVoucherDataService _voucherDataService;

        public VoucherController(IVoucherDataService voucherDataService)
        {
            _voucherDataService = voucherDataService;
        }

        [HttpPost]
        [VErpAction(EnumActionType.View)]
        [Route("{voucherTypeId}/Search")]
        public async Task<PageDataTable> GetVoucherBills([FromRoute] int voucherTypeId, [FromBody] VoucherTypeBillsRequestModel request)
        {
            if (request == null) throw new BadRequestException(GeneralCode.InvalidParams);

            return await _voucherDataService.GetVoucherBills(voucherTypeId, request.Keyword, request.Filters, request.ColumnsFilters, request.OrderBy, request.Asc, request.Page, request.Size).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("{voucherTypeId}/{fId}")]
        public async Task<PageDataTable> GetVoucherBillInfoRows([FromRoute] int voucherTypeId, [FromRoute] long fId, [FromQuery] string orderByFieldName, [FromQuery] bool asc, [FromQuery] int? page, [FromQuery] int? size)
        {
            return await _voucherDataService.GetVoucherBillInfoRows(voucherTypeId, fId, orderByFieldName, asc, page ?? 0, size ?? 0).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("{voucherTypeId}/{fId}/info")]
        public async Task<BillInfoModel> GetVoucherBillInfo([FromRoute] int voucherTypeId, [FromRoute] long fId)
        {
            return await _voucherDataService.GetVoucherBillInfo(voucherTypeId, fId).ConfigureAwait(true);
        }


        [HttpPost]
        [Route("{voucherTypeId}")]
        public async Task<long> CreateVoucherBill([FromRoute] int voucherTypeId, [FromBody] BillInfoModel data)
        {
            if (data == null) throw new BadRequestException(GeneralCode.InvalidParams);

            return await _voucherDataService.CreateVoucherBill(voucherTypeId, data).ConfigureAwait(true);
        }

        [HttpPut]
        [Route("{voucherTypeId}/{fId}")]
        public async Task<bool> UpdateVoucherBill([FromRoute] int voucherTypeId, [FromRoute] long fId, [FromBody] BillInfoModel data)
        {
            if (data == null) throw new BadRequestException(GeneralCode.InvalidParams);

            return await _voucherDataService.UpdateVoucherBill(voucherTypeId, fId, data).ConfigureAwait(true);
        }

        [HttpPut]
        [Route("{voucherTypeId}/multiple")]
        public async Task<bool> UpdateMultipleVoucherBills([FromRoute] int voucherTypeId, [FromBody] UpdateMultipleModel data)
        {
            if (data == null) throw new BadRequestException(GeneralCode.InvalidParams);
            return await _voucherDataService.UpdateMultipleVoucherBills(voucherTypeId, data.FieldName, data.OldValue, data.NewValue, data.FIds).ConfigureAwait(true);
        }

        [HttpDelete]
        [Route("{voucherTypeId}/{fId}")]
        public async Task<bool> DeleteVoucherBill([FromRoute] int voucherTypeId, [FromRoute] long fId)
        {
            return await _voucherDataService.DeleteVoucherBill(voucherTypeId, fId).ConfigureAwait(true);
        }

        [HttpPost]
        [Route("{voucherTypeId}/importFromMapping")]
        public async Task<bool> ImportVoucherBillFromMapping([FromRoute] int voucherTypeId, [FromFormString] ImportBillExelMapping mapping, IFormFile file)
        {
            if (file == null)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }
            return await _voucherDataService.ImportVoucherBillFromMapping(voucherTypeId, mapping, file.OpenReadStream()).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("{voucherTypeId}/{fId}/datafile")]
        public async Task<FileStreamResult> ExportVoucherBill([FromRoute] int voucherTypeId, [FromRoute] long fId)
        {
            var result = await _voucherDataService.ExportVoucherBill(voucherTypeId, fId);
            return new FileStreamResult(result.Stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") { FileDownloadName = result.FileName };
        }

        [HttpGet]
        [Route("{voucherTypeId}/{fId}/info/pkl/{voucherTypeBKLId}")]
        public async Task<BillInfoModel> GetPackingListInfo([FromRoute] int voucherTypeId, [FromRoute] long fId, [FromRoute] int voucherTypeBKLId)
        {
            if (voucherTypeId == 0) throw new BadRequestException(GeneralCode.InvalidParams);
            return await _voucherDataService.GetPackingListInfo(voucherTypeBKLId, fId);
        }

        [HttpGet]
        [Route("OrderDetailByPurchasingRequest")]
        public async Task<PageDataTable> OrderDetailByPurchasingRequest(
            [FromQuery] string keyword, [FromQuery] long? fromDate, [FromQuery] long? toDate, [FromQuery] bool? isCreatedPurchasingRequest,
            [FromQuery] int page, [FromQuery] int size)
        {
            return await _voucherDataService.OrderDetailByPurchasingRequest(keyword, fromDate, toDate, isCreatedPurchasingRequest, page, size);
        }

        [HttpPost("OrderDetails")]
        [VErpAction(EnumActionType.View)]
        public async Task<IList<NonCamelCaseDictionary>> OrderDetails([FromBody] IList<long> orderDetailIds)
        {
            return await _voucherDataService.OrderDetails(orderDetailIds);
        }
    }
}
