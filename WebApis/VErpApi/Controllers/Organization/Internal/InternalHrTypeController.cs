using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper.Hr;
using VErp.Services.Organization.Service.HrConfig;

namespace VErpApi.Controllers.Organization.Internal
{

    [Route("api/internal/[controller]")]
    public class InternalHrTypeController : CrossServiceBaseController
    {
        public readonly IHrTypeService _hrTypeService;

        public InternalHrTypeController(IHrTypeService hrTypeService)
        {
            _hrTypeService = hrTypeService;
        }

        [HttpGet]
        [Route("simpleList")]
        public async Task<IList<HrTypeSimpleModel>> GetSimpleList()
        {
            return await _hrTypeService.GetHrTypeSimpleList().ConfigureAwait(true);
        }
    }
}