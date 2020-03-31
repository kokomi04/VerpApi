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

namespace VErpApi.Controllers.System
{
    [Route("api/categories")]

    public class CategoryController : VErpBaseController
    {
        private readonly ICategoryService _categoryService;
        private readonly ICategoryFieldService _categoryFieldService;
        public CategoryController(ICategoryService categoryService
            , ICategoryFieldService categoryFieldService
            )
        {
            _categoryService = categoryService;
            _categoryFieldService = categoryFieldService;
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
        public async Task<ServiceResult<PageData<CategoryFieldOutputModel>>> GetCategoryFields([FromRoute]int categoryId, [FromQuery]string keyword, [FromQuery]int page, [FromQuery]int size, [FromQuery]bool? isFull)
        {
            var updatedUserId = UserId;
            return await _categoryFieldService.GetCategoryFields(categoryId, keyword, page, size, isFull);
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
        public async Task<ServiceResult> UpdateCategoryField([FromRoute]int categoryFieldId, [FromBody] CategoryFieldInputModel categoryField)
        {
            var updatedUserId = UserId;
            return await _categoryFieldService.UpdateCategoryField(updatedUserId, categoryFieldId, categoryField);
        }

        [HttpDelete]
        [Route("{categoryId}/categoryfields/{categoryFieldId}")]
        public async Task<ServiceResult> DeleteCategoryField([FromRoute]int categoryFieldId)
        {
            var updatedUserId = UserId;
            return await _categoryFieldService.DeleteCategoryField(updatedUserId, categoryFieldId);
        }
    }
}