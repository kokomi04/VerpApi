using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface.DynamicBill;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper.General;
using VErp.Services.Accountancy.Service.Input;

namespace VErpApi.Controllers.Accountancy.Action
{


    public abstract class InputActionExecControllerAbstract : VErpBaseController
    {
        private readonly IActionButtonExecHelper _actionButtonExecHelper;
        public InputActionExecControllerAbstract(IActionButtonExecHelper actionButtonExecHelper)
        {
            _actionButtonExecHelper = actionButtonExecHelper;
        }

        [HttpGet]
        [Route("{inputBillId}/ActionButtons")]
        public async Task<IList<ActionButtonModel>> ActionButtons([FromRoute] int inputBillId)
        {
            return await _actionButtonExecHelper.GetActionButtons(inputBillId).ConfigureAwait(true);
        }

        [HttpPost]
        [Route("{inputTypeId}/{inputBillId}/Exec/{actionButtonId}")]       
        [ActionButtonDataApi("actionButtonId")]
        public async Task<List<NonCamelCaseDictionary>> ExecInputAction([FromRoute] int inputTypeId, [FromRoute] int actionButtonId, [FromRoute] long inputBillId, [FromBody] BillInfoModel data)
        {
            return await _actionButtonExecHelper.ExecActionButton(actionButtonId, inputTypeId, inputBillId, data).ConfigureAwait(true);
        }
    }

    [Route("api/accountancy/InputActionExec")]
    [ObjectDataApi(EnumObjectType.InputType, "inputTypeId")]
    public class InputActionExecController : InputActionExecControllerAbstract
    {
        public InputActionExecController(IInputPrivateActionExecService actionButtonExecHelper) : base(actionButtonExecHelper)
        {
        }
    }


    [Route("api/accountancy/public/InputActionExec")]
    [ObjectDataApi(EnumObjectType.InputTypePublic, "inputTypeId")]
    public class InputPublicActionExecController : InputActionExecControllerAbstract
    {
        public InputPublicActionExecController(IInputPublicActionExecService actionButtonExecHelper) : base(actionButtonExecHelper)
        {
        }
    }
}