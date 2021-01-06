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
using VErp.Services.Master.Service.Config;
using VErp.Services.PurchaseOrder.Model.Voucher;
using VErp.Services.PurchaseOrder.Service.Voucher;

namespace VErpApi.Controllers.System.Internal
{
    [Route("api/internal/InternalActionButton")]

    public class InternalActionButtonController : CrossServiceBaseController
    {
        private readonly IActionButtonService _actionButtonService;
        public InternalActionButtonController(IActionButtonService actionButtonService)
        {
            _actionButtonService = actionButtonService;
        }

        [HttpGet]
        [Route("Configs")]
        public async Task<IList<ActionButtonModel>> GetList([FromQuery] EnumObjectType objectTypeId, [FromQuery] int? objectId)
        {
            return await _actionButtonService.GetActionButtonConfigs(objectTypeId, objectId).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("{actionButtonId}")]
        public async Task<ActionButtonModel> ActionButtonInfo([FromRoute] int actionButtonId, [FromQuery] EnumObjectType objectTypeId, [FromQuery] int objectId)
        {
            return await _actionButtonService.ActionButtonInfo(actionButtonId, objectTypeId, objectId).ConfigureAwait(true);
        }

        [HttpPost("")]
        public async Task<ActionButtonModel> AddActionButton([FromBody] ActionButtonModel data)
        {
            return await _actionButtonService.AddActionButton(data).ConfigureAwait(true);
        }

        [HttpPut]
        [Route("{actionButtonId}")]
        public async Task<ActionButtonModel> UpdateActionButton([FromRoute] int actionButtonId, [FromBody] ActionButtonModel data)
        {
            return await _actionButtonService.UpdateActionButton(actionButtonId, data).ConfigureAwait(true);
        }

        [HttpDelete]
        [Route("DeleteByType")]
        public async Task<bool> DeleteActionButtonsByType([FromBody] ActionButtonIdentity data)
        {
            return await _actionButtonService.DeleteActionButtonsByType(data).ConfigureAwait(true);
        }

        [HttpDelete]
        [Route("{actionButtonId}")]
        public async Task<bool> DeleteActionButton([FromRoute] int actionButtonId, [FromBody] ActionButtonIdentity data)
        {
            return await _actionButtonService.DeleteActionButton(actionButtonId, data).ConfigureAwait(true);
        }

    }
}