using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ApiCore.ModelBinders;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.Config;
using VErp.Services.Master.Model.PrintConfig;
using VErp.Services.Master.Service.Config;
using VErp.Services.Master.Service.Config.Implement;
using VErp.Services.Master.Service.PrintConfig;

namespace VErpApi.Controllers.System.Config
{
    [Route("api/printConfig/custom")]
    public class PrintConfigCustomController : PrintConfigControllerAbstract<PrintConfigCustomModel>
    {
        private readonly IPrintConfigCustomService _printConfigCustomService;
        private readonly IObjectPrintConfigService _objectPrintConfigService;

        public PrintConfigCustomController(IPrintConfigCustomService printConfigCustomService, IObjectPrintConfigService objectPrintConfigService) : base(printConfigCustomService)
        {
            _printConfigCustomService = printConfigCustomService;
            _objectPrintConfigService = objectPrintConfigService;
        }


        [HttpPut]
        [Route("{printConfigId}/rollback")]
        public Task<bool> RollbackPrintConfigCustom([FromRoute] int printConfigId)
        {
            return _printConfigCustomService.RollbackPrintConfigCustom(printConfigId);
        }


        [HttpPost]
        [Route("GetByObject")]
        [VErpAction(EnumActionType.View)]
        [GlobalApi]
        public async Task<IList<PrintConfigCustomModel>> GetByObject([FromQuery] EnumObjectType objectTypeId, [FromQuery] int objectId)
        {
            var objectPrintConfigMapping = await _objectPrintConfigService.GetObjectPrintConfigMapping(objectTypeId, objectId);
            if (objectPrintConfigMapping?.PrintConfigIds?.Length > 0)
            {
                return await GetByIds(objectPrintConfigMapping.PrintConfigIds);
            }
            return null;
        }
    }
}
