using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VErp.Commons.Enums.StandardEnum;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Stock.Model.FileResources;
using VErp.Services.Stock.Service.FileResources;

namespace VErpApi.Controllers.System
{
    [Route("api/filestorage/media")]
    [ApiController]
    public class MediaLibrariesController : ControllerBase
    {
        private readonly IMediaLibaryService _mediaService;

        public MediaLibrariesController(IMediaLibaryService visualDirectoryService)
        {
            _mediaService = visualDirectoryService;
        }

        [HttpGet]
        [Route("structure")]
        public async Task<DirectoryStructure> GetDirectoryStructure()
        {
            return await _mediaService.GetDirectoryStructure();
        }

        [HttpPost]
        [Route("structure")]
        public async Task<bool> GetDirectoryStructure([FromQuery] string root, [FromQuery] string subdirectory)
        {
            return await _mediaService.CreateSubdirectory(root, subdirectory);
        }

        [HttpGet]
        [Route("files")]
        public async Task<PageData<VisualFile>> GetVisualFiles([FromQuery] string directory, [FromQuery] string keyWord, [FromQuery] int page, [FromQuery] int size)
        {
            return await _mediaService.GetVisualFiles(directory, keyWord, page, size);
        }

        [HttpPost]
        [Route("files")]
        public async Task<bool> GetVisualFiles([FromForm] string directory, [FromForm] List<IFormFile> files)
        {
            return await _mediaService.UploadFiles(directory, files);
        }

        [HttpDelete]
        [Route("files")]
        public async Task<bool> DeleteVisualFiles([FromBody] List<VisualFile> files)
        {
            return await _mediaService.DeleteFiles(files);
        }

        [AllowAnonymous]
        [Route("view")]
        [HttpGet]
        public async Task<IActionResult> Preview([FromQuery] string filePath, [FromQuery] bool thumb)
        {
            if (string.IsNullOrWhiteSpace(filePath)) return NotFound();

            var r = await _mediaService.GetFileStream(filePath, thumb);

            return new FileStreamResult(r.file, !string.IsNullOrWhiteSpace(r.contentType) ? r.contentType : "application/octet-stream") { FileDownloadName = r.fileName };
        }

    }
}
