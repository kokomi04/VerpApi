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
        Task<PageData<NonCamelCaseDictionary>> GetCategoryRows(int categoryId, string keyword, string filters, int page, int size);

        Task<ServiceResult<NonCamelCaseDictionary>> GetCategoryRow(int categoryId, int fId);

        Task<ServiceResult<int>> AddCategoryRow(int categoryId, Dictionary<string, string> data);

        Task<int> UpdateCategoryRow(int categoryId, int fId, Dictionary<string, string> data);

        Task<int> DeleteCategoryRow(int categoryId, int fId);

        Task<List<MapObjectOutputModel>> MapToObject(MapObjectInputModel[] categoryValues);
    }
}
