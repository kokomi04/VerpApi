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
        private readonly IGenCodeConfigService _genCodeConfigService;
        private readonly IObjectGenCodeService _objectGenCodeService;
        public InternalCustomGenCodeController(IGenCodeConfigService genCodeConfigService, IObjectGenCodeService objectGenCodeService)
        {
            _genCodeConfigService = genCodeConfigService;
            _objectGenCodeService = objectGenCodeService;
        }

        [HttpPost]
        [Route("multiconfigs")]
        public async Task<bool> UpdateMultiConfig([FromQuery] EnumObjectType targetObjectTypeId, [FromQuery] EnumObjectType configObjectTypeId, [FromBody] Dictionary<long, int> objectCustomGenCodes)
        {
            return await _objectGenCodeService.UpdateMultiConfig(targetObjectTypeId, configObjectTypeId, objectCustomGenCodes).ConfigureAwait(true);
        }

        [HttpGet]
        [Route("currentConfig")]
        public async Task<CustomGenCodeOutputModel> GetCurrentConfig([FromQuery] EnumObjectType targetObjectTypeId, [FromQuery] EnumObjectType configObjectTypeId, [FromQuery] long configObjectId)
        {
            return await _objectGenCodeService.GetCurrentConfig(targetObjectTypeId, configObjectTypeId, configObjectId);
        }

        [HttpGet]
        [Route("generateCode")]
        public async Task<CustomCodeModel> GenerateCode([FromQuery] int customGenCodeId, [FromQuery] int lastValue)
        {
            return await _genCodeConfigService.GenerateCode(customGenCodeId, lastValue);
        }

        [HttpPut]
        [Route("confirmCode")]
        public async Task<bool> ConfirmCode([FromQuery] int customGenCodeId)
        {
            return await _genCodeConfigService.ConfirmCode(customGenCodeId);
        }
    }
}