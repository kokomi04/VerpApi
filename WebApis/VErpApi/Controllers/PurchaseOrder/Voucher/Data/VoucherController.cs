using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ApiCore.ModelBinders;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.PurchaseOrder.Model.Voucher;
using VErp.Services.PurchaseOrder.Service.Voucher;
using VErp.Services.PurchaseOrder.Service.Voucher.Implement.Facade;

namespace VErpApi.Controllers.PurchaseOrder.Data
{
    [ObjectDataApi(EnumObjectType.VoucherType, "voucherTypeId")]
    [Route("api/PurchasingOrder/data/VoucherBills")]

    public class VoucherController : VErpBaseController
    {
        private readonly IVoucherDataService _voucherDataService;
        private readonly IVoucherDataExportFacadeService _voucherDataExportFacadeService;

        public VoucherController(IVoucherDataService voucherDataService, IVoucherDataExportFacadeService voucherDataExportFacadeService)
        {
            _voucherDataService = voucherDataService;
            _voucherDataExportFacadeService = voucherDataExportFacadeService;
        }

        [HttpPost]
        [VErpAction(EnumActionType.View)]
        [Route("{voucherTypeId}/Search")]
        public async Task<PageDataTable> GetVoucherBills([FromRoute] int voucherTypeId, [FromBody] VoucherTypeBillsFilterPagingModel request)
        {
            if (request == null) throw new BadRequestException(GeneralCode.InvalidParams);

            return await _voucherDataService.GetVoucherBills(voucherTypeId, request.IsMultirow, request.FromDate, request.ToDate, request.Keyword, request.Filters, request.ColumnsFilters, request.OrderBy, request.Asc, request.Page, request.Size).ConfigureAwait(true);
        }

        [HttpPost]
        [VErpAction(EnumActionType.View)]
        [Route("{voucherTypeId}/Export")]
        public async Task<IActionResult> ExportList([FromRoute] int voucherTypeId, [FromBody] VoucherTypeBillsExportModel req)
        {
            if (req == null)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }
            var (stream, fileName, contentType) = await _voucherDataExportFacadeService.Export(voucherTypeId, req);

            return new FileStreamResult(stream, !string.IsNullOrWhiteSpace(contentType) ? contentType : "application/octet-stream") { FileDownloadName = fileName };
        }

        [HttpGet]
        [Route("{voucherTypeId}/{fId}")]
        public async Task<PageDataTable> GetVoucherBillInfoRows([FromRoute] int voucherTypeId, [FromRoute] long fId, [FromQuery] string orderByFieldName, [FromQuery] bool asc, [FromQuery] int? page, [FromQuery] int? size)
        {
            return await _voucherDataService.GetVoucherBillInfoRows(voucherTypeId, fId, orderByFieldName, asc, page ?? 0, size ?? 0).ConfigureAwait(true);
        }
        [HttpPost]
        [VErpAction(EnumActionType.View)]
        [Route("{voucherTypeId}/getByListIds")]
        public async Task<IDictionary<long, BillInfoModel>> GetListVoucherBillInfoRows([FromRoute] int voucherTypeId, [FromBody] IList<long> fIds)
        {
            return await _voucherDataService.GetListVoucherBillInfoRows(voucherTypeId, fIds).ConfigureAwait(true);
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
            return await _voucherDataService.UpdateMultipleVoucherBills(voucherTypeId, data.FieldName, data.OldValue, data.NewValue, data.BillIds, data.DetailIds).ConfigureAwait(true);
        }

        [HttpDelete]
        [Route("{voucherTypeId}/{fId}")]
        public async Task<bool> DeleteVoucherBill([FromRoute] int voucherTypeId, [FromRoute] long fId)
        {
            return await _voucherDataService.DeleteVoucherBill(voucherTypeId, fId).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("{voucherTypeId}/fieldDataForMapping")]
        public async Task<CategoryNameModel> GetFieldDataForMapping([FromRoute] int voucherTypeId, [FromQuery] int? areaId = null, [FromQuery] bool? isExport=null)
        {
            return await _voucherDataService.GetFieldDataForMapping(voucherTypeId, areaId, isExport);
        }


