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
using Microsoft.AspNetCore.Authorization;
using VErp.Services.Master.Model.Config;
using VErp.Commons.GlobalObject.InternalDataInterface;
using VErp.Services.Accountancy.Model;
using System.Reflection;
using Verp.Services.PurchaseOrder.Model;
using VErp.Services.Stock.Model;
using VErp.Services.Manafacturing.Model;

namespace VErpApi.Controllers.System
{
    [Route("api/printConfig")]

    public class PrintConfigController : VErpBaseController
    {
        private readonly IPrintConfigService _printConfigService;

        private readonly Dictionary<EnumModuleType, Assembly> AssemblyModuleModel = new Dictionary<EnumModuleType, Assembly>()
        {
            { EnumModuleType.Accountant,    AccountancyModelAssembly.Assembly },
            { EnumModuleType.PurchaseOrder,PurchaseOrderModelAssembly.Assembly },
            { EnumModuleType.Stock, StockModelAssembly.Assembly },
            { EnumModuleType.Manufacturing, ManufacturingModelAssembly.Assembly }
        };

        public PrintConfigController(IPrintConfigService printConfigService)
        {
            _printConfigService = printConfigService;
        }

        [HttpGet]
        [Route("")]
        public async Task<ICollection<PrintConfigModel>> Get([FromQuery] int moduleTypeId, [FromQuery] int activeForId)
        {
            return await _printConfigService.GetPrintConfigs(moduleTypeId, activeForId);
        }

        [HttpGet]
        [Route("{printConfigId}")]
        public async Task<PrintConfigModel> GetPrintConfig([FromRoute] int printConfigId)
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

        [HttpPost]
        [Route("printTemplate")]
        public async Task<long> UploadPrintTemplate(IFormFile file)
        {
            return await _printConfigService.Upload(EnumObjectType.PrintConfig, EnumFileType.Document, string.Empty, file).ConfigureAwait(true);
        }

        [HttpPost]
        [Route("{printConfigId}/generatePrintTemplate/{fileId}")]
        public async Task<IActionResult> GeneratePrintTemplate([FromRoute] int printConfigId, [FromRoute] int fileId, PrintTemplateInput templateModel)
        {
            var r = await _printConfigService.GeneratePrintTemplate(printConfigId, fileId, templateModel);

            return new FileStreamResult(r.file, !string.IsNullOrWhiteSpace(r.contentType) ? r.contentType : "application/octet-stream") { FileDownloadName = r.fileName };
        }
        [HttpGet]
        [Route("suggestionField")]
        public async Task<IList<EntityField>> GetSuggestionField([FromQuery] int moduleTypeId)
        {
            var fields = await _printConfigService.GetSuggestionField(moduleTypeId);
            if (fields.Count == 0 && AssemblyModuleModel.ContainsKey((EnumModuleType)moduleTypeId))
                fields = await _printConfigService.GetSuggestionField(AssemblyModuleModel[(EnumModuleType)moduleTypeId]);
            return fields;
        }

        [HttpPut]
        [Route("rollback")]
        public async Task<bool> RollbackAllPrintConfig([FromQuery] long printConfigId)
        {
            return await _printConfigService.RollbackPrintConfig(printConfigId);
        }
        [HttpPut]
        [Route("rollback/onlyTemplate")]
        public async Task<bool> RollbackTemplatePrintConfig([FromQuery] long printConfigId)
        {
            return await _printConfigService.RollbackPrintConfigOnlyTemplate(printConfigId);
        }
        [HttpPut]
        [Route("rollback/withoutTemplate")]
        public async Task<bool> RollbackPrintConfigWithoutTemplate([FromQuery] long printConfigId)
        {
            return await _printConfigService.RollbackPrintConfigWithoutTemplate(printConfigId);
        }
    }
}