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
    public abstract class PrintConfigHeaderControllerAbstract<TModel, TViewModel> : VErpBaseController
    {
        private readonly IPrintConfigHeaderService<TModel, TViewModel> _printConfigHeaderService;

        public PrintConfigHeaderControllerAbstract(IPrintConfigHeaderService<TModel, TViewModel> printConfigHeaderService)
        {
            _printConfigHeaderService = printConfigHeaderService;
        }

        [HttpPost]
        [Route("search")]
        [VErpAction(EnumActionType.View)]
        [GlobalApi]
        public async Task<PageData<TViewModel>> Search(string keyword, int page, int size)
        {
            return await _printConfigHeaderService.Search(keyword, page, size);
        }

        [HttpGet]
        [Route("{printConfigHeaderId}")]
        [GlobalApi]
        public async Task<TModel> GetPrintConfigHeader([FromRoute] int printConfigHeaderId)
        {
            return await _printConfigHeaderService.GetHeaderById(printConfigHeaderId);
        }

        [HttpPost]
        [Route("")]
        [GlobalApi]
        public async Task<int> AddPrintHeaderConfig([FromBody] TModel model)
        {
            return await _printConfigHeaderService.CreateHeader(model);
        }

        [HttpPut]
        [Route("{printConfigHeaderId}")]
        [GlobalApi]
        public async Task<bool> UpdatePrintConfig([FromRoute] int printConfigHeaderId, [FromBody] TModel model)
        {
            return await _printConfigHeaderService.UpdateHeader(printConfigHeaderId, model);
        }

        [HttpDelete]
        [Route("{printConfigHeaderId}")]
        [GlobalApi]
        public async Task<bool> DeletePrintHeaderConfig([FromRoute] int printConfigHeaderId)
        {
            return await _printConfigHeaderService.DeleteHeader(printConfigHeaderId);
        }
    }
}
