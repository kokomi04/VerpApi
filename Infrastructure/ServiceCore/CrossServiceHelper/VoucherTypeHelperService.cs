using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Commons.GlobalObject;

namespace VErp.Infrastructure.ServiceCore.CrossServiceHelper
{
    public interface IVoucherTypeHelperService
    {
        Task<bool> CheckReferFromCategory(ReferFromCategoryModel req);
        Task<IList<VoucherTypeSimpleModel>> GetVoucherTypeSimpleList();
        Task<IList<VoucherOrderDetailSimpleModel>> OrderByCodes(IList<string> orderCodes);
        Task<IList<ObjectBillSimpleInfoModel>> GetBillNotApprovedYet(int voucherTypeId);
        Task<IList<ObjectBillSimpleInfoModel>> GetBillNotChekedYet(int voucherTypeId);
    }
    public class VoucherTypeHelperService : IVoucherTypeHelperService
    {
        private readonly IHttpCrossService _httpCrossService;

        public VoucherTypeHelperService(IHttpCrossService httpCrossService)
        {
            _httpCrossService = httpCrossService;
        }

        public async Task<bool> CheckReferFromCategory(ReferFromCategoryModel req)
        {

            return await _httpCrossService.Post<bool>($"api/internal/InternalVoucher/CheckReferFromCategory", req);
        }
       
        public async Task<IList<VoucherTypeSimpleModel>> GetVoucherTypeSimpleList()
        {
            return await _httpCrossService.Get<List<VoucherTypeSimpleModel>>($"api/internal/InternalVoucher/simpleList");
        }

        public async Task<IList<VoucherOrderDetailSimpleModel>> OrderByCodes(IList<string> orderCodes)
        {
            return await _httpCrossService.Post<IList<VoucherOrderDetailSimpleModel>>($"api/internal/InternalVoucher/OrderByCodes", orderCodes);
        }

        public async Task<IList<ObjectBillSimpleInfoModel>> GetBillNotApprovedYet(int voucherTypeId)
        {
            return await _httpCrossService.Get<List<ObjectBillSimpleInfoModel>>($"api/internal/InternalVoucher/{voucherTypeId}/GetBillNotApprovedYet");
        }

        public async Task<IList<ObjectBillSimpleInfoModel>> GetBillNotChekedYet(int voucherTypeId)
        {
            return await _httpCrossService.Get<List<ObjectBillSimpleInfoModel>>($"api/internal/InternalVoucher/{voucherTypeId}/GetBillNotChekedYet");
        }
    }
}
