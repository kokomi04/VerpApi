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
    [Route("api/templates")]
    public class TemplateFilesController : VErpBaseController
    {
        private readonly ITemplateFileService _templateFileService;
        public TemplateFilesController(ITemplateFileService templateFileService)
        {
            _templateFileService = templateFileService;
        }


        /// <summary>
        /// Get file url base on file paths
        /// </summary>
        /// <param name="filePaths"></param>
        /// <returns></returns>
        [GlobalApi]
        [HttpPost]
        [VErpAction(EnumActionType.View)]
        [Route("GetFilesUrls")]
        public IList<TemplateFileToDownloadInfo> GetFilesUrls([FromBody] IList<string> filePaths)
        {
            return _templateFileService.GetFilesUrls(filePaths);
        }

        /// <summary>
        /// Uoload file template
        /// </summary>
        /// <param name="fileTypeId"></param>
        /// <param name="file"></param>
        /// <returns>string file path</returns>
        [HttpPost]
        [Route("{fileTypeId}/upload")]
        public async Task<string> Upload([FromRoute] EnumFileType fileTypeId, [FromForm] IFormFile file)
        {
            return await _templateFileService.Upload(fileTypeId, file).ConfigureAwait(true);
        }

    }
}