using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ServiceCore.Service;
using VErp.Commons.GlobalObject;

namespace VErp.Infrastructure.ServiceCore.CrossServiceHelper
{
    public interface ICategoryHelperService
    {
        Task<bool> CheckReferFromCategory(string categoryCode, IList<string> fieldNames = null, NonCamelCaseDictionary categoryRow = null);
        Task<List<ReferFieldModel>> GetReferFields(IList<string> categoryCodes, IList<string> fieldNames);
    }
    public class CategoryHelperService : ICategoryHelperService
    {
        private readonly IHttpCrossService _httpCrossService;

        public CategoryHelperService(IHttpCrossService httpCrossService)
        {
            _httpCrossService = httpCrossService;
        }
      
        public async Task<bool> CheckReferFromCategory(string categoryCode, IList<string> fieldNames = null, NonCamelCaseDictionary categoryRow = null)
        {
            ReferFromCategoryModel data = new ReferFromCategoryModel
            {
                CategoryCode = categoryCode,
                FieldNames = fieldNames,
                CategoryRow = categoryRow
            };
            return await _httpCrossService.Post<bool>($"api/internal/InternalInput/CheckReferFromCategory", data)
                || await _httpCrossService.Post<bool>($"api/internal/InternalVoucher/CheckReferFromCategory", data);
        }

        public async Task<List<ReferFieldModel>> GetReferFields(IList<string> categoryCodes, IList<string> fieldNames)
        {
            ReferInputModel data = new ReferInputModel
            {
                CategoryCodes = categoryCodes,
                FieldNames = fieldNames
            };
            return await _httpCrossService.Post<List<ReferFieldModel>>($"api/internal/InternalCategory/ReferFields", data);
        }
    }
}
