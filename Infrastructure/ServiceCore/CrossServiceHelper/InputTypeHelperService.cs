﻿using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ServiceCore.Service;

namespace VErp.Infrastructure.ServiceCore.CrossServiceHelper
{
    public interface IInputTypeHelperService
    {
        Task<bool> CheckReferFromCategory(ReferFromCategoryModel req);
        Task<IList<InputTypeSimpleModel>> GetInputTypeSimpleList();
        Task<IList<ObjectBillSimpleInfoModel>> GetBillNotApprovedYet(int inputTypeId);
        Task<IList<ObjectBillSimpleInfoModel>> GetBillNotChekedYet(int inputTypeId);
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

        public async Task<IList<ObjectBillSimpleInfoModel>> GetBillNotApprovedYet(int inputTypeId)
        {
            return await _httpCrossService.Get<List<ObjectBillSimpleInfoModel>>($"api/internal/InternalInput/{inputTypeId}/GetBillNotApprovedYet");
        }

        public async Task<IList<ObjectBillSimpleInfoModel>> GetBillNotChekedYet(int inputTypeId)
        {
            return await _httpCrossService.Get<List<ObjectBillSimpleInfoModel>>($"api/internal/InternalInput/{inputTypeId}/GetBillNotChekedYet");
        }
    }
}
