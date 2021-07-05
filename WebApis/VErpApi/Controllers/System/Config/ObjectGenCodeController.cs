using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.EF.MasterDB;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.Config;
using VErp.Services.Master.Model.Users;
using VErp.Services.Master.Service.Config;
using VErp.Services.Master.Service.Users;

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
        public async Task<PageData<ObjectGenCodeMappingTypeModel>> GetObjectGenCodeMappingTypes([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _objectGenCodeService.GetObjectGenCodeMappingTypes(keyword, page, size);
        }

        [HttpGet("currentConfig")]
        [GlobalApi]
        public async Task<CustomGenCodeOutputModel> GetCurrentConfig([FromQuery] EnumObjectType targetObjectTypeId, [FromQuery] EnumObjectType configObjectTypeId, [FromQuery] long configObjectId, [FromQuery] long? fId, [FromQuery] string code = "", [FromQuery] long? date = null)
        {
            return await _objectGenCodeService.GetCurrentConfig(targetObjectTypeId, configObjectTypeId, configObjectId, fId, code, date);
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