using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Services.Accountancy.Service.Input;

namespace VErpApi.Controllers.Accountancy.Action
{   
    public abstract class InputActionConfigControllerAbstract : VErpBaseController
    {
        private readonly IActionButtonConfigHelper _actionButtonConfigHelper;
        public InputActionConfigControllerAbstract(IActionButtonConfigHelper inputActionConfigService)
        {
            _actionButtonConfigHelper = inputActionConfigService;
        }

        [HttpGet]
        [Route("")]
        public async Task<IList<ActionButtonModel>> GetList()
        {
            return await _actionButtonConfigHelper.GetActionButtonConfigs().ConfigureAwait(true);
        }

        [HttpPost]
        [Route("")]
        public async Task<ActionButtonModel> Create([FromBody] ActionButtonUpdateModel model)
        {
            return await _actionButtonConfigHelper.AddActionButton(model).ConfigureAwait(true);
        }

        [HttpPut]
        [Route("{actionButtonId}")]
        public async Task<ActionButtonModel> Update([FromRoute] int actionButtonId, [FromBody] ActionButtonUpdateModel model)
        {
            return await _actionButtonConfigHelper.UpdateActionButton(actionButtonId, model).ConfigureAwait(true);
        }

        [HttpDelete]
        [Route("{actionButtonId}")]
        public async Task<bool> Delete([FromRoute] int actionButtonId)
        {
            return await _actionButtonConfigHelper.DeleteActionButton(actionButtonId).ConfigureAwait(true);
        }

        [HttpPost]
        [Route("Mapping/{inputTypeId}")]
        public async Task<int> AddMapping([FromRoute] int inputTypeId, [FromBody] ActionButtonBillTypeMappingModel model)
        {
            return await _actionButtonConfigHelper.AddActionButtonBillType(model.ActionButtonId, inputTypeId).ConfigureAwait(true);
        }

        [HttpDelete]
        [Route("Mapping/{inputTypeId}")]
        public async Task<bool> RemoveMapping([FromRoute] int inputTypeId, [FromBody] ActionButtonBillTypeMappingModel model)
        {
            return await _actionButtonConfigHelper.RemoveActionButtonBillType(model.ActionButtonId, inputTypeId, "").ConfigureAwait(true);
        }

    }


    [Route("api/accountancy/InputActionConfig")]

    public class InputActionConfigController : InputActionConfigControllerAbstract
    {
        public InputActionConfigController(IInputPrivateActionConfigService inputActionConfigService) : base(inputActionConfigService)
        {
        }
    }

    [Route("api/accountancy/public/InputActionConfig")]

    public class InputPublicActionConfigController : InputActionConfigControllerAbstract
    {
        public InputPublicActionConfigController(IInputPublicActionConfigService inputActionConfigService) : base(inputActionConfigService)
        {
        }
    }
}