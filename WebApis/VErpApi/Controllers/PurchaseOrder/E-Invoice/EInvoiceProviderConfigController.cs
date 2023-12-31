﻿using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.E_Invoice;
using VErp.Infrastructure.ApiCore;
using VErp.Services.PurchaseOrder.Model.E_Invoice;
using VErp.Services.PurchaseOrder.Service.E_Invoice;

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
