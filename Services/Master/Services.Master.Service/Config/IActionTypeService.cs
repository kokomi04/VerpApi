using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Infrastructure.EF.MasterDB;

namespace VErp.Services.Master.Service.Config
{
    public interface IActionTypeService
    {
        Task<IList<ActionType>> GetList();      

        Task<bool> Update(int actionTypeId, ActionType model);

        Task<bool> Delete(int actionTypeId);

        Task<int> Create(ActionType model);
    }
}
