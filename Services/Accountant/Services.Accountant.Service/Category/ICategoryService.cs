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
        Task<PageData<CategoryModel>> GetCategories(string keyword, bool? isModule, bool? hasParent, int page, int size);
        Task<ServiceResult<int>> AddCategory(int updatedUserId, CategoryModel data);
        Task<Enum> UpdateCategory(int updatedUserId, int categoryId, CategoryModel data);
        Task<Enum> DeleteCategory(int updatedUserId, int categoryId);

        Task<PageData<DataTypeModel>> GetDataTypes(int page, int size);
        Task<PageData<FormTypeModel>> GetFormTypes(int page, int size);
        Task<PageData<OperatorModel>> GetOperators(int page, int size);

        Task<PageData<LogicOperatorModel>> GetLogicOperators(int page, int size);
        Task<PageData<ModuleTypeModel>> GetModuleTypes(int page, int size);
    }
}
