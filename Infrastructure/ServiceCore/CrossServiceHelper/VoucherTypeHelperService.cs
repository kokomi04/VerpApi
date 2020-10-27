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
    }
}
