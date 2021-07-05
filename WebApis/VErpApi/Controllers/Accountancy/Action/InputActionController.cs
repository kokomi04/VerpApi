using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Accountancy.Model.Data;
using VErp.Services.Accountancy.Model.Input;
using VErp.Services.Accountancy.Service.Input;
using VErp.Services.Accountancy.Service.Input.Implement;

namespace VErpApi.Controllers.Accountancy.Action
{
    [Route("api/accountancy/InputAction")]

    public class InputActionController : VErpBaseController
    {
        private readonly IInputActionService _inputActionService;
        public InputActionController(IInputActionService inputActionService)
        {
            _inputActionService = inputActionService;
        }

        [HttpGet]
        [Route("{inputTypeId}")]
        public async Task<IList<ActionButtonModel>> GetList([FromRoute] int inputTypeId)
        {
            return await _inputActionService.GetActionButtonConfigs(inputTypeId).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("{inputTypeId}/use")]
        public async Task<IList<ActionButtonSimpleModel>> GetListUse([FromRoute] int inputTypeId)
        {
            return await _inputActionService.GetActionButtons(inputTypeId).ConfigureAwait(true);
        }

        [HttpPost]
        [Route("{inputTypeId}")]
        public async Task<ActionButtonModel> InputTypeGroupCreate([FromRoute] int inputTypeId, [FromBody] ActionButtonModel model)
        {
            return await _inputActionService.AddActionButton(inputTypeId, model).ConfigureAwait(true);
        }

        [HttpPut]
        [Route("{inputTypeId}/{actionButtonId}")]
        public async Task<ActionButtonModel> UpdateInputAction([FromRoute] int inputTypeId, [FromRoute] int actionButtonId, [FromBody] ActionButtonModel model)
        {
            return await _inputActionService.UpdateActionButton(inputTypeId, actionButtonId, model).ConfigureAwait(true);
        }

        [HttpDelete]
        [Route("{inputTypeId}/{actionButtonId}")]
        public async Task<bool> DeleteInputAction([FromRoute] int inputTypeId, [FromRoute] int actionButtonId)
        {
            return await _inputActionService.DeleteActionButton(inputTypeId, actionButtonId).ConfigureAwait(true);
        }

        [HttpPost]
        [Route("{inputTypeId}/{inputBillId}/Exec/{inputActionId}")]
        [ObjectDataApi(EnumObjectType.InputType, "inputTypeId")]
        [ActionButtonDataApi("inputActionId")]
        public async Task<List<NonCamelCaseDictionary>> ExecInputAction([FromRoute] int inputTypeId, [FromRoute] int inputActionId, [FromRoute] long inputBillId, [FromBody] BillInfoModel data)
        {
            return await _inputActionService.ExecActionButton(inputTypeId, inputActionId, inputBillId, data).ConfigureAwait(true);
        }
    }
}