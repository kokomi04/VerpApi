using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ServiceCore.CrossServiceHelper;
using VErp.Services.Organization.Service.HrConfig;

namespace VErpApi.Controllers.System.Internal {

    [Route("api/internal/[controller]")]
    public class InternalHrTypeController : CrossServiceBaseController
    {
        public readonly IHrTypeService _hrTypeService;

        public InternalHrTypeController(IHrTypeService hrTypeService) {
            _hrTypeService = hrTypeService;
        }

        [HttpGet]
        [Route("simpleList")]
        public async Task<IList<HrTypeSimpleModel>> GetSimpleList() {
            return await _hrTypeService.GetHrTypeSimpleList().ConfigureAwait(true);
        }
    }
}