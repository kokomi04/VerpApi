using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Services.Accountancy.Service.Input;

namespace VErpApi.Controllers.Accountancy.Action
{
    [Route("api/accountancy/InputActionExec")]

    public class InputActionExecController : VErpBaseController
    {
        private readonly IInputActionExecService _inputActionExecService;
        public InputActionExecController(IInputActionExecService inputActionExecService)
        {
            _inputActionExecService = inputActionExecService;
        }

        [HttpGet]
        [Route("{inputBillId}/ActionButtons")]
        public async Task<IList<ActionButtonModel>> ActionButtons([FromRoute] int inputBillId)
        {
            return await _inputActionExecService.GetActionButtons(inputBillId).ConfigureAwait(true);
        }

        [HttpPost]
        [Route("{inputTypeId}/{inputBillId}/Exec/{actionButtonId}")]
        [ObjectDataApi(EnumObjectType.InputType, "inputTypeId")]
        [ActionButtonDataApi("actionButtonId")]
        public async Task<List<NonCamelCaseDictionary>> ExecInputAction([FromRoute] int inputTypeId, [FromRoute] int actionButtonId, [FromRoute] long inputBillId, [FromBody] BillInfoModel data)
        {
            return await _inputActionExecService.ExecActionButton(actionButtonId, inputTypeId, inputBillId, data).ConfigureAwait(true);
        }
    }
}