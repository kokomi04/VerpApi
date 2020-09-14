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

namespace VErpApi.Controllers.Accountancy.Config
{
    [Route("api/categoryconfig")]

    public class CategoryConfigController : VErpBaseController
    {
        private readonly ICategoryConfigService _categoryConfigService;
 
            
        public CategoryConfigController(ICategoryConfigService categoryConfigService)
        {
            _categoryConfigService = categoryConfigService;
        }

        [HttpGet]
        [Route("GetCategoryIdByCode/{categoryCode}")]
        public async Task<int> GetCategoryIdByCode([FromRoute] string categoryCode)
        {
            return await _categoryConfigService.GetCategoryIdByCode(categoryCode);
        }

        [HttpGet]
        [Route("")]
        public async Task<PageData<CategoryModel>> Get([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _categoryConfigService.GetCategories(keyword, page, size);
        }

        [HttpPost]
        [Route("categoryFieldsByCodes")]
        public async Task<List<CategoryFieldReferModel>> GetCategoryFieldsByCodes([FromBody] string[] categoryCodes)
        {
            return await _categoryConfigService.GetCategoryFieldsByCodes(categoryCodes);
        }

        [HttpPost]
        [Route("")]
        public async Task<int> AddCategory([FromBody] CategoryModel category)
        {
            return await _categoryConfigService.AddCategory(category);
        }

        [HttpGet]
        [Route("{categoryId}")]
        public async Task<CategoryFullModel> GetCategory([FromRoute] int categoryId)
        {
            return await _categoryConfigService.GetCategory(categoryId);
        }


        [HttpGet]
        [Route("categoryByCode/{categoryCode}")]
        public async Task<CategoryFullModel> GetCategory([FromRoute] string categoryCode)
        {
            return await _categoryConfigService.GetCategory(categoryCode);
        }

        [HttpPut]
        [Route("{categoryId}")]
        public async Task<bool> UpdateCategory([FromRoute] int categoryId, [FromBody] CategoryModel category)
        {
            return await _categoryConfigService.UpdateCategory(categoryId, category);
        }

        [HttpDelete]
        [Route("{categoryId}")]
        public async Task<bool> DeleteCategory([FromRoute] int categoryId)
        {
            return await _categoryConfigService.DeleteCategory(categoryId);
        }

        [HttpGet]
        [Route("{categoryId}/categoryfields")]
        public async Task<PageData<CategoryFieldModel>> GetCategoryFields([FromRoute] int categoryId, [FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _categoryConfigService.GetCategoryFields(categoryId, keyword, page, size);
        }

        [HttpGet]
        [Route("categoryfieldsByCode")]
        public async Task<PageData<CategoryFieldModel>> GetCategoryFieldsByCode([FromQuery] string categoryCode, [FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _categoryConfigService.GetCategoryFieldsByCode(categoryCode, keyword, page, size);
        }

        [HttpPost]
        [VErpAction(EnumAction.View)]
        [Route("categoryfields")]
        public async Task<List<CategoryFieldModel>> GetCategoryFields([FromBody] IList<int> categoryIds)
        {
            return await _categoryConfigService.GetCategoryFields(categoryIds);
        }

        [HttpGet]
        [Route("{categoryId}/categoryfields/{categoryFieldId}")]
        public async Task<CategoryFieldModel> GetCategoryField([FromRoute] int categoryId, [FromRoute] int categoryFieldId)
        {
            return await _categoryConfigService.GetCategoryField(categoryId, categoryFieldId);
        }

        [HttpPost]
        [Route("{categoryId}/multifields")]
        public async Task<bool> UpdateMultiField([FromRoute] int categoryId, [FromBody] List<CategoryFieldModel> fields)
        {
            return await _categoryConfigService.UpdateMultiField(categoryId, fields);
        }

        [HttpDelete]
        [Route("{categoryId}/categoryfields/{categoryFieldId}")]
        public async Task<bool> DeleteCategoryField([FromRoute] int categoryId, [FromRoute] int categoryFieldId)
        {
            return await _categoryConfigService.DeleteCategoryField(categoryId, categoryFieldId);
        }

        [HttpGet]
        [Route("datatypes")]
        public PageData<DataTypeModel> GetDataTypes([FromQuery] int page, [FromQuery] int size)
        {
            return _categoryConfigService.GetDataTypes(page, size);
        }

        [HttpGet]
        [Route("formtypes")]
        public PageData<FormTypeModel> GetFormTypes([FromQuery] int page, [FromQuery] int size)
        {
            return _categoryConfigService.GetFormTypes(page, size);
        }

        [HttpGet]
        [Route("operators")]
        public PageData<OperatorModel> GetOperators([FromQuery] int page, [FromQuery] int size)
        {
            return _categoryConfigService.GetOperators(page, size);
        }

        [HttpGet]
        [Route("logicoperators")]
        public PageData<LogicOperatorModel> GetLogicOperators([FromQuery] int page, [FromQuery] int size)
        {
            return _categoryConfigService.GetLogicOperators(page, size);
        }

        [HttpGet]
        [Route("moduletypes")]
        public PageData<ModuleTypeModel> GetModuleTypes([FromQuery] int page, [FromQuery] int size)
        {
            return _categoryConfigService.GetModuleTypes(page, size);
        }
    }
}