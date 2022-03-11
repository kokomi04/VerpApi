using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Services.Organization.Service.HrConfig;

namespace VErpApi.Controllers.PurchaseOrder.Action
{
    [Route("api/Organization/HrActionExec")]

    public class HrActionExecController : VErpBaseController
    {
        private readonly IHrActionExecService _hrActionExecService;
        public HrActionExecController(IHrActionExecService hrActionExecService)
        {
            _hrActionExecService = hrActionExecService;
        }


        [HttpGet]
        [Route("{hrTypeId}/ActionButtons")]
        public async Task<IList<ActionButtonModel>> ActionButtons([FromRoute] int hrTypeId)
        {
            return await _hrActionExecService.GetActionButtons(hrTypeId).ConfigureAwait(true);
        }

        [HttpPost]
        [Route("{hrTypeId}/{hrBillId}/Exec/{actionButtonId}")]
        [ObjectDataApi(EnumObjectType.InputType, "hrTypeId")]
        [ActionButtonDataApi("actionButtonId")]
        public async Task<List<NonCamelCaseDictionary>> ExecInputAction([FromRoute] int hrTypeId, [FromRoute] int actionButtonId, [FromRoute] long hrBillId, [FromBody] BillInfoModel data)
        {
            return await _hrActionExecService.ExecActionButton(actionButtonId, hrTypeId, hrBillId, data).ConfigureAwait(true);
        }
    }
}