using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ApiCore;
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
        [Route("InputType/{inputTypeId}")]
        public async Task<IList<InputActionModel>> GetList(int inputTypeId)
        {
            return await _inputActionService.GetInputActions(inputTypeId).ConfigureAwait(true);
        }

        [HttpPost]
        public async Task<InputActionModel> InputTypeGroupCreate([FromBody] InputActionModel model)
        {
            return await _inputActionService.AddInputAction(model).ConfigureAwait(true);
        }

        [HttpPut]
        [Route("{inputActionId}")]
        public async Task<InputActionModel> UpdateInputAction([FromRoute] int inputActionId, [FromBody] InputActionModel model)
        {
            return await _inputActionService.UpdateInputAction(inputActionId, model).ConfigureAwait(true);
        }

        [HttpDelete]
        [Route("{inputActionId}")]
        public async Task<bool> DeleteInputAction([FromRoute] int inputActionId)
        {
            return await _inputActionService.DeleteInputAction(inputActionId).ConfigureAwait(true);
        }

        [HttpPost]
        [Route("ExecInputAction/{inputActionId}")]
        public async Task<List<NonCamelCaseDictionary>> ExecInputAction([FromRoute] int inputActionId, [FromBody] BillInfoModel data)
        {
            return await _inputActionService.ExecInputAction(inputActionId, data).ConfigureAwait(true);
        }
    }
}