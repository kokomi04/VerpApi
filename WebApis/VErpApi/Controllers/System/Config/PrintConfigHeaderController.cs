using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Infrastructure.ApiCore.ModelBinders;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.PrintConfig;
using VErp.Services.Master.Service.PrintConfig;

namespace VErpApi.Controllers.System.Config
{
    [Route("api/printConfigHeader")]
    public class PrintConfigHeaderController : VErpBaseController
    {
        private readonly IPrintConfigHeaderService _printConfigHeaderService;

        public PrintConfigHeaderController(IPrintConfigHeaderService printConfigHeaderService)
        {
            _printConfigHeaderService = printConfigHeaderService;
        }

        [HttpPost]
        [Route("search")]
        [VErpAction(EnumActionType.View)]
        [GlobalApi]
        public async Task<PageData<PrintConfigHeaderViewModel>> Search(string keyword, int page, int size)
        {
            return await _printConfigHeaderService.Search(keyword, page, size);
        }

        [HttpGet]
        [Route("{printConfigHeaderId}")]
        [GlobalApi]
        public async Task<PrintConfigHeaderModel> GetPrintConfigHeader([FromRoute] int printConfigHeaderId)
        {
            return await _printConfigHeaderService.GetHeaderById(printConfigHeaderId);
        }

        [HttpPost]
        [Route("")]
        public async Task<int> AddPrintHeaderConfig([FromBody] PrintConfigHeaderModel model)
        {
            return await _printConfigHeaderService.CreateHeader(model);
        }

        [HttpPut]
        [Route("{printConfigHeaderId}")]
        public async Task<bool> UpdatePrintConfig([FromRoute] int printConfigHeaderId, [FromBody] PrintConfigHeaderModel model)
        {
            return await _printConfigHeaderService.UpdateHeader(printConfigHeaderId, model);
        }

        [HttpDelete]
        [Route("{printConfigHeaderId}")]
        public async Task<bool> DeletePrintHeaderConfig([FromRoute] int printConfigHeaderId)
        {
            return await _printConfigHeaderService.DeleteHeader(printConfigHeaderId);
        }

        [HttpPut]
        [Route("{printConfigHeaderId}/mapping")]
        public Task<bool> MapToPrintConfigCustom([FromRoute] int printConfigHeaderId, List<int> printConfigIds)
        {
            return _printConfigHeaderService.MapToPrintConfigCustom(printConfigHeaderId, printConfigIds);
        }
    }
}
