using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.Config;

namespace VErp.Services.Master.Service.Config
{
    public interface IMenuService
    {
        Task<ICollection<MenuOutputModel>> GetMeMenuList();

        Task<ICollection<MenuOutputModel>> GetList();
        
        Task<bool> Update(int menuId, MenuInputModel model);
        
        Task<bool> Delete(int menuId);
        
        Task<int> Create(MenuInputModel model);
    }
}
