using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
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
    public class PrintConfigCustomController : VErpBaseController
    {
        private readonly IPrintConfigCustomService _printConfigCustomService;

        public PrintConfigCustomController(IPrintConfigCustomService printConfigCustomService)
        {
            _printConfigCustomService = printConfigCustomService;
        }

        [HttpPost]
        [Route("search")]
        [VErpAction(EnumActionType.View)]
        public Task<PageData<PrintConfigCustomModel>> Search([FromQuery] int moduleTypeId, [FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size, [FromQuery] string orderByField, [FromQuery] bool asc)
        {
            return _printConfigCustomService.Search(moduleTypeId, keyword, page, size, orderByField, asc);
        }

        [HttpGet]
        [Route("{printConfigId}")]
        public Task<PrintConfigCustomModel> GetPrintConfigCustom([FromRoute] int printConfigId)
        {
            return _printConfigCustomService.GetPrintConfigCustom(printConfigId);
        }

        [HttpPost]
        [Route("")]
        public Task<int> AddPrintConfigCustom([FromFormString] PrintConfigCustomModel model, IFormFile file)
        {
            return _printConfigCustomService.AddPrintConfigCustom(model, file);
        }

        [HttpPut]
        [Route("{printConfigId}")]
        public Task<bool> UpdatePrintConfigCustom([FromRoute] int printConfigId, [FromFormString] PrintConfigCustomModel model, IFormFile file)
        {
            return _printConfigCustomService.UpdatePrintConfigCustom(printConfigId, model, file);
        }

        [HttpDelete]
        [Route("{printConfigId}")]
        public Task<bool> DeletePrintConfigCustom([FromRoute] int printConfigId)
        {
            return _printConfigCustomService.DeletePrintConfigCustom(printConfigId);
        }

        [HttpPost]
        [Route("{printConfigId}/template")]
        public async Task<IActionResult> GetPrintConfigTemplateFile([FromRoute] int printConfigId)
        {
            var file = await _printConfigCustomService.GetPrintConfigTemplateFile(printConfigId);
            return new FileStreamResult(file.file, !string.IsNullOrWhiteSpace(file.contentType) ? file.contentType : "application/octet-stream") { FileDownloadName = file.fileName };
        }

        [HttpPost]
        [Route("{printConfigId}/template/fillData")]
        public async Task<IActionResult> GeneratePrintTemplate([FromRoute] int printConfigId, [FromBody] NonCamelCaseDictionary templateModel)
        {
            var r = await _printConfigCustomService.GeneratePrintTemplate(printConfigId, templateModel);

            return new FileStreamResult(r.file, !string.IsNullOrWhiteSpace(r.contentType) ? r.contentType : "application/octet-stream") { FileDownloadName = r.fileName };
        }

        [HttpPut]
        [Route("{printConfigId}/rollback")]
        public Task<bool> RollbackPrintConfigCustom([FromRoute] int printConfigId)
        {
            return _printConfigCustomService.RollbackPrintConfigCustom(printConfigId);
        }
    }
}
