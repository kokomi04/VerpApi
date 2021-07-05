using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.Config;
using VErp.Services.Master.Service.Config;

namespace VErpApi.Controllers.System.Config
{
    [Route("api/ObjectPrintConfig")]
    public class ObjectPrintConfigController : VErpBaseController
    {
        private readonly IObjectPrintConfigService _objectPrintConfigService;

        public ObjectPrintConfigController(IObjectPrintConfigService objectPrintConfigService)
        {
            _objectPrintConfigService = objectPrintConfigService;
        }

        [HttpGet]
        [Route("search")]
        public async Task<PageData<ObjectPrintConfigSearch>> GetObjectPrintConfigSearch(string keyword, int page, int size)
        {
            return await _objectPrintConfigService.GetObjectPrintConfigSearch(keyword, page, size);
        }

        [HttpPost]
        [Route("")]
        public async Task<bool> MapObjectPrintConfig([FromBody] ObjectPrintConfig mapping)
        {
            return await _objectPrintConfigService.MapObjectPrintConfig(mapping);
        }

        [HttpGet]
        [Route("")]
        public async Task<ObjectPrintConfig> GetObjectPrintConfigMapping([FromQuery]EnumObjectType objectTypeId, [FromQuery] int objectId)
        {
            return await _objectPrintConfigService.GetObjectPrintConfigMapping(objectTypeId, objectId);
        }
    }
}
