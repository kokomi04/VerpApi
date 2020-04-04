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
using VErp.Services.Accountant.Service.Category;
using VErp.Services.Accountant.Model.Category;
using System.Collections.Generic;

namespace VErpApi.Controllers.Accountant
{
    [Route("api/categories")]

    public class CategoryController : VErpBaseController
    {
        private readonly ICategoryService _categoryService;
        private readonly ICategoryFieldService _categoryFieldService;
        private readonly ICategoryRowService _categoryRowService;
        public CategoryController(ICategoryService categoryService
            , ICategoryFieldService categoryFieldService
            , ICategoryRowService categoryRowService
            )
        {
            _categoryService = categoryService;
            _categoryFieldService = categoryFieldService;
            _categoryRowService = categoryRowService;
        }


        [HttpGet]
        [Route("")]
        public async Task<ServiceResult<PageData<CategoryModel>>> Get([FromQuery] string keyword, [FromQuery] bool? isModule, [FromQuery] int page, [FromQuery] int size)
        {
            return await _categoryService.GetCategories(keyword, isModule, page, size);
        }

        [HttpPost]
        [Route("")]
        public async Task<ServiceResult<int>> AddCategory([FromBody] CategoryModel category)
        {
            var updatedUserId = UserId;
            return await _categoryService.AddCategory(updatedUserId, category);
        }

        [HttpGet]
        [Route("{categoryId}")]
        public async Task<ServiceResult<CategoryFullModel>> GetCategory([FromRoute] int categoryId)
        {
            return await _categoryService.GetCategory(categoryId);
        }

        [HttpPut]
        [Route("{categoryId}")]
        public async Task<ServiceResult> UpdateCategory([FromRoute] int categoryId, [FromBody] CategoryModel category)
        {
            var updatedUserId = UserId;
            return await _categoryService.UpdateCategory(updatedUserId, categoryId, category);
        }

        [HttpDelete]
        [Route("{categoryId}")]
        public async Task<ServiceResult> DeleteCategory([FromRoute] int categoryId)
        {
            var updatedUserId = UserId;
            return await _categoryService.DeleteCategory(categoryId, updatedUserId);
        }

        [HttpGet]
        [Route("{categoryId}/categoryfields")]
        public async Task<ServiceResult<PageData<CategoryFieldOutputModel>>> GetCategoryFields([FromRoute] int categoryId, [FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size, [FromQuery] bool? isFull)
        {
            var updatedUserId = UserId;
            return await _categoryFieldService.GetCategoryFields(categoryId, keyword, page, size, isFull);
        }

        [HttpGet]
        [Route("{categoryId}/categoryfields/{categoryFieldId}")]
        public async Task<ServiceResult<CategoryFieldOutputFullModel>> GetCategoryField([FromRoute] int categoryId, [FromRoute] int categoryFieldId)
        {
            var updatedUserId = UserId;
            return await _categoryFieldService.GetCategoryField(categoryId, categoryFieldId);
        }

        [HttpPost]
        [Route("{categoryId}/categoryfields")]
        public async Task<ServiceResult<int>> AddCategoryField([FromBody] CategoryFieldInputModel categoryField)
        {
            var updatedUserId = UserId;
            return await _categoryFieldService.AddCategoryField(updatedUserId, categoryField);
        }

        [HttpPut]
        [Route("{categoryId}/categoryfields/{categoryFieldId}")]
        public async Task<ServiceResult> UpdateCategoryField([FromRoute] int categoryId, [FromRoute] int categoryFieldId, [FromBody] CategoryFieldInputModel categoryField)
        {
            var updatedUserId = UserId;
            return await _categoryFieldService.UpdateCategoryField(updatedUserId, categoryId, categoryFieldId, categoryField);
        }

        [HttpDelete]
        [Route("{categoryId}/categoryfields/{categoryFieldId}")]
        public async Task<ServiceResult> DeleteCategoryField([FromRoute] int categoryId, [FromRoute] int categoryFieldId)
        {
            var updatedUserId = UserId;
            return await _categoryFieldService.DeleteCategoryField(updatedUserId, categoryId, categoryFieldId);
        }

        [HttpGet]
        [Route("{categoryId}/categoryrows")]
        public async Task<ServiceResult<PageData<CategoryRowOutputModel>>> GetCategoryRows([FromRoute] int categoryId, [FromQuery] int page, [FromQuery] int size)
        {
            var updatedUserId = UserId;
            return await _categoryRowService.GetCategoryRows(categoryId, page, size);
        }

        [HttpGet]
        [Route("{categoryId}/categoryrows/{categoryRowId}")]
        public async Task<ServiceResult<CategoryRowOutputModel>> GetCategoryRow([FromRoute] int categoryId, [FromRoute] int categoryRowId)
        {
            var updatedUserId = UserId;
            return await _categoryRowService.GetCategoryRow(categoryId, categoryRowId);
        }

        [HttpPost]
        [Route("{categoryId}/categoryrows")]
        public async Task<ServiceResult<int>> AddCategoryRow([FromRoute] int categoryId, [FromBody] CategoryRowInputModel data)
        {
            var updatedUserId = UserId;
            return await _categoryRowService.AddCategoryRow(updatedUserId, categoryId, data);
        }

        [HttpPut]
        [Route("{categoryId}/categoryrows/{categoryRowId}")]
        public async Task<ServiceResult> UpdateCategoryRow([FromRoute] int categoryId, [FromRoute] int categoryRowId, [FromBody] CategoryRowInputModel data)
        {
            var updatedUserId = UserId;
            return await _categoryRowService.UpdateCategoryRow(updatedUserId, categoryId, categoryRowId, data);
        }

        [HttpDelete]
        [Route("{categoryId}/categoryrows/{categoryRowId}")]
        public async Task<ServiceResult> DeleteCategoryRow([FromRoute] int categoryId, [FromRoute] int categoryRowId)
        {
            var updatedUserId = UserId;
            return await _categoryRowService.DeleteCategoryRow(updatedUserId, categoryId, categoryRowId);
        }

        [HttpGet]
        [Route("datatypes")]
        public async Task<ServiceResult<PageData<DataTypeModel>>> GetDataTypes([FromQuery] int page, [FromQuery] int size)
        {
            return await _categoryService.GetDataTypes(page, size);
        }

        [HttpGet]
        [Route("formtypes")]
        public async Task<ServiceResult<PageData<FormTypeModel>>> GetFormTypes([FromQuery] int page, [FromQuery] int size)
        {
            return await _categoryService.GetFormTypes(page, size);
        }
    }
}