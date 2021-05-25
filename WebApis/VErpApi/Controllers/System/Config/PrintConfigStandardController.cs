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
    [Route("api/printConfig/standard")]
    public class PrintConfigStandardController : VErpBaseController
    {
        private readonly IPrintConfigStandardService _printConfigStandardService;

        public PrintConfigStandardController(IPrintConfigStandardService printConfigStandardService)
        {
            _printConfigStandardService = printConfigStandardService;
        }

        [HttpPost]
        [Route("search")]
        [VErpAction(EnumActionType.View)]
        public Task<PageData<PrintConfigStandardModel>> Search([FromQuery] int moduleTypeId, [FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size, [FromQuery] string orderByField, [FromQuery] bool asc)
        {
            return _printConfigStandardService.Search(moduleTypeId, keyword, page, size, orderByField, asc);
        }

        [HttpGet]
        [Route("{printConfigId}")]
        public Task<PrintConfigStandardModel> GetPrintConfigStandard([FromRoute] int printConfigId)
        {
            return _printConfigStandardService.GetPrintConfigStandard(printConfigId);
        }

        [HttpPost]
        [Route("")]
        public Task<int> AddPrintConfigStandard([FromFormString] PrintConfigStandardModel model, IFormFile file)
        {
            return _printConfigStandardService.AddPrintConfigStandard(model, file);
        }

        [HttpPut]
        [Route("{printConfigId}")]
        public Task<bool> UpdatePrintConfigStandard([FromRoute] int printConfigId, [FromFormString] PrintConfigStandardModel model, IFormFile file)
        {
            return _printConfigStandardService.UpdatePrintConfigStandard(printConfigId, model, file);
        }

        [HttpDelete]
        [Route("{printConfigId}")]
        public Task<bool> DeletePrintConfigStandard([FromRoute] int printConfigId)
        {
            return _printConfigStandardService.DeletePrintConfigStandard(printConfigId);
        }

        [HttpPost]
        [Route("{printConfigId}/template")]
        public async Task<IActionResult> GetPrintConfigTemplateFile([FromRoute] int printConfigId)
        {
            var file = await _printConfigStandardService.GetPrintConfigTemplateFile(printConfigId);
            return new FileStreamResult(file.file, !string.IsNullOrWhiteSpace(file.contentType) ? file.contentType : "application/octet-stream") { FileDownloadName = file.fileName };
        }

        [HttpPost]
        [Route("{printConfigId}/template/fillData")]
        public async Task<IActionResult> GeneratePrintTemplate([FromRoute] int printConfigId, [FromBody] NonCamelCaseDictionary templateModel, [FromQuery] bool isDoc = false)
        {
            var r = await _printConfigStandardService.GeneratePrintTemplate(printConfigId, templateModel, isDoc);

            return new FileStreamResult(r.file, !string.IsNullOrWhiteSpace(r.contentType) ? r.contentType : "application/octet-stream") { FileDownloadName = r.fileName };
        }
    }
}
