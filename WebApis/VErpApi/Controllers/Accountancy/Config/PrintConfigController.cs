using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Enums.StockEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Service.Config;
using VErp.Services.Stock.Service.FileResources;
using System.Collections.Generic;
using VErp.Commons.Library;
using System;
using Newtonsoft.Json;
using System.IO;
using VErp.Commons.Enums.AccountantEnum;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Services.Accountancy.Service.Input;
using VErp.Services.Accountancy.Model.Input;

namespace VErpApi.Controllers.Accountancy.Config
{
    [Route("api/printConfig")]

    public class PrintConfigController : VErpBaseController
    {
        private readonly IPrintConfigService _printConfigService;
 
        public PrintConfigController(IPrintConfigService printConfigService)
        {
            _printConfigService = printConfigService;
        }

        [HttpGet]
        [Route("")]
        public async Task<ICollection<PrintConfigModel>> Get([FromQuery] int inputTypeId)
        {
            return await _printConfigService.GetPrintConfigs(inputTypeId);
        }

        [HttpGet]
        [Route("{printConfigId}")]
        public async Task<ServiceResult<PrintConfigModel>> GetPrintConfig([FromRoute] int printConfigId)
        {
            return await _printConfigService.GetPrintConfig(printConfigId);
        }

        [HttpPut]
        [Route("{printConfigId}")]
        public async Task<bool> UpdatePrintConfig([FromRoute] int printConfigId, [FromBody] PrintConfigModel data)
        {
            return await _printConfigService.UpdatePrintConfig(printConfigId, data);
        }
    
        [HttpPost]
        [Route("")]
        public async Task<int> AddPrintConfig([FromBody] PrintConfigModel data)
        {
            return await _printConfigService.AddPrintConfig(data);
        }

        [HttpDelete]
        [Route("{printConfigId}")]
        public async Task<bool> DeletePrintConfig([FromRoute] int printConfigId)
        {
            return await _printConfigService.DeletePrintConfig(printConfigId);
        }
    }
}