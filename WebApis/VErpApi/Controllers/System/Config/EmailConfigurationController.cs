using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Master.Model.Notification;
using VErp.Services.Master.Service.Notification;

namespace VErpApi.Controllers.System
{
    [Route("api/EmailConfiguration")]
    public class EmailConfigurationController : VErpBaseController
    {
        private readonly IEmailConfigurationService _emailConfigurationService;

        public EmailConfigurationController(IEmailConfigurationService emailConfigurationService)
        {
            _emailConfigurationService = emailConfigurationService;
        }

        [HttpGet]
        [Route("")]
        public async Task<EmailConfigurationModel> GetEmailConfiguration()
        {
            return await _emailConfigurationService.GetEmailConfiguration();
        }
        
        [HttpPut]
        [Route("")]
        public async Task<bool> UpdateEmailConfiguration([FromBody] EmailConfigurationModel model)
        {
            return await _emailConfigurationService.UpdateEmailConfiguration(model);
        }
    }
}