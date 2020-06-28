using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Accountancy.Model;
using VErp.Services.Accountancy.Model.Category;
using VErp.Services.Accountancy.Model.Data;

namespace VErp.Services.Accountancy.Service.Category
{
    public interface ICategoryDataService
    {
        Task<PageData<NonCamelCaseDictionary>> GetCategoryRows(string categoryCode, string keyword, string filters, int page, int size);

        Task<ServiceResult<NonCamelCaseDictionary>> GetCategoryRow(string categoryCode, int fId);

        Task<ServiceResult<int>> AddCategoryRow(string categoryCode, Dictionary<string, string> data);

        Task<int> UpdateCategoryRow(string categoryCode, int fId, Dictionary<string, string> data);

        Task<int> DeleteCategoryRow(string categoryCode, int fId);

        Task<List<MapObjectOutputModel>> MapToObject(MapObjectInputModel[] categoryValues);
    }
}
