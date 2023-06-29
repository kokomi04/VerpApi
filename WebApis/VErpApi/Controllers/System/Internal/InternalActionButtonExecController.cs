using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject.InternalDataInterface.DynamicBill;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Master.Service.Config;

namespace VErpApi.Controllers.System.Internal
{
    [Route("api/internal/InternalActionButtonExec")]

    public class InternalActionButtonExecController : CrossServiceBaseController
    {
        private readonly IActionButtonExecService _actionButtonExecService;
        public InternalActionButtonExecController(IActionButtonExecService actionButtonExecService)
        {
            _actionButtonExecService = actionButtonExecService;
        }

        [HttpGet]
        [Route("")]
        public async Task<IList<ActionButtonModel>> GetList([FromQuery] EnumObjectType billTypeObjectTypeId, [FromQuery] int billTypeObjectId)
        {
            return await _actionButtonExecService.GetActionButtonsByBillType(billTypeObjectTypeId, billTypeObjectId).ConfigureAwait(true);
        }


        [HttpGet]
        [Route("{actionButtonId}")]
        public async Task<ActionButtonModel> ActionButtonInfo([FromRoute] int actionButtonId, [FromQuery] EnumObjectType billTypeObjectTypeId, [FromQuery] int billTypeObjectId)
        {
            return await _actionButtonExecService.ActionButtonExecInfo(actionButtonId, billTypeObjectTypeId, billTypeObjectId).ConfigureAwait(true);
        }

    }
}