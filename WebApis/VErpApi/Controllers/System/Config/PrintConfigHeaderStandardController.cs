using Microsoft.AspNetCore.Mvc;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Master.Model.PrintConfig;
using VErp.Services.Master.Service.PrintConfig;

namespace VErpApi.Controllers.System.Config
{
    [Route("api/printConfigHeaderStandard")]
    public class PrintConfigHeaderStandardController : PrintConfigHeaderControllerAbstract<PrintConfigHeaderStandardModel, PrintConfigHeaderStandardViewModel>
    {
        public PrintConfigHeaderStandardController(IPrintConfigHeaderStandardService printConfigHeaderStandardService) : base(printConfigHeaderStandardService)
        {
        }
    }
}
