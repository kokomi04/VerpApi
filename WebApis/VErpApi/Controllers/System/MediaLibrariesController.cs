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
        [Route("structure/create")]
        public async Task<bool> CreateDirectory([FromQuery] string root, [FromQuery] string subdirectory)
        {
            return await _mediaService.CreateSubdirectory(root, subdirectory);
        }

        [HttpDelete]
        [Route("structure/delete")]
        public async Task<bool> DeleteDirectory([FromQuery] string directory)
        {
            return await _mediaService.DeletedDirectory(directory);
        }
        /// <summary>
        /// thay đổi tên directory
        /// </summary>
        /// <param name="directory">đường dẫn folder</param>
        /// <param name="newName">tên folder thay thế</param>
        /// <returns></returns>
        [HttpPost]
        [Route("structure/rename")]
        public async Task<bool> RenameDirectory([FromQuery] string directory, [FromQuery] string newName)
        {
            return await _mediaService.RenameDirectory(directory, newName);
        }

        /// <summary>
        /// Copy directory
        /// </summary>
        /// <param name="directory">đường dẫn folder gốc</param>
        /// <param name="newDirectory">đường dẫn folder chuyển tới</param>
        /// <returns></returns>
        [HttpPost]
        [Route("structure/copy")]
        public async Task<bool> CopyDirectory([FromQuery] string directory, [FromQuery] string newDirectory)
        {
            return await _mediaService.CopyDirectory(directory, newDirectory);
        }

        /// <summary>
        /// move directory
        /// </summary>
        /// <param name="directory">đường dẫn folder gốc</param>
        /// <param name="newDirectory">đường dẫn folder chuyển tới</param>
        /// <returns></returns>
        [HttpPost]
        [Route("structure/move")]
        public async Task<bool> MoveDirectory([FromQuery] string directory, [FromQuery] string newDirectory)
        {
            return await _mediaService.MoveDirectory(directory, newDirectory);
        }

        [HttpGet]
        [Route("files")]
        public async Task<PageData<VisualFile>> GetVisualFiles([FromQuery] string directory, [FromQuery] string keyWord, [FromQuery] int page, [FromQuery] int size)
        {
            return await _mediaService.GetVisualFiles(directory, keyWord, page, size);
        }

        [HttpPost]
        [Route("files/upload")]
        public async Task<bool> UploadFiles([FromForm] string directory, [FromForm] List<IFormFile> files)
        {
            return await _mediaService.UploadFiles(directory, files);
        }

        [HttpDelete]
        [Route("files/delete")]
        public async Task<bool> DeleteFiles([FromBody] List<string> files)
        {
            return await _mediaService.DeleteFiles(files);
        }

        /// <summary>
        /// Copy file sang directory mới
        /// </summary>
        /// <param name="directory">đường dẫn directory chứa file mới</param>
        /// <param name="files">Danh sách file copy</param>
        /// <returns></returns>
        [HttpPost]
        [Route("files/copy")]
        public async Task<bool> CopyFiles([FromQuery] string directory, [FromBody] List<string> files)
        {
            return await _mediaService.CopyFiles(files, directory);
        }

        /// <summary>
        /// Di chuyển file sang directory mới
        /// </summary>
        /// <param name="directory">đường dẫn directory chứa file mới</param>
        /// <param name="files">Danh sách file cần chuyển</param>
        /// <returns></returns>
        [HttpPost]
        [Route("files/move")]
        public async Task<bool> MoveFiles([FromQuery] string directory, [FromBody] List<string> files)
        {
            return await _mediaService.MoveFiles(files, directory);
        }

        /// <summary>
        /// Đổi tên file
        /// </summary>
        /// <param name="file">đường dẫn file</param>
        /// <param name="nfile">Tên mới</param>
        /// <returns></returns>
        [HttpPost]
        [Route("files/rename")]
        public async Task<bool> RenameFile([FromQuery] string file, [FromQuery] string nfile)
        {
            return await _mediaService.RenameFile(file, nfile);
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
