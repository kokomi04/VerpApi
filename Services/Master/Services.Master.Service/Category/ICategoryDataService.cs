﻿using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface.Category;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.Category;

namespace VErp.Services.Master.Service.Category
{
    public interface ICategoryDataService
    {
        Task<PageData<NonCamelCaseDictionary>> GetCategoryRows(string categoryCode, string keyword, Dictionary<int, object> filters, Clause columnsFilters, NonCamelCaseDictionary filterData, string extraFilter, ExtraFilterParam[] extraFilterParams, int page, int size, string orderBy, bool asc);
        Task<PageData<NonCamelCaseDictionary>> GetCategoryRows(int categoryId, string keyword, Dictionary<int, object> filters, Clause columnsFilters, NonCamelCaseDictionary filterData, string extraFilter, ExtraFilterParam[] extraFilterParams, int page, int size, string orderBy, bool asc);

        Task<NonCamelCaseDictionary> GetCategoryRow(int categoryId, int fId);

        Task<NonCamelCaseDictionary> GetCategoryRow(string categoryCode, int fId);

        Task<int> AddCategoryRow(int categoryId, NonCamelCaseDictionary data);

        Task<int> AddCategoryRowToDb(int categoryId, NonCamelCaseDictionary data);

        Task<int> UpdateCategoryRow(int categoryId, int fId, NonCamelCaseDictionary data, bool validateDataIsOld);

        Task<int> DeleteCategoryRow(int categoryId, int fId);

        Task<List<MapObjectOutputModel>> MapToObject(MapObjectInputModel[] categoryValues);

        Task<bool> ImportCategoryRowFromMapping(int categoryId, ImportExcelMapping mapping, Stream stream);
    }
}
