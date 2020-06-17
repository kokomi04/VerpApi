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
        Task<ServiceResult<int>> AddCategory(CategoryModel data);
        Task<Enum> UpdateCategory(int categoryId, CategoryModel data);
        Task<Enum> DeleteCategory(int categoryId);
    }
}
