using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Services.Master.Model.RolePermission;

namespace VErp.Services.Master.Service.RolePermission
{
    public interface IModuleService
    {
        Task<IList<ModuleGroupOutput>> GetModuleGroups();
        Task<IList<ModuleOutput>> GetList();        
    }
}
