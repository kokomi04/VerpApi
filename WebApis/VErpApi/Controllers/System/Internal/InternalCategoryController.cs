using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.Enums.MasterEnum;
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
        public InternalCategoryController(ICategoryConfigService userService)
        {
            _categoryConfigService = userService;
        }

        [HttpPost]
        [Route("ReferFields")]
        public async Task<List<ReferFieldModel>> GetReferFields([FromBody] ReferInputModel input)
        {
            return await _categoryConfigService.GetReferFields(input.CategoryCodes, input.FieldNames).ConfigureAwait(true);
        }
    }
}