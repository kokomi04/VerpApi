using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface.DynamicBill;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Services.Master.Service.Category;

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