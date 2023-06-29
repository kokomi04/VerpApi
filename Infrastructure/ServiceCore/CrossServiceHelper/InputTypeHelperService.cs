using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject.InternalDataInterface.DynamicBill;
using VErp.Infrastructure.ServiceCore.Service;

namespace VErp.Infrastructure.ServiceCore.CrossServiceHelper
{
    public interface IInputTypeHelperServiceBase
    {
        Task<bool> CheckReferFromCategory(ReferFromCategoryModel req);
        Task<IList<InputTypeSimpleModel>> GetInputTypeSimpleList();
        Task<IList<ObjectBillSimpleInfoModel>> GetBillNotApprovedYet(int inputTypeId);
        Task<IList<ObjectBillSimpleInfoModel>> GetBillNotChekedYet(int inputTypeId);
    }

    public interface IInputPrivateTypeHelperService: IInputTypeHelperServiceBase
    {
       
    }

    public interface IInputPublicTypeHelperService : IInputTypeHelperServiceBase
    {

    }

    public class InputPrivateTypeHelperService : IInputPrivateTypeHelperService
    {
        private readonly IHttpCrossService _httpCrossService;

        public InputPrivateTypeHelperService(IHttpCrossService httpCrossService)
        {
            _httpCrossService = httpCrossService;
        }

        public async Task<bool> CheckReferFromCategory(ReferFromCategoryModel req)
        {

            return await _httpCrossService.Post<bool>($"api/internal/InternalInputPrivate/CheckReferFromCategory", req);
        }

        public async Task<IList<InputTypeSimpleModel>> GetInputTypeSimpleList()
        {
            return await _httpCrossService.Get<List<InputTypeSimpleModel>>($"api/internal/InternalInputPrivate/simpleList");
        }

        public async Task<IList<ObjectBillSimpleInfoModel>> GetBillNotApprovedYet(int inputTypeId)
        {
            return await _httpCrossService.Get<List<ObjectBillSimpleInfoModel>>($"api/internal/InternalInputPrivate/{inputTypeId}/GetBillNotApprovedYet");
        }

        public async Task<IList<ObjectBillSimpleInfoModel>> GetBillNotChekedYet(int inputTypeId)
        {
            return await _httpCrossService.Get<List<ObjectBillSimpleInfoModel>>($"api/internal/InternalInputPrivate/{inputTypeId}/GetBillNotChekedYet");
        }
    }


    public class InputPublicTypeHelperService : IInputPublicTypeHelperService
    {
        private readonly IHttpCrossService _httpCrossService;

        public InputPublicTypeHelperService(IHttpCrossService httpCrossService)
        {
            _httpCrossService = httpCrossService;
        }

        public async Task<bool> CheckReferFromCategory(ReferFromCategoryModel req)
        {

            return await _httpCrossService.Post<bool>($"api/internal/InternalInputPublic/CheckReferFromCategory", req);
        }

        public async Task<IList<InputTypeSimpleModel>> GetInputTypeSimpleList()
        {
            return await _httpCrossService.Get<List<InputTypeSimpleModel>>($"api/internal/InternalInputPublic/simpleList");
        }

        public async Task<IList<ObjectBillSimpleInfoModel>> GetBillNotApprovedYet(int inputTypeId)
        {
            return await _httpCrossService.Get<List<ObjectBillSimpleInfoModel>>($"api/internal/InternalInputPublic/{inputTypeId}/GetBillNotApprovedYet");
        }

        public async Task<IList<ObjectBillSimpleInfoModel>> GetBillNotChekedYet(int inputTypeId)
        {
            return await _httpCrossService.Get<List<ObjectBillSimpleInfoModel>>($"api/internal/InternalInputPublic/{inputTypeId}/GetBillNotChekedYet");
        }
    }
}
