using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Services.Master.Model.Config;

namespace VErp.Services.Master.Service.Config
{
    public interface IMenuService
    {
        Task<ICollection<MenuOutputModel>> GetMeMenuList();

        Task<ICollection<MenuOutputModel>> GetList();
        Task<MenuOutputModel> Get(int menuId);

        Task<bool> Update(int menuId, MenuInputModel model);

        Task<bool> Delete(int menuId);

        Task<int> Create(MenuInputModel model);
    }
}
