using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject.InternalDataInterface.DynamicBill;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Master.Service.Category;

namespace VErpApi.Controllers.PurchaseOrder.Action
{
    [Route("api/Category/CategoryActionConfig")]

    public class CategoryActionConfigController : VErpBaseController
    {
        private readonly ICategoryActionConfigService _categoryActionConfigService;
        public CategoryActionConfigController(ICategoryActionConfigService categoryActionConfigService)
        {
            _categoryActionConfigService = categoryActionConfigService;
        }

        [HttpGet]
        [Route("")]
        public async Task<IList<ActionButtonModel>> GetList()
        {
            return await _categoryActionConfigService.GetActionButtonConfigs().ConfigureAwait(true);
        }

        [HttpPost]
        [Route("")]
        public async Task<ActionButtonModel> Create([FromBody] ActionButtonUpdateModel model)
        {
            return await _categoryActionConfigService.AddActionButton(model).ConfigureAwait(true);
        }

        [HttpPut]
        [Route("{actionButtonId}")]
        public async Task<ActionButtonModel> Update([FromRoute] int actionButtonId, [FromBody] ActionButtonUpdateModel model)
        {
            return await _categoryActionConfigService.UpdateActionButton(actionButtonId, model).ConfigureAwait(true);
        }

        [HttpDelete]
        [Route("{actionButtonId}")]
        public async Task<bool> Delete([FromRoute] int actionButtonId)
        {
            return await _categoryActionConfigService.DeleteActionButton(actionButtonId).ConfigureAwait(true);
        }

        [HttpPost]
        [Route("Mapping/{categoryId}")]
        public async Task<int> AddActionButtonBillType([FromRoute] int categoryId, [FromBody] ActionButtonBillTypeMappingModel model)
        {
            return await _categoryActionConfigService.AddActionButtonBillType(model.ActionButtonId, categoryId).ConfigureAwait(true);
        }

        [HttpDelete]
        [Route("Mapping/{categoryId}")]
        public async Task<bool> RemoveActionButtonBillType([FromRoute] int categoryId, [FromBody] ActionButtonBillTypeMappingModel model)
        {
            return await _categoryActionConfigService.RemoveActionButtonBillType(model.ActionButtonId, categoryId, "").ConfigureAwait(true);
        }

    }
}