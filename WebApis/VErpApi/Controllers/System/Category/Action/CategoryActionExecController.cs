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
    [Route("api/Category/CategoryActionExec")]

    public class CategoryActionExecController : VErpBaseController
    {
        private readonly ICategoryActionExecService _categoryActionExecService;
        public CategoryActionExecController(ICategoryActionExecService voucherActionExecService)
        {
            _categoryActionExecService = voucherActionExecService;
        }

        [HttpGet]
        [Route("{categoryId}/ActionButtons")]
        public async Task<IList<ActionButtonModel>> ActionButtons([FromRoute] int categoryId)
        {
            return await _categoryActionExecService.GetActionButtons(categoryId).ConfigureAwait(true);
        }

        [HttpPost]
        [Route("{categoryId}/{fId}/Exec/{actionButtonId}")]
        [ObjectDataApi(EnumObjectType.Category, "categoryId")]
        [ActionButtonDataApi("actionButtonId")]
        public async Task<List<NonCamelCaseDictionary>> ExecInputAction([FromRoute] int categoryId, [FromRoute] int fId, [FromRoute] int actionButtonId, [FromBody] BillInfoModel data)
        {
            return await _categoryActionExecService.ExecActionButton(actionButtonId, categoryId, fId, data).ConfigureAwait(true);
        }
    }
}