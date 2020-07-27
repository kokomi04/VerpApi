using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using VErp.Commons.Enums.StandardEnum;
using VErp.Services.Stock.Service.FileResources;

namespace VErpApi.Controllers.Stock.Files
{
    [Route("filestorage")]
    public class FileStorageController : Controller
    {
        private readonly IFileStoreService _fileStoreService;
        public FileStorageController(IFileStoreService fileStoreService)
        {
            _fileStoreService = fileStoreService;
        }

        [AllowAnonymous]
        [Route("view/{fileName}")]
        [HttpGet]
        public async Task<IActionResult> Preview([FromRoute] string fileName, [FromQuery] string fileKey)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return NotFound();

            var r = await _fileStoreService.GetFileStream(fileKey);
            if (!r.Code.IsSuccess())
            {
                return new JsonResult(r);
            }

            return new FileStreamResult(r.Data.file, !string.IsNullOrWhiteSpace(r.Data.contentType) ? r.Data.contentType : "application/octet-stream");
        }
    }
}