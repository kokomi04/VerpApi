using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject.InternalDataInterface;

namespace VErp.Services.Master.Service.Config
{
    public interface IActionButtonConfigService
    {
        Task<IList<ActionButtonModel>> GetActionButtonConfigs(EnumObjectType billTypeObjectTypeId);
        Task<ActionButtonModel> AddActionButton(ActionButtonModel data, string typeTitle);
        Task<ActionButtonModel> UpdateActionButton(int actionButtonId, ActionButtonModel data, string typeTitle);
        Task<bool> DeleteActionButton(int actionButtonId, ActionButtonIdentity data, string typeTitle);
        Task<int> AddActionButtonBillType(ActionButtonBillTypeMapping data, string objectTitle);
        Task<bool> RemoveActionButtonBillType(ActionButtonBillTypeMapping data, string objectTitle);
        Task<bool> RemoveAllByBillType(ActionButtonBillTypeMapping data, string objectTitle);

        Task<IList<ActionButtonBillTypeMapping>> GetMappings(EnumObjectType billTypeObjectTypeId, long? billTypeObjectId);
    }

}
