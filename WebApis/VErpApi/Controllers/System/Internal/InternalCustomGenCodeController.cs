using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject.InternalDataInterface;
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
        public async Task<CustomGenCodeOutputModel> GetCurrentConfig([FromQuery] EnumObjectType targetObjectTypeId, [FromQuery] EnumObjectType configObjectTypeId, [FromQuery] long configObjectId, [FromQuery] long? fId, [FromQuery] string code, [FromQuery] long? date)
        {
            return await _objectGenCodeService.GetCurrentConfig(targetObjectTypeId, configObjectTypeId, configObjectId, fId, code, date);
        }

        [HttpGet]
        [Route("generateCode")]
        public async Task<CustomCodeGeneratedModel> GenerateCode([FromQuery] int customGenCodeId, [FromQuery] int lastValue, [FromQuery] long? fId, [FromQuery] string code, [FromQuery] long? date)
        {
            return await _genCodeConfigService.GenerateCode(customGenCodeId, lastValue, fId, code, date);
        }

        [HttpPut]
        [Route("{customGenCodeId}/confirmCode")]
        public async Task<bool> ConfirmCode([FromRoute] int customGenCodeId, [FromQuery] string baseValue)
        {
            return await _genCodeConfigService.ConfirmCode(customGenCodeId, baseValue);
        }
    }
}