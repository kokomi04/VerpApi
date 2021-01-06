using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject.InternalDataInterface;

namespace VErp.Services.Master.Service.Config
{
    public interface IActionButtonService
    {
        Task<IList<ActionButtonModel>> GetActionButtonConfigs(EnumObjectType objectTypeId, int? objectId);

        Task<IList<ActionButtonSimpleModel>> GetActionButtons(EnumObjectType objectTypeId, int objectId);

        Task<ActionButtonModel> AddActionButton(ActionButtonModel data);

        Task<ActionButtonModel> UpdateActionButton(int actionButtonId, ActionButtonModel data);

        Task<ActionButtonModel> ActionButtonInfo(int actionButtonId, EnumObjectType objectTypeId, int objectId);

        Task<bool> DeleteActionButtonsByType(ActionButtonIdentity data);

        Task<bool> DeleteActionButton(int actionButtonId, ActionButtonIdentity data);
    }

}
