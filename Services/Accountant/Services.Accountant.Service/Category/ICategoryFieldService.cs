using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Accountant.Model.Category;

namespace VErp.Services.Accountant.Service.Category
{
    public interface ICategoryFieldService
    {
        Task<PageData<CategoryFieldOutputModel>> GetCategoryFields(int categoryId, string keyword, int page, int size);

        Task<ServiceResult<CategoryFieldOutputModel>> GetCategoryField(int categoryId, int categoryFieldId);

        Task<ServiceResult<int>> AddCategoryField(int categoryId, CategoryFieldInputModel data);

        Task<Enum> UpdateCategoryField(int categoryId, int categoryFieldId, CategoryFieldInputModel data);

        Task<Enum> DeleteCategoryField(int categoryId, int categoryFieldId);

        Task<ServiceResult<int>> UpdateMultiField(int categoryId, List<CategoryFieldInputModel> fields);
    }
}
