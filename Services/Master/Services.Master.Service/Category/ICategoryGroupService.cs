using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Services.Master.Model.Category;

namespace VErp.Services.Master.Service.Category
{
    public interface ICategoryGroupService
    {
        Task<IList<CategoryGroupModel>> GetList();
        Task<bool> Delete(int categoryGroupId);
        Task<int> Add(CategoryGroupModel model);
        Task<bool> Update(int categoryGroupId, CategoryGroupModel model);
    }
}
