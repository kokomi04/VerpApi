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
        [Route("create")]
        public async Task<CreateElectronicInvoiceSuccess> CreateElectronicInvoice([FromQuery] string pattern, [FromQuery] string serial, [FromQuery] long voucherTypeId, [FromBody] IEnumerable<NonCamelCaseDictionary> data)
        {
            return await _easyInvoiceProviderService.CreateElectronicInvoice(pattern, serial, voucherTypeId, data);
        }

        [HttpGet]
        [Route("viewPdf")]
        public async Task<IActionResult> GetElectronicInvoicePdf([FromQuery] string ikey, [FromQuery] string pattern, [FromQuery] int option)
        {
            var (stream, fileName, contentType) = await _easyInvoiceProviderService.GetElectronicInvoicePdf(ikey, pattern, option);

            return new FileStreamResult(stream, !string.IsNullOrWhiteSpace(contentType) ? contentType : "application/octet-stream") { FileDownloadName = fileName };
        }

        [HttpPut]
        [Route("modify")]
        public async Task<ModifyElectronicInvoiceSuccess> ModifyElectronicInvoice([FromQuery] string ikey, [FromQuery] string pattern, [FromQuery] string serial, [FromQuery] long voucherTypeId, [FromBody] IEnumerable<NonCamelCaseDictionary> data)
        {
            return await _easyInvoiceProviderService.ModifyElectronicInvoice(ikey, pattern, serial, voucherTypeId, data);
        }

        [HttpPut]
        [Route("publish")]
        public async Task<PublishElectronicInvoiceSuccess> PublishElectronicInvoice([FromBody] IList<string> ikeys, [FromQuery] string pattern, [FromQuery] string serial, [FromQuery] string signature)
        {
            return await _easyInvoiceProviderService.PublishElectronicInvoice(ikeys, pattern, serial, signature);
        }

        [HttpPut]
        [Route("publishTemp")]
        public async Task<PublishElectronicInvoiceSuccess> PublishTempElectronicInvoice([FromBody] IList<string> ikeys, [FromQuery] string pattern, [FromQuery] string serial, [FromQuery] string certString)
        {
            return await _easyInvoiceProviderService.PublishTempElectronicInvoice(ikeys, pattern, serial, certString);
        }

    }
}
