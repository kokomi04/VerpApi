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
using VErp.Services.Master.Model.Config;
using VErp.Services.Master.Service.Config;
using VErp.Services.PurchaseOrder.Model.Voucher;
using VErp.Services.PurchaseOrder.Service.Voucher;

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