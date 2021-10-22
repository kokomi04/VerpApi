using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.E_Invoice;
using VErp.Services.PurchaseOrder.Model.E_Invoice;

namespace VErp.Services.PurchaseOrder.Service.E_Invoice
{
    public interface IElectronicInvoiceProviderService
    {
        Task<IList<ElectronicInvoiceProviderModel>> GetList();
        Task<ElectronicInvoiceProviderModel> GetInfo(EnumElectronicInvoiceProvider electronicInvoiceProviderId);
        Task<bool> Update(EnumElectronicInvoiceProvider electronicInvoiceProviderId, ElectronicInvoiceProviderModel model);
    }
}
