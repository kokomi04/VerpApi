using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Accountant.Model.Category;

namespace VErp.Services.Accountant.Service.Category
{
    public interface ICategoryService
    {
        Task<ServiceResult<CategoryFullModel>> GetCategory(int categoryId);
        Task<PageData<CategoryModel>> GetCategories(string keyword, int page, int size);
        Task<ServiceResult<int>> AddCategory(CategoryModel data);
        Task<Enum> UpdateCategory(int categoryId, CategoryModel data);
        Task<Enum> DeleteCategory(int categoryId);

        Task<PageData<DataTypeModel>> GetDataTypes(int page, int size);
        PageData<FormTypeModel> GetFormTypes(int page, int size);
        PageData<OperatorModel> GetOperators(int page, int size);

        PageData<LogicOperatorModel> GetLogicOperators(int page, int size);
        PageData<ModuleTypeModel> GetModuleTypes(int page, int size);
    }
}
