using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Services.PurchaseOrder.Model.E_Invoice;

namespace VErp.Services.PurchaseOrder.Service.E_Invoice
{
    public interface IElectronicInvoiceMappingService
    {
        Task<int> AddElectronicInvoiceMapping(ElectronicInvoiceMappingModel model);
        Task<bool> DeleteElectronicInvoiceMapping(int electronicInvoiceMappingId);
        Task<ElectronicInvoiceMappingModel> GetInfo(int electronicInvoiceMappingId);
        Task<IList<ElectronicInvoiceMappingModel>> GetList();
        Task<bool> UpdateElectronicInvoiceMapping(int electronicInvoiceMappingId, ElectronicInvoiceMappingModel model);
    }
}