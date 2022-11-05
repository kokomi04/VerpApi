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
    [Route("api/printConfig/standard")]
    public class PrintConfigStandardController : PrintConfigControllerAbstract<PrintConfigStandardModel>
    {
        
        public PrintConfigStandardController(IPrintConfigStandardService printConfigStandardService) : base(printConfigStandardService)
        {
           
        }

    }
}
