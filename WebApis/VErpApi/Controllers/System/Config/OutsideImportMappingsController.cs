using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.OutsideMapping;
using VErp.Services.Master.Service.Config;

namespace VErpApi.Controllers.System
{

    [Route("api/system/config/outsideImportMappings")]

    public class OutsideImportMappingsController : VErpBaseController
    {

        private readonly IOutsideImportMappingService _outsideMappingService;
        public OutsideImportMappingsController(IOutsideImportMappingService outsideMappingService)
        {
            _outsideMappingService = outsideMappingService;
        }

        [HttpGet("")]
        public async Task<PageData<OutsideMappingModelList>> GetList([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size)
        {
            return await _outsideMappingService.GetList(keyword, page, size);
        }

        [HttpPost("")]
        public async Task<int> CreateImportMapping([FromBody] OutsideMappingModel model)
        {
            return await _outsideMappingService.CreateImportMapping(model);
        }

        [HttpGet("{outsideImportMappingFunctionId}")]
        public async Task<OutsideMappingModel> GetImportMappingInfo([FromRoute] int outsideImportMappingFunctionId)
        {
            return await _outsideMappingService.GetImportMappingInfo(outsideImportMappingFunctionId);
        }

        [HttpGet("InfoByKey/{mappingFunctionKey}")]
        [GlobalApi]
        public async Task<OutsideMappingModel> GetImportMappingInfo([FromRoute] string mappingFunctionKey)
        {
            return await _outsideMappingService.GetImportMappingInfo(mappingFunctionKey);
        }

        [HttpPut("{outsideImportMappingFunctionId}")]
        public async Task<bool> UpdateImportMapping([FromRoute] int outsideImportMappingFunctionId, [FromBody] OutsideMappingModel model)
        {
            return await _outsideMappingService.UpdateImportMapping(outsideImportMappingFunctionId, model);
        }

        [HttpDelete("{outsideImportMappingFunctionId}")]
        public async Task<bool> DeleteImportMapping([FromRoute] int outsideImportMappingFunctionId)
        {
            return await _outsideMappingService.DeleteImportMapping(outsideImportMappingFunctionId);
        }


        [HttpGet]
        [GlobalApi]
        [Route("MappingObjectInfo")]
        public async Task<OutsideImportMappingObjectModel> MappingObjectInfo([FromQuery] string mappingFunctionKey, [FromQuery] string objectId)
        {
            return await _outsideMappingService.MappingObjectInfo(mappingFunctionKey, objectId).ConfigureAwait(true);
        }
    }
}
