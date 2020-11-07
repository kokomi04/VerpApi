using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model;
using VErp.Services.Master.Model.Category;
using VErp.Services.Master.Model.CategoryConfig;

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
