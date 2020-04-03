using System;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Accountant.Model.Category;

namespace VErp.Services.Accountant.Service.Category
{
    public interface ICategoryFieldService
    {
        Task<PageData<CategoryFieldInputModel>> GetCategoryFields(int categoryId, string keyword, int page, int size, bool? isFull);

        Task<ServiceResult<int>> AddCategoryField(int updatedUserId, CategoryFieldInputModel data);

        Task<Enum> UpdateCategoryField(int updatedUserId, int categoryFieldId, CategoryFieldInputModel data);

        Task<Enum> DeleteCategoryField(int updatedUserId, int categoryFieldId);
    }
}
