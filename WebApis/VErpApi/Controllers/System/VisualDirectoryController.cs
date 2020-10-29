using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VErp.Infrastructure.ServiceCore.Model;
using VErp.Services.Master.Model.VisualDirectory;
using VErp.Services.Master.Service.VisualDirectory;

namespace VErpApi.Controllers.System
{
    [Route("api/VisualDirectory")]
    [ApiController]
    public class VisualDirectoryController : ControllerBase
    {
        private readonly IMediaLibaryService _visualDirectoryService;

        public VisualDirectoryController(IMediaLibaryService visualDirectoryService)
        {
            _visualDirectoryService = visualDirectoryService;
        }

        [HttpGet]
        [Route("structure")]
        public async Task<DirectoryStructure> GetDirectoryStructure()
        {
            return await _visualDirectoryService.GetDirectoryStructure();
        }

        [HttpPost]
        [Route("structure")]
        public async Task<bool> GetDirectoryStructure([FromQuery] string root, [FromQuery] string subdirectory)
        {
            return await _visualDirectoryService.CreateSubdirectory(root, subdirectory);
        }

        [HttpGet]
        [Route("files")]
        public async Task<PageData<VisualFile>> GetVisualFiles([FromQuery] string directory, [FromQuery] string keyWord, [FromQuery] int page, [FromQuery] int size)
        {
            return await _visualDirectoryService.GetVisualFiles(directory, keyWord, page, size);
        }

        [HttpPost]
        [Route("files")]
        public async Task<bool> GetVisualFiles([FromForm] string directory, [FromForm] List<IFormFile> files)
        {
            return await _visualDirectoryService.UploadFiles(directory, files);
        }

        [HttpDelete]
        [Route("files")]
        public async Task<bool> DeleteVisualFiles([FromBody] List<VisualFile> files)
        {
            return await _visualDirectoryService.DeleteFiles(files);
        }
    }
}
