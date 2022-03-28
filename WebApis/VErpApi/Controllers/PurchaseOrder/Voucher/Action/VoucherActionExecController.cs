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
using VErp.Services.Master.Service.Category;
using VErp.Services.PurchaseOrder.Service.Voucher;

namespace VErpApi.Controllers.PurchaseOrder.Action
{
    [Route("api/PurchasingOrder/VoucherActionExec")]

    public class VoucherActionExecController : VErpBaseController
    {
        private readonly IVoucherActionExecService _voucherActionExecService;
        public VoucherActionExecController(IVoucherActionExecService voucherActionExecService)
        {
            _voucherActionExecService = voucherActionExecService;
        }


        [HttpGet]
        [Route("{voucherTypeId}/ActionButtons")]
        public async Task<IList<ActionButtonModel>> ActionButtons([FromRoute] int voucherTypeId)
        {
            return await _voucherActionExecService.GetActionButtons(voucherTypeId).ConfigureAwait(true);
        }

        [HttpPost]
        [Route("{voucherTypeId}/{voucherBillId}/Exec/{actionButtonId}")]
        [ObjectDataApi(EnumObjectType.VoucherType, "voucherTypeId")]
        [ActionButtonDataApi("actionButtonId")]
        public async Task<List<NonCamelCaseDictionary>> ExecInputAction([FromRoute] int voucherTypeId, [FromRoute] int actionButtonId, [FromRoute] long voucherBillId, [FromBody] BillInfoModel data)
        {
            return await _voucherActionExecService.ExecActionButton(actionButtonId, voucherTypeId, voucherBillId, data).ConfigureAwait(true);
        }
    }
}