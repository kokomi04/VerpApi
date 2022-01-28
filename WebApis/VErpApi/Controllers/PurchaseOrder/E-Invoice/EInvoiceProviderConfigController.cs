using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.Enums.E_Invoice;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.MasterEnum.PO;
using VErp.Commons.Enums.PO;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.PurchaseOrder.Model;
using VErp.Services.PurchaseOrder.Model.E_Invoice;
using VErp.Services.PurchaseOrder.Model.PoProviderPricing;
using VErp.Services.PurchaseOrder.Service.E_Invoice;
using VErp.Services.PurchaseOrder.Service.Po;

namespace VErpApi.Controllers.PurchaseOrder.EInvoice
{
    [Route("api/PurchasingOrder/e-invoice/providerConfig")]
    public class EInvoiceProviderConfigController : VErpBaseController
    {
        private readonly IElectronicInvoiceProviderService electronicInvoiceProviderService;

        public EInvoiceProviderConfigController(IElectronicInvoiceProviderService electronicInvoiceProviderService)
        {
            this.electronicInvoiceProviderService = electronicInvoiceProviderService;
        }

        [HttpGet("")]
        public Task<IList<ElectronicInvoiceProviderModel>> GetList()
        {
            return electronicInvoiceProviderService.GetList();
        }

        [HttpGet("{electronicInvoiceProviderId}")]
        public Task<ElectronicInvoiceProviderModel> Info([FromRoute] EnumElectronicInvoiceProvider electronicInvoiceProviderId)
        {
            return electronicInvoiceProviderService.GetInfo(electronicInvoiceProviderId);
        }

        [HttpPut("{electronicInvoiceProviderId}")]
        public Task<bool> Update([FromRoute] EnumElectronicInvoiceProvider electronicInvoiceProviderId, [FromBody] ElectronicInvoiceProviderModel model)
        {
            return electronicInvoiceProviderService.Update(electronicInvoiceProviderId, model);
        }

    }
}