        [HttpPost]
        [Route("{voucherTypeId}/importFromMapping")]
        public async Task<bool> ImportVoucherBillFromMapping([FromRoute] int voucherTypeId, [FromFormString] ImportExcelMapping mapping, IFormFile file)
        {
            if (file == null)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }
            return await _voucherDataService.ImportVoucherBillFromMapping(voucherTypeId, mapping, file.OpenReadStream()).ConfigureAwait(true);
        }

        [HttpPost]
        [Route("{voucherTypeId}/parseExcelFromMapping")]
        [VErpAction(EnumActionType.View)]
        public async Task<BillInfoModel> ParseBillFromMapping([FromRoute] int voucherTypeId, [FromFormString] BillParseMapping parseMapping, IFormFile file)
        {
            if (file == null)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }
            return await _voucherDataService.ParseBillFromMapping(voucherTypeId, parseMapping, file.OpenReadStream()).ConfigureAwait(true);
        }


        [HttpGet]
        [Route("{voucherTypeId}/{fId}/datafile")]
        public async Task<FileStreamResult> ExportVoucherBill([FromRoute] int voucherTypeId, [FromRoute] long fId)
        {
            var result = await _voucherDataService.ExportVoucherBill(voucherTypeId, fId);
            return new FileStreamResult(result.Stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") { FileDownloadName = result.FileName };
        }

        [HttpGet]
        [Route("{invoiceVoucherTypeId}/{invoiceId}/info/pkl/{voucherTypeId}")]
        public async Task<BillInfoModel> GetPackingListInfo([FromRoute] int invoiceVoucherTypeId, [FromRoute] long invoiceId, [FromRoute] int voucherTypeId)
        {
            var packingListVoucherTypeId = voucherTypeId;
            if (invoiceVoucherTypeId == 0) throw new BadRequestException(GeneralCode.InvalidParams);
            return await _voucherDataService.GetPackingListInfo(packingListVoucherTypeId, invoiceId);
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


        [HttpPost("OrderByCodes")]
        [VErpAction(EnumActionType.View)]
        public async Task<IList<VoucherOrderDetailSimpleModel>> OrderByCodes([FromBody] IList<string> orderCodes)
        {
            return await _voucherDataService.OrderByCodes(orderCodes);
        }

        [HttpPost("OrderRowsByCodes")]
        [VErpAction(EnumActionType.View)]
        [GlobalApi]
        public async Task<IList<NonCamelCaseDictionary>> OrderRowsByCodes([FromBody] IList<string> orderCodes)
        {
            return await _voucherDataService.OrderRowsByCodes(orderCodes);
        }

        [HttpGet]
        [Route("{voucherTypeId}/GetBillNotApprovedYet")]
        public async Task<IList<ObjectBillSimpleInfoModel>> GetBillNotApprovedYet([FromRoute] int voucherTypeId)
        {
            return await _voucherDataService.GetBillNotApprovedYet(voucherTypeId);
        }

        [HttpGet]
        [Route("{voucherTypeId}/GetBillNotChekedYet")]
        public async Task<IList<ObjectBillSimpleInfoModel>> GetBillNotChekedYet([FromRoute] int voucherTypeId)
        {
            return await _voucherDataService.GetBillNotChekedYet(voucherTypeId);
        }

        [HttpPut]
        [Route("{voucherTypeId}/CheckAllBillInList")]
        public async Task<bool> CheckAllBillInList([FromRoute] int voucherTypeId, [FromBody] IList<ObjectBillSimpleInfoModel> models)
        {
            return await _voucherDataService.CheckAllBillInList(models);
        }

        [HttpPut]
        [Route("{voucherTypeId}/ApproveAllBillInList")]
        public async Task<bool> ApproveAllBillInList([FromRoute] int voucherTypeId, [FromBody] IList<ObjectBillSimpleInfoModel> models)
        {
            return await _voucherDataService.ApproveAllBillInList(models);
        }
    }
}
