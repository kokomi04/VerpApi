using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.Enums.MasterEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.EF.EFExtensions;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.Config;
using VErp.Services.Master.Service.Config;

namespace VErpApi.Controllers.System.Internal
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
        public async Task<bool> MapObjectCustomGenCode([FromRoute] int objectTypeId,[FromBody] Dictionary<int,int> data)
        {
            return await _customGenCodeService.UpdateMultiConfig(objectTypeId, data).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("currentConfig")]
        public async Task<CustomGenCodeOutputModel> GetCurrentConfig([FromQuery] int objectTypeId, [FromQuery] int objectId)
        {
            return await _customGenCodeService.GetCurrentConfig(objectTypeId, objectId);
        }

        [HttpGet]
        [Route("generateCode")]
        public async Task<CustomCodeModel> GenerateCode([FromQuery] int customGenCodeId, [FromQuery] int lastValue)
        {
            return await _customGenCodeService.GenerateCode(customGenCodeId, lastValue);
        }

        [HttpPut]
        [Route("confirmCode")]
        public async Task<bool> ConfirmCode([FromQuery] int objectTypeId, [FromQuery] int objectId)
        {
            return await _customGenCodeService.ConfirmCode(objectTypeId, objectId);
        }
    }
}