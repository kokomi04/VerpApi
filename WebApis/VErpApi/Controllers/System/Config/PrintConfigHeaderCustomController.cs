using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Services.Master.Model.PrintConfig;
using VErp.Services.Master.Service.PrintConfig;

namespace VErpApi.Controllers.System.Config
{
    [Route("api/printConfigHeaderCustom")]
    public class PrintConfigHeaderCustomController : PrintConfigHeaderControllerAbstract<PrintConfigHeaderCustomModel, PrintConfigHeaderCustomViewModel>
    {

        private readonly IPrintConfigHeaderCustomService _printConfigHeaderCustomService;
        public PrintConfigHeaderCustomController(IPrintConfigHeaderCustomService printConfigHeaderCustomService) : base(printConfigHeaderCustomService)
        {
            _printConfigHeaderCustomService = printConfigHeaderCustomService;
        }

        [HttpPut]
        [Route("{printConfigHeaderId}/rollback")]
        [GlobalApi]
        public async Task<bool> RollbackPrintConfigHeaderCustom(int printConfigHeaderCustomId)
        {
            return await _printConfigHeaderCustomService.RollbackPrintConfigHeaderCustom(printConfigHeaderCustomId);
        }
    }
}
