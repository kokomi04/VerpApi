using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Services.PurchaseOrder.Model.E_Invoice;
using VErp.Services.PurchaseOrder.Service.E_Invoice;

namespace VErpApi.Controllers.PurchaseOrder.EInvoice
{
    [Route("api/PurchasingOrder/e-invoice/mapping")]
    public class EInvoiceProviderMappingController : VErpBaseController
    {
        private readonly IElectronicInvoiceMappingService _electronicInvoiceMappingService;

        public EInvoiceProviderMappingController(IElectronicInvoiceMappingService electronicInvoiceMappingService)
        {
            _electronicInvoiceMappingService = electronicInvoiceMappingService;
        }

        [Route("")]
        [HttpPost]
        public async Task<int> AddElectronicInvoiceMapping([FromBody] ElectronicInvoiceMappingModel model)
        {
            return await _electronicInvoiceMappingService.AddElectronicInvoiceMapping(model);
        }

        [Route("{electronicInvoiceMappingId}")]
        [HttpDelete]
        public async Task<bool> DeleteElectronicInvoiceMapping([FromRoute] int electronicInvoiceMappingId)
        {
            return await _electronicInvoiceMappingService.DeleteElectronicInvoiceMapping(electronicInvoiceMappingId);
        }

        [Route("{electronicInvoiceMappingId}")]
        [HttpGet]
        public async Task<ElectronicInvoiceMappingModel> GetInfo([FromRoute] int electronicInvoiceMappingId)
        {
            return await _electronicInvoiceMappingService.GetInfo(electronicInvoiceMappingId);
        }

        [Route("")]
        [HttpGet]
        public async Task<IList<ElectronicInvoiceMappingModel>> GetList()
        {
            return await _electronicInvoiceMappingService.GetList();
        }

        [Route("{electronicInvoiceMappingId}")]
        [HttpPut]
        public async Task<bool> UpdateElectronicInvoiceMapping([FromRoute] int electronicInvoiceMappingId, [FromBody] ElectronicInvoiceMappingModel model)
        {
            return await _electronicInvoiceMappingService.UpdateElectronicInvoiceMapping(electronicInvoiceMappingId, model);
        }
    }
}
