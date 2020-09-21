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
using VErp.Services.Accountancy.Service.Category;
using VErp.Services.Accountancy.Model.Category;
using VErp.Commons.GlobalObject;
using VErp.Services.Accountancy.Model.Data;
using VErp.Commons.Library.Model;

namespace VErpApi.Controllers.Accountancy.Config
{
    [Route("api/categorydata")]

    public class CategoryDataController : VErpBaseController
    {
        private readonly ICategoryDataService _categoryDataService;
        private readonly ICategoryConfigService _categoryConfigService;

        public CategoryDataController(ICategoryDataService categoryDataService, ICategoryConfigService categoryConfigService)
        {
            _categoryDataService = categoryDataService;
            _categoryConfigService = categoryConfigService;
        }

        [HttpPost]
        [VErpAction(EnumAction.View)]
        [GlobalApi]
        [Route("{categoryId}/categoryrows/Search")]
        public async Task<PageData<NonCamelCaseDictionary>> GetCategoryRows([FromRoute] int categoryId, [FromBody] CategoryFilterModel request)
        {
            if (request == null) throw new BadRequestException(GeneralCode.InvalidParams);

            return await _categoryDataService.GetCategoryRows(categoryId, request.Keyword, request.Filters, request.ExtraFilter, request.ExtraFilterParams, request.Page, request.Size);
        }

        [HttpGet]
        [Route("{categoryId}/categoryrows/{categoryRowId}")]
        public async Task<NonCamelCaseDictionary> GetCategoryRow([FromRoute] int categoryId, [FromRoute] int categoryRowId)
        {
            return await _categoryDataService.GetCategoryRow(categoryId, categoryRowId);
        }

        [HttpPost]
        [Route("{categoryId}/categoryrows")]
        public async Task<int> GetCategoryRow([FromRoute] int categoryId, [FromBody] Dictionary<string, string> data)
        {
            return await _categoryDataService.AddCategoryRow(categoryId, data);
        }

        [HttpPut]
        [Route("{categoryId}/categoryrows/{categoryRowId}")]
        public async Task<int> GetCategoryRow([FromRoute] int categoryId, [FromRoute] int categoryRowId, [FromBody] Dictionary<string, string> data)
        {
            return await _categoryDataService.UpdateCategoryRow(categoryId, categoryRowId, data);
        }

        [HttpDelete]
        [Route("{categoryId}/categoryrows/{categoryRowId}")]
        public async Task<int> DeleteCategoryRow([FromRoute] int categoryId, [FromRoute] int categoryRowId)
        {
            return await _categoryDataService.DeleteCategoryRow(categoryId, categoryRowId);
        }

        [HttpPost]
        [Route("mapToObject")]
        public async Task<List<MapObjectOutputModel>> MapToObject([FromBody] MapObjectInputModel[] data)
        {
            return await _categoryDataService.MapToObject(data);
        }

        [HttpGet]
        [Route("{categoryId}/fieldDataForMapping")]
        public async Task<CategoryNameModel> GetFieldDataForMapping([FromRoute] int categoryId)
        {
            return await _categoryConfigService.GetFieldDataForMapping(categoryId).ConfigureAwait(true);
        }

        [HttpPost]
        [Route("{categoryId}/importFromMapping")]
        public async Task<bool> ImportFromMapping([FromRoute] int categoryId, [FromForm] string mapping, [FromForm] IFormFile file)
        {
            if (file == null)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }
            return await _categoryDataService.ImportCategoryRowFromMapping(categoryId, JsonConvert.DeserializeObject<CategoryImportExelMapping>(mapping), file.OpenReadStream()).ConfigureAwait(true);
        }
    }
}