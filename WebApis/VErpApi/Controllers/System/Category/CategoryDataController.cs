using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Verp.Cache.Caching;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface.Category;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ApiCore.ModelBinders;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.Category;
using VErp.Services.Master.Service.Category;
using static VErp.Commons.Constants.CurrencyCateConstants;

namespace VErpApi.Controllers.System.Category
{
    [Route("api/categorydata")]
    //[TypeFilter(typeof(CategoryPermissionAttribute))]
    [ObjectDataApi(EnumObjectType.Category, "categoryId")]
    public class CategoryDataController : VErpBaseController
    {
        private readonly ICategoryDataService _categoryDataService;
        private readonly ICategoryConfigService _categoryConfigService;
        private readonly ICachingService _cachingService;

        public CategoryDataController(ICategoryDataService categoryDataService, ICategoryConfigService categoryConfigService, ICachingService cachingService)
        {
            _categoryDataService = categoryDataService;
            _categoryConfigService = categoryConfigService;
            _cachingService = cachingService;
        }

        [HttpPost]
        [VErpAction(EnumActionType.View)]
        [GlobalApi]
        [Route("{categoryId}/categoryrows/Search")]
        public async Task<PageData<NonCamelCaseDictionary>> GetCategoryRows([FromRoute] int categoryId, [FromBody] CategoryFilterModel request)
        {
            if (request == null) throw new BadRequestException(GeneralCode.InvalidParams);

            return await _categoryDataService.GetCategoryRows(categoryId, request.Keyword, request.Filters, request.ColumnsFilters, request.FilterData, request.ExtraFilter, request.ExtraFilterParams, request.Page, request.Size, request.OrderBy, request.Asc);
        }


        [HttpPost]
        [Route("{categoryCode}/data/Search")]
        [GlobalApi]
        public async Task<PageData<NonCamelCaseDictionary>> GetCategoryRows([FromRoute] string categoryCode, [FromBody] CategoryFilterModel request)
        {
            if (request == null) throw new BadRequestException(GeneralCode.InvalidParams);

            return await _categoryDataService.GetCategoryRows(categoryCode, request.Keyword, request.Filters, request.ColumnsFilters, request.FilterData, request.ExtraFilter, request.ExtraFilterParams, request.Page, request.Size, request.OrderBy, request.Asc);
        }

        [GlobalApi]
        [HttpGet]
        [Route("{categoryId}/categoryrows/{categoryRowId}")]
        public async Task<NonCamelCaseDictionary> GetCategoryRow([FromRoute] int categoryId, [FromRoute] int categoryRowId)
        {
            return await _categoryDataService.GetCategoryRow(categoryId, categoryRowId);
        }


        [GlobalApi]
        [HttpGet]
        [Route("byCode/{categoryCode}/row/{categoryRowId}")]
        public async Task<NonCamelCaseDictionary> GetCategoryRow([FromRoute] string categoryCode, [FromRoute] int categoryRowId)
        {
            return await _categoryDataService.GetCategoryRow(categoryCode, categoryRowId);
        }


        [HttpPost]
        [Route("{categoryId}/categoryrows")]
        public async Task<int> AddCategoryRow([FromRoute] int categoryId, [FromBody] NonCamelCaseDictionary data)
        {
            return await _categoryDataService.AddCategoryRow(categoryId, data);
        }

        [HttpPut]
        [Route("{categoryId}/categoryrows/{categoryRowId}")]
        public async Task<int> UpdateCategoryRow([FromRoute] int categoryId, [FromRoute] int categoryRowId, [FromBody] NonCamelCaseDictionary data)
        {
            return await _categoryDataService.UpdateCategoryRow(categoryId, categoryRowId, data);
        }

        [HttpDelete]
        [Route("{categoryId}/categoryrows/{categoryRowId}")]
        public async Task<int> DeleteCategoryRow([FromRoute] int categoryId, [FromRoute] int categoryRowId)
        {
            return await _categoryDataService.DeleteCategoryRow(categoryId, categoryRowId);
        }

        [GlobalApi]
        [HttpPost]
        [VErpAction(EnumActionType.View)]
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
        public async Task<bool> ImportFromMapping([FromRoute] int categoryId, [FromFormString] ImportExcelMapping mapping, IFormFile file)
        {
            if (file == null)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }
            mapping.FileName = file.FileName;
            return await _categoryDataService.ImportCategoryRowFromMapping(categoryId, mapping, file.OpenReadStream()).ConfigureAwait(true);
        }



        [GlobalApi]
        [Route("primaryCurrency")]
        [HttpGet]
        public async Task<NonCamelCaseDictionary> PrimaryCurrency()
        {
            var columnsFilters = new SingleClause()
            {
                DataType = EnumDataType.Boolean,
                FieldName = "IsPrimary",
                Operator = EnumOperator.Equal,
                Value = true
            };
            var data = await _categoryDataService.GetCategoryRows(CurrencyCategoryCode, null, null, columnsFilters, null, null, null, 1, 1, null, true);
            return data?.List?.FirstOrDefault();
        }
    }
}