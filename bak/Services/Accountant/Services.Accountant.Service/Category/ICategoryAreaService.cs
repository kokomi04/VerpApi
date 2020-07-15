using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Accountant.Model.Category;

namespace VErp.Services.Accountant.Service.Category
{
    public interface ICategoryAreaService
    {
        Task<ServiceResult<CategoryAreaModel>> GetCategoryArea(int categoryId, int categoryAreaId);
        Task<PageData<CategoryAreaModel>> GetCategoryAreas(int CategoryTypeIdCategoryTypeId, string keyword, int page, int size);
        Task<ServiceResult<int>> AddCategoryArea(int CategoryTypeId, CategoryAreaInputModel data);
        Task<Enum> UpdateCategoryArea(int CategoryTypeId, int categoryAreaId, CategoryAreaInputModel data);
        Task<Enum> DeleteCategoryArea(int CategoryTypeId, int categoryAreaId);
    }
}
