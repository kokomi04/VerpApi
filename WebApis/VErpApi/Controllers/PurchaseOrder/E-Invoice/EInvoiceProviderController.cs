using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ApiCore;
using VErp.Services.PurchaseOrder.Model.E_Invoice;
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
        public async Task<bool> CreateElectronicInvoice([FromQuery] string pattern, [FromQuery] string serial, [FromQuery] long voucherTypeId, [FromQuery] long voucherBillId, [FromBody] IEnumerable<NonCamelCaseDictionary> data)
        {
            return await _easyInvoiceProviderService.IssueElectronicInvoice(pattern, serial, voucherTypeId, voucherBillId, data);
        }

        [HttpGet]
        [Route("viewPdf")]
        public async Task<IActionResult> GetElectronicInvoicePdf([FromQuery] string ikey, [FromQuery] string pattern, [FromQuery] string serial, [FromQuery] int option)
        {
            var (stream, fileName, contentType) = await _easyInvoiceProviderService.GetElectronicInvoicePdf(ikey, pattern, serial, option);

            return new FileStreamResult(stream, !string.IsNullOrWhiteSpace(contentType) ? contentType : "application/octet-stream") { FileDownloadName = fileName };
        }

        [HttpPut]
        [Route("adjust")]
        public async Task<bool> ModifyElectronicInvoice([FromQuery] long voucherBillId, [FromQuery] string ikey, [FromQuery] string pattern, [FromQuery] string serial, [FromQuery] long voucherTypeId, [FromBody] IEnumerable<NonCamelCaseDictionary> data)
        {
            return await _easyInvoiceProviderService.ModifyElectronicInvoice(voucherBillId, ikey, pattern, serial, voucherTypeId, data);
        }

        [HttpPut]
        [Route("cancel")]
        public async Task<bool> CancelElectronicInvoice([FromQuery] long voucherBillId, [FromQuery] string ikey, [FromQuery] string pattern, [FromQuery] string serial)
        {
            return await _easyInvoiceProviderService.CancelElectronicInvoice(voucherBillId, ikey, pattern, serial);
        }

        [HttpPut]
        [Route("replace")]
        public async Task<bool> ReplaceElectronicInvoice([FromQuery] long voucherBillId, [FromQuery] string ikey, [FromQuery] string pattern, [FromQuery] string serial, [FromQuery] long voucherTypeId, [FromBody] IEnumerable<NonCamelCaseDictionary> data)
        {
            return await _easyInvoiceProviderService.ReplaceElectronicInvoice(voucherBillId, ikey, pattern, serial, voucherTypeId, data);
        }

    }
}
