using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Enums.StockEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Service.Config;
using VErp.Services.Stock.Service.FileResources;
using System.Collections.Generic;
using VErp.Commons.Library;
using System;
using Newtonsoft.Json;
using System.IO;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Services.Master.Service.Category;
using VErp.Services.Master.Model.Category;
using VErp.Services.Master.Model.CategoryConfig;
using VErp.Commons.GlobalObject.InternalDataInterface;

namespace VErpApi.Controllers.System.Category
{
    [Route("api/categoryGroup")]

    public class CategoryGroupController : VErpBaseController
    {
        private readonly ICategoryGroupService _categoryGroupService;


        public CategoryGroupController(ICategoryGroupService categoryConfigService)
        {
            _categoryGroupService = categoryConfigService;
        }

        [GlobalApi]
        [HttpGet]
        [Route("")]
        public async Task<IList<CategoryGroupModel>> GetList()
        {
            return await _categoryGroupService.GetList();
        }


        [HttpPost]
        [Route("")]
        public async Task<int> Add([FromBody] CategoryGroupModel model)
        {
            return await _categoryGroupService.Add(model);
        }

        [HttpPut]
        [Route("{categoryGroupId}")]
        public async Task<bool> Update([FromRoute] int categoryGroupId, [FromBody] CategoryGroupModel model)
        {
            return await _categoryGroupService.Update(categoryGroupId, model);
        }

        [HttpDelete]
        [Route("{categoryGroupId}")]
        public async Task<bool> DeleteCategory([FromRoute] int categoryGroupId)
        {
            return await _categoryGroupService.Delete(categoryGroupId);
        }


    }
}