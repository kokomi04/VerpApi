using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.Enums.MasterEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Service.Config;

namespace VErpApi.Controllers.Stock.Internal
{
    [Route("api/internal/[controller]")]
    [ApiController]
    public class InternalCustomGenCodeController : CrossServiceBaseController
    {
        private readonly ICustomGenCodeService _customGenCodeService;
        public InternalCustomGenCodeController(ICustomGenCodeService customGenCodeService)
        {
            _customGenCodeService = customGenCodeService;
        }

        [HttpPost]
        [Route("{objectTypeId}/multiconfigs")]
        public async Task<ServiceResult> MapObjectCustomGenCode([FromRoute] int objectTypeId,[FromBody] Dictionary<int,int> data)
        {
            return await _customGenCodeService.UpdateMultiConfig(objectTypeId, data).ConfigureAwait(true);
        }
    }
}