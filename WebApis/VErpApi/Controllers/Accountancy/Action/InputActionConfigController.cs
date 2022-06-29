using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Accountancy.Service.Input;

namespace VErpApi.Controllers.Accountancy.Action
{
    [Route("api/accountancy/InputActionConfig")]

    public class InputActionConfigController : VErpBaseController
    {
        private readonly IInputActionConfigService _inputActionConfigService;
        public InputActionConfigController(IInputActionConfigService inputActionConfigService)
        {
            _inputActionConfigService = inputActionConfigService;
        }

        [HttpGet]
        [Route("")]
        public async Task<IList<ActionButtonModel>> GetList()
        {
            return await _inputActionConfigService.GetActionButtonConfigs().ConfigureAwait(true);
        }

        [HttpPost]
        [Route("")]
        public async Task<ActionButtonModel> Create([FromBody] ActionButtonUpdateModel model)
        {
            return await _inputActionConfigService.AddActionButton(model).ConfigureAwait(true);
        }

        [HttpPut]
        [Route("{actionButtonId}")]
        public async Task<ActionButtonModel> Update([FromRoute] int actionButtonId, [FromBody] ActionButtonUpdateModel model)
        {
            return await _inputActionConfigService.UpdateActionButton(actionButtonId, model).ConfigureAwait(true);
        }

        [HttpDelete]
        [Route("{actionButtonId}")]
        public async Task<bool> Delete([FromRoute] int actionButtonId)
        {
            return await _inputActionConfigService.DeleteActionButton(actionButtonId).ConfigureAwait(true);
        }

        [HttpPost]
        [Route("Mapping/{inputTypeId}")]
        public async Task<int> AddMapping([FromRoute] int inputTypeId, [FromBody] ActionButtonBillTypeMappingModel model)
        {
            return await _inputActionConfigService.AddActionButtonBillType(model.ActionButtonId, inputTypeId).ConfigureAwait(true);
        }

        [HttpDelete]
        [Route("Mapping/{inputTypeId}")]
        public async Task<bool> RemoveMapping([FromRoute] int inputTypeId, [FromBody] ActionButtonBillTypeMappingModel model)
        {
            return await _inputActionConfigService.RemoveActionButtonBillType(model.ActionButtonId, inputTypeId, "").ConfigureAwait(true);
        }

    }
}