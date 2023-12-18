using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Verp.Services.ReportConfig.Model;
using Verp.Services.ReportConfig.Service;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.Report;
using VErp.Commons.Enums.StockEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Service.FileResources;

namespace VErpApi.Controllers.Report.Internal
{
    [Route("api/report/internal/[controller]")]
    public class InternalReportTypeController : CrossServiceBaseController
    {
        private readonly IReportConfigService _reportConfigService;
        private readonly IFileService _fileService;
        public InternalReportTypeController(IReportConfigService reportConfigService, IFileService fileService)
        {
            _reportConfigService = reportConfigService;
            _fileService = fileService;
        }

     
        [HttpGet]
        [Route("simpleList")]
        public async Task<PageData<ReportTypeListModel>> GetReportTypes([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size, [FromQuery] int? moduleTypeId = null)
        {
            return await _reportConfigService
                .ReportTypes(keyword, page, size, moduleTypeId)
                .ConfigureAwait(true);
        }

        [HttpGet]
        [Route("Groups")]
        public async Task<IList<ReportTypeGroupList>> Groups()
        {
            return await _reportConfigService
                .ReportTypeGroupList()
                .ConfigureAwait(true);
        }

    }
}