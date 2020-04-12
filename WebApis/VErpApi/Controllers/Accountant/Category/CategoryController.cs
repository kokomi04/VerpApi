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
using VErp.Commons.Library;
using System;

namespace VErpApi.Controllers.Accountant
{
    [Route("api/categories")]

    public class CategoryController : VErpBaseController
    {
        private readonly ICategoryService _categoryService;
        private readonly ICategoryFieldService _categoryFieldService;
        private readonly ICategoryRowService _categoryRowService;
        private readonly ICategoryValueService _categoryValueService;
        private readonly IFileService _fileService;
        public CategoryController(ICategoryService categoryService
            , ICategoryFieldService categoryFieldService
            , ICategoryRowService categoryRowService
            , ICategoryValueService categoryValueService
            , IFileService fileService
            )
        {
            _fileService = fileService;
            _categoryService = categoryService;
            _categoryFieldService = categoryFieldService;
            _categoryRowService = categoryRowService;
            _categoryValueService = categoryValueService;
        }

        [HttpGet]
        [Route("{categoryId}/categoryfields/{categoryFieldId}/categoryvalues/reference")]
        public async Task<ServiceResult<PageData<CategoryValueModel>>> GetCategoryField([FromRoute] int categoryId, [FromRoute] int categoryFieldId, [FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _categoryValueService.GetReferenceValues(categoryId, categoryFieldId, keyword, page, size);
        }

        [HttpGet]
        [Route("")]
        public async Task<ServiceResult<PageData<CategoryModel>>> Get([FromQuery] string keyword, [FromQuery] bool? isModule, [FromQuery] bool? hasParent, [FromQuery] int page, [FromQuery] int size)
        {
            return await _categoryService.GetCategories(keyword, isModule, hasParent, page, size);
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
            return await _categoryService.DeleteCategory(updatedUserId, categoryId);
        }

        [HttpGet]
        [Route("{categoryId}/categoryfields")]
        public async Task<ServiceResult<PageData<CategoryFieldOutputModel>>> GetCategoryFields([FromRoute] int categoryId, [FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size, [FromQuery] bool? isFull)
        {
            return await _categoryFieldService.GetCategoryFields(categoryId, keyword, page, size, isFull);
        }

        [HttpGet]
        [Route("{categoryId}/categoryfields/{categoryFieldId}")]
        public async Task<ServiceResult<CategoryFieldOutputFullModel>> GetCategoryField([FromRoute] int categoryId, [FromRoute] int categoryFieldId)
        {
            return await _categoryFieldService.GetCategoryField(categoryId, categoryFieldId);
        }

        [HttpPost]
        [Route("{categoryId}/categoryfields")]
        public async Task<ServiceResult<int>> AddCategoryField([FromRoute] int categoryId, [FromBody] CategoryFieldInputModel categoryField)
        {
            var updatedUserId = UserId;
            return await _categoryFieldService.AddCategoryField(updatedUserId, categoryId, categoryField);
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
        [Route("{categoryId}/categoryfields/{categoryFieldId}/categoryvalues")]
        public async Task<PageData<CategoryValueModel>> GetDefaultCategoryValues([FromRoute] int categoryId, [FromRoute] int categoryFieldId, [FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _categoryValueService.GetDefaultCategoryValues(categoryId, categoryFieldId, keyword, page, size);
        }

        [HttpGet]
        [Route("{categoryId}/categoryfields/{categoryFieldId}/categoryvalues/{categoryValueId}")]
        public async Task<ServiceResult<CategoryValueModel>> GetDefaultCategoryValue([FromRoute] int categoryId, [FromRoute] int categoryFieldId, [FromRoute] int categoryValueId)
        {
            return await _categoryValueService.GetDefaultCategoryValue(categoryId, categoryFieldId, categoryValueId);
        }

        [HttpPost]
        [Route("{categoryId}/categoryfields/{categoryFieldId}/categoryvalues")]
        public async Task<ServiceResult<int>> AddDefaultCategoryValue([FromRoute] int categoryId, [FromRoute] int categoryFieldId, [FromBody] CategoryValueModel data)
        {
            var updatedUserId = UserId;
            return await _categoryValueService.AddDefaultCategoryValue(updatedUserId, categoryId, categoryFieldId, data);
        }

        [HttpPut]
        [Route("{categoryId}/categoryfields/{categoryFieldId}/categoryvalues/{categoryValueId}")]
        public async Task<ServiceResult> UpdateDefaultCategoryValue([FromRoute] int categoryId, [FromRoute] int categoryFieldId, [FromRoute] int categoryValueId, [FromBody] CategoryValueModel data)
        {
            var updatedUserId = UserId;
            return await _categoryValueService.UpdateDefaultCategoryValue(updatedUserId, categoryId, categoryFieldId, categoryValueId, data);
        }

        [HttpDelete]
        [Route("{categoryId}/categoryfields/{categoryFieldId}/categoryvalues/{categoryValueId}")]
        public async Task<ServiceResult> DeleteDefaultCategoryValue([FromRoute] int categoryId, [FromRoute] int categoryFieldId, [FromRoute] int categoryValueId)
        {
            var updatedUserId = UserId;
            return await _categoryValueService.DeleteDefaultCategoryValue(updatedUserId, categoryId, categoryFieldId, categoryValueId);
        }

        [HttpGet]
        [Route("{categoryId}/categoryrows")]
        public async Task<ServiceResult<PageData<CategoryRowOutputModel>>> GetCategoryRows([FromRoute] int categoryId, [FromQuery] int page, [FromQuery] int size)
        {
            return await _categoryRowService.GetCategoryRows(categoryId, page, size);
        }

        [HttpGet]
        [Route("{categoryId}/categoryrows/{categoryRowId}")]
        public async Task<ServiceResult<CategoryRowOutputModel>> GetCategoryRow([FromRoute] int categoryId, [FromRoute] int categoryRowId)
        {
            return await _categoryRowService.GetCategoryRow(categoryId, categoryRowId);
        }

        [HttpPost]
        [Route("{categoryId}/categoryrows")]
        public async Task<ServiceResult<int>> AddCategoryRow([FromRoute] int categoryId, [FromBody] CategoryRowInputModel data)
        {
            var updatedUserId = UserId;
            return await _categoryRowService.AddCategoryRow(updatedUserId, categoryId, data);
        }

        [HttpPost]
        [Route("{categoryId}/categoryrows/file")]
        public async Task<ServiceResult> ImportCategoryRow([FromRoute] int categoryId, [FromForm] IFormFile file)
        {
            var updatedUserId = UserId;
            var r = await _categoryRowService.ImportCategoryRow(updatedUserId, categoryId, file.OpenReadStream());
            if (r.IsSuccessCode())
            {
                await _fileService.Upload(EnumObjectType.Category, EnumFileType.Document, file.FileName, file).ConfigureAwait(false);
            }
            return r;

        }

        [HttpGet]
        [Route("{categoryId}/categoryrows/file")]
        public async Task<IActionResult> GetImportTemplateCategoryRow([FromRoute] int categoryId)
        {
            var r = await _categoryRowService.GetImportTemplateCategoryRow(categoryId);

            if (!r.IsSuccessCode())
            {
                return new JsonResult(r);
            }
            return File(r.Data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "template.xlsx");

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