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
    }
}
