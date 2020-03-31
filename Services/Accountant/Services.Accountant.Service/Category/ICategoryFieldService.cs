using System;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Accountant.Model.Category;

namespace VErp.Services.Accountant.Service.Category
{
    public interface ICategoryFieldService
    {
        Task<PageData<CategoryFieldOutputModel>> GetCategoryFields(int categoryId, string keyword, int page, int size, bool? isFull);

        Task<ServiceResult<int>> AddCategoryField(int updatedUserId, CategoryFieldInputModel data);
    }
}
