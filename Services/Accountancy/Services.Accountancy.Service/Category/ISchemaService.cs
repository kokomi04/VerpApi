using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Accountancy.Model;
using VErp.Services.Accountancy.Model.Category;

namespace VErp.Services.Accountancy.Service.Category
{
    public interface ISchemaService
    {
        Task<ServiceResult<CategoryFullModel>> GetCategory(int categoryId);
        Task<PageData<CategoryModel>> GetCategories(string keyword, int page, int size);
        Task<ServiceResult<int>> AddCategory(CategoryModel data);
        Task<Enum> UpdateCategory(int categoryId, CategoryModel data);
        Task<Enum> DeleteCategory(int categoryId);

        //Task<ServiceResult<CategoryAreaModel>> GetCategoryArea(int categoryId, int categoryAreaId);
        //Task<PageData<CategoryAreaModel>> GetCategoryAreas(int CategoryTypeIdCategoryTypeId, string keyword, int page, int size);
        //Task<ServiceResult<int>> AddCategoryArea(int CategoryTypeId, CategoryAreaInputModel data);
        //Task<Enum> UpdateCategoryArea(int CategoryTypeId, int categoryAreaId, CategoryAreaInputModel data);
        //Task<Enum> DeleteCategoryArea(int CategoryTypeId, int categoryAreaId);

        Task<PageData<CategoryFieldOutputModel>> GetCategoryFields(int categoryId, string keyword, int page, int size);
        Task<List<CategoryFieldOutputModel>> GetCategoryFields(IList<int> categoryIds);
        Task<ServiceResult<CategoryFieldOutputModel>> GetCategoryField(int categoryId, int categoryFieldId);
        Task<Enum> DeleteCategoryField(int categoryId, int categoryFieldId);
        Task<ServiceResult<int>> UpdateMultiField(int categoryId, List<CategoryFieldInputModel> fields);
    }
}
