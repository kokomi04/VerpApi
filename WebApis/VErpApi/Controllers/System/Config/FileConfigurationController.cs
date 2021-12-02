using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VErp.Infrastructure.ApiCore;
using VErp.Infrastructure.ApiCore.Attributes;
using VErp.Services.Master.Model.FileConfig;
using VErp.Services.Master.Model.Notification;
using VErp.Services.Master.Service.Notification;

namespace VErpApi.Controllers.System
{
    [Route("api/fileconfig")]
    public class FileConfigurationController : VErpBaseController
    {
        private readonly IFileConfigurationService _fileConfigurationService;

        public FileConfigurationController(IFileConfigurationService fileConfigurationService)
        {
            _fileConfigurationService = fileConfigurationService;
        }

        [HttpGet]
        [Route("")]
        [GlobalApi]
        public async Task<FileConfigurationModel> GetFileConfiguration()
        {
            return await _fileConfigurationService.GetFileConfiguration();
        }

        [HttpPut]
        [Route("")]
        public async Task<bool> UpdateFileConfiguration([FromBody] FileConfigurationModel model)
        {
            return await _fileConfigurationService.UpdateFileConfiguration(model);
        }
    }
}