using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Service.Config;

namespace VErpApi.Controllers.System.Internal
{
    [Route("api/internal/InternalBarcodeConfig")]

    public class InternalBarcodeConfigController : CrossServiceBaseController
    {
        private readonly IBarcodeConfigService _barcodeConfigService;
        public InternalBarcodeConfigController(IBarcodeConfigService barcodeConfigService)
        {
            _barcodeConfigService = barcodeConfigService;
        }

        [HttpGet]
        [Route("")]
        public async Task<PageData<BarcodeConfigListOutput>> Get([FromQuery] string keyword, [FromQuery] int page = 1, [FromQuery] int size = 0)
        {
            return await _barcodeConfigService.GetList(keyword, page, size);
        }

    }
}