using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model;
using VErp.Services.Master.Model.Category;
using VErp.Services.Master.Model.CategoryConfig;

namespace VErp.Services.Master.Service.Category
{
    public interface ICategoryConfigService
    {
        Task<int> GetCategoryIdByCode(string categoryCode);
        Task<CategoryFullModel> GetCategory(int categoryId);
        Task<CategoryFullModel> GetCategory(string categoryCode);
        Task<PageData<CategoryModel>> GetCategories(string keyword, int page, int size);
        Task<int> AddCategory(CategoryModel data);
        Task<bool> UpdateCategory(int categoryId, CategoryModel data);
        Task<bool> DeleteCategory(int categoryId);

        Task<CategoryNameModel> GetFieldDataForMapping(int categoryId);
        PageData<DataTypeModel> GetDataTypes(int page, int size);
        PageData<FormTypeModel> GetFormTypes(int page, int size);
        PageData<OperatorModel> GetOperators(int page, int size);
        PageData<LogicOperatorModel> GetLogicOperators(int page, int size);
        PageData<ModuleTypeModel> GetModuleTypes(int page, int size);

        Task<List<CategoryFieldReferModel>> GetCategoryFieldsByCodes(string[] categoryCodes);
        Task<PageData<CategoryFieldModel>> GetCategoryFieldsByCode(string categoryCode, string keyword, int page, int size);
        Task<PageData<CategoryFieldModel>> GetCategoryFields(int categoryId, string keyword, int page, int size);
        Task<List<CategoryFieldModel>> GetCategoryFields(IList<int> categoryIds);
        Task<CategoryFieldModel> GetCategoryField(int categoryId, int categoryFieldId);
        Task<bool> DeleteCategoryField(int categoryId, int categoryFieldId);
        Task<bool> UpdateMultiField(int categoryId, List<CategoryFieldModel> fields);

        Task<List<ReferFieldModel>> GetReferFields(IList<string> categoryCodes, IList<string> fieldNames);
    }
}
