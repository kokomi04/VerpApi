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
    public class PrintConfigControllerAbstract<TModel> : VErpBaseController
    {
        private readonly IPrintConfigService<TModel> _printConfigService;

        public PrintConfigControllerAbstract(IPrintConfigService<TModel> printConfigService)
        {
            _printConfigService = printConfigService;
        }

        [HttpPost]
        [Route("search")]
        [VErpAction(EnumActionType.View)]
        public Task<PageData<TModel>> Search([FromQuery] int moduleTypeId, [FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size, [FromQuery] string orderByField, [FromQuery] bool asc)
        {
            return _printConfigService.Search(moduleTypeId, keyword, page, size, orderByField, asc);
        }

        [HttpGet]
        [Route("{printConfigId}")]
        public Task<TModel> GetPrintConfigStandard([FromRoute] int printConfigId)
        {
            return _printConfigService.GetPrintConfig(printConfigId);
        }

        [HttpPost]
        [Route("")]
        public Task<int> AddPrintConfig([FromFormString] TModel model, IFormFile template, IFormFile background)
        {
            return _printConfigService.AddPrintConfig(model, template, background);
        }

        [HttpPut]
        [Route("{printConfigId}")]
        public Task<bool> UpdatePrintConfig([FromRoute] int printConfigId, [FromFormString] TModel model, IFormFile template, IFormFile background)
        {
            return _printConfigService.UpdatePrintConfig(printConfigId, model, template, background);
        }

        [HttpDelete]
        [Route("{printConfigId}")]
        public Task<bool> DeletePrintConfig([FromRoute] int printConfigId)
        {
            return _printConfigService.DeletePrintConfig(printConfigId);
        }

        [HttpGet]
        [Route("{printConfigId}/template")]
        public async Task<IActionResult> GetPrintConfigTemplateFile([FromRoute] int printConfigId)
        {
            var file = await _printConfigService.GetPrintConfigTemplateFile(printConfigId);
            return new FileStreamResult(file.file, !string.IsNullOrWhiteSpace(file.contentType) ? file.contentType : "application/octet-stream") { FileDownloadName = file.fileName };
        }


        [HttpGet]
        [Route("{printConfigId}/background")]
        public async Task<IActionResult> GetPrintConfigBackgroundFile([FromRoute] int printConfigId)
        {
            var file = await _printConfigService.GetPrintConfigBackgroundFile(printConfigId);
            return new FileStreamResult(file.file, !string.IsNullOrWhiteSpace(file.contentType) ? file.contentType : "application/octet-stream") { FileDownloadName = file.fileName };
        }

        [VErpAction(EnumActionType.View)]
        [HttpPost]
        [Route("{printConfigId}/template/fillData")]
        public async Task<IActionResult> GeneratePrintTemplate([FromRoute] int printConfigId, [FromBody] NonCamelCaseDictionary templateModel, [FromQuery] bool isDoc = false)
        {
            var r = await _printConfigService.GeneratePrintTemplate(printConfigId, templateModel, isDoc);

            return new FileStreamResult(r.file, !string.IsNullOrWhiteSpace(r.contentType) ? r.contentType : "application/octet-stream") { FileDownloadName = r.fileName };
        }
    }
}
