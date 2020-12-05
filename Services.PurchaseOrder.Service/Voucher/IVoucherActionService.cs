using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.PurchaseOrder.Model.Voucher;

namespace VErp.Services.PurchaseOrder.Service.Voucher
{
    public interface IVoucherActionService
    {
        Task<IList<VoucherActionModel>> GetVoucherActionConfigs(int voucherTypeId);
        Task<IList<VoucherActionUseModel>> GetVoucherActions(int voucherTypeId);
        Task<VoucherActionModel> AddVoucherAction(VoucherActionModel data);
        Task<VoucherActionModel> UpdateVoucherAction(int voucherActionId, VoucherActionModel data);
        Task<bool> DeleteVoucherAction(int voucherActionId);

        Task<List<NonCamelCaseDictionary>> ExecVoucherAction(int voucherTypeId, int voucherActionId, long voucherBillId, VoucherBillInfoModel data);
    }
}
