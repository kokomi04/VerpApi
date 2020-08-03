using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Accountancy.Model;
using VErp.Services.Accountancy.Model.Category;

namespace VErp.Services.Accountancy.Service.Category
{
    public interface ICategoryConfigService
    {
        Task<int> GetCategoryIdByCode(string categoryCode);
        Task<ServiceResult<CategoryFullModel>> GetCategory(int categoryId);
        Task<ServiceResult<CategoryFullModel>> GetCategory(string categoryCode);
        Task<PageData<CategoryModel>> GetCategories(string keyword, int page, int size);
        Task<ServiceResult<int>> AddCategory(CategoryModel data);
        Task<Enum> UpdateCategory(int categoryId, CategoryModel data);
        Task<Enum> DeleteCategory(int categoryId);

        Task<CategoryNameModel> GetFieldDataForMapping(int categoryId);

        PageData<DataTypeModel> GetDataTypes(int page, int size);
        PageData<FormTypeModel> GetFormTypes(int page, int size);
        PageData<OperatorModel> GetOperators(int page, int size);

        PageData<LogicOperatorModel> GetLogicOperators(int page, int size);
        PageData<ModuleTypeModel> GetModuleTypes(int page, int size);


        //Task<ServiceResult<CategoryAreaModel>> GetCategoryArea(int categoryId, int categoryAreaId);
        //Task<PageData<CategoryAreaModel>> GetCategoryAreas(int CategoryTypeIdCategoryTypeId, string keyword, int page, int size);
        //Task<ServiceResult<int>> AddCategoryArea(int CategoryTypeId, CategoryAreaInputModel data);
        //Task<Enum> UpdateCategoryArea(int CategoryTypeId, int categoryAreaId, CategoryAreaInputModel data);
        //Task<Enum> DeleteCategoryArea(int CategoryTypeId, int categoryAreaId);

        Task<List<CategoryFieldReferModel>> GetCategoryFieldsByCodes(string[] categoryCodes);
        Task<PageData<CategoryFieldModel>> GetCategoryFieldsByCode(string categoryCode, string keyword, int page, int size);
        Task<PageData<CategoryFieldModel>> GetCategoryFields(int categoryId, string keyword, int page, int size);
        Task<List<CategoryFieldModel>> GetCategoryFields(IList<int> categoryIds);
        Task<ServiceResult<CategoryFieldModel>> GetCategoryField(int categoryId, int categoryFieldId);
        Task<Enum> DeleteCategoryField(int categoryId, int categoryFieldId);
        Task<ServiceResult<int>> UpdateMultiField(int categoryId, List<CategoryFieldModel> fields);
    }
}
