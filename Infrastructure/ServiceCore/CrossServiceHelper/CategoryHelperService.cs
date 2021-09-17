using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Commons.GlobalObject.InternalDataInterface.Category;

namespace VErp.Infrastructure.ServiceCore.CrossServiceHelper
{
    public interface ICategoryHelperService
    {
        Task<bool> CheckReferFromCategory(string categoryCode, IList<string> fieldNames = null, NonCamelCaseDictionary categoryRow = null);
        Task<List<ReferFieldModel>> GetReferFields(IList<string> categoryCodes, IList<string> fieldNames);
        Task<PageData<NonCamelCaseDictionary>> GetDataRows(string categoryCode, CategoryFilterModel request);
        
        Task<IList<CategoryListModel>> GetDynamicCates();
    }
    public class CategoryHelperService : ICategoryHelperService
    {
        private readonly IHttpCrossService _httpCrossService;
        private readonly IInputTypeHelperService _inputTypeHelperService;
        private readonly IVoucherTypeHelperService _voucherTypeHelperService;

        public CategoryHelperService(IHttpCrossService httpCrossService, IInputTypeHelperService inputTypeHelperService, IVoucherTypeHelperService voucherTypeHelperService)
        {
            _httpCrossService = httpCrossService;
            _inputTypeHelperService = inputTypeHelperService;
            _voucherTypeHelperService = voucherTypeHelperService;
        }

        public async Task<bool> CheckReferFromCategory(string categoryCode, IList<string> fieldNames = null, NonCamelCaseDictionary categoryRow = null)
        {
            var data = new ReferFromCategoryModel
            {
                CategoryCode = categoryCode,
                FieldNames = fieldNames,
                CategoryRow = categoryRow
            };
            return await _inputTypeHelperService.CheckReferFromCategory(data)
                || await _voucherTypeHelperService.CheckReferFromCategory(data);
        }

        public async Task<List<ReferFieldModel>> GetReferFields(IList<string> categoryCodes, IList<string> fieldNames)
        {
            var data = new ReferInputModel
            {
                CategoryCodes = categoryCodes,
                FieldNames = fieldNames
            };
            return await _httpCrossService.Post<List<ReferFieldModel>>($"api/internal/InternalCategory/ReferFields", data);
        }

        public async Task<IList<CategoryListModel>> GetDynamicCates()
        {
            return await _httpCrossService.Get<List<CategoryListModel>>($"api/internal/InternalCategory/DynamicCates");
        }

        public async Task<PageData<NonCamelCaseDictionary>> GetDataRows(string categoryCode, CategoryFilterModel request)
        {
            return await _httpCrossService.Post<PageData<NonCamelCaseDictionary>>($"api/internal/InternalCategory/{categoryCode}/data/Search", request);
        }
    }
}
