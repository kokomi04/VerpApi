using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Commons.GlobalObject;

namespace VErp.Infrastructure.ServiceCore.CrossServiceHelper
{
    public interface IInputTypeHelperService
    {
        Task<bool> CheckReferFromCategory(ReferFromCategoryModel req);
        Task<IList<InputTypeSimpleModel>> GetInputTypeSimpleList();
        Task<IList<BillSimpleInfoModel>> GetBillNotApprovedYet(int inputTypeId);
        Task<IList<BillSimpleInfoModel>> GetBillNotChekedYet(int inputTypeId);
    }
    public class InputTypeHelperService : IInputTypeHelperService
    {
        private readonly IHttpCrossService _httpCrossService;

        public InputTypeHelperService(IHttpCrossService httpCrossService)
        {
            _httpCrossService = httpCrossService;
        }

        public async Task<bool> CheckReferFromCategory(ReferFromCategoryModel req)
        {

            return await _httpCrossService.Post<bool>($"api/internal/InternalInput/CheckReferFromCategory", req);
        }
       
        public async Task<IList<InputTypeSimpleModel>> GetInputTypeSimpleList()
        {
            return await _httpCrossService.Get<List<InputTypeSimpleModel>>($"api/internal/InternalInput/simpleList");
        }

        public async Task<IList<BillSimpleInfoModel>> GetBillNotApprovedYet(int inputTypeId)
        {
            return await _httpCrossService.Get<List<BillSimpleInfoModel>>($"api/internal/InternalInput/{inputTypeId}/GetBillNotApprovedYet");
        }

        public async Task<IList<BillSimpleInfoModel>> GetBillNotChekedYet(int inputTypeId)
        {
            return await _httpCrossService.Get<List<BillSimpleInfoModel>>($"api/internal/InternalInput/{inputTypeId}/GetBillNotChekedYet");
        }
    }
}
