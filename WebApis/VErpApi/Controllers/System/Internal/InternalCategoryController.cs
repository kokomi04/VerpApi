using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.EF.EFExtensions;
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
        public InternalCategoryController(ICategoryConfigService categoryConfigService)
        {
            _categoryConfigService = categoryConfigService;
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
    }
}