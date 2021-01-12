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
using VErp.Services.PurchaseOrder.Model.Voucher;
using VErp.Services.PurchaseOrder.Service.Voucher;

namespace VErpApi.Controllers.PurchaseOrder.Action
{
    [Route("api/PurchasingOrder/VoucherAction")]

    public class VoucherActionController : VErpBaseController
    {
        private readonly IVoucherActionService _voucherActionService;
        public VoucherActionController(IVoucherActionService voucherActionService)
        {
            _voucherActionService = voucherActionService;
        }

        [HttpGet]
        [Route("{inputTypeId}")]
        public async Task<IList<ActionButtonModel>> GetList([FromRoute] int inputTypeId)
        {
            return await _voucherActionService.GetActionButtonConfigs(inputTypeId).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("{inputTypeId}/use")]
        public async Task<IList<ActionButtonSimpleModel>> GetListUse([FromRoute] int inputTypeId)
        {
            return await _voucherActionService.GetActionButtons(inputTypeId).ConfigureAwait(true);
        }

        [HttpPost]
        [Route("{inputTypeId}")]
        public async Task<ActionButtonModel> InputTypeGroupCreate([FromRoute] int inputTypeId, [FromBody] ActionButtonModel model)
        {
            return await _voucherActionService.AddActionButton(inputTypeId, model).ConfigureAwait(true);
        }

        [HttpPut]
        [Route("{inputTypeId}/{actionButtonId}")]
        public async Task<ActionButtonModel> UpdateInputAction([FromRoute] int inputTypeId, [FromRoute] int actionButtonId, [FromBody] ActionButtonModel model)
        {
            return await _voucherActionService.UpdateActionButton(inputTypeId, actionButtonId, model).ConfigureAwait(true);
        }

        [HttpDelete]
        [Route("{inputTypeId}/{actionButtonId}")]
        public async Task<bool> DeleteInputAction([FromRoute] int inputTypeId, [FromRoute] int actionButtonId)
        {
            return await _voucherActionService.DeleteActionButton(inputTypeId, actionButtonId).ConfigureAwait(true);
        }

        [HttpPost]
        [Route("{voucherTypeId}/{voucherBillId}/Exec/{voucherActionId}")]
        [ObjectDataApi(EnumObjectType.VoucherType, "voucherTypeId")]
        [ActionButtonDataApi("voucherActionId")]
        public async Task<List<NonCamelCaseDictionary>> ExecVoucherAction([FromRoute] int voucherTypeId, [FromRoute] int voucherActionId, [FromRoute] long voucherBillId, [FromBody] BillInfoModel data)
        {
            return await _voucherActionService.ExecActionButton(voucherTypeId, voucherActionId, voucherBillId, data).ConfigureAwait(true);
        }
    }
}