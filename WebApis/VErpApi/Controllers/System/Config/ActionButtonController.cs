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

namespace VErpApi.Controllers.System.Config
{
    [Route("api/actionButton")]

    public class ActionButtonController : VErpBaseController
    {
        private readonly IActionButtonService _actionButtonService;
        public ActionButtonController(IActionButtonService actionButtonService)
        {
            _actionButtonService = actionButtonService;
        }

      

        [HttpGet]
        [GlobalApi]
        [Route("SimpleList")]
        public async Task<IList<ActionButtonSimpleModel>> SimpleList([FromQuery] EnumObjectType objectTypeId, [FromQuery] int objectId)
        {
            return await _actionButtonService.GetActionButtons(objectTypeId, objectId).ConfigureAwait(true);
        }
       
    }
}