using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Accountant.Model.Category;

namespace VErp.Services.Accountant.Service.Category
{
    public interface ICategoryValueService
    {
        Task<PageData<CategoryReferenceValueModel>> GetReferenceValues(int categoryId, int categoryFieldId, string keyword, FilterModel[] filters, int page, int size);

        Task<PageData<CategoryValueModel>> GetDefaultCategoryValues(int categoryId, int categoryFieldId, string keyword, int page, int size);

        Task<ServiceResult<CategoryValueModel>> GetDefaultCategoryValue(int categoryId, int categoryFieldId, int categoryValueId);

        Task<ServiceResult<int>> AddDefaultCategoryValue(int updatedUserId, int categoryId, int categoryFieldId, CategoryValueModel data);

        Task<Enum> UpdateDefaultCategoryValue(int updatedUserId, int categoryId, int categoryFieldId, int categoryValueId, CategoryValueModel data);

        Task<Enum> DeleteDefaultCategoryValue(int updatedUserId, int categoryId, int categoryFieldId, int categoryValueId);
    }
}
