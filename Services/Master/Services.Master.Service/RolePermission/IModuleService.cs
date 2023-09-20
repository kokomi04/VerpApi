using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Library.Model;
using VErp.Services.Master.Model.RolePermission;

namespace VErp.Services.Master.Service.RolePermission
{
    public interface IModuleService
    {
        Task<IList<ModuleGroupOutput>> GetModuleGroups();
        Task<IList<ModuleOutput>> GetList();

        Task<IList<CategoryNameModel>> GetRefCategoryForQuery(EnumModuleType moduleTypeId);
    }
}
