using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ApiCore;
using VErp.Services.PurchaseOrder.Service.Voucher;

namespace VErpApi.Controllers.PurchaseOrder.Action
{
    [Route("api/PurchasingOrder/VoucherActionConfig")]

    public class VoucherActionConfigController : VErpBaseController
    {
        private readonly IVoucherActionConfigService _voucherActionConfigService;
        public VoucherActionConfigController(IVoucherActionConfigService voucherActionConfigService)
        {
            _voucherActionConfigService = voucherActionConfigService;
        }

        [HttpGet]
        [Route("")]
        public async Task<IList<ActionButtonModel>> GetList()
        {
            return await _voucherActionConfigService.GetActionButtonConfigs().ConfigureAwait(true);
        }

        [HttpPost]
        [Route("")]
        public async Task<ActionButtonModel> Create([FromBody] ActionButtonUpdateModel model)
        {
            return await _voucherActionConfigService.AddActionButton(model).ConfigureAwait(true);
        }

        [HttpPut]
        [Route("{actionButtonId}")]
        public async Task<ActionButtonModel> Update([FromRoute] int actionButtonId, [FromBody] ActionButtonUpdateModel model)
        {
            return await _voucherActionConfigService.UpdateActionButton(actionButtonId, model).ConfigureAwait(true);
        }

        [HttpDelete]
        [Route("{actionButtonId}")]
        public async Task<bool> Delete([FromRoute] int actionButtonId)
        {
            return await _voucherActionConfigService.DeleteActionButton(actionButtonId).ConfigureAwait(true);
        }

        [HttpPost]
        [Route("Mapping/{voucherTypeId}")]
        public async Task<int> AddActionButtonBillType([FromRoute] int voucherTypeId, [FromBody] ActionButtonBillTypeMappingModel model)
        {
            return await _voucherActionConfigService.AddActionButtonBillType(model.ActionButtonId, voucherTypeId).ConfigureAwait(true);
        }

        [HttpDelete]
        [Route("Mapping/{voucherTypeId}")]
        public async Task<bool> RemoveActionButtonBillType([FromRoute] int voucherTypeId, [FromBody] ActionButtonBillTypeMappingModel model)
        {
            return await _voucherActionConfigService.RemoveActionButtonBillType(model.ActionButtonId, voucherTypeId, "").ConfigureAwait(true);
        }

    }
}