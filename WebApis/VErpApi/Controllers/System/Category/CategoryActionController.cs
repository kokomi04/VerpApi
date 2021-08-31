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
using VErp.Services.Master.Service.Category;

namespace VErpApi.Controllers.System.Category
{
    [Route("api/CategoryAction")]

    public class CategoryActionController : VErpBaseController
    {
        private readonly ICategoryActionService _categoryActionService;
        public CategoryActionController(ICategoryActionService categoryActionService)
        {
            _categoryActionService = categoryActionService;
        }

        [HttpGet]
        [Route("{categoryId}")]
        public async Task<IList<ActionButtonModel>> GetList([FromRoute] int categoryId)
        {
            return await _categoryActionService.GetActionButtonConfigs(categoryId).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("{categoryId}/use")]
        public async Task<IList<ActionButtonSimpleModel>> GetListUse([FromRoute] int categoryId)
        {
            return await _categoryActionService.GetActionButtons(categoryId).ConfigureAwait(true);
        }

        [HttpPost]
        [Route("{categoryId}")]
        public async Task<ActionButtonModel> AddActionButton([FromRoute] int categoryId, [FromBody] ActionButtonModel model)
        {
            return await _categoryActionService.AddActionButton(categoryId, model).ConfigureAwait(true);
        }

        [HttpPut]
        [Route("{categoryId}/{actionButtonId}")]
        public async Task<ActionButtonModel> UpdateInputAction([FromRoute] int categoryId, [FromRoute] int actionButtonId, [FromBody] ActionButtonModel model)
        {
            return await _categoryActionService.UpdateActionButton(categoryId, actionButtonId, model).ConfigureAwait(true);
        }

        [HttpDelete]
        [Route("{categoryId}/{actionButtonId}")]
        public async Task<bool> DeleteInputAction([FromRoute] int categoryId, [FromRoute] int actionButtonId)
        {
            return await _categoryActionService.DeleteActionButton(categoryId, actionButtonId).ConfigureAwait(true);
        }

        [HttpPost]
        [Route("{categoryId}/Exec/{categoryActionId}")]
        [ObjectDataApi(EnumObjectType.Category, "categoryId")]
        [ActionButtonDataApi("categoryId")]
        public async Task<List<NonCamelCaseDictionary>> ExecInputAction([FromRoute] int categoryId, [FromRoute] int categoryActionId, [FromBody] NonCamelCaseDictionary data)
        {
            return await _categoryActionService.ExecActionButton(categoryId, categoryActionId, data).ConfigureAwait(true);
        }
    }
}