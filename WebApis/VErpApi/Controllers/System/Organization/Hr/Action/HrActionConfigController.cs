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
using VErp.Services.Accountancy.Model.Data;
using VErp.Services.Accountancy.Model.Input;
using VErp.Services.Accountancy.Service.Input;
using VErp.Services.Accountancy.Service.Input.Implement;
using VErp.Services.Organization.Service.HrConfig;
using VErp.Services.PurchaseOrder.Service.Voucher;

namespace VErpApi.Controllers.System.Organization.Hr.Action
{
    [Route("api/Organization/HrActionConfig")]

    public class HrActionConfigController : VErpBaseController
    {
        private readonly IHrActionConfigService _hrActionConfigService;
        public HrActionConfigController(IHrActionConfigService hrActionConfigService)
        {
            _hrActionConfigService = hrActionConfigService;
        }

        [HttpGet]
        [Route("")]
        public async Task<IList<ActionButtonModel>> GetList()
        {
            return await _hrActionConfigService.GetActionButtonConfigs().ConfigureAwait(true);
        }

        [HttpPost]
        [Route("")]
        public async Task<ActionButtonModel> Create([FromBody] ActionButtonUpdateModel model)
        {
            return await _hrActionConfigService.AddActionButton(model).ConfigureAwait(true);
        }

        [HttpPut]
        [Route("{actionButtonId}")]
        public async Task<ActionButtonModel> Update([FromRoute] int actionButtonId, [FromBody] ActionButtonUpdateModel model)
        {
            return await _hrActionConfigService.UpdateActionButton(actionButtonId, model).ConfigureAwait(true);
        }

        [HttpDelete]
        [Route("{actionButtonId}")]
        public async Task<bool> Delete([FromRoute] int actionButtonId)
        {
            return await _hrActionConfigService.DeleteActionButton(actionButtonId).ConfigureAwait(true);
        }

        [HttpPost]
        [Route("Mapping/{hrTypeId}")]
        public async Task<int> AddActionButtonBillType([FromRoute] int hrTypeId, [FromBody] ActionButtonBillTypeMappingModel model)
        {
            return await _hrActionConfigService.AddActionButtonBillType(model.ActionButtonId, hrTypeId).ConfigureAwait(true);
        }

        [HttpDelete]
        [Route("Mapping/{hrTypeId}")]
        public async Task<bool> RemoveActionButtonBillType([FromRoute] int hrTypeId, [FromBody] ActionButtonBillTypeMappingModel model)
        {
            return await _hrActionConfigService.RemoveActionButtonBillType(model.ActionButtonId, hrTypeId, "").ConfigureAwait(true);
        }

    }
}