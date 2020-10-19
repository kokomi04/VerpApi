using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.PurchaseOrder.Model.Voucher;

namespace VErp.Services.PurchaseOrder.Service.Voucher
{
    public interface IVoucherActionService
    {
        Task<IList<VoucherActionListModel>> GetVoucherActions(int voucherTypeId);
        Task<VoucherActionModel> GetVoucherAction(int voucherActionId);
        Task<VoucherActionModel> AddVoucherAction(VoucherActionModel data);
        Task<VoucherActionModel> UpdateVoucherAction(int voucherActionId, VoucherActionModel data);
        Task<bool> DeleteVoucherAction(int voucherActionId);


    }
}
