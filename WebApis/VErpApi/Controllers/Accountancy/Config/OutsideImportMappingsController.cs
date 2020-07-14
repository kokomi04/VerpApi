using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Accountancy.Model.OutsideMapping;
using VErp.Services.Accountancy.Service.Input;

namespace VErpApi.Controllers.Accountancy.Config
{

    [Route("api/accountancy/config/outsideImportMappings")]

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
        public async Task<OutsideMappingModel> GetImportMappingInfo(string mappingFunctionKey)
        {
            return await _outsideMappingService.GetImportMappingInfo(mappingFunctionKey);
        }

        [HttpPut("{outsideImportMappingFunctionId}")]
        public async Task<bool> UpdateImportMapping(int outsideImportMappingFunctionId, OutsideMappingModel model)
        {
            return await _outsideMappingService.UpdateImportMapping(outsideImportMappingFunctionId, model);
        }

        [HttpDelete("{outsideImportMappingFunctionId}")]
        public async Task<bool> DeleteImportMapping(int outsideImportMappingFunctionId)
        {
            return await _outsideMappingService.DeleteImportMapping(outsideImportMappingFunctionId);
        }

    }
}
