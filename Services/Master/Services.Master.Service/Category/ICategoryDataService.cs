using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model;
using VErp.Services.Master.Model.Category;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Commons.Library.Model;
using VErp.Commons.GlobalObject.InternalDataInterface.Category;

namespace VErp.Services.Master.Service.Category
{
    public interface ICategoryDataService
    {
        Task<PageData<NonCamelCaseDictionary>> GetCategoryRows(string categoryCode, string keyword, Clause filters, string extraFilter, ExtraFilterParam[] extraFilterParams, int page, int size, string orderBy, bool asc);
        Task<PageData<NonCamelCaseDictionary>> GetCategoryRows(int categoryId, string keyword, Clause filters, string extraFilter, ExtraFilterParam[] extraFilterParams, int page, int size, string orderBy, bool asc);

        Task<NonCamelCaseDictionary> GetCategoryRow(int categoryId, int fId);

        Task<NonCamelCaseDictionary> GetCategoryRow(string categoryCode, int fId);

        Task<int> AddCategoryRow(int categoryId, Dictionary<string, string> data);

        Task<int> UpdateCategoryRow(int categoryId, int fId, Dictionary<string, string> data);

        Task<int> DeleteCategoryRow(int categoryId, int fId);

        Task<List<MapObjectOutputModel>> MapToObject(MapObjectInputModel[] categoryValues);

        Task<bool> ImportCategoryRowFromMapping(int categoryId, ImportExcelMapping mapping, Stream stream);
    }
}
