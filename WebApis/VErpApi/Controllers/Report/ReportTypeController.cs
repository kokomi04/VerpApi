using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Verp.Services.ReportConfig.Model;
using Verp.Services.ReportConfig.Service;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StockEnum;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Service.FileResources;

namespace VErpApi.Controllers.Report
{
    [Route("api/reportTypes")]
    public class ReportTypeController : VErpBaseController
    {
        private readonly IReportConfigService _reportConfigService;
        private readonly IFileService _fileService; 
        public ReportTypeController(IReportConfigService reportConfigService, IFileService fileService)
        {
            _reportConfigService = reportConfigService;
            _fileService = fileService;
        }

        [HttpGet]
        [Route("Groups")]
        public async Task<IList<ReportTypeGroupList>> Groups()
        {
            return await _reportConfigService
                .ReportTypeGroupList()
                .ConfigureAwait(true);
        }

        [HttpPost]
        [Route("Groups")]
        public async Task<int> GroupsCreate([FromBody] ReportTypeGroupModel model)
        {
            return await _reportConfigService
                .ReportTypeGroupCreate(model)
                .ConfigureAwait(true);
        }

        [HttpPost]
        [Route("DecryptExtraFilter")]
        public CipherFilterModel DecryptExtraFilter([FromBody] CipherFilterModel cipherFilter)
        {
            return _reportConfigService.DecryptExtraFilter(cipherFilter);
        }

        [HttpPut]
        [Route("Groups/{reportTypeGroupId}")]
        public async Task<bool> GroupsUpdate([FromRoute] int reportTypeGroupId, [FromBody] ReportTypeGroupModel model)
        {
            return await _reportConfigService
                .ReportTypeGroupUpdate(reportTypeGroupId, model)
                .ConfigureAwait(true);
        }

        [HttpDelete]
        [Route("Groups/{reportTypeGroupId}")]
        public async Task<bool> GroupDelete([FromRoute] int reportTypeGroupId)
        {
            return await _reportConfigService
                .ReportTypeGroupDelete(reportTypeGroupId)
                .ConfigureAwait(true);
        }

        [HttpGet]
        [Route("")]
        public async Task<PageData<ReportTypeListModel>> GetReportTypes([FromQuery] string keyword, [FromQuery] int page, [FromQuery] int size, [FromQuery] int? reportTypeGroupId = null)
        {
            return await _reportConfigService
                .ReportTypes(keyword, page, size, reportTypeGroupId)
                .ConfigureAwait(true);
        }

        [HttpGet]
        [Route("{reportTypeId}")]
        public async Task<ReportTypeListModel> GetReportType([FromRoute] int reportTypeId)
        {
            return await _reportConfigService
                .ReportType(reportTypeId)
                .ConfigureAwait(true);
        }

        [HttpPost]
        [Route("")]
        public async Task<int> AddReportType([FromBody] ReportTypeModel data)
        {
            return await _reportConfigService
                .AddReportType(data)
                .ConfigureAwait(true);
        }


        [HttpPut]
        [Route("{reportTypeId}")]
        public async Task<int> UpdateReportType([FromRoute] int reportTypeId, [FromBody] ReportTypeModel data)
        {
            return await _reportConfigService
                .UpdateReportType(reportTypeId, data)
                .ConfigureAwait(true);
        }

        [HttpDelete]
        [Route("{reportTypeId}")]
        public async Task<int> DeleteReportType([FromRoute] int reportTypeId)
        {
            return await _reportConfigService
                .DeleteReportType(reportTypeId)
                .ConfigureAwait(true);
        }

        [HttpGet]
        [Route("{reportTypeId}/ViewInfo")]
        public async Task<ReportTypeViewModel> ReportTypeViewInfo([FromRoute] int reportTypeId)
        {
            return await _reportConfigService
                .ReportTypeViewGetInfo(reportTypeId)
                .ConfigureAwait(true);
        }

        [HttpGet]
        [Route("config/{reportTypeId}/ViewInfo")]
        public async Task<ReportTypeViewModel> ReportTypeViewInfoConfig([FromRoute] int reportTypeId)
        {
            return await _reportConfigService
                .ReportTypeViewGetInfo(reportTypeId, true)
                .ConfigureAwait(true);
        }

        [HttpPut]
        [Route("{reportTypeId}/ViewInfo")]
        public async Task<bool> ViewInfoUpdate([FromRoute] int reportTypeId, [FromBody] ReportTypeViewModel model)
        {
            return await _reportConfigService
                .ReportTypeViewUpdate(reportTypeId, model)
                .ConfigureAwait(true);
        }

        [HttpPost]
        [Route("uploadReportTemplate")]
        public async Task<long> UploadReportTemplate(IFormFile file)
        {
            return await _fileService.Upload(EnumObjectType.ReportType, EnumFileType.Document, string.Empty, file).ConfigureAwait(true);
        }
    }
}