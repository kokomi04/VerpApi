using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.GlobalObject;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ApiCore.ModelBinders;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.PrintConfig;
using VErp.Services.Master.Service.PrintConfig;

namespace VErpApi.Controllers.System.Config
{
    [Route("api/printConfig/custom")]
    public class PrintConfigCustomController : PrintConfigControllerAbstract<PrintConfigCustomModel>
    {
        private readonly IPrintConfigCustomService _printConfigCustomService;

        public PrintConfigCustomController(IPrintConfigCustomService printConfigCustomService) : base(printConfigCustomService)
        {
            _printConfigCustomService = printConfigCustomService;
        }


        [HttpPut]
        [Route("{printConfigId}/rollback")]
        public Task<bool> RollbackPrintConfigCustom([FromRoute] int printConfigId)
        {
            return _printConfigCustomService.RollbackPrintConfigCustom(printConfigId);
        }
    }
}
