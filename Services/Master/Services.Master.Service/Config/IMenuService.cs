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
        Task<ICollection<MenuOutputModel>> GetList();
        
        Task<Enum> Update(int menuId, MenuInputModel model);
        
        Task<Enum> Delete(int menuId);
        
        Task<ServiceResult<int>> Create( MenuInputModel model);
    }
}
