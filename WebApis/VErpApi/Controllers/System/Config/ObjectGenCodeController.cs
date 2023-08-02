using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject.InternalDataInterface.System;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.Config;
using VErp.Services.Master.Service.Config;

namespace VErpApi.Controllers.System
{

    [Route("api/ObjectGenCode")]
    public class ObjectGenCodeController : VErpBaseController
    {
        private readonly IObjectGenCodeService _objectGenCodeService;
        public ObjectGenCodeController(IObjectGenCodeService objectGenCodeService
            )
        {
            _objectGenCodeService = objectGenCodeService;
        }


        [HttpGet]
        [Route("")]
        public async Task<PageData<ObjectGenCodeMappingTypeModel>> GetObjectGenCodeMappingTypes([FromQuery] EnumModuleType? moduleTypeId, [FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _objectGenCodeService.GetObjectGenCodeMappingTypes(moduleTypeId, keyword, page, size);
        }

        [HttpGet("currentConfig")]
        [GlobalApi]
        public async Task<CustomGenCodeOutputModel> GetCurrentConfig([FromQuery] EnumObjectType targetObjectTypeId, [FromQuery] EnumObjectType configObjectTypeId, [FromQuery] long configObjectId, [FromQuery] string configObjectTitle, [FromQuery] long? fId, [FromQuery] string code = "", [FromQuery] long? date = null)
        {
            return await _objectGenCodeService.GetCurrentConfig(targetObjectTypeId, configObjectTypeId, configObjectId, configObjectTitle, fId, code, date);
        }

        [HttpPost("objectCustomGenCode")]
        public async Task<bool> MapObjectCustomGenCode([FromBody] ObjectGenCodeMapping model)
        {
            return await _objectGenCodeService.MapObjectGenCode(model);
        }

        [HttpDelete("{objectCustomGenCodeMappingId}")]
        public async Task<bool> DeleteMapObjectGenCode([FromRoute] int objectCustomGenCodeMappingId)
        {
            return await _objectGenCodeService.DeleteMapObjectGenCode(objectCustomGenCodeMappingId);
        }
    }
}