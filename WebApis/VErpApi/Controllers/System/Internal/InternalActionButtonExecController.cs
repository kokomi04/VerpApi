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