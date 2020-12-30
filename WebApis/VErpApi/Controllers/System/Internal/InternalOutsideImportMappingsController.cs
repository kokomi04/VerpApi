using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.GlobalObject;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.OutsideMapping;
using VErp.Services.Master.Service.Config;

namespace VErpApi.Controllers.System
{

    [Route("api/internal/[controller]")]
    public class InternalOutsideImportMappingsController : VErpBaseController
    {

        private readonly IOutsideImportMappingService _outsideMappingService;
        public InternalOutsideImportMappingsController(IOutsideImportMappingService outsideMappingService)
        {
            _outsideMappingService = outsideMappingService;
        }

        [HttpGet("MappingObjectInfo")]
        public async Task<OutsideImportMappingObjectModel> MappingObjectInfo([FromQuery] string mappingFunctionKey, [FromQuery] string objectId)
        {
            return await _outsideMappingService.MappingObjectInfo(mappingFunctionKey, objectId);

        }

        [HttpPost("MappingObjectCreate")]
        public async Task<bool> MappingObjectCreate([FromBody] MappingObjectCreateRequest req)
        {
            if (req == null)
            {
                throw new BadRequestException(GeneralCode.InvalidParams);
            }
            return await _outsideMappingService.MappingObjectCreate(req.MappingFunctionKey, req.ObjectId, req.BillObjectTypeId, req.BillId);
        }

        [HttpDelete("MappingObjectDelete/{billObjectTypeId}/{billId}")]
        public async Task<bool> MappingObjectDelete([FromRoute] EnumObjectType billObjectTypeId, [FromRoute] long billId)
        {
            return await _outsideMappingService.MappingObjectDelete(billObjectTypeId, billId);

        }
    }
}
