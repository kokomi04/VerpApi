using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VErp.Infrastructure.ApiCore;
using VErp.Services.Master.Model.Notification;
using VErp.Services.Master.Service.Notification;

namespace VErpApi.Controllers.System.Internal
{
    [Route("api/internal/[controller]")]
    [ApiController]
    public class InternalEmailConfigurationController : CrossServiceBaseController
    {
        private readonly IEmailConfigurationService _emailConfigurationService;
        private readonly IMailTemplateService _mailTemplateService;

        public InternalEmailConfigurationController(IEmailConfigurationService emailConfigurationService, IMailTemplateService mailTemplateService)
        {
            _emailConfigurationService = emailConfigurationService;
            _mailTemplateService = mailTemplateService;
        }

        [HttpGet]
        [Route("")]
        public async Task<EmailConfigurationModel> GetEmailConfiguration()
        {
            return await _emailConfigurationService.GetEmailConfiguration();
        }

        [HttpGet]
        [Route("template")]
        public async Task<MailTemplateModel> GetMailTemplateByCode([FromQuery] string mailTemplateCode)
        {
            return await _mailTemplateService.GetMailTemplateByCode(mailTemplateCode);
        }

    }
}