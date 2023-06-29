using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Verp.Cache.Caching;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface.Category;
using VErp.Commons.GlobalObject.InternalDataInterface.DynamicBill;
using VErp.Commons.Library;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.Category;
using VErp.Services.Master.Service.Category;

namespace VErpApi.Controllers.System.Internal
{
    [Route("api/internal/[controller]")]
    [ApiController]
    public class InternalCategoryController : CrossServiceBaseController
    {
        private readonly ICategoryConfigService _categoryConfigService;
        private readonly ICategoryDataService _categoryDataService;
        private readonly ICachingService _cachingService;
        public InternalCategoryController(ICategoryConfigService categoryConfigService, ICategoryDataService categoryDataService, ICachingService cachingService)
        {
            _categoryConfigService = categoryConfigService;
            _categoryDataService = categoryDataService;
            _cachingService = cachingService;
        }

        [HttpPost]
        [Route("ReferFields")]
        public async Task<List<ReferFieldModel>> GetReferFields([FromBody] ReferInputModel input)
        {
            if (input == null) throw new BadRequestException(GeneralCode.InvalidParams);
            return await _categoryConfigService.GetReferFields(input.CategoryCodes, input.FieldNames).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("DynamicCates")]
        public async Task<IList<CategoryListModel>> GetDynamicCates()
        {
            return await _categoryConfigService.GetDynamicCates().ConfigureAwait(true);
        }

        [HttpPost]
        [Route("{categoryCode}/data/Search")]
        [VErpAction(EnumActionType.View)]
        public async Task<PageData<NonCamelCaseDictionary>> GetCategoryRows([FromRoute] string categoryCode, [FromBody] CategoryFilterModel request)
        {
            if (request == null) throw new BadRequestException(GeneralCode.InvalidParams);
            var key = categoryCode + "_" + JsonUtils.JsonSerialize(request).ToGuid();
            return await _cachingService.TryGetSet("CATEGORY", key, TimeSpan.FromMinutes(3), async () =>
             {
                 return await _categoryDataService.GetCategoryRows(categoryCode, request.Keyword, request.Filters, request.ColumnsFilters, request.FilterData, request.ExtraFilter, request.ExtraFilterParams, request.Page, request.Size, request.OrderBy, request.Asc);
             }, TimeSpan.FromMinutes(1));

        }

        [HttpGet]
        [Route("GetAllCategoryConfig")]
        public async Task<IList<CategoryFullModel>> GetAllCategoryConfig()
        {
            return await _categoryConfigService.GetAllCategoryConfig();
        }
    }
}