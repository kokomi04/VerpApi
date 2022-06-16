using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Services.PurchaseOrder.Service.E_Invoice.Implement;

namespace VErpApi.Controllers.PurchaseOrder.EInvoice
{
    [Route("api/PurchasingOrder/e-invoice/provider")]
    public class EInvoiceProviderController : VErpBaseController
    {
        private readonly IEasyInvoiceProviderService _easyInvoiceProviderService;

        public EInvoiceProviderController(IEasyInvoiceProviderService easyInvoiceProviderService)
        {
            _easyInvoiceProviderService = easyInvoiceProviderService;
        }


        [HttpPost]
        [Route("issue")]
        public async Task<bool> CreateElectronicInvoice([FromQuery] long voucherTypeId, [FromQuery] long voucherBillId)
        {
            return await _easyInvoiceProviderService.IssueElectronicInvoice(voucherTypeId, voucherBillId);
        }

        [HttpGet]
        [Route("viewPdf")]
        public async Task<IActionResult> GetElectronicInvoicePdf([FromQuery] long voucherTypeId, [FromQuery] long voucherBillId, [FromQuery] int option)
        {
            var (stream, fileName, contentType) = await _easyInvoiceProviderService.GetElectronicInvoicePdf(voucherTypeId, voucherBillId, option);

            return new FileStreamResult(stream, !string.IsNullOrWhiteSpace(contentType) ? contentType : "application/octet-stream") { FileDownloadName = fileName };
        }

        [HttpPut]
        [Route("cancel")]
        public async Task<bool> CancelElectronicInvoice([FromQuery] long voucherTypeId, [FromQuery] long voucherBillId)
        {
            return await _easyInvoiceProviderService.CancelElectronicInvoice(voucherTypeId, voucherBillId);
        }

    }
}
