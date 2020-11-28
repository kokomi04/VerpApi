using Microsoft.AspNetCore.Authorization;
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
using VErp.Infrastructure.ApiCore.Model;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.FileResources;
using VErp.Services.Stock.Service.FileResources;

namespace VErpApi.Controllers.Stock.Files
{
    [Route("api/files")]
    public class FilesController : VErpBaseController
    {
        private readonly IFileService _fileService;
        private readonly IFileStoreService _fileStoreService;
        public FilesController(IFileService fileService, IFileStoreService fileStoreService)
        {
            _fileService = fileService;
            _fileStoreService = fileStoreService;
        }

        /// <summary>
        /// Lấy thông tin file để download hoặc preview
        /// </summary>
        /// <param name="fileId"></param>
        /// <param name="thumb">EnumThumbnailSize: loại thumbnail sẽ trả về</param>
        /// <returns></returns>
        [GlobalApi]
        [HttpGet]
        [Route("{fileId}/GetFileUrl")]
        public async Task<FileToDownloadInfo> GetFileUrl([FromRoute] long fileId, [FromQuery] EnumThumbnailSize? thumb)
        {
            return await _fileService.GetFileUrl(fileId, thumb).ConfigureAwait(true);
        }

        [GlobalApi]
        [HttpPost]
        [VErpAction(EnumAction.View)]
        [Route("GetFilesUrls")]
        public async Task<IList<FileToDownloadInfo>> GetFilesUrls([FromBody] IList<long> fileIds, [FromQuery] EnumThumbnailSize? thumb)
        {
            return await _fileService.GetFilesUrls(fileIds, thumb).ConfigureAwait(true);
        }

        /// <summary>
        /// Lấy danh sách thumbnail
        /// </summary>  
        /// <returns></returns>
        [GlobalApi]
        [HttpPost]
        [Route("GetThumbnails")]
        [VErpAction(EnumAction.View)]
        public async Task<IList<FileThumbnailInfo>> GetThumbnails([FromBody] GetThumbnailsInput req)
        {
            return await _fileService.GetThumbnails(req?.FileIds, req.ThumbnailSize).ConfigureAwait(true);
        }


        /// <summary>
        /// Upload file
        /// </summary>
        /// <param name="objectTypeId"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        [GlobalApi]
        [HttpPost]
        [Route("{objectTypeId}/upload")]
        public async Task<long> Upload([FromRoute] EnumObjectType objectTypeId, [FromForm] IFormFile file)
        {

            return await _fileService.Upload(objectTypeId, string.Empty, file).ConfigureAwait(true);
        }

        [GlobalApi]
        [HttpPost]
        [Route("ParseExcel")]
        public IList<ExcelSheetDataModel> ParseExcel([FromForm] IFormFile file, [FromQuery] string sheetName, [FromQuery] int fromRow = 1, [FromQuery] int? toRow = null, [FromQuery] int? maxrows = null)
        {
            return _fileService.ParseExcel(file, sheetName, fromRow, toRow, maxrows);
        }
    }
}