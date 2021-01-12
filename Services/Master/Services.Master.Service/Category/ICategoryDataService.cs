using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model;
using VErp.Services.Master.Model.Category;

namespace VErp.Services.Master.Service.Category
{
    public interface ICategoryDataService
    {
        Task<PageData<NonCamelCaseDictionary>> GetCategoryRows(int categoryId, string keyword, string filters, string extraFilter, ExtraFilterParam[] extraFilterParams, int page, int size);

        Task<NonCamelCaseDictionary> GetCategoryRow(int categoryId, int fId);

        Task<NonCamelCaseDictionary> GetCategoryRow(string categoryCode, int fId);

        Task<int> AddCategoryRow(int categoryId, Dictionary<string, string> data);

        Task<int> UpdateCategoryRow(int categoryId, int fId, Dictionary<string, string> data);

        Task<int> DeleteCategoryRow(int categoryId, int fId);

        Task<List<MapObjectOutputModel>> MapToObject(MapObjectInputModel[] categoryValues);

        Task<bool> ImportCategoryRowFromMapping(int categoryId, CategoryImportExelMapping mapping, Stream stream);
    }
}
