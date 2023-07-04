using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject.InternalDataInterface.DynamicBill;

namespace VErp.Services.Master.Service.Config
{
    public interface IActionButtonExecService
    {
        Task<IList<ActionButtonModel>> GetActionButtonsByBillType(EnumObjectType billTypeObjectTypeId, long billTypeObjectId);

        Task<ActionButtonModel> ActionButtonExecInfo(int actionButtonId, EnumObjectType billTypeObjectTypeId, long billTypeObjectId);

    }

}
