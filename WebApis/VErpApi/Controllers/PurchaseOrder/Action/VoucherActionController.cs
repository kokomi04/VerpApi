using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.PurchaseOrder.Model.Voucher;
using VErp.Services.PurchaseOrder.Service.Voucher;

namespace VErpApi.Controllers.PurchaseOrder.Config
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
        [Route("voucherType/{voucherTypeId}")]
        public async Task<IList<VoucherActionListModel>> GetList(int voucherTypeId)
        {
            return await _voucherActionService.GetVoucherActions(voucherTypeId).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("{voucherActionId}")]
        public async Task<VoucherActionModel> GetVoucherAction(int voucherActionId)
        {
            return await _voucherActionService.GetVoucherAction(voucherActionId).ConfigureAwait(true);
        }

        [HttpPost]
        public async Task<VoucherActionModel> VoucherTypeGroupCreate([FromBody] VoucherActionModel model)
        {
            return await _voucherActionService.AddVoucherAction(model).ConfigureAwait(true);
        }

        [HttpPut]
        [Route("{voucherActionId}")]
        public async Task<VoucherActionModel> UpdateVoucherAction([FromRoute] int voucherActionId, [FromBody] VoucherActionModel model)
        {
            return await _voucherActionService.UpdateVoucherAction(voucherActionId, model).ConfigureAwait(true);
        }

        [HttpDelete]
        [Route("{voucherActionId}")]
        public async Task<bool> DeleteVoucherAction([FromRoute] int voucherActionId)
        {
            return await _voucherActionService.DeleteVoucherAction(voucherActionId).ConfigureAwait(true);
        }

        
    }
}