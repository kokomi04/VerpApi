using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject.InternalDataInterface.DynamicBill;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Master.Service.Config;

namespace VErpApi.Controllers.System.Internal
{
    [Route("api/internal/InternalActionButtonConfig")]

    public class InternalActionButtonConfigController : CrossServiceBaseController
    {
        private readonly IActionButtonConfigService _actionButtonConfigService;
        public InternalActionButtonConfigController(IActionButtonConfigService actionButtonConfigService)
        {
            _actionButtonConfigService = actionButtonConfigService;
        }

        [HttpGet]
        [Route("")]
        public async Task<IList<ActionButtonModel>> GetList([FromQuery] EnumObjectType billTypeObjectTypeId)
        {
            return await _actionButtonConfigService.GetActionButtonConfigs(billTypeObjectTypeId).ConfigureAwait(true);
        }


        [HttpPost("")]
        public async Task<ActionButtonModel> AddActionButton([FromBody] ActionButtonModel data, [FromQuery] string typeTitle)
        {
            return await _actionButtonConfigService.AddActionButton(data, typeTitle).ConfigureAwait(true);
        }

        [HttpPut]
        [Route("{actionButtonId}")]
        public async Task<ActionButtonModel> UpdateActionButton([FromRoute] int actionButtonId, [FromBody] ActionButtonModel data, [FromQuery] string typeTitle)
        {
            return await _actionButtonConfigService.UpdateActionButton(actionButtonId, data, typeTitle).ConfigureAwait(true);
        }

        [HttpDelete]
        [Route("{actionButtonId}")]
        public async Task<bool> DeleteActionButton([FromRoute] int actionButtonId, [FromBody] ActionButtonIdentity data, [FromQuery] string typeTitle)
        {
            return await _actionButtonConfigService.DeleteActionButton(actionButtonId, data, typeTitle).ConfigureAwait(true);
        }


        [HttpPost]
        [Route("ActionButtonBillType")]
        public async Task<int> AddActionButtonBillType([FromBody] ActionButtonBillTypeMapping data, [FromQuery] string objectTitle)
        {
            return await _actionButtonConfigService.AddActionButtonBillType(data, objectTitle).ConfigureAwait(true);
        }


        [HttpDelete]
        [Route("ActionButtonBillType")]
        public async Task<bool> RemoveActionButtonBillType([FromBody] ActionButtonBillTypeMapping data, [FromQuery] string objectTitle)
        {
            return await _actionButtonConfigService.RemoveActionButtonBillType(data, objectTitle).ConfigureAwait(true);
        }

        [HttpDelete]
        [Route("ActionButtonBillType/RemoveAllByBillType")]
        public async Task<bool> RemoveAllByBillType([FromBody] ActionButtonBillTypeMapping data, [FromQuery] string objectTitle)
        {
            return await _actionButtonConfigService.RemoveAllByBillType(data, objectTitle).ConfigureAwait(true);
        }


        [HttpDelete]
        [Route("ActionButtonBillTypes")]
        public async Task<IList<ActionButtonBillTypeMapping>> GetMappings([FromQuery] EnumObjectType billTypeObjectTypeId, [FromQuery] long? billTypeObjectId)
        {
            return await _actionButtonConfigService.GetMappings(billTypeObjectTypeId, billTypeObjectId).ConfigureAwait(true);
        }


    }
}