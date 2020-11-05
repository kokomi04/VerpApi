using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject;
using VErp.Services.Accountancy.Model.Data;
using VErp.Services.Accountancy.Model.Input;

namespace VErp.Services.Accountancy.Service.Input
{
    public interface IInputActionService
    {
        Task<IList<InputActionModel>> GetInputActions(int inputTypeId);
        Task<InputActionModel> AddInputAction(InputActionModel data);
        Task<InputActionModel> UpdateInputAction(int inputActionId, InputActionModel data);
        Task<bool> DeleteInputAction(int inputActionId);

        Task<List<NonCamelCaseDictionary>> ExecInputAction(int inputTypeId, int inputActionId, long inputBillId, BillInfoModel data);
    }
}
