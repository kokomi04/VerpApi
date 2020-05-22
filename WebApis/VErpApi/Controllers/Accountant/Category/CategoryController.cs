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
using Newtonsoft.Json;
using System.IO;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Infrastructure.ApiCore.Attributes;

namespace VErpApi.Controllers.Accountant
{
    [Route("api/categories")]

    public class CategoryController : VErpBaseController
    {
        private readonly ICategoryService _categoryService;
        private readonly ICategoryFieldService _categoryFieldService;
        private readonly ICategoryRowService _categoryRowService;
        private readonly ICategoryAreaService _categoryAreaService;
        private readonly IFileService _fileService;
        public CategoryController(ICategoryService categoryService
            , ICategoryFieldService categoryFieldService
            , ICategoryRowService categoryRowService
            , ICategoryAreaService categoryAreaService
            , IFileService fileService
            )
        {
            _fileService = fileService;
            _categoryService = categoryService;
            _categoryFieldService = categoryFieldService;
            _categoryRowService = categoryRowService;
            _categoryAreaService = categoryAreaService;
        }

        [HttpGet]
        [Route("")]
        public async Task<ServiceResult<PageData<CategoryModel>>> Get([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _categoryService.GetCategories(keyword, page, size);
        }

        [HttpPost]
        [Route("")]
        public async Task<ServiceResult<int>> AddCategory([FromBody] CategoryModel category)
        {
            return await _categoryService.AddCategory(category);
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
            return await _categoryService.UpdateCategory(categoryId, category);
        }

        [HttpDelete]
        [Route("{categoryId}")]
        public async Task<ServiceResult> DeleteCategory([FromRoute] int categoryId)
        {
            return await _categoryService.DeleteCategory(categoryId);
        }

        [HttpGet]
        [Route("{categoryId}/categoryareas")]
        public async Task<ServiceResult<PageData<CategoryAreaModel>>> GetInputAreas([FromRoute] int categoryId, [FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _categoryAreaService.GetCategoryAreas(categoryId, keyword, page, size);
        }

        [HttpGet]
        [Route("{categoryId}/categoryareas/{categoryAreaId}")]
        public async Task<ServiceResult<CategoryAreaModel>> GetInputArea([FromRoute] int categoryId, [FromRoute] int categoryAreaId)
        {
            return await _categoryAreaService.GetCategoryArea(categoryId, categoryAreaId);
        }

        [HttpPost]
        [Route("{categoryId}/categoryareas")]
        public async Task<ServiceResult<int>> AddInputArea([FromRoute] int categoryId, [FromBody] CategoryAreaInputModel categoryArea)
        {
            return await _categoryAreaService.AddCategoryArea(categoryId, categoryArea);
        }

        [HttpPut]
        [Route("{categoryId}/categoryareas/{categoryAreaId}")]
        public async Task<ServiceResult> UpdateInputArea([FromRoute] int categoryId, [FromRoute] int categoryAreaId, [FromBody] CategoryAreaInputModel categoryArea)
        {
            return await _categoryAreaService.UpdateCategoryArea(categoryId, categoryAreaId, categoryArea);
        }

        [HttpDelete]
        [Route("{categoryId}/categoryareas/{categoryAreaId}")]
        public async Task<ServiceResult> DeleteInputArea([FromRoute] int categoryId, [FromRoute] int categoryAreaId)
        {
            return await _categoryAreaService.DeleteCategoryArea(categoryId, categoryAreaId);
        }

        [HttpGet]
        [Route("{categoryId}/categoryfields")]
        public async Task<ServiceResult<PageData<CategoryFieldOutputModel>>> GetCategoryFields([FromRoute] int categoryId, [FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _categoryFieldService.GetCategoryFields(categoryId, keyword, page, size);
        }

        [HttpGet]
        [Route("{categoryId}/categoryfields/{categoryFieldId}")]
        public async Task<ServiceResult<CategoryFieldOutputModel>> GetCategoryField([FromRoute] int categoryId, [FromRoute] int categoryFieldId)
        {
            return await _categoryFieldService.GetCategoryField(categoryId, categoryFieldId);
        }

        [HttpPost]
        [Route("{categoryId}/categoryfields")]
        public async Task<ServiceResult<int>> AddCategoryField([FromRoute] int categoryId, [FromBody] CategoryFieldInputModel categoryField)
        {
            return await _categoryFieldService.AddCategoryField(categoryId, categoryField);
        }

        [HttpPost]
        [Route("{categoryId}/multifields")]
        public async Task<ServiceResult> UpdateMultiField([FromRoute] int categoryId, [FromBody] List<CategoryFieldInputModel> fields)
        {
            return await _categoryFieldService.UpdateMultiField(categoryId, fields);
        }



        [HttpPut]
        [Route("{categoryId}/categoryfields/{categoryFieldId}")]
        public async Task<ServiceResult> UpdateCategoryField([FromRoute] int categoryId, [FromRoute] int categoryFieldId, [FromBody] CategoryFieldInputModel categoryField)
        {
            return await _categoryFieldService.UpdateCategoryField(categoryId, categoryFieldId, categoryField);
        }

        [HttpDelete]
        [Route("{categoryId}/categoryfields/{categoryFieldId}")]
        public async Task<ServiceResult> DeleteCategoryField([FromRoute] int categoryId, [FromRoute] int categoryFieldId)
        {
            return await _categoryFieldService.DeleteCategoryField(categoryId, categoryFieldId);
        }

        [HttpGet]
        [Route("{categoryId}/categoryrows")]
        public async Task<ServiceResult<PageData<CategoryRowListOutputModel>>> GetCategoryRows([FromRoute] int categoryId, [FromQuery] string keyword, [FromQuery]string filters, [FromQuery] int page, [FromQuery] int size)
        {
            Clause filterClause = null;
            if (!string.IsNullOrEmpty(filters))
            {
                filterClause = JsonConvert.DeserializeObject<Clause>(filters);
            }
            return await _categoryRowService.GetCategoryRows(categoryId, keyword, filterClause, page, size);
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
            return await _categoryRowService.AddCategoryRow(categoryId, data);
        }

        [HttpPost]
        [Route("{categoryId}/categoryrows/file")]
        public async Task<ServiceResult> ImportCategoryRow([FromRoute] int categoryId, [FromForm] IFormFile file)
        {
            var r = await _categoryRowService.ImportCategoryRow(categoryId, file.OpenReadStream());
            if (r.IsSuccessCode())
            {
                await _fileService.Upload(EnumObjectType.Category, EnumFileType.Document, file.FileName, file).ConfigureAwait(true);
            }
            return r;
        }

        [HttpGet]
        [Route("{categoryId}/categoryrows/templatefile")]
        public async Task<ServiceResult<MemoryStream>> GetImportTemplateCategory([FromRoute] int categoryId)
        {
            var r = await _categoryRowService.GetImportTemplateCategory(categoryId);
            return r;
        }

        [HttpGet]
        [Route("{categoryId}/categoryrows/datafile")]
        public async Task<ServiceResult<MemoryStream>> ExportCategoryRow([FromRoute] int categoryId)
        {
            var r = await _categoryRowService.ExportCategory(categoryId);
            return r;
        }


        [HttpPut]
        [Route("{categoryId}/categoryrows/{categoryRowId}")]
        public async Task<ServiceResult> UpdateCategoryRow([FromRoute] int categoryId, [FromRoute] int categoryRowId, [FromBody] CategoryRowInputModel data)
        {
            return await _categoryRowService.UpdateCategoryRow(categoryId, categoryRowId, data);
        }

        [HttpDelete]
        [Route("{categoryId}/categoryrows/{categoryRowId}")]
        public async Task<ServiceResult> DeleteCategoryRow([FromRoute] int categoryId, [FromRoute] int categoryRowId)
        {
            return await _categoryRowService.DeleteCategoryRow(categoryId, categoryRowId);
        }

        [HttpPost]
        [Route("categoryvalues/maptitle")]
        public async Task<ServiceResult<List<MapTitleOutputModel>>> MapTitle([FromBody] MapTitleInputModel[] categoryValues)
        {
            return await _categoryRowService.MapTitle(categoryValues);
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

        [HttpGet]
        [Route("operators")]
        public async Task<ServiceResult<PageData<OperatorModel>>> GetOperators([FromQuery] int page, [FromQuery] int size)
        {
            return await _categoryService.GetOperators(page, size);
        }

        [HttpGet]
        [Route("logicoperators")]
        public async Task<ServiceResult<PageData<LogicOperatorModel>>> GetLogicOperators([FromQuery] int page, [FromQuery] int size)
        {
            return await _categoryService.GetLogicOperators(page, size);
        }

        [HttpGet]
        [Route("moduletypes")]
        public async Task<ServiceResult<PageData<ModuleTypeModel>>> GetModuleTypes([FromQuery] int page, [FromQuery] int size)
        {
            return await _categoryService.GetModuleTypes(page, size);
        }
    }
}