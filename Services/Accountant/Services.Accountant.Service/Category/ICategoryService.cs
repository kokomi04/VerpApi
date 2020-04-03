using System;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Accountant.Model.Category;

namespace VErp.Services.Accountant.Service.Category
{
    public interface ICategoryService
    {
        Task<ServiceResult<CategoryFullModel>> GetCategory(int categoryId);
        Task<PageData<CategoryModel>> GetCategories(string keyword, bool? isModule, int page, int size);
        Task<ServiceResult<int>> AddCategory(int updatedUserId, CategoryModel data);
        Task<Enum> UpdateCategory(int updatedUserId, int categoryId, CategoryModel data);
        Task<Enum> DeleteCategory(int updatedUserId, int categoryId);
    }
}
