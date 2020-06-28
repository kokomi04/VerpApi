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

namespace VErpApi.Controllers.Accountancy.Config
{
    [Route("api/categorydata")]

    public class CategoryDataController : VErpBaseController
    {
        private readonly ICategoryDataService _categoryDataService;
            
        public CategoryDataController(ICategoryDataService categoryDataService)
        {
            _categoryDataService = categoryDataService;
        }

        [HttpGet]
        [Route("{categoryCode}/categoryrows")]
        public async Task<ServiceResult<PageData<NonCamelCaseDictionary>>> GetCategoryRowsByCode([FromRoute] string categoryCode, [FromQuery] string keyword, [FromQuery]string filters, [FromQuery] int page, [FromQuery] int size)
        {
            return await _categoryDataService.GetCategoryRows(categoryCode, keyword, filters, page, size);
        }

        [HttpGet]
        [Route("{categoryCode}/categoryrows/{categoryRowId}")]
        public async Task<ServiceResult<NonCamelCaseDictionary>> GetCategoryRow([FromRoute] string categoryCode, [FromRoute] int categoryRowId)
        {
            return await _categoryDataService.GetCategoryRow(categoryCode, categoryRowId);
        }

        [HttpPost]
        [Route("{categoryCode}/categoryrows")]
        public async Task<ServiceResult<int>> GetCategoryRow([FromRoute] string categoryCode, [FromBody] Dictionary<string, string> data)
        {
            return await _categoryDataService.AddCategoryRow(categoryCode, data);
        }

        [HttpPut]
        [Route("{categoryCode}/categoryrows/{categoryRowId}")]
        public async Task<ServiceResult<int>> GetCategoryRow([FromRoute] string categoryCode, [FromRoute] int categoryRowId, [FromBody] Dictionary<string, string> data)
        {
            return await _categoryDataService.UpdateCategoryRow(categoryCode, categoryRowId, data);
        }

        [HttpDelete]
        [Route("{categoryCode}/categoryrows/{categoryRowId}")]
        public async Task<ServiceResult<int>> DeleteCategoryRow([FromRoute] string categoryCode, [FromRoute] int categoryRowId)
        {
            return await _categoryDataService.DeleteCategoryRow(categoryCode, categoryRowId);
        }

        [HttpPost]
        [Route("mapToObject")]
        public async Task<ServiceResult<List<MapObjectOutputModel>>> MapToObject([FromBody] MapObjectInputModel[] data)
        {
            return await _categoryDataService.MapToObject(data);
        }
    }
}