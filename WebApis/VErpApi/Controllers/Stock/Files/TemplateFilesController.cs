using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using VErp.Commons.Enums.MasterEnum;
using VErp.Commons.Enums.StandardEnum;
using VErp.Commons.Enums.StockEnum;
using VErp.Commons.Library.Model;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Services.Stock.Model.FileResources;
using VErp.Services.Stock.Service.FileResources;

namespace VErpApi.Controllers.Stock.Files
{
    [Route("api/files")]
    public class TemplateFilesController : VErpBaseController
    {
        private readonly ITemplateFileService _templateFileService;
        public TemplateFilesController(ITemplateFileService templateFileService)
        {
            _templateFileService = templateFileService;
        }


        [GlobalApi]
        [HttpPost]
        [VErpAction(EnumActionType.View)]
        [Route("GetFilesUrls")]
        public IList<TemplateFileToDownloadInfo> GetFilesUrls([FromBody] IList<string> filePaths)
        {
            return _templateFileService.GetFilesUrls(filePaths);
        }


        [HttpPost]
        [Route("{objectTypeId}/upload")]
        public async Task<string> Upload([FromRoute] EnumFileType fileTypeId, [FromForm] IFormFile file)
        {
            return await _templateFileService.Upload(fileTypeId, file).ConfigureAwait(true);
        }

    }
}