using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Accountant.Model.Category;

namespace VErp.Services.Accountant.Service.Category
{
    public interface ICategoryRowService
    {
        Task<PageData<CategoryRowOutputModel>> GetCategoryRows(int categoryId, int page, int size);

        Task<ServiceResult<CategoryRowOutputModel>> GetCategoryRow(int categoryId, int categoryRowId);

        Task<ServiceResult<int>> AddCategoryRow(int updatedUserId, int categoryId, CategoryRowInputModel data);
     
        Task<Enum> UpdateCategoryRow(int updatedUserId, int categoryId, int categoryRowId, CategoryRowInputModel data);

        Task<Enum> DeleteCategoryRow(int updatedUserId, int categoryId, int categoryRowId);
    }
}
