using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Services.Organization.Service.HrConfig;

namespace VErpApi.Controllers.System
{
    [Route("api/organization/config/hrAction")]
    public class HrActionController : VErpBaseController
    {
        public readonly IHrActionService _hrActionService;

        public HrActionController(IHrActionService hrActionService)
        {
            _hrActionService = hrActionService;
        }

        [HttpGet]
        [Route("{hrTypeId}")]
        public async Task<IList<ActionButtonModel>> GetList([FromRoute] int hrTypeId)
        {
            return await _hrActionService.GetActionButtonConfigs(hrTypeId).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("{hrTypeId}/use")]
        public async Task<IList<ActionButtonSimpleModel>> GetListUse([FromRoute] int hrTypeId)
        {
            return await _hrActionService.GetActionButtons(hrTypeId).ConfigureAwait(true);
        }

        [HttpPost]
        [Route("{hrTypeId}")]
        public async Task<ActionButtonModel> HrTypeGroupCreate([FromRoute] int hrTypeId, [FromBody] ActionButtonModel model)
        {
            return await _hrActionService.AddActionButton(hrTypeId, model).ConfigureAwait(true);
        }

        [HttpPut]
        [Route("{hrTypeId}/{actionButtonId}")]
        public async Task<ActionButtonModel> UpdateHrAction([FromRoute] int hrTypeId, [FromRoute] int actionButtonId, [FromBody] ActionButtonModel model)
        {
            return await _hrActionService.UpdateActionButton(hrTypeId, actionButtonId, model).ConfigureAwait(true);
        }

        [HttpDelete]
        [Route("{hrTypeId}/{actionButtonId}")]
        public async Task<bool> DeleteHrAction([FromRoute] int hrTypeId, [FromRoute] int actionButtonId)
        {
            return await _hrActionService.DeleteActionButton(hrTypeId, actionButtonId).ConfigureAwait(true);
        }

        [HttpPost]
        [Route("{hrTypeId}/{hrBillId}/Exec/{hrActionId}")]
        [ObjectDataApi(EnumObjectType.HrType, "hrTypeId")]
        [ActionButtonDataApi("hrActionId")]
        public async Task<List<NonCamelCaseDictionary>> ExecHrAction([FromRoute] int hrTypeId, [FromRoute] int hrActionId, [FromRoute] long hrBillId, [FromBody] BillInfoModel data)
        {
            return await _hrActionService.ExecActionButton(hrTypeId, hrActionId, hrBillId, data).ConfigureAwait(true);
        }
    }
}